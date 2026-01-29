# API Reference

## Overview

The SF Management API provides a comprehensive set of endpoints for managing financial operations related to poker management, including asset holders, transactions, wallet identifiers, and settlements. The API follows RESTful conventions and uses API versioning.

**Base URL**: `/api/v1`

**API Version**: 1.0

**Content-Type**: `application/json`

---

## Table of Contents

1. [Authentication](#authentication)
2. [Common Patterns](#common-patterns)
3. [Entity Controllers](#entity-controllers)
   - [Client](#client)
   - [Bank](#bank)
   - [Member](#member)
   - [PokerManager](#pokermanager)
4. [Asset Management](#asset-management)
   - [AssetPool](#assetpool)
   - [WalletIdentifier](#walletidentifier)
   - [CompanyAssetPool](#companyassetpool)
5. [Transaction Controllers](#transaction-controllers)
   - [FiatAssetTransaction](#fiatasettransaction)
   - [DigitalAssetTransaction](#digitalassettransaction)
   - [SettlementTransaction](#settlementtransaction)
6. [Import & Reconciliation](#import--reconciliation)
   - [ImportedTransaction](#importedtransaction)
7. [Supporting Resources](#supporting-resources)
   - [Category](#category)
   - [InitialBalance](#initialbalance)
8. [Diagnostics](#diagnostics)
9. [Error Handling](#error-handling)

---

## Authentication

The API uses Auth0 for authentication. All endpoints require a valid JWT bearer token.

```http
Authorization: Bearer <token>
```

For detailed authentication information, see [AUTHENTICATION.md](../05_INFRASTRUCTURE/AUTHENTICATION.md).

---

## Common Patterns

### Base Controller Endpoints

All entity controllers that extend `BaseApiController` provide these standard endpoints:

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/` | List all entities |
| GET | `/{id}` | Get entity by ID |
| POST | `/` | Create new entity |
| PUT | `/{id}` | Update entity |
| DELETE | `/{id}` | Delete entity |

### Asset Holder Controller Endpoints

Entity controllers that extend `BaseAssetHolderController` (Client, Bank, Member, PokerManager) additionally provide:

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/wallet-identifiers` | List wallet identifiers filtered by asset group/type |
| GET | `/{id}/wallet-identifiers` | Get wallet identifiers for specific entity |
| GET | `/{id}/statistics` | Get entity statistics |
| GET | `/{id}/can-delete` | Check if entity can be deleted |
| GET | `/{id}/balance` | Get balance by asset type |
| GET | `/{id}/transactions` | Get transaction statement |

### Response Caching

Some endpoints implement response caching for performance:

```csharp
[ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, NoStore = false)]
```

Cached endpoints include:
- `GET /wallet-identifiers-connected` (60 seconds)
- `GET /{id}/balance` (60 seconds)
- `GET /summary` (300 seconds)
- `GET /analytics` (600 seconds)

---

## Entity Controllers

### Client

Manages client entities (poker players/customers).

**Base Route**: `/api/v1/client`

#### Standard Endpoints (inherited from BaseAssetHolderController)

| Method | Route | Description | Response |
|--------|-------|-------------|----------|
| GET | `/` | List all clients | `List<ClientResponse>` |
| GET | `/{id}` | Get client by ID | `ClientResponse` |
| POST | `/` | Create client | `ClientResponse` |
| PUT | `/{id}` | Update client | `ClientResponse` |
| DELETE | `/{id}` | Delete client | `204 No Content` |
| GET | `/wallet-identifiers` | List wallets by type/group | `List<WalletIdentifierResponse>` |
| GET | `/{id}/wallet-identifiers` | Get client's wallets | `List<WalletIdentifierResponse>` |
| GET | `/{id}/statistics` | Get statistics | `AssetHolderStatistics` |
| GET | `/{id}/balance` | Get balance by asset type | `Dictionary<string, decimal>` |
| GET | `/{id}/transactions` | Get transactions | `StatementAssetHolderWithTransactions` |

#### Client-Specific Endpoints

| Method | Route | Description | Request | Response |
|--------|-------|-------------|---------|----------|
| GET | `/{id}/client-statistics` | Get client-specific stats | - | `ClientStatistics` |
| POST | `/{id}/send-brazilian-real` | ⚠️ **DEPRECATED** - Send BRL transaction | `FiatAssetTransactionRequest` | `FiatAssetTransaction` |

> ⚠️ **Deprecation Notice:** `POST /{id}/send-brazilian-real` is deprecated. Use `POST /api/v1/transfer` instead. See [Migration Guide](#deprecated-endpoints-migration).

---

### Bank

Manages bank entities for fiat currency operations.

**Base Route**: `/api/v1/bank`

#### Standard Endpoints (inherited from BaseAssetHolderController)

| Method | Route | Description | Response |
|--------|-------|-------------|----------|
| GET | `/` | List all banks | `List<BankResponse>` |
| GET | `/{id}` | Get bank by ID | `BankResponse` |
| POST | `/` | Create bank | `BankResponse` |
| PUT | `/{id}` | Update bank | `BankResponse` |
| DELETE | `/{id}` | Delete bank | `204 No Content` |
| GET | `/wallet-identifiers` | List wallets by type/group | `List<WalletIdentifierResponse>` |
| GET | `/{id}/wallet-identifiers` | Get bank's wallets | `List<WalletIdentifierResponse>` |
| GET | `/{id}/statistics` | Get statistics | `AssetHolderStatistics` |
| GET | `/{id}/balance` | Get balance by asset type | `Dictionary<string, decimal>` |
| GET | `/{id}/transactions` | Get transactions | `StatementAssetHolderWithTransactions` |

---

### Member

Manages member entities (business partners/stakeholders).

**Base Route**: `/api/v1/member`

#### Standard Endpoints (inherited from BaseAssetHolderController)

| Method | Route | Description | Response |
|--------|-------|-------------|----------|
| GET | `/` | List all members | `List<MemberResponse>` |
| GET | `/{id}` | Get member by ID | `MemberResponse` |
| POST | `/` | Create member | `MemberResponse` |
| PUT | `/{id}` | Update member | `MemberResponse` |
| DELETE | `/{id}` | Delete member | `204 No Content` |
| GET | `/wallet-identifiers` | List wallets by type/group | `List<WalletIdentifierResponse>` |
| GET | `/{id}/wallet-identifiers` | Get member's wallets | `List<WalletIdentifierResponse>` |
| GET | `/{id}/statistics` | Get statistics | `AssetHolderStatistics` |
| GET | `/{id}/balance` | Get balance by asset type | `Dictionary<string, decimal>` |
| GET | `/{id}/transactions` | Get transactions | `StatementAssetHolderWithTransactions` |

#### Member-Specific Endpoints

| Method | Route | Description | Request | Response |
|--------|-------|-------------|---------|----------|
| GET | `/{id}/member-statistics` | Get member-specific stats | - | `MemberStatistics` |
| POST | `/{id}/send-brazilian-real` | ⚠️ **DEPRECATED** - Send BRL transaction | `FiatAssetTransactionRequest` | `FiatAssetTransaction` |

> ⚠️ **Deprecation Notice:** `POST /{id}/send-brazilian-real` is deprecated. Use `POST /api/v1/transfer` instead. See [Migration Guide](#deprecated-endpoints-migration).

---

### PokerManager

Manages poker manager entities who oversee poker operations and settlements.

**Base Route**: `/api/v1/pokermanager`

#### Standard Endpoints (inherited from BaseAssetHolderController)

| Method | Route | Description | Response |
|--------|-------|-------------|----------|
| GET | `/` | List all poker managers | `List<PokerManagerResponse>` |
| GET | `/{id}` | Get poker manager by ID | `PokerManagerResponse` |
| POST | `/` | Create poker manager | `PokerManagerResponse` |
| PUT | `/{id}` | Update poker manager | `PokerManagerResponse` |
| DELETE | `/{id}` | Delete poker manager | `204 No Content` |
| GET | `/wallet-identifiers` | List wallets by type/group | `List<WalletIdentifierResponse>` |
| GET | `/{id}/wallet-identifiers` | Get manager's wallets | `List<WalletIdentifierResponse>` |
| GET | `/{id}/statistics` | Get statistics | `AssetHolderStatistics` |
| GET | `/{id}/transactions` | Get transactions | `StatementAssetHolderWithTransactions` |

#### PokerManager-Specific Endpoints

| Method | Route | Description | Request | Response |
|--------|-------|-------------|---------|----------|
| POST | `/{id}/send-brazilian-real` | ⚠️ **DEPRECATED** - Send BRL transaction | `FiatAssetTransactionRequest` | `FiatAssetTransaction` |
| GET | `/{id}/wallet-identifiers-connected` | Get connected wallets from other holders | - | `WalletIdentifiersConnectedResponse` |
| GET | `/{id}/conversion-wallets` | Get Internal wallets for self-conversion | - | `List<WalletIdentifierResponse>` |
| POST | `/{assetHolderId}/settlement-by-date` | Create settlement by date | `SettlementTransactionByDateRequest` | `SettlementTransactionByDateResponse` |
| GET | `/{id}/balance` | Get balance by **AssetGroup** (overridden) | - | `Dictionary<string, decimal>` |

> ⚠️ **Deprecation Notice:** `POST /{id}/send-brazilian-real` is deprecated. Use `POST /api/v1/transfer` instead. See [Migration Guide](#deprecated-endpoints-migration).

**Note**: PokerManager overrides the balance endpoint to return balances grouped by `AssetGroup` instead of `AssetType`.

---

## Asset Management

### AssetPool

Manages asset pools that group wallet identifiers.

**Base Route**: `/api/v1/assetpool`

#### Endpoints

| Method | Route | Description | Response |
|--------|-------|-------------|----------|
| GET | `/` | List all asset pools | `List<AssetPoolResponse>` |
| GET | `/{id}` | Get asset pool by ID | `AssetPoolResponse` |
| POST | `/` | Create asset pool | `AssetPoolResponse` |
| PUT | `/{id}` | Update asset pool | `AssetPoolResponse` |
| DELETE | `/{id}` | Delete asset pool | `204 No Content` |
| GET | `/asset-holder/{assetHolderId}` | Get pools for asset holder | `List<AssetPoolResponse>` |

---

### WalletIdentifier

Manages individual wallet identifiers within asset pools.

**Base Route**: `/api/v1/walletidentifier`

#### Endpoints

| Method | Route | Description | Request | Response |
|--------|-------|-------------|---------|----------|
| GET | `/` | List all wallet identifiers | - | `List<WalletIdentifierResponse>` |
| GET | `/{id}` | Get wallet identifier by ID | - | `WalletIdentifierResponse` |
| POST | `/` | Create wallet identifier | `WalletIdentifierRequest` | `WalletIdentifierResponse` |
| PUT | `/{id}` | Update wallet identifier | `WalletIdentifierRequest` | `WalletIdentifierResponse` |
| DELETE | `/{id}` | Delete wallet identifier | - | `204 No Content` |
| POST | `/internal-wallet` | Create internal wallet | `WalletIdentifierRequest` | `WalletIdentifierResponse` |
| POST | `/settlement-wallet` | Create settlement wallet | `WalletIdentifierRequest` | `WalletIdentifierResponse` |

---

### CompanyAssetPool

Manages company-owned asset pools (pools without a BaseAssetHolder).

**Base Route**: `/api/v1/company/asset-pools`

#### Endpoints

| Method | Route | Description | Request | Response |
|--------|-------|-------------|---------|----------|
| GET | `/` | List all company asset pools | - | `List<CompanyAssetPoolResponse>` |
| GET | `/{assetGroup}` | Get company pool by asset group | - | `CompanyAssetPoolResponse` |
| POST | `/` | Create company asset pool | `CompanyAssetPoolRequest` | `CompanyAssetPoolResponse` |
| DELETE | `/{assetGroup}` | Delete company asset pool | - | `204 No Content` |
| GET | `/summary` | Get summary with metrics | - | `CompanyAssetPoolSummaryResponse` |
| GET | `/analytics` | Get analytics by period | Query params | `CompanyAssetPoolAnalyticsResponse` |
| GET | `/system-wallet-to-pair-with/{walletIdentifierId}` | Get matching company-owned system wallet | - | `WalletIdentifierResponse` |

#### Analytics Query Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `year` | int | Yes | Year for analytics |
| `month` | int | No | Month (1-12) for analytics |
| `includeTransactions` | bool | No | Include transaction details |
| `transactionLimit` | int | No | Max transactions to include |

---

## Transaction Controllers

### Transfer (Unified Transfer Endpoint) ⭐ Recommended

**Base Route**: `/api/v1/transfer`

**Purpose**: Unified endpoint for all P2P transfers between asset holders, supporting both Fiat and Digital assets. This is the **recommended endpoint** for new implementations.

**Controller**: `TransferController`

#### Endpoints

| Method | Route | Description | Request | Response |
|--------|-------|-------------|---------|----------|
| POST | `/` | Create transfer | `TransferRequest` | `TransferResponse` |
| GET | `/{id}` | Get transfer by ID | Query: `entityType` | `TransferResponse` |

#### Key Features

- **Unified**: Works with both Fiat and Digital assets
- **Mode Inference**: Automatically detects TRANSFER vs INTERNAL mode
- **Bank Restrictions**: Enforces business rule blocking banks from TRANSFER mode
- **AssetGroup Restriction**: TRANSFER mode only allows Internal wallets (AssetGroup 4)
- **Explicit Wallet Creation**: Wallets must exist before transfer (auto-creation deprecated)
- **Balance Validation**: Optional balance checking

#### Request Body (POST)

```json
{
  "senderAssetHolderId": "guid",
  "receiverAssetHolderId": "guid",
  "assetType": 21,
  "amount": 1000.00,
  "date": "2026-01-22T10:00:00Z"
}
```

> ⚠️ **Note:** `createWalletsIfMissing` flag is **deprecated** (January 2026). If set to `true`, returns error `WALLETS_CREATION_DEPRECATED`. Create wallets explicitly before initiating transfer.

> **Note:** For detailed documentation including all parameters, error codes, and examples, see [TRANSACTION_API_ENDPOINTS.md](./TRANSACTION_API_ENDPOINTS.md).

---

### FiatAssetTransaction

Manages fiat currency transactions (BRL, USD, etc.).

**Base Route**: `/api/v1/fiatasettransaction`

#### Endpoints

| Method | Route | Description | Response |
|--------|-------|-------------|----------|
| GET | `/` | List all fiat transactions | `List<FiatAssetTransactionResponse>` |
| GET | `/{id}` | Get transaction by ID | `FiatAssetTransactionResponse` |
| POST | `/` | Create transaction | `FiatAssetTransactionResponse` |
| PUT | `/{id}` | Update transaction | `FiatAssetTransactionResponse` |
| DELETE | `/{id}` | Delete transaction | `204 No Content` |
| GET | `/bank-transactions` | Get bank-related transactions | `TableResponse<FiatAssetTransactionResponse>` |
| GET | `/direct-transactions` | Get non-bank transactions | `TableResponse<FiatAssetTransactionResponse>` |

#### Query Parameters for Transaction Lists

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `quantity` | int | No | 1000 | Number of records to return |
| `page` | int | No | 0 | Page number (0-indexed) |

---

### DigitalAssetTransaction

Manages digital asset transactions (poker chips, cryptocurrency).

**Base Route**: `/api/v1/digitalassettransaction`

#### Endpoints

| Method | Route | Description | Response |
|--------|-------|-------------|----------|
| GET | `/` | List all digital transactions | `List<DigitalAssetTransactionResponse>` |
| GET | `/{id}` | Get transaction by ID | `DigitalAssetTransactionResponse` |
| POST | `/` | Create transaction | `DigitalAssetTransactionResponse` |
| PUT | `/{id}` | Update transaction | `DigitalAssetTransactionResponse` |
| DELETE | `/{id}` | Delete transaction | `204 No Content` |
| GET | `/poker-manager-transactions` | Get poker manager transactions | `TableResponse<DigitalAssetTransactionResponse>` |

#### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `quantity` | int | No | 1000 | Number of records to return |
| `page` | int | No | 0 | Page number (0-indexed) |

---

### SettlementTransaction

Manages poker settlement transactions.

**Base Route**: `/api/v1/settlementtransaction`

#### Endpoints

| Method | Route | Description | Response |
|--------|-------|-------------|----------|
| GET | `/` | List all settlements | `List<SettlementTransactionResponse>` |
| GET | `/{id}` | Get settlement by ID | `SettlementTransactionResponse` |
| POST | `/` | Create settlement | `SettlementTransactionResponse` |
| PUT | `/{id}` | Update settlement | `SettlementTransactionResponse` |
| DELETE | `/{id}` | Delete settlement | `204 No Content` |
| GET | `/closings/{pokerManagerId}` | Get closings grouped by date | `SettlementClosingsGroupedResponse` |

---

## Import & Reconciliation

### ImportedTransaction

Handles importing transactions from external files (OFX bank statements, Excel spreadsheets).

**Base Route**: `/api/v1/importedtransaction`

#### Import Endpoints

| Method | Route | Description | Request | Response |
|--------|-------|-------------|---------|----------|
| POST | `/import/ofx` | Import OFX bank statement | `ImportOfxRequest` (form-data) | `ImportSummaryResponse` |
| POST | `/import/excel` | Import generic Excel file | `ImportExcelRequest` (form-data) | `ImportSummaryResponse` |
| POST | `/import/excel/buy` | Import buy transactions | `ImportExcelRequest` (form-data) | `ImportSummaryResponse` |
| POST | `/import/excel/sell` | Import sell transactions | `ImportExcelRequest` (form-data) | `ImportSummaryResponse` |
| POST | `/import/excel/transfer` | Import transfer transactions | `ImportExcelRequest` (form-data) | `ImportSummaryResponse` |

#### Reconciliation Endpoints

| Method | Route | Description | Request | Response |
|--------|-------|-------------|---------|----------|
| POST | `/reconcile` | Reconcile imported transaction | `ReconcileTransactionRequest` | `ReconciliationResponse` |
| POST | `/find-matches` | Find potential matches | `FindMatchesRequest` | `PotentialMatchesResponse` |

#### Query Endpoints

| Method | Route | Description | Response |
|--------|-------|-------------|----------|
| GET | `/` | List all imported transactions | `List<ImportedTransactionResponse>` |
| GET | `/{id}` | Get imported transaction by ID | `ImportedTransactionResponse` |
| GET | `/file/{fileName}/asset-holder/{baseAssetHolderId}` | Get transactions by file | `FileTransactionsResponse` |
| GET | `/unreconciled/asset-holder/{baseAssetHolderId}` | Get unreconciled transactions | `List<ImportedTransactionResponse>` |
| GET | `/dashboard/asset-holder/{baseAssetHolderId}` | Get import dashboard | `ImportedTransactionDashboard` |

---

## Supporting Resources

### Category

Manages transaction categories for classification.

**Base Route**: `/api/v1/category`

#### Endpoints

| Method | Route | Description | Response |
|--------|-------|-------------|----------|
| GET | `/` | List all categories (hierarchical) | `List<CategoryResponse>` |
| GET | `/{id}` | Get category by ID | `CategoryResponse` |
| POST | `/` | Create category | `CategoryResponse` |
| PUT | `/{id}` | Update category | `CategoryResponse` |
| DELETE | `/{id}` | Delete category | `204 No Content` |

---

### InitialBalance

Manages initial balance records for asset holders.

**Base Route**: `/api/v1/initialbalance`

#### Endpoints

| Method | Route | Description | Request | Response |
|--------|-------|-------------|---------|----------|
| GET | `/` | List all initial balances | - | `List<InitialBalanceResponse>` |
| GET | `/{id}` | Get initial balance by ID | - | `InitialBalanceResponse` |
| POST | `/asset-type` | Set balance for asset type | `InitialBalanceRequest` | `InitialBalanceResponse` |
| POST | `/asset-group` | Set balance for asset group | `InitialBalanceRequest` | `InitialBalanceResponse` |
| POST | `/unified` | Set balance (auto-detect type) | `InitialBalanceRequest` | `InitialBalanceResponse` |
| POST | `/validate` | Validate balance data | `InitialBalanceRequest` | `List<string>` |
| GET | `/asset-holder/{id}` | Get all balances for holder | - | `List<InitialBalanceResponse>` |
| GET | `/asset-holder/{id}/asset-types` | Get balances by asset type | - | `Dictionary<AssetType, InitialBalanceResponse>` |
| GET | `/asset-holder/{id}/asset-groups` | Get balances by asset group | - | `Dictionary<AssetGroup, InitialBalanceResponse>` |
| GET | `/asset-holder/{id}/asset-type/{type}/effective-balance` | Get effective balance for type | - | `decimal?` |
| GET | `/asset-holder/{id}/asset-group/{group}/effective-balance` | Get effective balance for group | - | `decimal?` |
| GET | `/asset-holder/{id}/summary` | Get balance summary | - | `InitialBalanceSummary` |
| GET | `/asset-holder/{id}/has-balances` | Check if has balances | - | `bool` |
| GET | `/asset-holder/{id}/count` | Get balance count | - | `int` |
| DELETE | `/asset-holder/{id}/asset-type/{type}` | Remove balance for type | - | `204 No Content` |
| DELETE | `/asset-holder/{id}/asset-group/{group}` | Remove balance for group | - | `204 No Content` |

---

## Diagnostics

### DiagnosticsController

Provides system diagnostics and monitoring endpoints. **Requires admin role.**

**Base Route**: `/api/v1/diagnostics`

#### Endpoints

| Method | Route | Description | Authorization | Response |
|--------|-------|-------------|---------------|----------|
| GET | `/cache-stats` | Get cache hit/miss statistics | `Role:admin` | `CacheStatistics` |

#### Cache Statistics Response

```json
{
  "categories": {
    "AvgRate": {
      "hits": 500,
      "misses": 50,
      "hitRate": 0.909
    },
    "SystemWallets": {
      "hits": 150,
      "misses": 10,
      "hitRate": 0.9375
    },
    "PokerManagerLookup": {
      "hits": 200,
      "misses": 20,
      "hitRate": 0.909
    }
  }
}
```

#### Response Properties

| Property | Type | Description |
|----------|------|-------------|
| `categories` | `Dictionary<string, CategoryStats>` | Cache statistics by category |
| `hits` | `long` | Number of cache hits |
| `misses` | `long` | Number of cache misses |
| `hitRate` | `double` | Ratio of hits to total requests (0-1) |

#### Usage

Monitor cache performance to identify optimization opportunities:

```http
GET /api/v1/diagnostics/cache-stats
Authorization: Bearer {admin-token}
```

**See Also:** [RATE_LIMITING_AND_PERFORMANCE.md](../05_INFRASTRUCTURE/RATE_LIMITING_AND_PERFORMANCE.md) for cache implementation details.

---

## Error Handling

### Error Response Format

All errors follow the RFC 7807 Problem Details standard:

```json
{
  "title": "Error Title",
  "status": 400,
  "detail": "Detailed error message",
  "extensions": {
    "requestId": "0HN5QKJI8J9RA:00000001",
    "timestamp": "2025-01-14T12:00:00Z",
    "code": "ERROR_CODE"
  }
}
```

### HTTP Status Codes

| Code | Description | When Used |
|------|-------------|-----------|
| 200 | OK | Successful GET/PUT |
| 201 | Created | Successful POST |
| 204 | No Content | Successful DELETE |
| 400 | Bad Request | Validation errors, invalid input |
| 401 | Unauthorized | Missing or invalid auth token |
| 403 | Forbidden | Insufficient permissions |
| 404 | Not Found | Entity not found |
| 409 | Conflict | Business rule violation, duplicate entity |
| 500 | Internal Server Error | Unexpected server error |

### Validation Error Response

```json
{
  "title": "Validation Failed",
  "status": 400,
  "errors": {
    "Name": ["Name is required", "Name must be at least 3 characters"],
    "Email": ["Invalid email format"]
  },
  "extensions": {
    "requestId": "0HN5QKJI8J9RA:00000001",
    "timestamp": "2025-01-14T12:00:00Z"
  }
}
```

### Exception Types

| Exception | Status Code | Description |
|-----------|-------------|-------------|
| `ValidationException` | 400 | Request validation failed |
| `EntityNotFoundException` | 404 | Entity not found in database |
| `DuplicateEntityException` | 409 | Entity with same unique constraint exists |
| `BusinessRuleException` | 409 | Business rule violation |
| `BusinessException` | 400 | General business error |

---

## Common Request/Response Models

### Request Models

See individual endpoint documentation for specific request model properties.

### Pagination Response

```json
{
  "data": [...],
  "total": 150
}
```

### Common Query Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `assetType` | AssetType enum | Filter by asset type |
| `assetGroup` | AssetGroup enum | Filter by asset group |
| `quantity` | int | Page size (default varies) |
| `page` | int | Page number (0-indexed) |

---

## Deprecated Endpoints Migration

The following endpoints are marked `[Obsolete]` and will be removed in API v2. Use the unified `/api/v1/transfer` endpoint instead.

### Migration Table

| Controller | Old Endpoint | New Endpoint |
|------------|-------------|--------------|
| ClientController | `POST /{id}/send-brazilian-real` | `POST /api/v1/transfer` |
| MemberController | `POST /{id}/send-brazilian-real` | `POST /api/v1/transfer` |
| PokerManagerController | `POST /{id}/send-brazilian-real` | `POST /api/v1/transfer` |

### Migration Example

**Old:**
```http
POST /api/v1/client/{clientId}/send-brazilian-real
Content-Type: application/json
Authorization: Bearer {token}

{
  "receiverId": "member-guid",
  "amount": 1000.00,
  "date": "2026-01-22"
}
```

**New:**
```http
POST /api/v1/transfer
Content-Type: application/json
Authorization: Bearer {token}

{
  "senderAssetHolderId": "client-guid",
  "receiverAssetHolderId": "member-guid",
  "assetType": 21,
  "amount": 1000.00,
  "date": "2026-01-22T00:00:00Z"
}
```

### Benefits of Migration

1. **Unified API**: Single endpoint for all transfer types
2. **Bank Restrictions**: Automatic enforcement of bank restrictions in TRANSFER mode
3. **Wallet Creation**: Optional auto-creation with confirmation flow
4. **Better Error Handling**: Detailed error responses with wallet creation information
5. **Future-Proof**: New features will be added to `/transfer` only

For detailed documentation, see [TRANSACTION_API_ENDPOINTS.md](./TRANSACTION_API_ENDPOINTS.md).

---

## Related Documentation

- [TRANSACTION_API_ENDPOINTS.md](./TRANSACTION_API_ENDPOINTS.md) - **Detailed transaction API reference**
- [AUTHENTICATION.md](../05_INFRASTRUCTURE/AUTHENTICATION.md) - Authentication details
- [ENUMS_AND_TYPE_SYSTEM.md](../07_REFERENCE/ENUMS_AND_TYPE_SYSTEM.md) - Enum definitions
- [ERROR_HANDLING.md](../05_INFRASTRUCTURE/ERROR_HANDLING.md) - Exception handling details
- [VALIDATION_SYSTEM.md](../05_INFRASTRUCTURE/VALIDATION_SYSTEM.md) - Validation rules

