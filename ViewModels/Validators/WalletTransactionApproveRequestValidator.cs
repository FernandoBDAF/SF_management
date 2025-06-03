using FluentValidation;

namespace SFManagement.ViewModels.Validators
{
    public class WalletTransactionApproveRequestValidator : AbstractValidator<WalletTransactionApproveRequest>
    {
        public WalletTransactionApproveRequestValidator()
        {
            RuleFor(x => x.ExchangeRate).GreaterThan(decimal.Zero);

            RuleFor(x => x.Value).GreaterThan(decimal.Zero);

            RuleFor(x => x.NicknameId).NotEmpty();

            RuleFor(x => x).Custom((obj, context) =>
            {
                if(obj.TagId == null && obj.ClientId == null && obj.WalletId == null)
                {
                    context.AddFailure($"Need send TagId or ClientId or WalletId.");
                }
            });
        }
    }
}
