using FluentValidation;

namespace SFManagement.ViewModels.Validators
{
    public class ImportBuyTransactionsRequestValidator : AbstractValidator<ImportBuyTransactionsRequest>
    {
        public ImportBuyTransactionsRequestValidator()
        {
            RuleFor(x => x.File).NotNull();
            RuleFor(x => x.WalletId).NotEmpty();
        }
    }
}
