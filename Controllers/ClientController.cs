using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Models;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClientController : BaseApiController<Client, ClientRequest, ClientResponse>
    {
        private readonly ILogger<ClientController> _logger;

        public ClientController(ClientService service, ILogger<ClientController> logger, IMapper mapper) : base(service, mapper)
        {
            _logger = logger;
        }
    }
}
