# Finance Module Vision

> **Status:** Active Development  
> **Last Updated:** January 27, 2026  
> **Purpose:** Consolidated vision and design for the Finance Module

---

## Table of Contents

- [Executive Summary](#executive-summary)
- [Company Revenue Model](#company-revenue-model)
- [BRL Conversion Requirement](#brl-conversion-requirement)
- [Revenue Source 1: Direct Income](#revenue-source-1-direct-income)
- [Revenue Source 2: Rake Commission](#revenue-source-2-rake-commission)
- [Revenue Source 3: Rate Fees](#revenue-source-3-rate-fees)
- [Revenue Source 4: Spread Profit](#revenue-source-4-spread-profit)
- [AvgRate System Design](#avgrate-system-design)
- [Financial Statement (Planilha)](#financial-statement-planilha)
- [Implementation Phases](#implementation-phases)
- [Design Decisions](#design-decisions)
- [Related Documentation](#related-documentation)

---

## Executive Summary

The Finance Module provides **company financial tracking and reporting**:

| Capability | Description | Status |
|------------|-------------|--------|
| **Balance Sheet View** | Assets vs Liabilities across all entities | ✅ Working |
| **Date-Filtered Balances** | Historical views by month | ⬜ Phase 1 |
| **Direct Income Tracking** | Categorized revenues/expenses | ✅ Working (needs BRL conversion) |
| **Rake Commission Tracking** | For RakeOverrideCommission managers | ⬜ Phase 2 |
| **Rate Fee Tracking** | Fee profit from transactions | ⬜ Phase 2 |
| **Spread Profit Tracking** | For Spread managers (AvgRate-based) | ⬜ Phase 2 |

---

## Company Revenue Model

The company earns revenue from **four primary sources**:

```
┌─────────────────────────────────────────────────────────────────────────┐
│                       COMPANY REVENUE SOURCES                            │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  1. DIRECT INCOME                                                        │
│     └── Categorized transactions (system operations)                     │
│         Source: Any transaction with Category and is SystemOperation     │
│         Direction: Determined by System Wallet position                  │
│         Unit: May be BRL or PokerAssets (needs AvgRate conversion)       │
│                                                                          │
│  2. RAKE COMMISSION (for ManagerProfitType.RakeOverrideCommission)       │
│     └── Percentage of rake from SettlementTransactions                   │
│         Source: SettlementTransaction                                    │
│         Formula: RakeAmount × ((RakeCommission - RakeBack) / 100)        │
│         Unit: PokerAssets (needs AvgRate conversion to BRL)              │
│                                                                          │
│  3. RATE FEES                                                            │
│     └── Fee charged on chip transactions                                 │
│         Source: DigitalAssetTransaction.Rate                             │
│         Formula: AssetAmount × (Rate / (100 + Rate))                     │
│         Unit: PokerAssets (needs AvgRate conversion to BRL)              │
│                                                                          │
│  4. SPREAD PROFIT (for ManagerProfitType.Spread)                         │
│     └── Difference between sale rate and cost basis (AvgRate)            │
│         Source: DigitalAssetTransaction (SALE type)                      │
│         Formula: AssetAmount × (ConversionRate - AvgRate)                │
│         Unit: BRL (calculated directly)                                  │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### ManagerProfitType Determines Calculation Method

```csharp
public enum ManagerProfitType
{
    Spread = 0,                    // Use AvgRate-based spread calculation
    RakeOverrideCommission = 1     // Use Settlement rake commission
}
```

| ManagerProfitType | Primary Revenue Source | Calculation Method |
|-------------------|------------------------|-------------------|
| `Spread` (0) | Spread Profit | Sale transactions: `Amount × (SaleRate - AvgRate)` |
| `RakeOverrideCommission` (1) | Rake Commission | Settlements: `RakeAmount × ((RakeCommission - RakeBack) / 100)` |

---

## BRL Conversion Requirement

### The Problem

Three revenue sources produce profit in **PokerAssets units** (chips), not BRL:

| Source | Output Unit | Needs Conversion |
|--------|-------------|------------------|
| Direct Income | May be PokerAssets | ✅ If not BRL |
| Rake Commission | PokerAssets | ✅ Always |
| Rate Fees | PokerAssets | ✅ Always |
| Spread Profit | BRL | ❌ No |

### The Solution: AvgRate for BRL Conversion

```
BRL Value = AssetAmount × AvgRate(at transaction date)
```

**Example:**
```
Company receives 500 PokerStars credits as commission
├── AssetAmount: 500 chips
├── AssetType: PokerStars (not BRL)
├── AvgRate at transaction date: 5.20 BRL/chip
└── BRL Value: 500 × 5.20 = 2,600 BRL
```

This means **AvgRate is required for ALL profit calculations**, not just Spread profit.

---

## Revenue Source 1: Direct Income

### Description

Direct income consists of **categorized transactions** (system operations) representing direct revenue or expenses not tied to chip trading profit.

### Source

- **Entity:** Any transaction type with `CategoryId IS NOT NULL` **AND** involving a System Wallet
- **System Operation:** Identified by having a **System Wallet** on either sender or receiver side
- **Category:** Used as a **label** for classification, NOT for direction
- **Direction:** Determined by the **System Wallet position** in the transaction

> **Note:** A System Wallet is a wallet belonging to an `AssetPool` with `AssetGroup.Internal` and `BaseAssetHolderId = NULL` (no owner = company system wallet).

### Determining Direction

```
┌─────────────────────────────────────────────────────────────────────────┐
│  DIRECTION LOGIC (based on System Wallet position)                       │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  System Wallet is SENDER (money going OUT):                              │
│  └── Transaction is EXPENSE (Despesa)                                    │
│      Company is paying/sending value to another entity                   │
│                                                                          │
│  System Wallet is RECEIVER (money coming IN):                            │
│  └── Transaction is REVENUE (Receita)                                    │
│      Company is receiving value from another entity                      │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### Calculation

```csharp
public decimal CalculateDirectIncomeValue(Transaction tx, decimal avgRate)
{
    decimal brlValue;
    
    // Convert to BRL if needed
    if (tx.AssetType == AssetType.BRL)
    {
        brlValue = tx.AssetAmount;
    }
    else
    {
        // Non-BRL asset needs AvgRate conversion
        brlValue = tx.AssetAmount * avgRate;
    }
    
    // Determine direction based on System Wallet position
    bool systemWalletIsSender = IsSystemWallet(tx.SenderWalletIdentifierId);
    
    if (systemWalletIsSender)
    {
        return -brlValue;  // EXPENSE: Money going OUT
    }
    else
    {
        return brlValue;   // REVENUE: Money coming IN
    }
}
```

### Example

```
Transaction: Company receives 500 PokerStars credits as commission
├── Category: "Referral Commission" (label only)
├── SenderWallet: Client's PokerStars wallet
├── ReceiverWallet: SYSTEM WALLET (company)
├── AssetAmount: 500 chips
├── AssetType: PokerStars (not BRL)
│
├── Direction: System wallet is RECEIVER → REVENUE
├── BRL Conversion: 500 × AvgRate(5.20) = 2,600 BRL
└── Direct Income: +2,600 BRL
```

---

## Revenue Source 2: Rake Commission

### Description

For PokerManagers with `ManagerProfitType.RakeOverrideCommission`, the company earns from the rake collected during settlements.

### Source

- **Entity:** `SettlementTransaction`
- **Fields:** `RakeAmount`, `RakeCommission`, `RakeBack`
- **Applies To:** Managers with `ManagerProfitType = 1 (RakeOverrideCommission)`

### Calculation

```
RakeProfit (in chips) = RakeAmount × ((RakeCommission - RakeBack) / 100)
RakeProfit (in BRL) = RakeProfit (in chips) × AvgRate
```

### Example

```
SettlementTransaction:
├── RakeAmount: 1,000 chips
├── RakeCommission: 50%
├── RakeBack: 10%
├── RakeProfit in chips: 1,000 × ((50 - 10) / 100) = 400 chips
│
├── AvgRate at transaction date: 5.20 BRL/chip
└── RakeProfit in BRL: 400 × 5.20 = 2,080 BRL
```

---

## Revenue Source 3: Rate Fees

### Description

When a transaction includes a `Rate` (fee percentage), the company earns the fee portion.

### Source

- **Entity:** `DigitalAssetTransaction`
- **Field:** `Rate` (decimal, percentage)
- **Caveat:** Transactions with `Rate` do **NOT** have `ConversionRate`

### Calculation

The `AssetAmount` **includes** the fee:

```
Fee (in chips) = AssetAmount × (Rate / (100 + Rate))
Fee (in BRL) = Fee (in chips) × AvgRate
```

### Example

```
DigitalAssetTransaction:
├── AssetAmount: 1,050 chips (includes fee)
├── Rate: 5%
├── ConversionRate: NULL (not set for rate transactions)
│
├── Fee in chips: 1050 × (5 / 105) = 50 chips
├── AvgRate at transaction date: 5.20 BRL/chip
└── Fee in BRL: 50 × 5.20 = 260 BRL
```

---

## Revenue Source 4: Spread Profit

### Description

For PokerManagers with `ManagerProfitType.Spread`, the company profits from the difference between the rate at which chips are sold versus the average cost of acquiring them.

### Business Context

- PokerCredits are typically **USD-based** (or tied to external markets)
- Company operates in **BRL**
- Conversion rates change dynamically
- Profit = difference between sale rate and cost basis (AvgRate)

### Source

- **Entity:** `DigitalAssetTransaction` where manager has `ManagerProfitType.Spread`
- **Transaction Types:** SALE (manager sends chips)
- **Field:** `ConversionRate` (available on SALE transactions)

### Calculation

```
SpreadProfit = SaleAmount × (SaleConversionRate - AvgRate)
```

**Note:** This is the **only** profit source that outputs directly in BRL.

### Example

```
Sale Transaction:
├── AssetAmount: 800 chips
├── ConversionRate: 5.30 BRL/chip
├── AvgRate at transaction date: 5.067 BRL/chip
│
└── SpreadProfit: 800 × (5.30 - 5.067) = 800 × 0.233 = 186.4 BRL
```

---

## AvgRate System Design

### Overview

AvgRate (weighted average cost) is **essential** for:
1. Converting non-BRL profits to BRL (Direct Income, Rake, Rate Fees)
2. Calculating Spread profit (cost basis comparison)

### Design: Monthly Snapshots with In-Memory Cache

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     AVGRATE SYSTEM ARCHITECTURE                          │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  STORAGE: In-Memory Cache (IMemoryCache)                                 │
│  └── Cache key: $"AvgRate:{ManagerId}:{Year}:{Month}"                    │
│  └── Fallback: Calculate on-demand                                       │
│  └── Future: Migrate to Azure Cache Service if needed                    │
│                                                                          │
│  CALCULATION TRIGGER:                                                    │
│  └── On transaction create/update/delete                                 │
│  └── Invalidate affected month AND all subsequent months                 │
│  └── Recalculate on next access (lazy loading)                           │
│                                                                          │
│  INITIAL STATE:                                                          │
│  └── All PMs have initial balance and AvgRate                            │
│  └── If not set, assume zero for both                                    │
│                                                                          │
│  CURRENT MONTH:                                                          │
│  └── Calculate dynamically (not from cache)                              │
│  └── Include all transactions up to current date                         │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### Cascade Invalidation Rule

```
When a transaction is created/updated/deleted for Month M:
1. Invalidate cache for Month M
2. Invalidate cache for ALL months after M
3. On next access, recalculate from Month M forward
```

**Reason:** Each month's AvgRate depends on the previous month's closing balance and AvgRate. Changing Month M affects all subsequent calculations.

### Data Model

```csharp
public class AvgRateSnapshot
{
    public Guid PokerManagerId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    
    /// <summary>
    /// Weighted average cost per chip (BRL/chip) at end of month
    /// </summary>
    public decimal AvgRate { get; set; }
    
    /// <summary>
    /// Total chips held at end of month
    /// </summary>
    public decimal TotalChips { get; set; }
    
    /// <summary>
    /// Total cost basis in BRL at end of month
    /// </summary>
    public decimal TotalCost { get; set; }
    
    /// <summary>
    /// When this snapshot was calculated
    /// </summary>
    public DateTime CalculatedAt { get; set; }
}
```

### PokerManager Initial State

```csharp
public class PokerManager
{
    // Existing fields...
    
    /// <summary>
    /// Initial chip balance when manager was onboarded
    /// </summary>
    public decimal InitialChipBalance { get; set; }
    
    /// <summary>
    /// Initial AvgRate (cost basis) when manager was onboarded
    /// Default: 0 if not set
    /// </summary>
    public decimal InitialAvgRate { get; set; }
    
    /// <summary>
    /// Date from which to start AvgRate calculations
    /// </summary>
    public DateTime? AvgRateStartDate { get; set; }
}
```

### Calculation Algorithm

```csharp
public async Task<AvgRateSnapshot> GetAvgRateForMonth(Guid managerId, int year, int month)
{
    // 1. Check cache first
    var cacheKey = $"AvgRate:{managerId}:{year}:{month}";
    if (_cache.TryGetValue(cacheKey, out AvgRateSnapshot cached))
        return cached;
    
    // 2. Get starting position
    AvgRateSnapshot previousSnapshot;
    
    if (IsFirstMonth(managerId, year, month))
    {
        // Use PM's initial values
        var manager = await GetPokerManager(managerId);
        previousSnapshot = new AvgRateSnapshot
        {
            TotalChips = manager.InitialChipBalance,
            TotalCost = manager.InitialChipBalance * manager.InitialAvgRate,
            AvgRate = manager.InitialAvgRate
        };
    }
    else
    {
        // Get previous month (recursively builds chain if needed)
        var prevYear = month == 1 ? year - 1 : year;
        var prevMonth = month == 1 ? 12 : month - 1;
        previousSnapshot = await GetAvgRateForMonth(managerId, prevYear, prevMonth);
    }
    
    // 3. Apply this month's transactions
    decimal totalChips = previousSnapshot.TotalChips;
    decimal totalCost = previousSnapshot.TotalCost;
    
    var transactions = await GetTransactionsForMonth(managerId, year, month);
    
    foreach (var tx in transactions.OrderBy(t => t.Date))
    {
        if (IsPurchase(tx, managerId))
        {
            // PURCHASE: Add to inventory
            totalChips += tx.AssetAmount;
            totalCost += tx.AssetAmount * tx.ConversionRate.Value;
        }
        else if (IsSale(tx, managerId))
        {
            // SALE: Remove from inventory proportionally
            if (totalChips > 0)
            {
                var proportion = tx.AssetAmount / totalChips;
                totalCost -= totalCost * proportion;
                totalChips -= tx.AssetAmount;
            }
        }
    }
    
    // 4. Calculate final AvgRate
    var avgRate = totalChips > 0 ? totalCost / totalChips : 0;
    
    // 5. Build and cache snapshot
    var snapshot = new AvgRateSnapshot
    {
        PokerManagerId = managerId,
        Year = year,
        Month = month,
        AvgRate = avgRate,
        TotalChips = totalChips,
        TotalCost = totalCost,
        CalculatedAt = DateTime.UtcNow
    };
    
    // Only cache completed months (not current month)
    if (!IsCurrentMonth(year, month))
    {
        _cache.Set(cacheKey, snapshot, TimeSpan.FromHours(24));
    }
    
    return snapshot;
}
```

### Current Month Handling

```csharp
public async Task<decimal> GetAvgRateAtDate(Guid managerId, DateTime date)
{
    // For dates in current month, calculate dynamically
    if (IsCurrentMonth(date.Year, date.Month))
    {
        return await CalculateAvgRateUpToDate(managerId, date);
    }
    
    // For past months, use cached snapshot
    var snapshot = await GetAvgRateForMonth(managerId, date.Year, date.Month);
    return snapshot.AvgRate;
}

private async Task<decimal> CalculateAvgRateUpToDate(Guid managerId, DateTime upToDate)
{
    // Get previous month's closing
    var prevMonth = upToDate.AddMonths(-1);
    var previousSnapshot = await GetAvgRateForMonth(
        managerId, prevMonth.Year, prevMonth.Month);
    
    // Apply transactions up to specified date
    decimal totalChips = previousSnapshot.TotalChips;
    decimal totalCost = previousSnapshot.TotalCost;
    
    var transactions = await GetTransactionsFromDateRange(
        managerId, 
        new DateTime(upToDate.Year, upToDate.Month, 1),
        upToDate);
    
    foreach (var tx in transactions.OrderBy(t => t.Date))
    {
        // Apply purchase/sale logic...
    }
    
    return totalChips > 0 ? totalCost / totalChips : 0;
}
```

### Cache Invalidation

```csharp
public async Task OnTransactionChanged(DigitalAssetTransaction tx, Guid managerId)
{
    // Invalidate affected month and ALL subsequent months
    var txMonth = new DateTime(tx.Date.Year, tx.Date.Month, 1);
    var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
    
    var monthToInvalidate = txMonth;
    while (monthToInvalidate <= currentMonth)
    {
        var cacheKey = $"AvgRate:{managerId}:{monthToInvalidate.Year}:{monthToInvalidate.Month}";
        _cache.Remove(cacheKey);
        monthToInvalidate = monthToInvalidate.AddMonths(1);
    }
}
```

---

## Financial Statement (Planilha)

### Component Architecture

```
/financeiro/planilha/page.tsx
│
├── DateSelector                    # Month/Year selection
│
├── ProfitSummaryCard (NEW)         # Company profit by source
│   ├── Direct Income (with BRL conversion)
│   ├── Rake Commission (with BRL conversion)
│   ├── Rate Fees (with BRL conversion)
│   └── Spread Profit
│
├── ConsolidatedView               # Assets vs Liabilities summary
│
├── ManagersTableView               # PokerManager positions
│   ├── Saldo em Fichas (chips)
│   ├── Cotação (current rate)
│   ├── Saldo em Dinheiro (BRL)
│   ├── Tipo de Lucro (ManagerProfitType)
│   └── Resultado Financeiro (profit by type)
│
├── BanksTableView                  # Bank balances
│
├── ClientsBalanceView              # Client balances
│   ├── Devedores (owe us money)
│   └── Credores (we owe them)
│
└── DirectIncomeTableView           # System operations by category
```

---

## Implementation Phases

### Phase 0: Dead Code Cleanup (Frontend Only)

- Remove `resetAvgRate` dead code
- Remove console.log statements
- Clean `AvgRateDependentComponents`

### Phase 1: Date-Filtered Balances

- **Backend:** Add `asOfDate` parameter to balance endpoints
- **Frontend:** Pass date as query parameter

### Phase 2: AvgRate System

#### 2.1 Add Initial Fields to PokerManager

```csharp
public decimal InitialChipBalance { get; set; }
public decimal InitialAvgRate { get; set; }
public DateTime? AvgRateStartDate { get; set; }
```

#### 2.2 Implement AvgRateService

- In-memory cache using `IMemoryCache`
- `GetAvgRateAtDate(managerId, date)` method
- `GetAvgRateForMonth(managerId, year, month)` method
- Cache invalidation on transaction changes
- Current month dynamic calculation

#### 2.3 Integrate with Transaction Hooks

- Invalidate affected + subsequent months on create/update/delete

### Phase 3: Profit Calculation Service

- Calculate Direct Income (with BRL conversion)
- Calculate Rake Commission (with BRL conversion)
- Calculate Rate Fees (with BRL conversion)
- Calculate Spread Profit

### Phase 4: Profit Display (Frontend)

- Create `ProfitService` and queries
- Create `ProfitSummaryCard` component
- Integrate into planilha page

---

## Design Decisions

### Decision 1: Initial AvgRate Seeding

**Decision:** All PMs come with an initial balance and its AvgRate. If not set, assume zero for both.

**Rationale:** This avoids complex historical recalculation and provides a clean starting point.

### Decision 2: Cache Storage

**Decision:** Use in-memory cache (`IMemoryCache`) initially. Migrate to Azure Cache Service if performance degrades.

**Rationale:** Simple to implement, no infrastructure changes needed. Can upgrade later if needed.

### Decision 3: AvgRate Must Always Exist

**Decision:** AvgRate must always exist for profit calculations. Use zero if no data available.

**Rationale:** All non-BRL profit sources require AvgRate for BRL conversion.

### Decision 4: Cascade Recalculation

**Decision:** When a transaction changes, invalidate the affected month AND all subsequent months.

**Rationale:** Each month's AvgRate depends on the previous month's closing position. Changes cascade forward.

---

## Related Documentation

| Document | Location | Purpose |
|----------|----------|---------|
| Backend Implementation Plan | `10_REFACTORING/FINANCE_MODULE_IMPLEMENTATION_PLAN_BACKEND.md` | Backend tasks |
| Frontend Implementation Plan | `SF_management-front/documentation/06_DEVELOPMENT/FINANCE_MODULE_IMPLEMENTATION_PLAN_FRONTEND.md` | Frontend tasks |
| Deferred Features | `07_REFERENCE/FINANCE_DEFERRED_FEATURES.md` | Future features |

---

*Document Version: 5.0 (Revised with BRL conversion and cache design)*  
*Last Updated: January 27, 2026*
