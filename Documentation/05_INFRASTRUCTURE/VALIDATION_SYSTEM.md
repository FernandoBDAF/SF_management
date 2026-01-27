# Validation System

## Overview

The SF Management system uses a multi-layered validation approach combining FluentValidation for request validation and custom service-level validation for business rules.

---

## Validation Layers

```
Request → FluentValidation → Service Validation → Domain Service → Database
```

### 1. FluentValidation (Request Models)

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

### 2. WalletIdentifierValidationService

**File**: `Services/WalletIdentifierValidationService.cs`

```csharp
public class WalletIdentifierValidationService
{
    public ValidationResult ValidateWalletMetadata(WalletIdentifier wallet)
    {
        var errors = new List<ValidationError>();

        // Skip validation for internal/settlement wallets
        if (wallet.AssetGroup == AssetGroup.Internal || 
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
| Internal | None |
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

