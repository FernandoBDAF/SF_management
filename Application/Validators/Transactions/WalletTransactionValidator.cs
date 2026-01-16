using SFManagement.Application.DTOs.Transactions;
using SFManagement.Application.DTOs.ImportedTransactions;
using SFManagement.Application.DTOs.Support;
﻿using FluentValidation;

namespace SFManagement.Application.Validators.Transactions;

public class WalletTransactionValidator : AbstractValidator<DigitalAssetTransactionRequest>
{
    public WalletTransactionValidator()
    {
        // RuleFor(x => x.Coins).NotEmpty();
        RuleFor(x => x.Date).NotEmpty();
        RuleFor(x => x.SenderWalletIdentifierId).NotEmpty();
        RuleFor(x => x.ReceiverWalletIdentifierId).NotEmpty();
        // RuleFor(x => x.ManagerId).NotEmpty();
    }
}