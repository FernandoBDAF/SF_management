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

    public override async Task<Wallet?> Get(Guid id)
    {
        var query = context.Wallets
            .Include(w => w.Client)
            .Include(w => w.Member)
            .Include(w => w.Bank)
            .Include(w => w.PokerManager)
            .Include(w => w.WalletIdentifiers);

        var wallet = await query.FirstOrDefaultAsync(x => x.Id == id && !x.DeletedAt.HasValue);
        if (wallet == null)
            return null;

        int ownerCount = 0;
        if (wallet.ClientId != null) ownerCount++;
        if (wallet.MemberId != null) ownerCount++;
        if (wallet.BankId != null) ownerCount++;
        if (wallet.PokerManagerId != null) ownerCount++;

        if (ownerCount != 1)
        {
            // Log the inconsistency (replace with your logger if available)
            Console.WriteLine($"[ERROR] Wallet {wallet.Id} has {ownerCount} owners set. Data inconsistency detected.");
            // Optionally, throw or return null
            throw new InvalidOperationException($"Wallet {wallet.Id} must have exactly one owner, but has {ownerCount}.");
        }
        return wallet;
    }
}