using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SFManagement.Models.Closing;
using SFManagement.Models.Transactions;

namespace SFManagement.Models;

public class Wallet : BaseDomain
{
    [Precision(18, 2)] public decimal? InitialCoins { get; set; }

    [Precision(18, 2)] public decimal? InitialValue { get; set; }

    [Precision(18, 2)] public decimal? InitialExchangeRate { get; set; }
    
    [Required] [MaxLength(20)] public string Name { get; set; }
    
    [ForeignKey("Manager")] public Guid ManagerId { get; set; }


    public virtual ICollection<WalletTransaction> WalletTransactions { get; set; } =  new List<WalletTransaction>();
    
    // public virtual ICollection<InternalTransaction> InternalTransactions { get; set; } = new List<InternalTransaction>();
    public virtual ICollection<Nickname> Nicknames { get; set; } = new List<Nickname>();

    public virtual ICollection<ClosingWallet> ClosingWallets { get; set; } = new List<ClosingWallet>();

}