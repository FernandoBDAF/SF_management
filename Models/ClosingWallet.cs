using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SFManagement.Models
{
    public class ClosingWallet : BaseDomain
    {
        [ForeignKey("ClosingManager")]
        public Guid ClosingManagerId { get; set; }

        public virtual ClosingManager ClosingManager { get; set; }

        [ForeignKey("Wallet")]
        public Guid WalletId { get; set; }

        public virtual Wallet Wallet { get; set; }

        [Precision(18, 2)]
        public decimal ReturnRake { get; set; }
    }
}
