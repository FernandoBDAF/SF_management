using FluentValidation;
using SFManagement.Models;

namespace SFManagement.ViewModels.Validators
{
    public class BankRequestValidator : AbstractValidator<BankRequest>
    {
        public BankRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty();

            RuleFor(x => x.Code).NotEmpty();
        }
    }
}
