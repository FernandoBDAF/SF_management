using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models;

namespace SFManagement.Services;

public class BaseService<Entity> where Entity : BaseDomain
{
    public readonly DbSet<Entity> _entity;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public readonly DataContext context;

    public BaseService(DataContext context, IHttpContextAccessor httpContextAccessor)
    {
        this.context = context;
        _entity = context.Set<Entity>();
        _httpContextAccessor = httpContextAccessor;
    }

    public virtual async Task<List<Entity>> List()
    {
        return await _entity.Where(x => !x.DeletedAt.HasValue).OrderByDescending(x => x.CreatedAt).ToListAsync();
    }

    public virtual async Task<Entity?> Get(Guid id)
    {
        var query = _entity.AsQueryable();
        
        // Include all virtual navigation properties
        foreach (var property in typeof(Entity).GetProperties())
        {
            if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
            {
                // Get the corresponding ID property if it exists
                var idPropertyName = property.Name + "Id";
                var idProperty = typeof(Entity).GetProperty(idPropertyName);
                
                if (idProperty != null)
                {
                    // Only include if the ID is not empty
                    query = query.Include(property.Name)
                        .Where(e => EF.Property<Guid?>(e, idPropertyName) != null && 
                                  EF.Property<Guid?>(e, idPropertyName) != Guid.Empty);
                }
                else
                {
                    query = query.Include(property.Name);
                }
            }
        }
        
        return await query.FirstOrDefaultAsync(x => x.Id == id && !x.DeletedAt.HasValue);
    }

    public virtual async Task<Entity> Add(Entity obj)
    {
        obj.CreatedAt = DateTime.Now;

        var user = _httpContextAccessor.HttpContext?.User;

        if (user != null) obj.CreatorId = Guid.Parse(user.Claims.FirstOrDefault(c => c.Type == "uid")?.Value);

        await _entity.AddAsync(obj);
        await context.SaveChangesAsync();

        return obj;
    }

    public virtual async Task<Entity> Update(Guid id, Entity obj)
    {
        var entity = await Get(id);
        if (entity == null)
            throw new KeyNotFoundException($"Entity with id {id} not found");

        entity.UpdatedAt = DateTime.Now;

        // Copy properties from obj to entity
        foreach (var property in typeof(Entity).GetProperties())
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