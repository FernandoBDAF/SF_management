using SFManagement.Data;
using SFManagement.Models;
using SFManagement.ViewModels;
using System.Data.Entity;

namespace SFManagement.Services
{
    public class BankService : BaseService<Bank>
    {
        public BankService(DataContext context) : base(context)
        {
        }

        public async Task<BalanceResponse> GetBalance(Guid bankId)
        {
            var bank = await context.Banks.Include(x => x.BankTransactions).FirstOrDefaultAsync(x => x.Id == bankId);

            return new BalanceResponse(decimal.Zero, bank.BankTransactions, new List<WalletTransaction>());
        }

        //TODO: Criar um endpoint com datas.
    }
}
