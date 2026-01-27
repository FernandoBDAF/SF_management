using SFManagement.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFManagement.Domain.Entities.Support;

public class Address : BaseDomain
{
    [Required] [MaxLength(10)] [Column(TypeName = "varchar(10)")] public string Postcode { get; set; } = string.Empty;
    
    [Required] public Guid BaseAssetHolderId { get; set; }
}