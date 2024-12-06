using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models;

namespace SFManagement.Services
{
    public class ClosingWalletService : BaseService<ClosingWallet>
    {
        public ClosingWalletService(DataContext context) : base(context)
        {
        }

        public async Task<List<ClosingWallet>> GetByClosingManagerId(Guid closingManagerId) => await _entity.Where(x => !x.DeletedAt.HasValue && x.ClosingManagerId == closingManagerId).ToListAsync();
    }
}
