using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Enums;
using SFManagement.Models.Entities;

namespace SFManagement.Services;

public class BalanceService(DataContext context)
{
    private readonly DataContext _context = context;

    public class AssetBalance
    {
        public AssetType AssetType { get; set; }
        public decimal? Value { get; set; }
    }

    public async Task<Dictionary<AssetType, decimal>> GetBalancesByAssetType(Client client)
    {
        var digitalAssetBalances = await _context.DigitalAssetTransactions
            .Where(x => x.AssetWallet != null && 
                        (x.AssetWallet.Id == client.Id || (x.WalletIdentifier != null && x.WalletIdentifier.Id == client.Id)))
            .Select(x => new AssetBalance { AssetType = x.AssetWallet.AssetType, Value = x.AssetAmount })
            .ToListAsync();

        var fiatAssetBalances = await _context.FiatAssetTransactions
            .Where(x => x.AssetWallet != null && 
                        (x.AssetWallet.Id == client.Id || (x.WalletIdentifier != null && x.WalletIdentifier.Id == client.Id)))
            .Select(x => new AssetBalance { AssetType = x.AssetWallet.AssetType, Value = x.AssetAmount })
            .ToListAsync();

        var allBalances = digitalAssetBalances.Concat(fiatAssetBalances).ToList();

        var balances = allBalances
            .GroupBy(x => x.AssetType)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(x => x.Value ?? 0)
            );

        return balances;
    }
}