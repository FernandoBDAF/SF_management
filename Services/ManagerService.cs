using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models;
using SFManagement.ViewModels;

namespace SFManagement.Services
{
    public class ManagerService : BaseService<Manager>
    {
        public ManagerService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context, httpContextAccessor)
        {
        }

        public async Task<BalanceResponse> GetBalance(Guid managerId)
        {
            var manager = (await context.Managers.Include(x => x.Wallets).ThenInclude(x => x.Transactions).Include(x => x.InternalTransactions).FirstOrDefaultAsync(x => x.Id == managerId));
            return new BalanceResponse(manager.Wallets, manager.InitialCoins, manager.InitialExchangeRate, manager.InitialValue);
        }
    }
}
