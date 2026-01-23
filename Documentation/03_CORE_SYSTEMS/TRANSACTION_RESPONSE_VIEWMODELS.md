# Transaction Response DTOs

## Table of Contents

- [Overview](#overview)
- [Response Model Hierarchy](#response-model-hierarchy)
- [Base Transaction Response](#base-transaction-response)
- [Supporting Models](#supporting-models)
  - [WalletIdentifierSummary](#walletidentifiersummary)
  - [AssetHolderSummary](#assetholdersummary)
- [Fiat Asset Transaction Response](#fiat-asset-transaction-response)
- [Digital Asset Transaction Response](#digital-asset-transaction-response)
- [Settlement Transaction Response](#settlement-transaction-response)
- [AutoMapper Overview](#automapper-overview)
- [API Response Examples](#api-response-examples)
- [Usage Examples](#usage-examples)
- [Design Benefits](#design-benefits)
- [Related Documentation](#related-documentation)

---

## Overview

This document describes the transaction response DTOs (Data Transfer Objects) used in the SF Management API. These models provide rich, structured information about transactions for API consumers, including wallet details, asset holder information, and context-specific metadata.

**File Location:** `Application/DTOs/Transactions/`

The API uses a hierarchy of response models that extend a common base, with each transaction type adding specialized information relevant to its domain (fiat banking, digital assets, or settlements).

---

## Response Model Hierarchy

```
BaseTransactionResponse
    ├── FiatAssetTransactionResponse    (bank/PIX transactions)
    ├── DigitalAssetTransactionResponse (poker/crypto transactions)
    └── SettlementTransactionResponse   (poker settlements)
```

---

## Base Transaction Response

The `BaseTransactionResponse` provides the common structure for all transaction types.

### Definition

```csharp
public class BaseTransactionResponse : BaseResponse
{
    // Core transaction data
    public DateTime Date { get; set; }
    public decimal AssetAmount { get; set; }
    public string? Description { get; set; }
    
    // Approval information
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    
    // Wallet information
    public WalletIdentifierSummary SenderWallet { get; set; } = new();
    public WalletIdentifierSummary ReceiverWallet { get; set; } = new();
    
    // Transaction metadata
    public CategoryResponse? Category { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public bool IsInternalTransfer { get; set; }
    public AssetType AssetType { get; set; }
}
```

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Date` | DateTime | When the transaction occurred |
| `AssetAmount` | decimal | Transaction amount (always positive) |
| `Description` | string? | Optional description of the transaction |
| `ApprovedAt` | DateTime? | When the transaction was approved (null if pending) |
| `ApprovedBy` | Guid? | User ID who approved the transaction |
| `SenderWallet` | WalletIdentifierSummary | Summary of the sending wallet |
| `ReceiverWallet` | WalletIdentifierSummary | Summary of the receiving wallet |
| `Category` | CategoryResponse? | Transaction classification category |
| `TransactionType` | string | Type identifier: "FiatAsset", "DigitalAsset", or "Settlement" |
| `IsInternalTransfer` | bool | True if both wallets belong to the same asset holder |
| `AssetType` | AssetType | The specific asset type (BrazilianReal, PokerStars, Bitcoin, etc.) |

---

## Supporting Models

### WalletIdentifierSummary

Provides simplified wallet information without exposing the full entity complexity.

```csharp
public class WalletIdentifierSummary
{
    public Guid Id { get; set; }
    public AssetGroup AssetGroup { get; set; }
    public AccountClassification AccountClassification { get; set; }
    public AssetType AssetType { get; set; }
    public AssetHolderSummary? AssetHolder { get; set; }
    public string? DisplayMetadata { get; set; }
}
```

| Property | Type | Description |
|----------|------|-------------|
| `Id` | Guid | Unique identifier of the wallet |
| `AssetGroup` | AssetGroup | Category: FiatAssets, PokerAssets, CryptoAssets, Internal, Settlements |
| `AccountClassification` | AccountClassification | Accounting type: ASSET, LIABILITY, EQUITY, etc. |
| `AssetType` | AssetType | Specific asset (BrazilianReal, PokerStars, Bitcoin, etc.) |
| `AssetHolder` | AssetHolderSummary? | Owner information (null for company-owned wallets) |
| `DisplayMetadata` | string? | Key metadata for display (PIX key, player nickname, wallet address) |

### AssetHolderSummary

Simplified information about the wallet owner.

```csharp
public class AssetHolderSummary
{
    public Guid Id { get; set; }
    public Guid BaseAssetHolderId { get; set; }
    public string Name { get; set; } = string.Empty;
    public AssetHolderType AssetHolderType { get; set; }
    public string? Email { get; set; }
}
```

| Property | Type | Description |
|----------|------|-------------|
| `Id` | Guid | Asset holder's unique identifier |
| `BaseAssetHolderId` | Guid | Base asset holder ID (used for cross-entity comparisons) |
| `Name` | string | Display name of the asset holder |
| `AssetHolderType` | AssetHolderType | Type: Client, Bank, Member, or PokerManager |
| `Email` | string? | Contact email if available |

---

## Fiat Asset Transaction Response

Extends the base response with banking and PIX-specific information.

### Definition

```csharp
public class FiatAssetTransactionResponse : BaseTransactionResponse
{
    public OfxTransactionSummary? OfxTransaction { get; set; }
    public BankTransactionInfo? BankInfo { get; set; }
    public PixTransactionInfo? PixInfo { get; set; }
}
```

### Supporting Models

#### OfxTransactionSummary

Information from OFX bank imports.

```csharp
public class OfxTransactionSummary
{
    public Guid Id { get; set; }
    public string? FitId { get; set; }           // Bank's unique transaction ID
    public decimal Value { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public string? BankName { get; set; }
    public string? FileName { get; set; }        // Source OFX file name
}
```

#### BankTransactionInfo

Bank account details extracted from wallet metadata.

```csharp
public class BankTransactionInfo
{
    public string? BankName { get; set; }
    public string? AccountNumber { get; set; }
    public string? RoutingNumber { get; set; }
    public string? AccountType { get; set; }     // Checking, Savings, etc.
}
```

#### PixTransactionInfo

PIX (Brazilian instant payment) details.

```csharp
public class PixTransactionInfo
{
    public string? PixKey { get; set; }
    public string? PixKeyType { get; set; }      // CPF, CNPJ, Email, Phone, Random
    public string? EndToEndId { get; set; }      // PIX transaction identifier
}
```

---

## Digital Asset Transaction Response

Extends the base response with poker and cryptocurrency-specific information.

### Definition

```csharp
public class DigitalAssetTransactionResponse : BaseTransactionResponse
{
    // Conversion support
    public AssetType? ConvertTo { get; set; }
    public decimal? ConversionRate { get; set; }
    public decimal? Rate { get; set; }
    public decimal? Profit { get; set; }
    public ConversionDetails? ConversionDetails { get; set; }
    
    // Import tracking
    public ExcelTransactionSummary? Excel { get; set; }
    
    // Context-specific information
    public PokerTransactionInfo? PokerInfo { get; set; }
    public CryptoTransactionInfo? CryptoInfo { get; set; }
}
```

### Supporting Models

#### ConversionDetails

Complete information for asset conversion transactions.

```csharp
public class ConversionDetails
{
    public AssetType FromAsset { get; set; }
    public AssetType ToAsset { get; set; }
    public decimal FromAmount { get; set; }
    public decimal ToAmount { get; set; }
    public decimal ExchangeRate { get; set; }
    public decimal? Fee { get; set; }
    public string? ExchangeUsed { get; set; }
}
```

#### ExcelTransactionSummary

Information about Excel file imports.

```csharp
public class ExcelTransactionSummary
{
    public Guid Id { get; set; }
    public string? FileName { get; set; }
    public string? FileType { get; set; }
    public string? PokerManagerName { get; set; }
    public DateTime ImportedAt { get; set; }
}
```

#### PokerTransactionInfo

Poker-specific transaction details.

```csharp
public class PokerTransactionInfo
{
    public string? PlayerNickname { get; set; }
    public string? PlayerEmail { get; set; }
    public string? AccountStatus { get; set; }
    public string? PokerSite { get; set; }       // PokerStars, GGPoker, etc.
    public string? TableInfo { get; set; }
    public string? GameType { get; set; }
}
```

#### CryptoTransactionInfo

Cryptocurrency-specific transaction details.

```csharp
public class CryptoTransactionInfo
{
    public string? WalletAddress { get; set; }
    public string? WalletCategory { get; set; }
    public string? NetworkType { get; set; }     // Bitcoin, Ethereum, etc.
    public string? TransactionHash { get; set; }
    public int? Confirmations { get; set; }
    public decimal? NetworkFee { get; set; }
}
```

---

## Settlement Transaction Response

Extends the base response with poker settlement-specific information including rake calculations.

### Definition

```csharp
public class SettlementTransactionResponse : BaseTransactionResponse
{
    public decimal RakeAmount { get; set; }
    public decimal RakeCommission { get; set; }
    public decimal? RakeBack { get; set; }
    public decimal SignedAssetAmount { get; set; }
    
    // Calculated properties
    public decimal NetSettlementAmount => 
        SignedAssetAmount + RakeAmount * (RakeCommission / 100);
    
    public decimal? EffectiveCommissionRate => 
        RakeAmount > 0 ? RakeAmount * ((RakeCommission - RakeBack) / 100) : 0;
    
    public SettlementDetails? SettlementInfo { get; set; }
}
```

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `RakeAmount` | decimal | Total rake collected from poker play |
| `RakeCommission` | decimal | Commission applied to the rake |
| `RakeBack` | decimal? | Rakeback returned to the player |
| `SignedAssetAmount` | decimal | Asset amount signed by poker manager position |
| `NetSettlementAmount` | decimal | **Calculated**: Signed amount + rake commission |
| `EffectiveCommissionRate` | decimal? | **Calculated**: Effective commission value (not percentage) |
| `SettlementInfo` | SettlementDetails? | Additional settlement context |

**SignedAssetAmount rule:**
- Negative when the poker manager is the **receiver**
- Positive when the poker manager is the **sender**

#### SettlementDetails

```csharp
public class SettlementDetails
{
    public string? SettlementPeriod { get; set; }     // Weekly, Monthly, etc.
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }
    public int? TransactionCount { get; set; }       // Hands/sessions in period
    public decimal? TotalVolume { get; set; }
    public string? SettlementType { get; set; }      // Poker Settlement, etc.
}
```

### Simplified Settlement Response

A lighter version for list views:

```csharp
public class SettlementTransactionSimplifiedResponse : BaseResponse
{
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public CategoryResponse? FinancialBehavior { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    
    public decimal AssetAmount { get; set; }
    public decimal Rake { get; set; }
    public decimal RakeCommission { get; set; }
    public decimal? RakeBack { get; set; }
}
```

---

## Settlement Closings Response DTOs

These DTOs are used by the `/api/v1/settlementtransaction/closings/{pokerManagerId}` endpoint to return settlement transactions grouped by date for daily reconciliation.

**Location:** `Application/DTOs/Transactions/SettlementClosingsResponse.cs`

### SettlementClosingsGroupedResponse

Top-level response containing a list of closing groups (one per settlement date).

```csharp
public class SettlementClosingsGroupedResponse
{
    public List<SettlementClosingGroup> ClosingGroups { get; set; } = new();
}
```

### SettlementClosingGroup

Represents a single day's settlements with computed aggregate metrics.

```csharp
public class SettlementClosingGroup
{
    public DateTime Date { get; set; }
    public List<SettlementTransactionResponse> Transactions { get; set; } = new();
    
    // Computed properties
    public int TransactionCount => Transactions.Count;
    public decimal TotalAssetAmount => Transactions.Sum(t => t.SignedAssetAmount);
    public decimal TotalRake => Transactions.Sum(t => t.RakeAmount);
    public decimal TotalRakeCommission => Transactions.Sum(t => t.RakeCommission);
    public decimal? TotalRakeBack => Transactions.Sum(t => t.RakeBack ?? 0);
    public decimal TotalNetSettlement => Transactions.Sum(t => t.NetSettlementAmount);
}
```

### Computed Aggregates

| Property | Type | Description |
|----------|------|-------------|
| `TransactionCount` | int | Number of settlements on this date |
| `TotalAssetAmount` | decimal | Sum of all `SignedAssetAmount` values |
| `TotalRake` | decimal | Sum of all `RakeAmount` values |
| `TotalRakeCommission` | decimal | Sum of all `EffectiveCommissionRate` values |
| `TotalRakeBack` | decimal? | Sum of all `RakeBack` values (null coalesced to 0) |
| `TotalNetSettlement` | decimal | Sum of all `NetSettlementAmount` values |

### API Response Example

```json
{
  "closingGroups": [
    {
      "date": "2026-01-22T00:00:00Z",
      "transactions": [
        {
          "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
          "date": "2026-01-22T00:00:00Z",
          "assetAmount": 10000.00,
          "rakeAmount": 500.00,
          "rakeCommission": 150.00,
          "rakeBack": 50.00,
          "netSettlementAmount": 9400.00,
          "effectiveCommissionRate": 1.5,
          "transactionType": "Settlement",
          "senderWallet": { ... },
          "receiverWallet": { ... }
        }
      ],
      "transactionCount": 15,
      "totalAssetAmount": 250000.00,
      "totalRake": 12500.00,
      "totalRakeCommission": 3750.00,
      "totalRakeBack": 1250.00,
      "totalNetSettlement": 235000.00
    },
    {
      "date": "2026-01-21T00:00:00Z",
      "transactions": [...],
      "transactionCount": 12,
      "totalAssetAmount": 180000.00,
      "totalRake": 9000.00,
      "totalRakeCommission": 2700.00,
      "totalRakeBack": 900.00,
      "totalNetSettlement": 168300.00
    }
  ]
}
```

### Business Context

A "Closing" (Portuguese: **Fechamento**) represents a daily batch of settlement transactions. Key uses:

1. **Daily Reconciliation**: View all settlements from a specific date as a group
2. **Aggregate Reporting**: See totals for amount, rake, commissions without manual calculation
3. **Last Settlement Tracking**: Used by `wallet-identifiers-connected` to show each wallet's most recent settlement

For detailed information about the Closing concept and business workflow, see [SETTLEMENT_WORKFLOW.md](./SETTLEMENT_WORKFLOW.md#closings-fechamentos).

---

## AutoMapper Overview

The DTOs are populated using AutoMapper with intelligent metadata extraction. Key mapping features include:

- **Transaction Type Identification**: Each transaction type sets its `TransactionType` string
- **Asset Type Detection**: Extracts from sender wallet's `AssetType`
- **Internal Transfer Detection**: Uses `IsInternalTransfer` computed property
- **Wallet Summary Mapping**: Creates `WalletIdentifierSummary` with display metadata based on `AssetGroup`
- **Context-Aware Info**: Automatically detects and extracts bank, PIX, poker, or crypto information
- **PIX Key Type Detection**: Automatically determines PIX key type (CPF, CNPJ, Email, Phone, Random)
- **Poker Site Mapping**: Derives poker site name from `AssetType`

> **Note:** For detailed AutoMapper configuration and mapping rules, see [AUTOMAPPER_CONFIGURATION.md](../02_ARCHITECTURE/AUTOMAPPER_CONFIGURATION.md).

---

## API Response Examples

### Fiat Asset Transaction

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "date": "2024-01-15T10:30:00Z",
  "assetAmount": 1000.00,
  "description": "PIX Transfer",
  "transactionType": "FiatAsset",
  "assetType": "BrazilianReal",
  "isInternalTransfer": false,
  "senderWallet": {
    "id": "8fa85f64-5717-4562-b3fc-2c963f66afa6",
    "assetGroup": "FiatAssets",
    "accountClassification": "ASSET",
    "assetType": "BrazilianReal",
    "assetHolder": {
      "id": "1fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "Bank of Brazil",
      "assetHolderType": "Bank"
    },
    "displayMetadata": "john@example.com"
  },
  "receiverWallet": {
    "id": "9fa85f64-5717-4562-b3fc-2c963f66afa6",
    "assetGroup": "FiatAssets",
    "accountClassification": "LIABILITY",
    "assetType": "BrazilianReal",
    "assetHolder": {
      "id": "2fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "John Doe",
      "assetHolderType": "Client"
    },
    "displayMetadata": "999.888.777-66"
  },
  "bankInfo": {
    "bankName": "Bank of Brazil",
    "accountNumber": "12345-6",
    "routingNumber": "0001",
    "accountType": "Checking"
  },
  "pixInfo": {
    "pixKey": "john@example.com",
    "pixKeyType": "Email"
  }
}
```

### Digital Asset Transaction (Poker)

```json
{
  "id": "4fa85f64-5717-4562-b3fc-2c963f66afa6",
  "date": "2024-01-15T14:00:00Z",
  "assetAmount": 500.00,
  "transactionType": "DigitalAsset",
  "assetType": "PokerStars",
  "isInternalTransfer": false,
  "senderWallet": {
    "id": "afa85f64-5717-4562-b3fc-2c963f66afa6",
    "assetGroup": "PokerAssets",
    "assetType": "PokerStars",
    "assetHolder": {
      "name": "Poker Manager ABC",
      "assetHolderType": "PokerManager"
    },
    "displayMetadata": "player123"
  },
  "receiverWallet": {
    "id": "bfa85f64-5717-4562-b3fc-2c963f66afa6",
    "assetGroup": "PokerAssets",
    "assetType": "PokerStars",
    "assetHolder": {
      "name": "John Doe",
      "assetHolderType": "Client"
    },
    "displayMetadata": "johndoe_poker"
  },
  "pokerInfo": {
    "playerNickname": "johndoe_poker",
    "playerEmail": "john@example.com",
    "accountStatus": "Active",
    "pokerSite": "PokerStars"
  }
}
```

### Settlement Transaction

```json
{
  "id": "5fa85f64-5717-4562-b3fc-2c963f66afa6",
  "date": "2024-01-20T00:00:00Z",
  "assetAmount": 10000.00,
  "signedAssetAmount": -10000.00,
  "transactionType": "Settlement",
  "assetType": "PokerStars",
  "isInternalTransfer": false,
  "rakeAmount": 500.00,
  "rakeCommission": 150.00,
  "rakeBack": 50.00,
  "netSettlementAmount": -9250.00,
  "effectiveCommissionRate": 500.0,
  "settlementInfo": {
    "settlementType": "Poker Settlement",
    "settlementPeriod": "Weekly",
    "periodStart": "2024-01-13T00:00:00Z",
    "periodEnd": "2024-01-19T23:59:59Z"
  }
}
```

---

## Usage Examples

### Client-Side Display Helper (TypeScript)

```typescript
interface TransactionDisplay {
  displayName: string;
  amount: number;
  direction: 'incoming' | 'outgoing';
  counterparty: string;
  metadata: string;
}

function mapTransactionForDisplay(
  transaction: BaseTransactionResponse, 
  userWalletId: string
): TransactionDisplay {
  const isIncoming = transaction.receiverWallet.id === userWalletId;
  const counterpartyWallet = isIncoming 
    ? transaction.senderWallet 
    : transaction.receiverWallet;
  
  return {
    displayName: transaction.description || `${transaction.transactionType} Transaction`,
    amount: transaction.assetAmount,
    direction: isIncoming ? 'incoming' : 'outgoing',
    counterparty: counterpartyWallet.assetHolder?.name || 'Company',
    metadata: counterpartyWallet.displayMetadata || ''
  };
}
```

### Server-Side Filtering (C#)

```csharp
// Filter by transaction type
var fiatTransactions = transactions
    .Where(t => t.TransactionType == "FiatAsset");

// Filter by asset group
var bankTransactions = transactions.Where(t => 
    t.SenderWallet.AssetGroup == AssetGroup.FiatAssets || 
    t.ReceiverWallet.AssetGroup == AssetGroup.FiatAssets);

// Filter by asset holder type
var clientTransactions = transactions.Where(t => 
    t.SenderWallet.AssetHolder?.AssetHolderType == AssetHolderType.Client ||
    t.ReceiverWallet.AssetHolder?.AssetHolderType == AssetHolderType.Client);

// Find company-to-client transactions
var companyToClientTransactions = transactions.Where(t =>
    t.SenderWallet.AssetHolder == null && // Company wallet (no asset holder)
    t.ReceiverWallet.AssetHolder?.AssetHolderType == AssetHolderType.Client);
```

### Settlement Analytics

```csharp
// Weekly settlement analysis
var settlementSummary = settlementTransactions
    .GroupBy(t => t.Date.Date)
    .Select(g => new {
        Date = g.Key,
        TotalVolume = g.Sum(t => t.AssetAmount),
        TotalRake = g.Sum(t => t.RakeAmount),
        TotalCommission = g.Sum(t => t.RakeCommission),
        TotalRakeback = g.Sum(t => t.RakeBack ?? 0),
        NetTotal = g.Sum(t => t.NetSettlementAmount),
        AverageCommissionRate = g.Average(t => t.EffectiveCommissionRate ?? 0)
    });
```

---

## Design Benefits

### Simplified Structure

- **Flat response format**: Essential information without deeply nested objects
- **Summary models**: Only the data needed for display and filtering
- **Predictable structure**: Same base properties across all transaction types

### Rich Context

- **Type identification**: Clear `TransactionType` string for client-side handling
- **Smart metadata**: `DisplayMetadata` provides the most relevant identifier for each wallet type
- **Calculated fields**: Settlement responses include pre-calculated business metrics

### Performance Optimization

- **Reduced payload size**: Summary models instead of full entity graphs
- **Selective inclusion**: Context-specific info only populated when relevant
- **Single-pass mapping**: All transformations done during AutoMapper execution

---

## Transfer Response DTOs

These DTOs are used by the unified `/api/v1/transfer` endpoint.

### TransferResponse

Response DTO for a completed transfer transaction.

**Location:** `Application/DTOs/Transactions/TransferResponse.cs`

```csharp
public class TransferResponse
{
    // Transaction identification
    public Guid TransactionId { get; set; }
    public string EntityType { get; set; } = string.Empty; // "fiat" or "digital"
    public AssetType AssetType { get; set; }
    
    // Sender details
    public Guid SenderWalletIdentifierId { get; set; }
    public Guid SenderAssetHolderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    
    // Receiver details
    public Guid ReceiverWalletIdentifierId { get; set; }
    public Guid ReceiverAssetHolderId { get; set; }
    public string ReceiverName { get; set; } = string.Empty;
    
    // Transaction details
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public bool IsInternalTransfer { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Wallet creation indicators
    public bool SenderWalletCreated { get; set; }
    public bool ReceiverWalletCreated { get; set; }
    public bool WalletsCreated => SenderWalletCreated || ReceiverWalletCreated;
}
```

**Key Features:**

| Property | Description |
|----------|-------------|
| `EntityType` | Indicates whether `FiatAssetTransaction` or `DigitalAssetTransaction` was created |
| `IsInternalTransfer` | Computed from sender/receiver asset holder comparison |
| `SenderWalletCreated` | True if sender wallet was auto-created (only on POST) |
| `ReceiverWalletCreated` | True if receiver wallet was auto-created (only on POST) |
| `WalletsCreated` | Computed: true if either wallet was created |

> **Note:** Wallet creation flags are only populated on POST requests. On GET, they are always `false` as creation state is not stored.

### WalletMissingError

Provides detailed information when wallet creation is required.

**Location:** `Application/DTOs/Transactions/WalletMissingError.cs`

```csharp
public class WalletMissingError
{
    public string Code { get; set; } = "WALLETS_REQUIRED";
    public string Message { get; set; } = 
        "One or more wallets need to be created to complete this transfer.";
    
    // Sender details
    public bool SenderWalletMissing { get; set; }
    public Guid? SenderAssetHolderId { get; set; }
    public string? SenderAssetHolderName { get; set; }
    public string? SenderAssetHolderType { get; set; }
    public string? SenderAssetTypeName { get; set; }
    
    // Receiver details
    public bool ReceiverWalletMissing { get; set; }
    public Guid? ReceiverAssetHolderId { get; set; }
    public string? ReceiverAssetHolderName { get; set; }
    public string? ReceiverAssetHolderType { get; set; }
    public string? ReceiverAssetTypeName { get; set; }
}
```

**Properties:**

| Property | Description |
|----------|-------------|
| `Code` | Error code, always `"WALLETS_REQUIRED"` |
| `SenderWalletMissing` | True if sender wallet needs to be created |
| `SenderAssetHolderName` | Display name of the sender (e.g., "Client João Silva") |
| `SenderAssetHolderType` | Type: Client, Member, Bank, or PokerManager |
| `SenderAssetTypeName` | Asset type name (e.g., "BrazilianReal", "PokerStars") |
| `ReceiverWalletMissing` | True if receiver wallet needs to be created |
| (Similar fields for receiver) | |

**Usage:**

This DTO is thrown as part of `WalletMissingException` and serialized in `ProblemDetails.Extensions["walletDetails"]`:

```json
{
  "title": "Wallet Creation Required",
  "status": 400,
  "extensions": {
    "errorCode": "WALLETS_REQUIRED",
    "walletDetails": {
      "code": "WALLETS_REQUIRED",
      "message": "One or more wallets need to be created...",
      "senderWalletMissing": true,
      "senderAssetHolderName": "Client João Silva",
      "senderAssetHolderType": "Client",
      "senderAssetTypeName": "BrazilianReal",
      "receiverWalletMissing": false
    }
  }
}
```

### TransferError and Error Codes

**Location:** `Application/DTOs/Transactions/TransferError.cs`

General error details for transfer operations:

```csharp
public class TransferError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Field { get; set; }
    public Dictionary<string, object>? Details { get; set; }
}
```

Static error codes:

```csharp
public static class TransferErrorCodes
{
    public const string SenderNotFound = "SENDER_NOT_FOUND";
    public const string ReceiverNotFound = "RECEIVER_NOT_FOUND";
    public const string SenderWalletNotFound = "SENDER_WALLET_NOT_FOUND";
    public const string ReceiverWalletNotFound = "RECEIVER_WALLET_NOT_FOUND";
    public const string WalletCreationFailed = "WALLET_CREATION_FAILED";
    public const string InsufficientBalance = "INSUFFICIENT_BALANCE";
    public const string InvalidAssetType = "INVALID_ASSET_TYPE";
    public const string AssetTypeMismatch = "ASSET_TYPE_MISMATCH";
    public const string WalletOwnershipMismatch = "WALLET_OWNERSHIP_MISMATCH";
    public const string InvalidAmount = "INVALID_AMOUNT";
    public const string CategoryNotFound = "CATEGORY_NOT_FOUND";
    public const string TransactionFailed = "TRANSACTION_FAILED";
    public const string SameSenderReceiverWallet = "SAME_SENDER_RECEIVER_WALLET";
}
```

---

## Related Documentation

For detailed information on related topics, refer to:

| Topic | Document |
|-------|----------|
| Transaction Infrastructure | [TRANSACTION_INFRASTRUCTURE.md](./TRANSACTION_INFRASTRUCTURE.md) |
| **Transaction API Endpoints** | [TRANSACTION_API_ENDPOINTS.md](../06_API/TRANSACTION_API_ENDPOINTS.md) |
| AutoMapper Configuration | [AUTOMAPPER_CONFIGURATION.md](../02_ARCHITECTURE/AUTOMAPPER_CONFIGURATION.md) |
| Asset Infrastructure | [ASSET_INFRASTRUCTURE.md](./ASSET_INFRASTRUCTURE.md) |
| Settlement Workflow | [SETTLEMENT_WORKFLOW.md](./SETTLEMENT_WORKFLOW.md) |
| API Reference | [API_REFERENCE.md](../06_API/API_REFERENCE.md) |
| Enum Definitions | [ENUMS_AND_TYPE_SYSTEM.md](../07_REFERENCE/ENUMS_AND_TYPE_SYSTEM.md) |
