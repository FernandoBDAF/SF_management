using SFManagement.Application.DTOs.Transactions;
using SFManagement.Application.DTOs.ImportedTransactions;
using SFManagement.Application.DTOs.Support;
﻿using FluentValidation;

namespace SFManagement.Application.Validators.Transactions;

public class FinancialBehaviorRequestValidator : AbstractValidator<CategoryRequest>
{
    public FinancialBehaviorRequestValidator()
    {
        RuleFor(x => x.Description).NotEmpty();
    }
}