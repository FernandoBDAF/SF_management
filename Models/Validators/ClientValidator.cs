using FluentValidation;

namespace SFManagement.Models.Validators
{
    public class ClientValidator : AbstractValidator<Client>
    {
        public ClientValidator()
        {
            RuleFor(x => x.Name).NotEmpty();

            RuleFor(x => x.CPF).NotEmpty();

            RuleFor(x => x.Birthday).NotEmpty();

            RuleFor(x => x.Email).NotEmpty().EmailAddress();
        }
    }
}
