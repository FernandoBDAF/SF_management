using FluentValidation;

namespace SFManagement.ViewModels.Validators;

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