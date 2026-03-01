using SFManagement.Application.DTOs.Infrastructure;

namespace SFManagement.Application.Services.Infrastructure;

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
