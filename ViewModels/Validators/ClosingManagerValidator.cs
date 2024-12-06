using FluentValidation;
using SFManagement.Models;

namespace SFManagement.ViewModels.Validators
{
    public class ClosingManagerValidator : AbstractValidator<ClosingManager>
    {
        public ClosingManagerValidator()
        {
            RuleFor(x => x.Start).LessThanOrEqualTo(x => x.End);
        }
    }
}
