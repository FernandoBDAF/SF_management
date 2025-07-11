using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models.AssetInfrastructure;
using SFManagement.Enums;

namespace SFManagement.Services;

public class AssetPoolService(DataContext context, IHttpContextAccessor httpContextAccessor)
    : BaseService<AssetPool>(context, httpContextAccessor)
{
    public override async Task<AssetPool> Add(AssetPool obj)
    {
        // Check if BaseAssetHolder already has an AssetPool for this AssetType
        var existingWallet = await context.AssetPools
            .FirstOrDefaultAsync(aw => aw.BaseAssetHolderId == obj.BaseAssetHolderId && 
                                     aw.AssetType == obj.AssetType && 
                                     !aw.DeletedAt.HasValue);
        
        if (existingWallet != null)
        {
            throw new InvalidOperationException($"BaseAssetHolder already has an AssetPool for {obj.AssetType}");
        }
        
        return await base.Add(obj);
    }

    public async Task<List<AssetPool>> GetAssetPools(Guid assetHolderId)
    {
        return await context.AssetPools
            .Include(aw => aw.BaseAssetHolder)
            .Include(aw => aw.WalletIdentifiers.Where(wi => !wi.DeletedAt.HasValue))
            .Where(aw => aw.BaseAssetHolderId == assetHolderId && !aw.DeletedAt.HasValue)
            .ToListAsync();
    }

    public async Task<AssetPool?> GetAssetPoolByType(AssetType assetType)
    {
        return await context.AssetPools
            .Include(aw => aw.BaseAssetHolder)
            .Include(aw => aw.WalletIdentifiers.Where(wi => !wi.DeletedAt.HasValue))
            .Where(aw => aw.AssetType == assetType && !aw.DeletedAt.HasValue)
            .FirstOrDefaultAsync();
    }

    public override async Task<AssetPool?> Get(Guid id)
    {
        return await context.AssetPools
            .Include(aw => aw.BaseAssetHolder)
            .Include(aw => aw.WalletIdentifiers.Where(wi => !wi.DeletedAt.HasValue))
            .FirstOrDefaultAsync(aw => aw.Id == id && !aw.DeletedAt.HasValue);
    }

    public async Task<AssetPool?> GetByBaseAssetHolderAndType(Guid baseAssetHolderId, AssetType assetType)
    {
        return await context.AssetPools
            .Include(aw => aw.BaseAssetHolder)
            .Include(aw => aw.WalletIdentifiers.Where(wi => !wi.DeletedAt.HasValue))
            .FirstOrDefaultAsync(aw => aw.BaseAssetHolderId == baseAssetHolderId && 
                                     aw.AssetType == assetType && 
                                     !aw.DeletedAt.HasValue);
    }

    // Get wallets with their identifiers grouped by wallet type
    public async Task<Dictionary<WalletType, List<WalletIdentifier>>> GetWalletIdentifiersByType(Guid AssetPoolId)
    {
        var assetPool = await context.AssetPools
            .Include(aw => aw.WalletIdentifiers.Where(wi => !wi.DeletedAt.HasValue))
            .FirstOrDefaultAsync(aw => aw.Id == AssetPoolId && !aw.DeletedAt.HasValue);

        if (assetPool == null)
            return new Dictionary<WalletType, List<WalletIdentifier>>();

        return assetPool.WalletIdentifiers
            .GroupBy(wi => wi.WalletType)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    // Get balance summary for an asset wallet
    public async Task<decimal> GetAssetPoolBalance(Guid AssetPoolId)
    {
        var walletIdentifiers = await context.WalletIdentifiers
            .Where(wi => wi.AssetPoolId == AssetPoolId && !wi.DeletedAt.HasValue)
            .Select(wi => wi.Id)
            .ToListAsync();

        if (!walletIdentifiers.Any())
            return 0;

        // Calculate balance from all transaction types
        var fiatBalance = await context.FiatAssetTransactions
            .Where(ft => !ft.DeletedAt.HasValue && 
                        (walletIdentifiers.Contains(ft.SenderWalletIdentifierId) || 
                         walletIdentifiers.Contains(ft.ReceiverWalletIdentifierId)))
            .SumAsync(ft => walletIdentifiers.Contains(ft.ReceiverWalletIdentifierId) ? ft.AssetAmount : -ft.AssetAmount);

        var digitalBalance = await context.DigitalAssetTransactions
            .Where(dt => !dt.DeletedAt.HasValue && 
                        (walletIdentifiers.Contains(dt.SenderWalletIdentifierId) || 
                         walletIdentifiers.Contains(dt.ReceiverWalletIdentifierId)))
            .SumAsync(dt => walletIdentifiers.Contains(dt.ReceiverWalletIdentifierId) ? dt.AssetAmount : -dt.AssetAmount);

        var settlementBalance = await context.SettlementTransactions
            .Where(st => !st.DeletedAt.HasValue && 
                        (walletIdentifiers.Contains(st.SenderWalletIdentifierId) || 
                         walletIdentifiers.Contains(st.ReceiverWalletIdentifierId)))
            .SumAsync(st => walletIdentifiers.Contains(st.ReceiverWalletIdentifierId) ? st.AssetAmount : -st.AssetAmount);

        return fiatBalance + digitalBalance + settlementBalance;
    }
}