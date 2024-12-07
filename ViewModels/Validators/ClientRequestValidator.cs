using FluentValidation;
using SFManagement.Models;

namespace SFManagement.ViewModels.Validators
{
    public class ClientRequestValidator : AbstractValidator<ClientRequest>
    {
        public ClientRequestValidator()
        {
            // RuleFor(x => x.Name).NotEmpty();
            // RuleFor(x => x.CPF).NotEmpty();
            // RuleFor(x => x.Birthday).NotEmpty();
            // RuleFor(x => x.Email).NotEmpty().EmailAddress();
        }
    }
}
