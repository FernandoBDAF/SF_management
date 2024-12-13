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
        private readonly TransactionService _transactionService;

        public ClientController(ClientService service, ILogger<ClientController> logger, IMapper mapper, ClientService clientService, TransactionService transactionService) : base(service, mapper)
        {
            _logger = logger;
            _clientService = clientService;
            _transactionService = transactionService;
        }

        [HttpGet]
        [Route("balance/{clientId}")]
        public async Task<BalanceResponse> Balance(Guid clientId) => await _clientService.GetBalance(clientId);

        [HttpGet]
        [Route("transactions/{clientId}/{startDate?}/{endDate?}/{quantity?}/{page?}")]
        public async Task<TableResponse<TransactionResponse>> Transactions(Guid clientId, DateTime? startDate = null, DateTime? endDate = null, int quantity = 100, int page = 0) => await _transactionService.GetTransactions(clientId, startDate, endDate, quantity, page);

        [HttpGet]
        [Route("transactions/{clientId}/{quantity?}/{page?}")]
        public async Task<TableResponse<TransactionResponse>> Transactions(Guid clientId, int quantity = 100, int page = 0) => await _transactionService.GetTransactions(clientId, null, null, quantity, page);

        [HttpPut]
        [Route("initial-balance/{clientId}")]
        public async Task<ClientResponse> UpdateInitialValue(Guid clientId, ClientRequest request) => await _clientService.UpdateInitialValue(clientId, request);
    }
}
