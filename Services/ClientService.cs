using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Enums;
using SFManagement.Models.Entities;
using SFManagement.Models.Transactions;
using SFManagement.ViewModels;

namespace SFManagement.Services;

public class ClientService : BaseAssetHolderService<Client>
{
    private readonly IMapper _mapper;

    public ClientService(DataContext context, IHttpContextAccessor httpContextAccessor, IMapper mapper) : base(context,
        httpContextAccessor)
    {
        _mapper = mapper;
    }

    // public async Task<BalanceResponse> GetBalance(Guid clientId, DateTime? date)
    // {
    //     // var now = DateTime.Now;
    //     // if (!date.HasValue || date.Value.Year == 1) date = now;
    //     // var client = await context.Clients.Include(x => x.BankTransactions)
    //     //     .Include(x => x.WalletTransactions)
    //     //     .Include(x => x.InternalTransactions)
    //     //     .FirstOrDefaultAsync(x => x.Id == clientId);
    //     //
    //     // return new BalanceResponse(client, date);
    //     await Task.Yield();
    //     return null;
    // }

    public async Task<ClientResponse> UpdateInitialValue(Guid clientId, ClientRequest request)
    {
        var client = await context.Clients.FindAsync(clientId);

        // client.InitialValue = request.InitialValue ?? client.InitialValue;

        await context.SaveChangesAsync();

        return _mapper.Map<ClientResponse>(client);
    }
    
    // public class AssetBalance
    // {
    //     public AssetType AssetType { get; set; }
    //     public decimal? Value { get; set; }
    // }

    // public Dictionary<AssetType, decimal> GetClientBalances(Client client)
    // {
    //     var balances = new Dictionary<AssetType, decimal>();
    //
    //     // 1. DigitalAssetTransactions from AssetWallets' WalletIdentifiers
    //     foreach (var assetWallet in client.AssetWallets ?? Enumerable.Empty<AssetWallet>())
    //     {
    //         foreach (var wi in assetWallet.WalletIdentifiers ?? Enumerable.Empty<WalletIdentifier>())
    //         {
    //             foreach (var tx in wi.DigitalAssetTransactions ?? Enumerable.Empty<DigitalAssetTransaction>())
    //             {
    //                 // LOGIC: This is from AssetWallet, so treat as OUTGOING for the client
    //                 var assetType = assetWallet.AssetType;
    //                 var value = -(tx.AssetAmount); // Outgoing, so subtract
    //                 if (!balances.ContainsKey(assetType)) balances[assetType] = 0;
    //                 balances[assetType] += value;
    //             }
    //         }
    //     }
    //
    //     // 2. FiatAssetTransactions from AssetWallets' WalletIdentifiers
    //     foreach (var assetWallet in client.AssetWallets ?? Enumerable.Empty<AssetWallet>())
    //     {
    //         foreach (var wi in assetWallet.WalletIdentifiers ?? Enumerable.Empty<WalletIdentifier>())
    //         {
    //             foreach (var tx in wi.FiatAssetTransactions ?? Enumerable.Empty<FiatAssetTransaction>())
    //             {
    //                 // LOGIC: This is from AssetWallet, so treat as OUTGOING for the client
    //                 var assetType = assetWallet.AssetType;
    //                 var value = -(tx.AssetAmount); // Outgoing, so subtract
    //                 if (!balances.ContainsKey(assetType)) balances[assetType] = 0;
    //                 balances[assetType] += value;
    //             }
    //         }
    //     }
    //
    //     // 3. DigitalAssetTransactions from Client's own WalletIdentifiers
    //     foreach (var wi in client.WalletIdentifiers ?? Enumerable.Empty<WalletIdentifier>())
    //     {
    //         foreach (var tx in wi.DigitalAssetTransactions ?? Enumerable.Empty<DigitalAssetTransaction>())
    //         {
    //             // LOGIC: This is INCOMING for the client
    //             var assetType = wi.AssetType;
    //             var value = tx.AssetAmount; // Incoming, so add
    //             if (!balances.ContainsKey(assetType)) balances[assetType] = 0;
    //             balances[assetType] += value;
    //         }
    //     }
    //
    //     // 4. FiatAssetTransactions from Client's own WalletIdentifiers
    //     foreach (var wi in client.WalletIdentifiers ?? Enumerable.Empty<WalletIdentifier>())
    //     {
    //         foreach (var tx in wi.FiatAssetTransactions ?? Enumerable.Empty<FiatAssetTransaction>())
    //         {
    //             // LOGIC: This is INCOMING for the client
    //             var assetType = wi.AssetType;
    //             var value = tx.AssetAmount; // Incoming, so add
    //             if (!balances.ContainsKey(assetType)) balances[assetType] = 0;
    //             balances[assetType] += value;
    //         }
    //     }
    //
    //     return balances;
    // }
    
    // public async Task<Dictionary<AssetType, decimal>> GetBalancesByAssetType(Guid clientId)
    // {
    //     var client = await context.Clients
    //         .Where(c => c.Id == clientId)
    //
    //         .Include(c => c.AssetWallets)
    //         .ThenInclude(aw => aw.WalletIdentifiers)
    //         .ThenInclude(wi => wi.DigitalAssetTransactions)
    //
    //         .Include(c => c.AssetWallets)
    //         .ThenInclude(aw => aw.WalletIdentifiers)
    //         .ThenInclude(wi => wi.FiatAssetTransactions)
    //
    //         .Include(c => c.WalletIdentifiers)
    //         .ThenInclude(wi => wi.DigitalAssetTransactions)
    //
    //         .Include(c => c.WalletIdentifiers)
    //         .ThenInclude(wi => wi.FiatAssetTransactions)
    //
    //         .FirstOrDefaultAsync() ?? throw new Exception("Client not found");
    //
    //     return GetClientBalances(client);
    // }
}
