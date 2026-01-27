using FluentValidation;
using SFManagement.Application.DTOs.Transactions;
using SFManagement.Domain.Enums.Assets;

namespace SFManagement.Application.Validators.Transactions;

public class TransferRequestValidator : AbstractValidator<TransferRequest>
{
    public TransferRequestValidator()
    {
        // Required fields
        RuleFor(x => x.SenderAssetHolderId)
            .NotEmpty().WithMessage("SenderAssetHolderId is required");

        RuleFor(x => x.ReceiverAssetHolderId)
            .NotEmpty().WithMessage("ReceiverAssetHolderId is required");

        RuleFor(x => x.AssetType)
            .NotEqual(AssetType.None).WithMessage("AssetType must be valid (not None)")
            .IsInEnum().WithMessage("AssetType must be a valid enum value");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0")
            .PrecisionScale(18, 2, true).WithMessage("Amount: max 18 digits, 2 decimal places");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)).WithMessage("Date cannot be more than 1 day in future");

        // Optional fields
        RuleFor(x => x.Description)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.ConversionRate)
            .GreaterThanOrEqualTo(0).When(x => x.ConversionRate.HasValue);

        RuleFor(x => x.Rate)
            .InclusiveBetween(0, 100).When(x => x.Rate.HasValue);

        // Optional wallet IDs
        RuleFor(x => x.SenderWalletIdentifierId)
            .NotEqual(Guid.Empty).When(x => x.SenderWalletIdentifierId.HasValue)
            .WithMessage("SenderWalletIdentifierId must be valid if provided");

        RuleFor(x => x.ReceiverWalletIdentifierId)
            .NotEqual(Guid.Empty).When(x => x.ReceiverWalletIdentifierId.HasValue)
            .WithMessage("ReceiverWalletIdentifierId must be valid if provided");
    }
}

