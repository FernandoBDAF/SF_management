using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models.Entities;
using SFManagement.ViewModels;

namespace SFManagement.Services;

public class PokerManagerService : BaseAssetHolderService<PokerManager>
{
    public PokerManagerService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context,
        httpContextAccessor)
    {
    }

    // Method to handle PokerManagerRequest and create both BaseAssetHolder and PokerManager
    public async Task<PokerManager> AddFromRequest(PokerManagerRequest request)
    {
        // Create BaseAssetHolder using helper method
        var baseAssetHolder = await CreateBaseAssetHolder(
            request.Name, 
            request.Email, 
            request.Cpf, 
            request.Cnpj
        );

        // Create PokerManager using the BaseAssetHolder's ID
        var pokerManager = new PokerManager
        {
            BaseAssetHolderId = baseAssetHolder.Id
        };

        // Use base service to add PokerManager (handles audit automatically)
        var result = await base.Add(pokerManager);

        // Return the poker manager with BaseAssetHolder included
        return await context.PokerManagers
            .Include(pm => pm.BaseAssetHolder)
            .FirstOrDefaultAsync(pm => pm.Id == result.Id);
    }

    // public async Task<BalanceResponse> GetBalance(Guid managerId, DateTime? date)
    // {
    //     // var now = DateTime.Now;
    //     // if (!date.HasValue || date.Value.Year == 1) date = now;
    //     //
    //     // var manager = await context.Managers
    //     //     .Include(x => x.BankTransactions)
    //     //     .Include(x => x.Wallets)
    //     //     .ThenInclude(x => x.Transactions).Include(x => x.InternalTransactions)
    //     //     .Include(x => x.WalletTransactions)
    //     //     .Include(x => x.ClosingManagers)
    //     //     .FirstOrDefaultAsync(x => x.Id == managerId);
    //     // var avgRate = await context.AvgRates.AsNoTracking().OrderByDescending(x => x.Date)
    //     //     .Where(x => x.Date < date && x.ManagerId == managerId).FirstOrDefaultAsync();
    //     // return new BalanceResponse(manager, avgRate, date);
    //     await Task.Yield();
    //     return null;
    // }

    // public async Task<ProfitResponse> GetProfit(Guid managerId, DateTime? start, DateTime? end)
    // {
    //     // var manager = await context.Managers.Include(x => x.BankTransactions).Include(x => x.ClosingManagers)
    //     //     .ThenInclude(x => x.InternalTransactions).Include(x => x.Wallets).ThenInclude(x => x.Transactions)
    //     //     .Include(x => x.InternalTransactions).Include(x => x.WalletTransactions)
    //     //     .FirstOrDefaultAsync(x => x.Id == managerId);
    //     //
    //     // if (start.HasValue && end.HasValue) return new ProfitResponse(manager, start.Value, end.Value);
    //     //
    //     // return new ProfitResponse(manager);
    //     await Task.Yield();
    //     return null;
    // }
}