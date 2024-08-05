using SFManagement.Data;
using SFManagement.Models;
using SFManagement.ViewModels;
using System.Data.Entity;

namespace SFManagement.Services
{
    public class ClientService : BaseService<Client>
    {
        public ClientService(DataContext context) : base(context)
        {
        }

        public async Task<BalanceResponse> GetBalance(Guid clientId)
        {
            var client = (await context.Clients.Include(x => x.BankTransactions).FirstOrDefaultAsync(x => x.Id == clientId));
            return new BalanceResponse(client.BankTransactions);
        }


    }
}
