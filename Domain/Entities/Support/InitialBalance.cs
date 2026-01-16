using SFManagement.Domain.Common;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SFManagement.Domain.Enums;
using SFManagement.Domain.Entities.AssetHolders;
using SFManagement.Domain.Enums.Assets;

namespace SFManagement.Domain.Entities.Support;

/// <summary>
/// Represents the initial balance for a BaseAssetHolder for a specific AssetType or AssetGroup
/// This is used as the starting point for balance calculations
/// </summary>
public class InitialBalance : BaseDomain
{
    /// <summary>
    /// The initial balance amount
    /// Can be negative to represent initial debts or adjustments
    /// </summary>
    [Precision(18, 2)] 
    [Required] 
    public decimal Balance { get; set; }
    
    /// <summary>
    /// The asset type this balance represents (e.g., BRL, USD, BTC)
    /// This is the unit of the balance
    /// </summary>
    [Required] 
    public AssetType AssetType { get; set; }
    
    /// <summary>
    /// Optional conversion rate to financial purposes
    /// </summary>
    [Precision(18, 4)] 
    public decimal? ConversionRate { get; set; }
    
    /// <summary>
    /// Optional target asset type for conversion to financial purposes
    /// </summary>
    public AssetType? BalanceAs { get; set; }

    /// <summary>
    /// The balance of an AssetGroup is the sum of the balances of all AssetTypes in the group
    /// If this has a value, AssetType must be 0
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