using System.ComponentModel.DataAnnotations;
using SFManagement.Enums;
using SFManagement.Interfaces;
using SFManagement.Models.Transactions;

namespace SFManagement.Models.Entities;

public class PokerManager : BaseDomain, IAssetHolder
{
    // public ManagerType ManagerType { get; set; }
    
    [Required] public Guid BaseAssetHolderId { get; set; }
    public virtual BaseAssetHolder? BaseAssetHolder { get; set; }
    
    public ManagerType? ManagerType { get; set; }
    
    public virtual ICollection<Excel> Excels { get; set; } = new HashSet<Excel>();
}