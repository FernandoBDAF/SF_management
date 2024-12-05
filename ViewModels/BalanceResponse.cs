using SFManagement.Models;

namespace SFManagement.ViewModels
{
    public class BalanceResponse
    {
        public BalanceResponse(decimal initialValue, IEnumerable<BankTransaction> bankTransactions, IEnumerable<WalletTransaction> walletTransactions)
        {
            Value = bankTransactions.Where(x => !x.DeletedAt.HasValue
                                                && ((!x.ApprovedAt.HasValue) || (x.ApprovedAt.HasValue && x.LinkedToId.HasValue)))
                                .Sum(x => x.BankTransactionType == Enums.BankTransactionType.Income ? x.Value : decimal.Negate(x.Value));

            Value += initialValue;

            Value += walletTransactions.Where(x => !x.DeletedAt.HasValue
                                            && ((!x.ApprovedAt.HasValue) || (x.ApprovedAt.HasValue && x.LinkedToId.HasValue)))
                                .Sum(x => x.WalletTransactionType == Enums.WalletTransactionType.Income ? x.Value : decimal.Negate(x.Value));
        }

        public decimal? Value { get; set; }
    }
}
