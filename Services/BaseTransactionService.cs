using SFManagement.Models.Transactions;
using SFManagement.ViewModels;
using Microsoft.EntityFrameworkCore;
using SFManagement.Data;

namespace SFManagement.Services;

public class BaseTransactionService<TEntity> : BaseService<TEntity> where TEntity : BaseTransaction
{
    public BaseTransactionService(DataContext context, IHttpContextAccessor httpContextAccessor) 
        : base(context, httpContextAccessor)
    {
    }
    
    public async Task<TableResponse<TEntity>> GetAssetHolderTransactions(
        Guid[] AssetPoolIds, 
        DateTime? startDate,
        DateTime? endDate, 
        int quantity = 100, 
        int page = 0)
    {
        // Get all wallet identifiers for the specified asset wallets
        var walletIdentifierIds = await context.WalletIdentifiers
            .Where(wi => AssetPoolIds.Contains(wi.AssetPoolId) && !wi.DeletedAt.HasValue)
            .Select(wi => wi.Id)
            .ToListAsync();

        if (!walletIdentifierIds.Any())
        {
            return new TableResponse<TEntity>
            {
                Data = new List<TEntity>(),
                Total = 0,
                Page = page,
                Show = quantity
            };
        }

        var query = _entity
            .Where(x => !x.DeletedAt.HasValue && 
                (walletIdentifierIds.Contains(x.SenderWalletIdentifierId) || 
                 walletIdentifierIds.Contains(x.ReceiverWalletIdentifierId)));

        // Apply date filters
        if (startDate.HasValue)
            query = query.Where(x => x.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(x => x.Date <= endDate.Value);

        // Get total count
        var total = await query.CountAsync();

        // Get paginated results with optimized includes
        var transactions = await query
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.CreatedAt)
            .Skip(page * quantity)
            .Take(quantity)
            .Include(x => x.Category)
            .Include(x => x.SenderWalletIdentifier)
                .ThenInclude(wi => wi.AssetPool)
                .ThenInclude(aw => aw.BaseAssetHolder)
            .Include(x => x.ReceiverWalletIdentifier)
                .ThenInclude(wi => wi.AssetPool)
                .ThenInclude(aw => aw.BaseAssetHolder)
            .ToListAsync();

        return new TableResponse<TEntity>
        {
            Data = transactions,
            Total = total,
            Page = page,
            Show = quantity
        };
    }
    
    public async Task<TableResponse<TEntity>> GetNonAssetHolderTransactions(
        Guid[]? AssetPoolIds, 
        DateTime? startDate,
        DateTime? endDate, 
        int quantity = 100, 
        int page = 0)
    {
        // Get all wallet identifiers for the specified asset wallets (if any)
        var walletIdentifierIds = new List<Guid>();
        
        if (AssetPoolIds != null && AssetPoolIds.Any())
        {
            walletIdentifierIds = await context.WalletIdentifiers
                .Where(wi => AssetPoolIds.Contains(wi.AssetPoolId) && !wi.DeletedAt.HasValue)
                .Select(wi => wi.Id)
                .ToListAsync();
        }

        var query = _entity
            .Where(x => !x.DeletedAt.HasValue && 
                (AssetPoolIds == null || !AssetPoolIds.Any() ||
                 (!walletIdentifierIds.Contains(x.SenderWalletIdentifierId) && 
                  !walletIdentifierIds.Contains(x.ReceiverWalletIdentifierId))));

        // Apply date filters
        if (startDate.HasValue)
            query = query.Where(x => x.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(x => x.Date <= endDate.Value);

        // Get total count
        var total = await query.CountAsync();

        // Get paginated results with optimized includes
        var transactions = await query
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.CreatedAt)
            .Skip(page * quantity)
            .Take(quantity)
            .Include(x => x.Category)
            .Include(x => x.SenderWalletIdentifier)
                .ThenInclude(wi => wi.AssetPool)
                .ThenInclude(aw => aw.BaseAssetHolder)
            .Include(x => x.ReceiverWalletIdentifier)
                .ThenInclude(wi => wi.AssetPool)
                .ThenInclude(aw => aw.BaseAssetHolder)
            .ToListAsync();

        return new TableResponse<TEntity>
        {
            Data = transactions,
            Total = total,
            Page = page,
            Show = quantity
        };
    }

    public async Task<TEntity[]> GetTransactionsByWalletIdentifier(
        Guid walletIdentifierId, 
        DateTime? startDate = null,
        DateTime? endDate = null, 
        int quantity = 100, 
        int page = 0)
    {
        var query = _entity
            .Where(x => !x.DeletedAt.HasValue && 
                (x.SenderWalletIdentifierId == walletIdentifierId || 
                 x.ReceiverWalletIdentifierId == walletIdentifierId));

        // Apply date filters
        if (startDate.HasValue)
            query = query.Where(x => x.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(x => x.Date <= endDate.Value);

        return await query
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.CreatedAt)
            .Skip(page * quantity)
            .Take(quantity)
            .Include(x => x.Category)
            .Include(x => x.SenderWalletIdentifier)
                .ThenInclude(wi => wi.AssetPool)
                .ThenInclude(aw => aw.BaseAssetHolder)
            .Include(x => x.ReceiverWalletIdentifier)
                .ThenInclude(wi => wi.AssetPool)
                .ThenInclude(aw => aw.BaseAssetHolder)
            .ToArrayAsync();
    }

    public async Task<decimal> GetBalanceForWalletIdentifier(Guid walletIdentifierId)
    {
        var incomingSum = await _entity
            .Where(x => !x.DeletedAt.HasValue && x.ReceiverWalletIdentifierId == walletIdentifierId)
            .SumAsync(x => x.AssetAmount);

        var outgoingSum = await _entity
            .Where(x => !x.DeletedAt.HasValue && x.SenderWalletIdentifierId == walletIdentifierId)
            .SumAsync(x => x.AssetAmount);

        return incomingSum - outgoingSum;
    }
}