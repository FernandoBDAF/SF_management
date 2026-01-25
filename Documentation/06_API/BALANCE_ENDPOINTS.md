# Balance Endpoints

> **Status:** Current Implementation Review  
> **Last Updated:** January 23, 2026  
> **Scope:** Backend balance calculation logic and API endpoints

---

## Overview

All asset holder balance endpoints follow the **`/{entity}/{id}/balance`** pattern.
There are **no** `/{entity}/balance/{id}` endpoints in the current backend.

| Entity        | Route                            | Grouping Strategy | Response Keys     |
|---------------|----------------------------------|-------------------|-------------------|
| Bank          | `GET /api/v1/bank/{id}/balance`         | By AssetType      | AssetType names   |
| Client        | `GET /api/v1/client/{id}/balance`       | By AssetType      | AssetType names   |
| Member        | `GET /api/v1/member/{id}/balance`       | By AssetType      | AssetType names   |
| Poker Manager | `GET /api/v1/pokermanager/{id}/balance` | By AssetGroup     | AssetGroup names  |

---

## Endpoint Details

### Generic Balance Endpoint (Banks, Clients, Members)

**Route:** `GET /api/v1/{entity}/{id}/balance`

**Controller:** `BaseAssetHolderController.GetBalance`

```csharp
[HttpGet("{id}/balance")]
[ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, NoStore = false)]
[ProducesResponseType(typeof(Dictionary<string, decimal>), StatusCodes.Status200OK)]
public virtual async Task<IActionResult> GetBalance(Guid id)
```

**Service:** `BaseAssetHolderService.GetBalancesByAssetType(Guid baseAssetHolderId)`

**Response:** `Dictionary<string, decimal>` where keys are `AssetType` enum names.

**Example Response:**
```json
{
  "BrazilianReal": 15000.50,
  "USDollar": 2500.00,
  "PokerStars": 1200.00,
  "GgPoker": 800.00
}
```

---

### Poker Manager Balance Endpoint (Override)

**Route:** `GET /api/v1/pokermanager/{id}/balance`

**Controller:** `PokerManagerController.GetBalance` (overrides base)

```csharp
[HttpGet("{id}/balance")]
public override async Task<IActionResult> GetBalance(Guid id)
{
    var balancesByAssetGroup = await _pokerManagerService.GetBalancesByAssetGroup(id);
    
    // Convert AssetGroup enum keys to strings for the response
    var response = balancesByAssetGroup.ToDictionary(
        kvp => kvp.Key.ToString(),
        kvp => kvp.Value
    );
    
    return Ok(response);
}
```

**Service:** `BaseAssetHolderService.GetBalancesByAssetGroup(Guid baseAssetHolderId)`

**Response:** `Dictionary<string, decimal>` where keys are `AssetGroup` enum names.

**Example Response:**
```json
{
  "FiatAssets": 25000.00,
  "PokerAssets": 8500.00,
  "CryptoAssets": 1200.00,
  "Settlements": -3500.00
}
```

---

## Enums Reference

### AssetType (used by Banks, Clients, Members)

```csharp
public enum AssetType
{
    None = 0,
    
    // Fiat
    BrazilianReal = 21,
    USDollar = 22,
    
    // Poker (USD-denominated)
    PokerStars = 101,
    GgPoker = 102,
    YaPoker = 103,
    AmericasCardRoom = 104,
    SupremaPoker = 105,
    AstroPayICash = 106,
    LuxonPoker = 107,
    CredBrasil = 108,
    
    // Crypto
    Bitcoin = 201,
    Ethereum = 202,
    Litecoin = 203,
    Ripple = 204,
    BitcoinCash = 205,
    Stellar = 206
}
```

### AssetGroup (used by Poker Managers)

```csharp
public enum AssetGroup
{
    None = 0,
    FiatAssets = 1,
    PokerAssets = 2,
    CryptoAssets = 3,
    Internal = 4,
    Settlements = 5
}
```

### AccountClassification (affects sign calculation)

```csharp
public enum AccountClassification
{
    ASSET = 1,
    LIABILITY = 2,
    EQUITY = 3,
    REVENUE = 4,
    EXPENSE = 5
}
```

---

## Balance Calculation Logic

### Data Sources

Both methods (`GetBalancesByAssetType` and `GetBalancesByAssetGroup`) aggregate balances from:

1. **Initial Balances** - Pre-configured starting balances for the asset holder
2. **Fiat Transactions** - `FiatAssetTransaction` records
3. **Digital Transactions** - `DigitalAssetTransaction` records
4. **Settlement Transactions** - `SettlementTransaction` records (see special rules below)

### ⚠️ Settlement Transaction Balance Logic

> **CRITICAL BUG (Known Issue):** Current implementation incorrectly uses `AssetAmount` for settlement balance calculation. This causes double-counting because the actual chip movements already flow via `DigitalAssetTransactions`. See [BALANCE_SYSTEM_ANALYSIS.md](../10_REFACTORING/BALANCE_SYSTEM_ANALYSIS.md) for fix plan.

**Correct Balance Calculation:**

| Method | Entity Type | Formula |
|--------|-------------|---------|
| `GetBalancesByAssetType` | Client/Member | `+RakeAmount × (RakeBack / 100)` |
| `GetBalancesByAssetGroup` | PokerManager | `-RakeAmount × (RakeCommission / 100)` |

**Field Meanings:**
- `RakeAmount`: Total rake client paid to poker site (chips)
- `RakeCommission`: % of rake poker site pays company
- `RakeBack`: % of rake company returns to client

**Why `AssetAmount` Should NOT Be Used:**
The `AssetAmount` represents chip flow that is already recorded in `DigitalAssetTransactions`. Using it again in settlement balance calculation results in double-counting.

### Sign Convention (Core Logic)

The base sign is calculated in `BaseTransaction.GetSignedAmountForWalletIdentifier`:

```csharp
public decimal GetSignedAmountForWalletIdentifier(Guid walletIdentifierId)
{
    if (SenderWalletIdentifierId == walletIdentifierId)
        return -AssetAmount; // Outgoing (negative)
    
    if (ReceiverWalletIdentifierId == walletIdentifierId)
        return AssetAmount; // Incoming (positive)
        
    throw new ArgumentException("Wallet identifier is not involved in this transaction");
}
```

**Rule:** Sender gets negative amount, Receiver gets positive amount.

### Account Classification Adjustment

When sender and receiver wallets have **different** `AccountClassification`, an adjustment is applied:

```csharp
var signedAmount = tx.GetSignedAmountForWalletIdentifier(relevantWalletId);

// Signal inversion for liability wallet when account classifications differ
if (!tx.HaveBothWalletsSameAccountClassification() && 
    tx.IsWalletIdentifierLiability(relevantWalletId))
{
    signedAmount = -signedAmount;
}
```

**Helper Methods:**

```csharp
public bool HaveBothWalletsSameAccountClassification()
{
    return SenderWalletIdentifier!.AccountClassification == 
           ReceiverWalletIdentifier!.AccountClassification;
}

public bool IsWalletIdentifierLiability(Guid walletIdentifierId)
{
    if (SenderWalletIdentifierId == walletIdentifierId)
        return SenderWalletIdentifier!.AccountClassification == AccountClassification.LIABILITY;
    
    return ReceiverWalletIdentifier!.AccountClassification == AccountClassification.LIABILITY;
}
```

**Business Rule:** Asset accounts follow the transaction's natural sign. Liability accounts get inverted signs when interacting with a different account type.

### Digital Transaction Rate Adjustment

For `DigitalAssetTransaction`, rates are applied:

**By AssetType (clients, banks, members):**
```csharp
// With BalanceAs and ConversionRate
if (tx.BalanceAs != null && tx.ConversionRate != null)
{
    balances[tx.BalanceAs.Value] += signedAmount * tx.ConversionRate.Value;
    continue;
}

// Standard rate adjustment
balances[assetType] += signedAmount / ((100 + (tx.Rate ?? 0)) / 100);
```

**By AssetGroup (poker managers):**
```csharp
balances[assetGroup] += signedAmount * (100 - (tx.Rate ?? 0)) / 100;
```

### Internal AssetGroup Handling (Poker Managers Only)

When processing transactions for poker managers, `Internal` asset group is remapped:

```csharp
if (assetGroup == AssetGroup.Internal)
{
    assetGroup = tx.ReceiverWalletIdentifier!.AssetType == AssetType.BrazilianReal 
        ? AssetGroup.FiatAssets 
        : AssetGroup.PokerAssets;
}
```

### Special Case: PokerManager Self-Conversion (Dual-Balance Impact)

When a `DigitalAssetTransaction` meets all of the following conditions:
- Both wallets belong to the same PokerManager
- One wallet is `AssetGroup.Internal`
- The other wallet is `AssetGroup.PokerAssets`
- `BalanceAs` is set
- `ConversionRate` is set

Then the transaction impacts **two** asset groups:
- **PokerAssets:** `AssetAmount` (chips held)
- **FiatAssets:** `AssetAmount * ConversionRate` (BRL owed)

This dual impact is required so the PokerManager reflects both the chips held and the BRL owed, and is settled later by a Fiat transaction.

**Example:**
```
Internal (PokerStars) → PokerAssets (PokerStars)
AssetAmount: 1000
BalanceAs: BRL
ConversionRate: 5.0

Result:
PokerAssets: +1000
FiatAssets: +5000
```

> See [TRANSACTION_INFRASTRUCTURE.md](../03_CORE_SYSTEMS/TRANSACTION_INFRASTRUCTURE.md#pokermanager-self-conversion) for the full trigger logic and business context.

---

## Initial Balances

Both methods check for pre-configured initial balances:

**By AssetType:**
```csharp
foreach (var initialBalance in assetHolder.InitialBalances)
{
    if (initialBalance.AssetType != AssetType.None && initialBalance.AssetGroup == AssetGroup.None)
        balances[initialBalance.AssetType] = initialBalance.Balance;
}
```

**By AssetGroup:**
```csharp
foreach (var initialBalance in assetHolder.InitialBalances)
{
    if (initialBalance.AssetGroup != AssetGroup.None && initialBalance.AssetType == AssetType.None)
        balances[initialBalance.AssetGroup] = initialBalance.Balance;
}
```

---

## Response Caching

All balance endpoints include response caching:

```csharp
[ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, NoStore = false)]
```

Balances are cached for **60 seconds** at any cache location.

---

## Error Handling

| Status Code | Condition |
|-------------|-----------|
| 200 OK | Balance retrieved successfully |
| 404 Not Found | Asset holder ID does not exist |
| 400 Bad Request | Business rule violation |
| 500 Internal Server Error | Unexpected error |

---

## Not Implemented Endpoints

The following route patterns do **NOT exist** in the backend:

- `GET /api/v1/bank/balance/{id}` ❌
- `GET /api/v1/client/balance/{id}` ❌
- `GET /api/v1/manager/balance/{id}` ❌
- `POST /api/v1/{entity}/{id}/balance` ❌ (no date-filtered balance)

Any frontend code referencing these routes will receive **404 Not Found**.

---

## Related Code Files

| File | Purpose |
|------|---------|
| `Api/Controllers/Base/BaseAssetHolderController.cs` | Generic `GetBalance` endpoint |
| `Api/Controllers/v1/AssetHolders/PokerManagerController.cs` | Poker manager override |
| `Application/Services/Base/BaseAssetHolderService.cs` | Balance calculation logic |
| `Domain/Entities/Transactions/BaseTransaction.cs` | Sign calculation helpers |
| `Domain/Entities/Assets/WalletIdentifier.cs` | AccountClassification property |
| `Domain/Enums/Assets/AssetType.cs` | AssetType enum definition |
| `Domain/Enums/Assets/AssetGroup.cs` | AssetGroup enum definition |
| `Domain/Enums/Assets/AccountClassification.cs` | Account classification enum |

---

## Frontend Consumption

See `SF_management-front/documentation/03_CORE_SYSTEMS/BALANCE_DISPLAY_USAGE.md` for details on how the frontend consumes these endpoints and known inconsistencies.

---

## Follow-up Items

1. **Date-filtered balances:** Frontend planilha expects date-filtered balance endpoints that don't exist
2. **Typed DTO:** Consider returning a typed balance DTO instead of raw `Dictionary<string, decimal>`
3. **Frontend alignment:** Align frontend finance module with correct `/{entity}/{id}/balance` routes

---

*Last updated: January 23, 2026*
