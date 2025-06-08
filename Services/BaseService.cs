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
        return await _entity.FirstOrDefaultAsync(x => x.Id == id && !x.DeletedAt.HasValue);
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

        entity.UpdatedAt = DateTime.Now;

        _entity.Update(obj);

        await context.SaveChangesAsync();

        return obj;
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