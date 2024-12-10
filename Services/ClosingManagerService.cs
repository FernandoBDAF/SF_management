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

        public async Task<ClosingManager> Done(Guid closingManagerId)
        {
            var closingManager = await _entity.Include(x => x.ClosingNicknames).ThenInclude(x => x.Nickname)
                                              .Include(x => x.ClosingWallets)
                                              .FirstOrDefaultAsync(x => x.Id == closingManagerId);

            if (closingManager == null)
            {
                throw new AppException("Not found closing manager.");
            }

            if (closingManager.DoneAt.HasValue)
            {
                throw new AppException("Closing manager closed.");
            }

            closingManager.CalculatedAt = DateTime.Now;
            closingManager.RakeBruto = ClosingManager.CalcRake(closingManager.ClosingNicknames, closingManager.ClosingWallets);
            closingManager.TotalBalance = closingManager.ClosingNicknames.Sum(x => x.Balance);


            return closingManager;
        }
    }
}
