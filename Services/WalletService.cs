using SFManagement.Data;
using SFManagement.Models;

namespace SFManagement.Services
{
    public class WalletService : BaseService<Wallet>
    {
        public WalletService(DataContext context) : base(context)
        {
        }
    }
}
