# Company Asset Pool API Guide

## Table of Contents

- [Overview](#overview)
- [Base URL](#base-url)
- [Endpoints](#endpoints)
- [View Models](#view-models)
- [Validation Rules](#validation-rules)
- [Error Codes Reference](#error-codes-reference)
- [Usage Examples](#usage-examples)
- [Performance Notes](#performance-notes)
- [Related Documentation](#related-documentation)

---

## Overview

This document describes the API endpoints for managing **company-owned asset pools** in SF Management. These endpoints provide dedicated functionality for handling assets owned directly by the company, separate from those belonging to specific asset holders (Clients, Banks, Members, Poker Managers).

Company asset pools are identified by having `BaseAssetHolderId = null`.

---

## Base URL

```
/api/v{version}/company/asset-pools
```

All endpoints are versioned. Current version: `v1`

---

## Endpoints

### 1. Get All Company Asset Pools

Retrieves all company-owned asset pools with calculated metrics.

```http
GET /api/v1/company/asset-pools
```

**Response: `200 OK`**

```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "assetGroup": "FiatAssets",
    "ownerName": "Company",
    "baseAssetHolderId": null,
    "currentBalance": 150000.00,
    "walletIdentifierCount": 5,
    "transactionCount": 89,
    "createdAt": "2025-01-01T00:00:00Z",
    "lastTransactionDate": "2025-01-14T10:30:00Z",
    "description": "Main company BRL pool",
    "businessJustification": "Central liquidity management",
    "walletIdentifiers": [...]
  }
]
```

| Field | Type | Description |
|-------|------|-------------|
| `id` | `Guid` | Unique identifier for the pool |
| `assetGroup` | `AssetGroup` | Asset category (FiatAssets, PokerAssets, CryptoAssets, etc.) |
| `ownerName` | `string` | Always "Company" for these pools |
| `baseAssetHolderId` | `Guid?` | Always `null` for company pools |
| `currentBalance` | `decimal` | Calculated balance across all wallet identifiers |
| `walletIdentifierCount` | `int` | Number of active wallet identifiers |
| `transactionCount` | `int` | Total transactions involving this pool |
| `createdAt` | `DateTime` | Pool creation timestamp |
| `lastTransactionDate` | `DateTime?` | Most recent transaction date |
| `walletIdentifiers` | `List` | Detailed wallet identifiers in this pool |

---

### 2. Get Company Asset Pool by Asset Group

Retrieves a specific company pool by its asset group.

```http
GET /api/v1/company/asset-pools/{assetGroup}
```

| Parameter | Type | Location | Description |
|-----------|------|----------|-------------|
| `assetGroup` | `AssetGroup` | Path | The asset group (FiatAssets, PokerAssets, CryptoAssets, Flexible, Settlements) |

**Response: `200 OK`** - Same structure as single pool object above

**Response: `404 Not Found`**

```json
{
  "title": "Company Asset Pool Not Found",
  "detail": "No company asset pool found for asset group FiatAssets",
  "status": 404,
  "requestId": "0HNDVHGQTNTTP:00000001",
  "assetGroup": "FiatAssets"
}
```

---

### 3. Create Company Asset Pool

Creates a new company-owned asset pool.

```http
POST /api/v1/company/asset-pools
```

**Request Body:**

```json
{
  "assetGroup": "FiatAssets",
  "description": "Main company BRL liquidity pool",
  "initialBalance": 500000.00,
  "businessJustification": "Central treasury management for operational expenses"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `assetGroup` | `AssetGroup` | Yes | Asset category for the pool |
| `description` | `string` | No | Purpose description (max 500 chars) |
| `initialBalance` | `decimal` | No | Starting balance for reference |
| `businessJustification` | `string` | No | Business reason for creating (max 1000 chars) |

**Response: `201 Created`** - Returns the created pool with Location header

**Response: `400 Bad Request`** - Validation errors

**Response: `409 Conflict`** - Duplicate pool for asset group

---

### 4. Get Company Asset Pool Summary

Retrieves an aggregate summary of all company pools with activity metrics.

```http
GET /api/v1/company/asset-pools/summary
```

**Caching:** Response is cached for 5 minutes.

**Response: `200 OK`**

```json
{
  "totalPools": 3,
  "totalBalance": 250000.00,
  "assetGroupBalances": [
    {
      "assetGroup": "FiatAssets",
      "assetGroupName": "FiatAssets",
      "balance": 150000.00,
      "walletIdentifierCount": 8,
      "transactionCount": 200,
      "lastTransactionDate": "2025-01-14T10:30:00Z"
    }
  ],
  "recentActivity": {
    "transactionsLast30Days": 45,
    "balanceChangeLast30Days": 25000.00,
    "mostActiveAssetGroup": "FiatAssets",
    "largestTransactionAmount": 50000.00
  }
}
```

---

### 5. Delete Company Asset Pool

Soft-deletes a company asset pool after validation.

```http
DELETE /api/v1/company/asset-pools/{assetGroup}
```

| Parameter | Type | Location | Description |
|-----------|------|----------|-------------|
| `assetGroup` | `AssetGroup` | Path | The asset group to delete |

**Response: `204 No Content`** - Successfully deleted

**Response: `404 Not Found`** - Pool doesn't exist

**Response: `409 Conflict`** - Cannot delete due to constraints (active wallets or transactions)

---

### 6. Get Company Asset Pool Analytics

Retrieves detailed analytics for company pools by period.

```http
GET /api/v1/company/asset-pools/analytics?year=2025&month=1&includeTransactions=true&transactionLimit=50
```

**Caching:** Response is cached for 10 minutes.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `year` | `int` | Yes | - | Year for analytics (2020-2050) |
| `month` | `int` | No | - | Month (1-12). If omitted, returns yearly data |
| `includeTransactions` | `bool` | No | `true` | Include transaction details |
| `transactionLimit` | `int` | No | `100` | Max transactions per pool (1-1000) |

**Response: `200 OK`**

```json
{
  "period": {
    "year": 2025,
    "month": 1,
    "periodName": "January 2025",
    "startDate": "2025-01-01T00:00:00Z",
    "endDate": "2025-01-31T23:59:59Z",
    "totalDays": 31
  },
  "summary": {
    "activePoolsCount": 3,
    "totalEndingBalance": 750000.00,
    "totalStartingBalance": 500000.00,
    "netBalanceChange": 250000.00,
    "totalTransactionCount": 145,
    "totalTransactionVolume": 2500000.00,
    "averageTransactionAmount": 17241.38,
    "largestTransaction": 100000.00,
    "mostActiveAssetGroup": "FiatAssets"
  },
  "assetPoolData": [
    {
      "assetPoolId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "assetGroup": "FiatAssets",
      "startingBalance": 300000.00,
      "endingBalance": 450000.00,
      "netBalanceChange": 150000.00,
      "transactionCount": 89,
      "transactionBreakdown": {
        "fiatTransactions": { "count": 65, "totalVolume": 1200000.00 },
        "digitalTransactions": { "count": 15, "totalVolume": 400000.00 },
        "settlementTransactions": { "count": 9, "totalVolume": 200000.00 }
      },
      "transactions": [...]
    }
  ]
}
```

---

### 7. Get System Wallet to Pair With

Finds a **company-owned** system wallet that can be paired with an external wallet for transactions.

```http
GET /api/v1/company/asset-pools/system-wallet-to-pair-with/{walletIdentifierId}
```

| Parameter | Type | Location | Description |
|-----------|------|----------|-------------|
| `walletIdentifierId` | `Guid` | Path | The external wallet identifier to find a pair for |

**Response: `200 OK`** - Returns a `WalletIdentifierResponse` for the matching system wallet

**Rules:**
- Only Flexible wallets with `BaseAssetHolderId = null` are eligible (company-owned)
- Result is deterministic (ordered by creation date)

**Use Case:** When creating transactions, this endpoint helps find the appropriate company wallet to use as the counterparty based on matching `AssetType`.

---

## View Models

### CompanyAssetPoolRequest

```csharp
public class CompanyAssetPoolRequest
{
    [Required]
    public AssetGroup AssetGroup { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public decimal? InitialBalance { get; set; }
    
    [MaxLength(1000)]
    public string? BusinessJustification { get; set; }
}
```

### CompanyAssetPoolResponse

```csharp
public class CompanyAssetPoolResponse : BaseResponse
{
    public AssetGroup AssetGroup { get; set; }
    public string OwnerName => "Company";           // Always "Company"
    public Guid? BaseAssetHolderId => null;         // Always null
    public decimal CurrentBalance { get; set; }
    public int WalletIdentifierCount { get; set; }
    public int TransactionCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastTransactionDate { get; set; }
    public string? Description { get; set; }
    public string? BusinessJustification { get; set; }
    public List<WalletIdentifierResponse> WalletIdentifiers { get; set; }
}
```

### CompanyAssetPoolSummaryResponse

```csharp
public class CompanyAssetPoolSummaryResponse
{
    public int TotalPools { get; set; }
    public decimal TotalBalance { get; set; }
    public List<CompanyAssetGroupBalance> AssetGroupBalances { get; set; }
    public CompanyPoolActivity RecentActivity { get; set; }
}
```

---

## Validation Rules

The `AssetPoolValidationService` enforces rules for company pool operations:

### Creation Rules

| Rule | Error Code | Description |
|------|------------|-------------|
| Valid AssetGroup | `INVALID_ASSET_GROUP` | Must be a valid enum value |
| No duplicates | `DUPLICATE_COMPANY_POOL` | Company can only have one pool per AssetGroup |
| Allowed asset groups | `RESTRICTED_COMPANY_ASSET_GROUP` | Some groups may be restricted |

### Deletion Rules

| Rule | Error Code | Description |
|------|------------|-------------|
| Pool exists | `ASSET_POOL_NOT_FOUND` | Pool must exist and not be deleted |
| No active wallets | `ACTIVE_WALLET_IDENTIFIERS` | Cannot delete with active wallet identifiers |
| No transactions | `EXISTING_TRANSACTIONS` | Cannot delete with associated transactions |

> **Note:** For detailed validation implementation, see [VALIDATION_SYSTEM.md](../05_INFRASTRUCTURE/VALIDATION_SYSTEM.md).

---

## Error Codes Reference

| Code | HTTP Status | Description |
|------|-------------|-------------|
| `INVALID_ASSET_GROUP` | 400 | Invalid AssetGroup enum value |
| `DUPLICATE_COMPANY_POOL` | 409 | Company already has a pool for this asset group |
| `RESTRICTED_COMPANY_ASSET_GROUP` | 400 | Asset group not allowed for company ownership |
| `ASSET_POOL_NOT_FOUND` | 404 | Pool doesn't exist |
| `ACTIVE_WALLET_IDENTIFIERS` | 409 | Cannot delete pool with active wallet identifiers |
| `EXISTING_TRANSACTIONS` | 409 | Cannot delete pool with associated transactions |

---

## Usage Examples

### Create a Company Fiat Pool

```bash
curl -X POST /api/v1/company/asset-pools \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d '{
    "assetGroup": "FiatAssets",
    "description": "Main company BRL liquidity pool",
    "initialBalance": 500000.00,
    "businessJustification": "Central treasury management"
  }'
```

### Get Dashboard Summary

```bash
curl -X GET /api/v1/company/asset-pools/summary \
  -H "Authorization: Bearer {token}"
```

### Get Monthly Analytics

```bash
curl -X GET "/api/v1/company/asset-pools/analytics?year=2025&month=1&includeTransactions=true" \
  -H "Authorization: Bearer {token}"
```

### Find System Wallet for Transaction Pairing

```bash
curl -X GET /api/v1/company/asset-pools/system-wallet-to-pair-with/{walletId} \
  -H "Authorization: Bearer {token}"
```

---

## Performance Notes

### Caching

| Endpoint | Cache Duration | Notes |
|----------|---------------|-------|
| `GET /summary` | 5 minutes | Aggregate data, frequently accessed |
| `GET /analytics` | 10 minutes | Heavy computation, less frequent updates |

### Optimization

- Balance calculations aggregate transactions efficiently
- Includes are optimized to load only required related entities
- Transaction limits prevent memory issues with large datasets

---

## Separation from Asset Holder Pools

The standard `AssetPoolRequest` used by `AssetPoolController` explicitly requires `BaseAssetHolderId`:

```csharp
[Required(ErrorMessage = "BaseAssetHolderId is required. For company pools, use the CompanyAssetPoolController.")]
public Guid BaseAssetHolderId { get; set; }
```

**Why the Separation?**

1. **Prevents Accidents** - Developers can't accidentally create orphaned company pools
2. **Clear Intent** - Using `CompanyAssetPoolController` makes the intent explicit
3. **Different Validation** - Company pools have different validation rules
4. **Richer Features** - Company-specific endpoints include metrics and analytics

---

## Related Documentation

| Topic | Document |
|-------|----------|
| Asset Infrastructure | [ASSET_INFRASTRUCTURE.md](../03_CORE_SYSTEMS/ASSET_INFRASTRUCTURE.md) |
| Company Ownership Model | [ASSET_POOL_COMPANY_OWNERSHIP_ANALYSIS.md](../07_REFERENCE/ASSET_POOL_COMPANY_OWNERSHIP_ANALYSIS.md) |
| Flexible Wallets | [FLEXIBLE_WALLET_IMPLEMENTATION.md](FLEXIBLE_WALLET_IMPLEMENTATION.md) |
| Validation System | [VALIDATION_SYSTEM.md](../05_INFRASTRUCTURE/VALIDATION_SYSTEM.md) |
| Transaction Infrastructure | [TRANSACTION_INFRASTRUCTURE.md](../03_CORE_SYSTEMS/TRANSACTION_INFRASTRUCTURE.md) |
| Complete API Reference | [API_REFERENCE.md](API_REFERENCE.md) |
