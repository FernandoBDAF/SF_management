using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SFManagement.Models
{
    public class ClosingManager : BaseDomain
    {
        [ForeignKey("Manager")]
        public Guid ManagerId { get; set; }

        public virtual Manager Manager { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public DateTime? DoneAt { get; set; }

        public DateTime? CalculatedAt { get; set; }

        [Precision(18, 2)]
        public decimal RakeBruto { get; set; }

        [Precision(18, 2)]
        public decimal TotalBalance { get; set; }

        [Precision(18, 2)]
        public decimal TotalRakeDiscounts { get; set; }

        public List<ClosingWallet> ClosingWallets { get; set; } = new List<ClosingWallet>();

        public List<ClosingNickname> ClosingNicknames { get; set; } = new List<ClosingNickname>();

        public List<InternalTransaction> InternalTransactions { get; set; } = new List<InternalTransaction>();

        public static decimal CalcRake(List<ClosingNickname> closingNicknames, List<ClosingWallet> closingWallets)
        {
            var rakeBruto = decimal.Zero;

            foreach (var closingWallet in closingWallets)
            {
                rakeBruto += (closingNicknames.Where(x => x.Nickname.WalletId == closingWallet.WalletId).Sum(x => x.Rake)) * (closingWallet.ReturnRake / 100);
            }

            return rakeBruto;
        }

        public static InternalTransaction CreateRakeInternalTransaction(Guid managerId, decimal rakeBruto, string managerName, DateTime closureEnd, Guid closingManagerId)
        {
            return new InternalTransaction
            {
                Date = closureEnd,
                Description = $"{managerName} - Rake",
                InternalTransactionType = Enums.InternalTransactionType.Expense,
                Value = rakeBruto,
                ManagerId = managerId,
                ApprovedAt = DateTime.Now,
                ClosingManagerId = closingManagerId,
                IsProfit = true
            };
        }

        public static List<InternalTransaction> CreateRakeNicknameReleases(Guid managerId, List<ClosingNickname> closingNicknames, string managerName, DateTime closureEnd, Guid closingManagerId)
        {
            var list = new List<InternalTransaction>();

            foreach (var closingNickname in closingNicknames)
            {
                var rakeback = closingNickname.Rake * (closingNickname.Rakeback / 100);

                if (closingNickname.FatherNicknameId.HasValue)
                {
                    var rakebackParent = closingNickname.Rake * (closingNickname.FatherPercentual / 100);
                    rakeback = closingNickname.Rake * ((closingNickname.Rakeback - closingNickname.FatherPercentual) / 100);

                    if (rakebackParent != decimal.Zero)
                    {
                        list.Add(new InternalTransaction
                        {
                            Date = closureEnd,
                            Description = $"{managerName} - Rakeback (PAI) - {closingNickname.Nickname.Name}",
                            Value = rakebackParent,
                            ClientId = closingNickname.FatherNicknameId,
                            ManagerId = managerId,
                            InternalTransactionType = Enums.InternalTransactionType.Income,
                            ApprovedAt = DateTime.Now,
                            ClosingManagerId = closingManagerId,
                            IsProfit = true
                        });
                    }
                }

                if (rakeback != decimal.Zero)
                {
                    list.Add(new InternalTransaction
                    {
                        Date = closureEnd,
                        Description = $"{managerName} - Rakeback - {closingNickname.Nickname.Name}",
                        InternalTransactionType = Enums.InternalTransactionType.Income,
                        Value = rakeback,
                        ClientId = closingNickname.Nickname.ClientId,
                        ApprovedAt = DateTime.Now,
                        ClosingManagerId = closingManagerId,
                        IsProfit = true
                    });
                }
            }

            return list;
        }
    }
}
