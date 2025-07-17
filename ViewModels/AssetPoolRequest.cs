using SFManagement.Enums;
using System.ComponentModel.DataAnnotations;
using SFManagement.Enums.AssetInfrastructure;

namespace SFManagement.ViewModels;

/// <summary>
/// Request model for creating asset holder-owned asset pools
/// This model requires BaseAssetHolderId to prevent accidental company pool creation
/// For company pools, use CompanyAssetPoolRequest instead
/// </summary>
public class AssetPoolRequest
{
    /// <summary>
    /// Required BaseAssetHolderId - prevents accidental company pool creation
    /// For company pools, use the dedicated CompanyAssetPoolController
    /// </summary>
    [Required(ErrorMessage = "BaseAssetHolderId is required. For company pools, use the CompanyAssetPoolController.")]
    public Guid BaseAssetHolderId { get; set; }
    
    [Required]
    public AssetGroup AssetGroup { get; set; }
}