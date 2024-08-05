using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Models;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ClientController : BaseApiController<Client, ClientRequest, ClientResponse>
    {
        private readonly ILogger<ClientController> _logger;
        private readonly ClientService _clientService;

        public ClientController(ClientService service, ILogger<ClientController> logger, IMapper mapper, ClientService clientService) : base(service, mapper)
        {
            _logger = logger;
            _clientService = clientService;
        }

        [HttpGet]
        [Route("balance/{id}")]
        public async Task<BalanceResponse> Balance(Guid clientId) => await _clientService.GetBalance(clientId);
    }
}
