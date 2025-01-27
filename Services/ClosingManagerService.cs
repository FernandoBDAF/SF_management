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
                                              .Include(x => x.Manager.InternalTransactions)
                                              .Include(x => x.Manager.Wallets)
                                              .ThenInclude(x => x.InternalTransactions)
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

            if (closingManager.RakeBruto != decimal.Zero)
            {
                closingManager.Manager.InternalTransactions.Add(ClosingManager.CreateRakeInternalTransaction(closingManager.ManagerId, closingManager.RakeBruto, closingManager.Manager?.Name, closingManager.End, closingManagerId));
            }

            var nicknameDiscounts = ClosingManager.CreateRakeNicknameReleases(closingManager.ManagerId, closingManager.ClosingNicknames, closingManager.Manager.Name, closingManager.End, closingManager.Id);

            closingManager.TotalRakeDiscounts = nicknameDiscounts.Sum(x => x.Value);

            closingManager.Manager.InternalTransactions.AddRange(nicknameDiscounts);

            closingManager.DoneAt = DateTime.Now;

            foreach (var wallet in closingManager.Manager.Wallets)
            {
                var totalBalance = closingManager.ClosingNicknames.Where(x => x.Nickname.WalletId == wallet.Id).Sum(x => x.Balance);

                if (totalBalance != decimal.Zero)
                {
                    wallet.InternalTransactions.Add(new InternalTransaction()
                    {
                        InternalTransactionType = totalBalance > decimal.Zero ? Enums.InternalTransactionType.Expense : Enums.InternalTransactionType.Income,
                        Description = $"Fechamento balanço clube {wallet.Name}",
                        Coins = totalBalance > decimal.Zero ? totalBalance : decimal.Negate(totalBalance),
                        Date = closingManager.End,
                        ApprovedAt = DateTime.Now,
                        ClosingManagerId = closingManagerId,
                        ManagerId = closingManager.ManagerId,
                        ExchangeRate = 1,
                        Value = totalBalance > decimal.Zero ? totalBalance : decimal.Negate(totalBalance)
                    });
                }
            }

            await context.SaveChangesAsync();

            return closingManager;
        }
    }
}
