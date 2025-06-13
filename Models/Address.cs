using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFManagement.Models;

public class Address : BaseDomain
{

    [MaxLength(30)] public string? StreetAddress { get; set; }

    public int? Number { get; set; }
    
    [MaxLength(20)] public string? City { get; set; }

    [MaxLength(20)] public string? State { get; set; }
    
    [MaxLength(20)] public string? Country { get; set; }
    
    // ["xxxxx-xxx"]
    [Required] [MaxLength(10)] public string Postcode { get; set; }

    [MaxLength(20)] public string? Complement { get; set; }
    
    [Required] [ForeignKey("BaseAssetHolder")] public Guid? BaseAssetHolderId { get; set; }

}