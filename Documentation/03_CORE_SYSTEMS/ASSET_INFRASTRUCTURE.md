# Asset Infrastructure

## Table of Contents

- [Overview](#overview)
- [Core Domain Models](#core-domain-models)
  - [AssetPool](#assetpool)
  - [WalletIdentifier](#walletidentifier)
- [Internal Wallets (Creation vs Participation)](#internal-wallets-creation-vs-participation)
- [Metadata System](#metadata-system)
  - [Bank Wallet Metadata](#bank-wallet-metadata-fiat-assets)
  - [Poker Wallet Metadata](#poker-wallet-metadata)
  - [Crypto Wallet Metadata](#crypto-wallet-metadata)
- [Type-Safe Metadata Access](#type-safe-metadata-access)
- [Entity Relationships Diagram](#entity-relationships-diagram)
- [Usage Examples](#usage-examples)
- [Related Documentation](#related-documentation)

---

## Overview

This document describes the asset infrastructure system for **SF Management**, a multi-asset financial management platform designed to handle fiat currencies, poker platform balances, and cryptocurrency assets with unified transaction handling.

The system is built around four core concepts:
1. **Asset Holders** - Entities that own assets (Clients, Banks, Members, Poker Managers)
2. **Asset Pools** - Containers that group wallets by asset category for a holder
3. **Wallet Identifiers** - Specific accounts/wallets where assets are held
4. **Transactions** - Movement of assets between wallet identifiers

> **Note:** For detailed information about Asset Holders (BaseAssetHolder, Client, Bank, Member, PokerManager), see [ENTITY_INFRASTRUCTURE.md](./ENTITY_INFRASTRUCTURE.md).

---

## Core Domain Models

### AssetPool

An `AssetPool` groups wallet identifiers by asset category for a specific asset holder. It acts as a container that organizes wallets of similar types.

```csharp
public class AssetPool : BaseDomain
{
    // Nullable: null means company-owned pool
    public Guid? BaseAssetHolderId { get; set; }
    public virtual BaseAssetHolder? BaseAssetHolder { get; set; }
    
    public AssetGroup AssetGroup { get; set; }
    
    public virtual ICollection<WalletIdentifier> WalletIdentifiers { get; set; }
}
```

**Key Features:**
- Links to a `BaseAssetHolder` (nullable for company-owned pools)
- Uses `AssetGroup` to categorize wallet types
- One `AssetPool` per `AssetGroup` per `BaseAssetHolder`
- Container for multiple `WalletIdentifiers`

### WalletIdentifier

A `WalletIdentifier` represents a specific account or wallet where assets are held. It contains the details needed to identify and transact with the account.

```csharp
public class WalletIdentifier : BaseDomain
{
    [Required] public Guid AssetPoolId { get; set; }
    public virtual AssetPool AssetPool { get; set; }
    
    [Required] public AccountClassification AccountClassification { get; set; }
    [Required] public AssetType AssetType { get; set; }
    
    // Metadata stored as JSON
    [Column(TypeName = "nvarchar(2000)")]
    public string MetadataJson { get; set; } = "{}";
    
    [NotMapped]
    public Dictionary<string, string> Metadata { get; set; }
    
    // Referrals for this wallet
    public virtual ICollection<Referral> Referrals { get; set; }
    
    // Computed property based on AssetType
    [NotMapped]
    public AssetGroup AssetGroup { get; }
}
```

**Key Features:**
- Belongs to exactly one `AssetPool`
- Has an `AccountClassification` for accounting purposes (ASSET, LIABILITY, EQUITY, REVENUE, EXPENSE)
- Has a specific `AssetType` (e.g., BrazilianReal, PokerStars, Bitcoin)
- Flexible metadata system stored as JSON
- Type-safe metadata accessors for each wallet type

> **Note:** For enum definitions (`AssetGroup`, `AssetType`, `AccountClassification`), see [ENUMS_AND_TYPE_SYSTEM.md](../07_REFERENCE/ENUMS_AND_TYPE_SYSTEM.md).

---

## Internal Wallets (Creation vs Participation)

`AssetGroup.Internal` wallets are flexible wallets that bypass metadata validation. They are created explicitly and **participate** in transactions, but transaction modes never create them automatically.

### Creation Use Cases

| Use Case | Owner | Purpose | Creation Path |
|----------|-------|---------|---------------|
| **System Wallets** | Company (`BaseAssetHolderId = null`) | Financial operations and categorization | `POST /api/v1/WalletIdentifier/internal-wallet` |
| **Conversion Wallets** | PokerManager | Self-conversion trigger (dual-balance impact) | `POST /api/v1/WalletIdentifier/internal-wallet` |

### Participation vs Creation

Transaction modes can **use** Internal wallets but do **not** create them:

- TRANSFER and INTERNAL modes may select existing Internal wallets
- Auto-created wallets use the **natural** `AssetGroup` based on `AssetType` mapping

### System Wallet Pairing

System operations resolve the company-owned Internal wallet using:

```
GET /api/v1/company/asset-pools/system-wallet-to-pair-with/{walletIdentifierId}
```

This endpoint only returns **company-owned** Internal wallets (`BaseAssetHolderId = null`).

---

## Metadata System

Each wallet type has specific metadata fields defined by enums. Metadata is stored as JSON in the database and accessed via type-safe methods.

### Bank Wallet Metadata (Fiat Assets)

```csharp
public enum BankWalletMetadata
{
    BankName,        // Bank institution name
    AccountNumber,   // Account number
    AccountType,     // Checking, Savings, etc.
    BranchCode,      // Bank branch code
    RoutingNumber,   // Bank routing number
    PixKey           // PIX key (Brazilian instant payment)
}
```

**Required Fields:** `BankName`, `PixKey`

### Poker Wallet Metadata

```csharp
public enum PokerWalletMetadata
{
    PlayerNickname,       // Site-specific player nickname
    PlayerEmail,          // Player email
    PlayerPhone,          // Player phone
    AccountStatus,        // Account status (Verified, Pending, etc.)
    InputForTransactions  // Identifier used for transactions
}
```

**Required Fields:** `InputForTransactions`

### Crypto Wallet Metadata

```csharp
public enum CryptoWalletMetadata
{
    WalletAddress,   // Blockchain wallet address
    ExchangeName,    // Exchange name (if applicable)
    WalletCategory,  // Hot, Cold, Exchange, etc.
    NetworkType,     // Mainnet, Testnet, etc.
    ApiKey,          // Exchange API key (encrypted)
    ApiSecret,       // Exchange API secret (encrypted)
    DisplayName      // User-friendly display name
}
```

**Required Fields:** `WalletAddress`, `WalletCategory`

### Example Metadata JSON

```json
// Bank Wallet
{
  "BankName": "Banco do Brasil",
  "PixKey": "email@example.com",
  "AccountType": "Checking",
  "AccountNumber": "12345-6",
  "RoutingNumber": "001"
}

// Poker Wallet
{
  "InputForTransactions": "player@pokerstars.com",
  "PlayerNickname": "PlayerOne",
  "PlayerEmail": "player@email.com",
  "AccountStatus": "Verified"
}

// Crypto Wallet
{
  "WalletAddress": "bc1qxy2kgdygjrsqtzq2n0yrf2493p83kkfjhx0wlh",
  "WalletCategory": "Hot",
  "ExchangeName": "Binance",
  "NetworkType": "Mainnet"
}
```

---

## Type-Safe Metadata Access

`WalletIdentifier` provides type-safe methods for metadata access:

```csharp
// Reading metadata
var bankName = wallet.GetBankMetadata(BankWalletMetadata.BankName);
var playerNick = wallet.GetPokerMetadata(PokerWalletMetadata.PlayerNickname);
var address = wallet.GetCryptoMetadata(CryptoWalletMetadata.WalletAddress);

// Writing metadata
wallet.SetBankMetadata(BankWalletMetadata.PixKey, "new@pix.key");
wallet.SetPokerMetadata(PokerWalletMetadata.AccountStatus, "Verified");
wallet.SetCryptoMetadata(CryptoWalletMetadata.WalletCategory, "Cold");

// Bulk set from individual fields
wallet.SetMetadataFromFields(
    inputForTransactions: "player@site.com",
    playerNickname: "PlayerOne",
    playerEmail: "email@example.com"
);
```

---

## Entity Relationships Diagram

```
┌──────────────────────────────────────────────────────────────────┐
│                        BaseAssetHolder                           │
│  ┌─────────┬─────────┬─────────┬──────────────┐                  │
│  │ Client  │  Bank   │ Member  │ PokerManager │  (1:1 exclusive) │
│  └─────────┴─────────┴─────────┴──────────────┘                  │
└────────────────────────────┬─────────────────────────────────────┘
                             │
                             │ 1:N
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│                          AssetPool                               │
│                     (grouped by AssetGroup)                      │
└────────────────────────────┬─────────────────────────────────────┘
                             │
                             │ 1:N
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│                      WalletIdentifier                            │
│           (specific accounts with metadata)                      │
└───────┬──────────────────────────────────────────────┬───────────┘
        │                                              │
        │ N:1 (Sender/Receiver)                        │ N:M
        ▼                                              ▼
┌───────────────────┐                       ┌──────────────────────┐
│   Transactions    │                       │      Referrals       │
│  - Fiat           │                       │  (commission links)  │
│  - Digital        │                       └──────────────────────┘
│  - Settlement     │
│  - Imported       │
└───────────────────┘
```

---

## Usage Examples

### Creating a Complete Asset Holder Structure

```csharp
// 1. Create the BaseAssetHolder
var baseAssetHolder = new BaseAssetHolder
{
    Name = "João Silva",
    TaxEntityType = TaxEntityType.CPF,
    GovernmentNumber = "123.456.789-00"
};

// 2. Create the specific entity type
var client = new Client
{
    BaseAssetHolderId = baseAssetHolder.Id,
    Birthday = new DateTime(1990, 5, 15)
};
baseAssetHolder.Client = client;

// 3. Create an AssetPool for fiat assets
var fiatPool = new AssetPool
{
    BaseAssetHolderId = baseAssetHolder.Id,
    AssetGroup = AssetGroup.FiatAssets
};

// 4. Create a bank wallet identifier
var bankWallet = new WalletIdentifier
{
    AssetPoolId = fiatPool.Id,
    AssetType = AssetType.BrazilianReal,
    AccountClassification = AccountClassification.ASSET
};
bankWallet.SetMetadataFromFields(
    bankName: "Banco do Brasil",
    pixKey: "joao@email.com",
    accountType: "Checking",
    accountNumber: "12345-6"
);
```

### Creating a Poker Wallet

```csharp
// Create a poker asset pool
var pokerPool = new AssetPool
{
    BaseAssetHolderId = baseAssetHolder.Id,
    AssetGroup = AssetGroup.PokerAssets
};

// Create a PokerStars wallet
var pokerWallet = new WalletIdentifier
{
    AssetPoolId = pokerPool.Id,
    AssetType = AssetType.PokerStars,
    AccountClassification = AccountClassification.ASSET
};
pokerWallet.SetMetadataFromFields(
    inputForTransactions: "player123@pokerstars.com",
    playerNickname: "ProPlayer123",
    playerEmail: "joao@email.com",
    accountStatus: "Verified"
);
```

### Creating a Transaction

```csharp
// Digital asset transaction (poker transfer)
var transaction = new DigitalAssetTransaction
{
    Date = DateTime.UtcNow,
    SenderWalletIdentifierId = senderWallet.Id,
    ReceiverWalletIdentifierId = receiverWallet.Id,
    AssetAmount = 1000.00m,
    ConversionRate = 1.0m
};

// Settlement transaction with rake
var settlement = new SettlementTransaction
{
    Date = DateTime.UtcNow,
    SenderWalletIdentifierId = managerWallet.Id,
    ReceiverWalletIdentifierId = clientWallet.Id,
    AssetAmount = 5000.00m,
    RakeAmount = 250.00m,
    RakeCommission = 50.00m,
    RakeBack = 25.00m
};
```

---

## Summary

The SF Management asset infrastructure provides:

1. **Flexible Asset Organization** - `AssetPool` groups wallets by `AssetGroup` (Fiat, Poker, Crypto, Internal, Settlements)
2. **Extensible Wallet System** - `WalletIdentifier` with JSON-based metadata for wallet-specific details
3. **Type-Safe Metadata** - Enum-based metadata keys with type-safe accessors
4. **Company-Owned Pools** - Support for company assets via nullable `BaseAssetHolderId`

This architecture supports:
- Multiple asset types with a single, consistent model
- Complex ownership and referral relationships
- Detailed accounting with `AccountClassification`
- Extensibility for new asset types without schema changes
- Comprehensive audit trail via `BaseDomain` timestamps

---

## Related Documentation

For detailed information on related topics, refer to:

| Topic | Document |
|-------|----------|
| Entity Infrastructure | [ENTITY_INFRASTRUCTURE.md](./ENTITY_INFRASTRUCTURE.md) |
| Transaction System | [TRANSACTION_INFRASTRUCTURE.md](./TRANSACTION_INFRASTRUCTURE.md) |
| Referral System | [REFERRAL_SYSTEM.md](./REFERRAL_SYSTEM.md) |
| Enum Definitions | [ENUMS_AND_TYPE_SYSTEM.md](../07_REFERENCE/ENUMS_AND_TYPE_SYSTEM.md) |
| Validation System | [VALIDATION_SYSTEM.md](../05_INFRASTRUCTURE/VALIDATION_SYSTEM.md) |
| Soft Delete | [SOFT_DELETE_AND_DATA_LIFECYCLE.md](../05_INFRASTRUCTURE/SOFT_DELETE_AND_DATA_LIFECYCLE.md) |
| Database Schema | [DATABASE_SCHEMA.md](../02_ARCHITECTURE/DATABASE_SCHEMA.md) |
| API Endpoints | [API_REFERENCE.md](../06_API/API_REFERENCE.md) |
