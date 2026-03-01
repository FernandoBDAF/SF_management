# Financial Report Data Model

## Overview

The financial report is the most complex data aggregation in the system. It pulls data from multiple services and endpoints to present a unified financial overview including:

- **Profit Summary** — Company revenue from four distinct sources
- **Profit By Manager** — Per-manager breakdown (excluding Direct Income)
- **Entity Balances** — Banks, Clients, Members, and Managers
- **Devedores / Credores** — Receivables and payables derived from client/member balances
- **Ativos / Passivos** — Asset and liability totals with manager-type-specific rules

This document describes the complete data model, formulas, and endpoints that the financial report consumes.

---

## Table of Contents

1. [Profit Summary](#profit-summary)
2. [Profit By Manager](#profit-by-manager)
3. [Cotação (AvgRate) Rules](#cotação-avgrate-rules)
4. [Balance Components](#balance-components)
5. [RakeOverrideCommission Manager Balance Special Rule](#rakeoverridecommission-manager-balance-special-rule)
6. [Devedores and Credores](#devedores-and-credores)
7. [Ativos (Assets) Formula](#ativos-assets-formula)
8. [Passivos (Liabilities) Formula](#passivos-liabilities-formula)
9. [Profit Detail Modals](#profit-detail-modals)
10. [AvgRates Endpoint](#avgrates-endpoint)
11. [SystemImplementation Constant](#systemimplementation-constant)
12. [Data Flow Diagram](#data-flow-diagram)
13. [Related Documentation](#related-documentation)

---

## Profit Summary

**Source:** `ProfitController` → `IProfitCalculationService.GetProfitSummary()`

**Endpoint:** `GET /api/v1/finance/profit/summary`

The profit summary aggregates revenue from four sources. All values are in BRL.

### 1. Direct Income

Revenue (or expense) from categorized transactions involving system wallets.

```
DirectIncome = Σ (signed BRL amount of each categorized system wallet transaction)
```

- System wallet is **receiver** → positive (revenue)
- System wallet is **sender** → negative (expense)
- Digital asset transactions are converted to BRL using the counterparty manager's AvgRate

### 2. Rake Commission

Applies only to managers with `ManagerProfitType = RakeOverrideCommission`.

```
RakeCommission = Σ (RakeAmount × ((RakeCommission% − RakeBack%) / 100) × AvgRate)
```

**Example:**

| Field | Value |
|-------|-------|
| `RakeAmount` | 1,000 chips |
| `RakeCommission` | 50% |
| `RakeBack` | 10% |
| `AvgRate` | 1.00 (RakeOverride manager) |

```
Chips earned = 1,000 × ((50 − 10) / 100) = 400 chips
BRL earned   = 400 × 1.00 = 400 BRL
```

> **Note:** `RakeBack` is nullable. When null, it defaults to 0 in the formula.

### 3. Rate Fees

Revenue extracted from transactions that have an embedded fee rate (`Rate` field).

```
RateFees = Σ (AssetAmount × (Rate / (100 + Rate)) × AvgRate)
```

**Example:**

| Field | Value |
|-------|-------|
| `AssetAmount` | 1,050 chips (includes fee) |
| `Rate` | 5% |
| `AvgRate` | 5.25 BRL/chip |

```
FeeInChips = 1,050 × (5 / 105) = 50 chips
FeeInBRL   = 50 × 5.25 = 262.50 BRL
```

The `Rate / (100 + Rate)` formula reverses the embedded fee — the total amount already includes the fee, so extraction requires the inverse formula.

### 4. Spread Profit

Applies only to managers with `ManagerProfitType = Spread`.

```
SpreadProfit = Σ (SaleAmount × (SaleConversionRate − AvgRate))
```

The result is already in BRL because both `ConversionRate` and `AvgRate` are BRL/chip prices.

**Example:**

| Field | Value |
|-------|-------|
| `AssetAmount` | 1,000 chips |
| `ConversionRate` (sale price) | 5.50 BRL/chip |
| `AvgRate` (cost basis) | 5.25 BRL/chip |

```
SpreadProfit = 1,000 × (5.50 − 5.25) = 250 BRL
```

Negative spread is possible when `ConversionRate < AvgRate` (sold at a loss).

### Total Profit

```
TotalProfit = DirectIncome + RakeCommission + RateFees + SpreadProfit
```

### Response DTO

```csharp
public class ProfitSummary
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid? ManagerId { get; set; }

    public decimal DirectIncome { get; set; }
    public decimal RakeCommission { get; set; }
    public decimal RateFees { get; set; }
    public decimal SpreadProfit { get; set; }

    public decimal TotalProfit => DirectIncome + RakeCommission + RateFees + SpreadProfit;
}
```

---

## Profit By Manager

**Source:** `IProfitCalculationService.GetProfitByManager()`

**Endpoint:** `GET /api/v1/finance/profit/by-manager`

Per-manager profit breakdown **excluding Direct Income**. Direct Income is a system-level metric not attributable to individual managers — it derives from system wallet transactions that span the entire company.

### Response DTO

```csharp
public class ProfitByManager
{
    public Guid ManagerId { get; set; }
    public string ManagerName { get; set; }
    public int? ManagerProfitType { get; set; }  // 0=Spread, 1=RakeOverrideCommission, null=not configured

    public decimal RakeCommission { get; set; }  // RakeOverride managers only
    public decimal RateFees { get; set; }         // All managers
    public decimal SpreadProfit { get; set; }     // Spread managers only

    public decimal TotalProfit => RakeCommission + RateFees + SpreadProfit;
}
```

### Revenue Source Applicability by Manager Type

| Revenue Source | Spread Manager | RakeOverrideCommission Manager |
|----------------|:-:|:-:|
| RakeCommission | 0.00 | Calculated |
| RateFees | Calculated | Calculated |
| SpreadProfit | Calculated | 0.00 |

Results are sorted by `TotalProfit` descending.

---

## Cotação (AvgRate) Rules

The AvgRate used in profit calculations depends on the manager type:

| Manager Type | AvgRate Value | Rationale |
|-------------|--------------|-----------|
| `Spread` | Weighted average from `AvgRateService` | Tracks actual cost basis of chip inventory |
| `RakeOverrideCommission` | `1` (fixed) | Chips are BRL-quoted; 1 chip = 1 BRL |
| Non-manager entities | `0` | Not applicable to profit calculations |

### AvgRate Retrieval

```
GetAvgRateAtDate(managerId, date)
    → Spread manager:                calculated weighted average
    → RakeOverrideCommission manager: 1
    → Non-manager:                    0
```

The AvgRate calculation is iterative, month-by-month, using cached monthly snapshots. The current month is always calculated dynamically. See [PROFIT_CALCULATION_SYSTEM.md](PROFIT_CALCULATION_SYSTEM.md) for the full algorithm.

---

## Balance Components

The financial report fetches balances for all four entity types via their respective endpoints.

### Banks

**Endpoint:** `GET /api/v1/bank/{id}/balance`

Returns `Dictionary<string, decimal>` keyed by `AssetType` name.

```json
{
  "BrazilianReal": 150000.00,
  "USDollar": 25000.00
}
```

### Clients

**Endpoint:** `GET /api/v1/client/{id}/balance`

Returns `Dictionary<string, decimal>` keyed by `AssetType` name.

```json
{
  "BrazilianReal": -5000.00,
  "PokerStars": 1200.00
}
```

### Members

**Endpoint:** `GET /api/v1/member/{id}/balance`

Returns `Dictionary<string, decimal>` keyed by `AssetType` name.

```json
{
  "BrazilianReal": 8000.00
}
```

### Managers

**Endpoint:** `GET /api/v1/pokermanager/{id}/balance`

Returns `Dictionary<string, decimal>` keyed by `AssetGroup` name.

```json
{
  "FiatAssets": 25000.00,
  "PokerAssets": 8500.00,
  "Settlements": -3500.00
}
```

For the financial report, the relevant manager balance components are:

| Report Field | Source | Description |
|-------------|--------|-------------|
| Saldo em Dinheiro (Cash Balance) | `FiatAssets` value | Manager's fiat currency balance |
| Saldo em Fichas (Chip Balance) | `PokerAssets` value | Manager's chip inventory quantity |
| Cotação (Rate) | `AvgRate` from `/avg-rates` endpoint | Average rate for chip-to-BRL conversion |

---

## RakeOverrideCommission Manager Balance Special Rule

For managers with `ManagerProfitType = RakeOverrideCommission`, settlement transactions have a special balance impact. The `SettlementTransaction.AssetAmount` is added to **both** `PokerAssets` and `FiatAssets` balances.

### Signal Logic

| Role in Transaction | PokerAssets Impact | FiatAssets Impact |
|--------------------|--------------------|-------------------|
| **Receiver** | +AssetAmount | +AssetAmount |
| **Sender** | −AssetAmount | −AssetAmount |

This dual impact reflects that for RakeOverrideCommission managers, chip movements through settlements represent both a chip position change and a corresponding BRL-equivalent change (since their chips are 1:1 BRL-quoted).

---

## Devedores and Credores

Derived from **Client** and **Member** balances combined.

### Devedores (Receivables)

Clients and Members whose total balance is **negative** (they owe money to the company).

```
Devedores = All (Clients ∪ Members) WHERE balance < 0
```

Displayed as **absolute value** in the report.

### Credores (Payables)

Clients and Members whose total balance is **positive** (the company owes money to them).

```
Credores = All (Clients ∪ Members) WHERE balance > 0
```

---

## Ativos (Assets) Formula

Assets represent what the company owns or is owed.

```
Ativos = PositiveBankBalances
       + Devedores (absolute value)
       + PositiveManagerCashBalances
       + SpreadManagerChips × Cotação
```

### Component Breakdown

| Component | Source | Condition |
|-----------|--------|-----------|
| Positive bank balances | Bank balance endpoints | `balance > 0` |
| Devedores | Client/Member balances | `balance < 0` (displayed as absolute value) |
| Positive manager cash balance | Manager `FiatAssets` balance | `Saldo em Dinheiro > 0` |
| Chip value (Spread only) | Manager `PokerAssets` × AvgRate | **Spread managers only** — NOT RakeOverrideCommission |

### Why Spread Chips Are Assets

Spread managers hold chips as inventory purchased at a cost (`AvgRate`). Their chip balance × AvgRate represents the BRL value of inventory on hand — a company asset.

### Why RakeOverrideCommission Chips Are NOT Assets

RakeOverrideCommission managers' chips represent chips on poker platforms that are owed by or to the platform. They are not company-owned inventory. Their chip value appears under Passivos (Liabilities) instead.

---

## Passivos (Liabilities) Formula

Liabilities represent what the company owes.

```
Passivos = |NegativeBankBalances|
         + Credores
         + |NegativeManagerCashBalances|
         + RakeOverrideManagerChips × Cotação
```

### Component Breakdown

| Component | Source | Condition |
|-----------|--------|-----------|
| Negative bank balances | Bank balance endpoints | `balance < 0` (displayed as absolute value) |
| Credores | Client/Member balances | `balance > 0` |
| Negative manager cash balance | Manager `FiatAssets` balance | `Saldo em Dinheiro < 0` (displayed as absolute value) |
| Chip value (RakeOverride only) | Manager `PokerAssets` × Cotação | **RakeOverrideCommission managers only** (Cotação = 1) |

### Manager Type → Asset/Liability Classification Summary

| Balance Component | Spread Manager | RakeOverrideCommission Manager |
|------------------|:-:|:-:|
| Positive Cash Balance | → **Ativos** | → **Ativos** |
| Negative Cash Balance | → **Passivos** | → **Passivos** |
| Saldo em Fichas × Cotação | → **Ativos** | → **Passivos** |

---

## Profit Detail Modals

Each line item in the profit summary is clickable and opens a detail modal. The modals fetch itemized transaction data from dedicated endpoints.

| Report Line | Detail Endpoint | Response Type |
|------------|----------------|---------------|
| Direct Income | `GET /api/v1/finance/profit/direct-income-details` | `DirectIncomeDetailsResponse` |
| Rate Fees | `GET /api/v1/finance/profit/rate-fee-details` | `RateFeeDetailsResponse` |
| Rake Commission | `GET /api/v1/finance/profit/rake-commission-details` | `RakeCommissionDetailsResponse` |
| Spread Profit | `GET /api/v1/finance/profit/spread-details` | `SpreadProfitDetailsResponse` |

### Query Parameters (All Endpoints)

| Parameter | Type | Required | Default |
|-----------|------|:--------:|---------|
| `startDate` | DateTime | No | `SystemImplementation.FinanceDataStartDateUtc` |
| `endDate` | DateTime | No | Today (UTC) |

All detail endpoints validate that `startDate ≤ endDate` and return `400 Bad Request` if violated.

---

## AvgRates Endpoint

**Endpoint:** `GET /api/v1/finance/profit/avg-rates`

Returns the AvgRate for each manager as of a specific date.

**Query Parameter:** `asOfDate` (DateTime, optional — defaults to today UTC)

**Response:** `Dictionary<Guid, decimal>` mapping `BaseAssetHolderId` to AvgRate.

```json
{
  "a1b2c3d4-...": 5.25,
  "e5f6g7h8-...": 1.00
}
```

| Manager Type | Returned AvgRate |
|-------------|-----------------|
| Spread | Actual calculated weighted average cost basis |
| RakeOverrideCommission | Always `1` |

This endpoint is consumed by the financial report to compute the BRL value of each manager's chip holdings.

---

## SystemImplementation Constant

**File:** `Domain/Common/SystemImplementation.cs`

```csharp
public static class SystemImplementation
{
    public static readonly DateTime FinanceDataStartDateUtc =
        new(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);
}
```

**Value:** `2026-02-01 UTC`

This is the earliest date considered by all finance queries. Used as the default `startDate` when no date parameter is provided to profit endpoints, and as the backward lookback floor for AvgRate calculations.

**Design decision:** The system started tracking financial data from this date. Queries before this date would return inaccurate or incomplete results.

---

## Data Flow Diagram

```
┌──────────────────────────────────────────────────────────────────────┐
│                    FINANCIAL REPORT ASSEMBLY                         │
├──────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  1. PROFIT SECTION                                                   │
│     ├── GET /finance/profit/summary                                  │
│     │       → DirectIncome, RakeCommission, RateFees, SpreadProfit   │
│     ├── GET /finance/profit/by-manager                               │
│     │       → Per-manager breakdown (no DirectIncome)                │
│     └── GET /finance/profit/avg-rates                                │
│             → Manager AvgRates (Cotação)                             │
│                                                                      │
│  2. BALANCE SECTION                                                  │
│     ├── For each Bank:    GET /bank/{id}/balance                     │
│     ├── For each Client:  GET /client/{id}/balance                   │
│     ├── For each Member:  GET /member/{id}/balance                   │
│     └── For each Manager: GET /pokermanager/{id}/balance             │
│                                                                      │
│  3. DERIVED CALCULATIONS                                             │
│     ├── Devedores = Clients ∪ Members WHERE balance < 0              │
│     ├── Credores  = Clients ∪ Members WHERE balance > 0              │
│     ├── Ativos    = +Banks + Devedores + +Mgr Cash + Spread Chips    │
│     └── Passivos  = |−Banks| + Credores + |−Mgr Cash| + Rake Chips  │
│                                                                      │
│  4. DETAIL DRILLDOWNS (on click)                                     │
│     ├── GET /finance/profit/direct-income-details                    │
│     ├── GET /finance/profit/rate-fee-details                         │
│     ├── GET /finance/profit/rake-commission-details                  │
│     └── GET /finance/profit/spread-details                           │
│                                                                      │
└──────────────────────────────────────────────────────────────────────┘
```

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [PROFIT_CALCULATION_SYSTEM.md](PROFIT_CALCULATION_SYSTEM.md) | Full calculation pipeline, AvgRate algorithm, caching, edge cases |
| [FINANCE_SYSTEM.md](FINANCE_SYSTEM.md) | High-level revenue model overview and API endpoints |
| [TRANSACTION_BALANCE_IMPACT.md](TRANSACTION_BALANCE_IMPACT.md) | How transactions impact balances (sign conventions) |
| [ASSET_VALUATION_RULES.md](../08_BUSINESS_RULES/ASSET_VALUATION_RULES.md) | InitialBalance configuration, AvgRate business rules |
| [BALANCE_ENDPOINTS.md](../06_API/BALANCE_ENDPOINTS.md) | Balance endpoint patterns and calculation logic |
| [ENTITY_BUSINESS_BEHAVIOR.md](ENTITY_BUSINESS_BEHAVIOR.md) | Entity types and their business roles |
| [SETTLEMENT_WORKFLOW.md](SETTLEMENT_WORKFLOW.md) | How settlements and rake are recorded |

---

*Created: February 27, 2026*
