using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models;

namespace SFManagement.Services
{
    public class TagService : BaseService<Tag>
    {
        public TagService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context, httpContextAccessor)
        {
        }

        public override async Task<List<Tag>> List()
        {
            // var list = new List<Tag>();

            var query = await context.Tags.Where(x => !x.ParentId.HasValue).Include(x => x.Children).ThenInclude(x => x.Children).ToListAsync();

            // foreach (var tag in query)
            // {
            //     tag.Children.AddRange(await GetChildren(tag.Id));
            //     list.Add(tag);
            // }

            return query;
        }

        private async Task<List<Tag>> GetChildren(Guid tagId)
        {
            var list = new List<Tag>();
            var chd = await context.Tags.Where(x => x.ParentId == tagId).ToListAsync();

            foreach (var tag in chd)
            {
                tag.Children.AddRange(await GetChildren(tag.Id));
                list.Add(tag);
            }

            return list;
        }
    }
}
