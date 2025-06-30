using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Models.Transactions;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers.v1;

[ApiController]
[Route("api/v{verion:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class
    FiatAssetTransactionController : BaseApiController<FiatAssetTransaction, FiatAssetTransactionRequest, FiatAssetTransactionResponse>
{
    private readonly FiatAssetTransactionService _fiatAssetTransactionService;
    private readonly IMapper _mapper;
    private readonly BankService _bankService;

    public FiatAssetTransactionController(FiatAssetTransactionService service,
        BankService bankService ,IMapper mapper) : base(service, mapper)
    {
        _fiatAssetTransactionService = service;
        _mapper = mapper;
        _bankService = bankService;
    }
    
    [HttpGet]
    [Route("bank-transactions")]
    public async Task<TableResponse<FiatAssetTransactionResponse>> BankTransactions([FromQuery] int? quantity, [FromQuery] int? page)
    {
        var bankAssetWalletIds = await _bankService.GetBankAssetWalletIds();

        if (bankAssetWalletIds.Length == 0)
        {
            return new TableResponse<FiatAssetTransactionResponse>
            {
                Data = [],
                Total = 0
            };
        }

        var transactions = await _fiatAssetTransactionService
            .GetAssetHolderTransactions(bankAssetWalletIds, null, null, quantity ?? 100, page ?? 0);
         
        return _mapper.Map<TableResponse<FiatAssetTransactionResponse>>(transactions);
    }
    
    [HttpGet]
    [Route("direct-transactions")]
    public async Task<TableResponse<FiatAssetTransactionResponse>> DirectTransactions([FromQuery] int? quantity, [FromQuery] int? page)
    {
        var bankAssetWalletIds = await _bankService.GetBankAssetWalletIds();

        var transactions = await _fiatAssetTransactionService
            .GetNonAssetHolderTransactions(bankAssetWalletIds,
            null,null, quantity ?? 100, page ?? 0);
        
        return _mapper.Map<TableResponse<FiatAssetTransactionResponse>>(transactions);
    }

    // [HttpGet]
    // public override async Task<List<FiatAssetTransactionResponse>> Get()
    // {
    //     return _mapper.Map<List<FiatAssetTransactionResponse>>(await _fiatAssetTransactionService.List());
    // }

    // [HttpPut]
    // [Route("approve/{bankTransactionId}")]
    // public async Task<FiatAssetTransactionResponse> Approve(Guid bankTransactionId,
    //     [FromBody] BankTransactionApproveRequest model)
    // {
    //     return _mapper.Map<FiatAssetTransactionResponse>(await _fiatAssetTransactionService.Approve(bankTransactionId, model));
    // }
    //
    // [HttpPut]
    // [Route("unapprove/{bankTransactionId}")]
    // public async Task<FiatAssetTransactionResponse> Unapprove(Guid bankTransactionId)
    // {
    //     return _mapper.Map<FiatAssetTransactionResponse>(await _fiatAssetTransactionService.Unapprove(bankTransactionId));
    // }
    //
    // [HttpPut]
    // [Route("link/{fromBankTransactionId}/{toBankTransactionId}")]
    // public async Task<(FiatAssetTransactionResponse from, FiatAssetTransactionResponse to)> Link(Guid fromBankTransactionId,
    //     Guid toBankTransactionId)
    // {
    //     return _mapper.Map<(FiatAssetTransactionResponse from, FiatAssetTransactionResponse to)>(
    //         await _fiatAssetTransactionService.Link(fromBankTransactionId, toBankTransactionId));
    // }
    //
    // [HttpGet]
    // [Route("list/{clientId}/{bankId}")]
    // public async Task<List<FiatAssetTransactionResponse>> ListByClienteId(Guid? clientId, Guid? bankId)
    // {
    //     return _mapper.Map<List<FiatAssetTransactionResponse>>(
    //         await _fiatAssetTransactionService.ListByClientIdAndBankId(clientId, bankId));
    // }
}