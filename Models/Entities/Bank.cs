using System.ComponentModel.DataAnnotations;
using SFManagement.Interfaces;
using SFManagement.Models.Transactions;

namespace SFManagement.Models.Entities;

public class Bank : BaseDomain, IAssetHolder
{
    [Required] public Guid BaseAssetHolderId { get; set; }
    
    public virtual BaseAssetHolder? BaseAssetHolder { get; set; }
    
    [Required] [MaxLength(10)] public string Code { get; set; }
}