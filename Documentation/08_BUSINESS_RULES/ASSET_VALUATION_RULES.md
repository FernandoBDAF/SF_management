# Asset Valuation Rules

> **Status:** Active  
> **Created:** January 28, 2026  
> **Purpose:** Document balance calculation modes and AvgRate usage rules

---

## Table of Contents

1. [Overview](#overview)
2. [InitialBalance as Configuration](#initialbalance-as-configuration)
3. [Mutual Exclusivity Rules](#mutual-exclusivity-rules)
4. [AvgRate Rules (Spread Managers Only)](#avgrate-rules-spread-managers-only)
5. [Consolidation Assumptions](#consolidation-assumptions)
6. [Examples](#examples)

---

## Overview

Balances are calculated either per AssetType (default) or consolidated by AssetGroup.
The calculation mode is configured by `InitialBalance`.

---

## InitialBalance as Configuration

`InitialBalance` has a dual purpose:

1. **Starting Balance** for calculations
2. **Configuration** that determines balance mode

### Mode A: Per-AssetType (Default)

- No AssetGroup InitialBalance exists
- AssetType InitialBalance entries are allowed (one per asset type)
- Each AssetType is tracked independently

### Mode B: Consolidated by AssetGroup

- AssetGroup InitialBalance exists (AssetType = None)
- All AssetTypes in that group are consolidated into a single balance

---

## Mutual Exclusivity Rules

```
IF InitialBalance(AssetHolder, AssetGroup=X) EXISTS
THEN InitialBalance(AssetHolder, AssetType∈X) is BLOCKED

IF InitialBalance(AssetHolder, AssetType=Y) EXISTS (where Y belongs to group X)
THEN InitialBalance(AssetHolder, AssetGroup=X) is BLOCKED
```

These rules prevent conflicting configurations for the same AssetGroup.

---

## AvgRate Rules (Spread Managers Only)

AvgRate exists **only** to calculate Spread profit.

```
Spread Profit = AssetAmount × (SaleConversionRate - AvgRate)
```

### Required For
- Managers with `ManagerProfitType = Spread`
- Even if no InitialBalance exists (AvgRate starts at 0)

### Not Required For
- Managers with `ManagerProfitType = RakeOverrideCommission`
- Managers with `ManagerProfitType = null` (not configured)
- Clients
- Banks
- Members

### Starting AvgRate

```
IF InitialBalance.ConversionRate > 0 AND BalanceAs is set:
    StartingAvgRate = ConversionRate
    StartingChips = Balance
ELSE:
    StartingAvgRate = 0
    StartingChips = Balance (if provided)
```

This seeding applies in **both** calculation paths:
- Monthly iterative snapshot (`CalculateMonthlySnapshotIterative`)
- Current-month dynamic calculation (`CalculateAvgRateUpToDate`)

### Receive Pricing Rule (No ConversionRate)

For Spread managers, all incoming chips are tracked in AvgRate inventory:

```
IF ConversionRate is provided:
    ReceivePrice = ConversionRate
ELSE:
    ReceivePrice = CurrentAvgRate (TotalCost / TotalChips, or 0 if empty)

TotalChips += AssetAmount
TotalCost  += AssetAmount × ReceivePrice
```

This keeps inventory accurate and is price-neutral when `ConversionRate` is missing (AvgRate does not jump because chips are added at the current average cost).

### Known Limitation: Borrow/Lend Financing Cycles

When chips are borrowed and later repaid at a different price, spread profit is still calculated per sale leg using AvgRate at sale time. Repayment financing cost is not explicitly netted as part of the same cycle. This can create a temporary mismatch between economic P&L and reported spread profit in borrow/lend-heavy flows.

### Backward Lookback Floor

The AvgRate algorithm walks backward month-by-month to find cached data or the first calculable month. A hard floor prevents lookback before the system implementation date:

```
Stop if month < SystemImplementation.FinanceDataStartDateUtc (2025-07-17)
```

**File:** `Domain/Common/SystemImplementation.cs`

---

## Consolidation Assumptions

For consolidated AssetGroups:

- **PokerAssets**: All platforms are valued at 1:1 USD and can be summed directly
- **CryptoAssets** (future): Must document if the same assumption applies
- **If a platform uses a different base currency**: consolidation must not be used; switch to per-AssetType mode

---

## Examples

### Example 1: PokerManager with Consolidated PokerAssets

```
InitialBalance:
BaseAssetHolderId = PM1
AssetGroup = PokerAssets
AssetType = None
Balance = 10,000 chips
BalanceAs = BRL
ConversionRate = 5.0
```

Result:
- All poker platforms are consolidated
- AvgRate starts at 5.0 BRL/chip

### Example 2: Client with Per-AssetType Balances

```
InitialBalance:
BaseAssetHolderId = Client1
AssetType = PokerStars
AssetGroup = None
Balance = 1,000 chips
```

Result:
- PokerStars balance tracked independently
- No consolidation

---

## Related Documentation

- `Documentation/10_REFACTORING/ASSET_VALUATION_IMPLEMENTATION_PLAN.md`
- `Documentation/04_SUPPORTING_SYSTEMS/INITIAL_BALANCES.md`
- `Documentation/03_CORE_SYSTEMS/ENTITY_BUSINESS_BEHAVIOR.md`
