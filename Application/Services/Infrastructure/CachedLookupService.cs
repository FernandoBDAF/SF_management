using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SFManagement.Infrastructure.Data;

namespace SFManagement.Application.Services.Infrastructure;

public class CachedLookupService : ICachedLookupService
{
    private const string PokerManagerCacheKey = "lookups.poker-manager-ids";
    private static readonly TimeSpan PokerManagerCacheDuration = TimeSpan.FromMinutes(10);

    private readonly DataContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedLookupService> _logger;

    public CachedLookupService(
        DataContext context,
        IMemoryCache cache,
        ILogger<CachedLookupService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

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
        _cache.Set(PokerManagerCacheKey, idSet, PokerManagerCacheDuration);
        return idSet;
    }

    public async Task<bool> IsPokerManagerAsync(Guid baseAssetHolderId)
    {
        var ids = await GetPokerManagerIdsAsync();
        return ids.Contains(baseAssetHolderId);
    }

    public void InvalidatePokerManagerCache()
    {
        _cache.Remove(PokerManagerCacheKey);
    }
}
