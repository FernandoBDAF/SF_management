using System.ComponentModel.DataAnnotations;
using SFManagement.Interfaces;

namespace SFManagement.Models.Entities;

public class Member : BaseDomain, IAssetHolder<Member>
{
    [Required] public Guid BaseAssetHolderId { get; set; }
    
    public double Share { get; set; }
    
    public DateTime? Birthday { get; set; }
}