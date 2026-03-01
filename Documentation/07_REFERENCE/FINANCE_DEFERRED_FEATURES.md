# Finance Module - Deferred & Implemented Features

> **Status:** Partially Implemented  
> **Created:** January 27, 2026  
> **Last Updated:** February 27, 2026  
> **Purpose:** Track planned finance features — both implemented and still deferred  
> **Revisit When:** After remaining deferred features are prioritized

---

## Overview

This document tracks finance features that were identified during planning. Some were originally deferred but have since been **implemented** (February 2026). The remaining features are **still deferred** for future implementation.

### Related Documentation

| Document | Purpose |
|----------|---------|
| [FINANCE_MODULE_VISION.md](../10_REFACTORING/FINANCE_MODULE_VISION.md) | Overall vision and design |
| [FINANCE_MODULE_IMPLEMENTATION_PLAN_BACKEND.md](../10_REFACTORING/FINANCE_MODULE_IMPLEMENTATION_PLAN_BACKEND.md) | Backend implementation |
| Frontend Implementation Plan | Documented independently in the frontend project |

---

## Implemented Features (Formerly Deferred)

The following features were originally deferred but were **implemented in February 2026** as part of the Finance Module buildout.

### Profit Detail Endpoints

**Status:** ✅ Implemented (February 2026)

The following detail endpoints were implemented in `ProfitCalculationService` and exposed via `ProfitController`:

| Endpoint | Purpose |
|----------|---------|
| `GET /api/v1/profit/direct-income-details` | Itemized direct income transactions |
| `GET /api/v1/profit/rate-fee-details` | Itemized rate fee calculations |
| `GET /api/v1/profit/rake-commission-details` | Itemized rake commission calculations |
| `GET /api/v1/profit/spread-details` | Itemized spread profit calculations |

These endpoints support the `/financeiro/planilha` page and provide drill-down from summary totals to individual transaction-level detail.

### AvgRate Endpoint

**Status:** ✅ Implemented (February 2026)

The `AvgRateService` and corresponding endpoint were implemented to provide weighted average rate (Cotação) calculations for poker managers. The service supports:
- Point-in-time rate lookups
- Monthly snapshots with caching (24h TTL for past months)
- Cache invalidation on transaction changes

### DirectIncome Exclusion from Per-Manager Breakdown

**Status:** ✅ Implemented (February 2026)

`GetProfitByManager` now excludes DirectIncome from the per-manager profit breakdown. DirectIncome is a company-level revenue source (system wallet transactions) and is not attributable to any specific poker manager. It is still included in the overall `GetProfitSummary` totals.

---

## Still Deferred Features

---

## Deferred Feature 1: Member Financial Module

### Description

Members can have profit-sharing agreements (Share %) and fixed salaries. This feature would:
- Calculate member share of company profits
- Track salary payments
- Provide member financial dashboards

### Existing Infrastructure

**Database Fields (already exist):**
```csharp
// Member entity
public decimal Share { get; set; }   // Percentage of profit (0-100)
public decimal Salary { get; set; }  // Fixed periodic payment
```

### Planned Implementation

#### 1. Share Distribution

**Business Questions to Resolve:**
1. Share of what? (Total profit? Rake only? Specific managers?)
2. When calculated? (Monthly? Quarterly? On-demand?)
3. How paid? (Automatic credit to balance? Manual approval?)

**Suggested Entity:**
```csharp
public class MemberShareDistribution
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public decimal SharePercentage { get; set; }
    public decimal ProfitBase { get; set; }       // What profit it's based on
    public decimal ShareAmount { get; set; }       // Calculated share
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public bool IsPaid { get; set; }
    public Guid? PaymentTransactionId { get; set; }
}
```

**Suggested Workflow:**
```
1. Calculate company profit for period (uses ProfitCalculationService)
2. For each Member with Share > 0:
   ShareAmount = TotalProfit × (Share / 100)
3. Create distribution records
4. Credit to Member's FiatAssets balance (or manual payment)
```

#### 2. Salary Management

**Business Questions to Resolve:**
1. Payment frequency? (Monthly? Bi-weekly?)
2. Automatic or manual payment?
3. Tracked in balance or separate payroll?

**Suggested Entity:**
```csharp
public class MemberSalaryPayment
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public Guid? TransactionId { get; set; }  // Link to FiatTransaction
    public PaymentStatus Status { get; set; } // Pending, Paid, Cancelled
}
```

### API Endpoints (Planned)

```
GET  /api/v1/member/{id}/share-distributions
POST /api/v1/member/{id}/calculate-share?periodStart=&periodEnd=
POST /api/v1/member/{id}/pay-share/{distributionId}
GET  /api/v1/member/{id}/salary-payments
POST /api/v1/member/{id}/pay-salary
```

### Frontend Components (Planned)

Frontend components for member financial views will be documented in the frontend project.

### Dependencies

- **Requires:** ~~ProfitCalculationService (Phase 2)~~ — now available
- **Requires:** Business rules definition session with Product Owner

### Estimated Effort

~25 hours (after business rules defined)

---

## Deferred Feature 2: Client Credit Management

### Description

Control how much debt (negative balance) a client can accumulate. This is a risk management feature to prevent excessive client debt exposure.

### Planned Implementation

#### 1. Database Changes

```sql
ALTER TABLE Clients ADD CreditLimit DECIMAL(18,2) NULL;
```

**Behavior:**
| CreditLimit Value | Behavior |
|-------------------|----------|
| `NULL` | No limit (unlimited credit) |
| `0` | No credit allowed (prepaid only) |
| `5000` | Can owe up to 5000 BRL |

#### 2. Credit Service

```csharp
public interface IClientCreditService
{
    Task ValidateCreditLimit(Guid clientId, decimal proposedBalance);
    Task<ClientCreditStatus> GetCreditStatus(Guid clientId);
    Task<bool> UpdateCreditLimit(Guid clientId, decimal? newLimit);
}

public class ClientCreditStatus
{
    public Guid ClientId { get; set; }
    public string ClientName { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal? CreditLimit { get; set; }
    public decimal? AvailableCredit { get; set; }
    public decimal? UtilizationPercentage { get; set; }
}
```

#### 3. Transaction Validation Integration

```csharp
// In TransferService, before creating transactions that debit clients:
if (newBalance < -client.CreditLimit)
{
    throw new BusinessException(
        $"Transaction would exceed credit limit of {client.CreditLimit}",
        "CREDIT_LIMIT_EXCEEDED");
}
```

### API Endpoints (Planned)

```
GET /api/v1/client/{id}/credit-status
PUT /api/v1/client/{id}/credit-limit
```

### Frontend Components (Planned)

Frontend components for client credit management will be documented in the frontend project.

### Business Questions

1. Should credit limit be per-client or have default tiers?
2. What happens when limit exceeded? (Hard block? Soft warning?)
3. Should there be admin override capability?
4. Alert thresholds? (e.g., warning at 80% utilization)

### Dependencies

- **Independent** - Can be implemented anytime

### Estimated Effort

~12 hours

---

## Deferred Feature 3: Referral Commission Integration

### Description

Integrate the existing referral system with balance calculations so referrers automatically receive commission credits.

### Existing Infrastructure

**Referral Entity:**
```csharp
public class Referral
{
    public Guid ReferrerId { get; set; }      // Client who referred
    public Guid ReferredId { get; set; }      // Client who was referred
    public decimal ParentCommission { get; set; }  // Commission percentage
}
```

### Planned Integration

```csharp
// In SettlementTransaction processing:
foreach (var referral in activeReferrals)
{
    // Calculate referrer's commission from settlement rake
    var commission = settlementRake * (referral.ParentCommission / 100);
    // Credit to referrer's FiatAssets balance
}
```

### Business Questions

1. When is commission credited? (Per settlement? Period end?)
2. Which balance? (Referrer's FiatAssets?)
3. How to handle multi-level referrals?

### Dependencies

- **Requires:** Clear business rules definition

### Estimated Effort

~6 hours (after rules defined)

---

## Deferred Feature 4: Advanced Reporting & Export

### Description

Generate financial reports in various formats for external use.

### Planned Features

1. **PDF Export** - Formatted financial statements
2. **Excel Export** - Detailed transaction data
3. **Scheduled Reports** - Automatic generation and email delivery

### Frontend Components (Planned)

Frontend components for reporting and export will be documented in the frontend project.

### Dependencies

- **Requires:** All other finance features stable

### Estimated Effort

~15 hours

---

## Deferred Feature 5: ManagerProfitType Refactoring

### Description

Clean up and clarify the `ManagerProfitType` enum and related logic.

### Current State

```csharp
public enum ManagerProfitType
{
    None = 0,
    Spread = 1,
    RakeOverrideCommission = 2
}
```

### Proposed Changes

1. **Rename:** `ManagerProfitType` → `CompanyRevenueSource`
2. **Clarify:** This tracks how the COMPANY earns, not the manager
3. **Consolidate:** Scattered profit logic into `ProfitCalculationService`

### Dependencies

- **Requires:** ~~Phase 3 (Profit Calculation Service) complete~~ — now available

### Estimated Effort

~4 hours

---

## Implementation Priority (When Revisiting Deferred Features)

| Feature | Priority | Dependencies | Status |
|---------|----------|--------------|--------|
| Client Credit Management | High | None | Still Deferred |
| Member Financial Module | Medium | ProfitCalculationService (**now available**), Business Rules | Still Deferred |
| Referral Commission | Medium | Business Rules | Still Deferred |
| Advanced Reporting | Lower | All features stable | Still Deferred |
| ManagerProfitType Refactor | Lower | ProfitCalculationService (**now available**) | Still Deferred |

> **Note:** ProfitCalculationService and AvgRateService are now implemented, unblocking Member Financial Module and ManagerProfitType Refactor from a technical dependency perspective. Business rules definition is still required for Member Financial Module.

---

*Document Version: 2.0*  
*Last Updated: February 27, 2026*  
*Status: Partially Implemented — see Implemented Features section above*
