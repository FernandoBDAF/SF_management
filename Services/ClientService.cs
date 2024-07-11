using SFManagement.Data;
using SFManagement.Models;

namespace SFManagement.Services
{
    public class ClientService : BaseService<Client>
    {
        public ClientService(DataContext context) : base(context)
        {
        }
    }
}
