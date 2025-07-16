using FluentValidation;

namespace SFManagement.ViewModels.Validators;

public class FinancialBehaviorRequestValidator : AbstractValidator<CategoryRequest>
{
    public FinancialBehaviorRequestValidator()
    {
        RuleFor(x => x.Description).NotEmpty();
    }
}