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

        public async Task<List<IGrouping<Guid,ClosingNickname>>> GetByClosingManagerId(Guid closingManagerId) => await _entity
            .Where(x => !x.DeletedAt.HasValue && x.ClosingManagerId == closingManagerId)
            .Include(x => x.Nickname)
            .GroupBy(x => x.Nickname.WalletId)
            .ToListAsync();
    }
}
