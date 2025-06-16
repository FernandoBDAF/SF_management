using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;
using SFManagement.Models.Closing;
using SFManagement.Models.Transactions;

namespace SFManagement.Models.Entities;

public class PokerManager : BaseAssetHolder
{
    // public ManagerType ManagerType { get; set; }
    
    // this will be removed and replaced by cache
    // public virtual ICollection<AvgRate> AvgRates { get; set; } = new HashSet<AvgRate>();

    public virtual ICollection<Excel> Excels { get; set; } = new HashSet<Excel>();
    
    public virtual ICollection<InitialBalance> InitialBalances { get; set; } = new HashSet<InitialBalance>();
    
    public virtual ICollection<ContactPhone> PhonesNumbers { get; set; } = new HashSet<ContactPhone>();
    
    // public virtual ICollection<AssetWallet> AssetWallets { get; set; } = new HashSet<AssetWallet>();
    //
    // public virtual ICollection<WalletIdentifier> WalletIdentifiers { get; set; } =  new HashSet<WalletIdentifier>();
}