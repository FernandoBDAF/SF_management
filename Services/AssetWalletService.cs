using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models.Entities;
using SFManagement.ViewModels;

namespace SFManagement.Services;

public class AssetWalletService(DataContext context, IHttpContextAccessor httpContextAccessor)
    : BaseService<AssetWallet>(context,
        httpContextAccessor)
{

    public override async Task<AssetWallet> Add(AssetWallet obj)
    {
        EnforceSingleOwner(obj);
        return await base.Add(obj);
    }

    public override async Task<AssetWallet> Update(Guid id, AssetWallet obj)
    {
        EnforceSingleOwner(obj);
        return await base.Update(id, obj);
    }

    private static void EnforceSingleOwner(AssetWallet address)
    {
        var ownerCount = new[] { address.ClientId, address.BankId, address.MemberId, address.PokerManagerId }
            .Count(id => id != null);
        if (ownerCount != 1)
            throw new InvalidOperationException("AssetWallet must be linked to exactly one owner (Client, Bank, Member, or PokerManager).");
    }

    public async Task<List<AssetWallet>> GetWalletsByManagerId(Guid managerId)
    {
        // return await context.Wallets.Where(x => x.ManagerId == managerId).ToListAsync();
        await Task.Yield();
        return null;
    }

    public async Task<BalanceResponse> GetBalance(Guid walletId)
    {
        // var assetWallet = await context.Wallets.Include(x => x.Transactions).Include(x => x.InternalTransactions)
        //     .FirstOrDefaultAsync(x => x.Id == walletId);
        // return new BalanceResponse(assetWallet, null);
        await Task.Yield();
        return null;
    }

    public override async Task<AssetWallet?> Get(Guid id)
    {
        var query = context.AssetWallets
            .Include(w => w.Client)
            .Include(w => w.Member)
            .Include(w => w.Bank)
            .Include(w => w.PokerManager)
            .Include(w => w.WalletIdentifiers);

        var wallet = await query.FirstOrDefaultAsync(x => x.Id == id && !x.DeletedAt.HasValue);
        if (wallet == null)
            return null;

        var ownerCount = 0;
        if (wallet.ClientId != null) ownerCount++;
        if (wallet.MemberId != null) ownerCount++;
        if (wallet.BankId != null) ownerCount++;
        if (wallet.PokerManagerId != null) ownerCount++;

        if (ownerCount != 1)
        {
            // Log the inconsistency (replace with your logger if available)
            Console.WriteLine($"[ERROR] AssetWallet {wallet.Id} has {ownerCount} owners set. Data inconsistency detected.");
            // Optionally, throw or return null
            throw new InvalidOperationException($"AssetWallet {wallet.Id} must have exactly one owner, but has {ownerCount}.");
        }
        return wallet;
    }
}