using SFManagement.Data;
using SFManagement.Enums;
using SFManagement.Models.Entities;
using SFManagement.Models.Transactions;
using SFManagement.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace SFManagement.Services;

public class BaseAssetHolderService<TEntity> : BaseService<TEntity> where TEntity : BaseAssetHolder
{
    public BaseAssetHolderService(DataContext context, IHttpContextAccessor httpContextAccessor) 
        : base(context, httpContextAccessor)
    {
    }

    // public virtual async Task<TEntity?> Get(Guid id)
    // {
    //     var query = _entity.AsQueryable();
    //     
    //     if (typeof(TEntity) == typeof(Client))
    //     {
    //         query = ((IQueryable<Client>)query)
    //             .Include(c => c.AssetWallets)
    //             .Include(c => c.WalletIdentifiers)
    //             .Include(c => c.Address)
    //             .Include(c => c.ContactPhones)
    //             .Include(c => c.InitialBalances)
    //             .Cast<TEntity>();
    //     }
    //     
    //     else if (typeof(TEntity) == typeof(Bank))
    //     {
    //         query = ((IQueryable<Bank>)query)
    //             .Include(c => c.AssetWallets)
    //             .Include(c => c.WalletIdentifiers)
    //             .Include(c => c.Address)
    //             // .Include(c => c.ContactPhones)
    //             // .Include(c => c.InitialBalances)
    //             .Include(b => b.Ofxs)
    //             .Cast<TEntity>();
    //     }
    //     
    //     else if (typeof(TEntity) == typeof(Member))
    //     {
    //         query = ((IQueryable<Member>)query)
    //             .Include(c => c.AssetWallets)
    //             .Include(c => c.WalletIdentifiers)
    //             .Include(c => c.Address)
    //             .Include(c => c.ContactPhones)
    //             .Include(c => c.InitialBalances)
    //             .Cast<TEntity>();
    //     }
    //     
    //     else if (typeof(TEntity) == typeof(PokerManager))
    //     {
    //         query = ((IQueryable<PokerManager>)query)
    //             .Include(c => c.AssetWallets)
    //             .Include(c => c.WalletIdentifiers)
    //             .Include(c => c.Address)
    //             .Include(c => c.ContactPhones)
    //             .Include(c => c.InitialBalances)
    //             .Include(pm => pm.Excels)
    //             .Cast<TEntity>();
    //     }
    //     
    //     return await query.FirstOrDefaultAsync(x => x.Id == id && !x.DeletedAt.HasValue);
    // }
    
    public class AssetBalance
    {
        public AssetType AssetType { get; set; }
        public decimal? Value { get; set; }
    }

    public async Task<Dictionary<AssetType, decimal>> GetBalancesByAssetType(Guid id)
    {
        var assetHolder = await GetAssetHolderWithTransactions(id);
            
        var balances = new Dictionary<AssetType, decimal>();

        foreach (var aw in assetHolder.AssetWallets ?? Enumerable.Empty<AssetWallet>())
        {
            foreach (var tx in aw.DigitalAssetTransactions ?? Enumerable.Empty<DigitalAssetTransaction>())
            {
                if (tx.DeletedAt.HasValue)
                {
                    continue;
                }
                var assetType = aw.AssetType;
                var value = tx.TransactionDirection == TransactionDirection.Income ?
                    (tx.AssetAmount) : -(tx.AssetAmount);
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
                    (tx.AssetAmount) : -(tx.AssetAmount);
                if (!balances.ContainsKey(assetType)) balances[assetType] = 0;
                balances[assetType] += value;
            }
        }

        foreach (var wi in assetHolder.WalletIdentifiers ?? Enumerable.Empty<WalletIdentifier>())
        {
            foreach (var tx in wi.DigitalAssetTransactions ?? Enumerable.Empty<DigitalAssetTransaction>())
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
                        -(tx.AssetAmount) : (tx.AssetAmount);
                    if (!balances.ContainsKey(assetType)) balances[assetType] = 0;
                    balances[assetType] += value;
                }
            }
            
            foreach (var tx in wi.FiatAssetTransactions ?? Enumerable.Empty<FiatAssetTransaction>())
            {
                if (tx.DeletedAt.HasValue)
                {
                    continue;
                }
                
                var assetType = wi.AssetType;
                var value = tx.TransactionDirection == TransactionDirection.Income ?
                    -(tx.AssetAmount) : (tx.AssetAmount);
                if (!balances.ContainsKey(assetType)) balances[assetType] = 0;
                balances[assetType] += value;
            }
        }

        return balances;
    }
    
    public async Task<BaseAssetHolder> GetAssetHolderWithTransactions(Guid id)
    {
        var query = (IQueryable<BaseAssetHolder>)_entity.AsQueryable();
        query = query
            .Include(c => c.AssetWallets)
            .ThenInclude(aw => aw.DigitalAssetTransactions)
            .Include(c => c.AssetWallets)
            .ThenInclude(aw => aw.FiatAssetTransactions)

            .Include(c => c.WalletIdentifiers)
            .ThenInclude(wi => wi.DigitalAssetTransactions)
            .Include(c => c.WalletIdentifiers)
            .ThenInclude(wi => wi.FiatAssetTransactions);
            

        if (typeof(TEntity) == typeof(Client))
        {
            query = query.Cast<Client>();
        }
        else if (typeof(TEntity) == typeof(Bank))
        {
            query = query.Cast<Bank>();
        }
        else if (typeof(TEntity) == typeof(Member))
        {
            query = query.Cast<Member>();
        }
        else if (typeof(TEntity) == typeof(PokerManager))
        {
            query = query.Cast<PokerManager>();
        }
        else
        {
            throw new KeyNotFoundException($"Entity type {typeof(TEntity).Name} is not supported");
        }
        

        var assetHolder = await query.FirstOrDefaultAsync(x => 
                                  x.Id == id) ?? throw new Exception("AssetHolder not found");

        return assetHolder;
    }
    
    public async Task<BaseAssetHolder> GetAssetHolderWithTransactionsNoCascade(Guid id)
    {
        // Use a completely different approach with projection to avoid cascade
        var query = (IQueryable<BaseAssetHolder>)_entity.AsQueryable();
        
        if (typeof(TEntity) == typeof(Client))
        {
            query = query.Cast<Client>();
        }
        else if (typeof(TEntity) == typeof(Bank))
        {
            query = query.Cast<Bank>();
        }
        else if (typeof(TEntity) == typeof(Member))
        {
            query = query.Cast<Member>();
        }
        else if (typeof(TEntity) == typeof(PokerManager))
        {
            query = query.Cast<PokerManager>();
        }
        else
        {
            throw new KeyNotFoundException($"Entity type {typeof(TEntity).Name} is not supported");
        }

        // First, get the asset holder with basic info
        var assetHolder = await query
            .Include(c => c.AssetWallets)
            .Include(c => c.WalletIdentifiers)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id) ?? throw new Exception("AssetHolder not found");

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
                        // Id = dat.WalletIdentifier.Id,
                        // RouteInfo = dat.WalletIdentifier.RouteInfo,
                        // IdentifierInfo = dat.WalletIdentifier.IdentifierInfo,
                        // DescriptiveInfo = dat.WalletIdentifier.DescriptiveInfo,
                        // ExtraInfo = dat.WalletIdentifier.ExtraInfo,
                        InputForTransactions = dat.WalletIdentifier.InputForTransactions,
                        AssetType = dat.WalletIdentifier.AssetType,
                        // DefaultRakeCommission = dat.WalletIdentifier.DefaultRakeCommission,
                        // DefaultParentCommission = dat.WalletIdentifier.DefaultParentCommission,
                        BankId = dat.WalletIdentifier.BankId,
                        ClientId = dat.WalletIdentifier.ClientId,
                        MemberId = dat.WalletIdentifier.MemberId,
                        PokerManagerId = dat.WalletIdentifier.PokerManagerId,
                        Bank = dat.WalletIdentifier.BankId.HasValue ? new Bank { Id = dat.WalletIdentifier.Bank.Id, Name = dat.WalletIdentifier.Bank.Name } : null,
                        Client = dat.WalletIdentifier.ClientId.HasValue ? new Client { Id = dat.WalletIdentifier.Client.Id, Name = dat.WalletIdentifier.Client.Name } : null,
                        PokerManager = dat.WalletIdentifier.PokerManagerId.HasValue ? new PokerManager { Id = dat.WalletIdentifier.PokerManager.Id, Name = dat.WalletIdentifier.PokerManager.Name } : null
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
                    // TagId = fat.TagId,
                    // ApprovedAt = fat.ApprovedAt,
                    WalletIdentifier = new WalletIdentifier
                    {
                        Id = fat.WalletIdentifier.Id,
                        // RouteInfo = fat.WalletIdentifier.RouteInfo,
                        // IdentifierInfo = fat.WalletIdentifier.IdentifierInfo,
                        // DescriptiveInfo = fat.WalletIdentifier.DescriptiveInfo,
                        // ExtraInfo = fat.WalletIdentifier.ExtraInfo,
                        InputForTransactions = fat.WalletIdentifier.InputForTransactions,
                        AssetType = fat.WalletIdentifier.AssetType,
                        // DefaultRakeCommission = fat.WalletIdentifier.DefaultRakeCommission,
                        // DefaultParentCommission = fat.WalletIdentifier.DefaultParentCommission,
                        BankId = fat.WalletIdentifier.BankId,
                        ClientId = fat.WalletIdentifier.ClientId,
                        MemberId = fat.WalletIdentifier.MemberId,
                        PokerManagerId = fat.WalletIdentifier.PokerManagerId,
                        Bank = fat.WalletIdentifier.BankId.HasValue ? new Bank { Id = fat.WalletIdentifier.Bank.Id, Name = fat.WalletIdentifier.Bank.Name } : null,
                        Client = fat.WalletIdentifier.ClientId.HasValue ? new Client { Id = fat.WalletIdentifier.Client.Id, Name = fat.WalletIdentifier.Client.Name } : null,
                        PokerManager = fat.WalletIdentifier.PokerManagerId.HasValue ? new PokerManager { Id = fat.WalletIdentifier.PokerManager.Id, Name = fat.WalletIdentifier.PokerManager.Name } : null
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
                    // Profit = dat.Profit,
                    AssetWalletId = dat.AssetWalletId,
                    WalletIdentifierId = dat.WalletIdentifierId,
                    // TagId = dat.TagId,
                    // ApprovedAt = dat.ApprovedAt,
                    AssetWallet = new AssetWallet
                    {
                        Id = dat.AssetWallet.Id,
                        AssetType = dat.AssetWallet.AssetType,
                        // DefaultAgreedCommission = dat.AssetWallet.DefaultAgreedCommission,
                        ClientId = dat.AssetWallet.ClientId,
                        MemberId = dat.AssetWallet.MemberId,
                        BankId = dat.AssetWallet.BankId,
                        PokerManagerId = dat.AssetWallet.PokerManagerId,
                        Bank = dat.AssetWallet.BankId.HasValue ? new Bank { Id = dat.AssetWallet.Bank.Id, Name = dat.AssetWallet.Bank.Name } : null,
                        Client = dat.AssetWallet.ClientId.HasValue ? new Client { Id = dat.AssetWallet.Client.Id, Name = dat.AssetWallet.Client.Name } : null,
                        PokerManager = dat.AssetWallet.PokerManagerId.HasValue ? new PokerManager { Id = dat.AssetWallet.PokerManager.Id, Name = dat.AssetWallet.PokerManager.Name } : null
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
                    // TagId = fat.TagId,
                    // ApprovedAt = fat.ApprovedAt,
                    AssetWallet = new AssetWallet
                    {
                        Id = fat.AssetWallet.Id,
                        AssetType = fat.AssetWallet.AssetType,
                        // DefaultAgreedCommission = fat.AssetWallet.DefaultAgreedCommission,
                        ClientId = fat.AssetWallet.ClientId,
                        MemberId = fat.AssetWallet.MemberId,
                        BankId = fat.AssetWallet.BankId,
                        PokerManagerId = fat.AssetWallet.PokerManagerId,
                        Bank = fat.AssetWallet.BankId.HasValue ? new Bank { Id = fat.AssetWallet.Bank.Id, Name = fat.AssetWallet.Bank.Name } : null,
                        Client = fat.AssetWallet.ClientId.HasValue ? new Client { Id = fat.AssetWallet.Client.Id, Name = fat.AssetWallet.Client.Name } : null,
                        PokerManager = fat.AssetWallet.PokerManagerId.HasValue ? new PokerManager { Id = fat.AssetWallet.PokerManager.Id, Name = fat.AssetWallet.PokerManager.Name } : null
                    }
                })
                .ToListAsync();
        }

        return assetHolder;
    }

    public async Task<StatementAssetHolderWithTransactions> GetAssetHolderWithTransactionsAsStatement(Guid id)
    {
        var assetHolder = await GetAssetHolderWithTransactionsNoCascade(id);
        
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
                    string? counterPartyName = null;
                    if (dat.WalletIdentifier != null)
                    {
                        if (dat.WalletIdentifier.Bank != null)
                            counterPartyName = dat.WalletIdentifier.Bank.Name;
                        else if (dat.WalletIdentifier.Client != null)
                            counterPartyName = dat.WalletIdentifier.Client.Name;
                        else if (dat.WalletIdentifier.Member != null)
                            counterPartyName = dat.WalletIdentifier.Member.Name;
                        else if (dat.WalletIdentifier.PokerManager != null)
                            counterPartyName = dat.WalletIdentifier.PokerManager.Name;
                    }
                    
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
                    string? counterPartyName = null;
                    if (fat.WalletIdentifier != null)
                    {
                        if (fat.WalletIdentifier.Bank != null)
                            counterPartyName = fat.WalletIdentifier.Bank.Name;
                        else if (fat.WalletIdentifier.Client != null)
                            counterPartyName = fat.WalletIdentifier.Client.Name;
                        else if (fat.WalletIdentifier.Member != null)
                            counterPartyName = fat.WalletIdentifier.Member.Name;
                        else if (fat.WalletIdentifier.PokerManager != null)
                            counterPartyName = fat.WalletIdentifier.PokerManager.Name;
                    }
                    
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
                    string? counterPartyName = null;
                    if (dat.AssetWallet != null)
                    {
                        if (dat.AssetWallet.Bank != null)
                            counterPartyName = dat.AssetWallet.Bank.Name;
                        else if (dat.AssetWallet.Client != null)
                            counterPartyName = dat.AssetWallet.Client.Name;
                        else if (dat.AssetWallet.Member != null)
                            counterPartyName = dat.AssetWallet.Member.Name;
                        else if (dat.AssetWallet.PokerManager != null)
                            counterPartyName = dat.AssetWallet.PokerManager.Name;
                    }
                    
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
                    string? counterPartyName = null;
                    if (fat.AssetWallet != null)
                    {
                        if (fat.AssetWallet.Bank != null)
                            counterPartyName = fat.AssetWallet.Bank.Name;
                        else if (fat.AssetWallet.Client != null)
                            counterPartyName = fat.AssetWallet.Client.Name;
                        else if (fat.AssetWallet.Member != null)
                            counterPartyName = fat.AssetWallet.Member.Name;
                        else if (fat.AssetWallet.PokerManager != null)
                            counterPartyName = fat.AssetWallet.PokerManager.Name;
                    }
                    
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
}