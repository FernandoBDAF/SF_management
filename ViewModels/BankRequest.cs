using System.ComponentModel.DataAnnotations;

namespace SFManagement.ViewModels;

public class BankRequest : BaseAssetHolderRequest
{
    [Required]
    [StringLength(10, MinimumLength = 1)]
    public string Code { get; set; }
}