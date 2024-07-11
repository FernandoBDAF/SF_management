using SFManagement.Data;
using SFManagement.Models;

namespace SFManagement.Services
{
    public class BankService : BaseService<Bank>
    {
        public BankService(DataContext context) : base(context)
        {
        }
    }
}
