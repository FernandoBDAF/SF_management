using SFManagement.Domain.Common;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SFManagement.Domain.Enums;
using SFManagement.Domain.Entities.AssetHolders;
using SFManagement.Domain.Enums.Assets;

namespace SFManagement.Domain.Entities.Support;

/// <summary>
/// Represents the initial balance for a BaseAssetHolder and acts as configuration
/// for how balances are calculated.
///
/// DUAL PURPOSE:
/// 1. Provides starting balance for calculations
/// 2. Configures calculation mode:
///    - AssetGroup set (AssetType = None): consolidate all assets in the group
///    - AssetType set (AssetGroup = None): track this asset individually
///
/// MUTUAL EXCLUSIVITY:
/// - A BaseAssetHolder cannot have both AssetGroup and AssetType InitialBalances
///   for the same AssetGroup.
///
/// AVGRATE (Spread Managers only):
/// - If ConversionRate > 0 and BalanceAs is set, it becomes the starting AvgRate
/// - If ConversionRate is null/0, AvgRate starts at 0 but Balance still applies
/// </summary>
public class InitialBalance : BaseDomain
{
    /// <summary>
    /// The initial balance amount in asset units.
    /// Can be negative to represent initial debts or adjustments.
    /// For consolidated groups, this is the sum of all assets in the group.
    /// </summary>
    [Precision(18, 2)] 
    [Required] 
    public decimal Balance { get; set; }
    
    /// <summary>
    /// The asset type this balance represents (e.g., BRL, USD, BTC).
    /// This is the unit of the balance.
    /// Must be AssetType.None when AssetGroup is set.
    /// </summary>
    [Required] 
    public AssetType AssetType { get; set; }
    
    /// <summary>
    /// Optional conversion rate used as the starting AvgRate for Spread managers.
    /// When set, BalanceAs must also be set.
    /// </summary>
    [Precision(18, 4)] 
    public decimal? ConversionRate { get; set; }
    
    /// <summary>
    /// Optional target asset type for financial valuation (e.g., BRL).
    /// Required when ConversionRate is set.
    /// </summary>
    public AssetType? BalanceAs { get; set; }

    /// <summary>
    /// The asset group for consolidated tracking.
    /// When set, all AssetTypes in this group are consolidated into a single balance.
    /// Must be AssetGroup.None when AssetType is set.
    /// </summary>
    public AssetGroup AssetGroup { get; set; }
    
    /// <summary>
    /// The BaseAssetHolder this initial balance belongs to
    /// </summary>
    [Required] 
    public Guid BaseAssetHolderId { get; set; }
    public virtual BaseAssetHolder BaseAssetHolder { get; set; } = null!;
    
    /// <summary>
    /// Gets the effective balance in the target asset type or asset group
    /// If AssetGroup is set, the balance is the sum of the balances of all AssetTypes in the group
    /// If AssetType is set, the balance is the balance of the asset type
    /// </summary>
    public decimal EffectiveBalance 
    {
        get
        {
            // Validate that AssetGroup and AssetType are not both set
            if (AssetGroup != 0 && AssetType != 0)
            {
                throw new InvalidOperationException("AssetGroup and AssetType cannot be set at the same time");
            }
            
            return Balance;
        }
    }
    
    /// <summary>
    /// Checks if this initial balance is currently active (not soft deleted)
    /// </summary>
    public bool IsActive => !DeletedAt.HasValue;
}