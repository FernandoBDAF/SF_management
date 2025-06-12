using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;
using SFManagement.Models.Closing;
using SFManagement.Models.Transactions;

namespace SFManagement.Models;

public class Manager : BaseDomain
{
    public string? Name { get; set; }

    public ManagerType ManagerType { get; set; }

    public virtual ICollection<Wallet> Wallets { get; set; } = new HashSet<Wallet>();

    public virtual ICollection<Excel> Excels { get; set; } = new HashSet<Excel>();

    public virtual ICollection<ClosingManager> ClosingManagers { get; set; } = new HashSet<ClosingManager>();
    
    public virtual ICollection<BankTransaction> BankTransactions { get; set; } = new HashSet<BankTransaction>();

    public virtual ICollection<WalletTransaction> WalletTransactions { get; set; } = new HashSet<WalletTransaction>();

    public virtual ICollection<InternalTransaction> InternalTransactions { get; set; } = new HashSet<InternalTransaction>();

    public virtual ICollection<AvgRate> AvgRates { get; set; } = new HashSet<AvgRate>();

    [Precision(18, 2)] public decimal InitialValue { get; set; }

    [Precision(18, 2)] public decimal InitialExchangeRate { get; set; }

    [Precision(18, 2)] public decimal InitialCoins { get; set; }
}