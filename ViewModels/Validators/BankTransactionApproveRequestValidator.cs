using FluentValidation;

namespace SFManagement.ViewModels.Validators
{
    public class BankTransactionApproveRequestValidator : AbstractValidator<BankTransactionApproveRequest>
    {
        public BankTransactionApproveRequestValidator()
        {
            RuleFor(x => x).Custom((obj, context) =>
            {
                if (obj.TagId == null && obj.ClientId == null)
                {
                    context.AddFailure($"Need send TagId or ClientId.");
                }
            });

        }
    }
}
