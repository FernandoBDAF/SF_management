using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models;

namespace SFManagement.Services
{
    public class ClosingWalletService : BaseService<ClosingWallet>
    {
        public ClosingWalletService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context, httpContextAccessor)
        {
        }

        public async Task<List<ClosingWallet>> GetByClosingManagerId(Guid closingManagerId) => await _entity.Where(x => !x.DeletedAt.HasValue && x.ClosingManagerId == closingManagerId).Include(x => x.Wallet).ToListAsync();

        public override async Task<ClosingWallet> Update(Guid id, ClosingWallet obj)
        {
            var existing = await context.ClosingWallets.FirstOrDefaultAsync(x => x.Id == id);

            if(existing == null)
            {
                throw new AppException("Not found closing wallet.");
            }

            existing.ReturnRake = obj.ReturnRake;

            context.ClosingWallets.Update(existing);

            await context.SaveChangesAsync();

            return existing;
        }
    }
}
