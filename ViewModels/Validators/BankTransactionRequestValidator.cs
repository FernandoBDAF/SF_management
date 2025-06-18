using FluentValidation;

namespace SFManagement.ViewModels.Validators;

public class BankTransactionRequestValidator : AbstractValidator<FiatAssetTransactionRequest>
{
    public BankTransactionRequestValidator()
    {
        RuleFor(x => x.TransactionDirection).NotEmpty();
        // RuleFor(x => x.BankId).NotEmpty();
        // RuleFor(x => x.Value).NotEmpty();
    }
}