using SFManagement.Application.DTOs.Assets;
﻿using FluentValidation;

namespace SFManagement.Application.Validators.Assets;

public class WalletValidator : AbstractValidator<AssetPoolRequest>
{
    public WalletValidator()
    {
        // RuleFor(x => x.Name).NotEmpty();
        // RuleFor(x => x.ManagerId).NotEmpty();
    }
}