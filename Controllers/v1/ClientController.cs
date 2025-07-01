using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Enums;
using SFManagement.Models.Entities;
using SFManagement.Models.Transactions;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers.v1;

[ApiController]
[Route("api/v{verion:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class ClientController : BaseApiController<Client, ClientRequest, ClientResponse>
{
    private readonly ClientService _clientService;
    private readonly FiatAssetTransactionService _fiatAssetTransactionService;

    private readonly IMapper _mapper;

    public ClientController(ClientService service, FiatAssetTransactionService fiatAssetTransactionService, 
        IMapper mapper, ClientService clientService) : base(service, mapper)
    {
        _fiatAssetTransactionService = fiatAssetTransactionService;
        _clientService = clientService;
        _mapper = mapper;
    }

    // [HttpPut]
    // [Route("initial-balance/{clientId}")]
    // public async Task<ClientResponse> UpdateInitialValue(Guid clientId, ClientRequest request)
    // {
    //     return await _clientService.UpdateInitialValue(clientId, request);
    // }
    
    [HttpPost]
    [Route("{clientId}/send-brazilian-real")]
    public async Task<FiatAssetTransaction> SendBrazilianReais(Guid clientId, FiatAssetTransactionRequest request)
    {
        var client = await _clientService.Get(clientId);
        
        return await _fiatAssetTransactionService.SendBrazilianReais(client, request);
    }
    
    [HttpGet]
    [Route("{id}/balance")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, NoStore = false)]
    public async Task<Dictionary<AssetType,decimal>> Balance(Guid id)
    {
        return await _clientService.GetBalancesByAssetType(id);
    }
    
    [HttpGet]
    [Route("{id}/transactions")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, NoStore = false)]
    public async Task<StatementAssetHolderWithTransactions> GetAssetHolderWithTransactions(Guid id)
    {
        return await _clientService.GetAssetHolderWithTransactionsAsStatement(id);
    }
}