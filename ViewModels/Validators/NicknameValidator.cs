using FluentValidation;

namespace SFManagement.ViewModels.Validators;

public class NicknameValidator : AbstractValidator<NicknameRequest>
{
    public NicknameValidator()
    {
        RuleFor(x => x.Name).NotEmpty();

        RuleFor(x => x.WalletId).NotEmpty();

        RuleFor(x => x.ClientId).NotEmpty();
    }
}