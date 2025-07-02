using System.ComponentModel.DataAnnotations;
using SFManagement.Interfaces;
using SFManagement.Models.Transactions;

namespace SFManagement.Models.Entities;

public class Bank : BaseDomain, IAssetHolder<Bank>
{
    [Required] public Guid BaseAssetHolderId { get; set; }
    
    [Required] public int Code { get; set; }
    
    public virtual ICollection<Ofx> Ofxs { get; set; } = new HashSet<Ofx>();
}