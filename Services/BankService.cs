using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models;
using SFManagement.ViewModels;

namespace SFManagement.Services
{
    public class BankService : BaseService<Bank>
    {
        public BankService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context, httpContextAccessor)
        {
        }

        public async Task<BalanceResponse> GetBalance(Guid bankId)
        {
            var bank = await context.Banks.Include(x => x.BankTransactions).Include(x => x.InternalTransactions).Include(x => x.InternalTransactions).FirstOrDefaultAsync(x => x.Id == bankId);

            return new BalanceResponse(bank.BankTransactions, bank.InternalTransactions);
        }
    }
}
