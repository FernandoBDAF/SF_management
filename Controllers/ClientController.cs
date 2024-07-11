using Microsoft.AspNetCore.Mvc;
using SFManagement.Models;
using SFManagement.Services;

namespace SFManagement.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClientController : BaseApiController<Client>
    {
        private readonly ILogger<ClientController> _logger;

        public ClientController(ClientService service, ILogger<ClientController> logger) : base(service)
        {
            _logger = logger;
        }
    }
}
