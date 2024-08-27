using System.ComponentModel.DataAnnotations.Schema;

namespace SFManagement.Models
{
    public class Excel : BaseDomain
    {
        [ForeignKey("Wallet")]
        public Guid WalletId { get; set; }

        public virtual Wallet Wallet { get; set; }

        public ICollection<WalletTransaction> WalletTransactions { get; set; } = new HashSet<WalletTransaction>();
    }
}
