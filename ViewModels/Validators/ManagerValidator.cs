using FluentValidation;

namespace SFManagement.ViewModels.Validators;

public class ManagerValidator : AbstractValidator<ManagerRequest>
{
    public ManagerValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}