using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Interfaces;
using SFManagement.Models.Entities;
using SFManagement.ViewModels;
using SFManagement.Enums;
using SFManagement.Models.AssetInfrastructure;
using SFManagement.Enums.AssetInfrastructure;

namespace SFManagement.Services;

public class PokerManagerService : BaseAssetHolderService<PokerManager>
{
    public PokerManagerService(
        DataContext context, 
        IHttpContextAccessor httpContextAccessor,
        IAssetHolderDomainService domainService,
        ReferralService referralService,
        InitialBalanceService initialBalanceService) 
        : base(context, httpContextAccessor, domainService, referralService, initialBalanceService)
    {
    }
    
    /// <summary>
    /// Creates a new poker manager with comprehensive validation
    /// </summary>
    public async Task<PokerManager> AddFromRequest(PokerManagerRequest request)
    {
        return await base.AddFromRequest(
            request,
            baseAssetHolder => new PokerManager
            {
                BaseAssetHolderId = baseAssetHolder.Id
            },
            _domainService.ValidatePokerManagerCreation
        );
    }

    /// <summary>
    /// Updates a poker manager with validation
    /// </summary>
    public async Task<PokerManager> UpdateFromRequest(Guid pokerManagerId, PokerManagerRequest request)
    {
        return await base.UpdateFromRequest(
            pokerManagerId,
            request,
            (pokerManager, req) => 
            {
                // PokerManager doesn't have additional properties to update beyond BaseAssetHolder
                // This is here for consistency and future extensibility
            },
            _domainService.ValidatePokerManagerCreation
        );
    }
    
    //Get all wallet identifiers of other AssetHolders for all AssetPool types a Manager have
    public async Task<Dictionary<AssetType, List<WalletIdentifier>>> GetWalletIdentifiersFromOthers(Guid pokerManagerId)
    {
        // Get the poker manager with their asset wallets
        var pokerManager = await context.PokerManagers
            .Include(pm => pm.BaseAssetHolder)
            .ThenInclude(bah => bah.AssetPools)
            .ThenInclude(aw => aw.WalletIdentifiers)
            .FirstOrDefaultAsync(pm => pm.BaseAssetHolderId == pokerManagerId);

        if (pokerManager == null)
            throw new Exception("PokerManager not found");

        // Get all asset types that this poker manager has
        var assetTypes = pokerManager.BaseAssetHolder?.AssetPools
            .SelectMany(ap => ap.WalletIdentifiers.Select(wi => wi.AssetType))
            .Distinct()
            .ToList();

        if (assetTypes == null || assetTypes.Count == 0)
            return [];

        // Get all wallet identifiers from other asset holders (excluding this poker manager)
        // that match the asset types this poker manager has
        var walletIdentifiers = await context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
            .ThenInclude(aw => aw.BaseAssetHolder)
            .ThenInclude(aw => aw.Client)
            .Include(wi => wi.Referrals)
            // .Include(wi => wi.SettlementTransactions.Where(st => !st.DeletedAt.HasValue))
            .Where(wi => assetTypes.Contains(wi.AssetType) && 
                        !wi.DeletedAt.HasValue &&
                        wi.AssetPool.AssetGroup == AssetGroup.PokerAssets &&
                        wi.AssetPool.BaseAssetHolderId != pokerManager.BaseAssetHolderId)
            .ToListAsync();

        // Group by asset type
        var groupedWalletIdentifiers = walletIdentifiers
            .GroupBy(wi => wi.AssetType)
            .ToDictionary(
                group => group.Key,
                group => group.ToList()
            );

        return groupedWalletIdentifiers;
    }

    // public new async Task<Dictionary<AssetGroup, decimal>> GetBalancesByAssetGroup(Guid pokerManagerId)
    // {
    //     // Get the poker manager to get the BaseAssetHolderId
    //     var pokerManager = await context.PokerManagers
    //         .FirstOrDefaultAsync(pm => pm.BaseAssetHolderId == pokerManagerId && !pm.DeletedAt.HasValue);
        
    //     if (pokerManager == null)
    //         throw new ArgumentException($"PokerManager not found: {pokerManagerId}");
        
    //     // Call the base class method with the BaseAssetHolderId
    //     return await base.GetBalancesByAssetGroup(pokerManager.BaseAssetHolderId);
    // }

    // public async Task<BalanceResponse> GetBalance(Guid managerId, DateTime? date)
    // {
    //     // var now = DateTime.Now;
    //     // if (!date.HasValue || date.Value.Year == 1) date = now;
    //     //
    //     // var manager = await context.Managers
    //     //     .Include(x => x.BankTransactions)
    //     //     .Include(x => x.Wallets)
    //     //     .ThenInclude(x => x.Transactions).Include(x => x.InternalTransactions)
    //     //     .Include(x => x.WalletTransactions)
    //     //     .Include(x => x.ClosingManagers)
    //     //     .FirstOrDefaultAsync(x => x.Id == managerId);
    //     // var avgRate = await context.AvgRates.AsNoTracking().OrderByDescending(x => x.Date)
    //     //     .Where(x => x.Date < date && x.ManagerId == managerId).FirstOrDefaultAsync();
    //     // return new BalanceResponse(manager, avgRate, date);
    //     await Task.Yield();
    //     return null;
    // }

    // public async Task<ProfitResponse> GetProfit(Guid managerId, DateTime? start, DateTime? end)
    // {
    //     // var manager = await context.Managers.Include(x => x.BankTransactions).Include(x => x.ClosingManagers)
    //     //     .ThenInclude(x => x.InternalTransactions).Include(x => x.Wallets).ThenInclude(x => x.Transactions)
    //     //     .Include(x => x.InternalTransactions).Include(x => x.WalletTransactions)
    //     //     .FirstOrDefaultAsync(x => x.Id == managerId);
    //     //
    //     // if (start.HasValue && end.HasValue) return new ProfitResponse(manager, start.Value, end.Value);
    //     //
    //     // return new ProfitResponse(manager);
    //     await Task.Yield();
    //     return null;
    // }
}