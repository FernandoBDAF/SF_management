using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models;

namespace SFManagement.Services
{
    public class WalletService : BaseService<Wallet>
    {
        public WalletService(DataContext context) : base(context)
        {
        }

        public async Task<List<Wallet>> GetWalletsByManagerId(Guid managerId)
        {
            return await context.Wallets.Where(x => x.ManagerId == managerId).ToListAsync();
        }
    }
}
