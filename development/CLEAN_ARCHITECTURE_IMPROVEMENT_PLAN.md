# Clean Architecture & DDD Improvement Plan

## Executive Summary

This document presents a comprehensive code review of the SF_management project, identifying improvements needed to align with **Clean Code**, **Clean Architecture**, and **Domain-Driven Design (DDD)** principles.

### Overall Assessment

| Layer | Clean Architecture | DDD Compliance | Clean Code |
|-------|-------------------|----------------|------------|
| Domain | 5/10 | 4/10 | 6/10 |
| Application | 6/10 | 5/10 | 6/10 |
| Infrastructure | 6/10 | 6/10 | 7/10 |
| API | 7/10 | 6/10 | 7/10 |
| **Overall** | **6/10** | **5/10** | **6.5/10** |

### Critical Issues Summary

1. **Anemic Domain Models** - Entities lack behavior and validation
2. **Business Logic Leakage** - Domain logic in Application/API layers
3. **Missing Repository Pattern** - Direct DbContext access in services
4. **Missing Value Objects** - Primitive obsession throughout
5. **Dependency Violations** - Domain layer depends on Application layer

---

## Table of Contents

1. [Domain Layer Issues](#1-domain-layer-issues)
2. [Application Layer Issues](#2-application-layer-issues)
3. [Infrastructure Layer Issues](#3-infrastructure-layer-issues)
4. [API Layer Issues](#4-api-layer-issues)
5. [Implementation Roadmap](#5-implementation-roadmap)
6. [Quick Reference Tables](#6-quick-reference-tables)

---

## 1. Domain Layer Issues

### 1.1 Anemic Domain Models (CRITICAL)

**Problem**: Most entities are data bags with public setters and no behavior.

| Entity | File | Issue |
|--------|------|-------|
| `BaseAssetHolder` | `Domain/Entities/AssetHolders/BaseAssetHolder.cs` | Public setters, no validation, no factory methods |
| `Client` | `Domain/Entities/AssetHolders/Client.cs` | No birthday validation (future dates allowed) |
| `Bank` | `Domain/Entities/AssetHolders/Bank.cs` | No code validation |
| `Member` | `Domain/Entities/AssetHolders/Member.cs` | Share can exceed 100%, no validation |
| `PokerManager` | `Domain/Entities/AssetHolders/PokerManager.cs` | Only one property, no behavior |
| `BaseTransaction` | `Domain/Entities/Transactions/BaseTransaction.cs` | Comment says "only positive amounts" but no enforcement |
| `Address` | `Domain/Entities/Support/Address.cs` | Should be a Value Object |
| `ContactPhone` | `Domain/Entities/Support/ContactPhone.cs` | Should be a Value Object |

**Example of Current (Anemic) vs Rich Model**:

```csharp
// CURRENT: Anemic model
public class Member : BaseDomain
{
    public decimal Share { get; set; }  // Can be set to 150%!
    public decimal Salary { get; set; }
}

// RECOMMENDED: Rich model
public class Member : BaseDomain
{
    public decimal Share { get; private set; }
    public decimal Salary { get; private set; }
    
    private Member() { } // EF Core
    
    public static Member Create(decimal share, decimal salary, Guid assetHolderId)
    {
        if (share < 0 || share > 100)
            throw new DomainException("Share must be between 0 and 100%");
        
        return new Member
        {
            Share = share,
            Salary = salary,
            BaseAssetHolderId = assetHolderId
        };
    }
    
    public void UpdateShare(decimal newShare)
    {
        if (newShare < 0 || newShare > 100)
            throw new DomainException("Share must be between 0 and 100%");
        Share = newShare;
    }
}
```

### 1.2 Dependency Violations (CRITICAL)

**Problem**: Domain layer has dependencies on Application layer, violating Clean Architecture.

| Location | Issue |
|----------|-------|
| `Domain/Entities/Assets/WalletIdentifier.cs:73` | Calls `WalletIdentifierValidationService` from Application layer |
| `Domain/Interfaces/IAssetHolderDomainService.cs:1-2` | References `Application.DTOs` |

**Recommended Fix**: 
- Move validation logic to domain entities or create domain validation services
- Move `DomainValidationResult` to `Domain/Common/`
- Remove all Application layer references from Domain

### 1.3 Missing Value Objects (HIGH)

**Problem**: Primitive obsession - using primitives where Value Objects provide better type safety and validation.

| Concept | Current | Recommended |
|---------|---------|-------------|
| Government Number | `string` | `GovernmentNumber` (CPF/CNPJ validation) |
| Address | Entity with public setters | `Address` Value Object (immutable) |
| Phone | Entity with public setters | `PhoneNumber` Value Object (validation) |
| Money/Amount | `decimal` | `Money` Value Object (with currency) |
| Bank Code | `string` | `BankCode` Value Object (validation) |

**Example Value Object**:

```csharp
// Domain/ValueObjects/GovernmentNumber.cs
public sealed class GovernmentNumber : IEquatable<GovernmentNumber>
{
    public string Value { get; }
    public TaxEntityType EntityType { get; }
    
    private GovernmentNumber(string value, TaxEntityType entityType)
    {
        Value = value;
        EntityType = entityType;
    }
    
    public static GovernmentNumber Create(string value)
    {
        var cleaned = new string(value.Where(char.IsDigit).ToArray());
        
        return cleaned.Length switch
        {
            11 when IsValidCpf(cleaned) => new GovernmentNumber(cleaned, TaxEntityType.Individual),
            14 when IsValidCnpj(cleaned) => new GovernmentNumber(cleaned, TaxEntityType.Company),
            _ => throw new DomainException("Invalid government number")
        };
    }
    
    // Equality, validation methods...
}
```

### 1.4 Missing Encapsulation (HIGH)

**Problem**: All entities use public setters, allowing invalid state.

**Examples**:
- `BaseAssetHolder.Name` can be empty
- `BaseAssetHolder.GovernmentNumber` has no format validation
- `Member.Share` can exceed 100%
- `BaseTransaction.AssetAmount` can be negative

**Recommended Fix**: Use private setters with factory methods or constructors that validate invariants.

### 1.5 Unclear Aggregate Boundaries (MEDIUM)

**Problem**: Aggregate roots are not clearly defined or documented.

**Identified Aggregates**:
| Aggregate Root | Entities |
|----------------|----------|
| `BaseAssetHolder` | `Client`, `Bank`, `Member`, `PokerManager`, `Address`, `ContactPhone`, `InitialBalance`, `Referral` |
| `AssetPool` | `WalletIdentifier` |
| `BaseTransaction` | (standalone) |

**Recommendation**: Document aggregate boundaries in code using interfaces or attributes.

### 1.6 Exception Hierarchy (LOW)

**Minor Issue**: `WalletMissingException` is separate from the `BusinessException` hierarchy.

**Recommendation**: Have `WalletMissingException` inherit from `BusinessException` for consistency.

---

## 2. Application Layer Issues

### 2.1 Business Logic in Services (CRITICAL)

**Problem**: Services contain complex business logic that should be in the domain layer.

| Service | Method | Lines | Issue |
|---------|--------|-------|-------|
| `BaseAssetHolderService` | `GetBalancesByAssetType` | 413-572 | Balance calculation logic |
| `BaseAssetHolderService` | `GetBalancesByAssetGroup` | 575-765 | Duplicate balance logic |
| `ProfitCalculationService` | Multiple | 152-492 | Profit calculation algorithms |
| `AvgRateService` | `CalculateAvgRate` | 176-336 | Weighted average calculation |
| `TransferService` | `TransferAsync` | 42-316 | 274 lines with multiple responsibilities |

**Example of Logic to Move to Domain**:

```csharp
// CURRENT: Business logic in Application service
// Application/Services/Finance/ProfitCalculationService.cs
public async Task<decimal> CalculateRakeCommission(...)
{
    // Complex business formula here
    return rakeAmount * ((rakeCommission - rakeBack) / 100);
}

// RECOMMENDED: Domain service
// Domain/Services/ProfitCalculationDomainService.cs
public class ProfitCalculator
{
    public static decimal CalculateRakeCommission(
        decimal rakeAmount, 
        decimal rakeCommission, 
        decimal rakeBack)
    {
        if (rakeCommission < rakeBack)
            throw new DomainException("Commission cannot be less than rakeback");
        
        return rakeAmount * ((rakeCommission - rakeBack) / 100);
    }
}
```

### 2.2 Single Responsibility Violations (CRITICAL)

| Service | Lines | Issue |
|---------|-------|-------|
| `BaseAssetHolderService` | 920 | Too large, should be split |
| `TransferService.TransferAsync` | 274 | Too long, multiple responsibilities |

**Recommended Split for `BaseAssetHolderService`**:
- `AssetHolderQueryService` - Read operations
- `AssetHolderCommandService` - Write operations
- `BalanceCalculationService` - Balance logic (or move to Domain)

### 2.3 Missing Interface Abstractions (HIGH)

**Services without interfaces**:
- `WalletIdentifierService`
- `WalletIdentifierValidationService`
- `CachedLookupService`

**Impact**: Difficult to test and mock.

### 2.4 Direct DataContext Access (HIGH)

**Problem**: Services directly access `DataContext` instead of repositories.

**Examples**:
- `BaseAssetHolderService.cs` lines 99-106, 419-421
- `TransferService.cs` lines 94-99, 121-127

### 2.5 Validation Logic in Services (HIGH)

**Problem**: Validation mixed with business logic in `TransferService` (lines 54-155).

**Recommendation**: Use FluentValidation validators consistently.

### 2.6 Code Duplication (MEDIUM)

**Duplicated Logic**:
- Balance calculation in `GetBalancesByAssetType` and `GetBalancesByAssetGroup`
- Wallet validation logic in multiple places

### 2.7 Naming Conventions (MEDIUM)

**Issue**: `_AssetPoolService` uses PascalCase instead of camelCase for private field.

**File**: `Application/Services/Assets/WalletIdentifierService.cs:20`

### 2.8 Incomplete Validators (MEDIUM)

| Validator | Issue |
|-----------|-------|
| `ClientRequestValidator.cs` | Empty |
| `WalletValidator.cs` | Minimal validation |
| `WalletTransactionValidator.cs` | Basic validation only |

### 2.9 Business Logic in AutoMapper (MEDIUM)

**File**: `Application/Mappings/AutoMapperProfile.cs`
**Lines**: 86-116, 157-202

**Issue**: `AfterMap` contains business logic for metadata extraction.

### 2.10 Commented-Out Code (LOW)

**File**: `DigitalAssetTransactionService.cs` lines 93-363

**Recommendation**: Remove or move to version control history.

---

## 3. Infrastructure Layer Issues

### 3.1 Missing Repository Pattern (CRITICAL)

**Problem**: No repository pattern. Services directly access `DbSet<TEntity>`.

**Impact**:
- Tight coupling between Application and Infrastructure
- Difficult to test
- Hard to swap data access implementations

**Recommended Implementation**:

```csharp
// Domain/Interfaces/IRepository.cs
public interface IRepository<TEntity> where TEntity : BaseDomain
{
    Task<TEntity?> GetByIdAsync(Guid id);
    Task<List<TEntity>> GetAllAsync();
    Task<TEntity> AddAsync(TEntity entity);
    Task UpdateAsync(TEntity entity);
    Task DeleteAsync(Guid id);
    IQueryable<TEntity> Query();
}

// Infrastructure/Data/Repositories/Repository.cs
public class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseDomain
{
    private readonly DataContext _context;
    private readonly DbSet<TEntity> _dbSet;
    
    public Repository(DataContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }
    
    public async Task<TEntity?> GetByIdAsync(Guid id) 
        => await _dbSet.FirstOrDefaultAsync(e => e.Id == id && !e.DeletedAt.HasValue);
    
    // Other implementations...
}
```

### 3.2 Missing Unit of Work Pattern (CRITICAL)

**Problem**: Each service method calls `SaveChangesAsync()` individually.

**Impact**: Cannot coordinate multiple repository operations in a single transaction.

**Recommended Implementation**:

```csharp
// Domain/Interfaces/IUnitOfWork.cs
public interface IUnitOfWork : IDisposable
{
    IRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseDomain;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

### 3.3 Business Logic in Infrastructure (CRITICAL)

**File**: `Infrastructure/Data/DataContext.cs`
**Lines**: 535-614

**Issues**:
- `SetDefaultProperties()` contains audit trail logic
- `GetCurrentUserId()` contains user ID generation logic
- Soft delete logic mixed with audit logic

**Recommendation**: Move to Application layer `IEntityAuditor` service.

### 3.4 Hardcoded Configuration Paths (HIGH)

**File**: `Infrastructure/Data/DataContext.cs` lines 24-25

```csharp
// CURRENT: Hardcoded paths
.AddJsonFile("/Users/fernandobarroso/.microsoft/usersecrets/...", optional: true)
.AddJsonFile("/etc/secrets/secrets.json", optional: true)
```

**Recommendation**: Use environment variables or configuration builder patterns.

### 3.5 Missing Specification Pattern (HIGH)

**Problem**: Complex queries embedded in services, not reusable.

**Recommendation**: Implement Specification pattern for composable queries.

### 3.6 Large OnModelCreating (HIGH)

**File**: `Infrastructure/Data/DataContext.cs` lines 83-527 (444 lines)

**Recommendation**: Split into `IEntityTypeConfiguration<TEntity>` classes.

### 3.7 Sensitive Data Logging (HIGH)

**File**: `Program.cs` line 46

**Issue**: `EnableSensitiveDataLogging()` is always enabled.

**Recommendation**: Only enable in Development environment.

### 3.8 Inconsistent AsNoTracking (MEDIUM)

**Problem**: Some read-only queries don't use `AsNoTracking()`.

### 3.9 Missing Global Query Filters (MEDIUM)

**Problem**: Soft delete filtering repeated in every query.

**Recommendation**: Use EF Core global query filters.

### 3.10 Design-Time Classes in DataContext (MEDIUM)

**Issue**: `DesignTimeLoggingService` and `HttpContextAccessor` in DataContext file.

---

## 4. API Layer Issues

### 4.1 Business Logic in Controllers (CRITICAL)

| Controller | Method | Lines | Issue |
|------------|--------|-------|-------|
| `PokerManagerController` | `GetWalletIdentifiersFromOthers` | 135-191 | Complex mapping and business logic |
| `CompanyAssetPoolController` | Multiple | 69-156 | Enrichment logic |
| `SettlementTransactionController` | `GetClosingsGrouped`, `GetSignedAssetAmount` | 45-86 | Business transformations |
| `ImportedTransactionController` | `GenerateMatchReasons`, `CalculateMatchScore` | 375-414 | Reconciliation logic |

**Recommendation**: Move all business logic to service layer.

### 4.2 Missing Input Validation (HIGH)

**File**: `Api/Controllers/Base/BaseApiController.cs` lines 71-77, 84-90

**Issue**: `Post` and `Put` methods don't validate `ModelState`.

### 4.3 Incomplete Implementations (HIGH)

**File**: `Api/Controllers/v1/AssetHolders/ClientController.cs`

**Methods throwing `NotImplementedException`**:
- `WalletIdentifierHas` (lines 99-101)
- `GetInitialBalance` (lines 118-122)

### 4.4 Middleware Order Issue (HIGH)

**File**: `Program.cs` line 116

**Issue**: `ErrorHandlerMiddleware` registered after `UseAuthorization()`.

**Recommendation**: Move before `UseAuthorization()`.

### 4.5 Inconsistent Error Responses (MEDIUM)

**Problem**: Mix of `ProblemDetails` and anonymous objects for errors.

**Example in `InitialBalanceController`**: Returns `BadRequest(new { error = ex.Message })`

### 4.6 Non-RESTful Endpoints (MEDIUM)

**Issue**: Some endpoints use POST for queries.

**Example**: `ImportedTransactionController.FindPotentialMatches`

---

## 5. Implementation Roadmap

### Phase 1: Foundation (Critical - Week 1-2)

1. **Create Value Objects**
   - `GovernmentNumber` (CPF/CNPJ)
   - `Money` (amount + currency)
   - Convert `Address` to Value Object
   - Convert `ContactPhone` to Value Object

2. **Fix Dependency Violations**
   - Remove Application layer references from Domain
   - Move `DomainValidationResult` to `Domain/Common/`

3. **Add Repository Pattern**
   - Create `IRepository<TEntity>` interface
   - Implement `Repository<TEntity>` in Infrastructure
   - Create `IUnitOfWork` interface and implementation

### Phase 2: Domain Enrichment (High - Week 3-4)

4. **Enrich Domain Models**
   - Add factory methods to all entities
   - Add private setters
   - Add validation in constructors
   - Add business methods where appropriate

5. **Create Domain Services**
   - `BalanceCalculationDomainService`
   - `ProfitCalculationDomainService`
   - `AvgRateDomainService`

6. **Move Business Logic from Application**
   - Extract calculation logic from services
   - Keep Application services as orchestrators only

### Phase 3: Infrastructure Cleanup (High - Week 5-6)

7. **Split Entity Configurations**
   - Create `IEntityTypeConfiguration<T>` per entity
   - Clean up `OnModelCreating`

8. **Fix Configuration Issues**
   - Remove hardcoded paths
   - Environment-aware sensitive data logging

9. **Add Global Query Filters**
   - Soft delete filter
   - Standardize `AsNoTracking()` usage

### Phase 4: API Cleanup (Medium - Week 7-8)

10. **Move Business Logic from Controllers**
    - `PokerManagerController` logic to service
    - `CompanyAssetPoolController` enrichment to service
    - `SettlementTransactionController` transformations to service
    - `ImportedTransactionController` reconciliation to service

11. **Standardize Error Handling**
    - Create `ApiResponseFactory`
    - Consistent `ProblemDetails` usage
    - Fix middleware order

12. **Complete Validators**
    - Implement all empty validators
    - Use FluentValidation consistently

### Phase 5: Polish (Low - Week 9-10)

13. **Code Cleanup**
    - Remove commented code
    - Fix naming conventions
    - Add missing interface abstractions

14. **Documentation**
    - Document aggregate boundaries
    - Update architecture documentation

---

## 6. Quick Reference Tables

### Issues by Priority

| Priority | Count | Categories |
|----------|-------|------------|
| Critical | 10 | Domain models, dependencies, repositories |
| High | 14 | Validation, business logic, configuration |
| Medium | 12 | Error handling, code quality |
| Low | 6 | Minor improvements |
| **Total** | **42** | |

### Issues by Layer

| Layer | Critical | High | Medium | Low | Total |
|-------|----------|------|--------|-----|-------|
| Domain | 2 | 3 | 1 | 1 | 7 |
| Application | 3 | 5 | 5 | 3 | 16 |
| Infrastructure | 3 | 4 | 3 | 1 | 11 |
| API | 2 | 2 | 3 | 1 | 8 |

### Files Requiring Most Changes

| File | Issues | Priority |
|------|--------|----------|
| `BaseAssetHolderService.cs` | 5 | Critical |
| `DataContext.cs` | 5 | Critical/High |
| `TransferService.cs` | 4 | Critical |
| `BaseApiController.cs` | 3 | High |
| `WalletIdentifier.cs` | 3 | Critical |

---

## Appendix: DDD Pattern Reference

### Aggregate Rules

1. Reference aggregates by identity only
2. Changes within an aggregate must be consistent
3. Only the aggregate root can be obtained from persistence

### Service Responsibilities

| Layer | Responsibility | Contains |
|-------|---------------|----------|
| **Domain Services** | Business logic that doesn't fit in entities | Calculations, complex validations |
| **Application Services** | Orchestration, use case coordination | Transaction management, DTO mapping |
| **Infrastructure Services** | External concerns | Data access, email, file storage |

### Clean Architecture Dependencies

```
[UI/API] → [Application] → [Domain]
             ↓
        [Infrastructure]
```

- Domain has NO dependencies
- Application depends on Domain only
- Infrastructure depends on Application and Domain
- API depends on Application

---

*Created: January 23, 2026*
*Review Scope: SF_management codebase*
