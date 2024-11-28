using System.ComponentModel.DataAnnotations.Schema;

namespace SFManagement.Models
{
    public class Excel : BaseDomain
    {
        [ForeignKey("Manager")]
        public Guid ManagerId { get; set; }

        public virtual Manager Manager { get; set; }

        public ICollection<WalletTransaction> WalletTransactions { get; set; } = new HashSet<WalletTransaction>();
    }
}
