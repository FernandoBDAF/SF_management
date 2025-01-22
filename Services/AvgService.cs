using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models;

namespace SFManagement.Services
{
    public class AvgService : BaseService<AvgRate>
    {
        private readonly WalletTransactionService _walletTransactionService;

        public AvgService(DataContext context, IHttpContextAccessor httpContextAccessor, WalletTransactionService walletTransactionService) : base(context, httpContextAccessor)
        {
            _walletTransactionService = walletTransactionService;
        }

        public async Task Reset(Guid managerId)
        {
            var firstDate = await context.WalletTransactions.Where(x => x.ManagerId == managerId && !x.DeletedAt.HasValue).OrderBy(x => x.Date).FirstOrDefaultAsync();
            await _walletTransactionService.CalcAvgRate(await context.Managers.FirstOrDefaultAsync(x => x.Id == managerId), firstDate.Date);
        }
    }
}
