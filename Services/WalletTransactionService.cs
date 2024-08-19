using SFManagement.Data;
using SFManagement.Models;

namespace SFManagement.Services
{
    public class WalletTransactionService : BaseService<WalletTransaction>
    {
        public WalletTransactionService(DataContext context) : base(context)
        {
        }
    }
}
