using System.Linq;
using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.ViewModels;
using AutoMapper;
using SFManagement.Models.Entities;

namespace SFManagement.Services;

public class TransactionService<TEntity> where TEntity : BaseAssetHolder
{
    public readonly DbSet<TEntity> _entity;
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public TransactionService(DataContext context, IMapper mapper)
    {
       this._context = context;
       _entity = context.Set<TEntity>();
        _mapper = mapper;
    }
    
    // public async Task<TableResponse<FiatAssetTransactionResponse>> GetBankFiatAssetTransactions(Guid[] bankAssetWalletIds, DateTime? startDate,
    //     DateTime? endDate, int quantity, int page)
    // {
    //     var response = new TableResponse<FiatAssetTransactionResponse>
    //     {
    //         Page = page,
    //         Show = quantity
    //     };
    //     
    //     var transactionsQuery = _context.FiatAssetTransactions
    //         .Where(x => !x.DeletedAt.HasValue && bankAssetWalletIds.Contains(x.AssetWalletId));
    //     
    //     if (startDate.HasValue)
    //     {
    //         transactionsQuery = transactionsQuery.Where(x => x.Date >= startDate.Value);
    //     }
    //     
    //     if (endDate.HasValue)
    //     {
    //         transactionsQuery = transactionsQuery.Where(x => x.Date <= endDate.Value);
    //     }
    //     
    //     response.Total = await transactionsQuery.CountAsync();
    //     
    //     var allTransactions = new List<FiatAssetTransactionResponse>();
    //     allTransactions.AddRange((await transactionsQuery
    //             .Include(x => x.WalletIdentifier)
    //                 .ThenInclude(y => y.Bank)
    //             .Include(x => x.WalletIdentifier)
    //                 .ThenInclude(y => y.Client)
    //             .Include(x => x.WalletIdentifier)
    //                 .ThenInclude(y => y.Member)
    //             .Include(x => x.WalletIdentifier)
    //                 .ThenInclude(y => y.PokerManager)
    //             
    //             .Include(x => x.AssetWallet)
    //                 .ThenInclude(y => y.Bank)
    //             .Include(x => x.AssetWallet)
    //                 .ThenInclude(y => y.Client)
    //             .Include(x => x.AssetWallet)
    //                 .ThenInclude(y => y.Member)
    //             .Include(x => x.AssetWallet)
    //                 .ThenInclude(y => y.PokerManager)
    //             .ToListAsync())
    //         .Select(_mapper.Map<FiatAssetTransactionResponse>));
    //     
    //     response.Data = [.. allTransactions.OrderBy(x => x.Date).Skip(page * quantity).Take(quantity)];
    //     
    //     return response;
        // }
        //
    // public async Task<TableResponse<FiatAssetTransactionResponse>> GetNonBankFiatAssetTransactions(Guid[]? bankAssetWalletIds, DateTime? startDate,
    //     DateTime? endDate, int quantity, int page)
    // {
    //     var response = new TableResponse<FiatAssetTransactionResponse>
    //     {
    //         Page = page,
    //         Show = quantity
    //     };
    //     
    //     var bankTransactionsQuery = _context.FiatAssetTransactions
    //         .Where(x => !x.DeletedAt.HasValue && (bankAssetWalletIds == null || !bankAssetWalletIds.Contains(x.AssetWalletId)));
    //     
    //     if (startDate.HasValue)
    //     {
    //         bankTransactionsQuery = bankTransactionsQuery.Where(x => x.Date >= startDate.Value);
    //     }
    //     
    //     if (endDate.HasValue)
    //     {
    //         bankTransactionsQuery = bankTransactionsQuery.Where(x => x.Date <= endDate.Value);
    //     }
    //     
    //     response.Total = await bankTransactionsQuery.CountAsync();
    //     
    //     var allTransactions = new List<FiatAssetTransactionResponse>();
    //     allTransactions.AddRange((await bankTransactionsQuery
    //                 .Include(x => x.WalletIdentifier)
    //                     .ThenInclude(y => y.Bank)
    //                 .Include(x => x.WalletIdentifier)
    //                     .ThenInclude(y => y.Client)
    //                 .Include(x => x.WalletIdentifier)
    //                     .ThenInclude(y => y.Member)
    //                 .Include(x => x.WalletIdentifier)
    //                     .ThenInclude(y => y.PokerManager)
    //                 
    //                 .Include(x => x.AssetWallet)
    //                     .ThenInclude(y => y.Bank)
    //                 .Include(x => x.AssetWallet)
    //                     .ThenInclude(y => y.Client)
    //                 .Include(x => x.AssetWallet)
    //                     .ThenInclude(y => y.Member)
    //                 .Include(x => x.AssetWallet)
    //                     .ThenInclude(y => y.PokerManager)
    //             .ToListAsync())
    //         .Select(_mapper.Map<FiatAssetTransactionResponse>));
    //     
    //     response.Data = allTransactions.OrderBy(x => x.Date).Skip(page * quantity).Take(quantity).ToList();
    //     
    //     return response;
        // }
        //
    // public async Task<TableResponse<DigitalAssetTransactionResponse>> GetPokerManagerDigitalAssetTransactions(Guid[] pokerManagerAssetWalletIds, DateTime? startDate,
    //     DateTime? endDate, int quantity, int page)
    // {
    //     var response = new TableResponse<DigitalAssetTransactionResponse>
    //     {
    //         Page = page,
    //         Show = quantity
    //     };
    //     
    //     var transactionsQuery = _context.DigitalAssetTransactions
    //         .Where(x => !x.DeletedAt.HasValue && pokerManagerAssetWalletIds.Contains(x.AssetWalletId));
    //     
    //     if (startDate.HasValue)
    //     {
    //         transactionsQuery = transactionsQuery.Where(x => x.Date >= startDate.Value);
    //     }
    //     
    //     if (endDate.HasValue)
    //     {
    //         transactionsQuery = transactionsQuery.Where(x => x.Date <= endDate.Value);
    //     }
    //     
    //     response.Total = await transactionsQuery.CountAsync();
    //     
    //     var allTransactions = new List<DigitalAssetTransactionResponse>();
    //     allTransactions.AddRange((await transactionsQuery
    //                 .Include(x => x.WalletIdentifier)
    //                     .ThenInclude(y => y.Bank)
    //                 .Include(x => x.WalletIdentifier)
    //                     .ThenInclude(y => y.Client)
    //                 .Include(x => x.WalletIdentifier)
    //                     .ThenInclude(y => y.Member)
    //                 .Include(x => x.WalletIdentifier)
    //                     .ThenInclude(y => y.PokerManager)
    //                 
    //                 .Include(x => x.AssetWallet)
    //                     .ThenInclude(y => y.Bank)
    //                 .Include(x => x.AssetWallet)
    //                     .ThenInclude(y => y.Client)
    //                 .Include(x => x.AssetWallet)
    //                     .ThenInclude(y => y.Member)
    //                 .Include(x => x.AssetWallet)
    //                     .ThenInclude(y => y.PokerManager)
    //             .ToListAsync())
    //         .Select(_mapper.Map<DigitalAssetTransactionResponse>));
    //     
    //     response.Data = [.. allTransactions.OrderBy(x => x.Date).Skip(page * quantity).Take(quantity)];
    //     
    //     return response;
    // }

    // public async Task<BaseAssetHolderWithTransactionsResponse> ConvertToAssetHolderWithTransactions(Guid id)
    // {
    //     var ah = await GetAssetHolderWithTransactions(id);
    //     return _mapper.Map<BaseAssetHolderWithTransactionsResponse>(ah);
    // }
    //
    // public async Task<BaseAssetHolder> GetAssetHolderWithTransactions(Guid id)
    // {
    //     var query = (IQueryable<BaseAssetHolder>)_entity.AsQueryable();
    //     query = query
    //         .Include(c => c.AssetWallets)
    //         .ThenInclude(aw => aw.DigitalAssetTransactions!)
    //         .ThenInclude(dat => dat.WalletIdentifier)
    //         .ThenInclude(wi => wi.Bank)
    //         .Include(c => c.AssetWallets)
    //         .ThenInclude(aw => aw.DigitalAssetTransactions)
    //         .ThenInclude(dat => dat.WalletIdentifier)
    //         .ThenInclude(wi => wi.Client)
    //         .Include(c => c.AssetWallets)
    //         .ThenInclude(aw => aw.DigitalAssetTransactions)
    //         .ThenInclude(dat => dat.WalletIdentifier)
    //         .ThenInclude(wi => wi.Member)
    //         .Include(c => c.AssetWallets)
    //         .ThenInclude(aw => aw.DigitalAssetTransactions)
    //         .ThenInclude(dat => dat.WalletIdentifier)
    //         .ThenInclude(wi => wi.PokerManager)
    //
    //         .Include(c => c.AssetWallets)
    //         .ThenInclude(aw => aw.FiatAssetTransactions)
    //         .ThenInclude(fat => fat.WalletIdentifier)
    //         .ThenInclude(wi => wi.Bank)
    //         .Include(c => c.AssetWallets)
    //         .ThenInclude(aw => aw.FiatAssetTransactions)
    //         .ThenInclude(fat => fat.WalletIdentifier)
    //         .ThenInclude(wi => wi.Client)
    //         .Include(c => c.AssetWallets)
    //         .ThenInclude(aw => aw.FiatAssetTransactions)
    //         .ThenInclude(fat => fat.WalletIdentifier)
    //         .ThenInclude(wi => wi.Member)
    //         .Include(c => c.AssetWallets)
    //         .ThenInclude(aw => aw.FiatAssetTransactions)
    //         .ThenInclude(fat => fat.WalletIdentifier)
    //         .ThenInclude(wi => wi.PokerManager)
    //
    //         .Include(c => c.WalletIdentifiers)
    //         .ThenInclude(aw => aw.DigitalAssetTransactions)
    //         .ThenInclude(dat => dat.AssetWallet)
    //         .ThenInclude(wi => wi.Bank)
    //         .Include(c => c.WalletIdentifiers)
    //         .ThenInclude(aw => aw.DigitalAssetTransactions)
    //         .ThenInclude(dat => dat.AssetWallet)
    //         .ThenInclude(wi => wi.Client)
    //         .Include(c => c.WalletIdentifiers)
    //         .ThenInclude(aw => aw.DigitalAssetTransactions)
    //         .ThenInclude(dat => dat.AssetWallet)
    //         .ThenInclude(wi => wi.Member)
    //         .Include(c => c.WalletIdentifiers)
    //         .ThenInclude(aw => aw.DigitalAssetTransactions)
    //         .ThenInclude(dat => dat.AssetWallet)
    //         .ThenInclude(wi => wi.PokerManager)
    //
    //         .Include(c => c.WalletIdentifiers)
    //         .ThenInclude(aw => aw.FiatAssetTransactions)
    //         .ThenInclude(fat => fat.AssetWallet)
    //         .ThenInclude(wi => wi.Bank)
    //         .Include(c => c.WalletIdentifiers)
    //         .ThenInclude(aw => aw.FiatAssetTransactions)
    //         .ThenInclude(fat => fat.AssetWallet)
    //         .ThenInclude(wi => wi.Client)
    //         .Include(c => c.WalletIdentifiers)
    //         .ThenInclude(aw => aw.FiatAssetTransactions)
    //         .ThenInclude(fat => fat.AssetWallet)
    //         .ThenInclude(wi => wi.Member)
    //         .Include(c => c.WalletIdentifiers)
    //         .ThenInclude(aw => aw.FiatAssetTransactions)!
    //         .ThenInclude(fat => fat.AssetWallet)
    //         .ThenInclude(wi => wi.PokerManager);
    //         
    //
    //     TEntity obj;
    //     if (typeof(TEntity) == typeof(Client))
    //     {
    //         query = query.Cast<Client>();
    //     }
    //     else if (typeof(TEntity) == typeof(Bank))
    //     {
    //         query = query.Cast<Bank>();
    //     }
    //     else if (typeof(TEntity) == typeof(Member))
    //     {
    //         query = query.Cast<Member>();
    //     }
    //     else if (typeof(TEntity) == typeof(PokerManager))
    //     {
    //         query = query.Cast<PokerManager>();
    //     }
    //     else
    //     {
    //         throw new KeyNotFoundException($"Entity type {typeof(TEntity).Name} is not supported");
    //     }
    //     
    //
    //     var assetHolder = await query.FirstOrDefaultAsync(x => 
    //                               x.Id == id) ?? throw new Exception("AssetHolder not found");
    //
    //     return assetHolder;
    // }
    //
    // public async Task<TableResponse<TransactionResponse>> GetTransactions(Guid clientId, DateTime? startDate,
    //     DateTime? endDate, int quantity, int page)
    // {
    //     // var response = new TableResponse<TransactionResponse>
    //     // {
    //     //     Page = page,
    //     //     Show = quantity
    //     // };
    //     //
    //     // var bankTransactionsQuery =
    //     //     _context.BankTransactions.Where(x => !x.DeletedAt.HasValue && x.ClientId == clientId);
    //     // var walletTransactionsQuery =
    //     //     _context.WalletTransactions.Where(x => !x.DeletedAt.HasValue && x.ClientId == clientId);
    //     // var internalTransactionsQuery =
    //     //     _context.InternalTransactions.Where(x => !x.DeletedAt.HasValue && x.ClientId == clientId);
    //     //
    //     //
    //     // if (startDate.HasValue)
    //     // {
    //     //     bankTransactionsQuery = bankTransactionsQuery.Where(x => x.Date >= startDate.Value);
    //     //     walletTransactionsQuery = walletTransactionsQuery.Where(x => x.Date >= startDate.Value);
    //     //     internalTransactionsQuery = internalTransactionsQuery.Where(x => x.Date >= startDate.Value);
    //     // }
    //     //
    //     // if (endDate.HasValue)
    //     // {
    //     //     bankTransactionsQuery = bankTransactionsQuery.Where(x => x.Date <= endDate.Value);
    //     //     walletTransactionsQuery = walletTransactionsQuery.Where(x => x.Date <= endDate.Value);
    //     //     internalTransactionsQuery = internalTransactionsQuery.Where(x => x.Date <= endDate.Value);
    //     // }
    //     //
    //     // response.Total = await bankTransactionsQuery.CountAsync() + await internalTransactionsQuery.CountAsync() +
    //     //                  await walletTransactionsQuery.CountAsync() + await internalTransactionsQuery.CountAsync();
    //     //
    //     // var allTransactions = new List<TransactionResponse>();
    //     // allTransactions.AddRange((await bankTransactionsQuery.ToListAsync()).Select(x => new TransactionResponse(x)));
    //     // allTransactions.AddRange((await walletTransactionsQuery.ToListAsync()).Select(x => new TransactionResponse(x)));
    //     // allTransactions.AddRange(
    //     //     (await internalTransactionsQuery.ToListAsync()).Select(x => new TransactionResponse(x)));
    //     //
    //     // response.Data = allTransactions.OrderBy(x => x.Date).Skip(page * quantity).Take(quantity).ToList();
    //     //
    //     // return response;
    //     await Task.Yield();
    //     return null;
        // }
        //
    // public async Task<TableResponse<TransactionResponse>> GetTagTransactions(Guid tagId, DateTime? startDate,
    //     DateTime? endDate, int quantity, int page)
    // {
    //     // var response = new TableResponse<TransactionResponse>
    //     // {
    //     //     Page = page,
    //     //     Show = quantity
    //     // };
    //     //
    //     // var walletTransactionsQuery = _context.WalletTransactions.Where(x => !x.DeletedAt.HasValue && x.TagId == tagId);
    //     // var internalTransactionsQuery =
    //     //     _context.InternalTransactions.Where(x => !x.DeletedAt.HasValue && x.TagId == tagId);
    //     // var bankTransactionsQuery = _context.BankTransactions.Where(x => !x.DeletedAt.HasValue && x.TagId == tagId);
    //     //
    //     //
    //     // if (startDate.HasValue)
    //     // {
    //     //     bankTransactionsQuery = bankTransactionsQuery.Where(x => x.Date >= startDate.Value);
    //     //     walletTransactionsQuery = walletTransactionsQuery.Where(x => x.Date >= startDate.Value);
    //     //     internalTransactionsQuery = internalTransactionsQuery.Where(x => x.Date >= startDate.Value);
    //     // }
    //     //
    //     // if (endDate.HasValue)
    //     // {
    //     //     bankTransactionsQuery = bankTransactionsQuery.Where(x => x.Date <= endDate.Value);
    //     //     walletTransactionsQuery = walletTransactionsQuery.Where(x => x.Date <= endDate.Value);
    //     //     internalTransactionsQuery = internalTransactionsQuery.Where(x => x.Date <= endDate.Value);
    //     // }
    //     //
    //     // response.Total = await bankTransactionsQuery.CountAsync() + await internalTransactionsQuery.CountAsync() +
    //     //                  await walletTransactionsQuery.CountAsync() + await internalTransactionsQuery.CountAsync();
    //     //
    //     // var allTransactions = new List<TransactionResponse>();
    //     // allTransactions.AddRange((await bankTransactionsQuery.ToListAsync()).Select(x => new TransactionResponse(x)));
    //     // allTransactions.AddRange((await walletTransactionsQuery.ToListAsync()).Select(x => new TransactionResponse(x)));
    //     // allTransactions.AddRange(
    //     //     (await internalTransactionsQuery.ToListAsync()).Select(x => new TransactionResponse(x)));
    //     //
    //     // response.Data = allTransactions.OrderBy(x => x.Date).Skip(page * quantity).Take(quantity).ToList();
    //     //
    //     // return response;
    //     await Task.Yield();
    //     return null;
        // }
        //
    // public async Task<TableResponse<TransactionResponse>> GetClosingManagerTransactions(
    //     Guid closingManagerTransactionId, DateTime? startDate, DateTime? endDate, int quantity, int page)
    // {
        // var response = new TableResponse<TransactionResponse>
        // {
        //     Page = page,
        //     Show = quantity
        // };
        //
        // var closingManager =
        //     await _context.ClosingManagers.FirstOrDefaultAsync(x => x.Id == closingManagerTransactionId);
        //
        // var internalTransactionsQuery =
        //     _context.InternalTransactions.Where(x => !x.DeletedAt.HasValue && x.ManagerId == closingManager.ManagerId);
        //
        // if (startDate.HasValue)
        //     internalTransactionsQuery = internalTransactionsQuery.Where(x => x.Date >= startDate.Value);
        //
        // if (endDate.HasValue) internalTransactionsQuery = internalTransactionsQuery.Where(x => x.Date <= endDate.Value);
        //
        // response.Total = await internalTransactionsQuery.CountAsync();
        //
        // var allTransactions = new List<TransactionResponse>();
        // allTransactions.AddRange(
        //     (await internalTransactionsQuery.ToListAsync()).Select(x => new TransactionResponse(x)));
        //
        // response.Data = allTransactions.OrderBy(x => x.Date).Skip(page * quantity).Take(quantity).ToList();
        //
        // return response;
    //     await Task.Yield();
    //     return null;
    // }
}