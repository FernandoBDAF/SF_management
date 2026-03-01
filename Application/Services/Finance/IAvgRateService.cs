using SFManagement.Application.DTOs.Finance;
using SFManagement.Domain.Enums.Assets;

namespace SFManagement.Application.Services.Finance;

/// <summary>
/// Service for calculating and caching AvgRate (weighted average cost basis).
/// AvgRate is only required for Spread managers to calculate spread profit:
/// Spread Profit = Amount × (SaleRate - AvgRate).
/// </summary>
public interface IAvgRateService
{
    /// <summary>
    /// Get AvgRate for a specific manager at a specific date.
    /// For current month: calculates dynamically up to the date.
    /// For past months: uses cached snapshot.
    /// </summary>
    Task<decimal> GetAvgRateAtDate(Guid pokerManagerId, DateTime date);
    
    /// <summary>
    /// Get or calculate monthly snapshot.
    /// Cached for past months, calculated dynamically for current month.
    /// </summary>
    Task<AvgRateSnapshotResponse> GetAvgRateForMonth(Guid pokerManagerId, int year, int month);
    
    /// <summary>
    /// Invalidate cache from a date forward.
    /// Call when transactions are created/updated/deleted.
    /// Invalidates the affected month AND all subsequent months.
    /// </summary>
    Task InvalidateFromDate(Guid pokerManagerId, DateTime fromDate);

    /// <summary>
    /// Invalidates cached poker manager wallet IDs.
    /// Call when wallets are created/deleted for a manager.
    /// </summary>
    void InvalidateManagerWalletCache(Guid pokerManagerId);

    /// <summary>
    /// Checks if an asset holder requires AvgRate tracking.
    /// Returns true ONLY if the asset holder is a manager with ManagerProfitType = Spread.
    /// </summary>
    Task<bool> RequiresAvgRateTracking(Guid assetHolderId);

    /// <summary>
    /// Gets the calculation mode for an asset holder based on InitialBalance configuration.
    /// </summary>
    Task<AvgRateCalculationMode> GetCalculationMode(Guid assetHolderId, AssetGroup assetGroup);
}

public enum AvgRateCalculationMode
{
    None,
    PerAssetType,
    Consolidated
}
