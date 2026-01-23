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

## Transaction Modes and Business Flows

The system supports 6 distinct transaction modes that represent different business operations:

### Intermediary-Based Transactions

These transactions involve an intermediary (Bank or PokerManager) that facilitates the transaction:

| Mode | Code | Description | Intermediary | Flow |
|------|------|-------------|--------------|------|
| **SALE** | `SALE` | Sale of digital assets to clients/members | PokerManager | PokerManager → Client/Member |
| **PURCHASE** | `PURCHASE` | Purchase of digital assets from clients/members | PokerManager | Client/Member → PokerManager |
| **RECEIPT** | `RECEIPT` | Receipt of fiat through bank | Bank | Client/Member → Bank |
| **PAYMENT** | `PAYMENT` | Payment of fiat through bank | Bank | Bank → Client/Member |

### Non-Intermediary Transactions

These transactions occur directly between parties without an intermediary:

| Mode | Code | Description | Flow |
|------|------|-------------|------|
| **TRANSFER** | `TRANSFER` | P2P transfer between different asset holders | Any → Any (same asset type) |
| **INTERNAL** | `INTERNAL` | Movement between wallets of the same asset holder | Wallet A → Wallet B (same holder) |

### Mode Determination

The system infers the transaction mode from the request data:

```csharp
bool isInternalTransfer = SenderAssetHolderId == ReceiverAssetHolderId;
// If true → INTERNAL mode
// If false → TRANSFER mode (or SALE/PURCHASE/RECEIPT/PAYMENT depending on participants)
```

### Transaction Type Mapping

| Frontend Mode | Backend Entity | Determination Logic |
|---------------|----------------|---------------------|
| SALE | DigitalAssetTransaction | Asset type is digital (poker/crypto) |
| PURCHASE | DigitalAssetTransaction | Asset type is digital (poker/crypto) |
| RECEIPT | FiatAssetTransaction | Asset type is fiat (BRL, USD) |
| PAYMENT | FiatAssetTransaction | Asset type is fiat (BRL, USD) |
| TRANSFER | **Either** | Based on asset type |
| INTERNAL | **Either** | Based on asset type |

---

## TransferService

**Location:** `Application/Services/Transactions/TransferService.cs`

### Purpose

Handles unified transfer operations between any two asset holders, supporting both Fiat and Digital assets. This service provides a single point of entry for P2P transfers, replacing scattered asset-holder-specific endpoints.

### Key Methods

#### TransferAsync

```csharp
public async Task<TransferResponse> TransferAsync(TransferRequest request)
```

Creates a transfer transaction between two asset holders.

**Features:**
- Automatic wallet creation (opt-in via `CreateWalletsIfMissing`)
- Asset type validation
- Balance validation (opt-in)
- Bank restriction for TRANSFER mode
- Transaction atomicity with rollback on errors

**Workflow:**

1. Validate asset type is not None
2. Determine entity type (fiat vs digital)
3. Validate sender and receiver exist
4. Infer mode and apply bank restriction (if TRANSFER)
5. Check wallet existence (if not creating)
6. Validate category (if provided)
7. Get or create sender wallet
8. Get or create receiver wallet
9. Validate not same wallet
10. Validate balance (if requested)
11. Create appropriate transaction (Fiat or Digital)
12. Save and commit
13. Return response with wallet creation flags

**Mode Inference:**

```csharp
var isInternalTransfer = request.SenderAssetHolderId == request.ReceiverAssetHolderId;
if (!isInternalTransfer)  // TRANSFER mode
{
    await ValidateNoBanksInTransferAsync(request);
}
// INTERNAL mode - skip bank validation
```

#### GetTransferAsync

```csharp
public async Task<TransferResponse?> GetTransferAsync(Guid transactionId, bool isFiat)
```

Retrieves a transfer transaction by ID.

**Parameters:**
- `transactionId`: Transaction GUID
- `isFiat`: True for FiatAssetTransaction, false for DigitalAssetTransaction

**Returns:** `TransferResponse` or null if not found

### Private Helper Methods

| Method | Purpose |
|--------|---------|
| `ValidateNoBanksInTransferAsync` | Ensures banks don't participate in TRANSFER mode |
| `CheckWalletsExistAsync` | Checks if wallets exist and returns detailed error if missing |
| `FindOrCreateWalletAsync` | Finds existing wallet or creates new one |
| `GetAndValidateWalletAsync` | Validates provided wallet belongs to expected holder |
| `DetermineAccountClassificationAsync` | Determines ASSET vs LIABILITY classification |
| `SetDefaultMetadata` | Sets placeholder metadata for auto-created wallets |
| `GetBalanceForWalletAsync` | Calculates current wallet balance |
| `FindWalletByAssetHolderAndType` | Queries for wallet by holder and asset type |
| `GetAssetHolderInfoAsync` | Retrieves asset holder name and type |
| `GetAssetHolderTypeAsync` | Determines asset holder type from ID |
| `GetAssetTypeName` | Converts AssetType enum to display name |

### Error Codes

See `TransferErrorCodes` static class (`Application/DTOs/Transactions/TransferError.cs`) for complete list:

| Code | Description |
|------|-------------|
| `SENDER_NOT_FOUND` | Sender asset holder does not exist |
| `RECEIVER_NOT_FOUND` | Receiver asset holder does not exist |
| `SENDER_WALLET_NOT_FOUND` | Sender wallet not found for asset type |
| `RECEIVER_WALLET_NOT_FOUND` | Receiver wallet not found for asset type |
| `WALLET_CREATION_FAILED` | Failed to create new wallet |
| `INSUFFICIENT_BALANCE` | Sender wallet has insufficient funds |
| `INVALID_ASSET_TYPE` | AssetType.None or invalid type |
| `ASSET_TYPE_MISMATCH` | Wallet asset type doesn't match request |
| `WALLET_OWNERSHIP_MISMATCH` | Wallet doesn't belong to expected holder |
| `SAME_SENDER_RECEIVER_WALLET` | Cannot transfer to same wallet |
| `BANK_NOT_ALLOWED_IN_TRANSFER` | Banks cannot participate in TRANSFER mode |
| `WALLETS_REQUIRED` | Wallets must be created (confirmation needed) |
| `TRANSACTION_FAILED` | General transaction error |

---

## Transaction Guardrails

### Guardrail 1: Wallet Creation Confirmation

**Purpose:** Prevent automatic wallet proliferation from bugs or unintended actions.

**Behavior:**
- Default: `CreateWalletsIfMissing = false`
- When wallets are missing, throws `WalletMissingException` with detailed info
- Frontend must explicitly confirm before retrying with `CreateWalletsIfMissing = true`

**Implementation:**

```csharp
// TransferService.cs
if (!request.CreateWalletsIfMissing)
{
    var walletError = await CheckWalletsExistAsync(request);
    if (walletError != null)
    {
        throw new WalletMissingException(walletError);
    }
}
```

**Error Response:**

```json
{
  "title": "Wallet Creation Required",
  "status": 400,
  "extensions": {
    "errorCode": "WALLETS_REQUIRED",
    "walletDetails": {
      "code": "WALLETS_REQUIRED",
      "message": "One or more wallets need to be created to complete this transfer.",
      "senderWalletMissing": true,
      "senderAssetHolderName": "Client João Silva",
      "senderAssetHolderType": "Client",
      "senderAssetTypeName": "BrazilianReal",
      "receiverWalletMissing": false
    }
  }
}
```

### Guardrail 2: Bank Transfer Restriction

**Purpose:** Enforce business rule that banks only participate via RECEIPT/PAYMENT modes, not TRANSFER.

**Rule:**
- **TRANSFER mode** (different asset holders): Banks not allowed
- **INTERNAL mode** (same asset holder): Banks allowed

**Implementation:**

```csharp
// Mode inference
var isInternalTransfer = request.SenderAssetHolderId == request.ReceiverAssetHolderId;

if (!isInternalTransfer)  // TRANSFER mode
{
    // Validate neither sender nor receiver is a bank
    await ValidateNoBanksInTransferAsync(request);
}
```

**Validation Matrix:**

| Mode | Bank as Sender | Bank as Receiver | Enforced By |
|------|----------------|------------------|-------------|
| SALE | N/A (PokerManager) | ❌ Blocked | Frontend |
| PURCHASE | ❌ Blocked | N/A (PokerManager) | Frontend |
| RECEIPT | ❌ Blocked | ✅ Allowed | Business logic |
| PAYMENT | ✅ Allowed | ❌ Blocked | Business logic |
| TRANSFER | ❌ Blocked | ❌ Blocked | **TransferService** |
| INTERNAL | ✅ Allowed | ✅ Allowed | No restriction |

**Error Response:**

```json
{
  "title": "Transfer Failed",
  "status": 400,
  "detail": "Banks cannot be the sender in a transfer. Use Payment mode instead.",
  "extensions": {
    "errorCode": "BANK_NOT_ALLOWED_IN_TRANSFER"
  }
}
```

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
| **API Endpoints (Detailed)** | [TRANSACTION_API_ENDPOINTS.md](../06_API/TRANSACTION_API_ENDPOINTS.md) |
| Response ViewModels | [TRANSACTION_RESPONSE_VIEWMODELS.md](./TRANSACTION_RESPONSE_VIEWMODELS.md) |
| Settlement Workflow | [SETTLEMENT_WORKFLOW.md](./SETTLEMENT_WORKFLOW.md) |
| Imported Transactions | [IMPORTED_TRANSACTIONS.md](./IMPORTED_TRANSACTIONS.md) |
| Asset Infrastructure | [ASSET_INFRASTRUCTURE.md](./ASSET_INFRASTRUCTURE.md) |
| Service Layer | [SERVICE_LAYER_ARCHITECTURE.md](../02_ARCHITECTURE/SERVICE_LAYER_ARCHITECTURE.md) |
| API Reference (Overview) | [API_REFERENCE.md](../06_API/API_REFERENCE.md) |
| Database Schema | [DATABASE_SCHEMA.md](../02_ARCHITECTURE/DATABASE_SCHEMA.md) |
| AutoMapper Config | [AUTOMAPPER_CONFIGURATION.md](../02_ARCHITECTURE/AUTOMAPPER_CONFIGURATION.md) |
| Enum Definitions | [ENUMS_AND_TYPE_SYSTEM.md](../07_REFERENCE/ENUMS_AND_TYPE_SYSTEM.md) |
| Transfer Implementation | [TRANSFER_ENDPOINT_IMPLEMENTATION_PLAN.md](../10_REFACTORING/TRANSFER_ENDPOINT_IMPLEMENTATION_PLAN.md) |
