using System.ComponentModel.DataAnnotations;

namespace SFManagement.Models.Support;

public class Address : BaseDomain
{
    [Required] [MaxLength(10)] public string Postcode { get; set; } = string.Empty;
    
    [Required] public Guid BaseAssetHolderId { get; set; }
}