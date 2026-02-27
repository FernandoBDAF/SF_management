using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SFManagement.Application.DTOs.Infrastructure;

namespace SFManagement.Application.Services.Infrastructure;

public class CacheMetricsService : ICacheMetricsService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CacheMetricsService> _logger;
    private readonly ConcurrentDictionary<string, CacheEntryStats> _stats = new();

    public CacheMetricsService(IMemoryCache cache, ILogger<CacheMetricsService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

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

    public void Remove(string key, string category)
    {
        _cache.Remove(key);
        _logger.LogDebug("Cache REMOVE [{Category}] {Key}", category, key);
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

    private sealed class CacheEntryStats
    {
        public long Hits;
        public long Misses;
    }
}
