using SFManagement.Application.DTOs.Transactions;
using SFManagement.Application.DTOs.ImportedTransactions;
using SFManagement.Application.DTOs.Support;
﻿using FluentValidation;

namespace SFManagement.Application.Validators.Transactions;

public class BankTransactionRequestValidator : AbstractValidator<FiatAssetTransactionRequest>
{
    public BankTransactionRequestValidator()
    {
        RuleFor(x => x.SenderWalletIdentifierId).NotEmpty();
        RuleFor(x => x.ReceiverWalletIdentifierId).NotEmpty();
        // RuleFor(x => x.BankId).NotEmpty();
        // RuleFor(x => x.Value).NotEmpty();
    }
}