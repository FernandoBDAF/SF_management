using SFManagement.Data;
using SFManagement.Models;

namespace SFManagement.Services
{
    public class TagService : BaseService<Tag>
    {
        public TagService(DataContext context) : base(context)
        {
        }
    }
}
