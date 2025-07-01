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

    // private readonly BaseService<AssetWallet> _assetWalletService;
    // private readonly BaseService<WalletIdentifier> _walletIdentifierService;

    public BaseService(DataContext context, IHttpContextAccessor httpContextAccessor)
    {
        this.context = context;
        _entity = context.Set<TEntity>();
        _httpContextAccessor = httpContextAccessor;
        // _assetWalletService = new BaseService<AssetWallet>(context, httpContextAccessor);
        // _walletIdentifierService = new BaseService<WalletIdentifier>(context, httpContextAccessor);
    }

    public virtual async Task<List<TEntity>> List()
    {
        return await _entity.Where(x => !x.DeletedAt.HasValue).OrderByDescending(x => x.CreatedAt).ToListAsync();
    }

    public virtual async Task<TEntity?> Get(Guid id)
    {
        var query = _entity.AsQueryable();
        
        if (typeof(TEntity) == typeof(Client))
        {
            query = ((IQueryable<Client>)query)
                .Include(c => c.AssetWallets)
                .Include(c => c.WalletIdentifiers)
                .Include(c => c.Address)
                .Include(c => c.ContactPhones)
                .Include(c => c.InitialBalances)
                .Cast<TEntity>();
        }
        
        else if (typeof(TEntity) == typeof(Bank))
        {
            query = ((IQueryable<Bank>)query)
                .Include(c => c.AssetWallets)
                .Include(c => c.WalletIdentifiers)
                .Include(c => c.Address)
                // .Include(c => c.ContactPhones)
                // .Include(c => c.InitialBalances)
                .Include(b => b.Ofxs)
                .Cast<TEntity>();
        }
        
        else if (typeof(TEntity) == typeof(Member))
        {
            query = ((IQueryable<Member>)query)
                .Include(c => c.AssetWallets)
                .Include(c => c.WalletIdentifiers)
                .Include(c => c.Address)
                .Include(c => c.ContactPhones)
                .Include(c => c.InitialBalances)
                .Cast<TEntity>();
        }
        
        else if (typeof(TEntity) == typeof(PokerManager))
        {
            query = ((IQueryable<PokerManager>)query)
                .Include(c => c.AssetWallets)
                .Include(c => c.WalletIdentifiers)
                .Include(c => c.Address)
                .Include(c => c.ContactPhones)
                .Include(c => c.InitialBalances)
                .Include(pm => pm.Excels)
                .Cast<TEntity>();
        }
        
        return await query.FirstOrDefaultAsync(x => x.Id == id && !x.DeletedAt.HasValue);
    }

    public virtual async Task<TEntity> Add(TEntity obj)
    {
        // if (obj is DigitalAssetTransaction || obj is FiatAssetTransaction)
        // {
        //     var aw = await _assetWalletService.Get((obj as BaseTransaction).AssetWalletId);
        //     var wi = await _walletIdentifierService.Get((obj as BaseTransaction).WalletIdentifierId);
        //     if (aw == null || wi == null)
        //     {
        //         throw new ApplicationException("Invalid asset wallet or wallet identifier");
        //     }
        //     if (aw.AssetType != wi.AssetType)
        //     {
        //         throw new Exception("Asset type mismatch");
        //     }
        // }
        
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