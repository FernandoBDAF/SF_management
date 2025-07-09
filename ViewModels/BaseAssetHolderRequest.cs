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
}