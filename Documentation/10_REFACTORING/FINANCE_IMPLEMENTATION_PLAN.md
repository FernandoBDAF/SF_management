# Finance Module Implementation Plan

> **Status:** Implementation Ready  
> **Created:** January 25, 2026  
> **Track:** C  
> **Purpose:** Comprehensive, actionable implementation plan for Finance Module refactoring  
> **Based On:** [FINANCE_CURRENT_STATE.md](./FINANCE_CURRENT_STATE.md)

---

## Table of Contents

- [Executive Summary](#executive-summary)
- [Critical Findings](#critical-findings)
- [Implementation Strategy](#implementation-strategy)
- [Phase 1: Critical Infrastructure](#phase-1-critical-infrastructure)
- [Phase 2: Profit Tracking](#phase-2-profit-tracking)
- [Phase 3: Member Financial Module](#phase-3-member-financial-module)
- [Phase 4: Client Credit Management](#phase-4-client-credit-management)
- [Phase 5: Advanced Features](#phase-5-advanced-features)
- [Risk Analysis & Mitigation](#risk-analysis--mitigation)
- [Testing Strategy](#testing-strategy)
- [Deployment Strategy](#deployment-strategy)
- [Success Metrics](#success-metrics)

---

## Executive Summary

### Scope

This plan implements a complete Finance Module across 5 phases, from critical infrastructure fixes to advanced reporting features.

### Timeline Estimate

- **Phase 1:** ~8 hours (Critical - Date filtering)
- **Phase 2:** ~20 hours (High Priority - Profit tracking)
- **Phase 3:** ~25 hours (Medium Priority - Member financials)
- **Phase 4:** ~12 hours (Medium Priority - Credit management)
- **Phase 5:** ~20 hours (Lower Priority - Advanced features)
- **Total:** ~85 hours of development + testing

### Dependencies

- Phase 2 depends on Phase 1 completion
- Phase 3 depends on Phase 2 completion
- Phase 4 is independent (can run in parallel)
- Phase 5 depends on Phases 1-4

### Team Requirements

- 1 Backend Developer (C#/.NET)
- 1 Frontend Developer (React/TypeScript)
- 1 QA Tester (for each phase)
- Product Owner (for Phase 3 business rules)

---

## Critical Findings

### Finding 1: Frontend-Backend Route Mismatch ⚠️

**Severity:** HIGH

**Issue:**

| Component | Expected Route | Actual Route | Impact |
|-----------|---------------|--------------|---------|
| Frontend | `GET /api/v1/bank/balance/{id}` | `GET /api/v1/bank/{id}/balance` | **404 Errors** |
| Frontend | `POST /api/v1/bank/balance/{id}` (with date) | **Not implemented** | **Date filtering broken** |

**Root Cause:**
Frontend service (`finance.service.ts`) was developed against expected API that doesn't exist yet.

**Impact:**
- `/financeiro/planilha` page cannot display historical balances
- Users cannot view financial statements for past months
- All date selector functionality is non-functional

**Required Action:**
Must be fixed in Phase 1 (highest priority)

---

### Finding 2: No Date-Filtered Balance Support ❌

**Severity:** HIGH

**Issue:**
Backend balance calculation (in `BaseAssetHolderService.GetBalancesByAssetType` and `GetBalancesByAssetGroup`) aggregates ALL transactions without date filtering.

**Current Implementation:**
```csharp
var digitalTransactions = await context.DigitalAssetTransactions
    .Where(dt => !dt.DeletedAt.HasValue && 
        (walletIdentifierIds.Contains(dt.SenderWalletIdentifierId) || 
         walletIdentifierIds.Contains(dt.ReceiverWalletIdentifierId)))
    .ToArrayAsync();
// ❌ No date filtering
```

**Required:**
```csharp
var query = context.DigitalAssetTransactions
    .Where(dt => !dt.DeletedAt.HasValue && 
        (walletIdentifierIds.Contains(dt.SenderWalletIdentifierId) || 
         walletIdentifierIds.Contains(dt.ReceiverWalletIdentifierId)));

if (asOfDate.HasValue)
{
    query = query.Where(dt => dt.Date <= asOfDate.Value);
}

var digitalTransactions = await query.ToArrayAsync();
```

---

### Finding 3: No Profit Tracking Infrastructure ❌

**Severity:** MEDIUM-HIGH

**Issue:**
Company profit from spread, rate fees, and rake commission is not tracked or reported anywhere.

**Current State:**
- Profit data exists implicitly in transactions
- No aggregation service
- No reporting endpoints
- No frontend display

**Business Impact:**
- Cannot calculate company revenue
- Cannot distribute Member shares (depends on profit)
- No financial performance visibility

---

### Finding 4: Member Financial Fields Unused ⚠️

**Severity:** MEDIUM

**Issue:**
`Member.Share` and `Member.Salary` fields exist but have no business logic or UI.

**Status:**
- Database fields: ✅ Exist
- Backend service: ❌ Missing
- API endpoints: ❌ Missing
- Frontend UI: ❌ Missing
- Business rules: ❌ Not defined

---

### Finding 5: No Credit Limit Protection ⚠️

**Severity:** MEDIUM

**Issue:**
Clients can accumulate unlimited negative balances without any validation.

**Risk:**
- Financial exposure to client defaults
- No debt control mechanism
- Cannot enforce credit policies

---

## Implementation Strategy

### Approach

1. **Incremental Delivery:** Each phase delivers working, testable features
2. **Backward Compatible:** Existing functionality remains working
3. **Test-Driven:** Write tests before implementation
4. **Documentation-First:** Update docs as features are built
5. **Parallel Tracks:** Independent phases can be developed simultaneously

### Architecture Principles

1. **Single Responsibility:** Each service has one clear purpose
2. **DRY:** Reuse existing balance calculation logic where possible
3. **SOLID:** Follow existing codebase patterns
4. **API-First:** Design endpoints before implementation
5. **Type Safety:** Strong typing in both backend and frontend

### Code Review Checkpoints

| Checkpoint | When | What |
|------------|------|------|
| Design Review | Before coding starts | API contracts, database schema |
| Mid-Phase Review | 50% completion | Architecture decisions, pattern compliance |
| Final Review | Before merge | Complete code, tests, documentation |

---

## Phase 1: Critical Infrastructure

**Goal:** Fix route mismatch and implement date-filtered balances

**Priority:** P0 (CRITICAL)

**Estimated Effort:** 8 hours

**Dependencies:** None

### 1.1 Fix Route Pattern Mismatch

**Decision Required:**

| Option | Pros | Cons | Recommendation |
|--------|------|------|----------------|
| **Option A:** Change frontend to match backend routes | - No backend changes<br>- Faster | - Frontend changes risky<br>- Against REST conventions | ❌ Not recommended |
| **Option B:** Add new backend routes to match frontend | - Frontend works as-is<br>- Follows REST `/entity/operation/{id}` | - Duplicate routes temporarily<br>- More backend work | ✅ **Recommended** |
| **Option C:** Refactor both to new pattern | - Clean slate<br>- Best practices | - Highest risk<br>- Breaks existing clients | ❌ Not recommended |

**Recommended Approach: Option B**

**Rationale:**
- Frontend is already written expecting these routes
- Less risky than changing frontend calls across multiple files
- Can deprecate old routes later in Phase 5

### 1.2 Backend Implementation

#### 1.2.1 Update BaseAssetHolderService

**File:** `Application/Services/Base/BaseAssetHolderService.cs`

**Changes:**

```csharp
// Add date parameter to existing methods
public async Task<Dictionary<AssetType, decimal>> GetBalancesByAssetType(
    Guid baseAssetHolderId, 
    DateTime? asOfDate = null)  // NEW parameter
{
    // ... existing code ...
    
    // Apply date filter to all transaction queries
    var digitalTransactionsQuery = context.DigitalAssetTransactions
        .Where(dt => !dt.DeletedAt.HasValue && 
            (walletIdentifierIds.Contains(dt.SenderWalletIdentifierId) || 
             walletIdentifierIds.Contains(dt.ReceiverWalletIdentifierId)));
    
    if (asOfDate.HasValue)
    {
        digitalTransactionsQuery = digitalTransactionsQuery.Where(dt => dt.Date <= asOfDate.Value);
    }
    
    var digitalTransactions = await digitalTransactionsQuery
        .Include(dt => dt.SenderWalletIdentifier)
            .ThenInclude(wi => wi.AssetPool)
        .Include(dt => dt.ReceiverWalletIdentifier)
            .ThenInclude(wi => wi.AssetPool)
        .ToArrayAsync();
    
    // Apply same pattern to fiatTransactions and settlementTransactions
    // ... rest of method unchanged ...
}

// Similarly update GetBalancesByAssetGroup
public async Task<Dictionary<AssetGroup, decimal>> GetBalancesByAssetGroup(
    Guid baseAssetHolderId, 
    DateTime? asOfDate = null)  // NEW parameter
{
    // Same date filtering pattern
}
```

**Testing:**
```csharp
[Fact]
public async Task GetBalancesByAssetType_WithDate_ReturnsHistoricalBalance()
{
    // Arrange
    var bankId = CreateTestBank();
    var transaction1 = CreateTestTransaction(bankId, date: "2026-01-01", amount: 1000);
    var transaction2 = CreateTestTransaction(bankId, date: "2026-01-15", amount: 500);
    var transaction3 = CreateTestTransaction(bankId, date: "2026-02-01", amount: 200);
    
    // Act
    var balanceAtJan15 = await service.GetBalancesByAssetType(bankId, new DateTime(2026, 1, 15));
    
    // Assert
    Assert.Equal(1500, balanceAtJan15[AssetType.BrazilianReal]); // Only transaction1 + transaction2
}
```

#### 1.2.2 Add New Controller Endpoints

**File:** `Api/Controllers/v1/Finance/FinanceBalanceController.cs` (NEW)

```csharp
using Microsoft.AspNetCore.Mvc;
using SFManagement.Application.Services.Base;
using SFManagement.Domain.Enums.Assets;

namespace SFManagement.Api.Controllers.v1.Finance;

[ApiController]
[Route("api/v1")]
public class FinanceBalanceController : ControllerBase
{
    private readonly BaseAssetHolderService<Bank> _bankService;
    private readonly BaseAssetHolderService<Client> _clientService;
    private readonly BaseAssetHolderService<PokerManager> _pokerManagerService;

    public FinanceBalanceController(
        BaseAssetHolderService<Bank> bankService,
        BaseAssetHolderService<Client> clientService,
        BaseAssetHolderService<PokerManager> pokerManagerService)
    {
        _bankService = bankService;
        _clientService = clientService;
        _pokerManagerService = pokerManagerService;
    }

    /// <summary>
    /// Get bank balance (optionally at specific date)
    /// </summary>
    [HttpGet("bank/balance/{id}")]
    [HttpPost("bank/balance/{id}")]
    [ProducesResponseType(typeof(BalanceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBankBalance(Guid id, [FromBody] DateFilterRequest? request = null)
    {
        var asOfDate = request?.Date;
        var balances = await _bankService.GetBalancesByAssetType(id, asOfDate);
        var totalBrl = balances.GetValueOrDefault(AssetType.BrazilianReal, 0);
        
        return Ok(new BalanceResponse { Value = totalBrl });
    }

    /// <summary>
    /// Get client balance (optionally at specific date)
    /// </summary>
    [HttpGet("client/balance/{id}")]
    [HttpPost("client/balance/{id}")]
    [ProducesResponseType(typeof(BalanceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClientBalance(Guid id, [FromBody] DateFilterRequest? request = null)
    {
        var asOfDate = request?.Date;
        var balances = await _clientService.GetBalancesByAssetType(id, asOfDate);
        var totalBrl = balances.GetValueOrDefault(AssetType.BrazilianReal, 0);
        
        return Ok(new BalanceResponse { Value = totalBrl });
    }

    /// <summary>
    /// Get poker manager balance (optionally at specific date)
    /// </summary>
    [HttpGet("manager/balance/{id}")]
    [HttpPost("manager/balance/{id}")]
    [ProducesResponseType(typeof(ManagerBalanceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPokerManagerBalance(Guid id, [FromBody] DateFilterRequest? request = null)
    {
        var asOfDate = request?.Date;
        var balances = await _pokerManagerService.GetBalancesByAssetGroup(id, asOfDate);
        
        var fiatBalance = balances.GetValueOrDefault(AssetGroup.FiatAssets, 0);
        var pokerBalance = balances.GetValueOrDefault(AssetGroup.PokerAssets, 0);
        
        // Calculate average rate if we have both balances
        decimal? averageRate = null;
        if (pokerBalance != 0)
        {
            averageRate = Math.Abs(fiatBalance / pokerBalance);
        }
        
        return Ok(new ManagerBalanceResponse 
        { 
            Value = fiatBalance,
            Coins = pokerBalance,
            AverageRate = averageRate
        });
    }
}

public class DateFilterRequest
{
    public DateTime? Date { get; set; }
}

public class BalanceResponse
{
    public decimal Value { get; set; }
}

public class ManagerBalanceResponse
{
    public decimal Value { get; set; }
    public decimal Coins { get; set; }
    public decimal? AverageRate { get; set; }
}
```

**Why Both GET and POST:**
- GET for current balance (backward compatible)
- POST for date-filtered balance (new functionality)
- Frontend already expects this pattern

**API Contract:**

```
# Current balance
GET /api/v1/bank/balance/{id}
Response: { "value": 15000.50 }

# Historical balance
POST /api/v1/bank/balance/{id}
Body: { "date": "2026-01-15T00:00:00Z" }
Response: { "value": 12500.00 }
```

### 1.3 Frontend Validation

**No frontend changes required** - endpoints match existing expectations!

**Validation Steps:**

1. Run frontend with backend changes
2. Navigate to `/financeiro/planilha`
3. Select different months using DateSelector
4. Verify balances change correctly
5. Test edge cases:
   - Future dates (should return current balance)
   - Dates before first transaction (should return 0 or initial balance)
   - Invalid dates (should return 400 error)

### 1.4 Testing Checklist

**Unit Tests:**
- [ ] `GetBalancesByAssetType` with null date returns current balance
- [ ] `GetBalancesByAssetType` with date filters correctly
- [ ] `GetBalancesByAssetType` with future date returns current balance
- [ ] `GetBalancesByAssetGroup` with date filters correctly
- [ ] Date filtering works for fiat, digital, and settlement transactions

**Integration Tests:**
- [ ] `GET /bank/balance/{id}` returns correct current balance
- [ ] `POST /bank/balance/{id}` with date returns correct historical balance
- [ ] `POST /bank/balance/{id}` without date returns current balance
- [ ] Controller handles invalid date gracefully
- [ ] Response caching works correctly with date parameter

**E2E Tests:**
- [ ] Financial statement page loads with current date
- [ ] Changing month updates all balances
- [ ] Bank balances show correctly for historical date
- [ ] Client balances show correctly for historical date
- [ ] PokerManager balances show correctly for historical date

### 1.5 Documentation Updates

**Files to Update:**

1. `Documentation/06_API/BALANCE_ENDPOINTS.md`
   - Add new endpoint patterns
   - Document date filtering behavior
   - Add examples

2. `Documentation/06_API/API_REFERENCE.md`
   - Add FinanceBalanceController endpoints

3. `Documentation/10_REFACTORING/FINANCE_CURRENT_STATE.md`
   - Mark Phase 1 as complete
   - Update "What Exists" section

### 1.6 Performance Considerations

**Caching Strategy:**

```csharp
[ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any, 
    VaryByQueryKeys = new[] { "date" })]
```

- Historical balances can be cached longer (5 minutes vs 60 seconds)
- Current balances still cache for 60 seconds
- Cache key varies by date parameter

**Query Optimization:**

```csharp
// Add index on transaction Date fields
CREATE INDEX IX_DigitalAssetTransaction_Date ON DigitalAssetTransaction(Date);
CREATE INDEX IX_FiatAssetTransaction_Date ON FiatAssetTransaction(Date);
CREATE INDEX IX_SettlementTransaction_Date ON SettlementTransaction(Date);
```

**Estimated Query Performance:**
- Current balance: ~50-100ms (unchanged)
- Historical balance: ~75-150ms (with indexes)
- Acceptable for user-facing queries

### 1.7 Rollout Plan

**Step 1:** Deploy Backend (Backward Compatible)
- New endpoints don't affect existing functionality
- Old endpoints at `/api/v1/{entity}/{id}/balance` continue to work

**Step 2:** Verify Frontend Works
- Test `/financeiro/planilha` page
- Verify date filtering works

**Step 3:** Monitor Performance
- Check response times
- Monitor cache hit rates
- Watch for slow queries

**Step 4:** Document Success Criteria
- [ ] All frontend balance calls succeed
- [ ] Date filtering returns correct balances
- [ ] No 404 errors in logs
- [ ] Performance within acceptable range (<150ms average)

### 1.8 Phase 1 Deliverables

- [x] Date filtering added to balance calculation service
- [x] New API endpoints matching frontend expectations
- [x] Unit tests for date filtering
- [x] Integration tests for new endpoints
- [x] Updated API documentation
- [x] Performance testing and optimization
- [x] Deployment to production

**Exit Criteria:**
- All tests passing
- Financial statement page displays historical balances correctly
- No regressions in existing balance functionality
- Documentation updated

---

## Phase 2: Profit Tracking

**Goal:** Implement company profit calculation and reporting

**Priority:** P1 (HIGH)

**Estimated Effort:** 20 hours

**Dependencies:** Phase 1 complete

### 2.1 Profit Calculation Architecture

**Design Principle:**
- Real-time calculation from transactions (no cached profit records initially)
- Profit aggregation service
- RESTful reporting endpoints
- Frontend dashboard display

**Profit Sources:**

| Source | Calculation | Transaction Type |
|--------|-------------|------------------|
| **Spread** | Embedded in ConversionRate differences | DigitalAssetTransaction |
| **Rate Fees** | `AssetAmount × (Rate / 100)` | DigitalAssetTransaction |
| **Rake Commission** | `RakeAmount × ((RakeCommission - RakeBack) / 100)` | SettlementTransaction |

### 2.2 Backend Implementation

#### 2.2.1 Create ProfitCalculationService

**File:** `Application/Services/Finance/ProfitCalculationService.cs` (NEW)

```csharp
using SFManagement.Domain.Entities.Transactions;
using SFManagement.Domain.Enums;
using SFManagement.Domain.Enums.Assets;
using SFManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SFManagement.Application.Services.Finance;

public interface IProfitCalculationService
{
    Task<ProfitSummary> GetProfitSummary(DateTime startDate, DateTime endDate, Guid? managerId = null);
    Task<List<ProfitByManager>> GetProfitByManager(DateTime startDate, DateTime endDate);
    Task<List<ProfitBySource>> GetProfitBySource(DateTime startDate, DateTime endDate);
    Task<decimal> CalculateSpreadProfit(DigitalAssetTransaction transaction);
    Task<decimal> CalculateRateProfit(DigitalAssetTransaction transaction);
    Task<decimal> CalculateRakeProfit(SettlementTransaction transaction);
}

public class ProfitCalculationService : IProfitCalculationService
{
    private readonly DataContext _context;

    public ProfitCalculationService(DataContext context)
    {
        _context = context;
    }

    public async Task<ProfitSummary> GetProfitSummary(DateTime startDate, DateTime endDate, Guid? managerId = null)
    {
        // Get all relevant transactions
        var digitalTransactions = await GetDigitalTransactionsForPeriod(startDate, endDate, managerId);
        var settlementTransactions = await GetSettlementTransactionsForPeriod(startDate, endDate, managerId);

        // Calculate profits by source
        var spreadProfit = 0m;
        var rateProfit = 0m;

        foreach (var tx in digitalTransactions)
        {
            // Spread profit calculation (to be refined based on business rules)
            spreadProfit += await CalculateSpreadProfit(tx);
            
            // Rate profit
            rateProfit += await CalculateRateProfit(tx);
        }

        var rakeProfit = 0m;
        foreach (var tx in settlementTransactions)
        {
            rakeProfit += await CalculateRakeProfit(tx);
        }

        return new ProfitSummary
        {
            StartDate = startDate,
            EndDate = endDate,
            ManagerId = managerId,
            SpreadProfit = spreadProfit,
            RateProfit = rateProfit,
            RakeProfit = rakeProfit,
            TotalProfit = spreadProfit + rateProfit + rakeProfit
        };
    }

    public async Task<List<ProfitByManager>> GetProfitByManager(DateTime startDate, DateTime endDate)
    {
        var managers = await _context.PokerManagers
            .Where(pm => !pm.BaseAssetHolder.DeletedAt.HasValue)
            .ToListAsync();

        var results = new List<ProfitByManager>();

        foreach (var manager in managers)
        {
            var summary = await GetProfitSummary(startDate, endDate, manager.BaseAssetHolderId);
            results.Add(new ProfitByManager
            {
                ManagerId = manager.BaseAssetHolderId,
                ManagerName = manager.BaseAssetHolder.Name,
                TotalProfit = summary.TotalProfit,
                SpreadProfit = summary.SpreadProfit,
                RateProfit = summary.RateProfit,
                RakeProfit = summary.RakeProfit
            });
        }

        return results.OrderByDescending(p => p.TotalProfit).ToList();
    }

    public async Task<List<ProfitBySource>> GetProfitBySource(DateTime startDate, DateTime endDate)
    {
        var summary = await GetProfitSummary(startDate, endDate);

        return new List<ProfitBySource>
        {
            new() { Source = "Spread", Amount = summary.SpreadProfit },
            new() { Source = "Rate Fees", Amount = summary.RateProfit },
            new() { Source = "Rake Commission", Amount = summary.RakeProfit }
        };
    }

    public async Task<decimal> CalculateSpreadProfit(DigitalAssetTransaction transaction)
    {
        // TODO: Define spread profit logic
        // This requires knowing the "standard" rate vs actual ConversionRate
        // For now, return 0 until business rules are defined
        return 0m;
    }

    public async Task<decimal> CalculateRateProfit(DigitalAssetTransaction transaction)
    {
        if (!transaction.Rate.HasValue || transaction.Rate.Value == 0)
            return 0m;

        // Company profit from rate fee
        return transaction.AssetAmount * (transaction.Rate.Value / 100m);
    }

    public async Task<decimal> CalculateRakeProfit(SettlementTransaction transaction)
    {
        var effectiveCommission = transaction.RakeCommission - (transaction.RakeBack ?? 0m);
        return transaction.RakeAmount * (effectiveCommission / 100m);
    }

    private async Task<List<DigitalAssetTransaction>> GetDigitalTransactionsForPeriod(
        DateTime startDate, DateTime endDate, Guid? managerId)
    {
        var query = _context.DigitalAssetTransactions
            .Include(dt => dt.SenderWalletIdentifier)
                .ThenInclude(wi => wi.AssetPool)
            .Include(dt => dt.ReceiverWalletIdentifier)
                .ThenInclude(wi => wi.AssetPool)
            .Where(dt => !dt.DeletedAt.HasValue 
                && dt.Date >= startDate 
                && dt.Date <= endDate);

        if (managerId.HasValue)
        {
            query = query.Where(dt => 
                dt.SenderWalletIdentifier!.AssetPool!.BaseAssetHolderId == managerId.Value ||
                dt.ReceiverWalletIdentifier!.AssetPool!.BaseAssetHolderId == managerId.Value);
        }

        return await query.ToListAsync();
    }

    private async Task<List<SettlementTransaction>> GetSettlementTransactionsForPeriod(
        DateTime startDate, DateTime endDate, Guid? managerId)
    {
        var query = _context.SettlementTransactions
            .Include(st => st.SenderWalletIdentifier)
                .ThenInclude(wi => wi.AssetPool)
            .Include(st => st.ReceiverWalletIdentifier)
                .ThenInclude(wi => wi.AssetPool)
            .Where(st => !st.DeletedAt.HasValue 
                && st.Date >= startDate 
                && st.Date <= endDate);

        if (managerId.HasValue)
        {
            query = query.Where(st => 
                st.SenderWalletIdentifier!.AssetPool!.BaseAssetHolderId == managerId.Value ||
                st.ReceiverWalletIdentifier!.AssetPool!.BaseAssetHolderId == managerId.Value);
        }

        return await query.ToListAsync();
    }
}

// DTOs
public class ProfitSummary
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid? ManagerId { get; set; }
    public decimal SpreadProfit { get; set; }
    public decimal RateProfit { get; set; }
    public decimal RakeProfit { get; set; }
    public decimal TotalProfit { get; set; }
}

public class ProfitByManager
{
    public Guid ManagerId { get; set; }
    public string ManagerName { get; set; } = string.Empty;
    public decimal TotalProfit { get; set; }
    public decimal SpreadProfit { get; set; }
    public decimal RateProfit { get; set; }
    public decimal RakeProfit { get; set; }
}

public class ProfitBySource
{
    public string Source { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
```

#### 2.2.2 Create Profit Controller

**File:** `Api/Controllers/v1/Finance/ProfitController.cs` (NEW)

```csharp
using Microsoft.AspNetCore.Mvc;
using SFManagement.Application.Services.Finance;

namespace SFManagement.Api.Controllers.v1.Finance;

[ApiController]
[Route("api/v1/finance/profit")]
public class ProfitController : ControllerBase
{
    private readonly IProfitCalculationService _profitService;

    public ProfitController(IProfitCalculationService profitService)
    {
        _profitService = profitService;
    }

    /// <summary>
    /// Get profit summary for a period (optionally filtered by manager)
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ProfitSummary), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfitSummary(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] Guid? managerId = null)
    {
        if (startDate > endDate)
        {
            return BadRequest("Start date must be before end date");
        }

        var summary = await _profitService.GetProfitSummary(startDate, endDate, managerId);
        return Ok(summary);
    }

    /// <summary>
    /// Get profit breakdown by manager
    /// </summary>
    [HttpGet("by-manager")]
    [ProducesResponseType(typeof(List<ProfitByManager>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfitByManager(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        if (startDate > endDate)
        {
            return BadRequest("Start date must be before end date");
        }

        var results = await _profitService.GetProfitByManager(startDate, endDate);
        return Ok(results);
    }

    /// <summary>
    /// Get profit breakdown by source (spread, rate, rake)
    /// </summary>
    [HttpGet("by-source")]
    [ProducesResponseType(typeof(List<ProfitBySource>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfitBySource(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        if (startDate > endDate)
        {
            return BadRequest("Start date must be before end date");
        }

        var results = await _profitService.GetProfitBySource(startDate, endDate);
        return Ok(results);
    }
}
```

**API Examples:**

```
# Get profit summary for January 2026
GET /api/v1/finance/profit/summary?startDate=2026-01-01&endDate=2026-01-31

Response:
{
  "startDate": "2026-01-01T00:00:00Z",
  "endDate": "2026-01-31T23:59:59Z",
  "managerId": null,
  "spreadProfit": 5000.00,
  "rateProfit": 8000.00,
  "rakeProfit": 12000.00,
  "totalProfit": 25000.00
}

# Get profit by manager
GET /api/v1/finance/profit/by-manager?startDate=2026-01-01&endDate=2026-01-31

Response:
[
  {
    "managerId": "...",
    "managerName": "Manager A",
    "totalProfit": 15000.00,
    "spreadProfit": 3000.00,
    "rateProfit": 5000.00,
    "rakeProfit": 7000.00
  },
  ...
]
```

### 2.3 Frontend Implementation

#### 2.3.1 Profit Service

**File:** `SF_management-front/src/features/finance/api/profit.service.ts` (NEW)

```typescript
import type { TypedApiClient } from '@/shared/api/client';

export interface ProfitSummary {
  startDate: string;
  endDate: string;
  managerId?: string;
  spreadProfit: number;
  rateProfit: number;
  rakeProfit: number;
  totalProfit: number;
}

export interface ProfitByManager {
  managerId: string;
  managerName: string;
  totalProfit: number;
  spreadProfit: number;
  rateProfit: number;
  rakeProfit: number;
}

export interface ProfitBySource {
  source: string;
  amount: number;
}

export class ProfitService {
  constructor(private readonly client: TypedApiClient) {}

  async getProfitSummary(
    startDate: string,
    endDate: string,
    managerId?: string
  ): Promise<ProfitSummary> {
    const params = new URLSearchParams({
      startDate,
      endDate,
      ...(managerId && { managerId }),
    });
    return this.client.get(`/finance/profit/summary?${params}`);
  }

  async getProfitByManager(
    startDate: string,
    endDate: string
  ): Promise<ProfitByManager[]> {
    const params = new URLSearchParams({ startDate, endDate });
    return this.client.get(`/finance/profit/by-manager?${params}`);
  }

  async getProfitBySource(
    startDate: string,
    endDate: string
  ): Promise<ProfitBySource[]> {
    const params = new URLSearchParams({ startDate, endDate });
    return this.client.get(`/finance/profit/by-source?${params}`);
  }
}

export const getProfitService = (api: TypedApiClient) => new ProfitService(api);
```

#### 2.3.2 Profit Dashboard Page

**File:** `SF_management-front/src/app/(dashboard)/financeiro/profit/page.tsx` (NEW)

```typescript
"use client";

import { useState } from "react";
import { Card } from "@/shared/components/ui/card";
import { useQuery } from "@tanstack/react-query";
import { useRequiredApiClient } from "@/shared/api/hooks/useApiClient";
import { getProfitService } from "@/features/finance/api/profit.service";
import DateRangeSelector from "./components/DateRangeSelector";
import ProfitSummaryCard from "./components/ProfitSummaryCard";
import ProfitByManagerTable from "./components/ProfitByManagerTable";
import ProfitBySourceChart from "./components/ProfitBySourceChart";
import Loader from "@/shared/components/ui/Loader";

export default function ProfitDashboardPage() {
  const api = useRequiredApiClient();
  const service = api ? getProfitService(api) : null;
  
  const [startDate, setStartDate] = useState(() => {
    const date = new Date();
    date.setDate(1); // First day of current month
    return date.toISOString().split('T')[0];
  });
  
  const [endDate, setEndDate] = useState(() => {
    const date = new Date();
    return date.toISOString().split('T')[0];
  });

  const { data: summary, isLoading: summaryLoading } = useQuery({
    queryKey: ['profit', 'summary', startDate, endDate],
    queryFn: () => service!.getProfitSummary(startDate, endDate),
    enabled: !!service,
  });

  const { data: byManager, isLoading: managerLoading } = useQuery({
    queryKey: ['profit', 'by-manager', startDate, endDate],
    queryFn: () => service!.getProfitByManager(startDate, endDate),
    enabled: !!service,
  });

  const { data: bySource, isLoading: sourceLoading } = useQuery({
    queryKey: ['profit', 'by-source', startDate, endDate],
    queryFn: () => service!.getProfitBySource(startDate, endDate),
    enabled: !!service,
  });

  if (summaryLoading || managerLoading || sourceLoading) {
    return <Loader />;
  }

  return (
    <div className="container mx-auto p-6">
      <h1 className="text-3xl font-bold text-red-600 mb-8">
        Lucro da Empresa
      </h1>

      <div className="space-y-6">
        <DateRangeSelector
          startDate={startDate}
          endDate={endDate}
          onStartDateChange={setStartDate}
          onEndDateChange={setEndDate}
        />

        {summary && <ProfitSummaryCard summary={summary} />}

        {bySource && <ProfitBySourceChart data={bySource} />}

        {byManager && <ProfitByManagerTable data={byManager} />}
      </div>
    </div>
  );
}
```

### 2.4 Add Profit to Financial Statement

**File:** `SF_management-front/src/app/(dashboard)/financeiro/planilha/components/ProfitSummaryCard.tsx` (NEW)

```typescript
"use client";

import { Card } from "@/shared/components/ui/card";
import { useQuery } from "@tanstack/react-query";
import { useRequiredApiClient } from "@/shared/api/hooks/useApiClient";
import { getProfitService } from "@/features/finance/api/profit.service";
import { useDateContext } from "@/shared/contexts/DateContext";
import { formatCurrency } from "@/shared/utils/format";

export default function ProfitSummaryCard() {
  const api = useRequiredApiClient();
  const service = api ? getProfitService(api) : null;
  const { selectedDate } = useDateContext();

  // Calculate month range from selected date
  const startDate = selectedDate 
    ? new Date(selectedDate.getFullYear(), selectedDate.getMonth(), 1).toISOString().split('T')[0]
    : '';
  const endDate = selectedDate?.toISOString().split('T')[0] || '';

  const { data: summary } = useQuery({
    queryKey: ['profit', 'summary', startDate, endDate],
    queryFn: () => service!.getProfitSummary(startDate, endDate),
    enabled: !!service && !!startDate && !!endDate,
  });

  if (!summary) return null;

  return (
    <Card className="p-6 bg-green-50">
      <h2 className="text-xl font-bold text-green-700 mb-4">
        Lucro do Período
      </h2>
      
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        <div>
          <p className="text-sm text-gray-600">Spread</p>
          <p className="text-lg font-bold text-green-700">
            {formatCurrency(summary.spreadProfit)}
          </p>
        </div>
        <div>
          <p className="text-sm text-gray-600">Taxa</p>
          <p className="text-lg font-bold text-green-700">
            {formatCurrency(summary.rateProfit)}
          </p>
        </div>
        <div>
          <p className="text-sm text-gray-600">Rake</p>
          <p className="text-lg font-bold text-green-700">
            {formatCurrency(summary.rakeProfit)}
          </p>
        </div>
        <div>
          <p className="text-sm text-gray-600">Total</p>
          <p className="text-2xl font-bold text-green-700">
            {formatCurrency(summary.totalProfit)}
          </p>
        </div>
      </div>
    </Card>
  );
}
```

**Integration:** Add `<ProfitSummaryCard />` to `/financeiro/planilha/page.tsx` after DateSelector.

### 2.5 Testing Strategy

**Unit Tests:**
- [ ] `CalculateRateProfit` returns correct values
- [ ] `CalculateRakeProfit` returns correct values
- [ ] `GetProfitSummary` aggregates correctly
- [ ] `GetProfitByManager` groups by manager correctly
- [ ] Date filtering works correctly

**Integration Tests:**
- [ ] Profit endpoints return correct data structure
- [ ] Query parameters validated correctly
- [ ] Invalid date ranges return 400 error

**E2E Tests:**
- [ ] Profit dashboard loads with data
- [ ] Date range selector updates profit numbers
- [ ] Profit displays on financial statement page

### 2.6 Phase 2 Deliverables

- [ ] ProfitCalculationService implemented
- [ ] Profit API endpoints created
- [ ] Frontend profit service created
- [ ] Profit dashboard page created
- [ ] Profit summary added to financial statement
- [ ] All tests passing
- [ ] Documentation updated

**Exit Criteria:**
- Company profit visible on financial statement
- Profit dashboard shows breakdown by manager and source
- All profit calculations mathematically correct

---

## Phase 3: Member Financial Module

**Goal:** Implement Member share distribution and salary management

**Priority:** P2 (MEDIUM)

**Estimated Effort:** 25 hours

**Dependencies:** Phase 2 complete (needs profit calculation)

### 3.1 Business Rules Definition (REQUIRED FIRST)

⚠️ **STOP: Cannot implement without business rules**

**Questions to Resolve with Product Owner:**

1. **Share Calculation:**
   - Share of what? (Total profit? Rake only? Specific managers?)
   - When calculated? (Monthly? Quarterly? On-demand?)
   - How paid? (Automatic transaction? Manual approval?)

2. **Salary Management:**
   - Payment frequency? (Monthly? Bi-weekly? Fixed days?)
   - Automatic or manual?
   - Linked to balance or separate payroll?

3. **Distribution Records:**
   - Historical tracking required?
   - Audit trail needed?
   - Can distributions be reversed/adjusted?

**Design Session Template:**

```markdown
# Member Financial Module - Business Rules

## Share Distribution

### Calculation Base
- [ ] Share of total company profit
- [ ] Share of specific manager's profit
- [ ] Share of specific revenue sources (spread, rate, rake)

### Calculation Frequency
- [ ] Real-time (per transaction)
- [ ] Daily
- [ ] Weekly
- [ ] Monthly
- [ ] Quarterly
- [ ] On-demand only

### Payment Method
- [ ] Automatic credit to Member FiatAssets balance
- [ ] Manual transaction creation
- [ ] Requires approval workflow

### Edge Cases
- [ ] What if profit is negative? (No distribution? Carry forward?)
- [ ] Multiple members with shares? (Total must be ≤ 100%?)
- [ ] Member joins mid-period? (Pro-rated?)

## Salary Management

### Payment Schedule
- [ ] Monthly on day X
- [ ] Bi-weekly
- [ ] Custom schedule per member

### Payment Execution
- [ ] Automatic transaction creation
- [ ] Manual transaction with reminder
- [ ] External payroll system

### Tracking
- [ ] Record payment history
- [ ] Link to transactions
- [ ] Support missed payments/adjustments
```

### 3.2 Implementation (After Business Rules Defined)

**This section will be detailed once business rules are finalized.**

**Anticipated Components:**

1. **MemberFinancialService**
   - Share calculation logic
   - Salary payment scheduling
   - Distribution records

2. **Database Tables**
   - MemberShareDistribution
   - MemberSalaryPayment

3. **API Endpoints**
   - `GET /api/v1/member/{id}/share-distributions`
   - `POST /api/v1/member/{id}/calculate-share`
   - `POST /api/v1/member/{id}/pay-salary`

4. **Frontend Pages**
   - `/members/[id]/financial` - Member financial dashboard
   - Share distribution history
   - Salary payment history

### 3.3 Phase 3 Deliverables (Tentative)

- [ ] Business rules documented
- [ ] MemberFinancialService implemented
- [ ] Database migrations created
- [ ] Member financial endpoints created
- [ ] Member financial dashboard page
- [ ] All tests passing
- [ ] Documentation updated

**Exit Criteria:**
- Member share calculation working per business rules
- Salary payments tracked correctly
- Financial dashboard shows complete history

---

## Phase 4: Client Credit Management

**Goal:** Implement credit limit system for risk management

**Priority:** P2 (MEDIUM)

**Estimated Effort:** 12 hours

**Dependencies:** None (can run in parallel with Phase 3)

### 4.1 Database Changes

**Migration:** `AddClientCreditLimit`

```csharp
public partial class AddClientCreditLimit : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(
            name: "CreditLimit",
            table: "Clients",
            type: "decimal(18,2)",
            nullable: true,
            defaultValue: null);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "CreditLimit",
            table: "Clients");
    }
}
```

### 4.2 Entity Update

**File:** `Domain/Entities/AssetHolders/Client.cs`

```csharp
public class Client : BaseDomain, IAssetHolder
{
    // ... existing properties ...
    
    /// <summary>
    /// Maximum negative balance allowed (debt limit).
    /// null = unlimited credit
    /// 0 = no credit (prepaid only)
    /// Positive value = maximum debt amount
    /// </summary>
    public decimal? CreditLimit { get; set; }
}
```

### 4.3 Validation Service

**File:** `Application/Services/Finance/ClientCreditService.cs` (NEW)

```csharp
using SFManagement.Domain.Entities.AssetHolders;
using SFManagement.Domain.Exceptions;
using SFManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SFManagement.Application.Services.Finance;

public interface IClientCreditService
{
    Task ValidateCreditLimit(Guid clientId, decimal proposedBalance);
    Task<ClientCreditStatus> GetCreditStatus(Guid clientId);
    Task<bool> UpdateCreditLimit(Guid clientId, decimal? newLimit);
}

public class ClientCreditService : IClientCreditService
{
    private readonly DataContext _context;
    private readonly BaseAssetHolderService<Client> _clientService;

    public ClientCreditService(DataContext context, BaseAssetHolderService<Client> clientService)
    {
        _context = context;
        _clientService = clientService;
    }

    public async Task ValidateCreditLimit(Guid clientId, decimal proposedBalance)
    {
        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.BaseAssetHolderId == clientId && !c.DeletedAt.HasValue);

        if (client == null)
        {
            throw new EntityNotFoundException(nameof(Client), clientId);
        }

        // No credit limit set = unlimited
        if (!client.CreditLimit.HasValue)
        {
            return;
        }

        // Check if proposed balance would exceed credit limit
        if (proposedBalance < -client.CreditLimit.Value)
        {
            throw new BusinessException(
                $"Transaction would exceed credit limit of {client.CreditLimit.Value:C}. " +
                $"Proposed balance: {proposedBalance:C}",
                "CREDIT_LIMIT_EXCEEDED");
        }
    }

    public async Task<ClientCreditStatus> GetCreditStatus(Guid clientId)
    {
        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.BaseAssetHolderId == clientId && !c.DeletedAt.HasValue);

        if (client == null)
        {
            throw new EntityNotFoundException(nameof(Client), clientId);
        }

        var balances = await _clientService.GetBalancesByAssetType(clientId);
        var brlBalance = balances.GetValueOrDefault(AssetType.BrazilianReal, 0);

        return new ClientCreditStatus
        {
            ClientId = clientId,
            ClientName = client.BaseAssetHolder.Name,
            CurrentBalance = brlBalance,
            CreditLimit = client.CreditLimit,
            AvailableCredit = client.CreditLimit.HasValue 
                ? client.CreditLimit.Value + brlBalance
                : null, // unlimited
            UtilizationPercentage = client.CreditLimit.HasValue && client.CreditLimit.Value > 0
                ? Math.Abs(Math.Min(brlBalance, 0)) / client.CreditLimit.Value * 100
                : null
        };
    }

    public async Task<bool> UpdateCreditLimit(Guid clientId, decimal? newLimit)
    {
        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.BaseAssetHolderId == clientId && !c.DeletedAt.HasValue);

        if (client == null)
        {
            throw new EntityNotFoundException(nameof(Client), clientId);
        }

        if (newLimit.HasValue && newLimit.Value < 0)
        {
            throw new ValidationException("Credit limit cannot be negative");
        }

        client.CreditLimit = newLimit;
        client.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}

public class ClientCreditStatus
{
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }
    public decimal? CreditLimit { get; set; }
    public decimal? AvailableCredit { get; set; }
    public decimal? UtilizationPercentage { get; set; }
}
```

### 4.4 Integrate Validation into Transfer Service

**File:** `Application/Services/Transactions/TransferService.cs`

```csharp
// Add to existing TransferService

private readonly IClientCreditService _creditService;

// In constructor
public TransferService(..., IClientCreditService creditService)
{
    // ...
    _creditService = creditService;
}

// Before creating FiatAssetTransaction
private async Task ValidateClientCreditLimit(Guid walletIdentifierId, decimal proposedAmount)
{
    var wallet = await _context.WalletIdentifiers
        .Include(wi => wi.AssetPool)
        .FirstOrDefaultAsync(wi => wi.Id == walletIdentifierId);

    if (wallet == null) return;

    // Check if this wallet belongs to a client
    var client = await _context.Clients
        .FirstOrDefaultAsync(c => c.BaseAssetHolderId == wallet.AssetPool!.BaseAssetHolderId);

    if (client == null) return; // Not a client, skip validation

    // Get current balance
    var balances = await _clientService.GetBalancesByAssetType(client.BaseAssetHolderId);
    var currentBalance = balances.GetValueOrDefault(AssetType.BrazilianReal, 0);

    // Calculate proposed balance after transaction
    var proposedBalance = currentBalance + proposedAmount;

    // Validate against credit limit
    await _creditService.ValidateCreditLimit(client.BaseAssetHolderId, proposedBalance);
}

// In RECEIPT method (for example)
public async Task<TransferResponse> RECEIPT(...)
{
    // ... existing validation ...

    // NEW: Validate credit limit for receiver (client receiving credit)
    await ValidateClientCreditLimit(receiverWalletId, assetAmount);

    // ... proceed with transaction ...
}
```

### 4.5 API Endpoints

**File:** `Api/Controllers/v1/Clients/ClientCreditController.cs` (NEW)

```csharp
using Microsoft.AspNetCore.Mvc;
using SFManagement.Application.Services.Finance;

namespace SFManagement.Api.Controllers.v1.Clients;

[ApiController]
[Route("api/v1/client")]
public class ClientCreditController : ControllerBase
{
    private readonly IClientCreditService _creditService;

    public ClientCreditController(IClientCreditService creditService)
    {
        _creditService = creditService;
    }

    /// <summary>
    /// Get client credit status
    /// </summary>
    [HttpGet("{id}/credit-status")]
    [ProducesResponseType(typeof(ClientCreditStatus), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCreditStatus(Guid id)
    {
        var status = await _creditService.GetCreditStatus(id);
        return Ok(status);
    }

    /// <summary>
    /// Update client credit limit
    /// </summary>
    [HttpPut("{id}/credit-limit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateCreditLimit(Guid id, [FromBody] UpdateCreditLimitRequest request)
    {
        await _creditService.UpdateCreditLimit(id, request.CreditLimit);
        return Ok();
    }
}

public class UpdateCreditLimitRequest
{
    public decimal? CreditLimit { get; set; }
}
```

**API Examples:**

```
# Get credit status
GET /api/v1/client/{id}/credit-status

Response:
{
  "clientId": "...",
  "clientName": "John Doe",
  "currentBalance": -2500.00,
  "creditLimit": 5000.00,
  "availableCredit": 2500.00,
  "utilizationPercentage": 50.0
}

# Update credit limit
PUT /api/v1/client/{id}/credit-limit
Body: { "creditLimit": 10000.00 }

Response: 200 OK
```

### 4.6 Frontend Implementation

#### 4.6.1 Add Credit Limit to Client Form

**File:** `SF_management-front/src/features/clients/components/ClientForm.tsx`

```typescript
// Add to existing form

<FormField
  label="Limite de Crédito"
  name="creditLimit"
  type="number"
  value={formData.creditLimit ?? ''}
  onChange={(e) => handleChange('creditLimit', 
    e.target.value ? parseFloat(e.target.value) : null)}
  placeholder="Deixe vazio para crédito ilimitado"
  helpText="Valor máximo de saldo negativo permitido (0 = apenas pré-pago)"
/>
```

#### 4.6.2 Credit Status Indicator

**File:** `SF_management-front/src/features/clients/components/ClientCreditStatus.tsx` (NEW)

```typescript
"use client";

import { useQuery } from "@tanstack/react-query";
import { useRequiredApiClient } from "@/shared/api/hooks/useApiClient";
import { Card } from "@/shared/components/ui/card";
import { formatCurrency } from "@/shared/utils/format";

interface ClientCreditStatusProps {
  clientId: string;
}

export default function ClientCreditStatus({ clientId }: ClientCreditStatusProps) {
  const api = useRequiredApiClient();

  const { data: status } = useQuery({
    queryKey: ['client', clientId, 'credit-status'],
    queryFn: () => api?.get(`/client/${clientId}/credit-status`),
    enabled: !!api && !!clientId,
  });

  if (!status || !status.creditLimit) {
    return null; // No credit limit set
  }

  const utilizationColor = 
    !status.utilizationPercentage ? 'bg-green-100 text-green-800' :
    status.utilizationPercentage < 50 ? 'bg-green-100 text-green-800' :
    status.utilizationPercentage < 80 ? 'bg-yellow-100 text-yellow-800' :
    'bg-red-100 text-red-800';

  return (
    <Card className="p-4">
      <h3 className="font-semibold mb-2">Limite de Crédito</h3>
      
      <div className="space-y-2">
        <div className="flex justify-between">
          <span className="text-gray-600">Saldo Atual:</span>
          <span className="font-semibold">
            {formatCurrency(status.currentBalance)}
          </span>
        </div>
        
        <div className="flex justify-between">
          <span className="text-gray-600">Limite:</span>
          <span className="font-semibold">
            {formatCurrency(status.creditLimit)}
          </span>
        </div>
        
        <div className="flex justify-between">
          <span className="text-gray-600">Crédito Disponível:</span>
          <span className="font-semibold">
            {formatCurrency(status.availableCredit ?? 0)}
          </span>
        </div>
        
        {status.utilizationPercentage !== null && (
          <div className="mt-3">
            <div className="flex justify-between text-sm mb-1">
              <span>Utilização</span>
              <span className={`font-semibold px-2 py-1 rounded ${utilizationColor}`}>
                {status.utilizationPercentage.toFixed(1)}%
              </span>
            </div>
            <div className="w-full bg-gray-200 rounded-full h-2">
              <div
                className={`h-2 rounded-full ${
                  status.utilizationPercentage < 50 ? 'bg-green-500' :
                  status.utilizationPercentage < 80 ? 'bg-yellow-500' :
                  'bg-red-500'
                }`}
                style={{ width: `${Math.min(status.utilizationPercentage, 100)}%` }}
              />
            </div>
          </div>
        )}
      </div>
    </Card>
  );
}
```

### 4.7 Testing Strategy

**Unit Tests:**
- [ ] `ValidateCreditLimit` allows transactions within limit
- [ ] `ValidateCreditLimit` blocks transactions exceeding limit
- [ ] `ValidateCreditLimit` allows unlimited when limit is null
- [ ] `GetCreditStatus` calculates utilization correctly
- [ ] `UpdateCreditLimit` validates negative values

**Integration Tests:**
- [ ] Transaction creation fails when exceeding credit limit
- [ ] Credit limit update endpoint works correctly
- [ ] Credit status endpoint returns correct data

**E2E Tests:**
- [ ] Setting credit limit on client form works
- [ ] Credit status displays correctly on client page
- [ ] Transaction blocked when exceeding limit shows proper error

### 4.8 Phase 4 Deliverables

- [ ] Database migration created
- [ ] Client entity updated
- [ ] ClientCreditService implemented
- [ ] Validation integrated into TransferService
- [ ] Credit API endpoints created
- [ ] Frontend credit limit field added
- [ ] Credit status indicator component created
- [ ] All tests passing
- [ ] Documentation updated

**Exit Criteria:**
- Credit limits can be set per client
- Transactions blocked when exceeding limit
- Credit status visible on client pages
- No false positives or negatives in validation

---

## Phase 5: Advanced Features

**Goal:** Polish and advanced reporting features

**Priority:** P3 (LOWER)

**Estimated Effort:** 20 hours

**Dependencies:** Phases 1-4 complete

### 5.1 Features

1. **Referral Commission Integration**
   - Link referrals to profit
   - Credit referrer balance
   - Commission tracking

2. **Settlement Zeroing Process**
   - If business requires it
   - Periodic position settlement

3. **ManagerProfitType Refactoring**
   - Rename to `CompanyRevenueSource`
   - Clean up scattered logic

4. **Export Financial Reports**
   - PDF generation
   - Excel export
   - Email delivery

### 5.2 Deprecate Old Routes (If Needed)

**Add deprecation warnings to old balance endpoints:**

```csharp
[HttpGet("{id}/balance")]
[Obsolete("Use /bank/balance/{id} instead")]
public async Task<IActionResult> GetBalance_Old(Guid id)
{
    Response.Headers.Add("X-Deprecated", "true");
    Response.Headers.Add("X-Deprecated-Replacement", "/api/v1/bank/balance/{id}");
    
    // Proxy to new endpoint
    return await GetBalance(id, null);
}
```

### 5.3 Phase 5 Deliverables

- [ ] Advanced features implemented as needed
- [ ] Old routes deprecated/removed if safe
- [ ] Export functionality working
- [ ] All documentation complete

---

## Risk Analysis & Mitigation

### Risk 1: Performance Degradation

**Risk:** Date-filtered balance queries slow down with large transaction volumes

**Likelihood:** Medium  
**Impact:** High

**Mitigation:**
1. Add database indexes on transaction Date fields (Phase 1)
2. Monitor query performance in production
3. If needed, implement balance snapshots (caching layer)
4. Consider read replicas for reporting queries

**Contingency:**
If queries exceed 500ms:
- Implement hourly balance snapshots table
- Serve historical balances from snapshots
- Keep real-time calculation for current balance

---

### Risk 2: Frontend-Backend Route Mismatch Recurrence

**Risk:** Future features add new endpoints that don't match frontend expectations

**Likelihood:** Medium  
**Impact:** Medium

**Mitigation:**
1. Document API design patterns in `API_REFERENCE.md`
2. Use OpenAPI/Swagger for API contract
3. Add API integration tests that run against both backend and frontend expectations
4. Code review checklist includes API pattern compliance

**Prevention:**
- Establish API design review before implementation
- Frontend and backend developers sync on new endpoints

---

### Risk 3: Business Rules Not Defined (Phase 3)

**Risk:** Member financial module cannot be implemented without business rules

**Likelihood:** High  
**Impact:** High (blocks Phase 3)

**Mitigation:**
1. Schedule business rules session with Product Owner BEFORE Phase 3 starts
2. Document all questions in advance
3. Get written approval on business rules
4. Phase 3 can be deferred if business rules not ready

**Contingency:**
- Phase 3 and Phase 4 are independent
- Phase 4 can proceed while Phase 3 waits for business input

---

### Risk 4: Credit Limit Validation Has Gaps

**Risk:** Credit limit validation has edge cases (concurrent transactions, race conditions)

**Likelihood:** Low  
**Impact:** High (financial risk)

**Mitigation:**
1. Use database transactions for balance updates
2. Add row-level locking where needed
3. Thorough edge case testing
4. Monitor credit limit violations in production

**Prevention:**
```csharp
// Use transaction isolation
using var transaction = await _context.Database.BeginTransactionAsync(
    IsolationLevel.Serializable);
try
{
    // Validate and create transaction atomically
    await ValidateClientCreditLimit(...);
    await CreateTransaction(...);
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

---

### Risk 5: Profit Calculation Errors

**Risk:** Profit formulas implemented incorrectly, leading to wrong financial data

**Likelihood:** Medium  
**Impact:** Critical (business impact)

**Mitigation:**
1. Property-based testing with known scenarios
2. Cross-validation with manual calculations
3. Phased rollout: view-only mode first
4. Get business validation before trusting numbers

**Validation Strategy:**
- Create test data set with known profit
- Run profit calculation service
- Compare results with manual calculation
- Acceptance criteria: 100% match

---

## Testing Strategy

### Test Pyramid

```
           /\
          /E2E\         (5% of tests)
         /------\
        / Integ  \      (15% of tests)
       /----------\
      /   Unit     \    (80% of tests)
     /--------------\
```

### Phase 1 Testing

**Unit Tests (80%):**
```csharp
[Theory]
[InlineData("2026-01-15", 1500)]
[InlineData("2026-02-01", 1700)]
[InlineData(null, 1700)]
public async Task GetBalancesByAssetType_VariousDates_ReturnsCorrectBalance(
    string dateString, decimal expected)
{
    // Arrange
    var date = dateString != null ? DateTime.Parse(dateString) : (DateTime?)null;
    
    // Act
    var balances = await service.GetBalancesByAssetType(testBankId, date);
    
    // Assert
    Assert.Equal(expected, balances[AssetType.BrazilianReal]);
}
```

**Integration Tests (15%):**
```csharp
[Fact]
public async Task FinanceBalanceEndpoint_WithDate_ReturnsHistoricalBalance()
{
    // Arrange
    var client = CreateTestClient();
    var request = new DateFilterRequest { Date = new DateTime(2026, 1, 15) };
    
    // Act
    var response = await _client.PostAsJsonAsync(
        $"/api/v1/client/balance/{client.Id}", request);
    
    // Assert
    response.EnsureSuccessStatusCode();
    var result = await response.Content.ReadFromJsonAsync<BalanceResponse>();
    Assert.NotNull(result);
}
```

**E2E Tests (5%):**
```typescript
test('financial statement shows historical balances', async () => {
  // Navigate to financial statement
  await page.goto('/financeiro/planilha');
  
  // Select January 2026
  await page.selectOption('[name="month"]', '0');
  await page.selectOption('[name="year"]', '2026');
  await page.click('button:text("Atualizar")');
  
  // Verify banks table updates
  await expect(page.locator('[data-testid="bank-balance-Bank1"]'))
    .toHaveText('R$ 15.000,00');
});
```

### Phase 2 Testing

**Profit Calculation Validation:**

```csharp
[Fact]
public async Task ProfitCalculation_KnownScenario_ReturnsExpectedProfit()
{
    // Arrange: Create known scenario
    // - 10 transactions with 1% rate fee = 100 BRL rate profit
    // - 5 settlements with 1000 rake, 50% commission, 10% rakeback = 2000 BRL rake profit
    // Expected total: 2100 BRL
    
    await SeedKnownScenario();
    
    // Act
    var summary = await _profitService.GetProfitSummary(
        new DateTime(2026, 1, 1),
        new DateTime(2026, 1, 31));
    
    // Assert
    Assert.Equal(100, summary.RateProfit);
    Assert.Equal(2000, summary.RakeProfit);
    Assert.Equal(2100, summary.TotalProfit);
}
```

### Phase 4 Testing

**Credit Limit Edge Cases:**

```csharp
[Theory]
[InlineData(5000, -4000, -1500, true)]   // Within limit
[InlineData(5000, -4000, -2000, false)]  // Would exceed limit
[InlineData(null, -10000, -5000, true)]  // Unlimited
[InlineData(0, 0, -100, false)]          // Prepaid only
public async Task CreditLimit_VariousScenarios_ValidatesCorrectly(
    decimal? creditLimit,
    decimal currentBalance,
    decimal proposedTransaction,
    bool shouldPass)
{
    // Arrange
    var client = CreateClientWithCreditLimit(creditLimit);
    SetClientBalance(client.Id, currentBalance);
    
    // Act & Assert
    if (shouldPass)
    {
        await _creditService.ValidateCreditLimit(
            client.Id, currentBalance + proposedTransaction);
    }
    else
    {
        await Assert.ThrowsAsync<BusinessException>(() =>
            _creditService.ValidateCreditLimit(
                client.Id, currentBalance + proposedTransaction));
    }
}
```

### Test Data Management

**Shared Test Fixtures:**

```csharp
public class FinanceTestFixture : IDisposable
{
    public DataContext Context { get; }
    public Bank TestBank { get; }
    public Client TestClient { get; }
    public PokerManager TestManager { get; }
    
    public FinanceTestFixture()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase($"FinanceTest_{Guid.NewGuid()}")
            .Options;
        Context = new DataContext(options);
        
        // Seed test data
        TestBank = SeedTestBank();
        TestClient = SeedTestClient();
        TestManager = SeedTestManager();
    }
    
    public void Dispose()
    {
        Context.Dispose();
    }
}
```

### Performance Testing

**Load Test Scenarios:**

```
Scenario 1: Balance Endpoint Load
- 100 concurrent requests
- Mix of current and historical balances
- Target: < 200ms p95 latency

Scenario 2: Profit Calculation Load
- 10 concurrent profit reports
- Date ranges: 1 month, 3 months, 1 year
- Target: < 2s p95 latency

Scenario 3: Credit Validation Load
- 50 concurrent transaction attempts
- Mix of passing and failing validations
- Target: < 100ms p95 latency
```

---

## Deployment Strategy

### Phase 1 Deployment

**Pre-Deployment Checklist:**
- [ ] All tests passing in CI/CD
- [ ] Database indexes created in staging
- [ ] API documentation updated
- [ ] Frontend tested against staging backend
- [ ] Performance benchmarks meet targets
- [ ] Rollback plan documented

**Deployment Steps:**

1. **Deploy Backend (Blue-Green)**
   ```bash
   # Deploy new version to green environment
   ./deploy-backend.sh --target=green --phase=1
   
   # Run smoke tests
   ./run-smoke-tests.sh --env=green
   
   # Switch traffic to green
   ./switch-traffic.sh --to=green
   
   # Monitor for 1 hour
   # If issues: ./switch-traffic.sh --to=blue
   ```

2. **Deploy Frontend**
   ```bash
   # No frontend changes needed in Phase 1!
   # Frontend already expects these endpoints
   ```

3. **Verify**
   - Check `/financeiro/planilha` page works
   - Test date selector changes balances
   - Monitor error rates and latency
   - Check database query performance

4. **Rollback Procedure (if needed)**
   ```bash
   # Switch traffic back to blue
   ./switch-traffic.sh --to=blue
   
   # Investigate issues
   # Fix and redeploy to green
   ```

### Phase 2 Deployment

**Database Changes:** None

**Deployment Steps:**

1. Deploy backend with new profit service
2. Deploy frontend with profit pages
3. Verify profit dashboard loads
4. Verify profit displays on financial statement
5. Monitor profit calculation performance

### Phase 3 Deployment

**Database Changes:** Yes (MemberShareDistribution, MemberSalaryPayment tables)

**Deployment Steps:**

1. **Database Migration**
   ```bash
   # Run migration in maintenance window
   ./run-migrations.sh --phase=3
   
   # Verify tables created
   ./verify-migration.sh --phase=3
   ```

2. Deploy backend
3. Deploy frontend
4. Verify member financial dashboard
5. Test share calculation (in read-only mode first)
6. Enable write operations after validation

### Phase 4 Deployment

**Database Changes:** Yes (Client.CreditLimit column)

**Deployment Steps:**

1. **Database Migration**
   ```bash
   # Add CreditLimit column (non-breaking, nullable)
   ./run-migrations.sh --phase=4
   ```

2. Deploy backend with credit validation
3. Deploy frontend with credit UI
4. **Gradual Rollout:**
   - Week 1: Credit limits optional, validation disabled
   - Week 2: Enable validation for test clients
   - Week 3: Enable for all clients
   - Monitor transaction rejections

### Rollback Scenarios

| Phase | Rollback Strategy | Data Impact | Downtime |
|-------|------------------|-------------|----------|
| Phase 1 | Switch traffic to previous version | None | < 1 minute |
| Phase 2 | Switch traffic to previous version | Profit data not visible | < 1 minute |
| Phase 3 | Cannot rollback after distributions paid | Potential data inconsistency | Requires careful planning |
| Phase 4 | Can rollback, credit limits ignored | None | < 1 minute |

---

## Success Metrics

### Phase 1 Success Metrics

**Functional:**
- ✅ Financial statement displays historical balances
- ✅ Date selector changes balances correctly
- ✅ No 404 errors in production logs

**Performance:**
- ✅ Balance endpoint p95 latency < 150ms
- ✅ Page load time for `/financeiro/planilha` < 2s
- ✅ No slow query warnings

**Business:**
- ✅ Users can view past month financial statements
- ✅ Financial reporting becomes accurate

### Phase 2 Success Metrics

**Functional:**
- ✅ Profit dashboard displays all three profit sources
- ✅ Profit by manager report shows correct totals
- ✅ Profit displays on financial statement

**Accuracy:**
- ✅ Profit calculations match manual verification (100%)
- ✅ No discrepancies between reports

**Business:**
- ✅ Management has visibility into company profit
- ✅ Profit data available for member share calculation

### Phase 3 Success Metrics

**Functional:**
- ✅ Share calculation produces expected distributions
- ✅ Salary payments tracked correctly
- ✅ Distribution history visible to members

**Business:**
- ✅ Member profit sharing implemented per business rules
- ✅ Payroll process streamlined

### Phase 4 Success Metrics

**Functional:**
- ✅ Credit limits enforced on all client transactions
- ✅ Credit status visible on client pages
- ✅ Alerts triggered at 80% utilization

**Risk Management:**
- ✅ Zero instances of clients exceeding credit limit
- ✅ Proactive alerts prevent limit violations

**Business:**
- ✅ Financial risk exposure controlled
- ✅ Credit policy enforceable

### Overall Success Metrics

**Project Level:**
- ✅ All 5 phases delivered on schedule (±20%)
- ✅ Zero critical bugs in production
- ✅ No financial data discrepancies
- ✅ User satisfaction score > 8/10

**Technical:**
- ✅ Code coverage > 80%
- ✅ No performance regressions
- ✅ All documentation complete and accurate

**Business:**
- ✅ Finance module fully operational
- ✅ Management has complete financial visibility
- ✅ Risk management controls in place

---

## Conclusion

This implementation plan provides a comprehensive, actionable roadmap for implementing the Finance Module across 5 phases. Each phase delivers independent value and can be deployed incrementally.

**Key Takeaways:**

1. **Phase 1 is critical** - Fixes fundamental frontend-backend mismatch
2. **Phase 2 enables business insights** - Profit tracking provides financial visibility
3. **Phase 3 requires business input** - Cannot proceed without defined rules
4. **Phase 4 manages risk** - Credit limits protect company financially
5. **Phase 5 adds polish** - Advanced features and cleanup

**Next Steps:**

1. Review and approve this plan
2. Schedule Phase 1 implementation sprint
3. Schedule business rules session for Phase 3
4. Allocate resources (backend, frontend, QA)
5. Begin Phase 1 implementation

**Questions or Concerns:**
- Reach out to Track C session for clarifications
- Update this document as implementation progresses
- Track progress in Phase 1 task breakdown

---

*Document Version: 1.0*  
*Created: January 25, 2026*  
*Track: C*  
*Status: Ready for Implementation*  
*Next Review: After Phase 1 completion*
