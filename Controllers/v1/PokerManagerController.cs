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
public class PokerManagerController(
    PokerManagerService service,
    FiatAssetTransactionService fiatAssetTransactionService,
    IMapper mapper,
    AssetWalletService assetWalletService)
    : BaseApiController<PokerManager, PokerManagerRequest, PokerManagerResponse>(service, mapper)
{
    private readonly IMapper _mapper = mapper;
    private readonly PokerManagerService _pokerManagerService = service;
    private readonly FiatAssetTransactionService _fiatAssetTransactionService = fiatAssetTransactionService;

    [HttpGet]
    [Route("wallets/{managerId}")]
    public async Task<List<AssetWalletResponse>> GetWalletsByManagerId(Guid managerId)
    {
        return _mapper.Map<List<AssetWalletResponse>>(await assetWalletService.GetWalletsByManagerId(managerId));
    }
    
    [HttpPost]
    [Route("{pokerManagerId}/send-brazilian-real")]
    public async Task<FiatAssetTransaction> SendBrazilianReais(Guid pokerManagerId, FiatAssetTransactionRequest request)
    {
        var pokerManager = await _pokerManagerService.Get(pokerManagerId) ?? throw new AppException("Poker manager not found");
        
        return await _fiatAssetTransactionService.SendBrazilianReais(pokerManager, request);
    }
    
    [HttpGet]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, NoStore = false)]
    [Route("{id}/balance")]
    public async Task<Dictionary<AssetType,decimal>> Balance(Guid id)
    {
        return await _pokerManagerService.GetBalancesByAssetType(id);
    }

    [HttpGet]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, NoStore = false)]
    [Route("{id}/transactions")]
    public async Task<StatementAssetHolderWithTransactions> GetAssetHolderWithTransactions(Guid id)
    {
        return await _pokerManagerService.GetAssetHolderWithTransactionsAsStatement(id);
    }


    // [HttpGet]
    // [Route("profit/{managerId}")]
    // public async Task<ProfitResponse> Profit(Guid managerId, DateTime? start, DateTime? end)
    // {
    //     return await pokerManagerService.GetProfit(managerId, start, end);
    // }

    // [HttpGet]
    // [Route("transactions/{managerId}/{startDate?}/{endDate?}/{quantity?}/{page?}")]
    // public async Task<TableResponse<TransactionResponse>> Transactions(Guid managerId, DateTime? startDate = null,
    //     DateTime? endDate = null, int quantity = 100, int page = 0)
    // {
    //     return await transactionService.GetManagerTransactions(managerId, startDate, endDate, quantity, page);
    // }
    //
    // [HttpGet]
    // [Route("transactions/{managerId}/{quantity?}/{page?}")]
    // public async Task<TableResponse<TransactionResponse>> Transactions(Guid managerId, int quantity = 100, int page = 0)
    // {
    //     return await transactionService.GetManagerTransactions(managerId, null, null, quantity, page);
    // }
}