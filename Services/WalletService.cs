using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models;
using SFManagement.ViewModels;

namespace SFManagement.Services
{
    public class WalletService : BaseService<Wallet>
    {
        public WalletService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context, httpContextAccessor)
        {
        }

        public async Task<List<Wallet>> GetWalletsByManagerId(Guid managerId)
        {
            return await context.Wallets.Where(x => x.ManagerId == managerId).ToListAsync();
        }

        public async Task<BalanceResponse> GetBalance(Guid walletId)
        {
            var wallet = (await context.Wallets.Include(x => x.Transactions).Include(x => x.InternalTransactions).FirstOrDefaultAsync(x => x.Id == walletId));
            return new BalanceResponse(wallet, null);
        }
    }
}
