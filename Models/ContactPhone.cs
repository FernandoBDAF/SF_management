using System.ComponentModel.DataAnnotations;

namespace SFManagement.Models;

public class ContactPhone : BaseDomain
{
    [Required] public int CountryCode { get; set; }
    public int? LocalCode { get; set; }
    [Required] [MaxLength(20)] public string PhoneNumber { get; set; }
    public string? SearchFor { get; set; }
    [Required] public Guid BaseAssetHolderId { get; set; }
    
}