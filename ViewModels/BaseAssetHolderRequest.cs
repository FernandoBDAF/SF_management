using System.ComponentModel.DataAnnotations;
using SFManagement.Enums;

namespace SFManagement.ViewModels;

public class BaseAssetHolderRequest
{
    public Guid? BaseAssetHolderId { get; set; }
    
    [Required]
    [StringLength(40, MinimumLength = 1)]
    public string Name { get; set; }
    
    [Required]
    public TaxEntityType TaxEntityType { get; set; }

    [Required]
    [MaxLength(20)] public string GovernmentNumber { get; set; }

    /// <summary>
    /// Optional referrer ID to establish referral relationship during creation
    /// Frontend should provide the ID of the BaseAssetHolder who is referring this one
    /// </summary>
    public Guid? ReferrerId { get; set; }
}