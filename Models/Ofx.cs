using System.ComponentModel.DataAnnotations.Schema;

namespace SFManagement.Models
{
    public class Ofx : BaseDomain
    {
        public Ofx() { }

        public Ofx(List<BankTransaction> transactions, Guid bankId)
        {
            BankId = bankId;
            BankTransactions = transactions;
        }

        [ForeignKey("Bank")]
        public Guid BankId { get; set; }

        public virtual Bank? Bank { get; set; }

        public string? FileName { get; set; }

        public byte[]? File { get; set; }

        public ICollection<BankTransaction> BankTransactions { get; set; } = new HashSet<BankTransaction>();
    }
}
