# Finance System

## Overview

The Finance System provides profit calculation and revenue tracking for the poker management business. It calculates company earnings from multiple sources and provides APIs for financial reporting.

---

## Table of Contents

1. [Company Revenue Model](#company-revenue-model)
2. [Revenue Source Details](#revenue-source-details)
3. [Profit Calculation Service](#profit-calculation-service)
4. [Finance API Endpoints](#finance-api-endpoints)
5. [Caching](#caching)
6. [Planned Finance Modules](#planned-finance-modules)
7. [Design Decisions](#design-decisions)
8. [Related Documentation](#related-documentation)

---

## Company Revenue Model

The company earns revenue from **four primary sources**:

```
┌─────────────────────────────────────────────────────────────────────────┐
│                       COMPANY REVENUE SOURCES                            │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  1. DIRECT INCOME                                                        │
│     └── Categorized transactions with System Wallets                     │
│     └── System is RECEIVER = Revenue (+)                                 │
│     └── System is SENDER = Expense (-)                                   │
│                                                                          │
│  2. RAKE COMMISSION (RakeOverrideCommission managers only)               │
│     └── Formula: RakeAmount × ((RakeCommission - RakeBack) / 100)        │
│     └── Converted to BRL via AvgRate                                     │
│                                                                          │
│  3. RATE FEES                                                            │
│     └── Transactions with Rate field                                     │
│     └── Formula: AssetAmount × (Rate / (100 + Rate)) × AvgRate           │
│                                                                          │
│  4. SPREAD PROFIT (Spread managers only)                                 │
│     └── Formula: SaleAmount × (SaleConversionRate - AvgRate)             │
│     └── Already in BRL, no conversion needed                             │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## Revenue Source Details

### 1. Direct Income

- **Source**: Transactions with `CategoryId` AND involving a System Wallet
- **System Wallets**: `AssetGroup.Flexible` with `BaseAssetHolderId == null`
- **Direction Logic**:
  - System wallet is RECEIVER = Revenue (positive)
  - System wallet is SENDER = Expense (negative)
- **BRL Conversion**: Digital asset transactions converted using AvgRate

### 2. Rake Commission

- **Applies To**: Managers with `ManagerProfitType = RakeOverrideCommission`
- **Source**: `SettlementTransaction` records
- **Formula**: `RakeAmount × ((RakeCommission - RakeBack) / 100) × AvgRate`
- **Example**:
  - RakeAmount: 1000 chips
  - RakeCommission: 50%
  - RakeBack: 10%
  - AvgRate: 5.25 BRL/chip
  - **Profit**: 1000 × ((50 - 10) / 100) × 5.25 = **2,100 BRL**

### 3. Rate Fees

- **Source**: `DigitalAssetTransaction` records with `Rate` field
- **Formula**: `AssetAmount × (Rate / (100 + Rate)) × AvgRate`
- **Example**:
  - 1050 chips with 5% rate embedded
  - Fee = 1050 × (5 / 105) = **50 chips**
  - In BRL: 50 × 5.25 = **262.50 BRL**

### 4. Spread Profit

- **Applies To**: Managers with `ManagerProfitType = Spread`
- **Source**: SALE transactions (manager sends chips)
- **Formula**: `SaleAmount × (SaleConversionRate - AvgRate)`
- **Example**:
  - Sell 1000 chips at 5.50 BRL/chip
  - AvgRate (cost basis): 5.25 BRL/chip
  - **Profit**: 1000 × (5.50 - 5.25) = **250 BRL**

---

## Profit Calculation Service

### Service Interface

**File:** `Application/Services/Finance/ProfitCalculationService.cs`

```csharp
public interface IProfitCalculationService
{
    // Summary endpoints
    Task<ProfitSummary> GetProfitSummary(DateTime startDate, DateTime endDate, Guid? managerId = null);
    Task<List<ProfitByManager>> GetProfitByManager(DateTime startDate, DateTime endDate);
    Task<List<ProfitBySource>> GetProfitBySource(DateTime startDate, DateTime endDate);
    
    // Detail endpoints (for modal views)
    Task<DirectIncomeDetailsResponse> GetDirectIncomeDetails(DateTime startDate, DateTime endDate);
    Task<RateFeeDetailsResponse> GetRateFeeDetails(DateTime startDate, DateTime endDate);
    Task<RakeCommissionDetailsResponse> GetRakeCommissionDetails(DateTime startDate, DateTime endDate);
    Task<SpreadProfitDetailsResponse> GetSpreadProfitDetails(DateTime startDate, DateTime endDate);
    
    // AvgRate endpoint
    Task<Dictionary<Guid, decimal>> GetManagerAvgRates(DateTime asOfDate);
}
```

### ProfitSummary DTO

```csharp
public class ProfitSummary
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid? ManagerId { get; set; }
    
    public decimal DirectIncome { get; set; }     // Categorized system operations
    public decimal RakeCommission { get; set; }   // RakeOverrideCommission managers
    public decimal RateFees { get; set; }         // Rate field transactions
    public decimal SpreadProfit { get; set; }     // Spread managers
    
    public decimal TotalProfit => DirectIncome + RakeCommission + RateFees + SpreadProfit;
}
```

### AvgRate Dependency

The profit calculation service depends on `IAvgRateService` for:
- Converting chip amounts to BRL (Rake Commission, Rate Fees)
- Calculating cost basis for Spread Profit

See [ASSET_VALUATION_RULES.md](../08_BUSINESS_RULES/ASSET_VALUATION_RULES.md) for AvgRate calculation rules.

---

## Finance API Endpoints

### Controller

**File:** `Api/Controllers/v1/Finance/FinanceController.cs`

### Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/v1/finance/profit/summary` | GET | Profit summary for date range |
| `/api/v1/finance/profit/by-manager` | GET | Profit breakdown by manager (excludes DirectIncome) |
| `/api/v1/finance/profit/by-source` | GET | Profit breakdown by source |
| `/api/v1/finance/profit/direct-income-details` | GET | Itemized direct income transactions |
| `/api/v1/finance/profit/rate-fee-details` | GET | Itemized rate fee transactions |
| `/api/v1/finance/profit/rake-commission-details` | GET | Itemized rake commission settlements |
| `/api/v1/finance/profit/spread-details` | GET | Itemized spread profit sales |
| `/api/v1/finance/profit/avg-rates` | GET | Manager AvgRates for a specific date |

> **Note:** The `/by-manager` endpoint returns only RakeCommission, RateFees, and SpreadProfit per manager. DirectIncome is a system-level metric not attributable to individual managers and should be fetched via `/direct-income-details`.

### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `startDate` | DateTime | No | `SystemImplementation.FinanceDataStartDateUtc` (2025-07-17) | Start of date range (YYYY-MM-DD) |
| `endDate` | DateTime | No | Today (UTC) | End of date range (YYYY-MM-DD) |
| `managerId` | Guid | No | null | Filter by manager (`BaseAssetHolderId` or `PokerManager.Id` accepted) |

See [PROFIT_CALCULATION_SYSTEM.md](PROFIT_CALCULATION_SYSTEM.md) for the complete calculation pipeline.

### Example Request

```
GET /api/v1/finance/profit/summary?startDate=2026-01-01&endDate=2026-01-31
```

### Example Response

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

---

## Caching

The finance system uses caching to improve performance:

| Data | TTL | Invalidation |
|------|-----|--------------|
| Monthly AvgRate Snapshots | 24 hours | On transaction create/update/delete |
| System Wallet IDs | 10 minutes | On wallet create |

### AvgRate Cascade Invalidation

When a transaction is modified or deleted, all cached monthly AvgRate snapshots from that transaction's date forward are invalidated. Because each month's AvgRate depends on the previous month's closing position, a change in any month cascades through all subsequent months. Lazy recalculation on next access ensures the updated values propagate correctly.

- **Cache key pattern**: `avgrate.{managerId}.{year}.{month}`
- **Invalidation scope**: The affected month **and** every subsequent month up to the current month
- **Recalculation**: Happens on next access (lazy loading), not eagerly

See [RATE_LIMITING_AND_PERFORMANCE.md](../05_INFRASTRUCTURE/RATE_LIMITING_AND_PERFORMANCE.md) for caching implementation details.

---

## Planned Finance Modules

| Module | Description | Status |
|--------|-------------|--------|
| **Notas Fiscais** (Invoices) | Invoice generation based on profits and tax obligations | Planned |
| **Despesas** (Expenses) | Recurring expense registration and management | Planned |
| **Consolidado** (Ledger) | Ledger-based document compliant with Brazilian law | Planned |

---

## Design Decisions

### Initial AvgRate Seeding

All PokerManagers are onboarded with an initial chip balance and corresponding AvgRate. These values bootstrap the calculation chain so that the first month's AvgRate can be computed without requiring historical transaction replay.

### Cache Storage Choice

`IMemoryCache` was chosen over a distributed cache because the backend runs as a single instance. This avoids infrastructure complexity while still providing fast lookups. If the system scales to multiple instances, migrating to a distributed cache (e.g., Azure Cache Service) is straightforward.

### AvgRate Must Always Exist

AvgRate never returns null or zero. For managers with no transaction history, it defaults to **1**. This guarantees that all profit calculations that depend on BRL conversion always have a valid multiplier.

### Cascade Recalculation

When a transaction is modified or deleted, the AvgRate is recalculated forward from that transaction's month through all subsequent months. Each month's AvgRate depends on the previous month's closing position, so any change must propagate through the entire chain to maintain accuracy.

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [PROFIT_CALCULATION_SYSTEM.md](PROFIT_CALCULATION_SYSTEM.md) | Deep-dive into profit calculation pipeline |
| [ASSET_VALUATION_RULES.md](../08_BUSINESS_RULES/ASSET_VALUATION_RULES.md) | AvgRate calculation rules and balance modes |
| [TRANSACTION_INFRASTRUCTURE.md](TRANSACTION_INFRASTRUCTURE.md) | Transaction system details |
| [SETTLEMENT_WORKFLOW.md](SETTLEMENT_WORKFLOW.md) | Settlement and rake calculation |
| [RATE_LIMITING_AND_PERFORMANCE.md](../05_INFRASTRUCTURE/RATE_LIMITING_AND_PERFORMANCE.md) | Caching patterns |
| [API_REFERENCE.md](../06_API/API_REFERENCE.md) | Complete API documentation |
| [FINANCE_MODULE_UPGRADE_PLAN.md](../../development/FINANCE_MODULE_UPGRADE_PLAN.md) | Finance module upgrade implementation details |

---

*Created: January 23, 2026*
*Last updated: February 27, 2026*
