using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;
using SFManagement.Models.Entities;
using SFManagement.Models.Transactions;

namespace SFManagement.Models.Closing;

public class ClosingManager : BaseDomain
{
    // public Guid ManagerId { get; set; }
    public Guid ManagerId { get; set; }
    
    // public virtual PokerManager PokerManager { get; set; } = new();

    public DateTime Start { get; set; }
    
    public DateTime End { get; set; }

    public DateTime? DoneAt { get; set; }
    
    public DateTime? CalculatedAt { get; set; }

    [Precision(18, 2)] public decimal RakeBruto { get; set; }
    
    [Precision(18, 2)] public decimal TotalBalance { get; set; }
    
    [Precision(18, 2)] public decimal TotalRakeDiscounts { get; set; }

    public List<ClosingWallet> ClosingWallets { get; set; } = new();
    public List<ClosingNickname> ClosingNicknames { get; set; } = new();

    public List<InternalTransaction> InternalTransactions { get; set; } = new();

    // public static decimal CalcRake(List<ClosingNickname> closingNicknames, List<ClosingWallet> closingWallets)
    // {
    //     var rakeBruto = decimal.Zero;
    //
    //     foreach (var closingWallet in closingWallets)
    //         rakeBruto += closingNicknames.Where(x => x.Nickname.WalletId == closingWallet.WalletId).Sum(x => x.Rake) *
    //                      (closingWallet.ReturnRake / 100);
    //
    //     return rakeBruto;
    // }
    //
    // public static InternalTransaction CreateRakeInternalTransaction(Guid managerId, decimal rakeBruto,
    //     string managerName, DateTime closureEnd, Guid closingManagerId)
    // {
    //     return new InternalTransaction
    //     {
    //         Date = closureEnd,
    //         Description = $"{managerName} - Rake",
    //         InternalTransactionType = InternalTransactionType.Expense,
    //         Value = rakeBruto,
    //         ManagerId = managerId,
    //         ApprovedAt = DateTime.Now,
    //         ClosingManagerId = closingManagerId,
    //         IsProfit = true
    //     };
    // }
    //
    // public static List<InternalTransaction> CreateRakeNicknameReleases(Guid managerId,
    //     List<ClosingNickname> closingNicknames, string managerName, DateTime closureEnd, Guid closingManagerId)
    // {
    //     var list = new List<InternalTransaction>();
    //
    //     foreach (var closingNickname in closingNicknames)
    //     {
    //         var rakeback = closingNickname.Rake * (closingNickname.Rakeback / 100);
    //
    //         if (closingNickname.FatherNicknameId.HasValue)
    //         {
    //             var rakebackParent = closingNickname.Rake * (closingNickname.FatherPercentual / 100);
    //             rakeback = closingNickname.Rake * ((closingNickname.Rakeback - closingNickname.FatherPercentual) / 100);
    //
    //             if (rakebackParent != decimal.Zero)
    //                 list.Add(new InternalTransaction
    //                 {
    //                     Date = closureEnd,
    //                     Description = $"{managerName} - Rakeback (PAI) - {closingNickname.Nickname.Name}",
    //                     Value = rakebackParent,
    //                     ClientId = closingNickname.FatherNicknameId,
    //                     ManagerId = managerId,
    //                     InternalTransactionType = InternalTransactionType.Income,
    //                     ApprovedAt = DateTime.Now,
    //                     ClosingManagerId = closingManagerId,
    //                     IsProfit = true
    //                 });
    //         }
    //
    //         if (rakeback != decimal.Zero)
    //             list.Add(new InternalTransaction
    //             {
    //                 Date = closureEnd,
    //                 Description = $"{managerName} - Rakeback - {closingNickname.Nickname.Name}",
    //                 InternalTransactionType = InternalTransactionType.Income,
    //                 Value = rakeback,
    //                 ClientId = closingNickname.Nickname.ClientId,
    //                 ApprovedAt = DateTime.Now,
    //                 ClosingManagerId = closingManagerId,
    //                 IsProfit = true
    //             });
    //     }
    //
    //     return list;
    // }
}