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
            .Include(c => c.WalletIdentifiers)
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
            .Include(bah => bah.WalletIdentifiers)
            .Where(bah => bah.WalletIdentifiers.Any(wi => wi.AssetType == assetType && !wi.DeletedAt.HasValue))
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

    public class AssetBalance
    {
        public AssetType AssetType { get; set; }
        public decimal? Value { get; set; }
    }

    public async Task<Dictionary<AssetType, decimal>> GetBalancesByAssetType(Guid baseAssetHolderId)
    {
        var assetHolder = await GetAssetHolderWithTransactions(baseAssetHolderId);
        var balances = new Dictionary<AssetType, decimal>();

        foreach (var aw in assetHolder.AssetWallets)
        {
            foreach (var tx in aw.DigitalAssetTransactions ?? Enumerable.Empty<DigitalAssetTransaction>())
            {
                if (tx.DeletedAt.HasValue)
                {
                    continue;
                }
                var assetType = aw.AssetType;
                var value = tx.TransactionDirection == TransactionDirection.Income ?
                    tx.AssetAmount : -tx.AssetAmount;
                if (!balances.ContainsKey(assetType)) balances[assetType] = 0;
                balances[assetType] += value;
            }
            
            foreach (var tx in aw.FiatAssetTransactions ?? Enumerable.Empty<FiatAssetTransaction>())
            {
                if (tx.DeletedAt.HasValue)
                {
                    continue;
                }
                var assetType = aw.AssetType;
                var value = tx.TransactionDirection == TransactionDirection.Income ?
                    tx.AssetAmount : -tx.AssetAmount;
                if (!balances.ContainsKey(assetType)) balances[assetType] = 0;
                balances[assetType] += value;
            }
        }

        foreach (var wi in assetHolder.WalletIdentifiers)
        {
            foreach (var tx in wi.DigitalAssetTransactions)
            {
                if (tx.DeletedAt.HasValue)
                {
                    continue;
                }
                
                if (tx.BalanceAs.HasValue)
                {
                    var assetType = tx.BalanceAs ?? AssetType.BrazilianReal;
                    var value = tx.TransactionDirection == TransactionDirection.Income ?
                        (tx.AssetAmount * (tx.ConversionRate ?? 1)) : -(tx.AssetAmount * (tx.ConversionRate ?? 1));
                    if (!balances.ContainsKey(assetType)) balances[assetType] = 0;
                    balances[assetType] += value;
                }
                else
                {
                    var assetType = wi.AssetType;
                    var value = tx.TransactionDirection == TransactionDirection.Income ?
                        -tx.AssetAmount : tx.AssetAmount;
                    if (!balances.ContainsKey(assetType)) balances[assetType] = 0;
                    balances[assetType] += value;
                }
            }
            
            foreach (var tx in wi.FiatAssetTransactions)
            {
                if (tx.DeletedAt.HasValue)
                {
                    continue;
                }
                
                var assetType = wi.AssetType;
                var value = tx.TransactionDirection == TransactionDirection.Income ?
                    -tx.AssetAmount : tx.AssetAmount;
                if (!balances.ContainsKey(assetType)) balances[assetType] = 0;
                balances[assetType] += value;
            }
        }

        return balances;
    }
    
    public async Task<BaseAssetHolder> GetAssetHolderWithTransactions(Guid baseAssetHolderId)
    {
        var query = context.BaseAssetHolders.AsQueryable();
        query = query
            .Include(c => c.AssetWallets)
            .ThenInclude(aw => aw.DigitalAssetTransactions)
            .Include(c => c.AssetWallets)
            .ThenInclude(aw => aw.FiatAssetTransactions)

            .Include(c => c.WalletIdentifiers)
            .ThenInclude(wi => wi.DigitalAssetTransactions)
            .Include(c => c.WalletIdentifiers)
            .ThenInclude(wi => wi.FiatAssetTransactions);
            
        var assetHolder = await query.FirstOrDefaultAsync(x => 
                                  x.Id == baseAssetHolderId) ?? throw new Exception("AssetHolder not found");

        return assetHolder;
    }
    
    public async Task<BaseAssetHolder> GetAssetHolderWithTransactionsNoCascade(Guid baseAssetHolderId)
    {
        // Use a completely different approach with projection to avoid cascade
        var query = context.BaseAssetHolders.AsQueryable();
        
        // First, get the asset holder with basic info
        var assetHolder = await query
            .Include(c => c.AssetWallets)
            .Include(c => c.WalletIdentifiers)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == baseAssetHolderId) ?? throw new Exception("AssetHolder not found");

        // Now load transactions separately with minimal includes
        foreach (var aw in assetHolder.AssetWallets)
        {
            // Load DigitalAssetTransactions with minimal WalletIdentifier info
            aw.DigitalAssetTransactions = await context.DigitalAssetTransactions
                .Where(dat => dat.AssetWalletId == aw.Id && !dat.DeletedAt.HasValue)
                .Select(dat => new DigitalAssetTransaction
                {
                    Id = dat.Id,
                    Date = dat.Date,
                    Description = dat.Description,
                    AssetAmount = dat.AssetAmount,
                    TransactionDirection = dat.TransactionDirection,
                    BalanceAs = dat.BalanceAs,
                    ConversionRate = dat.ConversionRate,
                    Rate = dat.Rate,
                    AssetWalletId = dat.AssetWalletId,
                    WalletIdentifierId = dat.WalletIdentifierId,
                    WalletIdentifier = new WalletIdentifier
                    {
                        InputForTransactions = dat.WalletIdentifier.InputForTransactions,
                        AssetType = dat.WalletIdentifier.AssetType,
                        BaseAssetHolder = new BaseAssetHolder
                        {
                            Id = dat.WalletIdentifier.BaseAssetHolder.Id, 
                            Name = dat.WalletIdentifier.BaseAssetHolder.Name
                        },
                       
                    }
                })
                .ToListAsync();

            // Load FiatAssetTransactions with minimal WalletIdentifier info
            aw.FiatAssetTransactions = await context.FiatAssetTransactions
                .Where(fat => fat.AssetWalletId == aw.Id && !fat.DeletedAt.HasValue)
                .Select(fat => new FiatAssetTransaction
                {
                    Id = fat.Id,
                    Date = fat.Date,
                    Description = fat.Description,
                    AssetAmount = fat.AssetAmount,
                    TransactionDirection = fat.TransactionDirection,
                    AssetWalletId = fat.AssetWalletId,
                    WalletIdentifierId = fat.WalletIdentifierId,
                    WalletIdentifier = new WalletIdentifier
                    {
                        Id = fat.WalletIdentifier.Id,
                        InputForTransactions = fat.WalletIdentifier.InputForTransactions,
                        AssetType = fat.WalletIdentifier.AssetType,
                        BaseAssetHolder = new BaseAssetHolder 
                            { 
                                Id = fat.WalletIdentifier.BaseAssetHolder.Id,
                                Name = fat.WalletIdentifier.BaseAssetHolder.Name 
                            },
                    }
                })
                .ToListAsync();
        }

        foreach (var wi in assetHolder.WalletIdentifiers)
        {
            // Load DigitalAssetTransactions with minimal AssetWallet info
            wi.DigitalAssetTransactions = await context.DigitalAssetTransactions
                .Where(dat => dat.WalletIdentifierId == wi.Id && !dat.DeletedAt.HasValue)
                .Select(dat => new DigitalAssetTransaction
                {
                    Id = dat.Id,
                    Date = dat.Date,
                    Description = dat.Description,
                    AssetAmount = dat.AssetAmount,
                    TransactionDirection = dat.TransactionDirection,
                    BalanceAs = dat.BalanceAs,
                    ConversionRate = dat.ConversionRate,
                    Rate = dat.Rate,
                    AssetWalletId = dat.AssetWalletId,
                    WalletIdentifierId = dat.WalletIdentifierId,
                    AssetWallet = new AssetWallet
                    {
                        Id = dat.AssetWallet.Id,
                        AssetType = dat.AssetWallet.AssetType,
                        BaseAssetHolder = new BaseAssetHolder
                        {
                            Id = dat.AssetWallet.BaseAssetHolder.Id, 
                            Name = dat.AssetWallet.BaseAssetHolder.Name
                        },
                    }
                })
                .ToListAsync();

            // Load FiatAssetTransactions with minimal AssetWallet info
            wi.FiatAssetTransactions = await context.FiatAssetTransactions
                .Where(fat => fat.WalletIdentifierId == wi.Id && !fat.DeletedAt.HasValue)
                .Select(fat => new FiatAssetTransaction
                {
                    Id = fat.Id,
                    Date = fat.Date,
                    Description = fat.Description,
                    AssetAmount = fat.AssetAmount,
                    TransactionDirection = fat.TransactionDirection,
                    AssetWalletId = fat.AssetWalletId,
                    WalletIdentifierId = fat.WalletIdentifierId,
                    AssetWallet = new AssetWallet
                    {
                        Id = fat.AssetWallet.Id,
                        AssetType = fat.AssetWallet.AssetType,
                        BaseAssetHolder = new BaseAssetHolder
                        {
                            Id = fat.AssetWallet.BaseAssetHolder.Id, 
                            Name = fat.AssetWallet.BaseAssetHolder.Name
                        },
                    }
                })
                .ToListAsync();
        }

        return assetHolder;
    }

    public async Task<StatementAssetHolderWithTransactions> GetAssetHolderWithTransactionsAsStatement(Guid baseAssetHolderId)
    {
        var assetHolder = await GetAssetHolderWithTransactionsNoCascade(baseAssetHolderId);
        
        var allTransactions = new List<StatementTransactionResponse>();
        
        // Process AssetWallet transactions
        foreach (var aw in assetHolder.AssetWallets ?? Enumerable.Empty<AssetWallet>())
        {
            // Process DigitalAssetTransactions from AssetWallet
            if (aw.DigitalAssetTransactions != null)
            {
                foreach (var dat in aw.DigitalAssetTransactions)
                {
                    var adjustedAmount = dat.AssetAmount;
                    // For AssetWallet: Income (1) = positive, Expense (2) = negative
                    if (dat.TransactionDirection == TransactionDirection.Expense)
                    {
                        adjustedAmount = -adjustedAmount;
                    }
                    
                    // Get counter party name (WalletIdentifier's asset holder)
                    var counterPartyName =  dat.WalletIdentifier?.BaseAssetHolder.Name;
                    
                    allTransactions.Add(new StatementTransactionResponse
                    {
                        Id = dat.Id,
                        Date = dat.Date,
                        Description = dat.Description,
                        AssetAmount = adjustedAmount,
                        BalanceAs = dat.BalanceAs,
                        ConversionRate = dat.ConversionRate,
                        Rate = dat.Rate,
                        AssetType = aw.AssetType,
                        CounterPartyName = counterPartyName,
                        WalletIdentifierInput = dat.WalletIdentifier?.InputForTransactions
                    });
                }
            }
            
            // Process FiatAssetTransactions from AssetWallet
            if (aw.FiatAssetTransactions != null)
            {
                foreach (var fat in aw.FiatAssetTransactions)
                {
                    var adjustedAmount = fat.AssetAmount;
                    // For AssetWallet: Income (1) = positive, Expense (2) = negative
                    if (fat.TransactionDirection == TransactionDirection.Expense)
                    {
                        adjustedAmount = -adjustedAmount;
                    }
                    
                    // Get counter party name (WalletIdentifier's asset holder)
                    var counterPartyName = fat.WalletIdentifier.BaseAssetHolder.Name;
                    
                    allTransactions.Add(new StatementTransactionResponse
                    {
                        Id = fat.Id,
                        Date = fat.Date,
                        Description = fat.Description,
                        AssetAmount = adjustedAmount,
                        BalanceAs = null, // Fiat transactions don't have BalanceAs
                        ConversionRate = null, // Fiat transactions don't have ConversionRate
                        Rate = null, // Fiat transactions don't have Rate
                        AssetType = aw.AssetType,
                        CounterPartyName = counterPartyName,
                        WalletIdentifierInput = fat.WalletIdentifier?.InputForTransactions
                    });
                }
            }
        }
        
        // Process WalletIdentifier transactions
        foreach (var wi in assetHolder.WalletIdentifiers ?? Enumerable.Empty<WalletIdentifier>())
        {
            // Process DigitalAssetTransactions from WalletIdentifier
            if (wi.DigitalAssetTransactions != null)
            {
                foreach (var dat in wi.DigitalAssetTransactions)
                {
                    var adjustedAmount = dat.AssetAmount;
                    // For WalletIdentifier: Income (1) = negative, Expense (2) = positive
                    if (dat.TransactionDirection == TransactionDirection.Income)
                    {
                        adjustedAmount = -adjustedAmount;
                    }
                    
                    // Get counter party name (AssetWallet's asset holder)
                    var counterPartyName = dat.AssetWallet.BaseAssetHolder.Name;
                    
                    allTransactions.Add(new StatementTransactionResponse
                    {
                        Id = dat.Id,
                        Date = dat.Date,
                        Description = dat.Description,
                        AssetAmount = adjustedAmount,
                        BalanceAs = dat.BalanceAs,
                        ConversionRate = dat.ConversionRate,
                        Rate = dat.Rate,
                        AssetType = wi.AssetType,
                        CounterPartyName = counterPartyName,
                        WalletIdentifierInput = wi.InputForTransactions
                    });
                }
            }
            
            // Process FiatAssetTransactions from WalletIdentifier
            if (wi.FiatAssetTransactions != null)
            {
                foreach (var fat in wi.FiatAssetTransactions)
                {
                    var adjustedAmount = fat.AssetAmount;
                    // For WalletIdentifier: Income (1) = negative, Expense (2) = positive
                    if (fat.TransactionDirection == TransactionDirection.Income)
                    {
                        adjustedAmount = -adjustedAmount;
                    }
                    
                    // Get counter party name (AssetWallet's asset holder)
                    var counterPartyName = fat.AssetWallet.BaseAssetHolder.Name;
                    
                    allTransactions.Add(new StatementTransactionResponse
                    {
                        Id = fat.Id,
                        Date = fat.Date,
                        Description = fat.Description,
                        AssetAmount = adjustedAmount,
                        BalanceAs = null, // Fiat transactions don't have BalanceAs
                        ConversionRate = null, // Fiat transactions don't have ConversionRate
                        Rate = null, // Fiat transactions don't have Rate
                        AssetType = wi.AssetType,
                        CounterPartyName = counterPartyName,
                        WalletIdentifierInput = wi.InputForTransactions
                    });
                }
            }
        }
        
        return new StatementAssetHolderWithTransactions
        {
            Id = assetHolder.Id,
            Name = assetHolder.Name,
            Transactions = allTransactions.ToArray()
        };
    }

    // Example method showing how to use the new BaseAssetHolder properties
    public async Task<string> GetAssetHolderTypeName(Guid baseAssetHolderId)
    {
        var assetHolder = await context.BaseAssetHolders
            .Include(bah => bah.Client)
            .Include(bah => bah.Bank)
            .Include(bah => bah.Member)
            .Include(bah => bah.PokerManager)
            .FirstOrDefaultAsync(bah => bah.Id == baseAssetHolderId) ?? throw new Exception("BaseAssetHolder not found");

        // Use the computed property to get the type
        return assetHolder.AssetHolderType switch
        {
            AssetHolderType.Client => "Client",
            AssetHolderType.Bank => "Bank", 
            AssetHolderType.Member => "Member",
            AssetHolderType.PokerManager => "Poker Manager",
            AssetHolderType.Unknown => "Unknown",
            _ => "Unknown"
        };
    }
    
    // Example method showing how to get the specific entity for filtering
    public async Task<List<BaseAssetHolder>> GetAssetHoldersByType(AssetHolderType type)
    {
        // Need to include navigation properties for AssetHolderType computed property to work
        var assetHolders = await context.BaseAssetHolders
            .Include(bah => bah.Client)
            .Include(bah => bah.Bank)
            .Include(bah => bah.Member)
            .Include(bah => bah.PokerManager)
            .Where(bah => bah.AssetHolderType == type)
            .ToListAsync();
            
        return assetHolders;
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