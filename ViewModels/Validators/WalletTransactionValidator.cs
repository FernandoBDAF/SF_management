using FluentValidation;

namespace SFManagement.ViewModels.Validators
{
    public class WalletTransactionValidator : AbstractValidator<WalletTransactionRequest>
    {
        public WalletTransactionValidator()
        {
            RuleFor(x => x.Value).NotEmpty();
            RuleFor(x => x.Description).NotEmpty();
            RuleFor(x => x.Date).NotEmpty();
            RuleFor(x => x.ApprovedAt).NotEmpty();
            RuleFor(x => x.WalletTransactionType).NotEmpty();
            RuleFor(x => x.WalletId).NotEmpty();
        }
    }
}
