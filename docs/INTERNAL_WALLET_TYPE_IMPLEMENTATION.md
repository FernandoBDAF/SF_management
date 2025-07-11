# Internal Wallet Type Implementation

## Overview

This document describes the implementation of the new `Internal` wallet type (WalletType.Internal = 4) that allows creating wallets with no metadata validation requirements.

## Business Context

### **Purpose**

Internal wallets are designed for company-internal transactions and operations where traditional wallet metadata (bank details, poker accounts, crypto addresses) is not applicable or required.

### **Use Cases**

- Internal company transfers
- System-generated transactions
- Administrative operations
- Temporary or placeholder wallets
- Inter-department transfers

## Technical Implementation

### **1. WalletType Enum Update**

```csharp
public enum WalletType
{
    // Fiat Assets
    BankWallet = 1,

    // Poker Assets
    PokerWallet = 2,

    // Crypto Assets
    CryptoWallet = 3,

    // Internal Transactions
    Internal = 4,
}
```

### **2. Metadata Validation Exemption**

#### **WalletIdentifier.ValidateMetadata()**

```csharp
public bool ValidateMetadata()
{
    return WalletType switch
    {
        WalletType.BankWallet => ValidateBankWalletMetadata(),
        WalletType.PokerWallet => ValidatePokerWalletMetadata(),
        WalletType.CryptoWallet => ValidateCryptoWalletMetadata(),
        WalletType.Internal => true, // Internal wallets require no metadata validation
        _ => false
    };
}
```

#### **WalletIdentifierValidationService**

```csharp
public ValidationResult ValidateWalletIdentifier(WalletIdentifier wallet)
{
    var result = new ValidationResult();

    // Metadata validation based on wallet type
    if (!wallet.ValidateMetadata())
    {
        result.AddError("Metadata", $"Invalid metadata for {wallet.WalletType}");
    }

    // Type-specific validation
    switch (wallet.WalletType)
    {
        case WalletType.BankWallet:
            ValidateBankWallet(wallet, result);
            break;
        case WalletType.PokerWallet:
            ValidatePokerWallet(wallet, result);
            break;
        case WalletType.CryptoWallet:
            ValidateCryptoWallet(wallet, result);
            break;
        case WalletType.Internal:
            ValidateInternalWallet(wallet, result);
            break;
    }

    return result;
}

private void ValidateInternalWallet(WalletIdentifier wallet, ValidationResult result)
{
    // No specific validation for Internal wallet type
}
```

### **3. Metadata Handling**

#### **SetMetadataFromFields() Method**

```csharp
public void SetMetadataFromFields(/* parameters */)
{
    var metadata = new Dictionary<string, string>();

    // Add fields based on wallet type
    if (WalletType == WalletType.PokerWallet)
    {
        // Poker-specific metadata
    }
    else if (WalletType == WalletType.BankWallet)
    {
        // Bank-specific metadata
    }
    else if (WalletType == WalletType.CryptoWallet)
    {
        // Crypto-specific metadata
    }
    else if (WalletType == WalletType.Internal)
    {
        // Internal wallets can have optional metadata, but none is required
        // For now, we don't set any specific metadata for internal wallets
    }

    Metadata = metadata;
}
```

#### **Type-Safe Accessors**

```csharp
// Generic accessor for Internal wallets (no enum constraints)
public string? GetInternalMetadata(string field) =>
    WalletType == WalletType.Internal ? GetMetadataValue(field) : null;

public void SetInternalMetadata(string field, string value)
{
    if (WalletType == WalletType.Internal)
        SetMetadataValue(field, value);
}
```

### **4. AutoMapper Integration**

```csharp
CreateMap<WalletIdentifier, WalletIdentifierResponse>()
    .AfterMap((src, dest, context) =>
    {
        // Extract metadata fields based on wallet type
        switch (src.WalletType)
        {
            case WalletType.PokerWallet:
                // Poker metadata extraction
                break;

            case WalletType.BankWallet:
                // Bank metadata extraction
                break;

            case WalletType.CryptoWallet:
                // Crypto metadata extraction
                break;

            case WalletType.Internal:
                // Internal wallets have no specific metadata fields to extract
                break;
        }
    });
```

### **5. Service Layer Enhancements**

#### **WalletIdentifierService New Methods**

```csharp
public async Task<List<WalletIdentifier>> GetInternalWallets()
{
    return await context.WalletIdentifiers
        .Include(wi => wi.AssetPool)
            .ThenInclude(aw => aw.BaseAssetHolder)
        .Include(wi => wi.Referral)
        .Where(wi => wi.WalletType == WalletType.Internal && !wi.DeletedAt.HasValue)
        .ToListAsync();
}

public async Task<List<WalletIdentifier>> GetInternalWalletsByMetadata(string metadataKey, string metadataValue)
{
    return await context.WalletIdentifiers
        .Include(wi => wi.AssetPool)
            .ThenInclude(aw => aw.BaseAssetHolder)
        .Include(wi => wi.Referral)
        .Where(wi => wi.WalletType == WalletType.Internal &&
                    wi.MetadataJson.Contains($"\"{metadataKey}\":\"{metadataValue}\"") &&
                    !wi.DeletedAt.HasValue)
        .ToListAsync();
}
```

## Key Features

### **1. No Metadata Requirements**

- Internal wallets can be created with empty metadata (`{}`)
- No validation errors for missing required fields
- Flexible metadata structure for future needs

### **2. Optional Metadata Support**

- Can still store custom metadata if needed
- Generic key-value accessor methods
- JSON storage maintains consistency with other wallet types

### **3. Consistent API**

- Same CRUD operations as other wallet types
- Integrated with existing validation pipeline
- Works with all existing transaction types

### **4. Type Safety**

- Dedicated accessor methods for Internal wallet metadata
- Compile-time safety through enum-based switching
- Runtime validation exemption

## Usage Examples

### **Creating Internal Wallets**

```csharp
// Minimal Internal wallet creation
var internalWallet = new WalletIdentifier
{
    AssetPoolId = assetPoolId,
    WalletType = WalletType.Internal,
    AccountClassification = AccountClassification.Asset,
    MetadataJson = "{}" // Empty metadata is valid
};

// Internal wallet with optional metadata
var internalWalletWithMeta = new WalletIdentifier
{
    AssetPoolId = assetPoolId,
    WalletType = WalletType.Internal,
    AccountClassification = AccountClassification.Asset,
    MetadataJson = JsonSerializer.Serialize(new Dictionary<string, string>
    {
        ["Purpose"] = "Inter-department transfer",
        ["Department"] = "Finance",
        ["CreatedBy"] = "System"
    })
};
```

### **API Request Examples**

```json
// POST /api/v1/wallet-identifiers
{
  "assetPoolId": "guid",
  "walletType": 4,
  "accountClassification": 1
  // No metadata fields required
}

// With optional metadata
{
  "assetPoolId": "guid",
  "walletType": 4,
  "accountClassification": 1,
  "metadataJson": "{\"Purpose\":\"Internal Transfer\",\"Department\":\"Finance\"}"
}
```

### **Querying Internal Wallets**

```csharp
// Get all internal wallets
var internalWallets = await walletIdentifierService.GetInternalWallets();

// Get internal wallets by metadata
var financeWallets = await walletIdentifierService.GetInternalWalletsByMetadata("Department", "Finance");

// Get internal wallet metadata
var purpose = wallet.GetInternalMetadata("Purpose");

// Set internal wallet metadata
wallet.SetInternalMetadata("LastUsed", DateTime.UtcNow.ToString());
```

## Benefits

### **1. Flexibility**

- No rigid metadata requirements
- Supports various internal use cases
- Easy to extend with custom metadata

### **2. Consistency**

- Follows existing wallet patterns
- Integrates with current validation system
- Works with all transaction types

### **3. Simplicity**

- Minimal setup required
- No complex validation rules
- Straightforward API usage

### **4. Future-Proof**

- Can add specific metadata requirements later
- Extensible metadata structure
- Maintains backward compatibility

## Validation Comparison

| Wallet Type  | Required Fields                                   | Validation                             |
| ------------ | ------------------------------------------------- | -------------------------------------- |
| BankWallet   | PixKey, AccountNumber, RoutingNumber, AccountType | Strict validation with business rules  |
| PokerWallet  | InputForTransactions                              | Required field validation              |
| CryptoWallet | WalletAddress, WalletCategory                     | Address format and category validation |
| **Internal** | **None**                                          | **No metadata validation**             |

## Integration Points

### **Transaction System**

- Internal wallets work with all transaction types
- Can be sender or receiver in any transaction
- No special transaction handling required

### **Asset Pool System**

- Can belong to any asset pool type
- Works with company pools and asset holder pools
- Supports all asset types (BrazilianReal, Bitcoin, etc.)

### **Reporting & Analytics**

- Included in balance calculations
- Appears in transaction reports
- Can be filtered by wallet type

## Best Practices

### **1. Use Cases**

- Use Internal wallets for company-internal operations
- Avoid for external customer transactions
- Consider metadata for tracking purposes

### **2. Metadata Management**

- Use descriptive metadata keys
- Include creation context in metadata
- Document custom metadata fields

### **3. Naming Conventions**

- Use clear, descriptive names for internal purposes
- Include department or function in metadata
- Maintain consistent metadata structure

## Security Considerations

### **1. Access Control**

- Internal wallets should have appropriate access restrictions
- Consider separate permissions for internal operations
- Audit internal wallet usage

### **2. Transaction Monitoring**

- Monitor internal transactions for compliance
- Implement appropriate approval workflows
- Maintain audit trails

### **3. Data Integrity**

- Validate business logic even without metadata requirements
- Ensure proper asset pool associations
- Maintain referential integrity

## Future Enhancements

### **Potential Additions**

- Internal wallet-specific metadata enums
- Enhanced validation rules for specific use cases
- Dedicated internal transaction types
- Advanced reporting for internal operations

### **Backward Compatibility**

- All changes maintain existing API compatibility
- No breaking changes to current wallet types
- Existing validation logic unchanged
