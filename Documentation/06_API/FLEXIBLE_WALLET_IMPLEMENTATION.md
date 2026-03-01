# Flexible Wallets Guide

## Overview

**Flexible wallets** are wallet identifiers that belong to `AssetPool`s with `AssetGroup.Flexible`.
They are designed for system/company operations where strict metadata is not required.

Compared to external-facing wallets, flexible wallets:
- skip metadata validation
- can be used for system and conversion workflows
- are created explicitly via a dedicated endpoint

---

## Classification

A wallet is flexible when:

```csharp
walletIdentifier.AssetPool.AssetGroup == AssetGroup.Flexible
```

### Characteristics

| Feature | External Wallets | Flexible Wallets |
|---------|------------------|------------------|
| Metadata required | Yes (type-specific) | No |
| Typical use case | Client/third-party accounts | System and conversion operations |
| Validation | Strict metadata rules | Metadata validation bypass |

---

## Creation Endpoint

```http
POST /api/v1/WalletIdentifier/flexible-wallet
```

Request example:

```json
{
  "baseAssetHolderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "assetType": "BrazilianReal",
  "accountClassification": "Asset"
}
```

This flow:
1. Finds or creates an `AssetPool` with `AssetGroup.Flexible`
2. Associates the new wallet to that pool
3. Applies flexible-wallet validation rules

---

## System Wallets and Conversion Wallets

Flexible wallets can behave in two main ways:

- **System wallets**: company-owned (`BaseAssetHolderId = null`)
- **Conversion wallets**: manager-owned flexible wallets used in conversion flows

### Get system wallet to pair with

```http
GET /api/v1/company/asset-pools/system-wallet-to-pair-with/{walletIdentifierId}
```

Returns a company-owned flexible wallet with matching `AssetType`.

### Get manager conversion wallets

```http
GET /api/v1/PokerManager/{id}/conversion-wallets
```

Returns flexible wallets owned by the manager that can be used in conversion flows.

---

## Validation Behavior

Flexible wallets bypass metadata requirements similarly to settlement wallets.

| AssetGroup | Required Metadata |
|------------|-------------------|
| FiatAssets | Bank metadata |
| PokerAssets | Poker metadata |
| CryptoAssets | Crypto metadata |
| Flexible | None |
| Settlements | None |

---

## Transaction Usage

Flexible wallets are used in:
- system operations with company wallets
- conversion flows (`Flexible` ↔ `PokerAssets`)
- transfer/self-transfer flows where frontend allows `AssetGroup 4`

> `INTERNAL` transaction mode (self-transfer code) is different from `AssetGroup.Flexible` (wallet category).

---

## System Wallet Accounting Behavior

System wallets (company-owned Flexible wallets) have special accounting rules that differ from entity-owned Flexible wallets.

### Identification

A wallet is a **system wallet** when:
```csharp
walletIdentifier.AssetPool.BaseAssetHolderId == null &&
walletIdentifier.AssetPool.AssetGroup == AssetGroup.Flexible
```

### AccountClassification

System wallets use `AccountClassification = Liability` (2). This is critical for correct balance impact calculations when transacting with entity wallets (Asset classification).

### Transaction Direction Rules

When creating transactions between system wallets and entity wallets (Bank, PokerManager):

| Operation Type | System Wallet Position | Entity Balance Impact |
|----------------|----------------------|----------------------|
| **Income** (Compra, Recebimento) | Sender | +Amount (entity balance UP) |
| **Expense** (Venda, Pagamento) | Receiver | -Amount (entity balance DOWN) |

### Why Direction Matters

The sender/receiver assignment determines the sign of balance impact:

1. **System as Sender (Income):**
   - System Wallet (Liability) sends → Base: -Amount
   - Entity Wallet (Asset) receives → Base: +Amount
   - Different classifications → Liability inverted: +Amount
   - Result: Entity gains (correct for income)

2. **System as Receiver (Expense):**
   - Entity Wallet (Asset) sends → Base: -Amount
   - System Wallet (Liability) receives → Base: +Amount
   - Different classifications → Liability inverted: -Amount (not inverted for Asset sender)
   - Result: Entity loses (correct for expense)

### Statement Display

When a transaction involves a system wallet:
- `counterPartyName` returns "Unknown" (no BaseAssetHolder)
- UI should display the **category name** instead
- Use `shouldShowSystemCategoryOrigin(transaction)` helper in frontend

### Future: Accounting Module

System wallet balance tracking is not currently implemented. When the accounting module is developed:
- System wallet balances will be tracked
- Consolidated financial reports will use system wallet data
- Double-entry bookkeeping will be enforced

---

## Related Documentation

- [ENUMS_AND_TYPE_SYSTEM.md](../07_REFERENCE/ENUMS_AND_TYPE_SYSTEM.md)
- [ASSET_INFRASTRUCTURE.md](../03_CORE_SYSTEMS/ASSET_INFRASTRUCTURE.md)
- [TRANSACTION_INFRASTRUCTURE.md](../03_CORE_SYSTEMS/TRANSACTION_INFRASTRUCTURE.md)
- [COMPANY_ASSET_POOL_ENDPOINTS.md](./COMPANY_ASSET_POOL_ENDPOINTS.md)
- [VALIDATION_SYSTEM.md](../05_INFRASTRUCTURE/VALIDATION_SYSTEM.md)
