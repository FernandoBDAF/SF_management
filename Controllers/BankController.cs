using Microsoft.AspNetCore.Mvc;
using SFManagement.Models;
using SFManagement.Services;

namespace SFManagement.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BankController : BaseApiController<Bank>
    {
        public BankController(BaseService<Bank> service) : base(service)
        {
        }
    }
}
