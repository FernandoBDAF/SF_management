# Finance System

## Overview

The Finance System provides profit calculation and revenue tracking for the poker management business. It calculates company earnings from multiple sources and provides APIs for financial reporting.

---

## Table of Contents

1. [Company Revenue Model](#company-revenue-model)
2. [Revenue Source Details](#revenue-source-details)
3. [Profit Calculation Service](#profit-calculation-service)
4. [Finance API Endpoints](#finance-api-endpoints)
5. [Related Documentation](#related-documentation)

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
- **System Wallets**: `AssetGroup.Internal` with `BaseAssetHolderId == null`
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
    Task<ProfitSummary> GetProfitSummary(DateTime startDate, DateTime endDate, Guid? managerId = null);
    Task<List<ProfitByManager>> GetProfitByManager(DateTime startDate, DateTime endDate);
    Task<List<ProfitBySource>> GetProfitBySource(DateTime startDate, DateTime endDate);
    Task<DirectIncomeDetailsResponse> GetDirectIncomeDetails(DateTime startDate, DateTime endDate);
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
| `/api/v1/finance/profit/by-manager` | GET | Profit breakdown by manager |
| `/api/v1/finance/profit/by-source` | GET | Profit breakdown by source |
| `/api/v1/finance/profit/direct-income-details` | GET | Itemized direct income transactions |

### Query Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `startDate` | DateTime | Yes | Start of date range (YYYY-MM-DD) |
| `endDate` | DateTime | Yes | End of date range (YYYY-MM-DD) |
| `managerId` | Guid | No | Filter by specific manager |

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

See [RATE_LIMITING_AND_PERFORMANCE.md](../05_INFRASTRUCTURE/RATE_LIMITING_AND_PERFORMANCE.md) for caching implementation details.

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [ASSET_VALUATION_RULES.md](../08_BUSINESS_RULES/ASSET_VALUATION_RULES.md) | AvgRate calculation rules and balance modes |
| [TRANSACTION_INFRASTRUCTURE.md](TRANSACTION_INFRASTRUCTURE.md) | Transaction system details |
| [SETTLEMENT_WORKFLOW.md](SETTLEMENT_WORKFLOW.md) | Settlement and rake calculation |
| [RATE_LIMITING_AND_PERFORMANCE.md](../05_INFRASTRUCTURE/RATE_LIMITING_AND_PERFORMANCE.md) | Caching patterns |
| [API_REFERENCE.md](../06_API/API_REFERENCE.md) | Complete API documentation |

---

*Created: January 23, 2026*
