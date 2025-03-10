using SFManagement.Models;

namespace SFManagement.ViewModels
{
    public class ProfitResponse
    {
        public ProfitResponse() { }

        public ProfitResponse(Manager manager)
        {
            switch (manager.ManagerType)
            {
                case Enums.ManagerType.Default:
                    Value = manager.WalletTransactions.Where(x => !x.DeletedAt.HasValue && ((!x.ApprovedAt.HasValue && !x.ExcelId.HasValue) ||
                        (x.ApprovedAt.HasValue &&
                         (x.LinkedToId.HasValue || x.ClientId.HasValue ||
                          x.TagId.HasValue ||
                          (x.ManagerId.HasValue && x.WalletId.HasValue))))).Sum(x => x.Profit);
                    break;
                case Enums.ManagerType.Apps:
                    manager.ClosingManagers.Where(x => !x.DeletedAt.HasValue).Sum(x => x.InternalTransactions.Where(i => !i.DeletedAt.HasValue && i.IsProfit).Sum(i => i.InternalTransactionType == Enums.InternalTransactionType.Income ? decimal.Negate(i.Value) : i.Value));
                    break;
            }
        }

        public ProfitResponse(Manager manager, DateTime start, DateTime end)
        {
            start = start.Date;
            end = end.Date;

            switch (manager.ManagerType)
            {
                case Enums.ManagerType.Default:
                    Value = manager.WalletTransactions.Where(x => !x.DeletedAt.HasValue && x.Date >= start && x.Date <= end).Sum(x => x.Profit);
                    break;
                case Enums.ManagerType.Apps:
                    manager.ClosingManagers.Where(x => !x.DeletedAt.HasValue).Sum(x => x.InternalTransactions.Where(i => !i.DeletedAt.HasValue && i.IsProfit && i.Date >= start && i.Date <= end).Sum(i => i.InternalTransactionType == Enums.InternalTransactionType.Income ? decimal.Negate(i.Value) : i.Value));
                    break;
            }
        }

        public decimal Value { get; set; }
    }
}
