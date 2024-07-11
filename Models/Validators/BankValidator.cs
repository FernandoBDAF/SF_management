using FluentValidation;

namespace SFManagement.Models.Validators
{
    public class BankValidator : AbstractValidator<Bank>
    {
        public BankValidator()
        {
            RuleFor(x => x.Name).NotEmpty();

            RuleFor(x => x.Code).NotEmpty();
        }
    }
}
