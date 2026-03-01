# Rate Limiting and Performance

## Overview

The SF Management API implements rate limiting and caching strategies to ensure optimal performance and prevent abuse.

---

## Rate Limiting

### Configuration

Rate limiting is configured in `appsettings.json`:

```json
{
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": true,
    "StackBlockedRequests": false,
    "RealIPHeader": "X-Real-IP",
    "ClientIdHeader": "X-ClientId",
    "HttpStatusCode": 429,
    "GeneralRules": [
      {
        "Endpoint": "*",
        "Period": "1m",
        "Limit": 100
      }
    ]
  }
}
```

### Registration

```csharp
// DependencyInjectionExtensions.cs
public static void AddRateLimitServices(this WebApplicationBuilder builder)
{
    builder.Services.Configure<IpRateLimitOptions>(
        builder.Configuration.GetSection("IpRateLimiting"));
    builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
    builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
    builder.Services.AddInMemoryRateLimiting();
}
```

---

## Response Caching

### Endpoint-Level Caching

```csharp
[ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, NoStore = false)]
public virtual async Task<IActionResult> GetBalance(Guid id)
```

### Cache Durations

| Duration | Endpoints |
|----------|-----------|
| 60 sec | Balance, wallet connections |
| 300 sec | Company pool summary |
| 600 sec | Company pool analytics |

---

## Query Optimization

### AsNoTracking for Read Operations

```csharp
return await query.AsNoTracking()
    .Where(x => !x.BaseAssetHolder.DeletedAt.HasValue)
    .ToListAsync();
```

### Selective Includes

Only include necessary navigation properties:

```csharp
.Include(x => x.Category)
.Include(x => x.SenderWalletIdentifier)
    .ThenInclude(wi => wi.AssetPool)
.ToListAsync();
```

---

## In-Memory Caching (IMemoryCache)

### Overview

The application uses `IMemoryCache` for caching expensive calculations and frequently-accessed lookup data. This is registered in `Program.cs`:

```csharp
builder.Services.AddMemoryCache();
```

### Current Cache Implementations

| Service | Cache Key | TTL | Purpose |
|---------|-----------|-----|---------|
| `AvgRateService` | `AvgRate:{ManagerId}:{Year}:{Month}` | 24 hours | Monthly AvgRate snapshots |
| `ProfitCalculationService` | `finance.system-wallet-ids` | 10 minutes | System wallet IDs for profit calculation |
| `CachedLookupService` | `lookups.poker-manager-ids` | 10 minutes | PokerManager ID lookups |
| `CacheMetricsService` | (wrapper) | Configurable | Cache with automatic hit/miss metrics |

### AvgRateService Caching

The AvgRate system caches monthly snapshots for completed months:

```csharp
// Cache key pattern
private string GetCacheKey(Guid managerId, int year, int month) 
    => $"AvgRate:{managerId}:{year}:{month}";

// Only cache completed (past) months
if (!IsCurrentMonth(year, month))
{
    _cache.Set(cacheKey, snapshot, TimeSpan.FromHours(24));
}
```

**Invalidation Strategy:**
- When a transaction affects a PokerManager, `InvalidateFromDate()` is called
- All cached months from the transaction date to present are invalidated
- Called from `TransferService` and `DigitalAssetTransactionService`

### ProfitCalculationService Caching

System wallet IDs are cached to avoid repeated queries:

```csharp
// Optimized query with direct join (no Include)
var walletIds = await (
    from wallet in _context.WalletIdentifiers.AsNoTracking()
    join pool in _context.AssetPools.AsNoTracking()
        on wallet.AssetPoolId equals pool.Id
    where !wallet.DeletedAt.HasValue
          && !pool.DeletedAt.HasValue
          && pool.AssetGroup == AssetGroup.Flexible
          && pool.BaseAssetHolderId == null
    select wallet.Id
).ToListAsync();

_cache.Set(SystemWalletCacheKey, walletIds, TimeSpan.FromMinutes(10));
```

### Query Optimization Notes

**Problem:** Navigation property filtering with `Include()` can cause slow queries on large datasets.

**Solution:** Use direct LINQ joins instead:

```csharp
// Slow: Include + navigation property filter
await _context.WalletIdentifiers
    .Include(w => w.AssetPool)
    .Where(w => w.AssetPool.AssetGroup == AssetGroup.Flexible)
    .ToListAsync();

// Fast: Direct join
await (
    from wallet in _context.WalletIdentifiers
    join pool in _context.AssetPools on wallet.AssetPoolId equals pool.Id
    where pool.AssetGroup == AssetGroup.Flexible
    select wallet
).ToListAsync();
```

---

## Infrastructure Caching Services

### CacheMetricsService

**File:** `Application/Services/Infrastructure/CacheMetricsService.cs`

The `CacheMetricsService` provides a wrapper around `IMemoryCache` with automatic hit/miss tracking and statistics.

#### Interface

```csharp
public interface ICacheMetricsService
{
    Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan duration,
        string category);
    
    void Remove(string key, string category);
    CacheStatisticsResponse GetStatistics();
}
```

#### Features

| Feature | Description |
|---------|-------------|
| **Automatic Metrics** | Tracks hits/misses per category |
| **Thread-Safe** | Uses `ConcurrentDictionary` for statistics |
| **Debug Logging** | Logs cache HIT/MISS events at Debug level |
| **Statistics API** | Exposes hit rates via `GetStatistics()` |

#### Usage Example

```csharp
var data = await _cacheMetrics.GetOrCreateAsync(
    key: "system-wallet-ids",
    factory: async () => await QuerySystemWalletIds(),
    duration: TimeSpan.FromMinutes(10),
    category: "SystemWallets"
);
```

#### Statistics Response

```json
{
  "categories": {
    "SystemWallets": {
      "hits": 150,
      "misses": 10,
      "hitRate": 0.9375
    },
    "AvgRate": {
      "hits": 500,
      "misses": 50,
      "hitRate": 0.909
    }
  }
}
```

#### Monitoring Endpoint

```
GET /api/v1/diagnostics/cache-stats
Authorization: Role:admin
```

Returns cache statistics for all tracked categories.

---

### CachedLookupService

**File:** `Application/Services/Infrastructure/CachedLookupService.cs`

The `CachedLookupService` provides cached lookups for frequently accessed entity type information.

#### Interface

```csharp
public interface ICachedLookupService
{
    Task<HashSet<Guid>> GetPokerManagerIdsAsync();
    Task<bool> IsPokerManagerAsync(Guid baseAssetHolderId);
    void InvalidatePokerManagerCache();
}
```

#### Methods

| Method | TTL | Description |
|--------|-----|-------------|
| `GetPokerManagerIdsAsync()` | 10 minutes | Returns all PokerManager BaseAssetHolder IDs |
| `IsPokerManagerAsync(Guid)` | Uses cached IDs | Fast O(1) check if entity is a PokerManager |
| `InvalidatePokerManagerCache()` | - | Manual cache invalidation |

#### Implementation Details

```csharp
public async Task<HashSet<Guid>> GetPokerManagerIdsAsync()
{
    if (_cache.TryGetValue(PokerManagerCacheKey, out HashSet<Guid>? cachedIds))
    {
        _logger.LogDebug("PokerManager cache hit");
        return cachedIds ?? new HashSet<Guid>();
    }

    _logger.LogDebug("PokerManager cache miss");

    var managerIds = await _context.PokerManagers
        .AsNoTracking()
        .Where(pm => !pm.DeletedAt.HasValue)
        .Select(pm => pm.BaseAssetHolderId)
        .ToListAsync();

    var idSet = managerIds.ToHashSet();
    _cache.Set(PokerManagerCacheKey, idSet, TimeSpan.FromMinutes(10));
    return idSet;
}
```

#### Usage Pattern

```csharp
// Fast entity type check
var isPokerManager = await _cachedLookupService.IsPokerManagerAsync(assetHolderId);

if (isPokerManager)
{
    // Apply PokerManager-specific logic (AvgRate, spread calculation, etc.)
}
```

#### Cache Invalidation

Call `InvalidatePokerManagerCache()` when:
- A new PokerManager is created
- A PokerManager is deleted
- PokerManager `BaseAssetHolderId` changes (rare)

---

## Best Practices

1. **Use AsNoTracking** for read-only queries
2. **Limit includes** to required navigation properties
3. **Paginate** large result sets
4. **Cache** expensive calculations
5. **Index** frequently queried columns
6. **Use direct joins** instead of Include + navigation filter for large datasets
7. **Cache lookup data** that rarely changes (e.g., system wallet IDs, manager IDs)

---

## Related Documentation

- [API_REFERENCE.md](../06_API/API_REFERENCE.md) - Endpoint documentation
- [SERVICE_LAYER_ARCHITECTURE.md](../02_ARCHITECTURE/SERVICE_LAYER_ARCHITECTURE.md) - Query patterns
- [CACHING_STRATEGY.md](CACHING_STRATEGY.md) - Future caching plan

