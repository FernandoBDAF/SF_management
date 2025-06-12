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
    WalletTransactionController : BaseApiController<WalletTransaction, WalletTransactionRequest,
    WalletTransactionResponse>
{
    private readonly IMapper _mapper;
    private readonly WalletTransactionService _walletTransactionService;

    public WalletTransactionController(BaseService<WalletTransaction> service, IMapper mapper,
        WalletTransactionService walletTransactionService) : base(service, mapper)
    {
        _walletTransactionService = walletTransactionService;
        _mapper = mapper;
    }

    [HttpPost]
    [Route("approve/{walletTransactionId}")]
    public async Task<WalletTransactionResponse> ApproveTransaction(Guid walletTransactionId,
        [FromBody] WalletTransactionApproveRequest model)
    {
        return await _walletTransactionService.ApproveTransaction(walletTransactionId, model);
    }

    [HttpPut]
    [Route("unapprove/{walletTransactionId}")]
    public async Task<WalletTransactionResponse> Unapprove(Guid walletTransactionId)
    {
        return _mapper.Map<WalletTransactionResponse>(
            await _walletTransactionService.UnApproveTransaction(walletTransactionId));
    }

    [HttpPut]
    [Route("link/{fromWalletTransactionId}/{toWalletTransactionId}")]
    public async Task<(WalletTransactionResponse from, WalletTransactionResponse to)> Link(Guid fromWalletTransactionId,
        Guid toWalletTransactionId)
    {
        return _mapper.Map<(WalletTransactionResponse from, WalletTransactionResponse to)>(
            await _walletTransactionService.Link(fromWalletTransactionId, toWalletTransactionId));
    }
}