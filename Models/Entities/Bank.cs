using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SFManagement.Models.Transactions;

namespace SFManagement.Models.Entities;

public class Bank : BaseAssetHolder
{
    [Required] public int Code { get; set; }
    
    public virtual ICollection<Ofx> Ofxs { get; set; } = new List<Ofx>();
    
    public virtual ICollection<InitialBalance> InitialBalances { get; set; } = new HashSet<InitialBalance>();
    
    public virtual ICollection<ContactPhone> PhonesNumbers { get; set; } = new HashSet<ContactPhone>();
    
    public virtual ICollection<Wallet> Wallets { get; set; } = new HashSet<Wallet>();
}