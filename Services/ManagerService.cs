using SFManagement.Data;
using SFManagement.Models;

namespace SFManagement.Services
{
    public class ManagerService : BaseService<Manager>
    {
        public ManagerService(DataContext context) : base(context)
        {
        }
    }
}
