using SFManagement.Application.DTOs.Common;
using SFManagement.Application.DTOs.Assets;
using SFManagement.Domain.Enums;
using SFManagement.Domain.Enums.Assets;

namespace SFManagement.Application.DTOs.CompanyAssets;

/// <summary>
/// Response model for company-owned asset pools
/// </summary>
public class CompanyAssetPoolResponse : BaseResponse
{
    public AssetGroup AssetGroup { get; set; }
    
    /// <summary>
    /// Always "Company" for company pools
    /// </summary>
    public string OwnerName => "Company";
    
    /// <summary>
    /// Always null for company pools
    /// </summary>
    public Guid? BaseAssetHolderId => null;
    
    /// <summary>
    /// Current balance of the pool
    /// </summary>
    public decimal CurrentBalance { get; set; }
    
    /// <summary>
    /// Number of active wallet identifiers in this pool
    /// </summary>
    public int WalletIdentifierCount { get; set; }
    
    /// <summary>
    /// Total number of transactions involving this pool
    /// </summary>
    public int TransactionCount { get; set; }
    
    /// <summary>
    /// Date when the pool was created
    /// </summary>
    public new DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Last transaction date for this pool
    /// </summary>
    public DateTime? LastTransactionDate { get; set; }
    
    /// <summary>
    /// Description of the pool purpose
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Business justification for this pool
    /// </summary>
    public string? BusinessJustification { get; set; }
    
    /// <summary>
    /// List of wallet identifiers in this pool
    /// </summary>
    public List<WalletIdentifierResponse> WalletIdentifiers { get; set; } = new();
} 