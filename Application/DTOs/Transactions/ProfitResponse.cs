using SFManagement.Application.DTOs.Common;
using SFManagement.Application.DTOs.Assets;
﻿using SFManagement.Domain.Enums;
using SFManagement.Domain.Common;
using SFManagement.Domain.Entities.AssetHolders;

namespace SFManagement.Application.DTOs.Transactions;

public class ProfitResponse
{
    public ProfitResponse()
    {
    }

    public ProfitResponse(PokerManager manager)
    {
        // switch (manager.ManagerProfitType)
        // {
        //     case ManagerProfitType.Default:
        //         Value = manager.WalletTransactions.Where(x => !x.DeletedAt.HasValue &&
        //                                                       ((!x.ApprovedAt.HasValue && !x.ExcelId.HasValue) ||
        //                                                        (x.ApprovedAt.HasValue &&
        //                                                         (x.LinkedToId.HasValue || x.ClientId.HasValue ||
        //                                                          x.TagId.HasValue ||
        //                                                          (x.ManagerId.HasValue && x.WalletId.HasValue)))))
        //             .Sum(x => x.Profit);
        //         break;
        //     case ManagerProfitType.Apps:
        //         manager.ClosingManagers.Where(x => !x.DeletedAt.HasValue).Sum(x =>
        //             x.InternalTransactions.Where(i => !i.DeletedAt.HasValue && i.IsProfit).Sum(i =>
        //                 i.InternalTransactionType == InternalTransactionType.Income
        //                     ? decimal.Negate(i.Value)
        //                     : i.Value));
        //         break;
        // }
    }

    public ProfitResponse(PokerManager manager, DateTime start, DateTime end)
    {
        // start = start.Date;
        // end = end.Date;
        //
        // switch (manager.ManagerProfitType)
        // {
        //     case ManagerProfitType.Default:
        //         Value = manager.WalletTransactions.Where(x => !x.DeletedAt.HasValue && x.Date >= start && x.Date <= end)
        //             .Sum(x => x.Profit);
        //         break;
        //     case ManagerProfitType.Apps:
        //         manager.ClosingManagers.Where(x => !x.DeletedAt.HasValue).Sum(x =>
        //             x.InternalTransactions
        //                 .Where(i => !i.DeletedAt.HasValue && i.IsProfit && i.Date >= start && i.Date <= end).Sum(i =>
        //                     i.InternalTransactionType == InternalTransactionType.Income
        //                         ? decimal.Negate(i.Value)
        //                         : i.Value));
        //         break;
        // }
    }

    public decimal Value { get; set; }
}