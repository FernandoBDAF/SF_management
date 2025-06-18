using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SFManagement.Models.Transactions;

namespace SFManagement.Models.Entities;

public class Bank : BaseAssetHolder
{
    [Required] public int Code { get; set; }
    
    public virtual ICollection<Ofx> Ofxs { get; set; }
    
    // public virtual ICollection<InitialBalance> InitialBalances { get; set; } = new HashSet<InitialBalance>();
    //
    // public virtual ICollection<ContactPhone> ContactPhones { get; set; } = new HashSet<ContactPhone>();
}