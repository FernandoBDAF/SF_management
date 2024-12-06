using SFManagement.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFManagement.ViewModels
{
    public class ClosingManagerResponse : BaseResponse
    {
        [ForeignKey("Manager")]
        public Guid ManagerId { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public List<ClosingWalletResponse> ClosingWallets { get; set; } = new List<ClosingWalletResponse>();

        public List<ClosingNicknameResponse> ClosingNicknames { get; set; } = new List<ClosingNicknameResponse>();
    }
}
