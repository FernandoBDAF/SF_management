using SFManagement.Enums;
using SFManagement.Models;
using SFManagement.Models.Entities;
using SFManagement.Models.Transactions;

namespace SFManagement.ViewModels;

public class BalanceResponse
{
    // public BalanceResponse(IEnumerable<FiatAssetTransaction> bankTransactions,
    //     // IEnumerable<InternalTransaction> internalTransaction, decimal initialValue, DateTime? date)
    //     decimal initialValue, DateTime? date)
    // {
    //     // Value = initialValue;
    //     //
    //     // Value += bankTransactions
    //     //     .Where(x => x.Date < date && !x.DeletedAt.HasValue && ((!x.ApprovedAt.HasValue && !x.OfxId.HasValue) ||
    //     //                                                            (x.ApprovedAt.HasValue && x.LinkedToId.HasValue) ||
    //     //                                                            (x.ApprovedAt.HasValue && (x.ClientId.HasValue ||
    //     //                                                                x.TagId.HasValue || x.ManagerId.HasValue))))
    //     //     .Sum(x => x.BankTransactionType == BankTransactionType.Income ? x.Value : decimal.Negate(x.Value));
    //     //
    //     // Value += internalTransaction.Where(x => x.Date < date && !x.DeletedAt.HasValue).Sum(x =>
    //     //     x.InternalTransactionType == InternalTransactionType.Income ? x.Value : decimal.Negate(x.Value));
    // }
    //
    // public BalanceResponse(Client client, DateTime? date)
    // {
    //     // Value = client.InitialValue;
    //     //
    //     // Value += client.BankTransactions
    //     //     .Where(x => x.Date < date && !x.DeletedAt.HasValue && !x.TagId.HasValue &&
    //     //                 (!x.ApprovedAt.HasValue || (x.ApprovedAt.HasValue && x.LinkedToId.HasValue))).Sum(x =>
    //     //         x.BankTransactionType == BankTransactionType.Income ? x.Value : decimal.Negate(x.Value));
    //     //
    //     // Value += client.WalletTransactions
    //     //     .Where(x => x.Date < date && !x.DeletedAt.HasValue && !x.TagId.HasValue &&
    //     //                 (!x.ApprovedAt.HasValue || (x.ApprovedAt.HasValue && x.LinkedToId.HasValue))).Sum(x =>
    //     //         x.WalletTransactionType == WalletTransactionType.Expense ? x.Value : decimal.Negate(x.Value));
    //     //
    //     // Value += client.InternalTransactions.Where(x => x.Date < date && !x.DeletedAt.HasValue).Sum(x =>
    //     //     !x.Coins.HasValue && x.InternalTransactionType == InternalTransactionType.Income
    //     //         ? x.Value
    //     //         : decimal.Negate(x.Value));
    //     //
    //     // Coins = client.WalletTransactions
    //     //     .Where(x => x.Date < date && !x.DeletedAt.HasValue && x.IsCoinBalance == true &&
    //     //                 (!x.ApprovedAt.HasValue || (x.ApprovedAt.HasValue && x.LinkedToId.HasValue))).Sum(x =>
    //     //         x.WalletTransactionType == WalletTransactionType.Expense
    //     //             ? x.Coins / (1 + (x.Rate / 100 ?? 0))
    //     //             : decimal.Negate(x.Coins / (1 + (x.Rate / 100 ?? 0))));
    // }
    //
    // public BalanceResponse(FinancialBehavior financialBehavior)
    // {
    //     // Value = financialBehavior.BankTransactions
    //     //     .Where(x => !x.DeletedAt.HasValue && x.TagId.HasValue &&
    //     //                 (!x.ApprovedAt.HasValue || (x.ApprovedAt.HasValue && x.LinkedToId.HasValue))).Sum(x =>
    //     //         x.BankTransactionType == BankTransactionType.Income ? x.Value : decimal.Negate(x.Value));
    //     //
    //     // Value += financialBehavior.WalletTransactions
    //     //     .Where(x => !x.DeletedAt.HasValue && x.TagId.HasValue &&
    //     //                 (!x.ApprovedAt.HasValue || (x.ApprovedAt.HasValue && x.LinkedToId.HasValue))).Sum(x =>
    //     //         x.WalletTransactionType == WalletTransactionType.Expense ? x.Value : decimal.Negate(x.Value));
    //     //
    //     // Value += financialBehavior.InternalTransactions.Where(x => !x.DeletedAt.HasValue).Sum(x =>
    //     //     !x.Coins.HasValue && x.InternalTransactionType == InternalTransactionType.Income
    //     //         ? x.Value
    //     //         : decimal.Negate(x.Value));
    // }
    //
    // public BalanceResponse(AssetPool AssetPool, DateTime? date)
    // {
    //     // Coins = AssetPool.IntialCoins;
    //     //
    //     // Coins += AssetPool.Transactions
    //     //     .Where(x => x.Date < date && !x.DeletedAt.HasValue &&
    //     //                 (!x.ApprovedAt.HasValue || (x.ApprovedAt.HasValue && x.LinkedToId.HasValue))).Sum(x =>
    //     //         x.WalletTransactionType == WalletTransactionType.Income ? decimal.Negate(x.Coins) : x.Coins);
    //     //
    //     // Coins += AssetPool.InternalTransactions.Where(x => x.Date < date && !x.DeletedAt.HasValue).Sum(x =>
    //     //     x.InternalTransactionType == InternalTransactionType.Income
    //     //         ? decimal.Negate(x.Coins ?? decimal.Zero)
    //     //         : x.Coins ?? decimal.Zero);
    // }
    //
    // public BalanceResponse(PokerManager manager, AvgRate? avgRate, DateTime? date)
    // {
    //     // Coins = manager.InitialCoins;
    //     //
    //     // Coins += manager.WalletTransactions
    //     //     .Where(x => x.Date < date && !x.DeletedAt.HasValue && ((!x.ApprovedAt.HasValue && !x.ExcelId.HasValue) ||
    //     //                                                            (x.ApprovedAt.HasValue && (x.LinkedToId.HasValue ||
    //     //                                                                x.ClientId.HasValue ||
    //     //                                                                x.TagId.HasValue ||
    //     //                                                                (x.ManagerId.HasValue && x.WalletId.HasValue)))))
    //     //     .Sum(x => x.WalletTransactionType == WalletTransactionType.Income ? decimal.Negate(x.Coins) : x.Coins);
    //     //
    //     // // Coins += manager.InternalTransactions.Where(x => x.Date < date && !x.DeletedAt.HasValue && x.Coins.HasValue).Sum(x =>
    //     // //     x.InternalTransactionType == InternalTransactionType.Income ? decimal.Negate(x.Coins) : x.Coins);
    //     //
    //     // Coins += manager.ClosingManagers.Where(x => x.End < date && !x.DeletedAt.HasValue)
    //     //     .Sum(x => decimal.Negate(x.TotalBalance));
    //     //
    //     // Value = manager.InitialValue;
    //     //
    //     // Value += manager.BankTransactions
    //     //     .Where(x => x.Date < date && !x.DeletedAt.HasValue && x.ManagerId == manager.Id)
    //     //     .Sum(x => x.BankTransactionType == BankTransactionType.Income ? x.Value : decimal.Negate(x.Value));
    //     //
    //     // Value += manager.InternalTransactions
    //     //     .Where(x => x.Date < date && !x.DeletedAt.HasValue && !x.ClosingManagerId.HasValue).Sum(x =>
    //     //         x.InternalTransactionType == InternalTransactionType.Expense ? decimal.Negate(x.Value) : x.Value);
    //     //
    //     // Value += manager.ClosingManagers.Where(x => x.End < date && !x.DeletedAt.HasValue)
    //     //     .Sum(x => decimal.Negate(x.TotalBalance + x.RakeBruto));
    //     //
    //     // Value += manager.WalletTransactions
    //     //     .Where(x => x.Date < date && !x.DeletedAt.HasValue && !x.TagId.HasValue && !x.ClientId.HasValue &&
    //     //                 (!x.ApprovedAt.HasValue || (x.ApprovedAt.HasValue && x.LinkedToId.HasValue))).Sum(x =>
    //     //         x.WalletTransactionType == WalletTransactionType.Expense ? x.Value : decimal.Negate(x.Value));
    //     //
    //     // // Value += manager.InternalTransactions.Where(x => x.Date < date && !x.DeletedAt.HasValue && !x.ClientId.HasValue)
    //     // //     .Sum(x => x.InternalTransactionType == InternalTransactionType.Income ? x.Value : decimal.Negate(x.Value));
    //     //
    //     // // var walletTransactions = manager.WalletTransactions.Where(x =>
    //     // //     !x.DeletedAt.HasValue && !x.TagId.HasValue &&
    //     // //     (!x.ApprovedAt.HasValue || (x.ApprovedAt.HasValue && x.LinkedToId.HasValue)));
    //     //
    //     // if (avgRate != null)
    //     //     AverateRate = avgRate.Value;
    //     // else
    //     //     AverateRate = manager.InitialExchangeRate;
    // }
    //
    // public decimal Value { get; set; }
    //
    // public decimal Coins { get; set; }
    //
    // public decimal AverateRate { get; set; }
}