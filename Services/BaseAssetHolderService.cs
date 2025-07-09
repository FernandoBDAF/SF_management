using SFManagement.Data;
using SFManagement.Enums;
using SFManagement.Models.Entities;
using SFManagement.Models.Transactions;
using SFManagement.ViewModels;
using Microsoft.EntityFrameworkCore;
using SFManagement.Interfaces;
using SFManagement.Models;
using SFManagement.Models.AssetInfrastructure;

namespace SFManagement.Services;

// IMPORTANT: This service contains several methods that need major updates for the new transaction model:
// - GetFilteredByWalletIdentifierType: Update to use AssetWallet.WalletIdentifiers navigation
// - GetBalancesByAssetType: Update to query transactions via SenderWalletIdentifierId/ReceiverWalletIdentifierId
// - GetAssetHolderWithTransactions: Update to use new transaction relationships
// - GetAssetHolderWithTransactionsNoCascade: Update transaction queries and projections
// - GetAssetHolderWithTransactionsAsStatement: Update transaction processing logic
public class BaseAssetHolderService<TEntity>(DataContext context, IHttpContextAccessor httpContextAccessor) 
    : BaseService<TEntity>(context, httpContextAccessor) where TEntity : BaseDomain, IAssetHolder
{
    public override async Task<List<TEntity>> List()
    {
        var query = _entity.AsQueryable();
        
        // Include BaseAssetHolder for all specific asset holder entities
        if (typeof(TEntity) == typeof(Bank))
        {
            query = ((IQueryable<Bank>)query).Include(b => b.BaseAssetHolder).Cast<TEntity>();
        }
        else if (typeof(TEntity) == typeof(Client))
        {
            query = ((IQueryable<Client>)query).Include(c => c.BaseAssetHolder).Cast<TEntity>();
        }
        else if (typeof(TEntity) == typeof(Member))
        {
            query = ((IQueryable<Member>)query).Include(m => m.BaseAssetHolder).Cast<TEntity>();
        }
        else if (typeof(TEntity) == typeof(PokerManager))
        {
            query = ((IQueryable<PokerManager>)query).Include(pm => pm.BaseAssetHolder).Cast<TEntity>();
        }
        
        return await query.Where(x => !x.DeletedAt.HasValue).OrderByDescending(x => x.CreatedAt).ToListAsync();
    }

    public override async Task<TEntity?> Get(Guid id)
    {
        // Get the BaseAssetHolder first since the ID is always from BaseAssetHolder
        var baseAssetHolder = await context.BaseAssetHolders
            .Include(c => c.AssetWallets)
                .ThenInclude(aw => aw.WalletIdentifiers)
            .Include(c => c.Address)
            .Include(c => c.ContactPhones)
            .Include(c => c.InitialBalances)
            .FirstOrDefaultAsync(x => x.Id == id && !x.DeletedAt.HasValue);
        
        if (baseAssetHolder == null)
            return null;
            
        // Simplify with generic to not repeat the code
        if (typeof(TEntity) == typeof(Bank))
        {
            var bank = await context.Banks
                .FirstOrDefaultAsync(x => x.BaseAssetHolderId == id && !x.DeletedAt.HasValue);
            if (bank != null)
            {
                bank.BaseAssetHolder = baseAssetHolder;
                return bank as TEntity;
            }
        }
        else if (typeof(TEntity) == typeof(Client))
        {
            var client = await context.Clients
                .FirstOrDefaultAsync(x => x.BaseAssetHolderId == id && !x.DeletedAt.HasValue);
            if (client != null)
            {
                client.BaseAssetHolder = baseAssetHolder;
                return client as TEntity;
            }
        }
        else if (typeof(TEntity) == typeof(Member))
        {
            var member = await context.Members
                .FirstOrDefaultAsync(x => x.BaseAssetHolderId == id && !x.DeletedAt.HasValue);
            if (member != null)
            {
                member.BaseAssetHolder = baseAssetHolder;
                return member as TEntity;
            }
        }
        else if (typeof(TEntity) == typeof(PokerManager))
        {
            var pokerManager = await context.PokerManagers
                .FirstOrDefaultAsync(x => x.BaseAssetHolderId == id && !x.DeletedAt.HasValue);
            if (pokerManager != null)
            {
                pokerManager.BaseAssetHolder = baseAssetHolder;
                return pokerManager as TEntity;
            }
        }
        else if (typeof(TEntity) == typeof(BaseAssetHolder))
        {
            return baseAssetHolder as TEntity;
        }
        
        // Fallback to base implementation for other entity types
        return await base.Get(id);
    }

    // this method may be removed
    public async Task<Guid[]> GetAssetHolderAssetWalletIds()
    {
        var assetHolderType = GetAssetHolderTypeForEntity<TEntity>();
        var assetWalletIds = await context.BaseAssetHolders
            .Where(bah => bah.AssetHolderType == assetHolderType)
            .Include(bah => bah.AssetWallets)
            .SelectMany(bah => bah.AssetWallets.Where(aw => !aw.DeletedAt.HasValue).Select(aw => aw.Id))
            .ToArrayAsync();
        
        return assetWalletIds;
    }
    
    public async Task<List<BaseAssetHolder>> GetFilteredByWalletIdentifierType(AssetType assetType)
    {
        var assetHolderType = GetAssetHolderTypeForEntity<TEntity>();
        
        // Build query based on the specific entity type to avoid using computed property
        IQueryable<BaseAssetHolder> query = context.BaseAssetHolders;
        
        if (typeof(TEntity) == typeof(Client))
        {
            query = query.Include(bah => bah.Client)
                .Where(bah => bah.Client != null);
        }
        else if (typeof(TEntity) == typeof(Bank))
        {
            query = query.Include(bah => bah.Bank)
                .Where(bah => bah.Bank != null);
        }
        else if (typeof(TEntity) == typeof(Member))
        {
            query = query.Include(bah => bah.Member)
                .Where(bah => bah.Member != null);
        }
        else if (typeof(TEntity) == typeof(PokerManager))
        {
            query = query.Include(bah => bah.PokerManager)
                .Where(bah => bah.PokerManager != null);
        }
        
        var baseAssetHolders = await query
            .Include(bah => bah.AssetWallets)
                .ThenInclude(aw => aw.WalletIdentifiers)
            .Where(bah => bah.AssetWallets.Any(aw => 
                aw.WalletIdentifiers.Any(wi => wi.AssetWallet.AssetType == assetType && !wi.DeletedAt.HasValue)))
            .ToListAsync();

        return baseAssetHolders;
    }

    // Helper method to get AssetHolderType for the specific entity type
    private AssetHolderType GetAssetHolderTypeForEntity<T>() where T : BaseDomain
    {
        return typeof(T).Name switch
        {
            nameof(Client) => AssetHolderType.Client,
            nameof(Bank) => AssetHolderType.Bank,
            nameof(Member) => AssetHolderType.Member,
            nameof(PokerManager) => AssetHolderType.PokerManager,
            _ => throw new InvalidOperationException($"Unknown entity type: {typeof(T).Name}")
        };
    }

    // this method may be removed
    public class AssetBalance
    {
        public AssetType AssetType { get; set; }
        public decimal? Value { get; set; }
    }

    public async Task<Dictionary<AssetType, decimal>> GetBalancesByAssetType(Guid baseAssetHolderId)
    {
        var balances = new Dictionary<AssetType, decimal>();

        var walletIdentifiers = await context.WalletIdentifiers
            .Include(wi => wi.AssetWallet)
            .Where(wi => wi.AssetWallet.BaseAssetHolderId == baseAssetHolderId && !wi.DeletedAt.HasValue)
            .ToListAsync();

        var walletIdentifierIds = walletIdentifiers.Select(wi => wi.Id).ToArray();
       
        var digitalTransactions = await context.DigitalAssetTransactions
            .Where(dt => !dt.DeletedAt.HasValue && 
                (walletIdentifierIds.Contains(dt.SenderWalletIdentifierId) || 
                 walletIdentifierIds.Contains(dt.ReceiverWalletIdentifierId)))
            .Include(dt => dt.SenderWalletIdentifier)
                .ThenInclude(wi => wi.AssetWallet)
            .Include(dt => dt.ReceiverWalletIdentifier)
                .ThenInclude(wi => wi.AssetWallet)
            .ToArrayAsync() ?? [];

        var fiatTransactions = await context.FiatAssetTransactions
            .Where(ft => !ft.DeletedAt.HasValue && 
                (walletIdentifierIds.Contains(ft.SenderWalletIdentifierId) || 
                 walletIdentifierIds.Contains(ft.ReceiverWalletIdentifierId)))
            .Include(ft => ft.SenderWalletIdentifier)
                .ThenInclude(wi => wi.AssetWallet)
            .Include(ft => ft.ReceiverWalletIdentifier)
                .ThenInclude(wi => wi.AssetWallet)
            .ToArrayAsync() ?? [];

        // Process digital asset transactions
        foreach (var tx in digitalTransactions)
        {
            var isReceiver = walletIdentifierIds.Contains(tx.ReceiverWalletIdentifierId);
            var assetType = isReceiver ? tx.ReceiverWalletIdentifier.AssetWallet.AssetType : tx.SenderWalletIdentifier.AssetWallet.AssetType;
            
            // If this asset holder is the receiver, it's income (positive)
            // If this asset holder is the sender, it's expense (negative)
            var value = isReceiver ? tx.AssetAmount : -tx.AssetAmount;
            
            if (!balances.ContainsKey(assetType)) balances[assetType] = 0;
            balances[assetType] += value;
        }

        // Process fiat asset transactions
        foreach (var tx in fiatTransactions)
        {
            var isReceiver = walletIdentifierIds.Contains(tx.ReceiverWalletIdentifierId);
            var assetType = isReceiver ? tx.ReceiverWalletIdentifier.AssetWallet.AssetType : tx.SenderWalletIdentifier.AssetWallet.AssetType;
            
            // If this asset holder is the receiver, it's income (positive)
            // If this asset holder is the sender, it's expense (negative)
            var value = isReceiver ? tx.AssetAmount : -tx.AssetAmount;
            
            if (!balances.ContainsKey(assetType)) balances[assetType] = 0;
            balances[assetType] += value;
        }

        return balances;
    }
    
    // public async Task<BaseAssetHolder> GetAssetHolderWithTransactionsNoCascade(Guid baseAssetHolderId)
    // {
    //     // Use a completely different approach with projection to avoid cascade
    //     var query = context.BaseAssetHolders.AsQueryable();
        
    //     // First, get the asset holder with basic info
    //     var assetHolder = await query
    //         .Include(c => c.AssetWallets)
    //         .Include(c => c.WalletIdentifiers)
    //         .AsNoTracking()
    //         .FirstOrDefaultAsync(x => x.Id == baseAssetHolderId) ?? throw new Exception("AssetHolder not found");

    //     // Now load transactions separately with minimal includes
    //     foreach (var aw in assetHolder.AssetWallets)
    //     {
    //         // Load DigitalAssetTransactions with minimal WalletIdentifier info
    //         aw.DigitalAssetTransactions = await context.DigitalAssetTransactions
    //             .Where(dat => dat.AssetWalletId == aw.Id && !dat.DeletedAt.HasValue)
    //             .Select(dat => new DigitalAssetTransaction
    //             {
    //                 Id = dat.Id,
    //                 Date = dat.Date,
    //                 Description = dat.Description,
    //                 AssetAmount = dat.AssetAmount,
    //                 TransactionDirection = dat.TransactionDirection,
    //                 BalanceAs = dat.BalanceAs,
    //                 ConversionRate = dat.ConversionRate,
    //                 Rate = dat.Rate,
    //                 AssetWalletId = dat.AssetWalletId,
    //                 WalletIdentifierId = dat.WalletIdentifierId,
    //                 WalletIdentifier = new WalletIdentifier
    //                 {
    //                     InputForTransactions = dat.WalletIdentifier.InputForTransactions,
    //                     AssetType = dat.WalletIdentifier.AssetType,
    //                     BaseAssetHolder = new BaseAssetHolder
    //                     {
    //                         Id = dat.WalletIdentifier.BaseAssetHolder.Id, 
    //                         Name = dat.WalletIdentifier.BaseAssetHolder.Name
    //                     },
                       
    //                 }
    //             })
    //             .ToListAsync();

    //         // Load FiatAssetTransactions with minimal WalletIdentifier info
    //         aw.FiatAssetTransactions = await context.FiatAssetTransactions
    //             .Where(fat => fat.AssetWalletId == aw.Id && !fat.DeletedAt.HasValue)
    //             .Select(fat => new FiatAssetTransaction
    //             {
    //                 Id = fat.Id,
    //                 Date = fat.Date,
    //                 Description = fat.Description,
    //                 AssetAmount = fat.AssetAmount,
    //                 TransactionDirection = fat.TransactionDirection,
    //                 AssetWalletId = fat.AssetWalletId,
    //                 WalletIdentifierId = fat.WalletIdentifierId,
    //                 WalletIdentifier = new WalletIdentifier
    //                 {
    //                     Id = fat.WalletIdentifier.Id,
    //                     InputForTransactions = fat.WalletIdentifier.InputForTransactions,
    //                     AssetType = fat.WalletIdentifier.AssetType,
    //                     BaseAssetHolder = new BaseAssetHolder 
    //                         { 
    //                             Id = fat.WalletIdentifier.BaseAssetHolder.Id,
    //                             Name = fat.WalletIdentifier.BaseAssetHolder.Name 
    //                         },
    //                 }
    //             })
    //             .ToListAsync();
    //     }

    //     foreach (var wi in assetHolder.WalletIdentifiers)
    //     {
    //         // Load DigitalAssetTransactions with minimal AssetWallet info
    //         wi.DigitalAssetTransactions = await context.DigitalAssetTransactions
    //             .Where(dat => dat.WalletIdentifierId == wi.Id && !dat.DeletedAt.HasValue)
    //             .Select(dat => new DigitalAssetTransaction
    //             {
    //                 Id = dat.Id,
    //                 Date = dat.Date,
    //                 Description = dat.Description,
    //                 AssetAmount = dat.AssetAmount,
    //                 TransactionDirection = dat.TransactionDirection,
    //                 BalanceAs = dat.BalanceAs,
    //                 ConversionRate = dat.ConversionRate,
    //                 Rate = dat.Rate,
    //                 AssetWalletId = dat.AssetWalletId,
    //                 WalletIdentifierId = dat.WalletIdentifierId,
    //                 AssetWallet = new AssetWallet
    //                 {
    //                     Id = dat.AssetWallet.Id,
    //                     AssetType = dat.AssetWallet.AssetType,
    //                     BaseAssetHolder = new BaseAssetHolder
    //                     {
    //                         Id = dat.AssetWallet.BaseAssetHolder.Id, 
    //                         Name = dat.AssetWallet.BaseAssetHolder.Name
    //                     },
    //                 }
    //             })
    //             .ToListAsync();

    //         // Load FiatAssetTransactions with minimal AssetWallet info
    //         wi.FiatAssetTransactions = await context.FiatAssetTransactions
    //             .Where(fat => fat.WalletIdentifierId == wi.Id && !fat.DeletedAt.HasValue)
    //             .Select(fat => new FiatAssetTransaction
    //             {
    //                 Id = fat.Id,
    //                 Date = fat.Date,
    //                 Description = fat.Description,
    //                 AssetAmount = fat.AssetAmount,
    //                 TransactionDirection = fat.TransactionDirection,
    //                 AssetWalletId = fat.AssetWalletId,
    //                 WalletIdentifierId = fat.WalletIdentifierId,
    //                 AssetWallet = new AssetWallet
    //                 {
    //                     Id = fat.AssetWallet.Id,
    //                     AssetType = fat.AssetWallet.AssetType,
    //                     BaseAssetHolder = new BaseAssetHolder
    //                     {
    //                         Id = fat.AssetWallet.BaseAssetHolder.Id, 
    //                         Name = fat.AssetWallet.BaseAssetHolder.Name
    //                     },
    //                 }
    //             })
    //             .ToListAsync();
    //     }

    //     return assetHolder;
    // }

    public async Task<StatementAssetHolderWithTransactions> GetAssetHolderWithTransactionsAsStatement(Guid baseAssetHolderId)
    {
        var walletIdentifiers = await context.WalletIdentifiers
            .Include(wi => wi.AssetWallet)
            .Where(wi => wi.AssetWallet.BaseAssetHolderId == baseAssetHolderId && !wi.DeletedAt.HasValue)
            .ToListAsync();

        var walletIdentifierIds = walletIdentifiers.Select(wi => wi.Id).ToArray();
    
        var digitalTransactions = await context.DigitalAssetTransactions
            .Where(dt => !dt.DeletedAt.HasValue && 
                (walletIdentifierIds.Contains(dt.SenderWalletIdentifierId) || 
                 walletIdentifierIds.Contains(dt.ReceiverWalletIdentifierId)))
            .Include(dt => dt.SenderWalletIdentifier)
                .ThenInclude(wi => wi.AssetWallet)
                    .ThenInclude(wi => wi.BaseAssetHolder)
            .Include(dt => dt.ReceiverWalletIdentifier)
                .ThenInclude(wi => wi.AssetWallet)
                    .ThenInclude(wi => wi.BaseAssetHolder)
            .ToArrayAsync() ?? [];
        
        var allTransactions = new List<StatementTransactionResponse>();

        foreach (var dat in digitalTransactions)
        {
            allTransactions.Add(new StatementTransactionResponse
            {
                Id = dat.Id,
                Date = dat.Date,
                Description = dat.Description,
                AssetAmount = dat.SenderWalletIdentifierId == dat.Id ? -dat.AssetAmount : dat.AssetAmount,
                BalanceAs = dat.BalanceAs,
                ConversionRate = dat.ConversionRate,
                Rate = dat.Rate,
                AssetType = dat.SenderWalletIdentifier.AssetWallet.AssetType,
                CounterPartyName = dat.SenderWalletIdentifierId == dat.Id ? dat.ReceiverWalletIdentifier.AssetWallet.BaseAssetHolder.Name : dat.SenderWalletIdentifier.AssetWallet.BaseAssetHolder.Name,
                WalletIdentifierInput = dat.SenderWalletIdentifierId == dat.Id ? dat.SenderWalletIdentifier.InputForTransactions : dat.ReceiverWalletIdentifier.InputForTransactions
            });
        }
        
        foreach (var wi in walletIdentifiers)
        {
            if (wi.GetDigitalAssetTransactions(context) != null)
            {
                foreach (var dat in wi.GetDigitalAssetTransactions(context))
                {
                    allTransactions.Add(new StatementTransactionResponse
                    {
                        Id = dat.Id,
                        Date = dat.Date,
                        Description = dat.Description,
                        AssetAmount = dat.SenderWalletIdentifierId == dat.Id ? -dat.AssetAmount : dat.AssetAmount,
                        BalanceAs = dat.BalanceAs,
                        ConversionRate = dat.ConversionRate,
                        Rate = dat.Rate,
                        AssetType = dat.SenderWalletIdentifier.AssetWallet.AssetType,
                        CounterPartyName = dat.SenderWalletIdentifierId == dat.Id ? dat.ReceiverWalletIdentifier.AssetWallet.BaseAssetHolder.Name : dat.SenderWalletIdentifier.AssetWallet.BaseAssetHolder.Name,
                        WalletIdentifierInput = dat.SenderWalletIdentifierId == dat.Id ? dat.SenderWalletIdentifier.InputForTransactions : dat.ReceiverWalletIdentifier.InputForTransactions
                    });
                }
            }

            if (wi.GetFiatAssetTransactions(context) != null)
            {
                foreach (var fat in wi.GetFiatAssetTransactions(context))
                {
                    allTransactions.Add(new StatementTransactionResponse
                    {
                        Id = fat.Id,
                        Date = fat.Date,
                        Description = fat.Description,
                        AssetAmount = fat.SenderWalletIdentifierId == fat.Id ? -fat.AssetAmount : fat.AssetAmount,
                        BalanceAs = null, // Fiat transactions don't have BalanceAs
                        ConversionRate = null, // Fiat transactions don't have ConversionRate
                        Rate = null, // Fiat transactions don't have Rate
                        AssetType = fat.SenderWalletIdentifier.AssetWallet.AssetType,
                        CounterPartyName = fat.SenderWalletIdentifierId == fat.Id ? fat.ReceiverWalletIdentifier.AssetWallet.BaseAssetHolder.Name : fat.SenderWalletIdentifier.AssetWallet.BaseAssetHolder.Name,
                        WalletIdentifierInput = fat.SenderWalletIdentifierId == fat.Id ? fat.SenderWalletIdentifier.InputForTransactions : fat.ReceiverWalletIdentifier.InputForTransactions
                    });
                }
            }
        }
        
        return new StatementAssetHolderWithTransactions
        {
            Id = baseAssetHolderId,
            Name = context.BaseAssetHolders.FirstOrDefault(x => x.Id == baseAssetHolderId)?.Name ?? "",
            Transactions = [.. allTransactions]
        };
    }

    // Helper method to create BaseAssetHolder using base service pattern
    protected async Task<BaseAssetHolder> CreateBaseAssetHolder(string name, string email = null, string cpf = null, string cnpj = null)
    {
        var baseAssetHolder = new BaseAssetHolder
        {
            Name = name,
            Email = email,
            Cpf = cpf,
            Cnpj = cnpj
        };

        // Use the base service pattern for consistency
        await context.BaseAssetHolders.AddAsync(baseAssetHolder);
        await context.SaveChangesAsync();
        
        return baseAssetHolder;
    }
}