# Transaction Balance Impact

> **Status:** Single Source of Truth  
> **Created:** January 24, 2026  
> **Purpose:** Define exactly how each transaction type impacts balances

---

## Table of Contents

- [Overview](#overview)
- [Balance Calculation Methods](#balance-calculation-methods)
- [FiatAssetTransaction](#fiatasettransaction)
- [DigitalAssetTransaction](#digitalassettransaction)
  - [With BalanceAs (Standard)](#with-balanceas-standard)
  - [Without BalanceAs (Coin Balance)](#without-balanceas-coin-balance)
  - [Self-Conversion](#self-conversion)
- [SettlementTransaction](#settlementtransaction)
- [Transaction Mode Examples](#transaction-mode-examples)
- [Sign Convention Reference](#sign-convention-reference)
- [Implementation Reference](#implementation-reference)

---

## Overview

Balance calculation in SF Management follows these principles:

1. **Double-Entry:** Every transaction has a sender and receiver
2. **Direction by Role:** Sender gets negative, Receiver gets positive
3. **Classification Adjustment:** Different AccountClassifications may invert signs
4. **Rate Application:** Digital transactions may apply rates or conversions

---

## Balance Calculation Methods

| Method | Used By | Groups By | Service |
|--------|---------|-----------|---------|
| `GetBalancesByAssetType` | Client, Member, Bank | AssetType (BRL, PokerStars, etc.) | BaseAssetHolderService |
| `GetBalancesByAssetGroup` | PokerManager | AssetGroup (FiatAssets, PokerAssets, etc.) | BaseAssetHolderService |

**Data Sources:**
1. Initial Balances
2. FiatAssetTransactions
3. DigitalAssetTransactions
4. SettlementTransactions

---

## FiatAssetTransaction

**Used For:** Bank transfers, PIX, cash movements

**Balance Formula:**

```
For each wallet involved:
  signedAmount = IsSender ? -AssetAmount : +AssetAmount
  
  If different AccountClassifications AND wallet is LIABILITY:
    signedAmount = -signedAmount
```

### Example: RECEIPT (Client pays Bank)

```
Transaction: Client → Bank (BRL)
AssetAmount: 5000

Balance Impacts:
├─ Client (LIABILITY): +5000 (debt reduced)
└─ Bank (ASSET): +5000 (company has money)

Why both positive?
- Client sends (-5000 base) but is LIABILITY with different classification → inverted to +5000
- Bank receives (+5000 base) as ASSET → stays +5000
```

### Example: PAYMENT (Bank pays Client)

```
Transaction: Bank → Client (BRL)
AssetAmount: 5000

Balance Impacts:
├─ Bank (ASSET): -5000 (company has less money)
└─ Client (LIABILITY): -5000 (company owes less OR client owes more)
```

---

## DigitalAssetTransaction

**Used For:** Poker chips, cryptocurrency

### With BalanceAs (Standard)

When `BalanceAs` and `ConversionRate` are set, the transaction converts balance to a different asset type.

**Balance Formula:**

```
For Client/Member (GetBalancesByAssetType):
  signedAmount = GetSignedAmountForWallet() * ConversionRate
  balances[BalanceAs] += signedAmount
  
  Note: NO impact on transaction's original AssetType
```

### Example: SALE with BalanceAs

```
Business: Client BUYS 1000 chips at 5.0 BRL

Transaction: PokerManager → Client (PokerStars)
AssetAmount: 1000
BalanceAs: BRL
ConversionRate: 5.0

Balance Impacts:
├─ PokerManager PokerAssets: -1000 (company inventory decreased)
├─ Client PokerAssets: NO CHANGE (chips not in their balance!)
└─ Client FiatAssets (BRL): -5000 (owes company 5000 BRL)
```

> **Key Insight:** With BalanceAs, client's debt is in BRL, not in chips.

### Example: PURCHASE with BalanceAs

```
Business: Client SELLS 1000 chips at 4.9 BRL

Transaction: Client → PokerManager (PokerStars)
AssetAmount: 1000
BalanceAs: BRL
ConversionRate: 4.9

Balance Impacts:
├─ Client PokerAssets: NO CHANGE
├─ Client FiatAssets (BRL): +4900 (company owes client 4900 BRL)
└─ PokerManager PokerAssets: +1000 (company inventory increased)
```

---

### Without BalanceAs (Coin Balance)

When `BalanceAs` is null (checkbox "Troca ou saldo em fichas"), debt is in the same asset type.

**Balance Formula:**

```
For Client/Member (GetBalancesByAssetType):
  signedAmount = GetSignedAmountForWallet()
  If Rate > 0:
    signedAmount = signedAmount * (100 / (100 + Rate))
  balances[AssetType] += signedAmount

For PokerManager (GetBalancesByAssetGroup):
  signedAmount = GetSignedAmountForWallet()
  balances[AssetGroup] += signedAmount
```

> **Important:** In this model, Rate adjustment is applied to Client/Member balances. PokerManager inventory keeps the raw transfer amount. Rate fee is accounted for separately in finance/profit (`RateFees`).

### Example: SALE without BalanceAs (Coin Balance)

```
Business: Client BUYS 1000 chips with 5% rate

Transaction: PokerManager → Client (PokerStars)
AssetAmount: 1050 (raw transfer includes embedded fee)
BalanceAs: null
Rate: 5

Balance Impacts:
├─ PokerManager PokerAssets: -1050 (raw transfer amount)
└─ Client PokerAssets: -1000 (1050 × 100/105)

Note: Client owes IN CHIPS, not in BRL
```

---

### Self-Conversion (CONVERSION Mode)

When a PokerManager moves between Internal and PokerAssets wallets with BalanceAs.
This is represented by the **CONVERSION** transaction mode in the frontend.

**Trigger Conditions:**
1. Both wallets belong to same PokerManager
2. One is `AssetGroup.Internal` (conversion wallet), other is `AssetGroup.PokerAssets`
3. `BalanceAs` is set (typically BRL)
4. `ConversionRate` is set

**Frontend Usage:**
```tsx
<AssetTransactionModal
  transactionMode="CONVERSION"
  creatorAssetHolderId={manager.baseAssetHolderId}
  creatorAssetHolderType="PokerManager"
/>
```

**Balance Formula:**

```
Direction: Internal → PokerAssets (chips entering system)
  PokerAssets += AssetAmount * (100 - Rate) / 100
  FiatAssets += AssetAmount * ConversionRate

Direction: PokerAssets → Internal (chips leaving system)
  PokerAssets -= AssetAmount * (100 - Rate) / 100
  FiatAssets -= AssetAmount * ConversionRate
```

### Example: Manager Self-Conversion

```
Business: PokerManager deposits own 1000 chips

Transaction: Internal → PokerAssets (PokerStars)
AssetAmount: 1000
BalanceAs: BRL
ConversionRate: 5.0

Balance Impacts:
├─ PokerManager PokerAssets: +1000 (company now holds chips)
└─ PokerManager FiatAssets: +5000 (company owes manager BRL)

Settlement: Later, company pays manager 5000 BRL via FiatTransaction
```

---

## SettlementTransaction

**Used For:** Poker settlements with rake, commission, and rakeback

### ⚠️ Critical Rule

**`AssetAmount` should NOT be used for balance calculation.**

The chips represented by `AssetAmount` already flowed via `DigitalAssetTransactions`. Using it again causes double-counting.

### Correct Balance Formula

```
For Client (GetBalancesByAssetType):
  If RakeBack > 0:
    balances[BrazilianReal] += RakeAmount * (RakeBack / 100)

For PokerManager (GetBalancesByAssetGroup):
  balances[FiatAssets] -= RakeAmount * (RakeCommission / 100)
```

### Example: Settlement with Rake

```
Settlement Transaction:
├─ RakeAmount: 1000 (chips client paid to poker site)
├─ RakeCommission: 50% (poker site pays company)
└─ RakeBack: 10% (company returns to client)

Balance Impacts (BRL):
├─ Client FiatAssets (BRL): +100 (receives 1000 × 10% rakeback)
└─ PokerManager FiatAssets (BRL): -500 (company earns 1000 × 50%)

Company Profit: 1000 × ((50 - 10) / 100) = 400 chips
```

---

## Transaction Mode Examples

### SALE (Client Buys Chips)

```
Mode: SALE
Flow: PokerManager → Client
Asset: PokerStars

With BalanceAs (BRL, Rate 5.0):
├─ PM PokerAssets: -1000
├─ Client PokerAssets: NO CHANGE
└─ Client FiatAssets: -5000 (debt in BRL)

Without BalanceAs (Rate 5%):
├─ PM PokerAssets: -1050 (raw transfer amount)
└─ Client PokerAssets: -1000 (net after embedded fee)
```

### PURCHASE (Client Sells Chips)

```
Mode: PURCHASE
Flow: Client → PokerManager
Asset: PokerStars

With BalanceAs (BRL, Rate 4.9):
├─ Client PokerAssets: NO CHANGE
├─ Client FiatAssets: +4900 (credit in BRL)
└─ PM PokerAssets: +1000

Without BalanceAs (Rate 5%):
├─ Client PokerAssets: +1000 (1050 × 100/105)
└─ PM PokerAssets: +1050 (raw transfer amount)
```

### Statement Display Rule (Rate Transactions)

For statement rows (`GetTransactionsStatementForAssetHolder`):

- `AssetAmount` remains the **raw signed value** (what was actually transferred)
- `RateFeeAmount` exposes the embedded fee portion when `Rate > 0` and `BalanceAs = null`
- UI should show a fee indicator (example: `Taxa: 20.00 fichas (2%)`) to explain why balance impact differs from raw amount

### RECEIPT (Client Pays Company)

```
Mode: RECEIPT
Flow: Client → Bank
Asset: BRL
Amount: 5000

Balance Impacts:
├─ Client FiatAssets: +5000 (debt reduced)
└─ Bank FiatAssets: +5000 (company has money)
```

### PAYMENT (Company Pays Client)

```
Mode: PAYMENT
Flow: Bank → Client
Asset: BRL
Amount: 5000

Balance Impacts:
├─ Bank FiatAssets: -5000 (company has less)
└─ Client FiatAssets: -5000 (company owes less)
```

### TRANSFER (P2P between Internal Wallets)

> **Restriction (January 2026):** TRANSFER mode only allows **Internal wallets** (AssetGroup 4). For other asset groups, use the appropriate mode (SALE/PURCHASE for PokerAssets, RECEIPT/PAYMENT for FiatAssets).

```
Mode: TRANSFER
Flow: Client A Internal Wallet → Client B Internal Wallet
Asset: Internal wallet (AssetGroup 4)

Balance Impacts:
├─ Client A: -Amount
└─ Client B: +Amount

Restrictions:
├─ Banks cannot participate
└─ Only Internal wallets allowed (AssetGroup 4)
```

### SELF_TRANSFER (Same Holder)

```
Mode: SELF_TRANSFER (INTERNAL)
Flow: Wallet A → Wallet B (same holder)

Balance Impacts:
├─ Wallet A: -Amount
└─ Wallet B: +Amount
└─ Net change: 0 (just moves between wallets)
```

---

## Sign Convention Reference

### Base Sign

```csharp
public decimal GetSignedAmountForWalletIdentifier(Guid walletId)
{
    if (SenderWalletIdentifierId == walletId)
        return -AssetAmount;  // Outgoing
    
    if (ReceiverWalletIdentifierId == walletId)
        return AssetAmount;   // Incoming
}
```

### AccountClassification Adjustment

```csharp
if (!tx.HaveBothWalletsSameAccountClassification() && 
    tx.IsWalletIdentifierLiability(relevantWalletId))
{
    signedAmount = -signedAmount;
}
```

### When to Invert

| Sender Class | Receiver Class | Sender Sign | Receiver Sign |
|--------------|----------------|-------------|---------------|
| ASSET | ASSET | - | + |
| LIABILITY | LIABILITY | - | + |
| ASSET | LIABILITY | - | - |
| LIABILITY | ASSET | + | + |

---

## Implementation Reference

### Code Location

| Logic | File | Method |
|-------|------|--------|
| Sign calculation | `BaseTransaction.cs` | `GetSignedAmountForWalletIdentifier` |
| By AssetType | `BaseAssetHolderService.cs` | `GetBalancesByAssetType` |
| By AssetGroup | `BaseAssetHolderService.cs` | `GetBalancesByAssetGroup` |
| Self-conversion | `BaseAssetHolderService.cs` | Inside `GetBalancesByAssetGroup` |

### Known Issue

⚠️ **SettlementTransaction Bug:** Current implementation incorrectly uses `AssetAmount`. See [BALANCE_SYSTEM_ANALYSIS.md](../10_REFACTORING/BALANCE_SYSTEM_ANALYSIS.md) for fix plan.

---

## Related Documentation

| Topic | Document |
|-------|----------|
| Entity Behaviors | [ENTITY_BUSINESS_BEHAVIOR.md](./ENTITY_BUSINESS_BEHAVIOR.md) |
| Transaction System | [TRANSACTION_INFRASTRUCTURE.md](./TRANSACTION_INFRASTRUCTURE.md) |
| Settlement Process | [SETTLEMENT_WORKFLOW.md](./SETTLEMENT_WORKFLOW.md) |
| Balance Endpoints | [BALANCE_ENDPOINTS.md](../06_API/BALANCE_ENDPOINTS.md) |
| Balance Analysis | [BALANCE_SYSTEM_ANALYSIS.md](../10_REFACTORING/BALANCE_SYSTEM_ANALYSIS.md) |

---

*Last updated: January 24, 2026*
