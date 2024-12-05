using FluentValidation;
using SFManagement.Models;
using SFManagement.Services;

namespace SFManagement.ViewModels.Validators
{
    public class BankTransactionRequestValidator : AbstractValidator<BankTransactionRequest>
    {
        public BankTransactionRequestValidator()
        {
            RuleFor(x => x.BankTransactionType).NotEmpty();
            RuleFor(x => x.BankId).NotEmpty();
            RuleFor(x => x.Value).NotEmpty();
            RuleFor(x => x.Description).NotEmpty();

            RuleFor(x => x).Custom(async (bankTransaction, context) =>
            {
                if (!bankTransaction.TagId.HasValue && !bankTransaction.ClientId.HasValue)
                {
                    context.AddFailure($"Need send TagId or ClientId.");
                }
            });
        }
    }
}
