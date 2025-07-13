using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;
using SFManagement.Models.Entities;

namespace SFManagement.Models.Support;

/// <summary>
/// Represents the initial balance for a BaseAssetHolder for a specific AssetType
/// This is used as the starting point for balance calculations
/// </summary>
public class InitialBalance : BaseDomain
{
    /// <summary>
    /// The initial balance amount
    /// </summary>
    [Precision(18, 2)] 
    [Required] 
    public decimal Balance { get; set; }
    
    /// <summary>
    /// The asset type this balance represents (e.g., BRL, USD, BTC)
    /// This is the unit of the balance
    /// </summary>
    [Required] 
    public AssetType BalanceUnit { get; set; }
    
    /// <summary>
    /// Optional conversion rate if the balance needs to be converted to another asset type
    /// Used when BalanceAs is different from BalanceUnit
    /// </summary>
    [Precision(18, 4)] 
    public decimal? ConversionRate { get; set; }
    
    /// <summary>
    /// Optional target asset type for conversion
    /// If specified, the balance should be converted using ConversionRate
    /// </summary>
    public AssetType? BalanceAs { get; set; }
    
    /// <summary>
    /// The BaseAssetHolder this initial balance belongs to
    /// </summary>
    [Required] 
    public Guid BaseAssetHolderId { get; set; }
    public virtual BaseAssetHolder BaseAssetHolder { get; set; }
    
    /// <summary>
    /// Optional description or reason for this initial balance
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Gets the effective balance in the target asset type
    /// If BalanceAs and ConversionRate are specified, returns converted amount
    /// Otherwise returns the original balance
    /// </summary>
    public decimal EffectiveBalance => 
        BalanceAs.HasValue && ConversionRate.HasValue 
            ? Balance * ConversionRate.Value 
            : Balance;
    
    /// <summary>
    /// Gets the asset type that should be used for balance calculations
    /// Returns BalanceAs if specified, otherwise BalanceUnit
    /// </summary>
    public AssetType EffectiveAssetType => BalanceAs ?? BalanceUnit;
    
    /// <summary>
    /// Checks if this initial balance is currently active (not soft deleted)
    /// </summary>
    public bool IsActive => !DeletedAt.HasValue;
}