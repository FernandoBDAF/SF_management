# Finance Module - Deferred Features

> **Status:** Deferred  
> **Created:** January 27, 2026  
> **Last Updated:** January 27, 2026  
> **Purpose:** Document planned finance features for future implementation  
> **Revisit When:** After Phase 1, 2 & 3 are complete and stable

---

## Overview

This document captures finance features that were identified during planning but are **deferred** for future implementation. These features are valuable but not required for the initial usable version of the Finance Module.

### Related Documentation

| Document | Purpose |
|----------|---------|
| [FINANCE_MODULE_VISION.md](../10_REFACTORING/FINANCE_MODULE_VISION.md) | Overall vision and design |
| [FINANCE_MODULE_IMPLEMENTATION_PLAN_BACKEND.md](../10_REFACTORING/FINANCE_MODULE_IMPLEMENTATION_PLAN_BACKEND.md) | Backend implementation |
| Frontend Implementation Plan | `SF_management-front/documentation/06_DEVELOPMENT/FINANCE_MODULE_IMPLEMENTATION_PLAN_FRONTEND.md` |

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

```
src/app/(dashboard)/membros/[id]/financial/page.tsx
src/features/members/components/ShareDistributionHistory.tsx
src/features/members/components/SalaryPaymentHistory.tsx
src/features/members/components/MemberFinancialDashboard.tsx
```

### Dependencies

- **Requires:** ProfitCalculationService (Phase 2)
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

```
src/features/clients/components/ClientCreditStatus.tsx
src/features/clients/components/CreditLimitInput.tsx
```

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

```
src/features/finance/components/ExportButton.tsx
src/features/finance/components/ReportScheduler.tsx
```

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
    Spread = 0,
    RakeOverrideCommission = 1
}
```

### Proposed Changes

1. **Rename:** `ManagerProfitType` → `CompanyRevenueSource`
2. **Clarify:** This tracks how the COMPANY earns, not the manager
3. **Consolidate:** Scattered profit logic into `ProfitCalculationService`

### Dependencies

- **Requires:** Phase 3 (Profit Calculation Service) complete

### Estimated Effort

~4 hours

---

## Implementation Priority (When Revisiting)

| Feature | Priority | Dependencies |
|---------|----------|--------------|
| Client Credit Management | High | None |
| Member Financial Module | Medium | ProfitCalculationService (Phase 3), Business Rules |
| Referral Commission | Medium | Business Rules |
| Advanced Reporting | Lower | All features stable |
| ManagerProfitType Refactor | Lower | Phase 3 complete |

---

---

*Document Version: 1.1*  
*Last Updated: January 27, 2026*  
*Status: Deferred - Will revisit after Phase 1, 2 & 3 complete*
