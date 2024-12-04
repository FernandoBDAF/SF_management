using FluentValidation;

namespace SFManagement.ViewModels.Validators
{
    public class WalletTransactionValidator : AbstractValidator<WalletTransactionRequest>
    {
        public WalletTransactionValidator()
        {
            RuleFor(x => x.Value).NotEmpty();
            RuleFor(x => x.Coins).NotEmpty();
            RuleFor(x => x.ExchangeRate).NotEmpty();
            RuleFor(x => x.Description).NotEmpty();
            RuleFor(x => x.Date).NotEmpty();
            RuleFor(x => x.WalletTransactionType).NotEmpty();
            RuleFor(x => x.WalletId).NotEmpty();

            //RuleFor(x => x).Custom((walletTransaction, context) =>
            //{
            //    if (!walletTransaction.TagId.HasValue && !walletTransaction.ClientId.HasValue)
            //    {
            //        context.AddFailure($"Need send TagId or ClientId.");
            //    }
            //});
        }
    }
}
