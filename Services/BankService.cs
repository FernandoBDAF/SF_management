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

            return new BalanceResponse(bank.BankTransactions, bank.InternalTransactions, bank.InitialValue);
        }


        public override async Task<Bank> Update(Guid id, Bank obj)
        {
            var existing = await context.Banks.FirstOrDefaultAsync(x => x.Id == id);

            if(existing == null)
            {
                throw new AppException("Not found bank");
            }

            existing.InitialValue = obj.InitialValue;
            existing.Code = obj.Code;
            existing.Name = obj.Name;

            context.Banks.Update(existing);

            await context.SaveChangesAsync();

            return obj;
        }
    }
}
