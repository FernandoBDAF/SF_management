# Finance Module Planning

> **Status:** Deferred Implementation  
> **Created:** January 24, 2026  
> **Purpose:** Document financial features for future implementation

---

## Table of Contents

- [Overview](#overview)
- [Company Profit Tracking](#company-profit-tracking)
  - [Spread Profit](#spread-profit)
  - [Rake Commission Profit](#rake-commission-profit)
  - [Combined Profit Calculation](#combined-profit-calculation)
- [Member Financial Features](#member-financial-features)
  - [Share Percentage](#share-percentage)
  - [Salary](#salary)
- [Client Credit Management](#client-credit-management)
- [Referral Commission Integration](#referral-commission-integration)
- [ManagerProfitType Refactoring](#managerprofittype-refactoring)
- [Settlement Zeroing Process](#settlement-zeroing-process)
- [Implementation Priority](#implementation-priority)
- [Technical Considerations](#technical-considerations)

---

## Overview

The Finance Module will handle:
1. **Company profit tracking** from spread and rake commission
2. **Member profit sharing** based on Share percentage
3. **Member salary** management
4. **Client credit limits** to control debt exposure
5. **Referral commission** integration with balance

These features exist conceptually or partially in the codebase but need proper implementation.

---

## Company Profit Tracking

### Spread Profit

**Source:** Price difference between buy and sell ConversionRates

**How It Works:**

```
Buy Transaction:  ConversionRate = 5.10 (client pays more per chip)
Sell Transaction: ConversionRate = 4.90 (client receives less per chip)
Spread: 0.20 BRL per chip
```

**Calculation:**

```
Per Transaction:
  If SALE (client buys):
    No direct profit record (profit embedded in ConversionRate)
  
  If PURCHASE (client sells):
    No direct profit record (profit embedded in ConversionRate)

Profit Realized When:
  Client's BRL debt is paid via RECEIPT
  Net effect: Company received more BRL than it will pay out
```

**Implementation Considerations:**
- Track "standard" buy/sell rates per PokerManager
- Calculate implied profit per transaction
- Generate profit reports by period

---

### Rake Commission Profit

**Source:** SettlementTransaction fields

**Formula:**

```
Company Profit = RakeAmount × ((RakeCommission - RakeBack) / 100)
```

**Example:**

```
SettlementTransaction:
├─ RakeAmount: 1000 chips
├─ RakeCommission: 50%
└─ RakeBack: 10%

Company Profit: 1000 × ((50 - 10) / 100) = 400 chips
```

**Implementation:**

```csharp
// Suggested helper method
public decimal CalculateCompanyProfit(SettlementTransaction tx)
{
    var effectiveCommission = tx.RakeCommission - (tx.RakeBack ?? 0);
    return tx.RakeAmount * (effectiveCommission / 100);
}
```

---

### Combined Profit Calculation

**Report Structure:**

```
Period: January 2026

Profit by Source:
├─ Spread (from transactions): 15,000 BRL
├─ Rake Commission (from settlements): 25,000 chips
└─ Total: 40,000 (converted to BRL at period-end rate)

Profit by PokerManager:
├─ Manager A: 20,000
├─ Manager B: 15,000
└─ Manager C: 5,000
```

---

## Member Financial Features

### Share Percentage

**Existing Field:** `Member.Share` (decimal, 0-100%)

**Business Rule:**
Members with a Share receive a percentage of company profits.

**Questions to Resolve:**
1. Share of what exactly? (Total profit? Manager-specific? Asset-type specific?)
2. When is share calculated? (Monthly? Per settlement?)
3. How is it paid? (Automatic credit? Manual payment?)

**Suggested Implementation:**

```csharp
public class MemberShareDistribution
{
    public Guid MemberId { get; set; }
    public decimal SharePercentage { get; set; }
    public decimal ProfitBase { get; set; }       // What profit it's based on
    public decimal ShareAmount { get; set; }       // Calculated share
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public bool IsPaid { get; set; }
}
```

**Workflow:**

```
1. Calculate company profit for period
2. For each Member with Share > 0:
   ShareAmount = TotalProfit × (Share / 100)
3. Create distribution records
4. Credit to Member's FiatAssets balance (or manual payment)
```

---

### Salary

**Existing Field:** `Member.Salary` (decimal)

**Business Rule:**
Members with a Salary receive fixed periodic payments.

**Questions to Resolve:**
1. Payment frequency? (Monthly? Bi-weekly?)
2. Automatic or manual payment?
3. Tracked in balance or separate payroll?

**Suggested Implementation:**

```csharp
public class MemberSalaryPayment
{
    public Guid MemberId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public Guid? TransactionId { get; set; }  // Link to FiatTransaction
    public PaymentStatus Status { get; set; }
}
```

---

## Client Credit Management

**Purpose:** Control how much debt (negative balance) a client can accumulate.

### Proposed Feature

```csharp
// Add to Client entity
public decimal? CreditLimit { get; set; }  // null = no limit

// Validation in TransferService
if (newBalance < -client.CreditLimit)
{
    throw new BusinessException(
        $"Transaction would exceed credit limit of {client.CreditLimit}",
        "CREDIT_LIMIT_EXCEEDED");
}
```

### Business Rules

| CreditLimit Value | Behavior |
|-------------------|----------|
| `null` | No limit (unlimited credit) |
| `0` | No credit allowed (prepaid only) |
| `5000` | Can owe up to 5000 BRL |

### Implementation Considerations

1. **Per-AssetType limits?** Or single BRL limit?
2. **Soft vs Hard limit?** Warning at 80%, block at 100%?
3. **Admin override?** Allow exceeding with approval?

---

## Referral Commission Integration

**Existing System:** `Referral` entity with `ParentCommission` percentage

**Current State:**
- Referrals are tracked
- Commission percentage exists
- NOT currently integrated with balance calculation

**Future Integration:**

```csharp
// In balance calculation for Client
foreach (var referral in activeReferrals)
{
    // Find settlements where this client was involved
    // Calculate referrer's commission
    // Credit to referrer's balance
}
```

**Questions to Resolve:**
1. When is commission credited? (Per settlement? Period end?)
2. Which balance? (Referrer's FiatAssets?)
3. How to handle multi-level referrals?

---

## ManagerProfitType Refactoring

**Current State:**

```csharp
public enum ManagerProfitType
{
    Spread = 0,
    RakeOverrideCommission = 1
}
```

**Issues:**
- Profit calculation logic is scattered
- No unified profit tracking
- Manager vs Company profit is confusing

**Proposed Refactoring:**

1. **Clarify naming:**
   - `ManagerProfitType` → `CompanyRevenueSource` (company earns, not manager)

2. **Add profit tracking:**
   ```csharp
   public class ProfitRecord
   {
       public Guid PokerManagerId { get; set; }
       public CompanyRevenueSource Source { get; set; }
       public decimal Amount { get; set; }
       public Guid? SourceTransactionId { get; set; }
       public DateTime RecordedAt { get; set; }
   }
   ```

3. **Unified calculation service:**
   ```csharp
   public interface IProfitCalculationService
   {
       decimal CalculateSpreadProfit(DigitalAssetTransaction tx);
       decimal CalculateRakeProfit(SettlementTransaction tx);
       Task<ProfitSummary> GetProfitSummary(DateRange period, Guid? managerId);
   }
   ```

---

## Settlement Zeroing Process

**Current State:** Not implemented

**Concept:**
Periodically zero out client positions and convert running balances to fixed debt/credit.

**Questions to Resolve:**
1. Is this needed?
2. What's the business case?
3. How often? (Weekly? Monthly?)

**If Implemented:**

```
Process:
1. For each client with PokerAssets balance:
   a. Create settlement transaction to zero PokerAssets
   b. Convert to FiatAssets debt at current rate
2. Client now owes/is owed BRL instead of chips
```

**Note:** This may not be necessary if current balance tracking is sufficient.

---

## Implementation Priority

### Phase 1: Essential (Before Balance Refactoring)

| Feature | Effort | Impact |
|---------|--------|--------|
| Fix SettlementTransaction balance bug | Low | High |
| Document company profit formula | Low | High |

### Phase 2: High Priority

| Feature | Effort | Impact |
|---------|--------|--------|
| Client credit limit | Medium | High |
| Rake commission profit tracking | Medium | High |
| Spread profit tracking | Medium | Medium |

### Phase 3: Medium Priority

| Feature | Effort | Impact |
|---------|--------|--------|
| Member Share calculation | High | Medium |
| Member Salary management | Medium | Medium |
| Profit reporting | High | Medium |

### Phase 4: Lower Priority

| Feature | Effort | Impact |
|---------|--------|--------|
| Referral commission integration | High | Medium |
| ManagerProfitType refactoring | Medium | Low |
| Settlement zeroing | High | Low (if needed) |

---

## Technical Considerations

### Database Changes

```sql
-- Client credit limit
ALTER TABLE Client ADD CreditLimit DECIMAL(18,2) NULL;

-- Member share distributions
CREATE TABLE MemberShareDistribution (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    MemberId UNIQUEIDENTIFIER NOT NULL,
    SharePercentage DECIMAL(7,4) NOT NULL,
    ProfitBase DECIMAL(18,2) NOT NULL,
    ShareAmount DECIMAL(18,2) NOT NULL,
    PeriodStart DATETIME2 NOT NULL,
    PeriodEnd DATETIME2 NOT NULL,
    IsPaid BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL,
    -- FK to Member
);

-- Profit records (optional - could use view instead)
CREATE TABLE ProfitRecord (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    PokerManagerId UNIQUEIDENTIFIER NOT NULL,
    Source INT NOT NULL, -- CompanyRevenueSource enum
    Amount DECIMAL(18,2) NOT NULL,
    SourceTransactionId UNIQUEIDENTIFIER NULL,
    RecordedAt DATETIME2 NOT NULL,
    -- FK to PokerManager
);
```

### API Endpoints (Suggested)

```
# Profit
GET /api/v1/finance/profit/summary?startDate=&endDate=&managerId=
GET /api/v1/finance/profit/by-manager?startDate=&endDate=
GET /api/v1/finance/profit/by-source?startDate=&endDate=

# Member Share
GET /api/v1/member/{id}/share-distributions
POST /api/v1/member/{id}/calculate-share?periodStart=&periodEnd=
POST /api/v1/member/{id}/pay-share/{distributionId}

# Credit Limit
PUT /api/v1/client/{id}/credit-limit
GET /api/v1/client/{id}/credit-status
```

---

## Related Documentation

| Topic | Document |
|-------|----------|
| Entity Behaviors | [ENTITY_BUSINESS_BEHAVIOR.md](../03_CORE_SYSTEMS/ENTITY_BUSINESS_BEHAVIOR.md) |
| Balance Impact | [TRANSACTION_BALANCE_IMPACT.md](../03_CORE_SYSTEMS/TRANSACTION_BALANCE_IMPACT.md) |
| Settlement Workflow | [SETTLEMENT_WORKFLOW.md](../03_CORE_SYSTEMS/SETTLEMENT_WORKFLOW.md) |
| Balance Analysis | [BALANCE_SYSTEM_ANALYSIS.md](./BALANCE_SYSTEM_ANALYSIS.md) |
| Referral System | [REFERRAL_SYSTEM.md](../03_CORE_SYSTEMS/REFERRAL_SYSTEM.md) |

---

*Last updated: January 24, 2026*
