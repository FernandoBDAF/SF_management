# Company-Owned Asset Pools

## Table of Contents

- [Overview](#overview)
- [The Ownership Model](#the-ownership-model)
- [Why Company-Owned Pools?](#why-company-owned-pools)
- [Creating Company Pools](#creating-company-pools)
- [Querying Company Pools](#querying-company-pools)
- [Key Constraints](#key-constraints)
- [Best Practices](#best-practices)
- [Summary](#summary)
- [Related Documentation](#related-documentation)

---

## Overview

This document explains the **company ownership model** for asset pools in SF Management. Asset pools can belong to either a specific asset holder (Client, Bank, Member, PokerManager) or directly to the company itself.

Understanding this ownership model is essential for:
- Managing company treasury and operational accounts
- Tracking centralized asset management
- Processing transactions between company and third-party pools

---

## The Ownership Model

### How Ownership Works

An `AssetPool` has an optional `BaseAssetHolderId` that determines ownership:

| `BaseAssetHolderId` | Ownership |
|---------------------|-----------|
| `null` | Company-owned pool |
| `Guid` value | Belongs to specific asset holder |

This design allows the same infrastructure to handle both company assets and client assets uniformly.

### Response Handling

When displaying asset pool information, the system handles null owners gracefully:
- API responses show `"ownerName": "Company"` for company pools
- Wallet identifiers in company pools show `"baseAssetHolderName": "Company"`
- Transaction counterparty names display "Company" when appropriate

> **Note:** For the `AssetPool` model definition and properties, see [ASSET_INFRASTRUCTURE.md](../03_CORE_SYSTEMS/ASSET_INFRASTRUCTURE.md#assetpool).

---

## Why Company-Owned Pools?

Company-owned pools serve several purposes:

| Purpose | Description |
|---------|-------------|
| **Treasury Management** | Hold company's own fiat and crypto assets |
| **Operational Accounts** | Manage internal poker accounts and settlement wallets |
| **Centralized Assets** | Track assets directly managed by the organization |
| **Transaction Counterparty** | Serve as the "other side" for client deposits/withdrawals |

### Example Scenarios

1. **Client Deposit**: Money moves FROM company's internal wallet TO client's wallet
2. **Client Withdrawal**: Money moves FROM client's wallet TO company's internal wallet
3. **Settlement Processing**: Company pools hold settlement funds before distribution

---

## Creating Company Pools

### Dedicated API Endpoint

Company pools are created through a dedicated controller to ensure proper handling:

```http
POST /api/v1/company/asset-pools
```

```json
{
  "assetGroup": "FiatAssets",
  "description": "Main company BRL liquidity pool",
  "businessJustification": "Central treasury management"
}
```

### Why a Separate Endpoint?

The standard `AssetPoolController` **requires** `BaseAssetHolderId`:

```csharp
[Required(ErrorMessage = "BaseAssetHolderId is required. For company pools, use the CompanyAssetPoolController.")]
public Guid BaseAssetHolderId { get; set; }
```

This separation:
- Prevents accidental creation of orphaned pools
- Makes developer intent explicit
- Applies company-specific validation rules
- Provides company-specific metrics and analytics

> **Note:** For complete API endpoint documentation, see [COMPANY_ASSET_POOL_ENDPOINTS.md](../06_API/COMPANY_ASSET_POOL_ENDPOINTS.md).

---

## Querying Company Pools

### Available Methods

The `AssetPoolService` provides dedicated methods for company pools:

| Method | Description |
|--------|-------------|
| `GetCompanyAssetPools()` | Gets all company-owned pools |
| `GetCompanyAssetPoolByType(assetGroup)` | Gets company pool for specific asset group |
| `IsCompanyPool(assetPoolId)` | Checks if a pool is company-owned |
| `CreateCompanyAssetPool(...)` | Creates a company pool with metadata |
| `GetCompanyAssetPoolSummary()` | Gets summary with balances and activity |
| `GetCompanyAssetPoolAnalytics(...)` | Gets detailed period-based analytics |

### Quick Usage

```csharp
// Check if a pool is company-owned
if (assetPool.BaseAssetHolderId == null)
{
    // This is a company pool
}

// Or use the service method
var isCompany = await assetPoolService.IsCompanyPool(poolId);
```

> **Note:** For service layer implementation details, see [SERVICE_LAYER_ARCHITECTURE.md](../02_ARCHITECTURE/SERVICE_LAYER_ARCHITECTURE.md).

---

## Key Constraints

### Uniqueness Rules

| Rule | Description |
|------|-------------|
| One pool per AssetGroup per holder | A `Client` can only have one `FiatAssets` pool |
| One company pool per AssetGroup | Company can only have one `FiatAssets` pool |
| Asset holder must exist | Referenced `BaseAssetHolder` must exist and not be deleted |

### Validation

Creation and deletion are validated by `AssetPoolValidationService`:

- **Creation**: Checks for duplicate company pools
- **Deletion**: Ensures no active wallets or transactions exist

> **Note:** For validation implementation details, see [VALIDATION_SYSTEM.md](../05_INFRASTRUCTURE/VALIDATION_SYSTEM.md).

---

## Best Practices

### When to Use Company Pools

✅ **Good Use Cases:**
- Treasury accounts (main company bank accounts)
- Operational settlement accounts
- Internal transfer wallets
- Escrow/holding accounts

### When NOT to Use Company Pools

❌ **Avoid for:**
- Client assets (always use client-owned pools)
- Partner accounts (create appropriate asset holder types)
- Temporary test data (use proper test asset holders)

### Naming Conventions

Since company pools don't have a named owner, use meaningful wallet metadata:

```csharp
wallet.SetMetadataFromFields(
    bankName: "Treasury - Operating Account",
    pixKey: "treasury@company.com"
);
```

---

## Summary

Company-owned asset pools provide a way to manage assets that belong directly to the organization.

**Key Points:**

| Aspect | Detail |
|--------|--------|
| Identification | `BaseAssetHolderId = null` |
| Limit | One company pool per `AssetGroup` |
| Creation | Use `CompanyAssetPoolController` |
| Display | Owner shows as "Company" in responses |
| Transactions | Full support with balance calculations |

This model enables clean separation between company assets and third-party assets while maintaining a unified transaction infrastructure.

---

## Related Documentation

| Topic | Document |
|-------|----------|
| Asset Pool Model | [ASSET_INFRASTRUCTURE.md](../03_CORE_SYSTEMS/ASSET_INFRASTRUCTURE.md) |
| Company Pool API Endpoints | [COMPANY_ASSET_POOL_ENDPOINTS.md](../06_API/COMPANY_ASSET_POOL_ENDPOINTS.md) |
| Internal Wallets | [INTERNAL_WALLET_TYPE_IMPLEMENTATION.md](../06_API/INTERNAL_WALLET_TYPE_IMPLEMENTATION.md) |
| Service Layer | [SERVICE_LAYER_ARCHITECTURE.md](../02_ARCHITECTURE/SERVICE_LAYER_ARCHITECTURE.md) |
| AutoMapper Mappings | [AUTOMAPPER_CONFIGURATION.md](../02_ARCHITECTURE/AUTOMAPPER_CONFIGURATION.md) |
| Validation System | [VALIDATION_SYSTEM.md](../05_INFRASTRUCTURE/VALIDATION_SYSTEM.md) |
| Transaction Infrastructure | [TRANSACTION_INFRASTRUCTURE.md](../03_CORE_SYSTEMS/TRANSACTION_INFRASTRUCTURE.md) |
