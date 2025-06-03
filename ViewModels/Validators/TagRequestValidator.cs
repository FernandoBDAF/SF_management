using FluentValidation;

namespace SFManagement.ViewModels.Validators
{
    public class TagRequestValidator : AbstractValidator<TagRequest>
    {
        public TagRequestValidator()
        {
            RuleFor(x => x.Description).NotEmpty();
        }
    }
}
