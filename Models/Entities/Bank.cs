using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SFManagement.Interfaces;
using SFManagement.Models.Transactions;

namespace SFManagement.Models.Entities;

public class Bank : BaseDomain, IAssetHolder
{
    [Required] public Guid BaseAssetHolderId { get; set; }
    
    public virtual BaseAssetHolder? BaseAssetHolder { get; set; }
    
    [Required] [MaxLength(10)] [Column(TypeName = "varchar(10)")] public string Code { get; set; }
}