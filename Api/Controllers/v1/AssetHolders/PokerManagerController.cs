using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Api.Controllers.Base;
using SFManagement.Application.DTOs.AssetHolders;
using SFManagement.Application.DTOs.Assets;
using SFManagement.Application.DTOs.Transactions;
using SFManagement.Application.Services.AssetHolders;
using SFManagement.Application.Services.Assets;
using SFManagement.Application.Services.Transactions;
using SFManagement.Domain.Entities.AssetHolders;
using SFManagement.Domain.Entities.Transactions;
using SFManagement.Domain.Enums;
using SFManagement.Domain.Enums.Metadata;
using SFManagement.Infrastructure.Authorization;

namespace SFManagement.Api.Controllers.v1.AssetHolders;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class PokerManagerController : BaseAssetHolderController<PokerManager, PokerManagerRequest, PokerManagerResponse>
{
    private readonly PokerManagerService _pokerManagerService;
    private readonly FiatAssetTransactionService _fiatAssetTransactionService;
    private readonly SettlementTransactionService _settlementTransactionService;

    public PokerManagerController(
        PokerManagerService service,
        FiatAssetTransactionService fiatAssetTransactionService,
        SettlementTransactionService settlementTransactionService,
        IMapper mapper,
        ILogger<BaseAssetHolderController<PokerManager, PokerManagerRequest, PokerManagerResponse>> logger,
        WalletIdentifierService walletIdentifierService)
        : base(service, walletIdentifierService, mapper, logger)
    {
        _pokerManagerService = service;
        _fiatAssetTransactionService = fiatAssetTransactionService;
        _settlementTransactionService = settlementTransactionService;
    }

    /// <summary>
    /// Creates an entity from request - PokerManager-specific implementation
    /// </summary>
    protected override async Task<PokerManager> CreateEntityFromRequest(PokerManagerRequest request)
    {
        return await _pokerManagerService.AddFromRequest(request);
    }

    /// <summary>
    /// Updates an entity from request - PokerManager-specific implementation
    /// </summary>
    protected override async Task<PokerManager> UpdateEntityFromRequest(Guid id, PokerManagerRequest request)
    {
        return await _pokerManagerService.UpdateFromRequest(id, request);
    }
    
    /// <summary>
    /// Send Brazilian Real transaction for poker manager
    /// </summary>
    [Obsolete("Use POST /api/v1/transfer instead. Will be removed in v2.")]
    [HttpPost("{id}/send-brazilian-real")]
    [ProducesResponseType(typeof(FiatAssetTransaction), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendBrazilianReais(Guid id, [FromBody] FiatAssetTransactionRequest request)
    {
        try
        {
            var transaction = await _fiatAssetTransactionService.SendBrazilianReais(id, request);
            return Ok(transaction);
        }
        catch (Exception)
        {
            return HandleGenericException("processing Brazilian Real transaction for");
        }
    }
    
    /// <summary>
    /// Get wallet identifiers connected to other asset holders
    /// </summary>
    [HttpGet("{id}/wallet-identifiers-connected")]
    // [RequirePermission("read:wallet-identifiers")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, NoStore = false)]
    [ProducesResponseType(typeof(WalletIdentifiersConnectedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWalletIdentifiersFromOthers(Guid id)
    {
        try
        {
            var groupedWalletIdentifiers = await _pokerManagerService.GetWalletIdentifiersFromOthers(id);
            var settlementTransactions = await _settlementTransactionService.GetClosings(id);
            var lastSettlementTransactions = settlementTransactions.OrderByDescending(st => st.Key).FirstOrDefault().Value;
            
            var response = new WalletIdentifiersConnectedResponse();
            
            foreach (var group in groupedWalletIdentifiers.OrderBy(g => g.Key))
            {
                var walletIdentifierResponses = new List<WalletIdentifierWithAssetHolderResponse>();
                
                foreach (var walletIdentifier in group.Value.ToList())
                {
                    var assetHolderType = walletIdentifier.AssetPool?.BaseAssetHolder?.AssetHolderType;
                    
                    // Get the most recent settlement transaction
                    var lastSettlementTransaction = lastSettlementTransactions?.Where(st => st.SenderWalletIdentifierId == walletIdentifier.Id || st.ReceiverWalletIdentifierId == walletIdentifier.Id)
                    .FirstOrDefault();
                    
                    walletIdentifierResponses.Add(new WalletIdentifierWithAssetHolderResponse
                    {
                        Id = walletIdentifier.Id,
                        InputForTransactions = walletIdentifier.GetPokerMetadata(PokerWalletMetadata.InputForTransactions),
                        AssetType = walletIdentifier.AssetType,
                        // Referral information
                        Referral = walletIdentifier.Referrals.FirstOrDefault() != null ? new ReferralInfo
                        {
                            Id = walletIdentifier.Referrals.First().Id,
                            AssetHolderId = walletIdentifier.Referrals.First().AssetHolderId,
                            Name = walletIdentifier.Referrals.First().AssetHolder.Name,
                            ActiveUntil = walletIdentifier.Referrals.First().ActiveUntil,
                            ParentCommission = walletIdentifier.Referrals.First().ParentCommission
                        } : null,
                        LastSettlementTransaction = lastSettlementTransaction != null ? _mapper.Map<SettlementTransactionSimplifiedResponse>(lastSettlementTransaction) : null,
                        BaseAssetHolderId = walletIdentifier.AssetPool?.BaseAssetHolder?.Id ?? Guid.Empty,
                        BaseAssetHolderName = walletIdentifier.AssetPool?.BaseAssetHolder?.Name ?? string.Empty,
                        AssetHolderType = assetHolderType ?? AssetHolderType.PokerManager
                    });
                }
                
                response.AssetTypeGroups.Add(new WalletIdentifierGroup
                {
                    AssetType = group.Key,
                    WalletIdentifiers = walletIdentifierResponses
                });
            }
            
            return Ok(response);
        }
        catch (Exception)
        {
            return HandleGenericException("retrieving wallet identifiers for");
        }
    }
    
    /// <summary>
    /// Create settlement transactions by date
    /// </summary>
    [HttpPost("{assetHolderId}/settlement-by-date")]
    // [RequireRole("admin")]
    [ProducesResponseType(typeof(SettlementTransactionByDateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSettlementTransactionsByDate(
        Guid assetHolderId, 
        [FromBody] SettlementTransactionByDateRequest request)
    {
        try
        {
            var result = await _settlementTransactionService.CreateSettlementTransactionsByDate(assetHolderId, request);
            return Ok(result);
        }
        catch (Exception)
        {
            return HandleGenericException("creating settlement transactions for");
        }
    }


    /// <summary>
    /// Gets balance by asset group for the poker manager
    /// </summary>
    [HttpGet("{id}/balance")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, NoStore = false)]
    [ProducesResponseType(typeof(Dictionary<string, decimal>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public override async Task<IActionResult> GetBalance(Guid id)
    {
        try
        {
            var balancesByAssetGroup = await _pokerManagerService.GetBalancesByAssetGroup(id);
            
            // Convert AssetGroup enum keys to strings for the response
            var response = balancesByAssetGroup.ToDictionary(
                kvp => kvp.Key.ToString(),
                kvp => kvp.Value
            );
            
            return Ok(response);
        }
        catch (Exception)
        {
            return HandleGenericException("retrieving balance for");
        }
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