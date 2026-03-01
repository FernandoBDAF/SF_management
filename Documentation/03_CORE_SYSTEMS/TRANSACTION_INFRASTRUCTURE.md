# Transaction Infrastructure

> **Last Updated:** March 2026

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
- [PokerManager Self-Conversion](#pokermanager-self-conversion)
- [Best Practices](#best-practices)
- [Entity Summary](#entity-summary)
- [Known Issues](#known-issues)
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
        return SenderWalletIdentifier.AccountClassification == AccountClassification.Liability;
    
    return ReceiverWalletIdentifier.AccountClassification == AccountClassification.Liability;
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

#### BalanceAs vs Coin Balance Scenarios

**With BalanceAs (Standard Flow):**

When `BalanceAs` and `ConversionRate` are set:
- Client's balance in the **BalanceAs asset** (e.g., BRL) is impacted
- Client's balance in the **transaction asset** (e.g., PokerStars) is **NOT** impacted
- PokerManager's balance in transaction asset IS impacted

```
Example: Client buys 1000 chips at 5.0 BRL

Transaction: PokerManager → Client (PokerStars)
BalanceAs: BRL, ConversionRate: 5.0

Balance Impacts:
├─ PokerManager PokerAssets: -1000 (company inventory decreased)
├─ Client PokerAssets: NO CHANGE (chips not in their balance)
└─ Client FiatAssets (BRL): -5000 (owes company 5000 BRL)
```

**Without BalanceAs (Coin Balance):**

When `BalanceAs` is null (checkbox "Troca ou saldo em fichas"):
- Client's balance in the **transaction asset** is impacted
- Represents debt in the SAME asset type
- `Rate` field applies as a fee/discount percentage

```
Example: Client buys 1000 chips with 5% rate

Transaction: PokerManager → Client (PokerStars)
BalanceAs: null, Rate: 5%

Balance Impacts:
├─ PokerManager PokerAssets: -1000
└─ Client PokerAssets: -1050 (owes 1000 + 5% fee IN CHIPS)
```

#### Rate vs ConversionRate

| Field | Purpose | Used When |
|-------|---------|-----------|
| `ConversionRate` | Currency exchange rate | BalanceAs is set |
| `Rate` | Fee/discount percentage | BalanceAs is null (Coin Balance) |

### SettlementTransaction

Used for poker settlement operations that record rake, commission, and rakeback.

```csharp
public class SettlementTransaction : BaseTransaction
{
    // Total rake the client paid to the poker site (in chips)
    [Precision(18, 2), Required] public decimal RakeAmount { get; set; }
    
    // Percentage of rake the poker site pays to the company
    [Precision(18, 2), Required] public decimal RakeCommission { get; set; }
    
    // Percentage of rake the company returns to the client
    [Precision(18, 2)] public decimal? RakeBack { get; set; }
}
```

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `RakeAmount` | decimal | Total rake client paid to poker site (chips) |
| `RakeCommission` | decimal | % of rake poker site pays company |
| `RakeBack` | decimal? | % of rake company returns to client |

> **Important:** `AssetAmount` (from BaseTransaction) represents chip flow for tracking purposes. It should NOT be used for balance calculation as those chips already flowed via DigitalAssetTransactions.

**Balance Impact:**

| Entity | Balance Impact | Formula |
|--------|---------------|---------|
| Client | Receives rakeback | `+RakeAmount × (RakeBack / 100)` |
| PokerManager | Company earns commission | `-RakeAmount × (RakeCommission / 100)` |

**Company Profit (Finance Module - TBD):**
```
Company Profit = RakeAmount × ((RakeCommission - RakeBack) / 100)
```

**Use Cases:**
- Weekly/monthly poker settlements
- Rake and commission tracking
- Rakeback distribution to clients

#### Settlement Rakeback in Entity Statements

When settlement transactions appear in client or member statements, the following fields are mapped from `SettlementTransaction` properties to the statement response:

| Statement Field | Source | Description |
|-----------------|--------|-------------|
| `RakeAmount` | `SettlementTransaction.RakeAmount` | Total rake paid to poker site |
| `RakeCommission` | `SettlementTransaction.RakeCommission` | % of rake paid to the company |
| `RakeBack` | `SettlementTransaction.RakeBack` | % of rake returned to entity |
| `RakeBackAmount` | Computed: `RakeAmount × (RakeBack / 100)` | Actual rakeback value in the statement |
| `AssetAmount` | `SettlementTransaction.AssetAmount` | Chip flow amount (for reference) |

> **Note:** In entity statements, the displayed **Valor** for settlement rows is the `RakeBackAmount` (not `AssetAmount`), since rakeback is the financially relevant impact for the client/member. The `AssetAmount` represents chip flow that was already recorded via `DigitalAssetTransaction`.

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

The system supports 7 distinct transaction modes that represent different business operations:

### Intermediary-Based Transactions

These transactions involve an intermediary (Bank or PokerManager) that facilitates the transaction:

| Mode | Code | Description | Business Meaning | Flow |
|------|------|-------------|------------------|------|
| **SALE** | `SALE` | Client/Member **buys** chips from company | Company sells chips | PokerManager → Client/Member |
| **PURCHASE** | `PURCHASE` | Client/Member **sells** chips to company | Company buys chips | Client/Member → PokerManager |
| **RECEIPT** | `RECEIPT` | Client/Member **pays** money to company | Company receives payment | Client/Member → Bank |
| **PAYMENT** | `PAYMENT` | Company **pays** money to Client/Member | Company makes payment | Bank → Client/Member |

> **Key Insight:** The perspective is from the Client/Member. "SALE" means they are buying (company is selling to them).

### Non-Intermediary Transactions

These transactions occur directly between parties without an intermediary:

| Mode | Code | Description | Flow |
|------|------|-------------|------|
| **TRANSFER** | `TRANSFER` | P2P transfer between different asset holders | Any → Any (same asset type) |
| **SELF_TRANSFER** | `INTERNAL` | Movement between wallets of the same asset holder | Wallet A → Wallet B (same holder) |
| **CONVERSION** | `CONVERSION` | PokerManager self-conversion with dual-balance impact | Internal ↔ PokerAssets (same PokerManager) |

> **Note:** The code `INTERNAL` refers to same-holder transfers (self-transfer). This is different from `AssetGroup.Flexible` which is a wallet category. See [ENUMS_AND_TYPE_SYSTEM.md](../07_REFERENCE/ENUMS_AND_TYPE_SYSTEM.md) for naming clarification.

### CONVERSION Mode (Self-Conversion)

The CONVERSION mode is specifically designed for PokerManager self-conversion transactions that trigger **dual-balance impact**.

**Requirements:**
- One wallet must be `AssetGroup.Flexible` (conversion wallet)
- Other wallet must be `AssetGroup.PokerAssets`
- Both wallets belong to the same PokerManager
- `ConversionRate` must be set
- `BalanceAs` is typically set to BRL (AssetType.BrazilianReal)

**Result:**
- PokerAssets balance is affected by the transaction amount
- FiatAssets balance is affected by `Amount * ConversionRate`

See [TRANSACTION_BALANCE_IMPACT.md](./TRANSACTION_BALANCE_IMPACT.md) for detailed balance calculation formulas.

### Additional Transaction Types (Planned/Future)

| Mode | Code | Description | Flow |
|------|------|-------------|------|
| **SYSTEM_OPERATION** | `SYSTEM_OPERATION` | Transaction involving System Wallets | Entity ↔ System Wallet |

These modes will be explicitly tracked as the system evolves.

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

### Sync Bank Transaction Direction

When a digital transaction (SALE or PURCHASE) includes a sync bank transaction (counterpart fiat transaction), the fiat direction must be **mode-aware** to correctly reflect the BRL flow:

| Mode | Chip Flow | BRL Flow | Sync Fiat Direction |
|------|-----------|----------|---------------------|
| SALE | PokerManager → Client | Client → Bank | Client is fiat sender, Bank is fiat receiver |
| PURCHASE | Client → PokerManager | Bank → Client | Bank is fiat sender, Client is fiat receiver |

In **SALE mode**, the sync bank transaction uses the same fiat direction as the original (bank receives BRL from the client, who is paying for chips). In **PURCHASE mode**, the sync bank transaction reverses direction (bank sends BRL to the client, who is being paid for chips sold).

> **Note:** This mode-aware logic prevents the sync from creating incorrectly-directed counterpart fiat transactions. Without it, SALE mode would create a fiat transaction showing the client receiving BRL (RECEIPT) instead of paying BRL (PAYMENT).

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
- Asset type validation
- Balance validation (opt-in)
- Bank restriction for TRANSFER mode
- Explicit system-operation support (company wallets via null/Guid.Empty holder IDs)
- Transaction atomicity with rollback on errors

> **Note:** Automatic wallet creation via `CreateWalletsIfMissing` is **deprecated** as of January 2026. Wallets must be created explicitly before initiating a transfer.

**Workflow:**

1. Reject deprecated `CreateWalletsIfMissing` flag if true
2. Validate asset type is not None
3. Determine entity type (fiat vs digital)
4. Validate sender and receiver exist
5. Infer mode and apply bank restriction (if TRANSFER)
6. Check wallet existence (wallets must exist)
7. Validate category (if provided)
8. Get sender wallet
9. Get receiver wallet
10. Validate not same wallet
11. Validate balance (if requested)
12. Create appropriate transaction (Fiat or Digital)
13. Save and commit
14. Return response

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
| `ValidateNoBanksInTransferAsync` | Validates bank participation rules. Allows banks in RECEIPT/PAYMENT (fiat only), blocks banks in TRANSFER mode, and blocks bank-to-bank transfers. |
| `CheckWalletsExistAsync` | Checks if wallets exist and returns detailed error if missing. Skips check for system operations (null/Guid.Empty asset holder ID). |
| `GetAndValidateWalletAsync` | Validates provided wallet. For system operations (Guid.Empty), validates wallet has `null` BaseAssetHolderId. For regular operations, validates wallet belongs to specified asset holder. |
| `DetermineAccountClassificationAsync` | Determines ASSET vs LIABILITY classification |
| `GetBalanceForWalletAsync` | Calculates current wallet balance |
| `FindWalletByAssetHolderAndType` | Queries for wallet by holder and asset type |
| `GetAssetHolderInfoAsync` | Retrieves asset holder name and type |
| `GetAssetHolderTypeAsync` | Returns "Company" for Guid.Empty, or "Client"/"Member"/"Bank"/"PokerManager" based on entity lookup. Returns null if not found. |
| `GetAssetTypeName` | Converts AssetType enum to display name |

#### GetAndValidateWalletAsync Details

Validates that a provided wallet identifier:
1. Exists and is not soft-deleted
2. Belongs to the expected asset holder (or is a system wallet for system operations)
3. Matches the requested asset type

**System Operation Validation:**
- When `expectedAssetHolderId == Guid.Empty`, the wallet must have `null` `BaseAssetHolderId` (company-owned)
- If the wallet has a non-null `BaseAssetHolderId`, throws `WALLET_OWNERSHIP_MISMATCH`

**Regular Operation Validation:**
- The wallet's `AssetPool.BaseAssetHolderId` must match `expectedAssetHolderId`
- If mismatch, throws `WALLET_OWNERSHIP_MISMATCH`

#### GetAssetHolderTypeAsync Details

Determines the asset holder type string for a given asset holder ID.

**Return Values:**
- `"Company"` - When `assetHolderId == Guid.Empty` (system operations)
- `"Client"` - When asset holder is a Client entity
- `"Member"` - When asset holder is a Member entity
- `"Bank"` - When asset holder is a Bank entity
- `"PokerManager"` - When asset holder is a PokerManager entity
- `null` - When asset holder ID doesn't match any entity

### Error Codes

See `TransferErrorCodes` static class (`Application/DTOs/Transactions/TransferError.cs`) for complete list:

| Code | Description |
|------|-------------|
| `SENDER_NOT_FOUND` | Sender asset holder does not exist |
| `RECEIVER_NOT_FOUND` | Receiver asset holder does not exist |
| `SENDER_WALLET_NOT_FOUND` | Sender wallet not found for asset type |
| `RECEIVER_WALLET_NOT_FOUND` | Receiver wallet not found for asset type |
| `INSUFFICIENT_BALANCE` | Sender wallet has insufficient funds |
| `INVALID_ASSET_TYPE` | AssetType.None or invalid type |
| `ASSET_TYPE_MISMATCH` | Wallet asset type doesn't match request |
| `WALLET_OWNERSHIP_MISMATCH` | Wallet doesn't belong to expected holder |
| `SAME_SENDER_RECEIVER_WALLET` | Cannot transfer to same wallet |
| `BANK_NOT_ALLOWED_IN_TRANSFER` | Banks cannot participate in TRANSFER mode |
| `BANK_TO_BANK_NOT_ALLOWED` | Bank-to-bank transfers are not allowed |
| `WALLETS_REQUIRED` | Wallets must be created before transfer |
| `WALLETS_CREATION_DEPRECATED` | Automatic wallet creation no longer supported |
| `TRANSACTION_FAILED` | General transaction error |

---

## System Operations

The system supports transactions involving company-owned system wallets. These are wallets that belong to the company itself rather than a specific asset holder (Client, Member, Bank, or PokerManager).

### Identification

- **System wallets** have `null` `BaseAssetHolderId` in their `AssetPool`
- In `TransferRequest`, system operations are indicated by:
  - `SenderAssetHolderId` or `ReceiverAssetHolderId` set to `null` or `Guid.Empty`
  - The corresponding `SenderWalletIdentifierId` or `ReceiverWalletIdentifierId` must be provided

### Behavior

- System operations display **"Company"** as the asset holder name in responses
- `GetAssetHolderTypeAsync` returns "Company" for `Guid.Empty` asset holder IDs
- System wallets are validated to ensure they have `null` `BaseAssetHolderId`
- `CheckWalletsExistAsync` skips wallet existence check for system operations (when wallet ID is provided)

### Implementation Details

```csharp
// TransferService.cs
var senderAssetHolderId = request.SenderAssetHolderId.GetValueOrDefault();
var isSenderSystem = !request.SenderAssetHolderId.HasValue || request.SenderAssetHolderId.Value == Guid.Empty;
string senderName = "Company";

if (isSenderSystem)
{
    // System operation: sender is the company
    // Validate that a sender wallet is provided for system operations
    if (!request.SenderWalletIdentifierId.HasValue)
    {
        throw new BusinessException(
            "System operations require a specific sender wallet",
            TransferErrorCodes.SenderWalletNotFound);
    }
}
```

### Use Cases

- **Company Internal Transfers:** Moving assets between company wallets
- **System-level Adjustments:** Administrative balance corrections
- **Company Treasury Operations:** Company-side receipts and payments

### System Wallet Position by Transaction Mode

When a system operation is created, the system wallet's position (sender or receiver) must be correctly set to ensure proper balance impact on entity wallets (Bank, PokerManager).

> **Fixed (February 2026):** The frontend implementation in `SystemOperationCheck.tsx` was corrected to use the proper sender/receiver positions. The SQL migration `SYSTEM_WALLET_DIRECTION_MIGRATION.sql` was applied to fix existing data. The table below shows the current correct positions.

**Correct System Wallet Positions:**

The correct positions are determined by the accounting principle: transactions between Asset (entity) and Liability (system) wallets need the Liability side's sign to be inverted.

| Mode | System Wallet Position | Entity Position | Entity Balance Impact | Rationale |
|------|----------------------|-----------------|----------------------|-----------|
| **SALE** | Receiver | Sender | -Amount (expense) | Company sells = entity sends chips |
| **PURCHASE** | Sender | Receiver | +Amount (income) | Company buys = entity receives chips |
| **RECEIPT** | Sender | Receiver | +Amount (income) | Company receives = entity receives money |
| **PAYMENT** | Receiver | Sender | -Amount (expense) | Company pays = entity sends money |

**Understanding the Accounting Logic:**

1. **Entity wallets (Bank, PokerManager PokerAssets):** `AccountClassification = Asset`
2. **System wallets (company-owned Flexible):** `AccountClassification = Liability`

When sender and receiver have **different** AccountClassifications, the Liability wallet's sign is inverted:
- **System as Sender (Liability):** Base sign is -Amount, inverted to +Amount → Entity receives (income)
- **System as Receiver (Liability):** Base sign is +Amount, but Entity as sender keeps -Amount → Entity sends (expense)

**TRANSFER Mode (depends on operation type + asset type):**

| Operation | Asset Type | System Position | Entity Balance Impact |
|-----------|------------|-----------------|----------------------|
| Despesa (Expense) | Any | Receiver | -Amount |
| Receita (Income) | Any | Sender | +Amount |

**Fixed-Side Conflict Resolution:**

When the system wallet's mode-determined side conflicts with a fixed-side entity (the transaction creator), the system resolves by automatically swapping the creator to the opposite side.

---

## Transaction Guardrails

### Guardrail 1: Explicit Wallet Creation (TRANSFER Mode)

**Purpose:** Ensure wallets are created intentionally before transfers, preventing automatic wallet proliferation.

**Behavior (as of January 2026):**
- Wallets must exist before transfer
- If wallets missing → Returns 400 with `WALLETS_REQUIRED` error
- `CreateWalletsIfMissing` flag is **deprecated** - if true → Returns 400 with `WALLETS_CREATION_DEPRECATED`

**Implementation:**

```csharp
// TransferService.cs
if (request.CreateWalletsIfMissing)
{
    throw new BusinessException(
        "Automatic wallet creation is no longer supported. Create wallets explicitly before initiating transfer.",
        "WALLETS_CREATION_DEPRECATED");
}

var walletError = await CheckWalletsExistAsync(request);
if (walletError != null)
{
    throw new WalletMissingException(walletError);
}
```

**Error Responses:**

Wallets Missing:
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

Deprecated Flag Used:
```json
{
  "title": "Transfer Failed",
  "status": 400,
  "detail": "Automatic wallet creation is no longer supported. Create wallets explicitly before initiating transfer.",
  "extensions": {
    "errorCode": "WALLETS_CREATION_DEPRECATED"
  }
}
```

**Frontend Flow (TRANSFER mode):**
1. User selects entity
2. If no wallet exists → "Criar Carteira" button appears
3. User clicks → inline preview shows wallet details
4. User clicks "Confirmar Criação" → confirmation dialog
5. User confirms → wallet created via wallet API
6. Wallet auto-selected in form
7. User completes transfer

### Guardrail 2: TRANSFER Mode Restrictions

**Purpose:** Enforce business rules for TRANSFER mode transactions.

**Rules:**
- **Bank Restriction:** Banks cannot participate in TRANSFER mode
- **AssetGroup Restriction (Frontend Guardrail):** UI limits TRANSFER mode to **Flexible wallets** (AssetGroup 4)
- **INTERNAL mode** (same asset holder): No restrictions

**AssetGroup Restriction (Frontend):**

| AssetGroup | TRANSFER Mode | Use Instead |
|------------|---------------|-------------|
| FiatAssets (1) | ❌ Not allowed | RECEIPT/PAYMENT with Bank |
| PokerAssets (2) | ❌ Not allowed | SALE/PURCHASE with PokerManager |
| CryptoAssets (3) | ❌ Not allowed | Crypto flow (future) |
| **Flexible (4)** | ✅ Allowed | P2P transfers |

**Bank Restriction (Backend):**

The `ValidateNoBanksInTransferAsync` method implements conditional bank validation:

```csharp
// Mode inference (handles nullable asset holder IDs for system operations)
var senderId = request.SenderAssetHolderId.GetValueOrDefault();
var receiverId = request.ReceiverAssetHolderId.GetValueOrDefault();
var isInternalTransfer = senderId == receiverId && senderId != Guid.Empty;

if (!isInternalTransfer)  // TRANSFER mode (or RECEIPT/PAYMENT)
{
    // ValidateNoBanksInTransferAsync allows banks in RECEIPT/PAYMENT modes (fiat only)
    await ValidateNoBanksInTransferAsync(request);
}
```

**Bank Validation Logic:**

1. **RECEIPT/PAYMENT Modes (Fiat Assets Only):**
   - ✅ **RECEIPT**: Non-bank → Bank (fiat assets) - **ALLOWED**
   - ✅ **PAYMENT**: Bank → Non-bank (fiat assets) - **ALLOWED**
   - ❌ **Bank-to-Bank**: Bank → Bank - **BLOCKED** (throws `BANK_TO_BANK_NOT_ALLOWED`)

2. **TRANSFER Mode:**
   - ❌ Banks cannot participate in TRANSFER mode transactions
   - Error message guides users to use RECEIPT/PAYMENT modes instead

3. **INTERNAL Mode:**
   - ✅ Banks allowed (no restrictions for same-holder transfers)

**Validation Matrix:**

| Mode | Bank as Sender | Bank as Receiver | Asset Type | Result | Enforced By |
|------|----------------|------------------|------------|--------|-------------|
| SALE | N/A (PokerManager) | ❌ Blocked | Digital | ❌ Blocked | Frontend |
| PURCHASE | ❌ Blocked | N/A (PokerManager) | Digital | ❌ Blocked | Frontend |
| RECEIPT | ❌ Blocked | ✅ Allowed | **Fiat only** | ✅ Allowed | **TransferService** |
| PAYMENT | ✅ Allowed | ❌ Blocked | **Fiat only** | ✅ Allowed | **TransferService** |
| RECEIPT/PAYMENT | ✅ Bank | ✅ Bank | Fiat | ❌ Blocked (Bank-to-Bank) | **TransferService** |
| TRANSFER | ❌ Blocked | ❌ Blocked | Any | ❌ Blocked | **TransferService** |
| INTERNAL | ✅ Allowed | ✅ Allowed | Any | ✅ Allowed | No restriction |

**Key Points:**
- Banks can **only** participate in RECEIPT/PAYMENT modes with **fiat assets** (BRL, USD, etc.)
- Bank-to-bank transfers are explicitly blocked
- TRANSFER mode does not allow banks (use RECEIPT/PAYMENT instead)
- The validation checks asset type to determine if bank participation is allowed

**Error Responses:**

Bank not allowed in transfer:
```json
{
  "title": "Transfer Failed",
  "status": 400,
  "detail": "Banks can only send fiat assets (BRL). Use Payment mode for fiat transactions.",
  "extensions": {
    "errorCode": "BANK_NOT_ALLOWED_IN_TRANSFER"
  }
}
```

Bank-to-bank not allowed:
```json
{
  "title": "Transfer Failed",
  "status": 400,
  "detail": "Bank-to-bank transfers are not allowed.",
  "extensions": {
    "errorCode": "BANK_TO_BANK_NOT_ALLOWED"
  }
}
```

---

## Transaction Update (PATCH) Support

Transactions support partial updates via PATCH endpoints, allowing modification of specific fields without replacing the entire resource.

### How It Works

1. **Update Validators** ensure only modifiable fields can be changed. Fields like `SenderWalletIdentifierId`, `ReceiverWalletIdentifierId`, and `AssetAmount` may have restrictions depending on the transaction type and its current state.
2. **The Update Service** handles recalculation of affected balances after a transaction is modified. When a field that impacts balances is changed (e.g., `AssetAmount`, `ConversionRate`, `RakeAmount`), the service recalculates the relevant wallet balances to keep the system consistent.
3. **Concurrency** is managed through standard EF Core optimistic concurrency, ensuring that simultaneous updates to the same transaction are detected and handled.

### Update Flow

```
PATCH /api/v1/{transactionType}/{id}
  → Validate modifiable fields
  → Apply partial update
  → Recalculate affected balances
  → Save and return updated transaction
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

## PokerManager Self-Conversion

PokerManager self-conversion transactions allow a PokerManager to move chips from a personal/external position into the managed system (or out of it) while impacting **both** PokerAssets and FiatAssets balances.

### Self-Conversion via Transfer Endpoint

Self-conversion allows a poker manager to convert between asset types within the same account. It is implemented via the **Transfer endpoint** (`POST /api/v1/transfer`) with the sender and receiver being the **same entity** (same `AssetHolderId`). This triggers `INTERNAL` mode detection, and when the wallet pair meets the conversion conditions (Flexible ↔ PokerAssets with `BalanceAs` and `ConversionRate` set), it is treated as a CONVERSION operation with dual-balance impact. This mechanism is commonly used for converting between different poker platform balances held by the same manager.

### Business Context

The PokerManager acts as an **intermediary/bank** in the poker management business:
- They hold poker chips **on behalf of the company**
- They facilitate conversions between chips and fiat currency
- When a **client** sends chips, the company owes the **client** money
- When the **manager themselves** sends chips (from their own holdings), the company owes the **manager** money

**Normal Client Transaction:**
```
Client sells 1000 chips to PokerManager:
- Client PokerAssets: -1000 (sent chips)
- Client FiatAssets: +5000 (owed by company)
- PokerManager PokerAssets: +1000 (holding for company)
```

**Manager Self-Conversion:**
```
PokerManager deposits own 1000 chips:
- PokerManager PokerAssets: +1000 (holding for company)
- PokerManager FiatAssets: +5000 (owed by company)
```

The **Flexible wallet** represents the manager's "external" or "personal" position that exists outside the managed system. When chips flow from Flexible to PokerAssets, they are entering the managed system.

### Trigger Conditions

A `DigitalAssetTransaction` is treated as self-conversion when **all** conditions are met:

1. Both sender and receiver wallets belong to the same PokerManager
2. One wallet is `AssetGroup.Flexible` and the other is `AssetGroup.PokerAssets`
3. `BalanceAs` is set
4. `ConversionRate` is set

### Balance Impact

| Direction | PokerAssets | FiatAssets | Meaning |
|----------|-------------|-----------|---------|
| Flexible → PokerAssets | +AssetAmount | +AssetAmount × ConversionRate | Chips entering the system |
| PokerAssets → Flexible | -AssetAmount | -AssetAmount × ConversionRate | Chips leaving the system |

### Settlement

The FiatAssets balance created by self-conversion is settled through normal `FiatAssetTransaction` flows, just like client balances. For example, if the manager deposited 1000 chips at 5.0 conversion rate:
- FiatAssets balance: +5000 BRL
- Later: Company pays manager via `FiatAssetTransaction`
- FiatAssets balance: 0 BRL (settled)

> **Note:** The self-conversion logic is implemented in `BaseAssetHolderService.GetBalancesByAssetGroup` and is tied to the Flexible wallet trigger.

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

## Known Issues

### Settlement Rake Commission in Finance Report (Planilha)

> **Status:** Deferred — requires investigation

There is an outstanding investigation needed for how settlement rake commissions are reflected in the finance report (planilha) balance calculations. The closings page correctly displays rake totals and profit, but the poker manager's balance in the planilha may show zero commission impact. Suspected areas include settlement wallet ownership, `AssetPool.BaseAssetHolderId` resolution, and the wallet identifier matching logic in `GetBalancesByAssetGroup`.

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
