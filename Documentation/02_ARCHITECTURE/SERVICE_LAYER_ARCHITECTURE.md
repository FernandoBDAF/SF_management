# Service Layer Architecture

## Overview

The SF Management system uses a layered service architecture that provides a clean separation of concerns between data access, business logic, and API presentation. The service layer is built on a hierarchy of generic base classes that provide common functionality while allowing specialized behavior through inheritance and composition.

This document describes the service layer design patterns, inheritance hierarchy, and best practices for working with services.

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Service Hierarchy](#service-hierarchy)
3. [BaseService](#baseservice)
4. [BaseAssetHolderService](#baseassetholderservice)
5. [BaseTransactionService](#basetransactionservice)
6. [Specialized Services](#specialized-services)
7. [Dependency Injection](#dependency-injection)
8. [Design Patterns Used](#design-patterns-used)

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                      Controllers                             │
│  (BaseApiController, BaseAssetHolderController)             │
└──────────────────────────┬──────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────┐
│                    Service Layer                             │
│  ┌─────────────┐  ┌─────────────────────┐  ┌──────────────┐│
│  │ BaseService │  │BaseAssetHolderService│  │BaseTransaction││
│  │             │  │                      │  │   Service    ││
│  └─────────────┘  └─────────────────────┘  └──────────────┘│
│         ▲                 ▲                      ▲          │
│         │                 │                      │          │
│  ┌──────┴─────┐    ┌─────┴──────┐      ┌───────┴────────┐ │
│  │Specialized │    │  Entity    │      │  Transaction   │ │
│  │ Services   │    │  Services  │      │   Services     │ │
│  └────────────┘    └────────────┘      └────────────────┘ │
└──────────────────────────┬──────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────┐
│                    Data Context                              │
│                   (Entity Framework)                         │
└─────────────────────────────────────────────────────────────┘
```

---

## Service Hierarchy

```
BaseService<TEntity>
├── CategoryService (Category)
├── AddressService (Address)
├── ContactPhoneService (ContactPhone)
├── InitialBalanceService (InitialBalance)
├── ReferralService (Referral)
├── AssetPoolService (AssetPool)
├── WalletIdentifierService (WalletIdentifier)
├── ImportedTransactionService (ImportedTransaction)
│
├── BaseAssetHolderService<TEntity>
│   ├── ClientService (Client)
│   ├── BankService (Bank)
│   ├── MemberService (Member)
│   └── PokerManagerService (PokerManager)
│
└── BaseTransactionService<TEntity>
    ├── FiatAssetTransactionService (FiatAssetTransaction)
    ├── DigitalAssetTransactionService (DigitalAssetTransaction)
    └── SettlementTransactionService (SettlementTransaction)
```

---

## BaseService

The foundation of all services in the system. Provides standard CRUD operations with soft delete support.

**File**: `Services/BaseService.cs`

### Definition

```csharp
public class BaseService<TEntity> where TEntity : BaseDomain
{
    public readonly DbSet<TEntity> _entity;
    public readonly DataContext context;

    public BaseService(DataContext context, IHttpContextAccessor httpContextAccessor)
    {
        this.context = context;
        _entity = context.Set<TEntity>();
    }
}
```

### Core Methods

| Method | Description | Behavior |
|--------|-------------|----------|
| `List()` | Returns all non-deleted entities | Orders by `CreatedAt` descending |
| `Get(Guid id)` | Retrieves single entity by ID | Returns `null` if not found or deleted |
| `Add(TEntity obj)` | Creates new entity | Auto-sets audit fields |
| `Update(Guid id, TEntity obj)` | Updates existing entity | Preserves `Id`, `CreatedAt`, `DeletedAt` |
| `Delete(Guid id)` | Soft deletes entity | Sets `DeletedAt` timestamp |

### Key Features

**Soft Delete Pattern**: Entities are never physically deleted. The `DeletedAt` timestamp indicates deletion status.

```csharp
public virtual async Task Delete(Guid id)
{
    var obj = await _entity.FirstOrDefaultAsync(x => x.Id == id && !x.DeletedAt.HasValue);
    if (obj != null)
    {
        obj.DeletedAt = DateTime.UtcNow;
        _entity.Update(obj);
        await context.SaveChangesAsync();
    }
}
```

**Automatic Filtering**: All queries automatically exclude soft-deleted records:

```csharp
return await _entity.Where(x => !x.DeletedAt.HasValue)
    .OrderByDescending(x => x.CreatedAt)
    .ToListAsync();
```

**Dynamic Property Updates**: The `Update` method copies properties dynamically while protecting system fields:

```csharp
foreach (var property in typeof(TEntity).GetProperties())
{
    if (property.Name != "Id" && property.Name != "CreatedAt" && 
        property.Name != "UpdatedAt" && property.Name != "DeletedAt")
    {
        // Copy non-null, non-empty-guid values
    }
}
```

---

## BaseAssetHolderService

Extends `BaseService` with specialized functionality for asset holder entities (Client, Bank, Member, PokerManager).

**File**: `Services/BaseAssetHolderService.cs`

### Definition

```csharp
public class BaseAssetHolderService<TEntity>(
    DataContext context, 
    IHttpContextAccessor httpContextAccessor, 
    IAssetHolderDomainService domainService, 
    ReferralService referralService, 
    InitialBalanceService initialBalanceService) 
    : BaseService<TEntity>(context, httpContextAccessor) 
    where TEntity : BaseDomain, IAssetHolder
```

### Strategy Pattern Implementation

The service uses the **Strategy Pattern** to handle different entity types without complex switch statements:

```csharp
// Include strategies for eager loading
private static readonly Dictionary<Type, Func<IQueryable<TEntity>, IQueryable<TEntity>>> 
    IncludeStrategies = new()
{
    [typeof(Bank)] = query => ((IQueryable<Bank>)query).Include(b => b.BaseAssetHolder).Cast<TEntity>(),
    [typeof(Client)] = query => ((IQueryable<Client>)query).Include(c => c.BaseAssetHolder).Cast<TEntity>(),
    [typeof(Member)] = query => ((IQueryable<Member>)query).Include(m => m.BaseAssetHolder).Cast<TEntity>(),
    [typeof(PokerManager)] = query => ((IQueryable<PokerManager>)query).Include(pm => pm.BaseAssetHolder).Cast<TEntity>()
};

// Entity retrieval strategies
private static readonly Dictionary<Type, Func<DataContext, Guid, Task<TEntity?>>> 
    EntityGetStrategies = new()
{
    [typeof(Bank)] = async (ctx, id) => 
    {
        var bank = await ctx.Banks.FirstOrDefaultAsync(x => x.BaseAssetHolderId == id && !x.DeletedAt.HasValue);
        return bank as TEntity;
    },
    // ... other strategies
};

// Entity type mapping
private static readonly Dictionary<Type, AssetHolderType> EntityTypeMapping = new()
{
    [typeof(Client)] = AssetHolderType.Client,
    [typeof(Bank)] = AssetHolderType.Bank,
    [typeof(Member)] = AssetHolderType.Member,
    [typeof(PokerManager)] = AssetHolderType.PokerManager
};
```

### Core Methods

#### Generic Create with Validation

```csharp
public async Task<TEntity> AddFromRequest<TRequest>(
    TRequest request, 
    Func<BaseAssetHolder, TEntity> entityFactory,
    Func<TRequest, Task<DomainValidationResult>> validationMethod) 
    where TRequest : BaseAssetHolderRequest
{
    // 1. Validate using domain service
    var validationResult = await validationMethod(request);
    if (!validationResult.IsValid)
        throw new ValidationException(validationResult.Errors);

    // 2. Create BaseAssetHolder
    var baseAssetHolder = await CreateBaseAssetHolder(
        request.Name,
        request.GovernmentNumber,
        request.TaxEntityType,
        request.ReferrerId
    );

    // 3. Create specific entity using factory
    var entity = entityFactory(baseAssetHolder);

    // 4. Save and return with includes
    var result = await base.Add(entity);
    return await Get(result.BaseAssetHolderId);
}
```

#### Generic Update with Validation

```csharp
public async Task<TEntity> UpdateFromRequest<TRequest>(
    Guid entityId, 
    TRequest request,
    Action<TEntity, TRequest> updateAction,
    Func<TRequest, Task<DomainValidationResult>> validationMethod) 
    where TRequest : BaseAssetHolderRequest
```

#### Business Rule Methods

| Method | Description |
|--------|-------------|
| `CanDelete(Guid entityId)` | Checks if entity can be deleted (no dependencies) |
| `DeleteWithValidation(Guid entityId)` | Validates then soft deletes entity and BaseAssetHolder |
| `GetAssetHolderStatistics(Guid entityId)` | Returns statistics including balance, active transactions |

#### Balance Calculation Methods

```csharp
// Get balances grouped by AssetType
public async Task<Dictionary<AssetType, decimal>> GetBalancesByAssetType(Guid baseAssetHolderId)

// Get balances grouped by AssetGroup
public async Task<Dictionary<AssetGroup, decimal>> GetBalancesByAssetGroup(Guid baseAssetHolderId)

// Get transaction statement
public async Task<StatementAssetHolderWithTransactions> GetTransactionsStatementForAssetHolder(Guid baseAssetHolderId)
```

---

## BaseTransactionService

Extends `BaseService` with specialized functionality for transaction entities.

**File**: `Services/BaseTransactionService.cs`

### Definition

```csharp
public class BaseTransactionService<TEntity> : BaseService<TEntity> 
    where TEntity : BaseTransaction
{
    public BaseTransactionService(DataContext context, IHttpContextAccessor httpContextAccessor) 
        : base(context, httpContextAccessor)
    {
    }
}
```

### Core Methods

#### Paginated Transaction Queries

```csharp
public async Task<TableResponse<TEntity>> GetAssetHolderTransactions(
    Guid[] AssetPoolIds, 
    DateTime? startDate,
    DateTime? endDate, 
    int quantity = 100, 
    int page = 0)
```

Returns transactions where either sender or receiver wallet belongs to the specified asset pools.

#### Excluding Asset Holder Transactions

```csharp
public async Task<TableResponse<TEntity>> GetNonAssetHolderTransactions(
    Guid[]? AssetPoolIds, 
    DateTime? startDate,
    DateTime? endDate, 
    int quantity = 100, 
    int page = 0)
```

Returns transactions that do NOT involve the specified asset pools.

#### Single Wallet Queries

```csharp
public async Task<TEntity[]> GetTransactionsByWalletIdentifier(
    Guid walletIdentifierId, 
    DateTime? startDate = null,
    DateTime? endDate = null, 
    int quantity = 100, 
    int page = 0)

public async Task<decimal> GetBalanceForWalletIdentifier(Guid walletIdentifierId)
```

### Query Optimization

All transaction queries include necessary navigation properties for response mapping:

```csharp
var transactions = await query
    .Include(x => x.Category)
    .Include(x => x.SenderWalletIdentifier)
        .ThenInclude(wi => wi.AssetPool)
        .ThenInclude(aw => aw.BaseAssetHolder)
    .Include(x => x.ReceiverWalletIdentifier)
        .ThenInclude(wi => wi.AssetPool)
        .ThenInclude(aw => aw.BaseAssetHolder)
    .ToListAsync();
```

---

## Specialized Services

### Entity-Specific Services

Each asset holder type has a specialized service that implements entity-specific logic:

**ClientService**
```csharp
public class ClientService : BaseAssetHolderService<Client>
{
    public async Task<Client> AddFromRequest(ClientRequest request)
    {
        return await AddFromRequest(
            request,
            baseAssetHolder => new Client 
            { 
                BaseAssetHolderId = baseAssetHolder.Id,
                BaseAssetHolder = baseAssetHolder,
                // Client-specific properties
                BirthDate = request.BirthDate
            },
            _domainService.ValidateClientCreation
        );
    }
    
    public async Task<ClientStatistics> GetClientStatistics(Guid id)
    {
        // Client-specific statistics including age
    }
}
```

**PokerManagerService**
```csharp
public class PokerManagerService : BaseAssetHolderService<PokerManager>
{
    // Specialized methods for poker operations
    public async Task<Dictionary<AssetType, List<WalletIdentifier>>> GetWalletIdentifiersFromOthers(Guid id)
    
    // Override to use AssetGroup instead of AssetType
    public override async Task<Dictionary<AssetGroup, decimal>> GetBalancesByAssetGroup(Guid id)
}
```

### Transaction-Specific Services

**FiatAssetTransactionService**
```csharp
public class FiatAssetTransactionService : BaseTransactionService<FiatAssetTransaction>
{
    public async Task<FiatAssetTransaction> SendBrazilianReais(Guid assetHolderId, FiatAssetTransactionRequest request)
}
```

**SettlementTransactionService**
```csharp
public class SettlementTransactionService : BaseTransactionService<SettlementTransaction>
{
    public async Task<Dictionary<DateOnly, List<SettlementTransaction>>> GetClosings(Guid pokerManagerId)
    
    public async Task<SettlementTransactionByDateResponse> CreateSettlementTransactionsByDate(
        Guid assetHolderId, 
        SettlementTransactionByDateRequest request)
}
```

### Validation Services

**WalletIdentifierValidationService**
- Validates wallet metadata based on AssetGroup
- Ensures required fields per wallet type

**AssetPoolValidationService**
- Validates asset pool creation and deletion
- Prevents orphaned pools

---

## Dependency Injection

All services are registered with scoped lifetime in `DependencyInjectionExtensions.cs`:

```csharp
public static void AddScopedServices(this WebApplicationBuilder builder)
{
    // Domain services
    builder.Services.AddScoped<IAssetHolderDomainService, AssetHolderDomainService>();
    
    // Entity services (dual registration pattern)
    builder.Services.AddScoped<BaseAssetHolderService<Client>, ClientService>();
    builder.Services.AddScoped<ClientService>();
    
    builder.Services.AddScoped<BaseAssetHolderService<Bank>, BankService>();
    builder.Services.AddScoped<BankService>();
    
    // Transaction services
    builder.Services.AddScoped<BaseTransactionService<FiatAssetTransaction>, FiatAssetTransactionService>();
    builder.Services.AddScoped<FiatAssetTransactionService>();
    
    // Support services
    builder.Services.AddScoped<BaseService<Category>, CategoryService>();
    builder.Services.AddScoped<CategoryService>();
}
```

### Dual Registration Pattern

Services are registered twice:
1. As their base type (for generic controller injection)
2. As their concrete type (for specialized controller methods)

This allows controllers to work with either abstraction:

```csharp
// Generic injection
public BaseApiController(BaseService<TEntity> service, IMapper mapper)

// Specific injection for specialized methods
public ClientController(ClientService service, ...)
```

---

## Design Patterns Used

### 1. Template Method Pattern

Base services define the algorithm skeleton, specialized services override specific steps:

```csharp
// BaseAssetHolderService defines the template
public async Task<TEntity> AddFromRequest<TRequest>(
    TRequest request, 
    Func<BaseAssetHolder, TEntity> entityFactory,      // Extension point
    Func<TRequest, Task<DomainValidationResult>> validationMethod)  // Extension point
```

### 2. Strategy Pattern

Entity-type-specific behavior is encapsulated in strategy dictionaries:

```csharp
private static readonly Dictionary<Type, Func<IQueryable<TEntity>, IQueryable<TEntity>>> 
    IncludeStrategies = new() { ... };
```

### 3. Factory Pattern

Entity creation uses factory functions passed to generic methods:

```csharp
entityFactory(baseAssetHolder)  // Creates the specific entity type
```

### 4. Repository Pattern

Services act as repositories, abstracting data access from controllers:

```csharp
public virtual async Task<List<TEntity>> List()
public virtual async Task<TEntity?> Get(Guid id)
public virtual async Task<TEntity> Add(TEntity obj)
```

### 5. Unit of Work Pattern

`DataContext` provides transaction management across service operations:

```csharp
await context.SaveChangesAsync();  // Commits all changes as a unit
```

---

## Best Practices

### 1. Use Typed Services When Possible

```csharp
// Prefer this (type-safe, IDE support)
private readonly ClientService _clientService;

// Over this (generic, less discoverable)
private readonly BaseAssetHolderService<Client> _service;
```

### 2. Leverage AsNoTracking for Read Operations

```csharp
return await query.AsNoTracking()
    .Where(x => !x.BaseAssetHolder.DeletedAt.HasValue)
    .ToListAsync();
```

### 3. Include Navigation Properties Explicitly

```csharp
.Include(x => x.SenderWalletIdentifier)
    .ThenInclude(wi => wi.AssetPool)
    .ThenInclude(aw => aw.BaseAssetHolder)
```

### 4. Handle Exceptions at Service Layer

```csharp
catch (ValidationException)
{
    throw;  // Re-throw for controller handling
}
catch (Exception ex)
{
    throw new BusinessException($"Failed to create {typeof(TEntity).Name}", ex);
}
```

---

## Related Documentation

- [CONTROLLER_LAYER_ARCHITECTURE.md](CONTROLLER_LAYER_ARCHITECTURE.md) - How controllers use services
- [DATABASE_SCHEMA.md](DATABASE_SCHEMA.md) - Entity relationships
- [ERROR_HANDLING.md](../05_INFRASTRUCTURE/ERROR_HANDLING.md) - Exception types
- [VALIDATION_SYSTEM.md](../05_INFRASTRUCTURE/VALIDATION_SYSTEM.md) - Validation services

