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

        var settlementTransactions = await context.SettlementTransactions
            .Where(st => !st.DeletedAt.HasValue && 
                (walletIdentifierIds.Contains(st.SenderWalletIdentifierId) || 
                 walletIdentifierIds.Contains(st.ReceiverWalletIdentifierId)))
            .Include(st => st.SenderWalletIdentifier)
                .ThenInclude(wi => wi.AssetWallet)
            .Include(st => st.ReceiverWalletIdentifier)
                .ThenInclude(wi => wi.AssetWallet)
            .ToArrayAsync() ?? [];

        // Process digital asset transactions using helper methods
        foreach (var tx in digitalTransactions)
        {
            var relevantWalletId = walletIdentifierIds.FirstOrDefault(id => 
                tx.SenderWalletIdentifierId == id || tx.ReceiverWalletIdentifierId == id);
            
            var assetType = tx.IsReceiver(relevantWalletId) ? 
                tx.ReceiverWalletIdentifier.AssetWallet.AssetType : 
                tx.SenderWalletIdentifier.AssetWallet.AssetType;
            
            var signedAmount = tx.GetSignedAmountForWalletIdentifier(relevantWalletId);
            
            if (!balances.ContainsKey(assetType)) balances[assetType] = 0;
            balances[assetType] += signedAmount;
        }

        // Process fiat asset transactions using helper methods
        foreach (var tx in fiatTransactions)
        {
            var relevantWalletId = walletIdentifierIds.FirstOrDefault(id => 
                tx.SenderWalletIdentifierId == id || tx.ReceiverWalletIdentifierId == id);
            
            var assetType = tx.IsReceiver(relevantWalletId) ? 
                tx.ReceiverWalletIdentifier.AssetWallet.AssetType : 
                tx.SenderWalletIdentifier.AssetWallet.AssetType;
            
            var signedAmount = tx.GetSignedAmountForWalletIdentifier(relevantWalletId);
            
            if (!balances.ContainsKey(assetType)) balances[assetType] = 0;
            balances[assetType] += signedAmount;
        }

        // Process settlement transactions using helper methods
        foreach (var tx in settlementTransactions)
        {
            var relevantWalletId = walletIdentifierIds.FirstOrDefault(id => 
                tx.SenderWalletIdentifierId == id || tx.ReceiverWalletIdentifierId == id);
            
            var assetType = tx.IsReceiver(relevantWalletId) ? 
                tx.ReceiverWalletIdentifier.AssetWallet.AssetType : 
                tx.SenderWalletIdentifier.AssetWallet.AssetType;
            
            var signedAmount = tx.GetSignedAmountForWalletIdentifier(relevantWalletId);
            
            if (!balances.ContainsKey(assetType)) balances[assetType] = 0;
            balances[assetType] += signedAmount;
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

    public async Task<StatementAssetHolderWithTransactions> GetTransactionsStatementForAssetHolder(Guid baseAssetHolderId)
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

        var fiatTransactions = await context.FiatAssetTransactions
            .Where(ft => !ft.DeletedAt.HasValue && 
                (walletIdentifierIds.Contains(ft.SenderWalletIdentifierId) || 
                 walletIdentifierIds.Contains(ft.ReceiverWalletIdentifierId)))
            .Include(ft => ft.SenderWalletIdentifier)
                .ThenInclude(wi => wi.AssetWallet)
                    .ThenInclude(wi => wi.BaseAssetHolder)
            .Include(ft => ft.ReceiverWalletIdentifier)
                .ThenInclude(wi => wi.AssetWallet)
                    .ThenInclude(wi => wi.BaseAssetHolder)
            .ToArrayAsync() ?? [];

        var settlementTransactions = await context.SettlementTransactions
            .Where(st => !st.DeletedAt.HasValue && 
                (walletIdentifierIds.Contains(st.SenderWalletIdentifierId) || 
                 walletIdentifierIds.Contains(st.ReceiverWalletIdentifierId)))
            .Include(st => st.SenderWalletIdentifier)
                .ThenInclude(wi => wi.AssetWallet)
                    .ThenInclude(wi => wi.BaseAssetHolder)
            .Include(st => st.ReceiverWalletIdentifier)
                .ThenInclude(wi => wi.AssetWallet)
                    .ThenInclude(wi => wi.BaseAssetHolder)
            .ToArrayAsync() ?? [];
        
        var allTransactions = new List<StatementTransactionResponse>();

        // Process digital asset transactions using helper methods
        foreach (var dat in digitalTransactions)
        {
            var relevantWalletId = walletIdentifierIds.FirstOrDefault(id => 
                dat.SenderWalletIdentifierId == id || dat.ReceiverWalletIdentifierId == id);

            allTransactions.Add(new StatementTransactionResponse
            {
                Id = dat.Id,
                Date = dat.Date,
                Description = dat.Description,
                AssetAmount = dat.GetSignedAmountForWalletIdentifier(relevantWalletId),
                BalanceAs = dat.BalanceAs,
                ConversionRate = dat.ConversionRate,
                Rate = dat.Rate,
                AssetType = dat.SenderWalletIdentifier.AssetWallet.AssetType,
                CounterPartyName = dat.GetCounterPartyName(relevantWalletId),
                WalletIdentifierInput = dat.GetWalletIdentifierInput(relevantWalletId)
            });
        }

        // Process fiat asset transactions using helper methods
        foreach (var fat in fiatTransactions)
        {
            var relevantWalletId = walletIdentifierIds.FirstOrDefault(id => 
                fat.SenderWalletIdentifierId == id || fat.ReceiverWalletIdentifierId == id);

            allTransactions.Add(new StatementTransactionResponse
            {
                Id = fat.Id,
                Date = fat.Date,
                Description = fat.Description,
                AssetAmount = fat.GetSignedAmountForWalletIdentifier(relevantWalletId),
                BalanceAs = null, // Fiat transactions don't have BalanceAs
                ConversionRate = null, // Fiat transactions don't have ConversionRate
                Rate = null, // Fiat transactions don't have Rate
                AssetType = fat.SenderWalletIdentifier.AssetWallet.AssetType,
                CounterPartyName = fat.GetCounterPartyName(relevantWalletId),
                WalletIdentifierInput = fat.GetWalletIdentifierInput(relevantWalletId)
            });
        }

        // Process settlement transactions using helper methods
        foreach (var st in settlementTransactions)
        {
            var relevantWalletId = walletIdentifierIds.FirstOrDefault(id => 
                st.SenderWalletIdentifierId == id || st.ReceiverWalletIdentifierId == id);

            allTransactions.Add(new StatementTransactionResponse
            {
                Id = st.Id,
                Date = st.Date,
                Description = st.Description,
                AssetAmount = st.GetSignedAmountForWalletIdentifier(relevantWalletId),
                BalanceAs = null, // Settlement transactions don't have BalanceAs
                ConversionRate = null, // Settlement transactions don't have ConversionRate
                Rate = null, // Settlement transactions don't have Rate
                AssetType = st.SenderWalletIdentifier.AssetWallet.AssetType,
                CounterPartyName = st.GetCounterPartyName(relevantWalletId),
                WalletIdentifierInput = st.GetWalletIdentifierInput(relevantWalletId)
            });
        }
        
        return new StatementAssetHolderWithTransactions
        {
            Id = baseAssetHolderId,
            Name = context.BaseAssetHolders.FirstOrDefault(x => x.Id == baseAssetHolderId)?.Name ?? "",
            Transactions = allTransactions.OrderByDescending(t => t.Date).ToArray()
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