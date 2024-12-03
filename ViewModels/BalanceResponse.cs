using SFManagement.Models;

namespace SFManagement.ViewModels
{
    public class BalanceResponse
    {
        public BalanceResponse(IEnumerable<BankTransaction> transactions)
        {
            Value = transactions.Where(x => !x.DeletedAt.HasValue
                                            && ((!x.ApprovedAt.HasValue) || (x.ApprovedAt.HasValue && x.LinkedToId.HasValue)))
                                .Sum(x => x.BankTransactionType == Enums.BankTransactionType.Income ? x.Value : decimal.Negate(x.Value));

            //TODO: ACRESCENTAR WALLETTRANSACTIONS.
        }

        public decimal? Value { get; set; }
    }
}
