using SFManagement.Data;
using SFManagement.Models;

namespace SFManagement.Services
{
    public class ClosingWalletService : BaseService<ClosingWallet>
    {
        public ClosingWalletService(DataContext context) : base(context)
        {
        }
    }
}
