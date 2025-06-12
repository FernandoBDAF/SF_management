using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SFManagement.Models.Transactions;

namespace SFManagement.Models;

public class Bank : BaseDomain
{
    [Required] public int Code { get; set; }

    [MaxLength(20)]
    [Required]
    public string Name { get; set; }

    public virtual ICollection<BankTransaction> BankTransactions { get; set; } = new List<BankTransaction>();
    
    public virtual ICollection<Ofx> Ofx { get; set; } = new List<Ofx>();

    public virtual ICollection<InternalTransaction> InternalTransactions { get; set; } =
        new List<InternalTransaction>();

    [Precision(18, 2)] public decimal? InitialValue { get; set; }
}