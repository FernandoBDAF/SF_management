using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models;
using SFManagement.Models.Entities;

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
                .Include(b => b.Ofxs)
                .Include(b => b.Address)
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