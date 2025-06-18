using FluentValidation;

namespace SFManagement.ViewModels.Validators;

public class WalletTransactionValidator : AbstractValidator<DigitalAssetTransactionRequest>
{
    public WalletTransactionValidator()
    {
        // RuleFor(x => x.Coins).NotEmpty();
        RuleFor(x => x.Date).NotEmpty();
        RuleFor(x => x.TransactionDirection).NotEmpty();
        RuleFor(x => x.AssetWalletId).NotEmpty();
        // RuleFor(x => x.ManagerId).NotEmpty();
    }
}