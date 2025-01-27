using Azure;
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

        public BalanceResponse(Client client)
        {
            Value = client.InitialValue;

            Value += client.BankTransactions.Where(x => !x.DeletedAt.HasValue && (!x.TagId.HasValue) && ((!x.ApprovedAt.HasValue) || (x.ApprovedAt.HasValue && x.LinkedToId.HasValue))).Sum(x => x.BankTransactionType == Enums.BankTransactionType.Income ? x.Value : decimal.Negate(x.Value));

            Value += client.WalletTransactions.Where(x => !x.DeletedAt.HasValue && (!x.TagId.HasValue) && ((!x.ApprovedAt.HasValue) || (x.ApprovedAt.HasValue && x.LinkedToId.HasValue))).Sum(x => x.WalletTransactionType == Enums.WalletTransactionType.Expense ? x.Value : decimal.Negate(x.Value));

            Value += client.InternalTransactions.Where(x => !x.DeletedAt.HasValue).Sum(x => !x.Coins.HasValue && x.InternalTransactionType == Enums.InternalTransactionType.Income ? x.Value : decimal.Negate(x.Value));
        }

        public BalanceResponse(Tag tag)
        {
            Value = tag.BankTransactions.Where(x => !x.DeletedAt.HasValue && (x.TagId.HasValue) && ((!x.ApprovedAt.HasValue) || (x.ApprovedAt.HasValue && x.LinkedToId.HasValue))).Sum(x => x.BankTransactionType == Enums.BankTransactionType.Income ? x.Value : decimal.Negate(x.Value));

            Value += tag.WalletTransactions.Where(x => !x.DeletedAt.HasValue && (x.TagId.HasValue) && ((!x.ApprovedAt.HasValue) || (x.ApprovedAt.HasValue && x.LinkedToId.HasValue))).Sum(x => x.WalletTransactionType == Enums.WalletTransactionType.Expense ? x.Value : decimal.Negate(x.Value));

            Value += tag.InternalTransactions.Where(x => !x.DeletedAt.HasValue).Sum(x => !x.Coins.HasValue && x.InternalTransactionType == Enums.InternalTransactionType.Income ? x.Value : decimal.Negate(x.Value));
        }

        public BalanceResponse(Wallet wallet)
        {
            Coins += wallet.IntialCoins;

            Coins += wallet.Transactions.Where(x => !x.DeletedAt.HasValue && ((!x.ApprovedAt.HasValue) || (x.ApprovedAt.HasValue && x.LinkedToId.HasValue))).Sum(x => x.WalletTransactionType == Enums.WalletTransactionType.Income ? decimal.Negate(x.Coins) : x.Coins);

            Coins += wallet.InternalTransactions.Where(x => !x.DeletedAt.HasValue).Sum(x => x.InternalTransactionType == Enums.InternalTransactionType.Income ? decimal.Negate(x.Coins ?? decimal.Zero) : x.Coins ?? decimal.Zero);
        }

        public BalanceResponse(Manager manager, AvgRate avgRate)
        {
            Coins = manager.Wallets.Sum(x => new BalanceResponse(x).Coins);

            Coins += manager.InitialCoins;

            Coins += manager.InternalTransactions.Where(x => !x.DeletedAt.HasValue).Sum(x => x.InternalTransactionType == Enums.InternalTransactionType.Income ? x.Coins ?? decimal.Zero : decimal.Negate(x.Coins ?? decimal.Zero));

            Value += manager.InitialValue;

            Value += manager.BankTransactions.Where(x => !x.DeletedAt.HasValue && (!x.TagId.HasValue) && ((!x.ApprovedAt.HasValue) || (x.ApprovedAt.HasValue && x.LinkedToId.HasValue))).Sum(x => x.BankTransactionType == Enums.BankTransactionType.Income ? x.Value : decimal.Negate(x.Value));

            Value += manager.WalletTransactions.Where(x => !x.DeletedAt.HasValue && (!x.TagId.HasValue) && (!x.ClientId.HasValue) && ((!x.ApprovedAt.HasValue) || (x.ApprovedAt.HasValue && x.LinkedToId.HasValue))).Sum(x => x.WalletTransactionType == Enums.WalletTransactionType.Expense ? x.Value : decimal.Negate(x.Value));

            Value += manager.InternalTransactions.Where(x => !x.DeletedAt.HasValue&& !x.ClientId.HasValue).Sum(x => x.InternalTransactionType == Enums.InternalTransactionType.Income ? x.Value : decimal.Negate(x.Value));

            var walletTransactions = manager.WalletTransactions.Where(x => !x.DeletedAt.HasValue && (!x.TagId.HasValue) && ((!x.ApprovedAt.HasValue) || (x.ApprovedAt.HasValue && x.LinkedToId.HasValue)));

            if (avgRate != null)
            {
                AverateRate = avgRate.Value;
            }
            else
            {
                AverateRate = manager.InitialExchangeRate;
            }
        }

        public decimal Value { get; set; }

        public decimal Coins { get; set; }

        public decimal AverateRate { get; set; }
    }
}
