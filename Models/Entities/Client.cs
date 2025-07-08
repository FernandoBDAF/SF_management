using System.ComponentModel.DataAnnotations;
using SFManagement.Interfaces;

namespace SFManagement.Models.Entities;

public class Client : BaseDomain, IAssetHolder
{
    [Required] public Guid BaseAssetHolderId { get; set; }
    
    public virtual BaseAssetHolder? BaseAssetHolder { get; set; }
    
    public DateTime? Birthday { get; set; }
}