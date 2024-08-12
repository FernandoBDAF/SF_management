using SFManagement.Data;
using SFManagement.Models;

namespace SFManagement.Services
{
    public class NicknameService : BaseService<Nickname>
    {
        public NicknameService(DataContext context) : base(context)
        {
        }
    }
}
