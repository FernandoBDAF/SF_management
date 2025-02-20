using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models;
using SFManagement.ViewModels;

namespace SFManagement.Services
{
    public class ManagerService : BaseService<Manager>
    {
        public ManagerService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context,
            httpContextAccessor)
        {
        }

        public async Task<BalanceResponse> GetBalance(Guid managerId, DateTime? date)
        {
            var now = DateTime.Now;
            if (!date.HasValue || date.Value.Year == 1)
            {
                date = now;
            }

            var manager = (await context.Managers.Include(x => x.BankTransactions).Include(x => x.Wallets)
                .ThenInclude(x => x.Transactions).Include(x => x.InternalTransactions)
                .Include(x => x.WalletTransactions).Include(x => x.ClosingManagers)
                .FirstOrDefaultAsync(x => x.Id == managerId));
            var avgRate = await context.AvgRates.FirstOrDefaultAsync(x =>
                x.Date.Month <= date.Value.Month && !x.DeletedAt.HasValue && x.ManagerId == manager.Id);
            return new BalanceResponse(manager, avgRate, date);
        }

        public async Task<ProfitResponse> GetProfit(Guid managerId, DateTime? start, DateTime? end)
        {
            var manager = (await context.Managers.Include(x => x.BankTransactions).Include(x => x.ClosingManagers)
                .ThenInclude(x => x.InternalTransactions).Include(x => x.Wallets).ThenInclude(x => x.Transactions)
                .Include(x => x.InternalTransactions).Include(x => x.WalletTransactions)
                .FirstOrDefaultAsync(x => x.Id == managerId));

            if (start.HasValue && end.HasValue)
            {
                return new ProfitResponse(manager, start.Value, end.Value);
            }
            else
            {
                return new ProfitResponse(manager);
            }
        }
    }
}