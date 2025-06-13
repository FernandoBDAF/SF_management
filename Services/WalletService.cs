using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models.Entities;
using SFManagement.ViewModels;

namespace SFManagement.Services;

public class WalletService(DataContext context, IHttpContextAccessor httpContextAccessor)
    : BaseService<Wallet>(context,
        httpContextAccessor)
{
    public override async Task<Wallet> Add(Wallet obj)
    {
        EnforceSingleOwner(obj);
        return await base.Add(obj);
    }

    public override async Task<Wallet> Update(Guid id, Wallet obj)
    {
        EnforceSingleOwner(obj);
        return await base.Update(id, obj);
    }

    private static void EnforceSingleOwner(Wallet address)
    {
        var ownerCount = new[] { address.ClientId, address.BankId, address.MemberId, address.PokerManagerId }
            .Count(id => id != null);
        if (ownerCount != 1)
            throw new InvalidOperationException("Wallet must be linked to exactly one owner (Client, Bank, Member, or PokerManager).");
    }

    public async Task<List<Wallet>> GetWalletsByManagerId(Guid managerId)
    {
        // return await context.Wallets.Where(x => x.ManagerId == managerId).ToListAsync();
        await Task.Yield();
        return null;
    }

    public async Task<BalanceResponse> GetBalance(Guid walletId)
    {
        // var wallet = await context.Wallets.Include(x => x.Transactions).Include(x => x.InternalTransactions)
        //     .FirstOrDefaultAsync(x => x.Id == walletId);
        // return new BalanceResponse(wallet, null);
        await Task.Yield();
        return null;
    }
}