# Transaction Infrastructure

## Table of Contents

- [Overview](#overview)
- [Core Concepts](#core-concepts)
- [Transaction Models](#transaction-models)
  - [BaseTransaction](#basetransaction-abstract)
  - [FiatAssetTransaction](#fiatasettransaction)
  - [DigitalAssetTransaction](#digitalassettransaction)
  - [SettlementTransaction](#settlementtransaction)
  - [ImportedTransaction](#importedtransaction)
- [Integration with Asset Infrastructure](#integration-with-asset-infrastructure)
- [Best Practices](#best-practices)
- [Entity Summary](#entity-summary)
- [Related Documentation](#related-documentation)

---

## Overview

The Transaction Infrastructure provides a unified system for recording and managing financial movements across all asset types in the SF Management system. The architecture follows a sender/receiver pattern where every transaction moves assets from one `WalletIdentifier` to another, enabling consistent handling of fiat currency, poker credits, cryptocurrency, and settlement operations.

---

## Core Concepts

### The Sender/Receiver Pattern

All transactions in the system share a fundamental design principle: **every transaction has a sender and a receiver**. This creates a consistent model regardless of asset type:

- **Sender WalletIdentifier**: The wallet from which assets are deducted
- **Receiver WalletIdentifier**: The wallet to which assets are added
- **Asset Amount**: Always stored as a positive value; direction is determined by the sender/receiver relationship

This pattern eliminates the need for separate "income" and "expense" transaction types—the direction is implicit in whether a wallet is the sender or receiver.

---

## Transaction Models

### BaseTransaction (Abstract)

The `BaseTransaction` class is the foundation for all transaction types. It cannot be instantiated directly but provides common functionality inherited by all concrete transaction types.

```csharp
public abstract class BaseTransaction : BaseDomain
{
    [Required] public DateTime Date { get; set; }
    
    public Guid? CategoryId { get; set; }
    public virtual Category? Category { get; set; }

    // Sender
    [Required] public Guid SenderWalletIdentifierId { get; set; }
    public virtual WalletIdentifier SenderWalletIdentifier { get; set; }
    
    // Receiver
    [Required] public Guid ReceiverWalletIdentifierId { get; set; }
    public virtual WalletIdentifier ReceiverWalletIdentifier { get; set; }
    
    // Only positive amounts are allowed
    [Required] [Precision(18, 2)] public decimal AssetAmount { get; set; }
    
    public DateTime? ApprovedAt { get; set; }
}
```

**Key Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `Date` | DateTime | When the transaction occurred |
| `CategoryId` | Guid? | Optional reference to a Category for classification |
| `SenderWalletIdentifierId` | Guid | Required. The wallet sending assets |
| `ReceiverWalletIdentifierId` | Guid | Required. The wallet receiving assets |
| `AssetAmount` | decimal | The transaction amount (always positive) |
| `ApprovedAt` | DateTime? | When the transaction was approved (null if pending) |

### Built-in Helper Methods

The `BaseTransaction` class provides several utility methods for working with transactions:

#### Direction Detection

```csharp
// Check if a wallet is the sender or receiver
public bool IsSender(Guid walletIdentifierId) 
    => SenderWalletIdentifierId == walletIdentifierId;

public bool IsReceiver(Guid walletIdentifierId) 
    => ReceiverWalletIdentifierId == walletIdentifierId;
```

#### Signed Amount Calculation

```csharp
// Get the amount with sign based on perspective
public decimal GetSignedAmountForWalletIdentifier(Guid walletIdentifierId)
{
    if (SenderWalletIdentifierId == walletIdentifierId)
        return -AssetAmount; // Outgoing (negative)
    
    if (ReceiverWalletIdentifierId == walletIdentifierId)
        return AssetAmount; // Incoming (positive)
        
    throw new ArgumentException("Wallet identifier is not involved in this transaction");
}
```

#### Counterparty Information

```csharp
// Get the other party in the transaction
public WalletIdentifier GetCounterpartyForWalletIdentifier(Guid walletIdentifierId)
{
    if (SenderWalletIdentifierId == walletIdentifierId)
        return ReceiverWalletIdentifier;
    
    if (ReceiverWalletIdentifierId == walletIdentifierId)
        return SenderWalletIdentifier;
        
    throw new ArgumentException("Wallet identifier is not involved in this transaction");
}

// Get the name of the counterparty's asset holder
public string GetCounterPartyName(Guid walletIdentifierId)
{
    if (SenderWalletIdentifierId == walletIdentifierId)
        return ReceiverWalletIdentifier?.AssetPool?.BaseAssetHolder?.Name ?? "Unknown";
    
    if (ReceiverWalletIdentifierId == walletIdentifierId)
        return SenderWalletIdentifier?.AssetPool?.BaseAssetHolder?.Name ?? "Unknown";
        
    throw new ArgumentException("Wallet identifier is not involved in this transaction");
}
```

#### Account Classification Helpers

```csharp
// Check if both wallets have the same account classification
public bool HaveBothWalletsSameAccountClassification()
{
    return SenderWalletIdentifier.AccountClassification == 
           ReceiverWalletIdentifier.AccountClassification;
}

// Check if a specific wallet is classified as a liability
public bool IsWalletIdentifierLiability(Guid walletIdentifierId)
{
    if (SenderWalletIdentifierId == walletIdentifierId)
        return SenderWalletIdentifier.AccountClassification == AccountClassification.LIABILITY;
    
    return ReceiverWalletIdentifier.AccountClassification == AccountClassification.LIABILITY;
}
```

#### Internal Transfer Detection

```csharp
// Computed property to check if both wallets belong to the same asset holder
[NotMapped]
public bool IsInternalTransfer => 
    SenderWalletIdentifier?.AssetPool?.BaseAssetHolderId == 
    ReceiverWalletIdentifier?.AssetPool?.BaseAssetHolderId;
```

---

### FiatAssetTransaction

Used for traditional currency transactions such as bank transfers, PIX payments, and cash movements.

```csharp
public class FiatAssetTransaction : BaseTransaction
{
    // Inherits all properties from BaseTransaction
    // No additional properties
}
```

**Use Cases:**
- Bank-to-bank transfers
- PIX transactions
- Cash deposits/withdrawals
- Payment processing

### DigitalAssetTransaction

Used for poker credits and cryptocurrency transactions, with support for asset conversion.

```csharp
public class DigitalAssetTransaction : BaseTransaction
{
    // Optional: Convert balance to a different asset type
    public AssetType? BalanceAs { get; set; }
    
    // Conversion rate when changing asset types
    [Precision(18, 4)] public decimal? ConversionRate { get; set; }
    
    // Exchange rate at time of transaction
    [Precision(18, 4)] public decimal? Rate { get; set; }
}
```

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `BalanceAs` | AssetType? | Target asset type for conversion (null if no conversion) |
| `ConversionRate` | decimal? | Rate used for asset conversion |
| `Rate` | decimal? | Exchange rate at transaction time |

**Use Cases:**
- Poker site credit transfers (PokerStars, GGPoker, etc.)
- Cryptocurrency movements (Bitcoin, USDT)
- Asset conversions between poker currencies
- Currency exchange operations

### SettlementTransaction

Used for poker settlement operations that include rake and commission calculations.

```csharp
public class SettlementTransaction : BaseTransaction
{
    // Total rake amount collected
    [Precision(18, 2), Required] public decimal RakeAmount { get; set; }
    
    // Commission paid on the rake
    [Precision(18, 2), Required] public decimal RakeCommission { get; set; }
    
    // Rakeback returned to player (if applicable)
    [Precision(18, 2)] public decimal? RakeBack { get; set; }
}
```

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `RakeAmount` | decimal | Total rake collected from poker play |
| `RakeCommission` | decimal | Commission percentage applied to rake |
| `RakeBack` | decimal? | Optional rakeback amount returned |

**Use Cases:**
- Weekly/monthly poker settlements
- Player profit calculations
- Rake tracking and reporting

### ImportedTransaction

Represents transactions imported from external files (OFX bank statements, Excel spreadsheets). This model is separate from the `BaseTransaction` hierarchy and serves as a staging area for external data.

```csharp
public class ImportedTransaction : BaseDomain
{
    [Required] public DateTime Date { get; set; }
    [Required] [Precision(18, 2)] public decimal Amount { get; set; }
    [MaxLength(100)] public string? Description { get; set; }
    [MaxLength(64)] public string? ExternalReferenceId { get; set; }
    
    [Required] public Guid BaseAssetHolderId { get; set; }
    public virtual BaseAssetHolder BaseAssetHolder { get; set; }
    
    [Required] public ImportFileType FileType { get; set; }
    [Required] [MaxLength(32)] public string FileName { get; set; }
    [MaxLength(64)] public string? FileHash { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? FileMetadata { get; set; }
    
    [Required] public ImportedTransactionStatus Status { get; set; }
    
    // Reconciliation tracking
    public ReconciledTransactionType? ReconciledTransactionType { get; set; }
    public Guid? ReconciledTransactionId { get; set; }
    public DateTime? ReconciledAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    
    // Helper properties
    public bool IsReconciled => ReconciledTransactionId.HasValue && 
                                ReconciledAt.HasValue && 
                                ReconciledTransactionType.HasValue;
    public bool IsProcessed => Status == ImportedTransactionStatus.Processed && 
                               ProcessedAt.HasValue;
}
```

**Key Features:**
- Tracks file origin (OFX, Excel, CSV)
- Maintains file integrity via hash
- Supports reconciliation with actual transactions
- Prevents duplicate imports via `ExternalReferenceId`

> **Note:** For detailed documentation on imported transactions and reconciliation, see [IMPORTED_TRANSACTIONS.md](./IMPORTED_TRANSACTIONS.md).

---

## Integration with Asset Infrastructure

### Relationship Diagram

```
BaseAssetHolder ─────────────┐
       │                     │
       │ owns                │ owns
       ▼                     ▼
  AssetPool (FiatAssets)  AssetPool (PokerAssets)
       │                     │
       │ contains            │ contains
       ▼                     ▼
WalletIdentifier ◄──────────────────────► WalletIdentifier
(BRL account)     Transaction (sender)    (PokerStars credits)
                    ▲        ▼
                    │        │
                    └────────┘
                   (receiver)
```

### Transaction Flow Example

1. **Client deposits Brazilian Reais to fund poker account:**
   - Sender: Client's Bank WalletIdentifier (FiatAssets)
   - Receiver: Client's PokerStars WalletIdentifier (PokerAssets)
   - Transaction Type: DigitalAssetTransaction

2. **Poker settlement at end of week:**
   - Sender: Settlement Pool WalletIdentifier (Settlements)
   - Receiver: Client's PokerStars WalletIdentifier (PokerAssets)
   - Transaction Type: SettlementTransaction
   - Includes: RakeAmount, RakeCommission, RakeBack

3. **Client withdraws winnings:**
   - Sender: Client's PokerStars WalletIdentifier (PokerAssets)
   - Receiver: Client's Bank WalletIdentifier (FiatAssets)
   - Transaction Type: FiatAssetTransaction

---

## Best Practices

### Creating Transactions

1. **Always validate wallet identifiers exist** before creating transactions
2. **Use the appropriate transaction type** for the asset being transferred
3. **Include Category** for proper financial classification
4. **Never set negative amounts** - the sender/receiver determines direction

### Querying Transactions

1. **Use the service layer methods** for optimized queries with proper includes
2. **Apply date filters** when possible to reduce result sets
3. **Use pagination** for large result sets
4. **Leverage indexes** by querying on Date, SenderWalletIdentifierId, or ReceiverWalletIdentifierId

### Performance Considerations

1. **Indexes are on concrete tables** - queries perform best when filtering by transaction type
2. **Balance calculations** use two separate queries (incoming/outgoing sums)
3. **Navigation properties are eagerly loaded** in service methods to avoid N+1 queries

---

## Entity Summary

| Entity | Purpose | Key Properties |
|--------|---------|----------------|
| `BaseTransaction` | Abstract base for all transactions | Date, Sender, Receiver, Amount, Category |
| `FiatAssetTransaction` | Fiat currency movements | Inherits BaseTransaction only |
| `DigitalAssetTransaction` | Poker/crypto transactions | BalanceAs, ConversionRate, Rate |
| `SettlementTransaction` | Poker settlements | RakeAmount, RakeCommission, RakeBack |
| `ImportedTransaction` | External file imports | FileType, FileName, Status, Reconciliation |

---

## Related Documentation

For detailed information on specific aspects of the transaction system, refer to:

| Topic | Document |
|-------|----------|
| Response ViewModels | [TRANSACTION_RESPONSE_VIEWMODELS.md](./TRANSACTION_RESPONSE_VIEWMODELS.md) |
| Settlement Workflow | [SETTLEMENT_WORKFLOW.md](./SETTLEMENT_WORKFLOW.md) |
| Imported Transactions | [IMPORTED_TRANSACTIONS.md](./IMPORTED_TRANSACTIONS.md) |
| Asset Infrastructure | [ASSET_INFRASTRUCTURE.md](./ASSET_INFRASTRUCTURE.md) |
| Service Layer | [SERVICE_LAYER_ARCHITECTURE.md](../02_ARCHITECTURE/SERVICE_LAYER_ARCHITECTURE.md) |
| API Endpoints | [API_REFERENCE.md](../06_API/API_REFERENCE.md) |
| Database Schema | [DATABASE_SCHEMA.md](../02_ARCHITECTURE/DATABASE_SCHEMA.md) |
| AutoMapper Config | [AUTOMAPPER_CONFIGURATION.md](../02_ARCHITECTURE/AUTOMAPPER_CONFIGURATION.md) |
| Enum Definitions | [ENUMS_AND_TYPE_SYSTEM.md](../07_REFERENCE/ENUMS_AND_TYPE_SYSTEM.md) |
