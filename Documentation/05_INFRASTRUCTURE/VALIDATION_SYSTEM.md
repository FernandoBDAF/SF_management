# Validation System

## Overview

The SF Management system uses a multi-layered validation approach combining FluentValidation for request validation and custom service-level validation for business rules.

---

## Validation Layers

```
Request → FluentValidation → Service Validation → Domain Service → Database
```

### 1. FluentValidation (Request Models)

#### Entity Creation Validators

**File**: `ViewModels/Validators/ClientRequestValidator.cs`

```csharp
public class ClientRequestValidator : AbstractValidator<ClientRequest>
{
    public ClientRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(255).WithMessage("Name cannot exceed 255 characters");

        RuleFor(x => x.GovernmentNumber)
            .Must(BeValidGovernmentNumber).WithMessage("Invalid government number format");
    }

    private bool BeValidGovernmentNumber(string? number)
    {
        if (string.IsNullOrEmpty(number)) return true;
        // CPF: 11 digits, CNPJ: 14 digits
        return number.Length == 11 || number.Length == 14;
    }
}
```

#### Transaction Update Validators

Transaction update requests use dedicated validators that support partial updates where all fields are optional.

**File**: `Application/Validators/Transactions/UpdateFiatAssetTransactionValidator.cs`

```csharp
public class UpdateFiatAssetTransactionValidator : AbstractValidator<UpdateFiatAssetTransactionRequest>
{
    public UpdateFiatAssetTransactionValidator()
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

        When(x => x.Description != null, () =>
        {
            RuleFor(x => x.Description!)
                .MaximumLength(500)
                .WithMessage("Description cannot exceed 500 characters.");
        });
    }
}
```

**File**: `Application/Validators/Transactions/UpdateDigitalAssetTransactionValidator.cs`

```csharp
public class UpdateDigitalAssetTransactionValidator : AbstractValidator<UpdateDigitalAssetTransactionRequest>
{
    public UpdateDigitalAssetTransactionValidator()
    {
        // Inherits Date, AssetAmount, Description validation from Fiat pattern

        When(x => x.ConversionRate.HasValue, () =>
        {
            RuleFor(x => x.ConversionRate!.Value)
                .GreaterThan(0)
                .WithMessage("Conversion rate must be greater than zero.");
        });
    }
}
```

**Key Characteristics:**
- All fields are nullable (optional) for partial updates
- Validation rules only apply when a field is provided (`When(x => x.Field.HasValue, ...)`)
- Validators are registered in DI container in `Program.cs`

#### Validator Registration

```csharp
// Program.cs
builder.Services.AddScoped<IValidator<UpdateFiatAssetTransactionRequest>, UpdateFiatAssetTransactionValidator>();
builder.Services.AddScoped<IValidator<UpdateDigitalAssetTransactionRequest>, UpdateDigitalAssetTransactionValidator>();
```

### 2. WalletIdentifierValidationService

**File**: `Services/WalletIdentifierValidationService.cs`

```csharp
public class WalletIdentifierValidationService
{
    public ValidationResult ValidateWalletMetadata(WalletIdentifier wallet)
    {
        var errors = new List<ValidationError>();

        // Skip validation for flexible/settlement wallets
        if (wallet.AssetGroup == AssetGroup.Flexible || 
            wallet.AssetGroup == AssetGroup.Settlements)
            return ValidationResult.Success();

        switch (wallet.AssetGroup)
        {
            case AssetGroup.FiatAssets:
                ValidateBankMetadata(wallet, errors);
                break;
            case AssetGroup.PokerAssets:
                ValidatePokerMetadata(wallet, errors);
                break;
            case AssetGroup.CryptoAssets:
                ValidateCryptoMetadata(wallet, errors);
                break;
        }

        return new ValidationResult(errors);
    }
}
```

### 3. AssetPoolValidationService

```csharp
public class AssetPoolValidationService
{
    public async Task<ValidationResult> ValidateAssetPoolCreation(AssetPool pool)
    {
        var errors = new List<ValidationError>();

        // Check for duplicate company pools
        if (pool.BaseAssetHolderId == null)
        {
            var existing = await GetCompanyAssetPoolByType(pool.AssetGroup);
            if (existing != null)
                errors.Add(new("AssetGroup", "Company pool already exists for this type"));
        }

        return new ValidationResult(errors);
    }

    public async Task<ValidationResult> ValidateAssetPoolDeletion(Guid poolId)
    {
        var errors = new List<ValidationError>();

        // Check for existing wallets
        var hasWallets = await HasWalletIdentifiers(poolId);
        if (hasWallets)
            errors.Add(new("Pool", "Cannot delete pool with existing wallets", "HAS_WALLETS"));

        // Check for transactions
        var hasTransactions = await HasTransactions(poolId);
        if (hasTransactions)
            errors.Add(new("Pool", "Cannot delete pool with transactions", "HAS_TRANSACTIONS"));

        return new ValidationResult(errors);
    }
}
```

### 4. Domain Service Validation

**File**: `Services/AssetHolderDomainService.cs`

```csharp
public class AssetHolderDomainService : IAssetHolderDomainService
{
    public async Task<DomainValidationResult> ValidateClientCreation(ClientRequest request)
    {
        var errors = new List<ValidationError>();

        // Business rules
        if (await IsDuplicateGovernmentNumber(request.GovernmentNumber))
            errors.Add(new("GovernmentNumber", "Government number already exists"));

        return new DomainValidationResult(errors);
    }

    public async Task<bool> CanDeleteAssetHolder(Guid id)
    {
        // Check for active transactions
        if (await HasActiveTransactions(id)) return false;
        
        // Check for non-zero balance
        if (await GetTotalBalance(id) != 0) return false;

        return true;
    }
}
```

---

## Required Metadata by AssetGroup

| AssetGroup | Required Fields |
|------------|-----------------|
| FiatAssets | BankName, AccountNumber, PixKey |
| PokerAssets | PlayerNickname, PlayerPhone, InputForTransactions |
| CryptoAssets | WalletAddress |
| Flexible | None |
| Settlements | None |

---

## Validation Response Format

```json
{
  "title": "Validation Failed",
  "status": 400,
  "errors": {
    "Name": ["Name is required"],
    "GovernmentNumber": ["Invalid format", "Already exists"]
  }
}
```

---

## Best Practices

1. **Validate early** - Use FluentValidation at API boundary
2. **Validate business rules in services** - Domain validation separate from format
3. **Collect all errors** - Don't stop at first error
4. **Use meaningful codes** - `REQUIRED`, `INVALID_FORMAT`, `DUPLICATE`

---

## Related Documentation

- [ERROR_HANDLING.md](ERROR_HANDLING.md) - Exception handling
- [ASSET_INFRASTRUCTURE.md](../03_CORE_SYSTEMS/ASSET_INFRASTRUCTURE.md) - Metadata details

