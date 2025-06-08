using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;

namespace SFManagement.Models;

public class Manager : BaseDomain
{
    public string? Name { get; set; }

    public ManagerType ManagerType { get; set; }

    public virtual List<Wallet> Wallets { get; set; } = new();

    public virtual List<Excel> Excels { get; set; } = new();

    public virtual List<ClosingManager> ClosingManagers { get; set; } = new();

    public virtual List<BankTransaction> BankTransactions { get; set; } = new();

    public virtual List<WalletTransaction> WalletTransactions { get; set; } = new();

    public virtual List<InternalTransaction> InternalTransactions { get; set; } = new();

    public virtual List<AvgRate> AvgRates { get; set; } = new();

    [Precision(18, 2)] public decimal InitialValue { get; set; }

    [Precision(18, 2)] public decimal InitialExchangeRate { get; set; }

    [Precision(18, 2)] public decimal InitialCoins { get; set; }
}