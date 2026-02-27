namespace SFManagement.Application.DTOs.Infrastructure;

public class CacheStatistics
{
    public Dictionary<string, CategoryStats> Categories { get; init; } = new();
}

public class CategoryStats
{
    public long Hits { get; init; }
    public long Misses { get; init; }
    public double HitRate { get; init; }
}
