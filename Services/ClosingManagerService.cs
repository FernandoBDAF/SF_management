using SFManagement.Data;
using SFManagement.Models;

namespace SFManagement.Services
{
    public class ClosingManagerService : BaseService<ClosingManager>
    {
        public ClosingManagerService(DataContext context) : base(context)
        {
        }
    }
}
