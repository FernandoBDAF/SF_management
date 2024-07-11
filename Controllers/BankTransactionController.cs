using Microsoft.AspNetCore.Mvc;
using SFManagement.Models;
using SFManagement.Services;

namespace SFManagement.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BankTransactionController : BaseApiController<BankTransaction>
    {
        public BankTransactionController(BaseService<BankTransaction> service) : base(service)
        {
        }
    }
}
