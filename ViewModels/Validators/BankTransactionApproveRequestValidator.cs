using FluentValidation;

namespace SFManagement.ViewModels.Validators;

public class BankTransactionApproveRequestValidator : AbstractValidator<BankTransactionApproveRequest>
{
    public BankTransactionApproveRequestValidator()
    {
        RuleFor(x => x).Custom((obj, context) =>
        {
            if (obj.TagId == null && obj.ClientId == null && obj.ManagerId == null)
                context.AddFailure("Need send TagId or ClientId or ManagerId.");

            if (obj.ClientId != null && obj.ManagerId != null)
                context.AddFailure("I have to choose client or manager.");
        });
    }
}