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
    
    public async Task<TEntity[]> GetAssetHolderTransactions(Guid[] assetWalletIds, DateTime? startDate,
        DateTime? endDate, int quantity, int page)
    {
        var response = new TableResponse<TEntity>
        {
            Page = page,
            Show = quantity
        };
        
        // Get all wallet identifiers for the specified asset wallets
        var walletIdentifierIds = await context.WalletIdentifiers
            .Where(wi => assetWalletIds.Contains(wi.AssetWalletId))
            .Select(wi => wi.Id)
            .ToListAsync();
        
        var transactionsQuery = _entity
            .Where(x => !x.DeletedAt.HasValue && 
                (walletIdentifierIds.Contains(x.SenderWalletIdentifierId) || 
                 walletIdentifierIds.Contains(x.ReceiverWalletIdentifierId)));
        
        if (startDate.HasValue)
        {
            transactionsQuery = transactionsQuery.Where(x => x.Date >= startDate.Value);
        }
        
        if (endDate.HasValue)
        {
            transactionsQuery = transactionsQuery.Where(x => x.Date <= endDate.Value);
        }
        
        response.Total = await transactionsQuery.CountAsync();
        
        var allTransactions = new List<TEntity>();
        allTransactions.AddRange((await transactionsQuery
            .Include(x => x.Category)
            .Include(x => x.SenderWalletIdentifier)
                .ThenInclude(wi => wi.AssetWallet)
                .ThenInclude(aw => aw.BaseAssetHolder)
            .Include(x => x.ReceiverWalletIdentifier)
                .ThenInclude(wi => wi.AssetWallet)
                .ThenInclude(aw => aw.BaseAssetHolder)
            .ToListAsync()));
        
        return allTransactions.OrderBy(x => x.Date).Skip(page * quantity).Take(quantity).ToArray();
    }
    
    public async Task<TEntity[]> GetNonAssetHolderTransactions(Guid[]? assetWalletIds, DateTime? startDate,
        DateTime? endDate, int quantity, int page)
    {
        var response = new TableResponse<TEntity>
        {
            Page = page,
            Show = quantity
        };
        
        // Get all wallet identifiers for the specified asset wallets (if any)
        var walletIdentifierIds = assetWalletIds != null 
            ? await context.WalletIdentifiers
                .Where(wi => assetWalletIds.Contains(wi.AssetWalletId))
                .Select(wi => wi.Id)
                .ToListAsync()
            : new List<Guid>();
        
        var bankTransactionsQuery = _entity
            .Where(x => !x.DeletedAt.HasValue && 
                (assetWalletIds == null || 
                 (!walletIdentifierIds.Contains(x.SenderWalletIdentifierId) && 
                  !walletIdentifierIds.Contains(x.ReceiverWalletIdentifierId))));
        
        if (startDate.HasValue)
        {
            bankTransactionsQuery = bankTransactionsQuery.Where(x => x.Date >= startDate.Value);
        }
        
        if (endDate.HasValue)
        {
            bankTransactionsQuery = bankTransactionsQuery.Where(x => x.Date <= endDate.Value);
        }
        
        response.Total = await bankTransactionsQuery.CountAsync();
        
        var allTransactions = new List<TEntity>();
        allTransactions.AddRange((await bankTransactionsQuery
            .Include(x => x.SenderWalletIdentifier)
                .ThenInclude(wi => wi.AssetWallet)
                .ThenInclude(aw => aw.BaseAssetHolder)
            .Include(x => x.ReceiverWalletIdentifier)
                .ThenInclude(wi => wi.AssetWallet)
                .ThenInclude(aw => aw.BaseAssetHolder)
            .ToListAsync()));
            
        return allTransactions.OrderBy(x => x.Date).Skip(page * quantity).Take(quantity).ToArray();
    }
}