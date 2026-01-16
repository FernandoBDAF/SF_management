using SFManagement.Application.DTOs.Transactions;
using SFManagement.Application.DTOs.ImportedTransactions;
using SFManagement.Application.DTOs.Support;
﻿using FluentValidation;

namespace SFManagement.Application.Validators.Transactions;

public class ImportBuyTransactionsRequestValidator : AbstractValidator<ImportBuySellTransactionsRequest>
{
    public ImportBuyTransactionsRequestValidator()
    {
        RuleFor(x => x.File).NotNull();
        RuleFor(x => x.WalletId).NotEmpty();
    }
}