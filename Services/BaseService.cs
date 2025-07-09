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
        
        if (typeof(TEntity) == typeof(BaseAssetHolder))
        {
            query = ((IQueryable<BaseAssetHolder>)query)
                .Include(c => c.AssetWallets)
                .ThenInclude(c => c.WalletIdentifiers)
                .Include(c => c.Address)
                .Include(c => c.ContactPhones)
                .Include(c => c.InitialBalances)
                .Cast<TEntity>();
        }
        
        return await query.FirstOrDefaultAsync(x => x.Id == id && !x.DeletedAt.HasValue);
    }

    public virtual async Task<TEntity> Add(TEntity obj)
    {
        // DataContext will handle CreatedAt, CreatedBy, and LastModifiedBy automatically
        await _entity.AddAsync(obj);
        await context.SaveChangesAsync();
        return obj;
    }

    public virtual async Task<TEntity> Update(Guid id, TEntity obj)
    {
        var entity = await Get(id);
        if (entity == null)
            throw new KeyNotFoundException($"Entity with id {id} not found");

        // DataContext will handle UpdatedAt and LastModifiedBy automatically

        // Copy properties to entity
        foreach (var property in typeof(TEntity).GetProperties())
        {
            if (property.Name != "Id" && property.Name != "CreatedAt" && 
                property.Name != "UpdatedAt" && property.Name != "LastModifiedBy" && 
                property.Name != "DeletedAt")
            {
                var value = property.GetValue(obj);
                if (value != null)
                {
                    // Skip empty GUIDs
                    if (property.PropertyType == typeof(Guid) && (Guid)value == Guid.Empty)
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
            obj.DeletedAt = DateTime.UtcNow; // DataContext will set DeletedBy automatically
            
            _entity.Update(obj);
            await context.SaveChangesAsync();
        }
    }
}