using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models;

namespace SFManagement.Services
{
    public class ClosingManagerService : BaseService<ClosingManager>
    {
        public ClosingManagerService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context, httpContextAccessor)
        {
        }

        public override async Task<ClosingManager> Add(ClosingManager obj)
        {
            var manager = await context.Managers.Include(x => x.Wallets).ThenInclude(x => x.Nicknames).FirstOrDefaultAsync(x => x.Id == obj.ManagerId);

            obj.ClosingWallets.AddRange(manager.Wallets.Select(x => new ClosingWallet(x)));

            obj.ClosingNicknames.AddRange(manager.Wallets.SelectMany(x => x.Nicknames.Select(n => new ClosingNickname(n))));

            return await base.Add(obj);
        }
    }
}
