using FluentValidation;

namespace SFManagement.ViewModels.Validators
{
    public class InternalTransactionApproveRequestValidator : AbstractValidator<InternalTransactionApproveRequest>
    {
        public InternalTransactionApproveRequestValidator()
        {
            RuleFor(x => x).Custom((obj, context) =>
            {
                if(obj.TagId == null && obj.ClientId == null && obj.WalletId == null && obj.BankId == null)
                {
                    context.AddFailure($"Need send TagId or ClientId or ManagerId.");
                }
            });
        }
    }
}
