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
            var list = new List<Tag>();

            var query = await context.Tags.Where(x => !x.ParentId.HasValue).OrderBy(x => x.ParentId).ToListAsync();

            foreach (var tag in query)
            {
                list.Add(tag);
                list.AddRange(await GetChildren(tag.Id));
            }

            return list;
        }

        private async Task<List<Tag>> GetChildren(Guid tagId, int level = 1)
        {
            var list = new List<Tag>();
            var chd = await context.Tags.Where(x => x.ParentId == tagId).ToListAsync();

            foreach (var item in chd)
            {
                var levelDescription = "";

                for (int i = 0; i < level; i++)
                {
                    levelDescription += "-";
                }

                list.Add(new Tag
                {
                    Id = item.Id,
                    Description = $"{levelDescription} {item.Description}",
                });

                list.AddRange(await GetChildren(item.Id, level + 1));
            }

            return list;
        }
    }
}
