using SFManagement.Application.DTOs.Common;
using SFManagement.Application.DTOs.Assets;
﻿namespace SFManagement.Application.DTOs.Transactions;

public class BankTransactionApproveRequest
{
    public Guid? FinancialBehaviorId { get; set; }

    public Guid? ClientId { get; set; }

    public Guid? ManagerId { get; set; }
}