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
public class PokerManagerController : BaseApiController<PokerManager, PokerManagerRequest, PokerManagerResponse>
{
    private readonly IMapper _mapper;
    private readonly PokerManagerService _pokerManagerService;
    private readonly FiatAssetTransactionService _fiatAssetTransactionService;
    private readonly SettlementTransactionService _settlementTransactionService;

    public PokerManagerController(
        PokerManagerService service,
        FiatAssetTransactionService fiatAssetTransactionService,
        SettlementTransactionService settlementTransactionService,
        IMapper mapper)
        : base(service, mapper)
    {
        _mapper = mapper;
        _pokerManagerService = service;
        _fiatAssetTransactionService = fiatAssetTransactionService;
        _settlementTransactionService = settlementTransactionService;
    }

    [HttpPost]
    [Route("")]
    public override async Task<PokerManagerResponse> Post(PokerManagerRequest request)
    {
        var pokerManager = await _pokerManagerService.AddFromRequest(request);
        return _mapper.Map<PokerManagerResponse>(pokerManager);
    }
    
    [HttpPost]
    [Route("{id}/send-brazilian-real")]
    public async Task<FiatAssetTransaction> SendBrazilianReais(Guid id, FiatAssetTransactionRequest request)
    {
        return await _fiatAssetTransactionService.SendBrazilianReais(id, request);
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

    
    [HttpGet]
    [Route("{id}/wallet-identifiers-connected")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, NoStore = false)]
    public async Task<WalletIdentifiersConnectedResponse> GetWalletIdentifiersFromOthers(Guid id)
    {
        var groupedWalletIdentifiers = await _pokerManagerService.GetWalletIdentifiersFromOthers(id);
        
        var response = new WalletIdentifiersConnectedResponse();
        
        foreach (var group in groupedWalletIdentifiers.OrderBy(g => g.Key))
        {
            var walletIdentifierResponses = new List<WalletIdentifierWithAssetHolderResponse>();
            
            foreach (var walletIdentifier in group.Value)
            {
                var assetHolderType = walletIdentifier.BaseAssetHolder.AssetHolderType;
                
                // Get the most recent settlement transaction
                var lastSettlementTransaction = walletIdentifier.SettlementTransactions
                    .OrderByDescending(st => st.CreatedAt)
                    .FirstOrDefault();
                
                walletIdentifierResponses.Add(new WalletIdentifierWithAssetHolderResponse
                {
                    Id = walletIdentifier.Id,
                    InputForTransactions = walletIdentifier.InputForTransactions,
                    AssetType = walletIdentifier.AssetType,
                    RouteInfo = walletIdentifier.RouteInfo,
                    IdentifierInfo = walletIdentifier.IdentifierInfo,
                    DescriptiveInfo = walletIdentifier.DescriptiveInfo,
                    ExtraInfo = walletIdentifier.ExtraInfo,
                    Referral = walletIdentifier.Referral != null ? new ReferralInfo
                    {
                        Id = walletIdentifier.Referral.Id,
                        AssetHolderId = walletIdentifier.Referral.AssetHolderId,
                        Name = walletIdentifier.Referral.AssetHolder.Name,
                        ActiveUntil = walletIdentifier.Referral.ActiveUntil,
                        ParentCommission = walletIdentifier.Referral.ParentCommission
                    } : null,
                    LastSettlementTransaction = lastSettlementTransaction != null ? _mapper.Map<SettlementTransactionSimplifiedResponse>(lastSettlementTransaction) : null,
                    BaseAssetHolderId = walletIdentifier.BaseAssetHolder.Id,
                    BaseAssetHolderName = walletIdentifier.BaseAssetHolder.Name,
                    AssetHolderType = assetHolderType
                });
            }
            
            response.AssetTypeGroups.Add(new WalletIdentifierGroup
            {
                AssetType = group.Key,
                WalletIdentifiers = walletIdentifierResponses
            });
        }
        
        return response;
    }
    
    [HttpPost]
    [Route("{assetHolderId}/settlement-by-date")]
    public async Task<SettlementTransactionByDateResponse> CreateSettlementTransactionsByDate(
        Guid assetHolderId, 
        [FromBody] SettlementTransactionByDateRequest request)
    {
        return await _settlementTransactionService.CreateSettlementTransactionsByDate(assetHolderId, request);
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