using FluentValidation;

namespace SFManagement.ViewModels.Validators;

public class FinancialBehaviorRequestValidator : AbstractValidator<FinancialBehaviorRequest>
{
    public FinancialBehaviorRequestValidator()
    {
        RuleFor(x => x.Description).NotEmpty();
    }
}