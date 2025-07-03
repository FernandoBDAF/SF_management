using System.ComponentModel.DataAnnotations;

namespace SFManagement.Models.Support;

public class Address : BaseDomain
{
    [MaxLength(30)] public string? StreetAddress { get; set; }
    
    [MaxLength(20)] public string? City { get; set; }

    [MaxLength(20)] public string? State { get; set; }
    
    [MaxLength(20)] public string? Country { get; set; }
    
    [Required] [MaxLength(10)] public string Postcode { get; set; } = string.Empty;

    [MaxLength(30)] public string? Complement { get; set; }
    
    [Required] public Guid BaseAssetHolderId { get; set; }
}