using FluentValidation;

namespace SFManagement.Models.Validators
{
    public class BankTransactionValidator : AbstractValidator<BankTransaction>
    {
        public BankTransactionValidator()
        {
            RuleFor(x => x.BankTransactionType).NotEmpty();
            RuleFor(x => x.BankId).NotEmpty();
            RuleFor(x => x.Value).NotEmpty();
            RuleFor(x => x.Description).NotEmpty();
        }
    }
}
