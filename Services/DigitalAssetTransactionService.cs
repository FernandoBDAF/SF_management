using SFManagement.Data;
using SFManagement.Models.Transactions;

namespace SFManagement.Services;

public class DigitalAssetTransactionService : BaseTransactionService<DigitalAssetTransaction>
{
    public DigitalAssetTransactionService(DataContext context, IHttpContextAccessor httpContextAccessor) :
        base(context, httpContextAccessor)
    {
    }

    // NOTE: Most methods in this service are commented out. When implementing them,
    // ensure they use the new transaction model with SenderWalletIdentifierId and ReceiverWalletIdentifierId
    // instead of the old AssetWalletId and WalletIdentifierId properties.

    // public override async Task<DigitalAssetTransaction> Add(DigitalAssetTransaction obj)
    // {
    //     // When implementing: ensure obj.SenderWalletIdentifierId and obj.ReceiverWalletIdentifierId are set
    //     obj = await base.Add(obj);
    //     return obj;
    // }

    // public async Task<DigitalAssetTransactionResponse> ApproveTransaction(Guid walletTransactionId,
    //     WalletTransactionApproveRequest model)
    // {
    //     // var walletTransaction = await base.Get(walletTransactionId);
    //     // if (walletTransaction == null) throw new AppException("AssetWallet transaction not found");
    //     //
    //     // walletTransaction.ApprovedAt = DateTime.Now;
    //     // walletTransaction.NicknameId = model.NicknameId;
    //     // walletTransaction.TagId = model.TagId;
    //     // walletTransaction.ClientId = model.ClientId;
    //     // walletTransaction.WalletId = model.WalletId;
    //     // walletTransaction.ExchangeRate = model.ExchangeRate;
    //     // walletTransaction.Value = model.Value;
    //     //
    //     // if (_user != null)
    //     //     walletTransaction.ApprovedBy = Guid.Parse(_user.Claims.FirstOrDefault(c => c.Type == "uid").Value);
    //     //
    //     // context.WalletTransactions.Update(walletTransaction);
    //     // await context.SaveChangesAsync();
    //     //
    //     // return _mapper.Map<DigitalAssetTransactionResponse>(walletTransaction);
    //     await Task.Yield();
    //     return null;
    // }
    //
    // public async Task<DigitalAssetTransaction> UnApproveTransaction(Guid walletTransactionId)
    // {
    //     // var walletTransaction = _entity.FirstOrDefault(x => x.Id == walletTransactionId);
    //     //
    //     // if (walletTransaction == null) throw new AppException("Não foi encontrado nenhuma transação.");
    //     //
    //     // if (!walletTransaction.ApprovedAt.HasValue) throw new AppException("Transação não está aprovada.");
    //     //
    //     // walletTransaction.ApprovedAt = null;
    //     // walletTransaction.ApprovedBy = null;
    //     // walletTransaction.TagId = null;
    //     //
    //     // if (walletTransaction.ExcelId.HasValue)
    //     // {
    //     //     walletTransaction.ClientId = null;
    //     //     walletTransaction.WalletId = null;
    //     //     walletTransaction.ExchangeRate = 0;
    //     //     walletTransaction.Value = 0;
    //     //     walletTransaction.TagId = null;
    //     //     var to = _entity.FirstOrDefault(x => x.LinkedToId == walletTransactionId);
    //     //
    //     //     if (to != null)
    //     //     {
    //     //         to.ApprovedAt = null;
    //     //         to.ApprovedBy = null;
    //     //         to.LinkedToId = null;
    //     //
    //     //         context.WalletTransactions.Update(to);
    //     //     }
    //     // }
    //     // else if (walletTransaction.LinkedToId.HasValue)
    //     // {
    //     //     var to = _entity.FirstOrDefault(x => x.Id == walletTransaction.LinkedToId);
    //     //
    //     //     if (to == null)
    //     //         throw new AppException("Não foi encontrado nenhuma transação que tem link com essa transação.");
    //     //
    //     //     to.ApprovedAt = null;
    //     //     to.ApprovedBy = null;
    //     //     to.ClientId = null;
    //     //     to.WalletId = null;
    //     //     to.ExchangeRate = 0;
    //     //     to.Value = 0;
    //     //     to.TagId = null;
    //     //
    //     //     walletTransaction.LinkedToId = null;
    //     //
    //     //     context.WalletTransactions.Update(to);
    //     // }
    //     //
    //     // context.WalletTransactions.Update(walletTransaction);
    //     //
    //     // await context.SaveChangesAsync();
    //     //
    //     // return walletTransaction;
    //     await Task.Yield();
    //     return null;
    // }
    //
    // public async Task<(DigitalAssetTransaction from, DigitalAssetTransaction to)> Link(Guid fromWalletTransactionId,
    //     Guid toWalletTransactionId)
    // {
    //     // var fromWalletTransaction = _entity.FirstOrDefault(x => x.Id == fromWalletTransactionId);
    //     //
    //     // if (fromWalletTransaction == null) throw new AppException("Não foi encontrado nenhuma transação de destino.");
    //     //
    //     // if (!fromWalletTransaction.ExcelId.HasValue)
    //     //     throw new AppException(
    //     //         "Não é uma transação de destino válida (Não é uma transação oriunda de arquivo EXCEL.)");
    //     //
    //     // if (context.WalletTransactions.Any(x => x.LinkedToId == fromWalletTransaction.Id))
    //     //     throw new AppException("Esta transação EXCEL já foi vinculada a uma transação manual.");
    //     //
    //     // var toWalletTransaction = _entity.FirstOrDefault(x => x.Id == toWalletTransactionId);
    //     //
    //     // if (toWalletTransaction == null) throw new AppException("Não foi encontrado nenhuma transação de início.");
    //     //
    //     // if (toWalletTransaction.ExcelId.HasValue)
    //     //     throw new AppException("Não é uma transação de início válida (Não é uma transação manual.)");
    //     //
    //     // if (toWalletTransaction.LinkedToId.HasValue)
    //     //     throw new AppException("Esta transação manual já foi vinculada a uma transação EXCEL.");
    //     //
    //     // toWalletTransaction.LinkedToId = fromWalletTransaction.Id;
    //     //
    //     // if (_user != null)
    //     //     toWalletTransaction.ApprovedBy = Guid.Parse(_user.Claims.FirstOrDefault(c => c.Type == "uid").Value);
    //     //
    //     // toWalletTransaction.ApprovedAt = DateTime.Now;
    //     //
    //     // context.WalletTransactions.Update(toWalletTransaction);
    //     //
    //     // fromWalletTransaction.ApprovedAt = DateTime.Now;
    //     //
    //     // if (_user != null)
    //     //     fromWalletTransaction.ApprovedBy = Guid.Parse(_user.Claims.FirstOrDefault(c => c.Type == "uid").Value);
    //     //
    //     // context.WalletTransactions.Update(fromWalletTransaction);
    //     //
    //     // await context.SaveChangesAsync();
    //     //
    //     // return (fromWalletTransaction, toWalletTransaction);
    //     await Task.Yield();
    //     return (null, null);
    // }
    //
    // public async Task CalcAvgRate(PokerManager manager, DateTime date)
    // {
    //     // var queryWalletTransactions = context.WalletTransactions.AsNoTracking()
    //     //     .Include(x => x.AssetWallet)
    //     //     .Where(x => x.AssetWallet.ManagerId == manager.Id && !x.DeletedAt.HasValue &&
    //     //                 ((!x.ApprovedAt.HasValue && !x.ExcelId.HasValue) ||
    //     //                  (x.ApprovedAt.HasValue && (x.LinkedToId.HasValue || x.ClientId.HasValue ||
    //     //                                             x.TagId.HasValue ||
    //     //                                             (x.ManagerId.HasValue && x.WalletId.HasValue)))));
    //     //
    //     // // var len = queryWalletTransactions.Count();
    //     //
    //     // // var expenses = queryWalletTransactions
    //     // //     .Where(x => x.WalletTransactionType == WalletTransactionType.Expense);
    //     // // var expensesTotal = expenses.Count();
    //     //
    //     // var queryWalletTransactionsCurrentDate = queryWalletTransactions
    //     //     .Where(x => x.Date.Date == date.Date);
    //     //
    //     // // var len2 = queryWalletTransactionsCurrentDate.Count();
    //     //
    //     // var lastAvg = await context.AvgRates.AsNoTracking().OrderByDescending(x => x.Date)
    //     //     .Where(x => x.Date.Date < date.Date).FirstOrDefaultAsync();
    //     // lastAvg ??= new AvgRate();
    //     //
    //     // var currentAvg = await context.AvgRates.FirstOrDefaultAsync(x =>
    //     //     x.Date.Date == date.Date && !x.DeletedAt.HasValue && x.ManagerId == manager.Id);
    //     // currentAvg ??= new AvgRate { Date = date.Date, ManagerId = manager.Id };
    //     //
    //     // var currentBalanceCoins = await queryWalletTransactions
    //     //     .Where(x => x.WalletTransactionType == WalletTransactionType.Expense && x.Date.Date == date.Date
    //     //         && x.IsCoinBalance != true && (x.ClientId.HasValue || (!x.TagId.HasValue && !x.NicknameId.HasValue)))
    //     //     .SumAsync(x => x.Coins);
    //     // var currentBalanceTotal = await queryWalletTransactions
    //     //     .Where(x => x.WalletTransactionType == WalletTransactionType.Expense && x.Date.Date == date.Date
    //     //         && x.IsCoinBalance != true && (x.ClientId.HasValue || (!x.TagId.HasValue && !x.NicknameId.HasValue)))
    //     //     .SumAsync(x => x.Coins * x.ExchangeRate);
    //     //
    //     // var balanceCoins =
    //     //     (await queryWalletTransactions.Where(x => x.Date.Date < date.Date).ToListAsync()).Sum(x =>
    //     //         x.WalletTransactionType == WalletTransactionType.Income ? decimal.Negate(x.Coins) : x.Coins) +
    //     //     manager.InitialCoins;
    //     //
    //     // if (lastAvg.Id == Guid.Empty)
    //     // {
    //     //     if (manager.InitialCoins + currentBalanceCoins > decimal.Zero)
    //     //         currentAvg.Value = (manager.InitialCoins * manager.InitialExchangeRate + currentBalanceTotal) /
    //     //                            (manager.InitialCoins + currentBalanceCoins);
    //     // }
    //     // else
    //     // {
    //     //     if (balanceCoins + currentBalanceCoins > decimal.Zero)
    //     //         currentAvg.Value = (balanceCoins * lastAvg.Value + currentBalanceTotal) /
    //     //                            (balanceCoins + currentBalanceCoins);
    //     // }
    //     //
    //     // if (currentAvg.Id == Guid.Empty)
    //     //     await context.AvgRates.AddAsync(currentAvg);
    //     // else
    //     //     context.AvgRates.Update(currentAvg);
    //     //
    //     // await context.SaveChangesAsync();
    //     //
    //     // // if there isn't any new transaction in this day, it should just copy the previous day
    //     // var nextWalletTransaction =
    //     //     queryWalletTransactions.Where(x => x.Date > date).OrderBy(x => x.Date).FirstOrDefault();
    //     //
    //     // if (nextWalletTransaction != null)
    //     //     await CalcAvgRate(manager, nextWalletTransaction.Date);
    //     // else
    //     //     await ClearAvgRate(manager, date);
    //     await Task.Yield();
    // }
    //
    // public async Task ClearAvgRate(PokerManager manager, DateTime date)
    // {
    //     var avgRatesToRemove = await context.AvgRates
    //         .Where(x => x.Date.Date > date.Date && x.PokerManagerId == manager.Id)
    //         .ToListAsync();
    //
    //     if (avgRatesToRemove.Count != 0)
    //     {
    //         context.AvgRates.RemoveRange(avgRatesToRemove);
    //         await context.SaveChangesAsync();
    //     }
    // }
    //
    // public async Task SetExchangeRate(Guid managerId)
    // {
    //     // var walletTransactions = await context.WalletTransactions.Where(x =>
    //     //         !x.DeletedAt.HasValue && x.ManagerId == managerId &&
    //     //         ((!x.ClientId.HasValue && x.TagId.HasValue) || x.IsCoinBalance == true))
    //     //     .ToListAsync();
    //     //
    //     // foreach (var group in walletTransactions.GroupBy(x => x.Date.Date))
    //     // {
    //     //     var avgRate = await context.AvgRates.OrderByDescending(x => x.Date)
    //     //         .FirstOrDefaultAsync(x => x.Date.Date <= group.Key.Date && x.ManagerId == managerId);
    //     //
    //     //     foreach (var walletTransaction in group)
    //     //     {
    //     //         walletTransaction.ExchangeRate = avgRate.Value;
    //     //         walletTransaction.Value = walletTransaction.Coins * walletTransaction.ExchangeRate;
    //     //
    //     //         context.WalletTransactions.Update(walletTransaction);
    //     //     }
    //     // }
    //     //
    //     // await context.SaveChangesAsync();
    //     await Task.Yield();
    // }
    //
    // public async Task CalcProfits(Guid managerId)
    // {
    //     // foreach (var walletTransaction in await context.WalletTransactions.Include(x => x.AssetWallet).Where(x =>
    //     //              !x.DeletedAt.HasValue && ((!x.ApprovedAt.HasValue && !x.ExcelId.HasValue) ||
    //     //                                        (x.ApprovedAt.HasValue &&
    //     //                                         (x.LinkedToId.HasValue || x.ClientId.HasValue ||
    //     //                                          x.TagId.HasValue ||
    //     //                                          (x.ManagerId.HasValue && x.WalletId.HasValue)))) &&
    //     //              (x.WalletTransactionType == WalletTransactionType.Income || x.IsCoinBalance == true) &&
    //     //              x.AssetWallet.ManagerId == managerId).ToListAsync())
    //     // {
    //     //     var avgRate = await context.AvgRates.OrderByDescending(x => x.Date).FirstOrDefaultAsync(x =>
    //     //         x.Date.Date <= walletTransaction.Date.Date && x.ManagerId == walletTransaction.AssetWallet.ManagerId);
    //     //
    //     //     if (walletTransaction.IsCoinBalance != true)
    //     //     {
    //     //         walletTransaction.Profit = (walletTransaction.ExchangeRate - avgRate.Value) * walletTransaction.Coins;
    //     //     }
    //     //     else
    //     //     {
    //     //         var r = walletTransaction.Rate ?? 0;
    //     //         walletTransaction.Profit = walletTransaction.Coins * (r / (100 + r)) * walletTransaction.ExchangeRate;
    //     //     }
    //     //
    //     //     await context.SaveChangesAsync();
    //     // }
    //     await Task.Yield();
    // }
}