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
    
    public async Task<TableResponse<TEntity>> GetAssetHolderTransactions(Guid[] AssetWalletIds, DateTime? startDate,
        DateTime? endDate, int quantity, int page)
    {
        var response = new TableResponse<TEntity>
        {
            Page = page,
            Show = quantity
        };
        
        var transactionsQuery = _entity
            .Where(x => !x.DeletedAt.HasValue && AssetWalletIds.Contains(x.AssetWalletId));
        
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
            .Include(x => x.WalletIdentifier)
            .ThenInclude(y => y.Bank)
            .Include(x => x.WalletIdentifier)
            .ThenInclude(y => y.Client)
            .Include(x => x.WalletIdentifier)
            .ThenInclude(y => y.Member)
            .Include(x => x.WalletIdentifier)
            .ThenInclude(y => y.PokerManager)

            .Include(x => x.AssetWallet)
            .ThenInclude(y => y.Bank)
            .Include(x => x.AssetWallet)
            .ThenInclude(y => y.Client)
            .Include(x => x.AssetWallet)
            .ThenInclude(y => y.Member)
            .Include(x => x.AssetWallet)
            .ThenInclude(y => y.PokerManager)
            .ToListAsync()));
            // .Select(_mapper.Map<FiatAssetTransactionResponse>));
        
        response.Data = [.. allTransactions.OrderBy(x => x.Date).Skip(page * quantity).Take(quantity)];
        
        return response;
    }
    
    public async Task<TableResponse<TEntity>> GetNonAssetHolderTransactions(Guid[]? AssetWalletIds, DateTime? startDate,
        DateTime? endDate, int quantity, int page)
    {
        var response = new TableResponse<TEntity>
        {
            Page = page,
            Show = quantity
        };
        
        var bankTransactionsQuery = _entity
            .Where(x => !x.DeletedAt.HasValue && (AssetWalletIds == null || !AssetWalletIds.Contains(x.AssetWalletId)));
        
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
            .Include(x => x.WalletIdentifier)
            .ThenInclude(y => y.Bank)
            .Include(x => x.WalletIdentifier)
            .ThenInclude(y => y.Client)
            .Include(x => x.WalletIdentifier)
            .ThenInclude(y => y.Member)
            .Include(x => x.WalletIdentifier)
            .ThenInclude(y => y.PokerManager)

            .Include(x => x.AssetWallet)
            .ThenInclude(y => y.Bank)
            .Include(x => x.AssetWallet)
            .ThenInclude(y => y.Client)
            .Include(x => x.AssetWallet)
            .ThenInclude(y => y.Member)
            .Include(x => x.AssetWallet)
            .ThenInclude(y => y.PokerManager)
            .ToListAsync()));
            // .Select(_mapper.Map<FiatAssetTransactionResponse>));
        
        response.Data = allTransactions.OrderBy(x => x.Date).Skip(page * quantity).Take(quantity).ToList();
        
        return response;
    }
}