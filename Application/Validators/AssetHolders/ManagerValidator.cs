using SFManagement.Application.DTOs.AssetHolders;
﻿using FluentValidation;

namespace SFManagement.Application.Validators.AssetHolders;

public class ManagerValidator : AbstractValidator<PokerManagerRequest>
{
    public ManagerValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}