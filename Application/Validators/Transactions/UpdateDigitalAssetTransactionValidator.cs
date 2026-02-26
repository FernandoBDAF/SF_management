using FluentValidation;
using SFManagement.Application.DTOs.Transactions;

namespace SFManagement.Application.Validators.Transactions;

public class UpdateDigitalAssetTransactionValidator : AbstractValidator<UpdateDigitalAssetTransactionRequest>
{
    public UpdateDigitalAssetTransactionValidator()
    {
        When(x => x.Date.HasValue, () =>
        {
            RuleFor(x => x.Date!.Value)
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("Transaction date cannot be in the future.");
        });

        When(x => x.AssetAmount.HasValue, () =>
        {
            RuleFor(x => x.AssetAmount!.Value)
                .GreaterThan(0)
                .WithMessage("Amount must be greater than zero.");
        });

        When(x => x.ConversionRate.HasValue, () =>
        {
            RuleFor(x => x.ConversionRate!.Value)
                .GreaterThan(0)
                .WithMessage("Conversion rate must be greater than zero.");
        });

        When(x => x.Description != null, () =>
        {
            RuleFor(x => x.Description!)
                .MaximumLength(500)
                .WithMessage("Description cannot exceed 500 characters.");
        });
    }
}
