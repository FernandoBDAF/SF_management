using System.ComponentModel.DataAnnotations.Schema;

namespace SFManagement.Models.Entities;

public class Member : BaseAssetHolder
{
    public double Share { get; set; }
    
    public DateTime? Birthday { get; set; }
    
    public virtual ICollection<ContactPhone> PhonesNumbers { get; set; } = new HashSet<ContactPhone>();
    
    public virtual ICollection<InitialBalance> InitialBalances { get; set; } = new HashSet<InitialBalance>();
    
    public virtual ICollection<AssetWallet> AssetWallets { get; set; } = new HashSet<AssetWallet>();
    
    public virtual ICollection<WalletIdentifier> WalletIdentifiers { get; set; } =  new HashSet<WalletIdentifier>();
}