# Caching Strategy

> **Status:** Mostly Implemented  
> **Created:** January 27, 2026  
> **Updated:** January 28, 2026  
> **Purpose:** Caching strategy documentation for heavy queries

> **Note:** Most planned caching is implemented. `CacheMetricsService` exists but current caches use `IMemoryCache` directly, so metrics are not collected.

---

## Overview

This document outlines the caching strategy for SF Management, including current implementations and planned extensions to improve performance of database-intensive operations.

---

## Current Caching Implementation

### Infrastructure

```csharp
// Program.cs
builder.Services.AddMemoryCache();
```

All caching uses `IMemoryCache` (in-process memory cache). This is suitable for the current scale but can be migrated to Redis/Azure Cache if distributed caching is needed.

### Active Cache Implementations

| Service | Cache Key | TTL | Purpose |
|---------|-----------|-----|---------|
| `AvgRateService` | `AvgRate:{ManagerId}:{Year}:{Month}` | 24 hours | Monthly AvgRate snapshots for completed months |
| `AvgRateService` | `avgrate.manager-wallet-ids:{managerId}` | 10 minutes | Manager wallet IDs to avoid repeated queries |
| `AvgRateService` | `avgrate.initial-balance:{managerId}` | 10 minutes | InitialBalance lookups for starting AvgRate |
| `AvgRateService` | `avgrate.first-month:{managerId}:{year}:{month}` | 10 minutes | First month detection results |
| `ProfitCalculationService` | `finance.system-wallet-ids` | 10 minutes | System wallet IDs for profit calculation |
| `ProfitCalculationService` | `profit.rake-manager-ids` | 10 minutes | RakeOverrideCommission manager IDs |
| `ProfitCalculationService` | `profit.spread-manager-ids` | 10 minutes | Spread manager IDs |
| `CachedLookupService` | `lookups.poker-manager-ids` | 10 minutes | All PokerManager IDs for quick lookups |

### Invalidation Strategy

**AvgRateService:**
- Invalidates on transaction creation/update affecting PokerManager wallets
- Cascade invalidation from affected month to present
- Called from `TransferService` and `DigitalAssetTransactionService`

**ProfitCalculationService:**
- Time-based expiry only (10 minutes)
- System wallets rarely change; short TTL handles edge cases

---

## Planned Caching Extensions

### Phase 1: High Priority (Frequent Calls, Rarely Changes)

#### 1.1 Manager Wallet IDs in AvgRateService

**File:** `Application/Services/Finance/AvgRateService.cs`  
**Method:** `GetPokerAssetWalletIds(Guid pokerManagerId)`

**Problem:** Called 3+ times per calculation, queries with `.Include()` each time.

**Solution:**
```csharp
private const string ManagerWalletsCacheKeyPrefix = "avgrate.manager-wallet-ids:";

private async Task<List<Guid>> GetPokerAssetWalletIds(Guid pokerManagerId)
{
    var cacheKey = $"{ManagerWalletsCacheKeyPrefix}{pokerManagerId}";
    
    if (_cache.TryGetValue(cacheKey, out List<Guid>? cached))
    {
        _logger.LogDebug("Cache HIT for manager wallet IDs: {ManagerId}", pokerManagerId);
        return cached ?? new List<Guid>();
    }
    
    _logger.LogDebug("Cache MISS for manager wallet IDs: {ManagerId}", pokerManagerId);
    
    // Use optimized direct join (not Include)
    var walletIds = await (
        from wallet in _context.WalletIdentifiers.AsNoTracking()
        join pool in _context.AssetPools.AsNoTracking()
            on wallet.AssetPoolId equals pool.Id
        where !wallet.DeletedAt.HasValue
              && !pool.DeletedAt.HasValue
              && pool.BaseAssetHolderId == pokerManagerId
              && pool.AssetGroup == AssetGroup.PokerAssets
        select wallet.Id
    ).ToListAsync();
    
    _cache.Set(cacheKey, walletIds, TimeSpan.FromMinutes(10));
    return walletIds;
}
```

**Cache Key:** `avgrate.manager-wallet-ids:{managerId}`  
**TTL:** 10 minutes  
**Invalidation:** When wallets are created/deleted for a manager

---

#### 1.2 PokerManager ID Lookups

**Files:** `BaseAssetHolderService.cs`, `DigitalAssetTransactionService.cs`, `TransferService.cs`

**Problem:** Multiple services query `PokerManagers.Select(pm => pm.BaseAssetHolderId)` to check if an asset holder is a manager. Called repeatedly in balance calculations.

**Solution:** Create a shared lookup cache helper:

```csharp
// New service: ICachedLookupService
public interface ICachedLookupService
{
    Task<HashSet<Guid>> GetPokerManagerIdsAsync();
    Task<HashSet<Guid>> GetManagerIdsByProfitTypeAsync(ManagerProfitType profitType);
    void InvalidatePokerManagerCache();
}
```

**Cache Key:** `lookups.poker-manager-ids`  
**TTL:** 10 minutes  
**Invalidation:** When PokerManagers are created/deleted

---

#### 1.3 Manager Profit Type Lookups

**File:** `Application/Services/Finance/ProfitCalculationService.cs`  
**Methods:** `CalculateRakeCommission()`, `CalculateSpreadProfit()`

**Problem:** Queries `PokerManagers.Where(m => m.ManagerProfitType == X)` multiple times per profit calculation.

**Solution:**
```csharp
private const string RakeManagersCacheKey = "profit.rake-manager-ids";
private const string SpreadManagersCacheKey = "profit.spread-manager-ids";

private async Task<List<Guid>> GetManagerIdsByProfitType(ManagerProfitType profitType)
{
    var cacheKey = profitType == ManagerProfitType.RakeOverrideCommission 
        ? RakeManagersCacheKey 
        : SpreadManagersCacheKey;
    
    if (_cache.TryGetValue(cacheKey, out List<Guid>? cached))
    {
        return cached ?? new List<Guid>();
    }
    
    var managerIds = await _context.PokerManagers
        .AsNoTracking()
        .Where(m => !m.DeletedAt.HasValue && m.ManagerProfitType == profitType)
        .Select(m => m.BaseAssetHolderId)
        .ToListAsync();
    
    _cache.Set(cacheKey, managerIds, TimeSpan.FromMinutes(10));
    return managerIds;
}
```

**Cache Keys:** `profit.rake-manager-ids`, `profit.spread-manager-ids`  
**TTL:** 10 minutes  
**Invalidation:** When PokerManager profit type is changed

---

### Phase 2: Cache Metrics & Monitoring

#### 2.1 Cache Wrapper Service

Create a centralized cache wrapper with metrics:

```csharp
public interface ICacheMetricsService
{
    Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan duration, string category);
    void Remove(string key, string category);
    CacheStatistics GetStatistics();
}

public class CacheMetricsService : ICacheMetricsService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CacheMetricsService> _logger;
    private readonly ConcurrentDictionary<string, CacheEntryStats> _stats = new();

    public async Task<T?> GetOrCreateAsync<T>(
        string key, 
        Func<Task<T>> factory, 
        TimeSpan duration, 
        string category)
    {
        var stats = _stats.GetOrAdd(category, _ => new CacheEntryStats());
        
        if (_cache.TryGetValue(key, out T? cached))
        {
            Interlocked.Increment(ref stats.Hits);
            _logger.LogDebug("Cache HIT [{Category}] {Key}", category, key);
            return cached;
        }
        
        Interlocked.Increment(ref stats.Misses);
        _logger.LogDebug("Cache MISS [{Category}] {Key}", category, key);
        
        var value = await factory();
        _cache.Set(key, value, duration);
        
        return value;
    }

    public CacheStatistics GetStatistics()
    {
        return new CacheStatistics
        {
            Categories = _stats.ToDictionary(
                kvp => kvp.Key,
                kvp => new CategoryStats
                {
                    Hits = kvp.Value.Hits,
                    Misses = kvp.Value.Misses,
                    HitRate = kvp.Value.Hits + kvp.Value.Misses > 0
                        ? (double)kvp.Value.Hits / (kvp.Value.Hits + kvp.Value.Misses)
                        : 0
                })
        };
    }
}
```

#### 2.2 Cache Statistics Endpoint

```csharp
[HttpGet("cache-stats")]
[Authorize(Roles = "Admin")]
public IActionResult GetCacheStatistics([FromServices] ICacheMetricsService cacheMetrics)
{
    return Ok(cacheMetrics.GetStatistics());
}
```

**Response Example:**
```json
{
  "categories": {
    "avgrate": { "hits": 1250, "misses": 45, "hitRate": 0.965 },
    "system-wallets": { "hits": 890, "misses": 12, "hitRate": 0.987 },
    "manager-lookups": { "hits": 450, "misses": 8, "hitRate": 0.983 }
  }
}
```

---

### Phase 3: Medium Priority

| Service | Method | Cache Key | TTL | Notes |
|---------|--------|-----------|-----|-------|
| `WalletIdentifierService` | `GetInternalWallets()` | `wallets.internal` | 10min | System wallets rarely change |
| `AssetPoolService` | `GetCompanyAssetPoolByGroup()` | `pools.company:{group}` | 10min | Company pools are static |
| `WalletIdentifierService` | `GetByAssetHolderAndAssetType()` | `wallets:{holderId}:{type}` | 5min | Called frequently in transfers |

---

## Cache Invalidation Triggers

```
┌─────────────────────────────────────────────────────────────────┐
│                    INVALIDATION TRIGGERS                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  PokerManager CRUD  ───► Invalidate:                            │
│                         • poker-manager-ids                     │
│                         • rake-manager-ids                      │
│                         • spread-manager-ids                    │
│                                                                 │
│  Wallet CRUD       ───► Invalidate:                             │
│                         • manager-wallet-ids:{managerId}        │
│                         • system-wallet-ids                     │
│                         • wallets.internal                      │
│                                                                 │
│  AssetPool CRUD    ───► Invalidate:                             │
│                         • pools.company:{group}                 │
│                         • Related wallet caches                 │
│                                                                 │
│  Transaction CUD   ───► Invalidate:                             │
│                         • AvgRate:{managerId}:{year}:{month}    │
│                         • (cascade to subsequent months)        │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## Implementation Checklist

| Step | Task | Priority | Complexity | Status |
|------|------|----------|------------|--------|
| 1 | System wallet ID caching | High | Low | ✅ Done |
| 2 | AvgRate snapshot caching | High | Medium | ✅ Done |
| 3 | Manager wallet IDs caching | High | Low | ✅ Done |
| 4 | PokerManager ID lookups | High | Low | ✅ Done (CachedLookupService) |
| 5 | Manager profit type lookups | High | Low | ✅ Done |
| 6 | Create ICacheMetricsService | Medium | Medium | ✅ Done (exists but not used) |
| 7 | Add cache statistics endpoint | Medium | Low | ✅ Done |
| 8 | Migrate existing caches to metrics service | Low | Medium | ⬜ Not Done |
| 9 | Internal wallet caching | Low | Low | ⬜ Not Done |
| 10 | InitialBalance caching | High | Low | ✅ Done |
| 11 | First month detection caching | High | Low | ✅ Done |

---

## Best Practices

1. **Use direct LINQ joins** instead of `Include()` + navigation filter for cached queries
2. **Keep TTL short** for data that can change (5-10 minutes)
3. **Use longer TTL** for static/rarely changing data (24+ hours)
4. **Log cache hits/misses** for monitoring
5. **Implement proper invalidation** when source data changes
6. **Use `HashSet<Guid>`** for ID lookups (O(1) contains check)
7. **Avoid caching large datasets** - cache IDs, not full entities

---

## Related Documentation

- [RATE_LIMITING_AND_PERFORMANCE.md](RATE_LIMITING_AND_PERFORMANCE.md) - Current caching implementation
- [SERVICE_LAYER_ARCHITECTURE.md](../02_ARCHITECTURE/SERVICE_LAYER_ARCHITECTURE.md) - Service patterns
- [FINANCE_MODULE_IMPLEMENTATION_PLAN_BACKEND.md](../10_REFACTORING/FINANCE_MODULE_IMPLEMENTATION_PLAN_BACKEND.md) - Finance module details

---

*Last Updated: January 27, 2026*
