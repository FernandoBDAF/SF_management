using System.ComponentModel.DataAnnotations;

namespace SFManagement.ViewModels;

public class BaseAssetHolderRequest
{
    public Guid? BaseAssetHolderId { get; set; }
    
    [Required]
    [StringLength(40, MinimumLength = 1)]
    public string Name { get; set; }

    [StringLength(40)]
    [EmailAddress]
    public string? Email { get; set; }
    
    [StringLength(20)]
    public string? Cpf { get; set; }
    
    [StringLength(20)]
    public string? Cnpj { get; set; }
    
    /// <summary>
    /// Optional referrer ID to establish referral relationship during creation
    /// Frontend should provide the ID of the BaseAssetHolder who is referring this one
    /// </summary>
    public Guid? ReferrerId { get; set; }
}