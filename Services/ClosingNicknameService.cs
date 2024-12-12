using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models;

namespace SFManagement.Services
{
    public class ClosingNicknameService : BaseService<ClosingNickname>
    {
        public ClosingNicknameService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context, httpContextAccessor)
        {
        }

        public async Task<List<IGrouping<Guid,ClosingNickname>>> GetByClosingManagerId(Guid closingManagerId) => await _entity.Include(x => x.Nickname).Where(x => !x.DeletedAt.HasValue && x.ClosingManagerId == closingManagerId).GroupBy(x => x.Nickname.WalletId).ToListAsync();
    }
}
