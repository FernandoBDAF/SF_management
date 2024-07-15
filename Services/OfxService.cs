using SFManagement.Data;
using SFManagement.Models;

namespace SFManagement.Services
{
    public class OfxService : BaseService<Ofx>
    {
        public OfxService(DataContext context) : base(context)
        {
        }
    }
}
