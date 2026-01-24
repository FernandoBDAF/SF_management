# Internal Wallets Guide

## Table of Contents

- [Overview](#overview)
- [How Internal Wallets Work](#how-internal-wallets-work)
- [Creating Internal Wallets](#creating-internal-wallets)
- [Validation Rules](#validation-rules)
- [Querying Internal Wallets](#querying-internal-wallets)
- [Optional Metadata](#optional-metadata)
- [Use Cases](#use-cases)
- [Settlement Wallets](#settlement-wallets)
- [Best Practices](#best-practices)
- [Related Documentation](#related-documentation)

---

## Overview

**Internal wallets** are wallet identifiers that belong to `AssetPool`s with `AssetGroup.Internal`. They are designed for company-internal operations where traditional wallet metadata (bank details, poker accounts, crypto addresses) is not applicable or required.

Unlike external-facing wallets, internal wallets have no mandatory metadata fields, making them flexible for various administrative and operational purposes.

---

## How Internal Wallets Work

### Asset Group Classification

Internal wallets are identified by their parent `AssetPool`'s `AssetGroup`. A wallet is considered "internal" when:

```csharp
walletIdentifier.AssetPool.AssetGroup == AssetGroup.Internal
```

> **Note:** For the complete `AssetGroup` enum definition and all available values, see [ENUMS_AND_TYPE_SYSTEM.md](../07_REFERENCE/ENUMS_AND_TYPE_SYSTEM.md#assetgroup).

### Key Characteristics

| Feature | External Wallets | Internal Wallets |
|---------|------------------|------------------|
| Metadata required | Yes (type-specific) | No |
| AssetType restriction | Must match AssetGroup | Any AssetType allowed |
| Use case | Client/third-party accounts | Company operations |
| Validation | Strict metadata rules | No metadata validation |

---

## Creating Internal Wallets

### API Endpoint

```http
POST /api/v1/WalletIdentifier/internal-wallet
```

**Request Body:**

```json
{
  "baseAssetHolderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "assetType": "BrazilianReal",
  "accountClassification": "ASSET"
}
```

**Response:** `201 Created` with the created `WalletIdentifierResponse`

This endpoint automatically:
1. Creates or finds an `AssetPool` with `AssetGroup.Internal` for the asset holder
2. Associates the new wallet with that pool
3. Skips metadata validation requirements

### How It Works

The `WalletIdentifierService.AddWithAssetGroup()` method handles internal wallet creation by:

1. Looking for an existing `AssetPool` with `AssetGroup.Internal` for the asset holder
2. Creating a new pool if none exists
3. Associating the wallet identifier with that pool

> **Note:** For service layer patterns and the `AddWithAssetGroup()` implementation, see [SERVICE_LAYER_ARCHITECTURE.md](../02_ARCHITECTURE/SERVICE_LAYER_ARCHITECTURE.md).

---

## Validation Rules

### Metadata Validation Exemption

Internal wallets bypass all metadata validation. The `ValidateMetadata()` method returns `true` for `AssetGroup.Internal` without checking any fields.

### AssetType Flexibility

Internal pools can accept **any** `AssetType`. This means an Internal pool can contain wallets for BrazilianReal, PokerStars, Bitcoin, or any other asset type.

### Validation Comparison

| Asset Group | Required Metadata | AssetType Restriction |
|-------------|-------------------|----------------------|
| FiatAssets | `BankName`, `PixKey` | BrazilianReal, USDollar |
| PokerAssets | `InputForTransactions` | PokerStars, GgPoker, etc. |
| CryptoAssets | `WalletAddress`, `WalletCategory` | Bitcoin, Ethereum, etc. |
| **Internal** | **None** | **Any** |
| **Settlements** | **None** | **Any** |

> **Note:** For detailed validation implementation, see [VALIDATION_SYSTEM.md](../05_INFRASTRUCTURE/VALIDATION_SYSTEM.md).

---

## Querying Internal Wallets

### Get All Internal Wallets

```csharp
var internalWallets = await context.WalletIdentifiers
    .Include(wi => wi.AssetPool)
        .ThenInclude(ap => ap.BaseAssetHolder)
    .Where(wi => wi.AssetPool.AssetGroup == AssetGroup.Internal && 
                 !wi.DeletedAt.HasValue)
    .ToListAsync();
```

### Find System Wallet to Pair With

When creating transactions, use this endpoint to find a company-owned system wallet as counterparty:

```http
GET /api/v1/company/asset-pools/system-wallet-to-pair-with/{walletIdentifierId}
```

This method:
- Takes an external wallet identifier ID
- Returns a company-owned internal wallet with matching `AssetType`
- Throws an error if the input wallet is already internal

**Use case:** When recording a client deposit, find the company's system wallet to use as the sender.

---

## Optional Metadata

While not required, internal wallets support flexible custom metadata for tracking purposes.

### Setting and Getting Metadata

```csharp
// Set custom metadata
wallet.SetInternalMetadata("Purpose", "Inter-department transfer");
wallet.SetInternalMetadata("Department", "Finance");

// Get metadata
var purpose = wallet.GetInternalMetadata("Purpose");
```

These methods only work when `AssetGroup == Internal`, returning `null` or doing nothing for other asset groups.

### Recommended Metadata Fields

Even though not required, consider adding descriptive metadata:

```json
{
  "Purpose": "Department operating account",
  "Department": "Finance",
  "CreatedBy": "admin@company.com",
  "CreatedAt": "2025-01-14T10:30:00Z"
}
```

This helps with audit trails, filtering, and understanding wallet purpose.

---

## Use Cases

### 1. System Wallets (Company-Owned)

```csharp
var treasuryWallet = new WalletIdentifier
{
    BaseAssetHolderId = companyAssetHolderId,
    AssetType = AssetType.BrazilianReal,
    AccountClassification = AccountClassification.ASSET
};
var result = await walletIdentifierService.AddWithAssetGroup(
    treasuryWallet, 
    AssetGroup.Internal);
```

### 2. Client Transaction Counterparty

```csharp
// Find internal wallet to use for client deposit
var internalWallet = await walletIdentifierService
    .GetSystemWalletToPairWith(clientWalletId);

// Create deposit transaction
var deposit = new DigitalAssetTransaction
{
    SenderWalletIdentifierId = internalWallet.Id,  // Company's internal wallet
    ReceiverWalletIdentifierId = clientWalletId,   // Client's wallet
    AssetAmount = 1000.00m
};
```

### 3. Conversion Wallets (PokerManager-Owned)

Internal wallets can also be created for PokerManagers to support self-conversion flows:

```csharp
var conversionWallet = new WalletIdentifier
{
    BaseAssetHolderId = pokerManagerId,
    AssetType = AssetType.PokerStars,
    AccountClassification = AccountClassification.ASSET
};
var result = await walletIdentifierService.AddWithAssetGroup(
    conversionWallet,
    AssetGroup.Internal);
```

These wallets are used to trigger dual-balance impact (PokerAssets + FiatAssets) when
`BalanceAs` and `ConversionRate` are set on a DigitalAssetTransaction.

### 4. Inter-Department Transfers

```csharp
var transaction = new FiatAssetTransaction
{
    Date = DateTime.UtcNow,
    SenderWalletIdentifierId = deptAWallet.Id,      // Internal wallet
    ReceiverWalletIdentifierId = deptBWallet.Id,    // Internal wallet
    AssetAmount = 10000.00m
};
```

---

## Settlement Wallets

The system also supports `AssetGroup.Settlements` which works similarly to Internal:

```http
POST /api/v1/WalletIdentifier/settlement-wallet
```

Settlement wallets:
- Have no metadata validation requirements
- Can accept any `AssetType`
- Are designed for poker settlement operations

---

## Best Practices

### When to Use Internal Wallets

✅ **Good Use Cases:**
- Company treasury accounts
- Inter-department transfers
- Administrative operations
- System-generated transactions
- Temporary holding accounts

❌ **Avoid for:**
- Client-facing accounts
- External third-party wallets
- Accounts requiring audit metadata

### Integration Notes

Internal wallets work seamlessly with all transaction types:
- **FiatAssetTransaction** - Internal fiat movements
- **DigitalAssetTransaction** - Internal poker/crypto movements
- **SettlementTransaction** - Poker settlements

Balance calculations handle internal wallets by mapping them to the appropriate asset group based on `AssetType`.

> **Note:** TRANSFER and INTERNAL transaction modes may use existing Internal wallets, but these modes do not create Internal wallets.

---

## Related Documentation

| Topic | Document |
|-------|----------|
| AssetGroup Enum | [ENUMS_AND_TYPE_SYSTEM.md](../07_REFERENCE/ENUMS_AND_TYPE_SYSTEM.md#assetgroup) |
| Validation System | [VALIDATION_SYSTEM.md](../05_INFRASTRUCTURE/VALIDATION_SYSTEM.md) |
| AutoMapper Mappings | [AUTOMAPPER_CONFIGURATION.md](../02_ARCHITECTURE/AUTOMAPPER_CONFIGURATION.md) |
| Service Layer | [SERVICE_LAYER_ARCHITECTURE.md](../02_ARCHITECTURE/SERVICE_LAYER_ARCHITECTURE.md) |
| Company Asset Pools | [COMPANY_ASSET_POOL_ENDPOINTS.md](COMPANY_ASSET_POOL_ENDPOINTS.md) |
| Asset Infrastructure | [ASSET_INFRASTRUCTURE.md](../03_CORE_SYSTEMS/ASSET_INFRASTRUCTURE.md) |
