using System.ComponentModel.DataAnnotations;

namespace SFManagement.Models.Support;

public class ContactPhone : BaseDomain
{
    public int? CountryCode { get; set; }
    
    public int? LocalCode { get; set; }
    
    [Required] [MaxLength(20)] public string PhoneNumber { get; set; }
    
    [MaxLength(30)] public string? SearchFor { get; set; }
    
    [Required] public Guid BaseAssetHolderId { get; set; }
}