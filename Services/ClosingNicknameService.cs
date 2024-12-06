using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models;

namespace SFManagement.Services
{
    public class ClosingNicknameService : BaseService<ClosingNickname>
    {
        public ClosingNicknameService(DataContext context) : base(context)
        {
        }
            
        public async Task<List<ClosingNickname>> GetByClosingManagerId(Guid closingManagerId) => await _entity.Where(x => !x.DeletedAt.HasValue && x.ClosingManagerId == closingManagerId).ToListAsync();
    }
}
