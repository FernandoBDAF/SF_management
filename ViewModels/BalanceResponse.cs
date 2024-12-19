using SFManagement.Models;

namespace SFManagement.ViewModels
{
    public class BalanceResponse
    {
        public BalanceResponse(IEnumerable<BankTransaction> bankTransactions, IEnumerable<InternalTransaction> internalTransaction, decimal initialValue)
        {
            Value = bankTransactions.Where(x => !x.DeletedAt.HasValue && ((!x.ApprovedAt.HasValue) || (x.ApprovedAt.HasValue && x.LinkedToId.HasValue))).Sum(x => x.BankTransactionType == Enums.BankTransactionType.Income ? x.Value : decimal.Negate(x.Value));

            Value += internalTransaction.Where(x => !x.DeletedAt.HasValue).Sum(x => x.InternalTransactionType == Enums.InternalTransactionType.Income ? x.Value : decimal.Negate(x.Value));

            Value += initialValue;
        }

        public BalanceResponse(decimal initialValue, IEnumerable<BankTransaction> bankTransactions, IEnumerable<WalletTransaction> walletTransactions, IEnumerable<InternalTransaction> internalTransactions)
        {
            Value = bankTransactions.Where(x => !x.DeletedAt.HasValue && (!x.TagId.HasValue)
                                                && ((!x.ApprovedAt.HasValue) || (x.ApprovedAt.HasValue && x.LinkedToId.HasValue)))
                                .Sum(x => x.BankTransactionType == Enums.BankTransactionType.Income ? x.Value : decimal.Negate(x.Value));

            Value += initialValue;

            Value += walletTransactions.Where(x => !x.DeletedAt.HasValue && (!x.TagId.HasValue)
                                                                         && ((!x.ApprovedAt.HasValue) || (x.ApprovedAt.HasValue && x.LinkedToId.HasValue)))
                                .Sum(x => x.WalletTransactionType == Enums.WalletTransactionType.Expense ? x.Value : decimal.Negate(x.Value));

            Value += internalTransactions.Where(x => !x.DeletedAt.HasValue)
                                         .Sum(x => x.InternalTransactionType == Enums.InternalTransactionType.Income ? x.Value : decimal.Negate(x.Value));
        }

        public BalanceResponse(IEnumerable<BankTransaction> bankTransactions, IEnumerable<WalletTransaction> walletTransactions, IEnumerable<InternalTransaction> internalTransactions)
        {
            Value = bankTransactions.Where(x => !x.DeletedAt.HasValue && (!x.TagId.HasValue) && ((!x.ApprovedAt.HasValue) || (x.ApprovedAt.HasValue && x.LinkedToId.HasValue))).Sum(x => x.BankTransactionType == Enums.BankTransactionType.Income ? x.Value : decimal.Negate(x.Value));

            Value += walletTransactions.Where(x => !x.DeletedAt.HasValue && (!x.TagId.HasValue) && ((!x.ApprovedAt.HasValue) || (x.ApprovedAt.HasValue && x.LinkedToId.HasValue))).Sum(x => x.WalletTransactionType == Enums.WalletTransactionType.Expense ? x.Value : decimal.Negate(x.Value));

            Value += internalTransactions.Where(x => !x.DeletedAt.HasValue).Sum(x => x.InternalTransactionType == Enums.InternalTransactionType.Income ? x.Value : decimal.Negate(x.Value));
        }

        public BalanceResponse(decimal initialCredits, decimal initialBalance, IEnumerable<WalletTransaction> walletTransactions, IEnumerable<InternalTransaction> internalTransactions)
        {
            Coins += initialCredits;
            Coins += walletTransactions.Where(x => !x.DeletedAt.HasValue && ((!x.ApprovedAt.HasValue) || (x.ApprovedAt.HasValue && x.LinkedToId.HasValue)))
                                       .Sum(x => x.WalletTransactionType == Enums.WalletTransactionType.Income ? x.Coins : decimal.Negate(x.Coins));
            Coins += internalTransactions.Where(x => !x.DeletedAt.HasValue)
                                         .Sum(x => x.InternalTransactionType == Enums.InternalTransactionType.Income ? x.Coins ?? decimal.Zero : decimal.Negate(x.Coins ?? decimal.Zero));
        }

        public BalanceResponse(List<Wallet> wallets, decimal initialCoins, decimal initialExchangeRate, decimal initialValue)
        {
            Coins = wallets.Sum(x => new BalanceResponse(x.IntialCoins, x.InitialValue, x.Transactions, x.InternalTransactions).Coins);
            Coins += initialCoins;
        }

        public decimal Value { get; set; }

        public decimal Coins { get; set; }
    }
}
