# Enums and Type System

## Overview

This document provides a comprehensive reference for all enumerations used in the SF Management system. These enums define the type system for assets, entities, transactions, and various metadata throughout the application.

Understanding these enums is essential for working with the API, interpreting data, and implementing business logic correctly.

---

## Table of Contents

1. [Asset Infrastructure](#asset-infrastructure)
   - [AssetType](#assettype)
   - [AssetGroup](#assetgroup)
   - [AccountClassification](#accountclassification)
2. [Entity Types](#entity-types)
   - [AssetHolderType](#assetholdertype)
   - [TaxEntityType](#taxentitytype)
3. [Imported Files](#imported-files)
   - [ImportFileType](#importfiletype)
   - [ExcelImportType](#excelimporttype)
   - [ImportedTransactionStatus](#importedtransactionstatus)
   - [ReconciledTransactionType](#reconciledtransactiontype)
4. [Wallet Metadata](#wallet-metadata)
   - [BankWalletMetadata](#bankwalletmetadata)
   - [PokerWalletMetadata](#pokerwalletmetadata)
   - [CryptoWalletMetadata](#cryptowalletmetadata)
5. [Business Enums](#business-enums)
   - [ManagerProfitType](#managerprofittype)
6. [Relationships Between Enums](#relationships-between-enums)

---

## Asset Infrastructure

### AssetType

Defines specific types of assets that can be held in the system. Each asset type belongs to an `AssetGroup`.

**File**: `Enums/AssetInfrastructure/AssetType.cs`

```csharp
public enum AssetType
{
    // Miscellaneous
    None = 0,

    // Fiat (AssetGroup.FiatAssets)
    BrazilianReal = 21,
    USDollar = 22,

    // Poker in USDollar (AssetGroup.PokerAssets)
    PokerStars = 101,
    GgPoker = 102,
    YaPoker = 103,
    AmericasCardRoom = 104,
    SupremaPoker = 105,
    AstroPayICash = 106,
    LuxonPoker = 107,

    // Crypto (AssetGroup.CryptoAssets)
    Bitcoin = 201,
    Ethereum = 202,
    Litecoin = 203,
    Ripple = 204,
    BitcoinCash = 205,
    Stellar = 206,
}
```

#### Value Ranges

The numeric values follow a logical grouping:

| Range | Category | Asset Group |
|-------|----------|-------------|
| 0 | Miscellaneous | None |
| 21-99 | Fiat Currencies | FiatAssets |
| 100-199 | Poker Platforms | PokerAssets |
| 200-299 | Cryptocurrencies | CryptoAssets |

#### Usage

AssetType is primarily used in:
- `WalletIdentifier.AssetType` - Identifies what type of asset a wallet holds
- `InitialBalance.AssetType` - Associates initial balances with specific asset types
- API filters for querying wallets and transactions

---

### AssetGroup

Defines broad categories of assets. Used for grouping wallet identifiers within asset pools.

**File**: `Enums/AssetInfrastructure/AssetGroup.cs`

```csharp
public enum AssetGroup
{
    None = 0,
    FiatAssets = 1,
    PokerAssets = 2,
    CryptoAssets = 3,
    Internal = 4,
    Settlements = 5,
}
```

| Value | Name | Description |
|-------|------|-------------|
| 0 | None | Unclassified or default |
| 1 | FiatAssets | Traditional currencies (BRL, USD) |
| 2 | PokerAssets | Poker platform chips and credits |
| 3 | CryptoAssets | Cryptocurrency holdings |
| 4 | Internal | Internal accounting entries |
| 5 | Settlements | Settlement transaction records |

#### Usage

AssetGroup is used in:
- `AssetPool.AssetGroup` - Categorizes asset pools
- Company asset pool management (route parameter)
- Balance calculations and reporting
- API filters

#### Special Groups

**Internal (`AssetGroup.Internal`)**: Used for internal accounting entries and company-level wallets. Wallet identifiers with this group skip metadata validation.

**Settlements (`AssetGroup.Settlements`)**: Specifically for settlement transaction tracking. Like Internal, these skip metadata validation requirements.

**Naming Clarification:**
- `AssetGroup.Internal` (enum) is a wallet category
- `INTERNAL` (transaction mode) is a same-holder transfer mode
- `IsInternalTransfer` (transaction property) checks if both wallets share the same owner

These are distinct concepts that happen to share similar naming.

---

### AccountClassification

Defines the accounting classification for wallet identifiers, following standard double-entry bookkeeping principles.

**File**: `Enums/AssetInfrastructure/AccountClassification.cs`

```csharp
public enum AccountClassification
{
    ASSET = 1,
    LIABILITY = 2,
    EQUITY = 3,
    REVENUE = 4,
    EXPENSE = 5
}
```

| Value | Name | Description |
|-------|------|-------------|
| 1 | ASSET | Resources owned by the company |
| 2 | LIABILITY | Obligations owed to others |
| 3 | EQUITY | Owner's stake in the business |
| 4 | REVENUE | Income from operations |
| 5 | EXPENSE | Costs of operations |

#### Business Usage by Entity Type

| Entity | Classification | Balance Meaning |
|--------|---------------|-----------------|
| **Bank** | ASSET | Positive = company HAS money |
| **PokerManager** (PokerAssets) | ASSET | Positive = company HAS chips |
| **PokerManager** (FiatAssets) | LIABILITY | Positive = company OWES PM |
| **Client** | LIABILITY | Positive = company OWES client |
| **Member** | LIABILITY | Positive = company OWES member |

#### Transaction Sign Behavior

When both wallets have the **same** classification: Standard sender(-)/receiver(+) signs apply.

When wallets have **different** classifications: LIABILITY wallet gets sign inverted.

#### Usage

This classification is crucial for:
- Proper accounting of transactions
- Balance calculations (assets increase with debits, liabilities with credits)
- Financial reporting
- The `HaveBothWalletsSameAccountClassification()` helper method in `BaseTransaction`

---

## Entity Types

### AssetHolderType

Identifies the type of entity that can hold assets in the system.

**File**: `Enums/AssetHolderType.cs`

```csharp
public enum AssetHolderType
{
    Unknown = 0,
    Client = 1,
    Bank = 2,
    Member = 3,
    PokerManager = 4
}
```

| Value | Name | Description |
|-------|------|-------------|
| 0 | Unknown | Unclassified holder |
| 1 | Client | Poker players/customers |
| 2 | Bank | Financial institutions |
| 3 | Member | Business partners/stakeholders |
| 4 | PokerManager | Poker operations managers |

#### Usage

- `BaseAssetHolder.AssetHolderType` property
- Filtering and querying asset holders
- Determining which controller/service to use
- API responses to identify entity types

---

### TaxEntityType

Defines the Brazilian tax entity classification for asset holders.

**File**: `Enums/TaxEntityType.cs`

```csharp
public enum TaxEntityType
{
    CPF = 1,
    CNPJ = 2,
    CNPJ_Not_Taxable = 3
}
```

| Value | Name | Description |
|-------|------|-------------|
| 1 | CPF | Individual (Cadastro de Pessoa Física) |
| 2 | CNPJ | Business (Cadastro Nacional de Pessoa Jurídica) |
| 3 | CNPJ_Not_Taxable | Non-taxable business entity |

#### Usage

- `BaseAssetHolder.TaxEntityType` property
- Tax reporting and compliance
- Document validation (CPF has 11 digits, CNPJ has 14 digits)

---

## Imported Files

### ImportFileType

Defines the types of files that can be imported to create transactions.

**File**: `Enums/ImportedFiles/ImportFileType.cs`

```csharp
public enum ImportFileType
{
    [Display(Name = "OFX File", Description = "Open Financial Exchange file format")]
    Ofx = 1,
    
    [Display(Name = "Excel File", Description = "Microsoft Excel spreadsheet")]
    Excel = 2,
    
    [Display(Name = "CSV File", Description = "Comma-separated values file")]
    Csv = 3,
    
    [Display(Name = "Bank Statement", Description = "Bank statement in various formats")]
    BankStatement = 4,
    
    [Display(Name = "Poker Transaction Export", Description = "Poker platform transaction export")]
    PokerExport = 5,
    
    [Display(Name = "Crypto Exchange Export", Description = "Cryptocurrency exchange transaction export")]
    CryptoExport = 6,
    
    [Display(Name = "Manual Entry", Description = "Manually entered transaction data")]
    Manual = 7,
    
    [Display(Name = "API Import", Description = "Transaction imported via API")]
    Api = 8
}
```

#### Currently Supported

| Value | Name | Status |
|-------|------|--------|
| 1 | Ofx | Fully supported via `/import/ofx` |
| 2 | Excel | Fully supported via `/import/excel/*` |
| 3-8 | Others | Reserved for future implementation |

---

### ExcelImportType

Defines the types of Excel imports supported for poker transactions.

**File**: `Enums/ImportedFiles/ExcelImportType.cs`

```csharp
public enum ExcelImportType
{
    [Display(Name = "Buy Transactions")]
    BuyTransactions = 1,
    
    [Display(Name = "Sell Transactions")]
    SellTransactions = 2,
    
    [Display(Name = "Transfer Transactions")]
    TransferTransactions = 3
}
```

| Value | Name | Description | Default Column Mapping |
|-------|------|-------------|----------------------|
| 1 | BuyTransactions | Purchase transactions from poker platforms | WalletIdentifier, Value, AssetPool, CreatedAt, Description |
| 2 | SellTransactions | Sale transactions from poker platforms | WalletIdentifier, Value, AssetPool, CreatedAt, Description |
| 3 | TransferTransactions | Transfer transactions between accounts | From, To, CreatedAt, Value, Description |

---

### ImportedTransactionStatus

Tracks the lifecycle status of an imported transaction.

**File**: `Enums/ImportedFiles/ImportedTransactionStatus.cs`

```csharp
public enum ImportedTransactionStatus
{
    Pending = 1,
    Processing = 2,
    Processed = 3,
    Reconciled = 4,
    Failed = 5,
    Duplicate = 6,
    Ignored = 7,
    RequiresReview = 8
}
```

#### Status Flow

```
Pending (1) → Processing (2) → Processed (3) → Reconciled (4)
                                    ↓
                              RequiresReview (8)
                                    ↓
                               Ignored (7)
```

| Value | Name | Description |
|-------|------|-------------|
| 1 | Pending | Imported but not yet processed |
| 2 | Processing | Currently being processed |
| 3 | Processed | Successfully processed, awaiting reconciliation |
| 4 | Reconciled | Matched with a BaseTransaction |
| 5 | Failed | Processing failed |
| 6 | Duplicate | Identified as duplicate (filtered during import) |
| 7 | Ignored | Manually ignored by user |
| 8 | RequiresReview | Requires manual review |

---

### ReconciledTransactionType

Identifies the type of base transaction that an imported transaction was reconciled with.

**File**: `Enums/ImportedFiles/ReconciledTransactionType.cs`

```csharp
public enum ReconciledTransactionType
{
    Fiat = 1,
    Digital = 2,
    Settlement = 3
}
```

| Value | Name | Reconciles With |
|-------|------|-----------------|
| 1 | Fiat | `FiatAssetTransaction` |
| 2 | Digital | `DigitalAssetTransaction` |
| 3 | Settlement | `SettlementTransaction` |

---

## Wallet Metadata

These enums define the metadata fields available for different types of wallets. Metadata is stored as JSON in the `WalletIdentifier.Metadata` field.

### BankWalletMetadata

Metadata fields for bank/fiat wallets.

**File**: `Enums/WalletsMetadata/BankWalletMetadata.cs`

```csharp
public enum BankWalletMetadata
{
    BankName,         // Bank institution name
    AccountNumber,    // Account number
    AccountType,      // Checking, Savings, etc.
    BranchCode,       // Bank branch code
    RoutingNumber,    // Bank routing number
    PixKey,           // Pix key
}
```

#### Required Fields for FiatAssets

- `BankName`
- `AccountNumber`
- `PixKey` (for Brazilian banks)

---

### PokerWalletMetadata

Metadata fields for poker platform wallets.

**File**: `Enums/WalletsMetadata/PokerWalletMetadata.cs`

```csharp
public enum PokerWalletMetadata
{
    PlayerNickname,       // Site-specific player nickname
    PlayerEmail,          // Player email
    PlayerPhone,          // Player phone
    AccountStatus,        // Account status (Verified, Pending, etc.)
    InputForTransactions  // How to fill the input for transactions
}
```

#### Required Fields for PokerAssets

- `PlayerNickname`
- `PlayerPhone`
- `InputForTransactions`

---

### CryptoWalletMetadata

Metadata fields for cryptocurrency wallets.

**File**: `Enums/WalletsMetadata/CryptoWalletMetadata.cs`

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

#### Required Fields for CryptoAssets

- `WalletAddress`

---

## Business Enums

### ManagerProfitType

Defines how the **company** profits from a poker manager's operations.

> **Important:** The PokerManager holds assets on behalf of the company. The profit goes to the company, not the manager personally.

**File**: `Enums/ManagerProfitType.cs`

```csharp
public enum ManagerProfitType
{
    Spread = 0,
    RakeOverrideCommission = 1
}
```

| Value | Name | Description | How Company Profits |
|-------|------|-------------|---------------------|
| 0 | Spread | Price spread between buy/sell rates | Different `ConversionRate` on buy vs sell transactions |
| 1 | RakeOverrideCommission | Commission from poker site | `RakeCommission` % in SettlementTransaction |

#### Spread Example

```
Client buys chips: ConversionRate = 5.10 (client pays more)
Client sells chips: ConversionRate = 4.90 (client receives less)
Company profit: 0.20 per chip traded
```

#### RakeOverrideCommission Example

```
SettlementTransaction:
- RakeAmount: 1000 (chips client paid to poker site)
- RakeCommission: 50% (poker site pays company)
- RakeBack: 10% (company returns to client)

Company profit: 1000 × ((50 - 10) / 100) = 400 chips
```

> **Note:** Implementation of company profit tracking is planned for the Finance Module (TBD).

---

## Relationships Between Enums

### AssetType to AssetGroup Mapping

```
AssetType                    → AssetGroup
────────────────────────────────────────────
BrazilianReal (21)          → FiatAssets (1)
USDollar (22)               → FiatAssets (1)
PokerStars (101)            → PokerAssets (2)
GgPoker (102)               → PokerAssets (2)
... other poker sites       → PokerAssets (2)
Bitcoin (201)               → CryptoAssets (3)
Ethereum (202)              → CryptoAssets (3)
... other crypto            → CryptoAssets (3)
```

### AssetGroup to WalletMetadata Mapping

```
AssetGroup      → Required Metadata Enum
───────────────────────────────────────────
FiatAssets      → BankWalletMetadata
PokerAssets     → PokerWalletMetadata
CryptoAssets    → CryptoWalletMetadata
Internal        → No metadata required
Settlements     → No metadata required
```

### AssetHolderType to Entity Mapping

```
AssetHolderType  → Entity Class        → Controller
───────────────────────────────────────────────────────
Client (1)       → Client              → ClientController
Bank (2)         → Bank                → BankController
Member (3)       → Member              → MemberController
PokerManager (4) → PokerManager        → PokerManagerController
```

---

## Best Practices

### 1. Use Enum Values, Not Magic Numbers

```csharp
// Good
var assetType = AssetType.PokerStars;

// Bad
var assetType = 101;
```

### 2. Check for Valid Enum Values

```csharp
if (!Enum.IsDefined(typeof(AssetType), request.AssetType))
{
    throw new ValidationException("Invalid AssetType");
}
```

### 3. Use Display Attributes for UI

```csharp
var displayName = importStatus
    .GetType()
    .GetField(importStatus.ToString())
    ?.GetCustomAttribute<DisplayAttribute>()
    ?.Name ?? importStatus.ToString();
```

### 4. Default Values

Always handle the `None` or `Unknown` values appropriately:

```csharp
if (assetType == AssetType.None)
{
    throw new BusinessException("AssetType must be specified");
}
```

---

## Related Documentation

- [ASSET_INFRASTRUCTURE.md](../03_CORE_SYSTEMS/ASSET_INFRASTRUCTURE.md) - How these enums are used in the asset system
- [VALIDATION_SYSTEM.md](../05_INFRASTRUCTURE/VALIDATION_SYSTEM.md) - Validation rules for enum values
- [API_REFERENCE.md](../06_API/API_REFERENCE.md) - API usage of enums

