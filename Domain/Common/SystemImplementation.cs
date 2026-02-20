namespace SFManagement.Domain.Common;

/// <summary>
/// Central system timeline constants used by finance calculations.
/// </summary>
public static class SystemImplementation
{
    /// <summary>
    /// Earliest date considered by finance logic when searching historical data.
    /// Update this value if historical data migration extends older than current start.
    /// </summary>
    public static readonly DateTime FinanceDataStartDateUtc = new(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);
}
