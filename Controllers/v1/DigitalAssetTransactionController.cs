using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Models;
using SFManagement.Models.Transactions;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers.v1;

[ApiController]
[Route("api/v{verion:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class
    DigitalAssetTransactionController : BaseApiController<DigitalAssetTransaction, DigitalAssetTransactionRequest,
    DigitalAssetTransactionResponse>
{
    private readonly IMapper _mapper;
    private readonly DigitalAssetTransactionService _digitalAssetTransactionService;
    private readonly PokerManagerService _pokerManagerService;

    public DigitalAssetTransactionController(DigitalAssetTransactionService service,
        PokerManagerService pokerManagerService, IMapper mapper) 
        : base(service, mapper)
    {
        _digitalAssetTransactionService = service;
        _mapper = mapper;
        _pokerManagerService = pokerManagerService;
    }
    
    [HttpGet]
    [Route("poker-manager-transactions")]
    public async Task<TableResponse<DigitalAssetTransactionResponse>> Transactions([FromQuery] int? quantity, [FromQuery] int? page)
    {
        var pokerManagerAssetWalletIds = await _pokerManagerService.GetPokerManagerAssetWalletIds();

        if (pokerManagerAssetWalletIds.Length == 0)
        {
            return new TableResponse<DigitalAssetTransactionResponse>
            {
                Data = [],
                Total = 0
            };
        }

        var transactions = await _digitalAssetTransactionService
            .GetAssetHolderTransactions(pokerManagerAssetWalletIds, null, null, quantity ?? 100, page ?? 0);
        
        return _mapper.Map<TableResponse<DigitalAssetTransactionResponse>>(transactions);
    }

    // [HttpPost]
    // [Route("approve/{walletTransactionId}")]
    // public async Task<DigitalAssetTransactionResponse> ApproveTransaction(Guid walletTransactionId,
    //     [FromBody] WalletTransactionApproveRequest model)
    // {
    //     return await _digitalAssetTransactionService.ApproveTransaction(walletTransactionId, model);
    // }
    //
    // [HttpPut]
    // [Route("unapprove/{walletTransactionId}")]
    // public async Task<DigitalAssetTransactionResponse> Unapprove(Guid walletTransactionId)
    // {
    //     return _mapper.Map<DigitalAssetTransactionResponse>(
    //         await _digitalAssetTransactionService.UnApproveTransaction(walletTransactionId));
    // }
    //
    // [HttpPut]
    // [Route("link/{fromWalletTransactionId}/{toWalletTransactionId}")]
    // public async Task<(DigitalAssetTransactionResponse from, DigitalAssetTransactionResponse to)> Link(Guid fromWalletTransactionId,
    //     Guid toWalletTransactionId)
    // {
    //     return _mapper.Map<(DigitalAssetTransactionResponse from, DigitalAssetTransactionResponse to)>(
    //         await _digitalAssetTransactionService.Link(fromWalletTransactionId, toWalletTransactionId));
    // }
}