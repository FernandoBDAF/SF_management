using FluentValidation;
using SFManagement.Models;

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
        }
    }
}
