using FluentValidation;

namespace SFManagement.ViewModels.Validators;

public class WalletValidator : AbstractValidator<AssetWalletRequest>
{
    public WalletValidator()
    {
        // RuleFor(x => x.Name).NotEmpty();
        // RuleFor(x => x.ManagerId).NotEmpty();
    }
}