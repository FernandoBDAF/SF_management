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

        public List<ClosingWallet> ClosingWallets { get; set; } = new List<ClosingWallet>();

        public List<ClosingNickname> ClosingNicknames { get; set; } = new List<ClosingNickname>();

        public static decimal CalcRake(List<ClosingNickname> closingNicknames, List<ClosingWallet> closingWallets)
        {
            var rakeBruto = decimal.Zero;

            foreach (var closingWallet in closingWallets)
            {
                rakeBruto += (closingNicknames.Where(x => x.Nickname.WalletId == closingWallet.Id).Sum(x => x.Rake)) * (closingWallet.ReturnRake / 100);
            }

            return rakeBruto;
        }

        //public static BankTransaction CreateRakeAccountAdminRelease(int accountAdminClientId, 
        //                                                            decimal rakeBruto, 
        //                                                            int accountAdminClosureId, 
        //                                                            string accountAdminName, 
        //                                                            DateTime closureEnd)
        //{
        //    return new BankTransaction
        //    {
        //        ExpirationDate = closureEnd,
        //        History = $"{accountAdminName} - Rake",
        //        PaymentDate = closureEnd,
        //        ReleaseType = Enums.ReleaseType.Expense,
        //        Value = rakeBruto,
        //        CreateInClientTimeline = true,
        //        IsForFinance = true,
        //        Confirmed = closureEnd,
        //        ClientId = accountAdminClientId,
        //        AccountAdminClosureId = accountAdminClosureId,
        //        CreatedAt = closureEnd
        //    };
        //}
    }
}
