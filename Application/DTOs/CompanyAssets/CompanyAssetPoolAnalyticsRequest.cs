using SFManagement.Application.DTOs.Common;
using SFManagement.Application.DTOs.Assets;
using System.ComponentModel.DataAnnotations;

namespace SFManagement.Application.DTOs.CompanyAssets;

/// <summary>
/// Request model for company asset pool analytics by period
/// </summary>
public class CompanyAssetPoolAnalyticsRequest
{
    /// <summary>
    /// Year for the analytics (required)
    /// </summary>
    [Required]
    [Range(2020, 2050, ErrorMessage = "Year must be between 2020 and 2050")]
    public int Year { get; set; }
    
    /// <summary>
    /// Month for the analytics (optional, if not provided returns yearly data)
    /// </summary>
    [Range(1, 12, ErrorMessage = "Month must be between 1 and 12")]
    public int? Month { get; set; }
    
    /// <summary>
    /// Include transaction details in the response
    /// </summary>
    public bool IncludeTransactions { get; set; } = true;
    
    /// <summary>
    /// Maximum number of transactions to include per pool
    /// </summary>
    [Range(1, 1000, ErrorMessage = "Transaction limit must be between 1 and 1000")]
    public int TransactionLimit { get; set; } = 100;
} 