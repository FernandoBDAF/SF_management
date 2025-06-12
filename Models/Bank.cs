using Microsoft.EntityFrameworkCore;
using SFManagement.Models.Transactions;

namespace SFManagement.Models;

public class Bank : BaseDomain
{
    public string? Code { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<BankTransaction> BankTransactions { get; set; } = new List<BankTransaction>();

    public virtual ICollection<InternalTransaction> InternalTransactions { get; set; } =
        new List<InternalTransaction>();

    [Precision(18, 2)] public decimal InitialValue { get; set; }
}