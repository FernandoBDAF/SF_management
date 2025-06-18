using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.ViewModels;

namespace SFManagement.Services;

public class TransactionService
{
    private readonly DataContext _context;

    public TransactionService(DataContext context)
    {
        _context = context;
    }

    public async Task<TableResponse<TransactionResponse>> GetWalletTransactions(Guid walletId, DateTime? startDate,
        DateTime? endDate, int quantity, int page)
    {
        // var response = new TableResponse<TransactionResponse>
        // {
        //     Page = page,
        //     Show = quantity
        // };
        //
        // var walletTransactionsQuery =
        //     _context.WalletTransactions.Where(x => !x.DeletedAt.HasValue && x.WalletId == walletId);
        // var internalTransactionsQuery =
        //     _context.InternalTransactions.Where(x => !x.DeletedAt.HasValue && x.WalletId == walletId);
        //
        // if (startDate.HasValue)
        // {
        //     walletTransactionsQuery = walletTransactionsQuery.Where(x => x.Date >= startDate.Value);
        //     internalTransactionsQuery = internalTransactionsQuery.Where(x => x.Date >= startDate.Value);
        // }
        //
        // if (endDate.HasValue)
        // {
        //     walletTransactionsQuery = walletTransactionsQuery.Where(x => x.Date <= endDate.Value);
        //     internalTransactionsQuery = internalTransactionsQuery.Where(x => x.Date <= endDate.Value);
        // }
        //
        // response.Total = await walletTransactionsQuery.CountAsync() + await internalTransactionsQuery.CountAsync();
        //
        // var allTransactions = new List<TransactionResponse>();
        // allTransactions.AddRange((await walletTransactionsQuery.ToListAsync()).Select(x => new TransactionResponse(x)));
        // allTransactions.AddRange(
        //     (await internalTransactionsQuery.ToListAsync()).Select(x => new TransactionResponse(x)));
        //
        // response.Data = allTransactions.OrderBy(x => x.Date).Skip(page * quantity).Take(quantity).ToList();
        //
        // return response;
        await Task.Yield();
        return null;
    }

    public async Task<TableResponse<TransactionResponse>> GetBankTransactions(Guid bankId, DateTime? startDate,
        DateTime? endDate, int quantity, int page)
    {
        // var response = new TableResponse<TransactionResponse>
        // {
        //     Page = page,
        //     Show = quantity
        // };
        //
        // var bankTransactionsQuery = _context.BankTransactions.Where(x => !x.DeletedAt.HasValue && x.BankId == bankId);
        // var internalTransactionsQuery =
        //     _context.InternalTransactions.Where(x => !x.DeletedAt.HasValue && x.BankId == bankId);
        //
        // if (startDate.HasValue)
        // {
        //     bankTransactionsQuery = bankTransactionsQuery.Where(x => x.Date >= startDate.Value);
        //     internalTransactionsQuery = internalTransactionsQuery.Where(x => x.Date >= startDate.Value);
        // }
        //
        // if (endDate.HasValue)
        // {
        //     bankTransactionsQuery = bankTransactionsQuery.Where(x => x.Date <= endDate.Value);
        //     internalTransactionsQuery = internalTransactionsQuery.Where(x => x.Date <= endDate.Value);
        // }
        //
        // response.Total = await bankTransactionsQuery.CountAsync() + await internalTransactionsQuery.CountAsync();
        //
        // var allTransactions = new List<TransactionResponse>();
        // allTransactions.AddRange((await bankTransactionsQuery.ToListAsync()).Select(x => new TransactionResponse(x)));
        // allTransactions.AddRange(
        //     (await internalTransactionsQuery.ToListAsync()).Select(x => new TransactionResponse(x)));
        //
        // response.Data = allTransactions.OrderBy(x => x.Date).Skip(page * quantity).Take(quantity).ToList();
        //
        // return response;
        await Task.Yield();
        return null;
    }

    public async Task<TableResponse<TransactionResponse>> GetManagerTransactions(Guid managerId, DateTime? startDate,
        DateTime? endDate, int quantity, int page)
    {
        // var response = new TableResponse<TransactionResponse>
        // {
        //     Page = page,
        //     Show = quantity
        // };
        //
        // var walletTransactionsQuery = _context.WalletTransactions.Where(x =>
        //     !x.DeletedAt.HasValue && x.AssetWallet != null && x.AssetWallet.ManagerId == managerId);
        // var internalTransactionsQuery =
        //     _context.InternalTransactions.Where(x => !x.DeletedAt.HasValue && x.ManagerId == managerId);
        //
        // if (startDate.HasValue)
        // {
        //     walletTransactionsQuery = walletTransactionsQuery.Where(x => x.Date >= startDate.Value);
        //     internalTransactionsQuery = internalTransactionsQuery.Where(x => x.Date >= startDate.Value);
        // }
        //
        // if (endDate.HasValue)
        // {
        //     walletTransactionsQuery = walletTransactionsQuery.Where(x => x.Date <= endDate.Value);
        //     internalTransactionsQuery = internalTransactionsQuery.Where(x => x.Date <= endDate.Value);
        // }
        //
        // response.Total = await walletTransactionsQuery.CountAsync() + await internalTransactionsQuery.CountAsync();
        //
        // var allTransactions = new List<TransactionResponse>();
        // allTransactions.AddRange((await walletTransactionsQuery.ToListAsync()).Select(x => new TransactionResponse(x)));
        // allTransactions.AddRange(
        //     (await internalTransactionsQuery.ToListAsync()).Select(x => new TransactionResponse(x)));
        //
        // response.Data = allTransactions.OrderBy(x => x.Date).Skip(page * quantity).Take(quantity).ToList();
        //
        // return response;
        await Task.Yield();
        return null;
    }

    public async Task<TableResponse<TransactionResponse>> GetTransactions(Guid clientId, DateTime? startDate,
        DateTime? endDate, int quantity, int page)
    {
        // var response = new TableResponse<TransactionResponse>
        // {
        //     Page = page,
        //     Show = quantity
        // };
        //
        // var bankTransactionsQuery =
        //     _context.BankTransactions.Where(x => !x.DeletedAt.HasValue && x.ClientId == clientId);
        // var walletTransactionsQuery =
        //     _context.WalletTransactions.Where(x => !x.DeletedAt.HasValue && x.ClientId == clientId);
        // var internalTransactionsQuery =
        //     _context.InternalTransactions.Where(x => !x.DeletedAt.HasValue && x.ClientId == clientId);
        //
        //
        // if (startDate.HasValue)
        // {
        //     bankTransactionsQuery = bankTransactionsQuery.Where(x => x.Date >= startDate.Value);
        //     walletTransactionsQuery = walletTransactionsQuery.Where(x => x.Date >= startDate.Value);
        //     internalTransactionsQuery = internalTransactionsQuery.Where(x => x.Date >= startDate.Value);
        // }
        //
        // if (endDate.HasValue)
        // {
        //     bankTransactionsQuery = bankTransactionsQuery.Where(x => x.Date <= endDate.Value);
        //     walletTransactionsQuery = walletTransactionsQuery.Where(x => x.Date <= endDate.Value);
        //     internalTransactionsQuery = internalTransactionsQuery.Where(x => x.Date <= endDate.Value);
        // }
        //
        // response.Total = await bankTransactionsQuery.CountAsync() + await internalTransactionsQuery.CountAsync() +
        //                  await walletTransactionsQuery.CountAsync() + await internalTransactionsQuery.CountAsync();
        //
        // var allTransactions = new List<TransactionResponse>();
        // allTransactions.AddRange((await bankTransactionsQuery.ToListAsync()).Select(x => new TransactionResponse(x)));
        // allTransactions.AddRange((await walletTransactionsQuery.ToListAsync()).Select(x => new TransactionResponse(x)));
        // allTransactions.AddRange(
        //     (await internalTransactionsQuery.ToListAsync()).Select(x => new TransactionResponse(x)));
        //
        // response.Data = allTransactions.OrderBy(x => x.Date).Skip(page * quantity).Take(quantity).ToList();
        //
        // return response;
        await Task.Yield();
        return null;
    }

    public async Task<TableResponse<TransactionResponse>> GetTagTransactions(Guid tagId, DateTime? startDate,
        DateTime? endDate, int quantity, int page)
    {
        // var response = new TableResponse<TransactionResponse>
        // {
        //     Page = page,
        //     Show = quantity
        // };
        //
        // var walletTransactionsQuery = _context.WalletTransactions.Where(x => !x.DeletedAt.HasValue && x.TagId == tagId);
        // var internalTransactionsQuery =
        //     _context.InternalTransactions.Where(x => !x.DeletedAt.HasValue && x.TagId == tagId);
        // var bankTransactionsQuery = _context.BankTransactions.Where(x => !x.DeletedAt.HasValue && x.TagId == tagId);
        //
        //
        // if (startDate.HasValue)
        // {
        //     bankTransactionsQuery = bankTransactionsQuery.Where(x => x.Date >= startDate.Value);
        //     walletTransactionsQuery = walletTransactionsQuery.Where(x => x.Date >= startDate.Value);
        //     internalTransactionsQuery = internalTransactionsQuery.Where(x => x.Date >= startDate.Value);
        // }
        //
        // if (endDate.HasValue)
        // {
        //     bankTransactionsQuery = bankTransactionsQuery.Where(x => x.Date <= endDate.Value);
        //     walletTransactionsQuery = walletTransactionsQuery.Where(x => x.Date <= endDate.Value);
        //     internalTransactionsQuery = internalTransactionsQuery.Where(x => x.Date <= endDate.Value);
        // }
        //
        // response.Total = await bankTransactionsQuery.CountAsync() + await internalTransactionsQuery.CountAsync() +
        //                  await walletTransactionsQuery.CountAsync() + await internalTransactionsQuery.CountAsync();
        //
        // var allTransactions = new List<TransactionResponse>();
        // allTransactions.AddRange((await bankTransactionsQuery.ToListAsync()).Select(x => new TransactionResponse(x)));
        // allTransactions.AddRange((await walletTransactionsQuery.ToListAsync()).Select(x => new TransactionResponse(x)));
        // allTransactions.AddRange(
        //     (await internalTransactionsQuery.ToListAsync()).Select(x => new TransactionResponse(x)));
        //
        // response.Data = allTransactions.OrderBy(x => x.Date).Skip(page * quantity).Take(quantity).ToList();
        //
        // return response;
        await Task.Yield();
        return null;
    }

    public async Task<TableResponse<TransactionResponse>> GetClosingManagerTransactions(
        Guid closingManagerTransactionId, DateTime? startDate, DateTime? endDate, int quantity, int page)
    {
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
        await Task.Yield();
        return null;
    }
}