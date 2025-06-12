using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SFManagement.Models.Transactions;

namespace SFManagement.Models;

public class Client : BaseDomain
{
    public string Name { get; set; }

    public string? Phone { get; set; }

    public string? CPF { get; set; }
    
    public string? Email { get; set; }

    public DateTime? Birthday { get; set; }
    [Precision(18, 2)] public decimal? InitialValue { get; set; }
    
    [ForeignKey("Address")] public Guid? AddressId { get; set; }

    public virtual List<BankTransaction> BankTransactions { get; set; } = new();

    public virtual List<WalletTransaction> WalletTransactions { get; set; } = new();

    public virtual List<InternalTransaction> InternalTransactions { get; set; } = new();

    public virtual List<Nickname> Nicknames { get; set; } = new();

}