namespace SFManagement.Application.Services.Infrastructure;

public interface ICachedLookupService
{
    Task<HashSet<Guid>> GetPokerManagerIdsAsync();
    Task<bool> IsPokerManagerAsync(Guid baseAssetHolderId);
    void InvalidatePokerManagerCache();
}
