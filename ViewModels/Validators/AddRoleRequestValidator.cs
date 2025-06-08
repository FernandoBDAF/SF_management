using FluentValidation;

namespace SFManagement.ViewModels.Validators;

public class AddRoleRequestValidator : AbstractValidator<AddRoleRequest>
{
    public AddRoleRequestValidator()
    {
        RuleFor(x => x.Email).EmailAddress().NotEmpty();

        RuleFor(x => x.Password).NotEmpty();

        RuleFor(x => x.Role).NotEmpty();
    }
}