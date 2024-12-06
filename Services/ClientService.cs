using SFManagement.Data;
using SFManagement.Models;
using SFManagement.ViewModels;
using System.Data.Entity;

namespace SFManagement.Services
{
    public class ClientService : BaseService<Client>
    {
        public ClientService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context, httpContextAccessor)
        {
        }

        public async Task<BalanceResponse> GetBalance(Guid clientId)
        {
            var client = (await context.Clients.Include(x => x.BankTransactions)
                                               .Include(x => x.WalletTransactions)
                                               .Include(x => x.InternalTransactions)
                                               .FirstOrDefaultAsync(x => x.Id == clientId));

            return new BalanceResponse(client.InitialValue, client.BankTransactions, client.WalletTransactions, client.InternalTransactions);
        }


    }
}
