using SFManagement.Application.DTOs.AssetHolders;
﻿using FluentValidation;

namespace SFManagement.Application.Validators.AssetHolders;

public class BankRequestValidator : AbstractValidator<BankRequest>
{
    public BankRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();

        RuleFor(x => x.Code).NotEmpty();
    }
}