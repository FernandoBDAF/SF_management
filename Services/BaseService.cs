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
                .Include(c => c.WalletIdentifiers)
                .Include(c => c.Address)
                .Include(c => c.ContactPhones)
                .Include(c => c.InitialBalances)
                .Cast<TEntity>();
        }
        
        return await query.FirstOrDefaultAsync(x => x.Id == id && !x.DeletedAt.HasValue);
    }

    public virtual async Task<TEntity> Add(TEntity obj)
    {
        obj.CreatedAt = DateTime.Now;

        var user = _httpContextAccessor.HttpContext?.User;
        if (user != null && user.Claims.Any(c => c.Type == "uid"))
        {
            var userIdClaim = user.Claims.FirstOrDefault(c => c.Type == "uid")?.Value;
            if (Guid.TryParse(userIdClaim, out Guid userId))
            {
                obj.LastModifiedBy = userId;
            }
        }
        
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
            if (property.Name != "Id" && property.Name != "CreatedAt" && property.Name != "LastModifiedBy")
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

        // Set LastModifiedBy after property copying
        var user = _httpContextAccessor.HttpContext?.User;
        if (user != null && user.Claims.Any(c => c.Type == "uid"))
        {
            var userIdClaim = user.Claims.FirstOrDefault(c => c.Type == "uid")?.Value;
            if (Guid.TryParse(userIdClaim, out Guid userId))
            {
                entity.LastModifiedBy = userId;
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
            
            var user = _httpContextAccessor.HttpContext?.User;
            if (user != null && user.Claims.Any(c => c.Type == "uid"))
            {
                var userIdClaim = user.Claims.FirstOrDefault(c => c.Type == "uid")?.Value;
                if (Guid.TryParse(userIdClaim, out Guid userId))
                {
                    obj.LastModifiedBy = userId;
                }
            }

            _entity.Update(obj);
            await context.SaveChangesAsync();
        }
    }
}