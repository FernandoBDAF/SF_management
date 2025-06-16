using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Enums;
using SFManagement.Models;
using SFManagement.Models.Entities;
using SFManagement.Models.Transactions;

namespace SFManagement.Services;

public class BaseService<TEntity> where TEntity : BaseDomain
{
    public readonly DbSet<TEntity> _entity;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public readonly DataContext context;

    public BaseService(DataContext context, IHttpContextAccessor httpContextAccessor)
    {
        this.context = context;
        _entity = context.Set<TEntity>();
        _httpContextAccessor = httpContextAccessor;
    }

    public virtual async Task<List<TEntity>> List()
    {
        return await _entity.Where(x => !x.DeletedAt.HasValue).OrderByDescending(x => x.CreatedAt).ToListAsync();
    }

    public virtual async Task<TEntity?> Get(Guid id)
    {
        var query = _entity.AsQueryable();
        
        // Special handling for Client entity
        if (typeof(TEntity) == typeof(Client))
        {
            query = ((IQueryable<Client>)query)
                .Include(c => c.ContactPhones)
                .Include(c => c.InitialBalances)
                .Include(c => c.AssetWallets)
                .Include(c => c.WalletIdentifiers)
                .Include(c => c.Address)
                .Cast<TEntity>();
        }
        // Special handling for Bank entity
        else if (typeof(TEntity) == typeof(Bank))
        {
            query = ((IQueryable<Bank>)query)
                .Include(b => b.ContactPhones)
                .Include(b => b.InitialBalances)
                .Include(b => b.AssetWallets)
                .Include(c => c.WalletIdentifiers)
                .Include(b => b.Address)
                .Include(b => b.Ofxs)
                .Cast<TEntity>();
        }
        // Special handling for Member entity
        else if (typeof(TEntity) == typeof(Member))
        {
            query = ((IQueryable<Member>)query)
                .Include(m => m.PhonesNumbers)
                .Include(m => m.InitialBalances)
                .Include(m => m.AssetWallets)
                .Include(m => m.WalletIdentifiers)
                .Include(m => m.Address)
                .Cast<TEntity>();
        }
        // Special handling for PokerManager entity
        else if (typeof(TEntity) == typeof(PokerManager))
        {
            query = ((IQueryable<PokerManager>)query)
                .Include(pm => pm.PhonesNumbers)
                .Include(pm => pm.InitialBalances)
                .Include(pm => pm.AssetWallets)
                .Include(pm => pm.WalletIdentifiers)
                .Include(pm => pm.Excels)
                .Include(pm => pm.Address)
                .Cast<TEntity>();
        }
        
        return await query.FirstOrDefaultAsync(x => x.Id == id && !x.DeletedAt.HasValue);
    }

    public virtual async Task<TEntity> Add(TEntity obj)
    {
        obj.CreatedAt = DateTime.Now;

        var user = _httpContextAccessor.HttpContext?.User;

        if (user != null) obj.CreatorId = Guid.Parse(user.Claims.FirstOrDefault(c => c.Type == "uid")?.Value);

        await _entity.AddAsync(obj);
        await context.SaveChangesAsync();

        return obj;
    }

    public virtual async Task<TEntity> Update(Guid id, TEntity obj)
    {
        var entity = await Get(id);
        if (entity == null)
            throw new KeyNotFoundException($"Entity with id {id} not found");

        entity.UpdatedAt = DateTime.Now;

        // Copy properties from obj to entity
        foreach (var property in typeof(TEntity).GetProperties())
        {
            if (property.Name != "Id" && property.Name != "CreatedAt" && property.Name != "CreatorId")
            {
                var value = property.GetValue(obj);
                if (value != null)
                {
                    // Skip empty GUIDs
                    if (property.PropertyType == typeof(Guid) && (Guid)value == Guid.Empty)
                        continue;
                    if (property.PropertyType == typeof(Guid?) && ((Guid?)value) == Guid.Empty)
                        continue;
                        
                    property.SetValue(entity, value);
                }
            }
        }

        _entity.Update(entity);
        await context.SaveChangesAsync();

        return entity;
    }

    public virtual async Task Delete(Guid id)
    {
        var obj = await _entity.FirstOrDefaultAsync(x => x.Id == id && !x.DeletedAt.HasValue);
        if (obj != null)
        {
            obj.DeletedAt = DateTime.Now;
            _entity.Update(obj);

            await context.SaveChangesAsync();
        }
    }


    // public virtual async Task Balance(Guid id)
    // {
    //     if (typeof(TEntity) != typeof(Client) && 
    //     typeof(TEntity) != typeof(Member) && 
    //     typeof(TEntity) != typeof(PokerManager) && 
    //     typeof(TEntity) != typeof(Bank))
    //         throw new InvalidOperationException("Balance can only be calculated for Client, Member, PokerManager or Bank");

    //     var client = await Get(id);
    //     if (client == null)
    //         throw new KeyNotFoundException($"Client with id {id} not found");

    //     var balanceService = new BalanceService(context);
    //     var balances = await balanceService.GetBalancesByAssetType(client);
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
                var assetType = aw.AssetType;
                var value = tx.TransactionDirection == TransactionDirection.Income ?
                    (tx.AssetAmount) : -(tx.AssetAmount);
                if (!balances.ContainsKey(assetType)) balances[assetType] = 0;
                balances[assetType] += value;
            }
            
            foreach (var tx in aw.FiatAssetTransactions ?? Enumerable.Empty<FiatAssetTransaction>())
            {
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
                if (tx.ConvertTo.HasValue)
                {
                    var assetType = tx.ConvertTo ?? AssetType.BrazilianReal;
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
            .ThenInclude(wi => wi.DigitalAssetTransactions)

            .Include(c => c.AssetWallets)
            .ThenInclude(wi => wi.FiatAssetTransactions)

            .Include(c => c.WalletIdentifiers)
            .ThenInclude(wi => wi.DigitalAssetTransactions)

            .Include(c => c.WalletIdentifiers)
            .ThenInclude(wi => wi.FiatAssetTransactions);

        TEntity obj;
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
                                  x.Id == id) ?? 
                              throw new Exception("AssetHolder not found");

        return assetHolder;
    }
}