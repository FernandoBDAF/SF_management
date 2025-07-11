# Asset Infrastructure Final Design Summary

## Overview

This document presents the final design for the asset infrastructure system supporting fiat, poker, and crypto assets with unified transaction handling.

## Core Architecture

### 1. AssetPool Model

```csharp
public class AssetPool : BaseDomain
{
    [Required] public Guid BaseAssetHolderId { get; set; }
    public virtual BaseAssetHolder BaseAssetHolder { get; set; }

    public AssetType AssetType { get; set; }  // BrazilianReal, PokerStars, Bitcoin, etc.

    public virtual ICollection<WalletIdentifier> WalletIdentifiers { get; set; }
}
```

**Key Features:**

- Links to BaseAssetHolder (Client, Bank, Member, PokerManager)
- Uses existing AssetType enum (granular: BrazilianReal, PokerStars, Bitcoin, etc.)
- One AssetPool per AssetType per BaseAssetHolder
- Container for multiple WalletIdentifiers

### 2. WalletIdentifier Model

```csharp
public class WalletIdentifier : BaseDomain
{
    [Required] public Guid AssetPoolId { get; set; }
    public virtual AssetPool AssetPool { get; set; }

    public WalletType WalletType { get; set; }  // BankWallet, PokerWallet, CryptoWallet

    [Required] public string InputForTransactions { get; set; }
    public bool IsActive { get; set; } = true;

    // Metadata stored as JSON
    public string MetadataJson { get; set; } = "{}";

    [NotMapped]
    public Dictionary<string, string> Metadata { get; set; }
}
```

**Key Features:**

- Removed redundant properties (RouteInfo, IdentifierInfo)
- All contextual data stored in validated metadata
- Type-safe metadata accessors
- JSON storage for flexibility

## 3. Enum Structure

### WalletType (3 main categories)

```csharp
public enum WalletType
{
    BankWallet = 1,     // Fiat assets
    PokerWallet = 2,    // Poker assets
    CryptoWallet = 3    // Crypto assets
}
```

### Metadata Field Enums (Type-specific, non-redundant)

#### BankWalletMetadata

```csharp
public enum BankWalletMetadata
{
    BankName,           // Bank institution name
    AccountNumber,      // Account number
    AccountType,        // Checking, Savings, etc.
    BranchCode,         // Bank branch code
    RoutingNumber,      // Bank routing number
    DisplayName         // User-friendly display name
}
```

#### PokerWalletMetadata

```csharp
public enum PokerWalletMetadata
{
    SiteName,          // PokerStars, GGPoker, etc.
    PlayerId,          // Site-specific player ID
    PlayerEmail,       // Player email/username
    VIPLevel,          // Bronze, Silver, Gold, etc.
    AccountStatus,     // Verified, Pending, etc.
    DisplayName        // User-friendly display name
}
```

#### CryptoWalletMetadata

```csharp
public enum CryptoWalletMetadata
{
    WalletAddress,     // Blockchain wallet address
    ExchangeName,      // Exchange name (if applicable)
    WalletCategory,    // Hot, Cold, Exchange, etc.
    NetworkType,       // Mainnet, Testnet, etc.
    ApiKey,            // Exchange API key (encrypted)
    ApiSecret,         // Exchange API secret (encrypted)
    DisplayName        // User-friendly display name
}
```

## 4. Redundancy Elimination

### Removed Redundancies:

1. **Currency**: Already defined in AssetPool.AssetType
2. **AccountHolder**: Already defined in AssetPool.BaseAssetHolder
3. **Individual Properties**: Replaced with flexible metadata system

### Clean Separation:

- **AssetPool**: Defines WHAT asset and WHO owns it
- **WalletIdentifier**: Defines HOW to access/identify the asset
- **Metadata**: Defines wallet-specific details

## 5. Database Schema

### Storage Strategy:

- **MetadataJson**: `nvarchar(max)` column storing JSON
- **Metadata**: `[NotMapped]` Dictionary for code usage
- **Automatic Conversion**: Between JSON and Dictionary

### Example Data:

```json
// Bank Wallet Metadata
{
  "BankName": "Banco do Brasil",
  "AccountNumber": "12345-6",
  "AccountType": "Checking",
  "BranchCode": "1234",
  "RoutingNumber": "001",
  "DisplayName": "Banco do Brasil - Conta Corrente"
}

// Poker Wallet Metadata
{
  "SiteName": "PokerStars",
  "PlayerId": "PS123456",
  "PlayerEmail": "player@email.com",
  "VIPLevel": "Gold",
  "AccountStatus": "Verified",
  "DisplayName": "PokerStars - Gold VIP"
}

// Crypto Wallet Metadata
{
  "WalletAddress": "bc1qxy2kgdygjrsqtzq2n0yrf2493p83kkfjhx0wlh",
  "ExchangeName": "Binance",
  "WalletCategory": "Hot",
  "NetworkType": "Mainnet",
  "DisplayName": "Bitcoin Hot Wallet"
}
```

## 6. Validation System

### Built-in Validation:

- **Type-specific**: Each WalletType has required fields
- **Compile-time Safety**: Enum-based metadata fields
- **Runtime Validation**: Service-level validation before CRUD operations

### Validation Service:

```csharp
public class WalletIdentifierValidationService
{
    public ValidationResult ValidateWalletIdentifier(WalletIdentifier wallet)
    {
        // Validates required fields based on WalletType
        // Returns detailed error messages
    }
}
```

## 7. Service Layer Enhancements

### WalletIdentifierService:

- **Validation**: Automatic validation on Add/Update
- **Metadata Search**: Search by metadata fields
- **Type-safe Accessors**: GetBankMetadata(), GetPokerMetadata(), etc.
- **Enhanced Queries**: GetByWalletType(), GetByAssetType()

### AssetPoolService:

- **Improved Validation**: Prevents duplicate AssetType per BaseAssetHolder
- **Balance Calculation**: Aggregates across all WalletIdentifiers
- **Relationship Management**: Proper Include statements

## 8. Transaction Integration

### Unchanged Transaction Models:

- **BaseTransaction**: Still uses SenderWalletIdentifierId/ReceiverWalletIdentifierId
- **All Transaction Types**: Work seamlessly with new structure
- **Existing Services**: Minimal impact on transaction processing

### Benefits:

- **Unified Transactions**: Single model works across all asset types
- **Flexible Routing**: Metadata enables complex routing scenarios
- **Audit Trail**: Complete transaction history maintained

## 9. Key Benefits

### 1. **Flexibility**

- Add new asset types without schema changes
- Extensible metadata system
- Support for complex wallet configurations

### 2. **Data Integrity**

- Strong validation rules
- Type-safe metadata access
- Prevents data inconsistencies

### 3. **Performance**

- JSON storage for metadata
- Efficient querying with indexes
- Minimal database overhead

### 4. **Maintainability**

- Clean separation of concerns
- Enum-based field definitions
- Comprehensive validation

### 5. **Scalability**

- Supports unlimited asset types
- Flexible metadata structure
- Future-proof design

## 10. Migration Strategy

### Database Changes:

1. Add `MetadataJson` column to WalletIdentifiers
2. Add `IsActive` column to WalletIdentifiers
3. Remove `RouteInfo` and `IdentifierInfo` columns
4. Update indexes for performance

### Code Changes:

1. Update existing WalletIdentifier usage
2. Migrate data to metadata format
3. Update validation logic
4. Test all transaction flows

## 11. Usage Examples

### Creating Wallets:

```csharp
// Bank Wallet
var bankWallet = new WalletIdentifier
{
    AssetPoolId = AssetPoolId,
    WalletType = WalletType.BankWallet,
    InputForTransactions = "001-1234-12345-6",
    Metadata = new Dictionary<string, string>
    {
        [BankWalletMetadata.BankName.ToString()] = "Banco do Brasil",
        [BankWalletMetadata.AccountNumber.ToString()] = "12345-6",
        [BankWalletMetadata.AccountType.ToString()] = "Checking",
        [BankWalletMetadata.BranchCode.ToString()] = "1234",
        [BankWalletMetadata.RoutingNumber.ToString()] = "001",
        [BankWalletMetadata.DisplayName.ToString()] = "Banco do Brasil - Conta Corrente"
    }
};

// Poker Wallet
var pokerWallet = new WalletIdentifier
{
    AssetPoolId = AssetPoolId,
    WalletType = WalletType.PokerWallet,
    InputForTransactions = "player@pokerstars.com",
    Metadata = new Dictionary<string, string>
    {
        [PokerWalletMetadata.SiteName.ToString()] = "PokerStars",
        [PokerWalletMetadata.PlayerId.ToString()] = "PS123456",
        [PokerWalletMetadata.PlayerEmail.ToString()] = "player@pokerstars.com",
        [PokerWalletMetadata.VIPLevel.ToString()] = "Gold",
        [PokerWalletMetadata.AccountStatus.ToString()] = "Verified",
        [PokerWalletMetadata.DisplayName.ToString()] = "PokerStars - Gold VIP"
    }
};
```

### Querying:

```csharp
// Type-safe metadata access
var bankName = wallet.GetBankMetadata(BankWalletMetadata.BankName);
var siteName = wallet.GetPokerMetadata(PokerWalletMetadata.SiteName);

// Search by metadata
var bbWallets = await service.GetBankWalletsByBankName("Banco do Brasil");
var psWallets = await service.GetPokerWalletsBySite("PokerStars");
```

## 12. Committee Decision Points

### ✅ **Approved Design Elements:**

1. **Metadata-based approach** for flexibility
2. **Three-tier WalletType** structure
3. **Elimination of redundant fields**
4. **Strong validation system**
5. **Backward compatibility** with transactions

### 🔍 **Review Required:**

1. **Migration timeline** and data conversion
2. **Performance impact** of JSON storage
3. **Additional validation rules** if needed
4. **Testing strategy** for all asset types

### 📋 **Next Steps:**

1. **Committee approval** of this design
2. **Migration script** development
3. **Comprehensive testing** plan
4. **Production deployment** strategy

---

**This design provides a robust, flexible, and maintainable foundation for multi-asset transaction management while eliminating redundancies and ensuring data integrity.**
