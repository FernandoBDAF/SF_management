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
    private readonly TransactionService _transactionService;
    private readonly PokerManagerService _pokerManagerService;

    public DigitalAssetTransactionController(BaseService<DigitalAssetTransaction> service, TransactionService transactionService,
        PokerManagerService pokerManagerService, IMapper mapper, DigitalAssetTransactionService digitalAssetTransactionService) 
        : base(service, mapper)
    {
        _digitalAssetTransactionService = digitalAssetTransactionService;
        _mapper = mapper;
        _transactionService = transactionService;
        _pokerManagerService = pokerManagerService; }
    
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

        return await _transactionService.GetPokerManagerDigitalAssetTransactions(pokerManagerAssetWalletIds, null, null, quantity ?? 100, page ?? 0);
    }

    [HttpPost]
    [Route("approve/{walletTransactionId}")]
    public async Task<DigitalAssetTransactionResponse> ApproveTransaction(Guid walletTransactionId,
        [FromBody] WalletTransactionApproveRequest model)
    {
        return await _digitalAssetTransactionService.ApproveTransaction(walletTransactionId, model);
    }

    [HttpPut]
    [Route("unapprove/{walletTransactionId}")]
    public async Task<DigitalAssetTransactionResponse> Unapprove(Guid walletTransactionId)
    {
        return _mapper.Map<DigitalAssetTransactionResponse>(
            await _digitalAssetTransactionService.UnApproveTransaction(walletTransactionId));
    }

    [HttpPut]
    [Route("link/{fromWalletTransactionId}/{toWalletTransactionId}")]
    public async Task<(DigitalAssetTransactionResponse from, DigitalAssetTransactionResponse to)> Link(Guid fromWalletTransactionId,
        Guid toWalletTransactionId)
    {
        return _mapper.Map<(DigitalAssetTransactionResponse from, DigitalAssetTransactionResponse to)>(
            await _digitalAssetTransactionService.Link(fromWalletTransactionId, toWalletTransactionId));
    }
}