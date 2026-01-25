# Finance Module Current State & Roadmap

> **Status:** Planning  
> **Created:** January 24, 2026  
> **Track:** C  
> **Purpose:** Document current finance (financeiro) implementation; create comprehensive refactor roadmap

---

## Table of Contents

- [Executive Summary](#executive-summary)
- [Business Context](#business-context)
- [Current Implementation](#current-implementation)
- [Critical Gaps](#critical-gaps)
- [Backend Infrastructure](#backend-infrastructure)
- [Frontend Implementation](#frontend-implementation)
- [Roadmap](#roadmap)

---

## Executive Summary

**Current State:**
- Basic category system implemented
- System operation flag (`isSystemOperation`) replaces legacy `financeiro`
- Settlement rake tracking exists (RakeCommission, RakeBack)
- **BUT:** No profit tracking, no Member share calculation, no Client credit limits

**Key Insight:** Frontend has `/financeiro/planilha` (financial statement/spreadsheet) with most UX for first version. Backend lacks corresponding endpoints.

**Critical Missing:**
1. Date-filtered balance endpoints (frontend expects them)
2. Company profit tracking endpoints
3. Member share calculation service
4. Client credit limit system

**Priority Actions:**
1. Implement date-filtered balance API
2. Create profit tracking service
3. Design Member financial module
4. Design Client credit system

---

## Business Context

### What "Finance" Means in SF Management

The Finance Module tracks **company financial performance** across:

1. **Revenue Sources:**
   - Spread profit (difference in buy/sell ConversionRate)
   - Rate fees (when transactions use Rate field)
   - Rake commission (SettlementTransaction: RakeAmount × RakeCommission%)

2. **Expense/Revenue Classification:**
   - System Operations (via `isSystemOperation` flag)
   - Categorized transactions (Category entity)

3. **Partner Financials:**
   - Member Share % (profit distribution)
   - Member Salary (fixed payments)
   - Client Credit Limits (risk management)

4. **Financial Reporting:**
   - Monthly balance statements
   - Profit by manager
   - Profit by source (spread, rate, rake)
   - Asset/liability summary

---

## Current Implementation

### Backend: What Exists

#### 1. Category System ✅

**Entity:** `Domain/Entities/Support/Category.cs`

```csharp
public class Category : BaseDomain
{
    public string Name { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
}
```

**Endpoints:** `/api/v1/category` (full CRUD)

**Service:** `Application/Services/Support/CategoryService.cs`

**Usage:**
- Optional on all transactions
- Required when `isSystemOperation = true`
- Hierarchical structure (parent/children)

**Documentation:** `Documentation/04_SUPPORTING_SYSTEMS/CATEGORY_SYSTEM.md`

---

#### 2. System Operation Flag ✅

**Fields:**
- Legacy: `financeiro` (bool) - Deprecated
- Current: Mapped to `isSystemOperation` in frontend

**Usage:**
- Marks transactions as company internal operations
- When enabled, `CategoryId` is required
- Used for financial reporting

**Status:** Partially migrated; some forms still reference old `financeiro`

---

#### 3. Settlement Rake Tracking ✅

**Entity:** `Domain/Entities/Transactions/SettlementTransaction.cs`

```csharp
public class SettlementTransaction : BaseTransaction
{
    public decimal RakeAmount { get; set; }         // Rake paid by client
    public decimal RakeCommission { get; set; }     // % company earns
    public decimal? RakeBack { get; set; }          // % returned to client
}
```

**Balance Impact (corrected in Phase 4):**
- PokerManager: `-RakeAmount × (RakeCommission / 100)` → BRL
- Client: `+RakeAmount × (RakeBack / 100)` → BRL

**Company Profit Formula:**
```
Profit = RakeAmount × ((RakeCommission - RakeBack) / 100)
```

**Documentation:** `Documentation/03_CORE_SYSTEMS/SETTLEMENT_WORKFLOW.md`

---

#### 4. Manager Profit Type ⚠️ (Needs Refactoring)

**Enum:** `Domain/Enums/ManagerProfitType.cs`

```csharp
public enum ManagerProfitType
{
    Spread = 0,                    // Profit from buy/sell difference
    RakeOverrideCommission = 1     // Profit from rake commission
}
```

**Current Issues:**
- Logic scattered across codebase
- Not integrated with profit tracking
- Needs renaming to `CompanyRevenueSource`

**Documented in:** `Documentation/10_REFACTORING/FINANCE_MODULE_PLANNING.md`

---

#### 5. Balance Calculation ✅ (with recent fixes)

**Service:** `Application/Services/Base/BaseAssetHolderService.cs`

**Methods:**
- `GetBalancesByAssetType(Guid)` - For Clients, Members, Banks
- `GetBalancesByAssetGroup(Guid)` - For PokerManagers

**Recent Fixes (Phase 4):**
- SettlementTransaction now uses RakeAmount/RakeCommission/RakeBack (NOT AssetAmount)
- PokerManager FiatAssets classified as LIABILITY (not ASSET)

**Documentation:** `Documentation/06_API/BALANCE_ENDPOINTS.md`

---

### Backend: What's MISSING

#### 1. Date-Filtered Balance Endpoints ❌

**Frontend Expects:**
```
POST /api/v1/bank/balance/{id}
Body: { date: "2026-01-15" }
Response: Dictionary<AssetType, decimal>
```

**Backend Has:**
```
GET /api/v1/bank/{id}/balance
Response: Dictionary<AssetType, decimal>  (current balance only)
```

**Impact:** `/financeiro/planilha` cannot show historical balances

**Action:** Implement date-filtered balance endpoint or add `?date=` query parameter

---

#### 2. Profit Tracking Endpoints ❌

**Missing:**
- `GET /api/v1/finance/profit/summary?startDate=&endDate=&managerId=`
- `GET /api/v1/finance/profit/by-manager?startDate=&endDate=`
- `GET /api/v1/finance/profit/by-source?startDate=&endDate=`

**Current Workaround:** None - profit calculated manually

---

#### 3. Member Financial Features ❌

**Missing:**
- `GET /api/v1/member/{id}/share-distributions`
- `POST /api/v1/member/{id}/calculate-share?periodStart=&periodEnd=`
- `POST /api/v1/member/{id}/pay-salary`

**Fields Exist But Unused:**
- `Member.Share` (decimal, % of profit)
- `Member.Salary` (decimal, fixed payment)

---

#### 4. Client Credit System ❌

**Missing:**
- `PUT /api/v1/client/{id}/credit-limit`
- `GET /api/v1/client/{id}/credit-status`
- Validation in `TransferService` to enforce credit limit

**Documented in:** `Documentation/10_REFACTORING/BALANCE_SYSTEM_ANALYSIS.md`

---

### Frontend: What Exists

#### 1. Financial Statement Page ✅

**Route:** `/financeiro/planilha`

**Location:** `src/app/(dashboard)/financeiro/planilha/`

**Features:**
- Consolidated view (Assets vs Liabilities)
- Banks table with balances
- Clients balance summary
- PokerManagers table
- **Date filtering** (expects backend support)

**Components:**
- `page.tsx` - Main page
- `ConsolidatedView.tsx` - Assets/Liabilities summary
- `BanksTableServer.tsx` - Banks table
- `ClientsBalanceServer.tsx` - Clients summary
- `ManagersTableClient.tsx` - PokerManagers table

**Current Limitations:**
- Date filtering doesn't work (backend doesn't support)
- No profit display
- No Member share information
- Manual data aggregation

---

#### 2. System Operation Check ✅

**Component:** `src/features/transactions/components/FormFields/SystemOperationCheck.tsx`

**Features:**
- Checkbox to mark transaction as system operation
- Type selection: `'despesa'` (expense) or `'receita'` (revenue)
- Category becomes required when enabled

**Props:**
```typescript
interface SystemOperationCheckProps {
  isSystemOperation: boolean;
  systemOperationType: 'despesa' | 'receita' | null;
  categoryId: string | null;
  onIsSystemOperationChange: (value: boolean) => void;
  onSystemOperationTypeChange: (value: 'despesa' | 'receita' | null) => void;
  onCategoryChange: (value: string | null) => void;
}
```

**Status:** Implemented and integrated into `AssetTransactionForm`

---

#### 3. Category Management ✅

**Service:** `src/features/categories/api/category.service.ts`

**Features:**
- Fetch hierarchical categories
- Create/update/delete categories
- Used in transaction forms

**Status:** Working

---

### Frontend: What's MISSING

#### 1. Profit Display ❌

- No profit metrics on `/financeiro/planilha`
- No profit breakdown by source
- No profit by manager report

---

#### 2. Member Financial Dashboard ❌

- Share % displayed but not used
- No share calculation UI
- No salary payment tracking
- No distribution history

---

#### 3. Client Credit Management ❌

- No credit limit field in client forms
- No credit status indicator
- No alerts for approaching limit

---

## Critical Gaps

### Gap 1: Date-Filtered Balance

**Impact:** High - Blocks historical financial statements

**Frontend Code:**
```typescript
// SF_management-front/src/app/(dashboard)/financeiro/planilha/page.tsx
// Expects date parameter but backend doesn't support
const balances = await Promise.all(
  banks.map(bank => 
    bankService.getBalance(bank.id, selectedDate)  // ❌ Date not supported
  )
);
```

**Backend Current:**
```csharp
[HttpGet("{id}/balance")]
public async Task<IActionResult> GetBalance(Guid id)
{
    // Returns current balance only, no date filtering
}
```

**Solution Options:**

**Option A:** Query parameter
```csharp
[HttpGet("{id}/balance")]
public async Task<IActionResult> GetBalance(Guid id, [FromQuery] DateTime? asOfDate = null)
```

**Option B:** New endpoint
```csharp
[HttpGet("{id}/balance/at-date")]
public async Task<IActionResult> GetBalanceAtDate(Guid id, [FromQuery] DateTime date)
```

**Recommendation:** Option A (simpler, backward compatible)

---

### Gap 2: Profit Tracking

**Impact:** High - Core finance feature missing

**Needed:**
1. **Service:** `ProfitCalculationService.cs`
   - Calculate spread profit from DigitalAssetTransactions
   - Calculate rate profit from DigitalAssetTransactions
   - Calculate rake profit from SettlementTransactions
   - Aggregate by time period, manager, source

2. **Endpoints:**
   ```
   GET /api/v1/finance/profit/summary?startDate=&endDate=&managerId=
   GET /api/v1/finance/profit/by-manager?startDate=&endDate=
   GET /api/v1/finance/profit/by-source?startDate=&endDate=
   ```

3. **Response DTOs:**
   - `ProfitSummaryResponse`
   - `ProfitByManagerResponse`
   - `ProfitBySourceResponse`

**Formulas Documented:**
- Spread: Tracked per transaction, needs aggregation service
- Rake: `RakeAmount × ((RakeCommission - RakeBack) / 100)`
- Rate: `AssetAmount × (Rate / 100)`

**Reference:** `Documentation/10_REFACTORING/FINANCE_MODULE_PLANNING.md`

---

### Gap 3: Member Financial Module

**Impact:** Medium - Fields exist but unused

**Needed:**
1. **Share Calculation Service**
   - Calculate total company profit for period
   - Distribute by Member.Share %
   - Create distribution records

2. **Salary Payment Service**
   - Track periodic salary payments
   - Link to transactions
   - Payment history

3. **Business Rules to Define:**
   - Share of what? (total profit, rake only, spread only?)
   - When calculated? (monthly, quarterly, per transaction?)
   - How paid? (automatic transaction, manual?)

**Status:** Design needed before implementation

---

### Gap 4: Client Credit Management

**Impact:** Medium - Risk management feature

**Needed:**
1. **Add `CreditLimit` field to Client entity**
2. **Validation in TransferService:**
   ```csharp
   if (client.Balance < -client.CreditLimit)
   {
       throw new BusinessException("Credit limit exceeded");
   }
   ```

3. **Endpoints:**
   ```
   PUT /api/v1/client/{id}/credit-limit
   GET /api/v1/client/{id}/credit-status
   ```

**Business Rules:**
- Personalized per client (like credit analysis)
- Negative balance allowed up to limit
- Alerts when approaching limit

**Reference:** `Documentation/10_REFACTORING/BALANCE_SYSTEM_ANALYSIS.md`

---

## Roadmap

### Phase 1: Critical Infrastructure (Foundation)

**Goal:** Enable date-filtered balances and fix settlement display

| Task | Priority | Effort | Status |
|------|----------|--------|--------|
| Implement date-filtered balance endpoint | P0 | 2-4 hours | ⬜ Pending |
| Update balance service to support date parameter | P0 | 1-2 hours | ⬜ Pending |
| Test with `/financeiro/planilha` page | P0 | 1 hour | ⬜ Pending |
| Document new endpoint in BALANCE_ENDPOINTS.md | P0 | 30 min | ⬜ Pending |

**Dependencies:** None

**Deliverables:**
- Working date-filtered balance API
- Frontend can show historical statements

---

### Phase 2: Profit Tracking (Core Feature)

**Goal:** Implement company profit calculation and reporting

| Task | Priority | Effort | Status |
|------|----------|--------|--------|
| Create `ProfitCalculationService.cs` | P1 | 4-6 hours | ⬜ Pending |
| Implement spread profit aggregation | P1 | 2-3 hours | ⬜ Pending |
| Implement rate profit aggregation | P1 | 2-3 hours | ⬜ Pending |
| Implement rake profit aggregation | P1 | 2-3 hours | ⬜ Pending |
| Create profit endpoints | P1 | 2-3 hours | ⬜ Pending |
| Create profit DTOs | P1 | 1 hour | ⬜ Pending |
| Add profit display to `/financeiro/planilha` | P1 | 2-3 hours | ⬜ Pending |
| Update FINANCE_MODULE_PLANNING.md | P1 | 30 min | ⬜ Pending |

**Dependencies:** Phase 1

**Deliverables:**
- Working profit tracking
- Profit displayed on financial statement

---

### Phase 3: Member Financial Module (Design First)

**Goal:** Define and implement Member share and salary features

**Design Tasks:**

| Task | Priority | Effort | Status |
|------|----------|--------|--------|
| Define Share calculation business rules | P2 | Design session | ⬜ Pending |
| Define Salary payment workflow | P2 | Design session | ⬜ Pending |
| Design distribution record structure | P2 | Design session | ⬜ Pending |
| Create Member Financial Module design doc | P2 | 2-3 hours | ⬜ Pending |

**Implementation Tasks (After Design):**

| Task | Priority | Effort | Status |
|------|----------|--------|--------|
| Create `MemberFinancialService.cs` | P2 | 3-4 hours | ⬜ Pending |
| Implement share calculation | P2 | 2-3 hours | ⬜ Pending |
| Implement salary payment tracking | P2 | 2-3 hours | ⬜ Pending |
| Create Member financial endpoints | P2 | 2-3 hours | ⬜ Pending |
| Add Member financial dashboard (frontend) | P2 | 4-6 hours | ⬜ Pending |

**Dependencies:** Business rules definition

**Deliverables:**
- Member share calculation service
- Member salary tracking
- Member financial dashboard

---

### Phase 4: Client Credit Management

**Goal:** Implement credit limit system for risk management

| Task | Priority | Effort | Status |
|------|----------|--------|--------|
| Add `CreditLimit` field to Client entity | P2 | 30 min | ⬜ Pending |
| Create database migration | P2 | 30 min | ⬜ Pending |
| Add credit limit validation to TransferService | P2 | 1 hour | ⬜ Pending |
| Create credit limit endpoints | P2 | 1-2 hours | ⬜ Pending |
| Add credit limit field to Client forms (frontend) | P2 | 1 hour | ⬜ Pending |
| Add credit status indicator | P2 | 1-2 hours | ⬜ Pending |
| Add alerts for approaching limit | P2 | 1-2 hours | ⬜ Pending |

**Dependencies:** None

**Deliverables:**
- Credit limit enforcement
- Credit status visibility
- Risk alerts

---

### Phase 5: Advanced Features

**Goal:** Additional financial features and integrations

| Task | Priority | Effort | Status |
|------|----------|--------|--------|
| Referral commission integration | P3 | 4-6 hours | ⬜ Deferred |
| Settlement zeroing process | P3 | 3-4 hours | ⬜ Deferred |
| ManagerProfitType refactoring | P3 | 2-3 hours | ⬜ Deferred |
| Export financial reports (PDF, Excel) | P3 | 4-6 hours | ⬜ Deferred |

**Dependencies:** Phases 1-4

**Deliverables:**
- Complete financial reporting suite

---

## Backend Infrastructure

### Current Services

| Service | Location | Status |
|---------|----------|--------|
| CategoryService | `Application/Services/Support/CategoryService.cs` | ✅ Complete |
| BaseAssetHolderService | `Application/Services/Base/BaseAssetHolderService.cs` | ✅ Complete (with Phase 4 fixes) |
| SettlementTransactionService | `Application/Services/Transactions/SettlementTransactionService.cs` | ✅ Complete |

### Services to Create

| Service | Purpose | Priority |
|---------|---------|----------|
| **ProfitCalculationService** | Calculate company profit from transactions | P1 |
| **MemberFinancialService** | Share calculation, salary tracking | P2 |
| **ClientCreditService** | Credit limit management | P2 |
| **FinancialReportingService** | Generate financial reports | P3 |

---

## Frontend Implementation

### Current Pages

| Route | Purpose | Status | Issues |
|-------|---------|--------|--------|
| `/financeiro/planilha` | Financial statement/spreadsheet | ✅ Implemented | Date filtering broken, no profit display |
| `/category` (if exists) | Category management | ❓ Unknown | |

### Pages to Create/Update

| Route | Purpose | Priority |
|-------|---------|----------|
| `/financeiro/profit` | Profit dashboard | P1 |
| `/financeiro/profit/by-manager` | Profit by manager report | P1 |
| `/financeiro/profit/by-source` | Profit by source report | P1 |
| `/members/[id]/financial` | Member financial dashboard | P2 |
| `/clients/[id]/credit` | Client credit status | P2 |

---

## Implementation Priorities

### Must Have (Phase 1)
- Date-filtered balance API
- Working financial statement with historical data

### Should Have (Phase 2)
- Profit tracking and display
- Basic financial reporting

### Nice to Have (Phase 3-5)
- Member share/salary features
- Client credit management
- Advanced reporting

---

## Dependencies & Coordination

### Track A Dependencies
- Date-filtered balance endpoint design impacts Track A alignment doc
- Must document in API_REFERENCE.md after implementation

### Track B Dependencies
- Settlement balance fix (already done in Phase 4) ensures correct statement display
- Statement pages will consume date-filtered balance API

### Internal Dependencies
- Phase 2 (Profit) depends on Phase 1 (Date filtering)
- Phase 3 (Member) depends on Phase 2 (Profit calculation for share)
- Phase 4 (Credit) is independent

---

## Research Questions

### Phase 1 (Date-Filtered Balance)
1. Should we recalculate from transactions or store snapshots?
2. Performance implications for date-filtered queries?
3. Caching strategy for historical balances?

### Phase 2 (Profit Tracking)
1. Should profit be calculated real-time or cached/pre-aggregated?
2. Where to store profit records? New table or aggregate from transactions?
3. How to handle multi-currency profit (BRL, USD, chips)?

### Phase 3 (Member Financial)
1. Share of WHAT exactly? (total profit, rake only, specific sources?)
2. When is share calculated? (real-time, monthly, on-demand?)
3. How is share paid? (automatic transaction, manual approval?)
4. Is salary tracked as recurring payment or one-time transactions?

### Phase 4 (Client Credit)
1. Should credit limit be per-client or have default tiers?
2. What happens when limit exceeded? (hard block, soft warning?)
3. How to adjust limits? (admin UI, automatic review?)

---

## Related Documentation

### Backend (Must Read for Implementation)
- [BALANCE_SYSTEM_ANALYSIS.md](./BALANCE_SYSTEM_ANALYSIS.md) - Complete balance rules and bugs
- [FINANCE_MODULE_PLANNING.md](./FINANCE_MODULE_PLANNING.md) - Future features planning
- [SETTLEMENT_WORKFLOW.md](../03_CORE_SYSTEMS/SETTLEMENT_WORKFLOW.md) - Rake profit formulas
- [TRANSACTION_BALANCE_IMPACT.md](../03_CORE_SYSTEMS/TRANSACTION_BALANCE_IMPACT.md) - Transaction impact formulas
- [ENTITY_BUSINESS_BEHAVIOR.md](../03_CORE_SYSTEMS/ENTITY_BUSINESS_BEHAVIOR.md) - Entity behavior rules

### Frontend
- `SF_management-front/documentation/03_CORE_SYSTEMS/BALANCE_DISPLAY_USAGE.md`
- `SF_management-front/documentation/03_CORE_SYSTEMS/TRANSACTION_SYSTEM.md`

### API Reference
- [BALANCE_ENDPOINTS.md](../06_API/BALANCE_ENDPOINTS.md)
- [API_REFERENCE.md](../06_API/API_REFERENCE.md)

---

## Next Steps

1. **Immediate:** Survey `/financeiro/planilha` frontend code in detail
2. **Design:** Date-filtered balance endpoint specification
3. **Implement:** Phase 1 (date-filtered balances)
4. **Plan:** Phase 2 (profit tracking) detailed implementation
5. **Design Sessions:** Phase 3 (Member financial) business rules

---

*Last Updated: January 24, 2026*  
*Managed by: Track C Session*  
*Status: Awaiting session start*
