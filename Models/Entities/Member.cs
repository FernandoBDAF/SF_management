using System.ComponentModel.DataAnnotations.Schema;

namespace SFManagement.Models.Entities;

public class Member : BaseAssetHolder
{
    public double Share { get; set; }
    
    public DateTime? Birthday { get; set; }
    
    public virtual ICollection<ContactPhone> ContactPhones { get; set; } = new HashSet<ContactPhone>();
    
    public virtual ICollection<InitialBalance> InitialBalances { get; set; } = new HashSet<InitialBalance>();
}