using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models;

namespace SFManagement.Services
{
    public class BaseService<Entity> where Entity : BaseDomain
    {
        public readonly DataContext context;
        public readonly DbSet<Entity> _entity;

        public BaseService(DataContext context)
        {
            this.context = context;
            _entity = context.Set<Entity>();
        }

        public virtual async Task<List<Entity>> List() => await _entity.Where(x => !x.DeletedAt.HasValue).OrderByDescending(x => x.CreatedAt).ToListAsync();

        public virtual async Task<Entity?> Get(Guid id) => await _entity.FirstOrDefaultAsync(x => x.Id == id && !x.DeletedAt.HasValue);

        public virtual async Task<Entity> Add(Entity obj)
        {
            obj.CreatedAt = DateTime.Now;
            await _entity.AddAsync(obj);
            await context.SaveChangesAsync();

            return obj;
        }

        public virtual async Task<Entity> Update(Entity obj)
        {
            obj.UpdatedAt = DateTime.Now;
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
}