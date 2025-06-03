using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SFManagement.Models
{
    public class ClosingNickname : BaseDomain
    {
        public ClosingNickname() { }

        public ClosingNickname(Nickname nickname)
        {
            NicknameId = nickname.Id;
            Balance = decimal.Zero;
            Rake = decimal.Zero;
            Rakeback = decimal.Zero;
            FatherNicknameId = null;
            FatherPercentual = decimal.Zero;
        }

        [ForeignKey("Nickname")]
        public Guid NicknameId { get; set; }

        public virtual Nickname Nickname { get; set; }

        public Guid ClosingManagerId { get; set; }

        public virtual ClosingManager ClosingManager { get; set; }

        [Precision(18, 2)]
        public decimal Balance { get; set; }

        [Precision(18, 2)]
        public decimal Rake { get; set; }

        [Precision(18, 2)]
        public decimal Rakeback { get; set; }

        public Guid? FatherNicknameId { get; set; }

        [Precision(18, 2)]
        public decimal FatherPercentual { get; set; }
    }
}
