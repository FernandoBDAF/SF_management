using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Api.Controllers.Base;
using SFManagement.Application.DTOs.Assets;
using SFManagement.Application.DTOs.CompanyAssets;
using SFManagement.Application.Services.Assets;
using SFManagement.Application.Services.Base;
using SFManagement.Domain.Entities.Assets;

namespace SFManagement.Api.Controllers.v1.Assets;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class AssetPoolController : BaseApiController<AssetPool, AssetPoolRequest, AssetPoolResponse>
{
    private readonly AssetPoolService _service;
    private readonly IMapper _mapper;
    public AssetPoolController(AssetPoolService service, IMapper mapper) : base(service, mapper)
    {
        _service = service;
        _mapper = mapper;
    }
    
    [HttpGet]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, NoStore = false)]
    [Route("asset-holder/{assetHolderId}")]
    public async Task<List<AssetPoolResponse>> GetAssetPools(Guid assetHolderId)
    {
        var assetPools = await _service.GetAssetPools(assetHolderId);
        return _mapper.Map<List<AssetPoolResponse>>(assetPools);
    }

    // [HttpGet]
    // [Route("balance/{walletId}")]
    // public async Task<BalanceResponse> Balance(Guid walletId)
    // {
    //     return await _AssetPoolService.GetBalance(walletId);
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