using SFManagement.Application.DTOs.Common;
using SFManagement.Application.DTOs.Assets;
using SFManagement.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using SFManagement.Domain.Enums.Assets;

namespace SFManagement.Application.DTOs.CompanyAssets;

/// <summary>
/// Request model for creating company-owned asset pools
/// </summary>
public class CompanyAssetPoolRequest
{
    [Required]
    public AssetGroup AssetGroup { get; set; }
    
    /// <summary>
    /// Optional description for the company pool purpose
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Initial balance for the pool (optional)
    /// </summary>
    public decimal? InitialBalance { get; set; }
    
    /// <summary>
    /// Business justification for creating this company pool
    /// </summary>
    [MaxLength(1000)]
    public string? BusinessJustification { get; set; }
} 