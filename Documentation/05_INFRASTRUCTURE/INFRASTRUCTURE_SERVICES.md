# Infrastructure Services

## Overview

This document covers the infrastructure-level services that provide cross-cutting capabilities to the application. These services handle caching with metrics, cached entity lookups, domain-level validation, and dependency injection registration.

---

## Table of Contents

1. [CacheMetricsService](#cachemetricsservice)
2. [CachedLookupService](#cachedlookupservice)
3. [AssetHolderDomainService](#assetholderdomainservice)
4. [Service Registration](#service-registration)

---

## CacheMetricsService

**File:** `Application/Services/Infrastructure/CacheMetricsService.cs`

**Interface:** `ICacheMetricsService`

**Lifetime:** Singleton

### Purpose

Wraps `IMemoryCache` with per-category hit/miss tracking. Provides a get-or-create pattern with automatic metric collection, enabling cache performance monitoring without modifying individual cache consumers.

### Interface

```csharp
public interface ICacheMetricsService
{
    Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan duration, string category);
    void Remove(string key, string category);
    CacheStatistics GetStatistics();
}
```

### Methods

#### `GetOrCreateAsync<T>(key, factory, duration, category)`

Checks the cache for the given key. On hit, increments the hit counter for the category and returns the cached value. On miss, increments the miss counter, invokes the factory to produce the value, stores it with the given duration, and returns it.

```csharp
var avgRate = await _cacheMetrics.GetOrCreateAsync(
    $"AvgRate:{managerId}:{year}:{month}",
    async () => await CalculateMonthlySnapshot(managerId, year, month),
    TimeSpan.FromHours(24),
    "avgrate");
```

#### `Remove(key, category)`

Removes a cache entry and logs the removal with its category.

```csharp
_cacheMetrics.Remove($"AvgRate:{managerId}:{year}:{month}", "avgrate");
```

#### `GetStatistics()`

Returns a `CacheStatistics` DTO containing per-category performance data.

### Statistics DTO

```csharp
public class CacheStatistics
{
    public Dictionary<string, CategoryStats> Categories { get; set; }
}

public class CategoryStats
{
    public long Hits { get; set; }
    public long Misses { get; set; }
    public double HitRate { get; set; }
}
```

### Hit Rate Calculation

```
HitRate = Hits / (Hits + Misses)
```

Returns `0` when no cache operations have occurred for a category.

### Thread Safety

Uses `ConcurrentDictionary<string, CacheEntryStats>` for the statistics store. Individual counters use `Interlocked.Increment` for lock-free thread-safe updates.

```csharp
private readonly ConcurrentDictionary<string, CacheEntryStats> _stats = new();

// Inside GetOrCreateAsync:
Interlocked.Increment(ref stats.Hits);   // on cache hit
Interlocked.Increment(ref stats.Misses); // on cache miss
```

### Monitoring Endpoint

Cache statistics are exposed via the diagnostics controller:

```
GET /api/v1/diagnostics/cache-stats
```

**Authorization:** Admin-only (`[RequireRole(Auth0Roles.Admin)]`)

**Example Response:**

```json
{
  "categories": {
    "avgrate": {
      "hits": 1250,
      "misses": 45,
      "hitRate": 0.965
    },
    "system-wallets": {
      "hits": 890,
      "misses": 12,
      "hitRate": 0.987
    },
    "manager-lookups": {
      "hits": 450,
      "misses": 8,
      "hitRate": 0.983
    }
  }
}
```

### Current Usage Status

The `CacheMetricsService` is registered and the diagnostics endpoint is functional. However, most existing caches in the application use `IMemoryCache` directly rather than routing through `CacheMetricsService`. Migration of existing cache calls to the metrics-instrumented service is a planned improvement (see [CACHING_STRATEGY.md](CACHING_STRATEGY.md) for roadmap).

---

## CachedLookupService

**File:** `Application/Services/Infrastructure/CachedLookupService.cs`

**Interface:** `ICachedLookupService`

**Lifetime:** Scoped

### Purpose

Provides cached lookups for frequently accessed entity IDs, particularly PokerManager IDs. Used by balance calculation services and the profit pipeline to quickly determine whether a given `BaseAssetHolderId` belongs to a poker manager (and, by extension, whether RakeOverrideCommission balance rules apply).

### Interface

```csharp
public interface ICachedLookupService
{
    Task<HashSet<Guid>> GetPokerManagerIdsAsync();
    Task<bool> IsPokerManagerAsync(Guid baseAssetHolderId);
    void InvalidatePokerManagerCache();
}
```

### Methods

#### `GetPokerManagerIdsAsync()`

Returns a cached `HashSet<Guid>` of all active (non-deleted) PokerManager `BaseAssetHolderId` values.

- **Cache Key:** `"lookups.poker-manager-ids"`
- **TTL:** 10 minutes
- **Query:** Uses `AsNoTracking()` for read-only performance

```csharp
var managerIds = await _context.PokerManagers
    .AsNoTracking()
    .Where(pm => !pm.DeletedAt.HasValue)
    .Select(pm => pm.BaseAssetHolderId)
    .ToListAsync();

var idSet = managerIds.ToHashSet();
_cache.Set(PokerManagerCacheKey, idSet, PokerManagerCacheDuration);
```

The `HashSet<Guid>` provides O(1) lookup performance for the `IsPokerManagerAsync` method.

#### `IsPokerManagerAsync(Guid baseAssetHolderId)`

Checks if a given `BaseAssetHolderId` belongs to a poker manager. Delegates to `GetPokerManagerIdsAsync()` and performs a `Contains` check on the cached set.

```csharp
public async Task<bool> IsPokerManagerAsync(Guid baseAssetHolderId)
{
    var ids = await GetPokerManagerIdsAsync();
    return ids.Contains(baseAssetHolderId);
}
```

#### `InvalidatePokerManagerCache()`

Removes the cached poker manager ID set, forcing the next call to re-query the database.

```csharp
public void InvalidatePokerManagerCache()
{
    _cache.Remove(PokerManagerCacheKey);
}
```

Should be called when poker managers are created or deleted.

### Consumers

- **Balance calculation services** — Identify RakeOverrideCommission managers for settlement balance dual-impact rules
- **`BaseAssetHolderService`** — Determine entity type for balance grouping decisions
- **Transaction services** — Identify manager-side wallets in rate fee attribution

---

## AssetHolderDomainService

**File:** `Application/Services/Domain/AssetHolderDomainService.cs`

**Interface:** `IAssetHolderDomainService`

**Lifetime:** Scoped

### Purpose

Domain service encapsulating business logic for asset holder operations. Handles cross-entity validation, deletion eligibility checks, and entity-specific creation validation.

### Key Methods

#### Deletion Eligibility

```csharp
Task<bool> CanDeleteAssetHolder(Guid assetHolderId)
```

Returns `true` only if the asset holder has no active transactions and no active asset pools. Prevents orphan data.

#### Entity Type Detection

```csharp
Task<AssetHolderType> DetermineAssetHolderType(Guid assetHolderId)
```

Queries the `BaseAssetHolder` with all entity-type navigation properties (`Client`, `Bank`, `Member`, `PokerManager`) to determine the concrete type.

#### Validation Methods

Each entity type has a dedicated validation method:

| Method | Entity | Additional Validations |
|--------|--------|----------------------|
| `ValidateClientCreation` | Client | Birthday range (not future, not >150 years ago) |
| `ValidateBankCreation` | Bank | Code required (1–10 chars), uniqueness check |
| `ValidateMemberCreation` | Member | Share range (0–100), birthday validation |
| `ValidatePokerManagerCreation` | PokerManager | Base validations only (name: 2–40 chars) |

All validation methods return a `DomainValidationResult` with error codes following the pattern `REQUIRED_FIELD`, `MIN_LENGTH`, `MAX_LENGTH`, `FUTURE_DATE`, `INVALID_RANGE`, `DUPLICATE_CODE`, etc.

#### Balance Calculation

```csharp
Task<decimal> GetTotalBalance(Guid assetHolderId)
```

Calculates the total balance across all transaction types (fiat, digital, settlement) by summing signed amounts for all wallets belonging to the asset holder.

#### Active Transaction Check

```csharp
Task<bool> HasActiveTransactions(Guid assetHolderId)
```

Checks all three transaction tables for any non-deleted transactions involving the asset holder's wallets. Used by `CanDeleteAssetHolder` and can be used independently for pre-operation checks.

---

## Service Registration

**File:** `Api/Configuration/DependencyInjectionExtensions.cs`

All application services are registered through extension methods on `WebApplicationBuilder`.

### Registration Methods

#### `AddStandardServices()`

Framework services: controllers, API explorer, Swagger, API versioning.

```csharp
builder.AddStandardServices();
```

#### `AddAuthServices()`

Authentication and authorization: Auth0 settings, JWT Bearer, authorization policies, role/permission handlers.

```csharp
builder.AddAuthServices();
```

Registers:
- `IAuth0UserService` (Scoped)
- `IAuthorizationHandler` for roles and permissions (Scoped)
- JWT Bearer authentication scheme
- All `Role:*` and `Permission:*` authorization policies

#### `AddScopedServices()`

All application-level services organized by category:

```csharp
builder.AddScopedServices();
```

| Category | Registered Services | Lifetime |
|----------|-------------------|----------|
| **Domain** | `IAssetHolderDomainService` → `AssetHolderDomainService` | Scoped |
| **Entities** | `BankService`, `ClientService`, `MemberService`, `PokerManagerService`, `AddressService`, `ContactPhoneService`, `InitialBalanceService` | Scoped |
| **Assets** | `AssetPoolService`, `AssetPoolValidationService`, `WalletIdentifierService` | Scoped |
| **Transactions** | `FiatAssetTransactionService`, `DigitalAssetTransactionService`, `SettlementTransactionService`, `ImportedTransactionService`, `TransferService` | Scoped |
| **Support** | `ReferralService`, `ClientReferralService`, `CategoryService` | Scoped |
| **Finance** | `IAvgRateService` → `AvgRateService`, `IProfitCalculationService` → `ProfitCalculationService` | Scoped |
| **Infrastructure (Caching)** | `ICacheMetricsService` → `CacheMetricsService` | **Singleton** |
| **Infrastructure (Lookups)** | `ICachedLookupService` → `CachedLookupService` | Scoped |
| **Logging** | `ILoggingService` → `LoggingService` | Scoped |

> **Note:** `CacheMetricsService` is registered as **Singleton** because it tracks cumulative statistics across all requests. `CachedLookupService` is **Scoped** because it depends on `DataContext` (which is scoped to the request).

#### `AddHealthCheckServices()`

Health check infrastructure with SQL Server connectivity check.

```csharp
builder.AddHealthCheckServices();
```

#### `AddRateLimitServices()`

IP-based rate limiting using `AspNetCoreRateLimit`.

```csharp
builder.AddRateLimitServices();
```

Registers: `IIpPolicyStore`, `IRateLimitCounterStore`, `IRateLimitConfiguration`, `IProcessingStrategy` (all Singleton).

### Registration Pattern

Entity services use dual registration — both as their concrete type and through their base class:

```csharp
builder.Services.AddScoped<BaseAssetHolderService<Client>, ClientService>();
builder.Services.AddScoped<ClientService>();
```

This enables injection via either the base class (in generic controllers) or the concrete class (in specialized services).

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [CACHING_STRATEGY.md](CACHING_STRATEGY.md) | Cache key inventory, TTLs, invalidation triggers, and roadmap |
| [RATE_LIMITING_AND_PERFORMANCE.md](RATE_LIMITING_AND_PERFORMANCE.md) | Rate limiting configuration and caching implementation |
| [CONFIGURATION_MANAGEMENT.md](CONFIGURATION_MANAGEMENT.md) | Application settings and dependency management |
| [SERVICE_LAYER_ARCHITECTURE.md](../02_ARCHITECTURE/SERVICE_LAYER_ARCHITECTURE.md) | Service layer patterns (BaseService, BaseAssetHolderService) |
| [VALIDATION_SYSTEM.md](VALIDATION_SYSTEM.md) | FluentValidation and service-level validation |
| [AUTHENTICATION.md](AUTHENTICATION.md) | Auth0 integration and RBAC system |

---

*Created: February 27, 2026*
