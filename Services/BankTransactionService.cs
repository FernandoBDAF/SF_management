using SFManagement.Data;
using SFManagement.Models;

namespace SFManagement.Services
{
    public class BankTransactionService : BaseService<BankTransaction>
    {
        public BankTransactionService(DataContext context) : base(context)
        {
        }
    }
}
