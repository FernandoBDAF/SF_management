using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SFManagement.Models.Entities;

namespace SFManagement.Models.Transactions;

public class Ofx : BaseDomain
{
    public Ofx()
    {
    }

    public Ofx(List<OfxTransaction> transactions, Guid bankId, string fileName)
    {
        BankId = bankId;
        FileName = fileName;
        OfxTransactions = transactions;
    }

    [Required] [ForeignKey("Bank")] public Guid BankId { get; set; }

    public virtual Bank? Bank { get; set; } = new();

    [Required] [MaxLength(20)] public string FileName { get; set; }

    public ICollection<OfxTransaction> OfxTransactions { get; set; } = new HashSet<OfxTransaction>();
}