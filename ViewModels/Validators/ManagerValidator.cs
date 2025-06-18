using FluentValidation;

namespace SFManagement.ViewModels.Validators;

public class ManagerValidator : AbstractValidator<PokerManagerRequest>
{
    public ManagerValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}