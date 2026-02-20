# Profit Calculation System

## Overview

The Profit Calculation System computes company revenue from four distinct sources. Each source has its own formula, data dependencies, and applies to specific entity types. This document explains the full calculation pipeline—from domain model through service logic to API response—so a reader can trace any profit number back to the exact database records and formulas that produced it.

---

## Table of Contents

1. [System Implementation Constant](#system-implementation-constant)
2. [Domain Prerequisites](#domain-prerequisites)
3. [Revenue Sources In Depth](#revenue-sources-in-depth)
   - [1. Direct Income](#1-direct-income)
   - [2. Rake Commission](#2-rake-commission)
   - [3. Rate Fees](#3-rate-fees)
   - [4. Spread Profit](#4-spread-profit)
4. [AvgRate — Weighted Average Cost Basis](#avgrate--weighted-average-cost-basis)
   - [What AvgRate Represents](#what-avgrate-represents)
   - [Who Needs AvgRate](#who-needs-avgrate)
   - [Calculation Algorithm](#calculation-algorithm)
   - [Caching Strategy](#caching-strategy)
   - [Cache Invalidation](#cache-invalidation)
5. [Service Architecture](#service-architecture)
6. [API Endpoints](#api-endpoints)
7. [Data Flow Diagrams](#data-flow-diagrams)
8. [Edge Cases and Guards](#edge-cases-and-guards)
9. [Related Documentation](#related-documentation)

---

## System Implementation Constant

**File:** `Domain/Common/SystemImplementation.cs`

All finance logic shares a single date constant that defines the earliest point the system considers valid financial data:

```csharp
public static class SystemImplementation
{
    public static readonly DateTime FinanceDataStartDateUtc = new(2025, 7, 17, 0, 0, 0, DateTimeKind.Utc);
}
```

This constant is used in two places:

1. **API default dates** — When profit endpoints are called without `startDate`, it defaults to `FinanceDataStartDateUtc`. When called without `endDate`, it defaults to today UTC.
2. **AvgRate backward lookback floor** — The iterative AvgRate algorithm stops walking backward when it reaches a month before `FinanceDataStartDateUtc`, preventing unnecessary lookback into non-existent data.

Update this value if historical data migration extends the earliest data point.

---

## Domain Prerequisites

Before understanding profit calculation, these domain concepts must be clear:

### ManagerProfitType

**File:** `Domain/Enums/ManagerProfitType.cs`

```csharp
public enum ManagerProfitType
{
    Spread = 0,
    RakeOverrideCommission = 1
}
```

Each `PokerManager` has a `ManagerProfitType?` property. This determines which profit sources apply to them:

| Profit Type | Spread Profit | Rake Commission | Rate Fees | Direct Income |
|------------|:---:|:---:|:---:|:---:|
| `Spread` | ✅ | ❌ | ✅ | ✅ |
| `RakeOverrideCommission` | ❌ | ✅ | ✅ | ✅ |

### System Wallets

System wallets are wallet identifiers belonging to asset pools where `AssetGroup = Internal` and `BaseAssetHolderId = null`. They represent company-owned wallets. Their IDs are used to identify Direct Income transactions.

**How they are resolved** (`ProfitCalculationService.GetSystemWalletIds`):

```csharp
var walletIds = await (
    from wallet in _context.WalletIdentifiers.AsNoTracking()
    join pool in _context.AssetPools.AsNoTracking()
        on wallet.AssetPoolId equals pool.Id
    where !wallet.DeletedAt.HasValue
          && !pool.DeletedAt.HasValue
          && pool.AssetGroup == AssetGroup.Internal
          && pool.BaseAssetHolderId == null
    select wallet.Id
).ToListAsync();
```

System wallet IDs are cached for 10 minutes.

### Key Transaction Fields

| Entity | Field | Purpose in Profit Calculation |
|--------|-------|-------------------------------|
| `BaseTransaction` | `AssetAmount` | The quantity of assets transferred |
| `BaseTransaction` | `SenderWalletIdentifierId` | Who sends |
| `BaseTransaction` | `ReceiverWalletIdentifierId` | Who receives |
| `BaseTransaction` | `CategoryId` | Must be present for Direct Income |
| `DigitalAssetTransaction` | `ConversionRate` | Sale price per chip (BRL); used in Spread Profit |
| `DigitalAssetTransaction` | `Rate` | Embedded fee percentage; used in Rate Fees |
| `SettlementTransaction` | `RakeAmount` | Chips raked from the table |
| `SettlementTransaction` | `RakeCommission` | Manager's commission percentage |
| `SettlementTransaction` | `RakeBack` | Percentage returned to players (nullable) |

---

## Revenue Sources In Depth

### 1. Direct Income

**What it is:** Revenue (or expense) from categorized transactions that involve a system wallet.

**Applies to:** All managers (not filtered by `ManagerProfitType`).

**Source tables:** `FiatAssetTransaction`, `DigitalAssetTransaction`

**Filter criteria:**
- `DeletedAt` is null
- `Date` within the requested range
- `CategoryId` is not null (categorized transactions only)
- At least one side of the transaction is a system wallet

**Direction logic:**
- System wallet is **receiver** → Revenue (positive)
- System wallet is **sender** → Expense (negative)

**BRL conversion for digital assets:**
Digital asset transactions are converted to BRL using the AvgRate of the non-system counterparty at the transaction date:

```
BRL Value = AssetAmount × AvgRate(counterpartyManagerId, transactionDate)
```

Fiat transactions are already in BRL; no conversion needed.

**Code:** `ProfitCalculationService.CalculateDirectIncome()`

---

### 2. Rake Commission

**What it is:** Company's share of the rake collected at poker tables, applicable only to managers configured as `RakeOverrideCommission`.

**Applies to:** Managers with `ManagerProfitType = RakeOverrideCommission` only.

**Source table:** `SettlementTransaction`

**Filter criteria:**
- `DeletedAt` is null
- `Date` within the requested range
- `RakeAmount > 0`
- Transaction involves a wallet belonging to a target `RakeOverrideCommission` manager

**Formula:**

```
RakeProfit (chips) = RakeAmount × ((RakeCommission - RakeBack) / 100)
RakeProfit (BRL)   = RakeProfit (chips) × AvgRate(managerId, transactionDate)
```

**Step-by-step example:**

| Field | Value |
|-------|-------|
| `RakeAmount` | 1,000 chips |
| `RakeCommission` | 50% |
| `RakeBack` | 10% |
| `AvgRate` at tx date | 5.25 BRL/chip |

```
Chips earned = 1,000 × ((50 - 10) / 100) = 400 chips
BRL earned   = 400 × 5.25 = 2,100 BRL
```

**Important:** `RakeBack` is nullable. When null, it defaults to 0 in the formula.

**Code:** `ProfitCalculationService.CalculateRakeCommission()`

---

### 3. Rate Fees

**What it is:** Revenue extracted from transactions that have an embedded fee rate. The rate is a percentage built into the chip amount — the actual chips traded already include the fee.

**Applies to:** All managers (not filtered by `ManagerProfitType`).

**Source table:** `DigitalAssetTransaction`

**Filter criteria:**
- `DeletedAt` is null
- `Date` within the requested range
- `Rate` is not null and not zero

**Formula:**

```
FeeInChips = AssetAmount × (Rate / (100 + Rate))
FeeInBRL   = FeeInChips × AvgRate(managerId, transactionDate)
```

**Step-by-step example:**

| Field | Value |
|-------|-------|
| `AssetAmount` | 1,050 chips (includes the fee) |
| `Rate` | 5% |
| `AvgRate` at tx date | 5.25 BRL/chip |

```
FeeInChips = 1,050 × (5 / 105) = 50 chips
FeeInBRL   = 50 × 5.25 = 262.50 BRL
```

**Why `Rate / (100 + Rate)` instead of `Rate / 100`:** Because the rate is embedded in the total amount. If 1,000 chips + 5% fee = 1,050 chips, extracting the fee from 1,050 requires the reverse formula.

**Code:** `ProfitCalculationService.CalculateRateFees()`

---

### 4. Spread Profit

**What it is:** Profit earned by selling chips at a price higher than their weighted average acquisition cost (AvgRate). This is the core business model for Spread managers.

**Applies to:** Managers with `ManagerProfitType = Spread` only.

**Source table:** `DigitalAssetTransaction`

**Filter criteria:**
- `DeletedAt` is null
- `Date` within the requested range
- Transaction sender is a wallet belonging to a `Spread` manager (manager is selling)
- `ConversionRate` is not null (sale price must be recorded)

**Formula:**

```
SpreadProfit = AssetAmount × (ConversionRate - AvgRate(managerId, saleDate))
```

**The result is already in BRL** — no conversion needed because:
- `ConversionRate` is the sale price in BRL/chip
- `AvgRate` is the cost basis in BRL/chip
- The difference × quantity = BRL profit

**Step-by-step example:**

| Field | Value |
|-------|-------|
| `AssetAmount` | 1,000 chips |
| `ConversionRate` (sale price) | 5.50 BRL/chip |
| `AvgRate` (cost basis) | 5.25 BRL/chip |

```
SpreadProfit = 1,000 × (5.50 - 5.25) = 250 BRL
```

**Negative spread is possible:** If `ConversionRate < AvgRate`, the manager sold at a loss. This is reflected as a negative value.

**Warning logged when AvgRate = 0:**

```csharp
if (avgRate == 0)
{
    _logger.LogWarning(
        "AvgRate is 0 for manager {ManagerId} at {Date}. Spread profit may be overstated.",
        mgrId, sale.Date);
}
```

**Code:** `ProfitCalculationService.CalculateSpreadProfit()`

---

## AvgRate — Weighted Average Cost Basis

### What AvgRate Represents

AvgRate is the **weighted average cost per chip in BRL** for a Spread manager's poker asset inventory. It answers: "On average, how much did each chip currently in inventory cost?"

```
AvgRate = TotalCost / TotalChips
```

Where:
- `TotalCost` = sum of (chips acquired × price paid per chip)
- `TotalChips` = chips currently held

### Who Needs AvgRate

**Only Spread managers.** The service explicitly checks:

```csharp
public async Task<bool> RequiresAvgRateTracking(Guid assetHolderId)
{
    return await _context.PokerManagers
        .AsNoTracking()
        .AnyAsync(pm => pm.BaseAssetHolderId == assetHolderId
            && pm.ManagerProfitType == ManagerProfitType.Spread
            && !pm.DeletedAt.HasValue);
}
```

For all other entity types, `GetAvgRateAtDate()` returns `0`.

### Calculation Algorithm

**File:** `Application/Services/Finance/AvgRateService.cs`

AvgRate is calculated monthly. The algorithm is **iterative** (not recursive, to avoid stack overflow on long histories):

#### Step 1: Walk backward from the target month

Starting at the target month, walk backward in time until:
- A **cached** month snapshot is found, OR
- The **first calculable month** is reached (determined by `InitialBalance.CreatedAt` or first transaction date), OR
- The month falls before `SystemImplementation.FinanceDataStartDateUtc` (hard floor — stops lookback immediately)

Each uncached month is pushed onto a stack.

#### Step 2: Determine starting state

If first month reached, use `InitialBalance`:

```
Starting TotalChips = InitialBalance.Balance (or 0)
Starting AvgRate    = InitialBalance.ConversionRate (or 0)
Starting TotalCost  = TotalChips × AvgRate
```

If cached month found, use the cached snapshot.

#### Step 3: Calculate forward month-by-month

Pop months from the stack and process each one's transactions:

**On chip RECEIVE (manager receives chips):**

```
ReceivePrice = ConversionRate (if provided)
            OR CurrentAvgRate (if ConversionRate is null)

TotalChips += AssetAmount
TotalCost  += AssetAmount × ReceivePrice
AvgRate     = TotalCost / TotalChips
```

This makes receives without `ConversionRate` price-neutral: chips are tracked in inventory while AvgRate remains stable.

**On chip SALE (manager sends chips):**

```
proportion  = AssetAmount / TotalChips
TotalCost  -= TotalCost × proportion
TotalChips -= AssetAmount
AvgRate     = TotalCost / TotalChips  (if TotalChips > 0)
```

**Internal transfers (same manager on both sides) are skipped.**

Transactions within a month are processed in order: `ORDER BY Date, CreatedAt`.

Each completed month is cached before proceeding to the next.

#### Visual walkthrough

```
Month 1 (Initial):  TotalChips = 10,000   TotalCost = 50,000   AvgRate = 5.00
  Buy 2,000 @ 5.20:  TotalChips = 12,000   TotalCost = 60,400   AvgRate = 5.03
  Sell 3,000:         proportion = 3,000/12,000 = 0.25
                      TotalCost = 60,400 - (60,400 × 0.25) = 45,300
                      TotalChips = 9,000   AvgRate = 5.03  (unchanged)
  Buy 1,000 @ 5.50:  TotalChips = 10,000   TotalCost = 50,800   AvgRate = 5.08
  → Cache snapshot: {TotalChips: 10,000, TotalCost: 50,800, AvgRate: 5.08}

Month 2:
  Uses Month 1 snapshot as starting point...
```

### Caching Strategy

| Data | Cache Key Pattern | TTL | Scope |
|------|-------------------|-----|-------|
| Monthly snapshots | `AvgRate:{managerId}:{year}:{month}` | 24 hours | Per manager, per month |
| Manager wallet IDs | `avgrate.manager-wallet-ids:{managerId}` | 10 minutes | Per manager |
| Initial balance | `avgrate.initial-balance:{managerId}` | 10 minutes | Per manager |
| First month flag | `avgrate.first-month:{managerId}:{year}:{month}` | 10 minutes | Per manager, per month |
| System wallet IDs | `finance.system-wallet-ids` | 10 minutes | Global |
| Rake manager IDs | `profit.rake-manager-ids` | 10 minutes | Global |
| Spread manager IDs | `profit.spread-manager-ids` | 10 minutes | Global |

**Current month is never cached** — it is always calculated dynamically up to the requested date via `CalculateAvgRateUpToDate()`.

**Current-month InitialBalance seeding:** When the current month is also the manager's first calculable month, `CalculateAvgRateUpToDate()` seeds its running state from `InitialBalance` before processing transactions. Without this, the initial chip inventory and cost basis would be ignored.

### Cache Invalidation

When transactions are created, updated, or deleted, `InvalidateFromDate()` removes cache entries from the affected month through the current month:

```csharp
public async Task InvalidateFromDate(Guid pokerManagerId, DateTime fromDate)
{
    var monthToInvalidate = new DateTime(fromDate.Year, fromDate.Month, 1);
    var currentMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
    while (monthToInvalidate <= currentMonth)
    {
        _cache.Remove(GetCacheKey(pokerManagerId, monthToInvalidate.Year, monthToInvalidate.Month));
        monthToInvalidate = monthToInvalidate.AddMonths(1);
    }
}
```

This is necessary because a change in any month affects all subsequent months' AvgRate.

---

## Service Architecture

### Dependency Graph

```
ProfitController
    └── IProfitCalculationService
            ├── DataContext (EF Core)
            ├── IAvgRateService
            │       ├── DataContext (EF Core)
            │       └── IMemoryCache
            └── IMemoryCache
```

### Interface: IProfitCalculationService

**File:** `Application/Services/Finance/IProfitCalculationService.cs`

```csharp
public interface IProfitCalculationService
{
    Task<ProfitSummary> GetProfitSummary(DateTime startDate, DateTime endDate, Guid? managerId = null);
    Task<List<ProfitByManager>> GetProfitByManager(DateTime startDate, DateTime endDate);
    Task<List<ProfitBySource>> GetProfitBySource(DateTime startDate, DateTime endDate);
    Task<DirectIncomeDetailsResponse> GetDirectIncomeDetails(DateTime startDate, DateTime endDate);
}
```

### Interface: IAvgRateService

**File:** `Application/Services/Finance/IAvgRateService.cs`

```csharp
public interface IAvgRateService
{
    Task<decimal> GetAvgRateAtDate(Guid pokerManagerId, DateTime date);
    Task<AvgRateSnapshot> GetAvgRateForMonth(Guid pokerManagerId, int year, int month);
    Task InvalidateFromDate(Guid pokerManagerId, DateTime fromDate);
    void InvalidateManagerWalletCache(Guid pokerManagerId);
    Task<bool> RequiresAvgRateTracking(Guid assetHolderId);
    Task<AvgRateCalculationMode> GetCalculationMode(Guid assetHolderId, AssetGroup assetGroup);
}
```

### Response DTOs

**`ProfitSummary`** — Aggregated profit from all four sources (all values in BRL):

| Field | Type | Description |
|-------|------|-------------|
| `StartDate` | DateTime | Start of date range |
| `EndDate` | DateTime | End of date range |
| `ManagerId` | Guid? | Filter (null = all managers) |
| `DirectIncome` | decimal | Net income from system wallet transactions |
| `RakeCommission` | decimal | Rake commission for RakeOverrideCommission managers |
| `RateFees` | decimal | Extracted rate fees |
| `SpreadProfit` | decimal | Spread profit for Spread managers |
| `TotalProfit` | decimal | Computed: sum of all four sources |

**`ProfitByManager`** — Same breakdown per manager, includes `ManagerName` and `ManagerProfitType?` (nullable — reflects actual DB value, not a default).

**`ProfitBySource`** — Simple `{Source, Amount}` pairs for chart-friendly output.

**`DirectIncomeDetailsResponse`** — Itemized list of income and expense transactions with origin, category, and amounts.

**`AvgRateSnapshot`** — Monthly state: `{PokerManagerId, Year, Month, AvgRate, TotalChips, TotalCost, CalculatedAt}`.

---

## API Endpoints

**Controller:** `Api/Controllers/v1/Finance/ProfitController.cs`

**Base route:** `/api/v1/finance/profit`

| Endpoint | Method | Response Type | Description |
|----------|--------|---------------|-------------|
| `/summary` | GET | `ProfitSummary` | Aggregated profit for a date range |
| `/by-manager` | GET | `List<ProfitByManager>` | Profit breakdown per manager (sorted by TotalProfit desc) |
| `/by-source` | GET | `List<ProfitBySource>` | Profit breakdown by revenue source |
| `/direct-income-details` | GET | `DirectIncomeDetailsResponse` | Itemized direct income/expense transactions |

### Query Parameters

| Parameter | Type | Required | Used By | Default | Description |
|-----------|------|:--------:|---------|---------|-------------|
| `startDate` | DateTime | ❌ | All | `SystemImplementation.FinanceDataStartDateUtc` | Start of date range (YYYY-MM-DD) |
| `endDate` | DateTime | ❌ | All | Today (UTC) | End of date range (YYYY-MM-DD) |
| `managerId` | Guid | ❌ | `/summary` only | null | Filter to a single manager |

**Date defaults:** When dates are omitted, the system defaults to the full historical range from the system implementation date to today. This prevents silent `0001-01-01` queries.

**Manager ID normalization:** The `managerId` parameter accepts either a `BaseAssetHolderId` or a `PokerManager.Id`. The service automatically resolves `PokerManager.Id` to the corresponding `BaseAssetHolderId` internally. This prevents silent mismatches when clients pass the wrong ID type.

### Validation

All endpoints validate `startDate <= endDate` (after defaults are applied) and return `400 Bad Request` if violated.

### Example: `/summary`

**Request:**

```
GET /api/v1/finance/profit/summary?startDate=2026-01-01&endDate=2026-01-31
```

**Response:**

```json
{
  "startDate": "2026-01-01",
  "endDate": "2026-01-31",
  "managerId": null,
  "directIncome": 15000.00,
  "rakeCommission": 8500.00,
  "rateFees": 1200.00,
  "spreadProfit": 3500.00,
  "totalProfit": 28200.00
}
```

### Example: `/by-manager`

**Request:**

```
GET /api/v1/finance/profit/by-manager?startDate=2026-01-01&endDate=2026-01-31
```

**Response:**

```json
[
  {
    "managerId": "a1b2c3d4-...",
    "managerName": "Manager Alpha",
    "managerProfitType": 0,
    "directIncome": 5000.00,
    "rakeCommission": 0.00,
    "rateFees": 600.00,
    "spreadProfit": 3500.00,
    "totalProfit": 9100.00
  },
  {
    "managerId": "e5f6g7h8-...",
    "managerName": "Manager Beta",
    "managerProfitType": 1,
    "directIncome": 10000.00,
    "rakeCommission": 8500.00,
    "rateFees": 600.00,
    "spreadProfit": 0.00,
    "totalProfit": 19100.00
  }
]
```

Note: `managerProfitType: 0` = Spread, `managerProfitType: 1` = RakeOverrideCommission, `null` = not configured.

---

## Data Flow Diagrams

### Profit Summary Calculation

```
Request: GET /summary?startDate=...&endDate=...&managerId=...
    │
    ▼
ProfitController.GetProfitSummary()
    ├── ResolveDateRange() → defaults if omitted
    │
    ▼
ProfitCalculationService.GetProfitSummary()
    ├── NormalizeManagerBaseAssetHolderId() → accepts PokerManager.Id or BaseAssetHolderId
    │
    ├── CalculateDirectIncome()
    │       ├── GetSystemWalletIds()  [cached 10min]
    │       ├── Query FiatAssetTransactions (categorized, system wallet involved)
    │       ├── Query DigitalAssetTransactions (categorized, system wallet involved)
    │       └── For digital: AvgRateService.GetAvgRateAtDate() for BRL conversion
    │
    ├── CalculateRakeCommission()
    │       ├── GetManagerIdsByProfitType(RakeOverrideCommission)  [cached 10min]
    │       ├── Get manager wallet IDs
    │       ├── Query SettlementTransactions (RakeAmount > 0)
    │       └── For each: AvgRateService.GetAvgRateAtDate() for BRL conversion
    │
    ├── CalculateRateFees()
    │       ├── Query DigitalAssetTransactions (Rate != null && Rate != 0)
    │       └── For each: AvgRateService.GetAvgRateAtDate() for BRL conversion
    │
    └── CalculateSpreadProfit()
            ├── GetManagerIdsByProfitType(Spread)  [cached 10min]
            ├── For each Spread manager: get PokerAssets wallet IDs
            ├── Query DigitalAssetTransactions (manager is sender, ConversionRate != null)
            └── For each: AvgRateService.GetAvgRateAtDate() for cost basis
```

### AvgRate Calculation

```
GetAvgRateAtDate(managerId, date)
    │
    ├── RequiresAvgRateTracking(managerId)?
    │       No  → return 0
    │       Yes ↓
    │
    ├── Is current month?
    │       Yes → CalculateAvgRateUpToDate()
    │               ├── Is first month? → Seed from InitialBalance
    │               └── Process transactions dynamically (not cached)
    │       No  ↓
    │
    └── GetAvgRateForMonth(managerId, year, month)
            │
            └── CalculateMonthlySnapshotIterative()
                    │
                    ├── Walk backward: find cached month or first month
                    │       Stop if month < FinanceDataStartDateUtc
                    │       Stack uncached months
                    │
                    ├── Determine starting state (InitialBalance or cached snapshot)
                    │
                    └── Walk forward: calculate each month, cache result
                            │
                            └── CalculateSingleMonth()
                                    Process transactions chronologically:
                                    - Receive chips → add to pool
                                    - Send chips → remove proportionally
                                    - Internal transfer → skip
```

---

## Edge Cases and Guards

| Scenario | Behavior |
|----------|----------|
| Manager has no wallets | Returns 0 for that revenue source |
| No system wallets exist | Direct Income returns 0 |
| `AvgRate = 0` for Spread manager | Warning logged; spread profit will equal `Amount × ConversionRate` |
| `TotalChips` goes to 0 or negative | Clamped to 0; `TotalCost` also clamped to 0 |
| `RakeBack` is null | Treated as 0 in the formula |
| Manager has `ManagerProfitType = null` | Returned as `null` in API; excluded from Spread and Rake profit calculations |
| Date parameters omitted | Defaults to `[FinanceDataStartDateUtc, today UTC]` |
| Date range validation fails | 400 Bad Request with ProblemDetails |
| AvgRate lookback reaches before `FinanceDataStartDateUtc` | Stops walking backward; starts from zero |
| `managerId` passed as `PokerManager.Id` instead of `BaseAssetHolderId` | Auto-normalized to `BaseAssetHolderId` internally |
| `managerId` matches neither ID type | Warning logged; calculations proceed (will return zero) |
| ManagerId filter on `/summary` for wrong profit type | Rake returns 0 for Spread managers, Spread returns 0 for Rake managers |
| Current month is manager's first month | `CalculateAvgRateUpToDate()` seeds from `InitialBalance` |
| Borrow/lend cycle closes at a different price (e.g., borrow then repay higher) | Spread profit can be overstated/understated because repayment financing cost is not netted in the current model |

---

## Known Limitation: Borrow/Lend Financing Cost

The current spread profit model calculates profit per sale transaction:

```
SpreadProfit = SaleAmount × (SaleConversionRate - AvgRateAtSaleTime)
```

If chips are borrowed and later repaid at a different market price, the financing effect of the repayment leg is not explicitly netted against the earlier spread profit. This means:

- Sell leg can show positive spread profit using current AvgRate
- Later repayment buy can happen at a higher price
- Economic P&L of the full borrow/lend cycle may differ from reported spread profit

This is a known modeling limitation. The current implementation prioritizes consistent inventory/cost tracking in AvgRate and keeps financing-cycle netting as a future enhancement.

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [FINANCE_SYSTEM.md](FINANCE_SYSTEM.md) | High-level revenue model overview |
| [ASSET_VALUATION_RULES.md](../08_BUSINESS_RULES/ASSET_VALUATION_RULES.md) | InitialBalance configuration and AvgRate business rules |
| [SETTLEMENT_WORKFLOW.md](SETTLEMENT_WORKFLOW.md) | How settlements and rake are recorded |
| [ENTITY_BUSINESS_BEHAVIOR.md](ENTITY_BUSINESS_BEHAVIOR.md) | Entity types and their business roles |
| [TRANSACTION_INFRASTRUCTURE.md](TRANSACTION_INFRASTRUCTURE.md) | Transaction models and fields |
| [RATE_LIMITING_AND_PERFORMANCE.md](../05_INFRASTRUCTURE/RATE_LIMITING_AND_PERFORMANCE.md) | Caching implementation details |
| [ENUMS_AND_TYPE_SYSTEM.md](../07_REFERENCE/ENUMS_AND_TYPE_SYSTEM.md) | ManagerProfitType, AssetGroup, AssetType values |

---

*Created: February 20, 2026*
*Last updated: February 20, 2026*
