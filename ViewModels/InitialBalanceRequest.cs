using System.ComponentModel.DataAnnotations;
using SFManagement.Enums;

namespace SFManagement.ViewModels;

/// <summary>
/// Request model for creating or updating initial balances
/// </summary>
public class InitialBalanceRequest
{
    /// <summary>
    /// The BaseAssetHolder this initial balance belongs to
    /// </summary>
    [Required]
    public Guid BaseAssetHolderId { get; set; }
    
    /// <summary>
    /// The asset type this balance represents (e.g., BRL, USD, BTC)
    /// Either AssetType or AssetGroup must be specified, but not both
    /// </summary>
    public AssetType AssetType { get; set; }
    
    /// <summary>
    /// The asset group this balance represents (sum of all AssetTypes in the group)
    /// Either AssetType or AssetGroup must be specified, but not both
    /// </summary>
    public AssetGroup AssetGroup { get; set; }
    
    /// <summary>
    /// The initial balance amount (must be non-negative)
    /// </summary>
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Balance must be non-negative")]
    public decimal Balance { get; set; }
    
    /// <summary>
    /// Optional target asset type for conversion to financial purposes
    /// Only applicable when AssetType is specified
    /// </summary>
    public AssetType? BalanceAs { get; set; }
    
    /// <summary>
    /// Optional conversion rate for financial purposes
    /// Only applicable when AssetType is specified
    /// </summary>
    [Range(0.0001, double.MaxValue, ErrorMessage = "ConversionRate must be positive")]
    public decimal? ConversionRate { get; set; }
    
    /// <summary>
    /// Optional description or reason for this initial balance
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }
}