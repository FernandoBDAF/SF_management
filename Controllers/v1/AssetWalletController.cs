using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Models;
using SFManagement.Models.Entities;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers.v1;

[ApiController]
[Route("api/v{verion:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class AssetWalletController : BaseApiController<AssetWallet, AssetWalletRequest, AssetWalletResponse>
{

    public AssetWalletController(BaseService<AssetWallet> service, IMapper mapper) : base(service, mapper)
    {
    }

    // [HttpGet]
    // [Route("balance/{walletId}")]
    // public async Task<BalanceResponse> Balance(Guid walletId)
    // {
    //     return await _assetWalletService.GetBalance(walletId);
    // }

    // [HttpGet]
    // [Route("transactions/{walletId}/{startDate?}/{endDate?}/{quantity?}/{page?}")]
    // public async Task<TableResponse<TransactionResponse>> Transactions(Guid walletId, DateTime? startDate = null,
    //     DateTime? endDate = null, int? quantity = 100, int? page = 0)
    // {
    //     return await _transactionService.GetWalletTransactions(walletId, startDate, endDate, quantity.Value,
    //         page.Value);
    // }

    // [HttpGet]
    // [Route("transactions/{walletId}/{quantity?}/{page?}")]
    // public async Task<TableResponse<TransactionResponse>> Transactions(Guid walletId, int? quantity = 100,
    //     int? page = 0)
    // {
    //     return await _transactionService.GetWalletTransactions(walletId, null, null, quantity.Value, page.Value);
    // }
}