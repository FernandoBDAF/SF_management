# Contracts Implementation Plan

> **Status:** Planning  
> **Priority:** Medium  
> **Category:** Future Feature

---

## Table of Contents

1. [Overview](#overview)
2. [Business Context](#business-context)
3. [Entity Design](#entity-design)
4. [Contract Types](#contract-types)
5. [Execution Logic](#execution-logic)
6. [API Endpoints](#api-endpoints)
7. [Service Layer](#service-layer)
8. [UI Integration](#ui-integration)
9. [Implementation Phases](#implementation-phases)
10. [Related Documentation](#related-documentation)

---

## Overview

### Purpose

Contracts represent **recurring financial obligations** that the system will automatically track and, optionally, execute. This feature enables the financial system to:

- Schedule and track periodic payments (salaries, services, subscriptions)
- Generate automatic transactions based on contract terms
- Forecast cash flow based on upcoming obligations
- Maintain a history of contract executions

### Problem Statement

Currently, recurring payments require manual transaction creation each period. This leads to:
- Risk of missed payments
- No visibility into upcoming obligations
- Manual effort for routine financial operations
- Difficulty in cash flow forecasting

### Solution

A **Contract** entity that defines recurring financial relationships with:
- Automatic transaction generation capability
- Upcoming payments dashboard
- Execution history tracking
- Integration with existing Category and Transfer systems

---

## Business Context

### Use Cases

| Use Case | Description | Example |
|----------|-------------|---------|
| **Employee Salaries** | Monthly fixed payments to Members | Pay R$ 5,000/month to team member |
| **Service Subscriptions** | Recurring service fees | Monthly poker platform fees |
| **Rent/Utilities** | Periodic operational expenses | Monthly office rent |
| **Partner Agreements** | Revenue sharing or fixed payments | Quarterly partner distributions |
| **Settlement Schedules** | Periodic reconciliation with managers | Weekly PokerManager settlements |

### Integration with Existing Systems

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         CONTRACT SYSTEM                                   │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                           │
│   Contract Entity                                                         │
│   ├── Links to: Category (for transaction classification)                │
│   ├── Links to: BaseAssetHolder (payer and payee)                        │
│   ├── Links to: WalletIdentifier (specific wallets, optional)            │
│   └── Generates: Transactions via TransferService                         │
│                                                                           │
│   Execution Flow:                                                         │
│   Contract → Scheduler → TransferService → Transaction Created            │
│                                                                           │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## Entity Design

### Contract Entity

**Proposed File:** `Domain/Entities/Support/Contract.cs`

```csharp
public class Contract : BaseDomain
{
    // Contract identification
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    // Parties involved
    [Required]
    public Guid PayerAssetHolderId { get; set; }
    public virtual BaseAssetHolder PayerAssetHolder { get; set; } = null!;
    
    [Required]
    public Guid PayeeAssetHolderId { get; set; }
    public virtual BaseAssetHolder PayeeAssetHolder { get; set; } = null!;
    
    // Optional: specific wallets (if null, system finds appropriate wallet)
    public Guid? PayerWalletIdentifierId { get; set; }
    public virtual WalletIdentifier? PayerWalletIdentifier { get; set; }
    
    public Guid? PayeeWalletIdentifierId { get; set; }
    public virtual WalletIdentifier? PayeeWalletIdentifier { get; set; }
    
    // Financial terms
    [Required]
    [Precision(18, 2)]
    public decimal Amount { get; set; }
    
    [Required]
    public AssetType AssetType { get; set; }
    
    // Scheduling
    [Required]
    public ContractFrequency Frequency { get; set; }
    
    public int? CustomFrequencyDays { get; set; } // For Frequency.Custom
    
    [Required]
    public DateTime StartDate { get; set; }
    
    public DateTime? EndDate { get; set; } // Null = indefinite
    
    public int? ExecutionDay { get; set; } // Day of month/week for execution
    
    // Status
    [Required]
    public ContractStatus Status { get; set; } = ContractStatus.Active;
    
    // Classification
    public Guid? CategoryId { get; set; }
    public virtual Category? Category { get; set; }
    
    // Execution tracking
    public DateTime? LastExecutedAt { get; set; }
    public DateTime? NextExecutionDate { get; set; }
    public int ExecutionCount { get; set; } = 0;
    
    // Execution settings
    public bool AutoExecute { get; set; } = false; // If true, auto-generate transactions
    public bool RequiresApproval { get; set; } = true; // Generated transactions need approval
    
    // Navigation
    public virtual ICollection<ContractExecution> Executions { get; set; } = [];
}
```

### ContractExecution Entity

Tracks each execution (attempted or successful) of a contract.

**Proposed File:** `Domain/Entities/Support/ContractExecution.cs`

```csharp
public class ContractExecution : BaseDomain
{
    [Required]
    public Guid ContractId { get; set; }
    public virtual Contract Contract { get; set; } = null!;
    
    [Required]
    public DateTime ScheduledDate { get; set; }
    
    public DateTime? ExecutedAt { get; set; }
    
    [Required]
    public ContractExecutionStatus Status { get; set; }
    
    // Link to generated transaction (if successful)
    public Guid? TransactionId { get; set; }
    public TransactionType? TransactionType { get; set; } // Fiat or Digital
    
    // Execution details
    [Precision(18, 2)]
    public decimal? ExecutedAmount { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    [MaxLength(500)]
    public string? FailureReason { get; set; }
}
```

### Enumerations

**Proposed File:** `Domain/Enums/Contracts/ContractEnums.cs`

```csharp
public enum ContractFrequency
{
    Weekly = 1,
    BiWeekly = 2,
    Monthly = 3,
    Quarterly = 4,
    SemiAnnually = 5,
    Annually = 6,
    Custom = 99  // Uses CustomFrequencyDays
}

public enum ContractStatus
{
    Draft = 0,      // Not yet active
    Active = 1,     // Currently active
    Paused = 2,     // Temporarily suspended
    Completed = 3,  // End date reached
    Cancelled = 4   // Manually cancelled
}

public enum ContractExecutionStatus
{
    Pending = 0,    // Scheduled but not yet due
    Due = 1,        // Due for execution
    Executed = 2,   // Successfully executed
    Skipped = 3,    // Manually skipped
    Failed = 4      // Execution failed
}

public enum TransactionType
{
    Fiat = 1,
    Digital = 2
}
```

---

## Contract Types

### By Purpose

| Type | Payer | Payee | Typical Frequency | Category |
|------|-------|-------|-------------------|----------|
| **Salary** | Company (Bank) | Member | Monthly | Expense > Salaries |
| **Service Fee** | Company (Bank) | External | Monthly/Annually | Expense > Services |
| **Rent** | Company (Bank) | External | Monthly | Expense > Operations |
| **Settlement** | PokerManager | Client | Weekly | Settlement |
| **Commission** | Company | PokerManager | Monthly | Expense > Commissions |
| **Subscription** | Company (Bank) | External | Monthly | Expense > Subscriptions |

### By Asset Type

| Asset Type | Transaction Entity | Notes |
|------------|-------------------|-------|
| FiatAssets (BRL, USD) | `FiatAssetTransaction` | Most common for operational expenses |
| PokerAssets | `DigitalAssetTransaction` | For chip-based agreements |
| CryptoAssets | `DigitalAssetTransaction` | For crypto-based payments |

---

## Execution Logic

### Scheduler Design

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     CONTRACT EXECUTION FLOW                              │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  1. SCHEDULING                                                           │
│     ┌──────────────┐                                                     │
│     │ Daily Check  │──▶ Find contracts where NextExecutionDate ≤ today  │
│     └──────────────┘                                                     │
│            │                                                             │
│            ▼                                                             │
│  2. VALIDATION                                                           │
│     ┌──────────────┐                                                     │
│     │ For each:    │                                                     │
│     │ - Status=Active                                                    │
│     │ - StartDate ≤ today                                                │
│     │ - EndDate null OR > today                                          │
│     └──────────────┘                                                     │
│            │                                                             │
│            ▼                                                             │
│  3. EXECUTION (if AutoExecute=true)                                      │
│     ┌──────────────┐                                                     │
│     │ Call Transfer│                                                     │
│     │ Service with │──▶ Creates Transaction                              │
│     │ contract data│                                                     │
│     └──────────────┘                                                     │
│            │                                                             │
│            ▼                                                             │
│  4. RECORD KEEPING                                                       │
│     ┌──────────────┐                                                     │
│     │ Create       │                                                     │
│     │ Execution    │──▶ Link to Transaction, update NextExecutionDate    │
│     │ Record       │                                                     │
│     └──────────────┘                                                     │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### Next Execution Date Calculation

```csharp
public DateTime CalculateNextExecutionDate(Contract contract)
{
    var baseDate = contract.LastExecutedAt ?? contract.StartDate;
    
    return contract.Frequency switch
    {
        ContractFrequency.Weekly => baseDate.AddDays(7),
        ContractFrequency.BiWeekly => baseDate.AddDays(14),
        ContractFrequency.Monthly => GetNextMonthlyDate(baseDate, contract.ExecutionDay),
        ContractFrequency.Quarterly => baseDate.AddMonths(3),
        ContractFrequency.SemiAnnually => baseDate.AddMonths(6),
        ContractFrequency.Annually => baseDate.AddYears(1),
        ContractFrequency.Custom => baseDate.AddDays(contract.CustomFrequencyDays ?? 30),
        _ => baseDate.AddMonths(1)
    };
}

private DateTime GetNextMonthlyDate(DateTime baseDate, int? executionDay)
{
    var nextMonth = baseDate.AddMonths(1);
    var day = executionDay ?? baseDate.Day;
    var daysInMonth = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);
    day = Math.Min(day, daysInMonth); // Handle months with fewer days
    
    return new DateTime(nextMonth.Year, nextMonth.Month, day);
}
```

### Integration with TransferService

```csharp
public async Task<ContractExecution> ExecuteContract(Guid contractId)
{
    var contract = await GetContractWithDetails(contractId);
    
    // Build transfer request from contract
    var transferRequest = new TransferRequest
    {
        SenderAssetHolderId = contract.PayerAssetHolderId,
        ReceiverAssetHolderId = contract.PayeeAssetHolderId,
        SenderWalletIdentifierId = contract.PayerWalletIdentifierId,
        ReceiverWalletIdentifierId = contract.PayeeWalletIdentifierId,
        AssetType = contract.AssetType,
        Amount = contract.Amount,
        Date = DateTime.UtcNow,
        CategoryId = contract.CategoryId,
        Description = $"Contract: {contract.Name} - Execution #{contract.ExecutionCount + 1}",
        AutoApprove = !contract.RequiresApproval
    };
    
    try
    {
        var result = await _transferService.TransferAsync(transferRequest);
        
        // Record successful execution
        return await RecordExecution(contract, ContractExecutionStatus.Executed, result);
    }
    catch (Exception ex)
    {
        // Record failed execution
        return await RecordExecution(contract, ContractExecutionStatus.Failed, null, ex.Message);
    }
}
```

---

## API Endpoints

### Contract Management

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/contracts` | List all contracts |
| GET | `/api/v1/contracts/{id}` | Get contract by ID |
| POST | `/api/v1/contracts` | Create new contract |
| PUT | `/api/v1/contracts/{id}` | Update contract |
| DELETE | `/api/v1/contracts/{id}` | Soft delete contract |
| POST | `/api/v1/contracts/{id}/pause` | Pause contract |
| POST | `/api/v1/contracts/{id}/resume` | Resume contract |
| POST | `/api/v1/contracts/{id}/cancel` | Cancel contract |

### Contract Execution

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/contracts/{id}/execute` | Manually execute contract |
| POST | `/api/v1/contracts/{id}/skip` | Skip next execution |
| GET | `/api/v1/contracts/{id}/executions` | Get execution history |
| GET | `/api/v1/contracts/{id}/executions/{executionId}` | Get specific execution |

### Dashboard Views

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/contracts/upcoming` | Upcoming payments (next 30 days) |
| GET | `/api/v1/contracts/due` | Contracts due for execution |
| GET | `/api/v1/contracts/summary` | Summary statistics |

### Request/Response DTOs

```csharp
// Create/Update Request
public class ContractRequest
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public Guid PayerAssetHolderId { get; set; }
    public Guid PayeeAssetHolderId { get; set; }
    public Guid? PayerWalletIdentifierId { get; set; }
    public Guid? PayeeWalletIdentifierId { get; set; }
    public decimal Amount { get; set; }
    public AssetType AssetType { get; set; }
    public ContractFrequency Frequency { get; set; }
    public int? CustomFrequencyDays { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? ExecutionDay { get; set; }
    public Guid? CategoryId { get; set; }
    public bool AutoExecute { get; set; }
    public bool RequiresApproval { get; set; }
}

// Response
public class ContractResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string PayerName { get; set; }
    public string PayeeName { get; set; }
    public decimal Amount { get; set; }
    public string AssetTypeName { get; set; }
    public string Frequency { get; set; }
    public string Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? NextExecutionDate { get; set; }
    public DateTime? LastExecutedAt { get; set; }
    public int ExecutionCount { get; set; }
    public string? CategoryName { get; set; }
}

// Upcoming Payments View
public class UpcomingPaymentResponse
{
    public Guid ContractId { get; set; }
    public string ContractName { get; set; }
    public string PayerName { get; set; }
    public string PayeeName { get; set; }
    public decimal Amount { get; set; }
    public string AssetTypeName { get; set; }
    public DateTime DueDate { get; set; }
    public int DaysUntilDue { get; set; }
}
```

---

## Service Layer

### ContractService

**Proposed File:** `Application/Services/Support/ContractService.cs`

```csharp
public interface IContractService
{
    // CRUD
    Task<List<ContractResponse>> ListAsync();
    Task<ContractResponse?> GetAsync(Guid id);
    Task<ContractResponse> CreateAsync(ContractRequest request);
    Task<ContractResponse> UpdateAsync(Guid id, ContractRequest request);
    Task<bool> DeleteAsync(Guid id);
    
    // Status management
    Task<ContractResponse> PauseAsync(Guid id);
    Task<ContractResponse> ResumeAsync(Guid id);
    Task<ContractResponse> CancelAsync(Guid id);
    
    // Execution
    Task<ContractExecution> ExecuteAsync(Guid id);
    Task<ContractExecution> SkipAsync(Guid id, string? reason);
    Task<List<ContractExecution>> GetExecutionsAsync(Guid id);
    
    // Dashboard
    Task<List<UpcomingPaymentResponse>> GetUpcomingPaymentsAsync(int days = 30);
    Task<List<ContractResponse>> GetDueContractsAsync();
    Task<ContractSummary> GetSummaryAsync();
    
    // Scheduler support
    Task ProcessDueContractsAsync(); // Called by background job
}
```

### Background Job (Optional)

For automatic execution, a background job can process due contracts:

```csharp
// Using Hangfire or similar
public class ContractSchedulerJob
{
    private readonly IContractService _contractService;
    
    [AutomaticRetry(Attempts = 0)]
    public async Task ProcessDueContracts()
    {
        await _contractService.ProcessDueContractsAsync();
    }
}

// Registration in Program.cs
RecurringJob.AddOrUpdate<ContractSchedulerJob>(
    "process-due-contracts",
    job => job.ProcessDueContracts(),
    Cron.Daily(6, 0) // Run daily at 6:00 AM
);
```

---

## UI Integration

### Contract Management Page

**Route:** `/financeiro/contratos`

**Features:**
- List all contracts with status indicators
- Create/Edit contract modal
- Execution history per contract
- Manual execute/skip actions

### Dashboard Widget

**Location:** `/financeiro/planilha` or `/dashboard`

**Features:**
- Upcoming payments summary (next 7/30 days)
- Total scheduled outgoing payments
- Due contracts alert
- Quick execute action

### Mockup Structure

```
┌─────────────────────────────────────────────────────────────────────────┐
│  Contracts Management                                           [+ New] │
├─────────────────────────────────────────────────────────────────────────┤
│  Filters: [All ▼] [Active ▼] [Monthly ▼]                     🔍 Search │
├─────────────────────────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────────────────────────┐ │
│  │ Contract Name      │ Amount     │ Frequency │ Next Due  │ Status  │ │
│  ├───────────────────────────────────────────────────────────────────┤ │
│  │ Team Salary - João │ R$ 5,000   │ Monthly   │ Feb 05    │ ● Active│ │
│  │ Office Rent        │ R$ 3,200   │ Monthly   │ Feb 01    │ ● Active│ │
│  │ Platform Fee       │ R$ 500     │ Monthly   │ Feb 10    │ ○ Paused│ │
│  └───────────────────────────────────────────────────────────────────┘ │
│                                                                         │
│  Upcoming Payments (Next 30 Days)                                       │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │ Total: R$ 8,700.00                                     3 items  │   │
│  └─────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## Implementation Phases

### Phase 1: Core Infrastructure (Backend)

**Effort:** ~3-4 days

1. Create entity models (`Contract`, `ContractExecution`)
2. Create enums (`ContractFrequency`, `ContractStatus`, `ContractExecutionStatus`)
3. Add DbSet and EF configuration
4. Create migration
5. Implement `ContractService` (CRUD, status management)
6. Implement `ContractController` with basic endpoints

### Phase 2: Execution Logic (Backend)

**Effort:** ~2-3 days

1. Implement `ExecuteAsync` with TransferService integration
2. Implement `ProcessDueContractsAsync` for batch processing
3. Add next execution date calculation logic
4. Create execution history tracking
5. Add validation (payer/payee exist, wallets available)

### Phase 3: Dashboard & Views (Backend)

**Effort:** ~1-2 days

1. Implement `GetUpcomingPaymentsAsync`
2. Implement `GetDueContractsAsync`
3. Implement `GetSummaryAsync`
4. Add query optimization (includes, pagination)

### Phase 4: Background Job (Optional)

**Effort:** ~1 day

1. Set up Hangfire (if not already)
2. Create `ContractSchedulerJob`
3. Configure daily execution schedule
4. Add monitoring/alerting

### Phase 5: Frontend Integration

**Effort:** ~3-4 days

1. Create contracts page (`/financeiro/contratos`)
2. Create contract list component
3. Create contract form modal
4. Create execution history view
5. Add dashboard widget
6. Connect to API endpoints

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [TRANSACTION_INFRASTRUCTURE.md](../03_CORE_SYSTEMS/TRANSACTION_INFRASTRUCTURE.md) | Transaction system integration |
| [CATEGORY_SYSTEM.md](../04_SUPPORTING_SYSTEMS/CATEGORY_SYSTEM.md) | Category classification |
| [ENTITY_INFRASTRUCTURE.md](../03_CORE_SYSTEMS/ENTITY_INFRASTRUCTURE.md) | AssetHolder relationships |
| [SERVICE_LAYER_ARCHITECTURE.md](../02_ARCHITECTURE/SERVICE_LAYER_ARCHITECTURE.md) | Service patterns |
| [TRANSACTION_API_ENDPOINTS.md](../06_API/TRANSACTION_API_ENDPOINTS.md) | Transfer API details |

---

## Open Questions

1. **Automatic Execution:** Should the system automatically execute contracts, or just notify users?
   - Recommendation: Start with manual execution + notifications, add auto-execute as opt-in feature

2. **Partial Payments:** Should contracts support partial payments or variable amounts?
   - Recommendation: Start with fixed amounts; add variable amount support later if needed

3. **Multi-Currency:** Should a single contract support multiple asset types?
   - Recommendation: One asset type per contract; create multiple contracts for multi-currency scenarios

4. **Approval Workflow:** How should generated transactions be approved?
   - Recommendation: Use existing `ApprovedAt` field; transactions await approval unless `AutoApprove` is set

---

*Plan created: January 29, 2026*  
*Status: Planning*
