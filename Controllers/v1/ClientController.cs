using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Enums;
using SFManagement.Models;
using SFManagement.Models.Entities;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers.v1;

[ApiController]
[Route("api/v{verion:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class ClientController : BaseApiController<Client, ClientRequest, ClientResponse>
{
    private readonly ClientService _clientService;
    private readonly BalanceService _balanceService;
    private readonly ILogger<ClientController> _logger;
    private readonly TransactionService _transactionService;

    public ClientController(ClientService service, ILogger<ClientController> logger, IMapper mapper,
        ClientService clientService, TransactionService transactionService, BalanceService balanceService) : base(service, mapper)
    {
        _logger = logger;
        _clientService = clientService;
        _transactionService = transactionService;
        _balanceService = balanceService;
    }

    [HttpGet]
    [Route("balance/{clientId}")]
    public async Task<Dictionary<AssetType,decimal>> Balance(Guid clientId)
    {
        return await _clientService.GetBalancesByAssetType(clientId);
    }

    [HttpPost]
    [Route("balance/{clientId}")]
    public async Task<BalanceResponse> Balance(Guid clientId, BalanceRequest request)
    {
        return await _clientService.GetBalance(clientId, request.Date);
    }

    [HttpGet]
    [Route("transactions/{clientId}/{startDate?}/{endDate?}/{quantity?}/{page?}")]
    public async Task<TableResponse<TransactionResponse>> Transactions(Guid clientId, DateTime? startDate = null,
        DateTime? endDate = null, int quantity = 100, int page = 0)
    {
        return await _transactionService.GetTransactions(clientId, startDate, endDate, quantity, page);
    }

    [HttpGet]
    [Route("transactions/{clientId}/{quantity?}/{page?}")]
    public async Task<TableResponse<TransactionResponse>> Transactions(Guid clientId, int quantity = 100, int page = 0)
    {
        return await _transactionService.GetTransactions(clientId, null, null, quantity, page);
    }

    [HttpPut]
    [Route("initial-balance/{clientId}")]
    public async Task<ClientResponse> UpdateInitialValue(Guid clientId, ClientRequest request)
    {
        return await _clientService.UpdateInitialValue(clientId, request);
    }
}