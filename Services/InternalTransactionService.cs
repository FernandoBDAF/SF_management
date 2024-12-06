using SFManagement.Data;
using SFManagement.Models;

namespace SFManagement.Services
{
    public class InternalTransactionService : BaseService<InternalTransaction>
    {
        public InternalTransactionService(DataContext context) : base(context)
        {
        }
    }
}
