using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SFManagement.Models;

public class Wallet : BaseDomain
{
    [Precision(18, 2)] public decimal IntialCoins { get; set; }

    [Precision(18, 2)] public decimal InitialValue { get; set; }

    [Precision(18, 2)] public decimal InitialExchangeRate { get; set; }

    [ForeignKey("Manager")] public Guid ManagerId { get; set; }

    public string? Name { get; set; }

    public virtual Manager Manager { get; set; }

    public virtual List<Nickname> Nicknames { get; set; } = new();

    public virtual List<WalletTransaction> Transactions { get; set; } = new();

    public virtual List<ClosingWallet> ClosingWallets { get; set; } = new();

    public virtual List<InternalTransaction> InternalTransactions { get; set; } = new();
}