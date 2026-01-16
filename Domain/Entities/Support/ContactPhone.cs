using SFManagement.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace SFManagement.Domain.Entities.Support;

public class ContactPhone : BaseDomain
{
    public int? CountryCode { get; set; }
    
    public int? AreaCode { get; set; }

    public int? PhoneNumber { get; set; }
    
    [Required] [MaxLength(20)] public string InputPhoneNumber { get; set; } = string.Empty;
    
    [MaxLength(30)] public string? SearchFor { get; set; }
    
    [Required] public Guid BaseAssetHolderId { get; set; }
}
