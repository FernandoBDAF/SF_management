using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models.AssetInfrastructure;

namespace SFManagement.Services;

public class AssetWalletService(DataContext context, IHttpContextAccessor httpContextAccessor)
    : BaseService<AssetWallet>(context, httpContextAccessor)
{

    public override async Task<AssetWallet> Add(AssetWallet obj)
    {
        var hasAssetWalletType = await context.BaseAssetHolders
            .Include(ah => ah.AssetWallets)
            .AnyAsync(ah => ah.AssetWallets.Any(aw => aw.Id == obj.Id));
        
        if (hasAssetWalletType == true)
        {
            throw new Exception($"Only one asset wallet is allowed.");
        }
        
        return await base.Add(obj);
    }

    // public async Task<List<AssetWallet>> GetWalletsByManagerId(Guid managerId)
    // {
    //     // return await context.Wallets.Where(x => x.ManagerId == managerId).ToListAsync();
    //     await Task.Yield();
    //     return null;
    // }
    //
    // public async Task<BalanceResponse> GetBalance(Guid walletId)
    // {
    //     // var assetWallet = await context.Wallets.Include(x => x.Transactions).Include(x => x.InternalTransactions)
    //     //     .FirstOrDefaultAsync(x => x.Id == walletId);
    //     // return new BalanceResponse(assetWallet, null);
    //     await Task.Yield();
    //     return null;
    // }

    public async Task<List<AssetWallet>> GetAssetWallets(Guid assetHolderId)
    {
        return await context.AssetWallets
            .Where(x => x.BaseAssetHolderId == assetHolderId && !x.DeletedAt.HasValue)
            .ToListAsync();
    }

    public override async Task<AssetWallet?> Get(Guid id)
    {
        var query = context.AssetWallets
            .Include(w => w.BaseAssetHolder);

        var wallet = await query.FirstOrDefaultAsync(x => x.Id == id && !x.DeletedAt.HasValue);
        
        if (wallet == null)
            return null;

        return wallet;
    }
}