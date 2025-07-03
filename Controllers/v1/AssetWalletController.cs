using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Models.AssetInfrastructure;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers.v1;

[ApiController]
[Route("api/v{verion:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class AssetWalletController : BaseApiController<AssetWallet, AssetWalletRequest, AssetWalletResponse>
{
    private readonly AssetWalletService _service;
    private readonly IMapper _mapper;
    public AssetWalletController(AssetWalletService service, IMapper mapper) : base(service, mapper)
    {
        _service = service;
        _mapper = mapper;
    }
    
    [HttpGet]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, NoStore = false)]
    [Route("asset-holder/{assetHolderId}")]
    public async Task<List<AssetWalletResponse>> GetAssetWallets(Guid assetHolderId)
    {
        var assetWallets = await _service.GetAssetWallets(assetHolderId);
        return _mapper.Map<List<AssetWalletResponse>>(assetWallets);
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