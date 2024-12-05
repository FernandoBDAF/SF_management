using System.ComponentModel.DataAnnotations.Schema;

namespace SFManagement.Models
{
    public class ClosingManager : BaseDomain
    {
        [ForeignKey("Manager")]
        public Guid ManagerId { get; set; }

        public virtual Manager Manager { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public List<ClosingWallet> ClosingWallets { get; set; } = new List<ClosingWallet>();

        public List<ClosingNickname> ClosingNicknames { get; set; } = new List<ClosingNickname>();
    }
}
