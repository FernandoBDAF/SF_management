using FluentValidation;

namespace SFManagement.ViewModels.Validators;

public class ClosingManagerValidator : AbstractValidator<ClosingManagerRequest>
{
    public ClosingManagerValidator()
    {
        RuleFor(x => x.Start).LessThanOrEqualTo(x => x.End);
    }
}