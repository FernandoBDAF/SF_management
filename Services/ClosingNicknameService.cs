using SFManagement.Data;
using SFManagement.Models;

namespace SFManagement.Services
{
    public class ClosingNicknameService : BaseService<ClosingNickname>
    {
        public ClosingNicknameService(DataContext context) : base(context)
        {
        }
    }
}
