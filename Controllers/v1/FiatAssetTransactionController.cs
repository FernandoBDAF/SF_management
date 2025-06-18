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
    FiatAssetTransactionController : BaseApiController<FiatAssetTransaction, FiatAssetTransactionRequest, FiatAssetTransactionResponse>
{
    private readonly FiatAssetTransactionService _fiatAssetTransactionService;
    private readonly IMapper _mapper;

    public FiatAssetTransactionController(BaseService<FiatAssetTransaction> service, IMapper mapper,
        FiatAssetTransactionService fiatAssetTransactionService) : base(service, mapper)
    {
        _fiatAssetTransactionService = fiatAssetTransactionService;
        _mapper = mapper;
    }

    // [HttpGet]
    // public override async Task<List<FiatAssetTransactionResponse>> Get()
    // {
    //     return _mapper.Map<List<FiatAssetTransactionResponse>>(await _fiatAssetTransactionService.List());
    // }

    [HttpPut]
    [Route("approve/{bankTransactionId}")]
    public async Task<FiatAssetTransactionResponse> Approve(Guid bankTransactionId,
        [FromBody] BankTransactionApproveRequest model)
    {
        return _mapper.Map<FiatAssetTransactionResponse>(await _fiatAssetTransactionService.Approve(bankTransactionId, model));
    }

    [HttpPut]
    [Route("unapprove/{bankTransactionId}")]
    public async Task<FiatAssetTransactionResponse> Unapprove(Guid bankTransactionId)
    {
        return _mapper.Map<FiatAssetTransactionResponse>(await _fiatAssetTransactionService.Unapprove(bankTransactionId));
    }

    [HttpPut]
    [Route("link/{fromBankTransactionId}/{toBankTransactionId}")]
    public async Task<(FiatAssetTransactionResponse from, FiatAssetTransactionResponse to)> Link(Guid fromBankTransactionId,
        Guid toBankTransactionId)
    {
        return _mapper.Map<(FiatAssetTransactionResponse from, FiatAssetTransactionResponse to)>(
            await _fiatAssetTransactionService.Link(fromBankTransactionId, toBankTransactionId));
    }

    [HttpGet]
    [Route("list/{clientId}/{bankId}")]
    public async Task<List<FiatAssetTransactionResponse>> ListByClienteId(Guid? clientId, Guid? bankId)
    {
        return _mapper.Map<List<FiatAssetTransactionResponse>>(
            await _fiatAssetTransactionService.ListByClientIdAndBankId(clientId, bankId));
    }
}