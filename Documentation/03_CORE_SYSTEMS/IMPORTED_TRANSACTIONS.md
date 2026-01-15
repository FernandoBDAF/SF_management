# Imported Transactions System

## Overview

The Imported Transactions system handles the import and reconciliation of financial transactions from external sources such as bank statements (OFX files) and Excel spreadsheets. This system enables tracking of transactions from their original source through to their reconciliation with the system's transaction records.

---

## Table of Contents

1. [Core Concepts](#core-concepts)
2. [Data Model](#data-model)
3. [Import Workflow](#import-workflow)
4. [Reconciliation Process](#reconciliation-process)
5. [File Formats](#file-formats)
6. [API Endpoints](#api-endpoints)
7. [Error Handling](#error-handling)

---

## Core Concepts

### Import vs Reconciliation

| Phase | Description |
|-------|-------------|
| **Import** | Reading external files and creating `ImportedTransaction` records |
| **Reconciliation** | Matching imported transactions with existing `BaseTransaction` records |

### Transaction Lifecycle

```
External File → Import → ImportedTransaction (Pending/Processed)
                              ↓
                         Find Matches
                              ↓
                         Reconcile
                              ↓
                    ImportedTransaction (Reconciled)
                              ↓
                    Linked to BaseTransaction
```

### Status Flow

```
┌─────────┐     ┌────────────┐     ┌───────────┐     ┌────────────┐
│ Pending │────▶│ Processing │────▶│ Processed │────▶│ Reconciled │
└─────────┘     └────────────┘     └───────────┘     └────────────┘
                      │                   │
                      ▼                   ▼
                 ┌────────┐        ┌──────────────┐
                 │ Failed │        │RequiresReview│
                 └────────┘        └──────────────┘
                                          │
                                          ▼
                                    ┌─────────┐
                                    │ Ignored │
                                    └─────────┘
```

---

## Data Model

### ImportedTransaction Entity

**File**: `Models/Transactions/ImportedTransaction.cs`

```csharp
public class ImportedTransaction : BaseDomain
{
    // Transaction data
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public string? ExternalReferenceId { get; set; }  // FitId for OFX
    
    // Source tracking
    public Guid BaseAssetHolderId { get; set; }
    public ImportFileType FileType { get; set; }
    public string FileName { get; set; }
    public string? FileHash { get; set; }
    public long FileSizeBytes { get; set; }
    public string? FileMetadata { get; set; }  // JSON
    
    // Processing status
    public ImportedTransactionStatus Status { get; set; }
    public DateTime? ProcessedAt { get; set; }
    
    // Reconciliation
    public ReconciledTransactionType? ReconciledTransactionType { get; set; }
    public Guid? ReconciledTransactionId { get; set; }
    public DateTime? ReconciledAt { get; set; }
    
    // Helper property
    public bool IsReconciled => ReconciledTransactionId.HasValue;
}
```

### Key Properties

| Property | Description |
|----------|-------------|
| `ExternalReferenceId` | Unique ID from source (OFX FitId, generated for Excel) |
| `FileHash` | SHA256 hash for duplicate file detection |
| `FileMetadata` | JSON with source-specific details |
| `ReconciledTransactionId` | Links to FiatAssetTransaction, DigitalAssetTransaction, or SettlementTransaction |

---

## Import Workflow

### OFX Import (Bank Statements)

```csharp
public async Task<List<ImportedTransaction>> ImportOfxFile(
    IFormFile file, 
    Guid baseAssetHolderId)
```

**Process:**
1. Validate BaseAssetHolder is a Bank
2. Calculate file SHA256 hash
3. Parse OFX XML content
4. Check for duplicates using `ExternalReferenceId` (FitId)
5. Create `ImportedTransaction` records
6. Set status to `Processed`

**OFX Parsing:**
```csharp
// Tags extracted from OFX
<STMTTRN>       // Transaction start
<TRNTYPE>       // Transaction type
<DTPOSTED>      // Date (yyyyMMdd format)
<TRNAMT>        // Amount
<FITID>         // Unique transaction ID
<MEMO>          // Description
```

### Excel Import (Poker Transactions)

```csharp
public async Task<List<ImportedTransaction>> ImportExcelFile(
    IFormFile file, 
    Guid baseAssetHolderId, 
    ExcelImportType importType,
    List<(int Column, string Name)> columnMapping)
```

**Import Types:**

| Type | Description | Default Columns |
|------|-------------|-----------------|
| `BuyTransactions` | Purchase from players | WalletIdentifier, Value, AssetPool, CreatedAt, Description |
| `SellTransactions` | Sale to players | WalletIdentifier, Value, AssetPool, CreatedAt, Description |
| `TransferTransactions` | Between accounts | From, To, CreatedAt, Value, Description |

**Process:**
1. Validate BaseAssetHolder is a PokerManager
2. Calculate file hash
3. Read Excel using EPPlus library
4. Generate unique `ExternalReferenceId` for each row
5. Check for duplicates
6. Create `ImportedTransaction` records

---

## Reconciliation Process

### Finding Potential Matches

```csharp
public async Task<List<(ReconciledTransactionType Type, Guid Id, DateTime Date, decimal Amount)>> 
    FindPotentialMatches(
        Guid importedTransactionId, 
        int daysTolerance = 3,
        decimal amountTolerance = 0.01m)
```

**Matching Algorithm:**
1. Search within date range (±3 days default)
2. Search within amount tolerance (±0.01 default)
3. Search across all transaction types (Fiat, Digital, Settlement)
4. Return sorted by date proximity, then amount difference

### Performing Reconciliation

```csharp
public async Task<ImportedTransaction> ReconcileTransaction(
    Guid importedTransactionId, 
    Guid baseTransactionId, 
    ReconciledTransactionType transactionType,
    string? notes = null)
```

**Process:**
1. Validate imported transaction exists
2. Validate base transaction exists (based on type)
3. Check not already reconciled
4. Update reconciliation fields
5. Set status to `Reconciled`

---

## File Formats

### OFX (Open Financial Exchange)

Standard bank statement format:

```xml
<STMTTRN>
  <TRNTYPE>DEBIT
  <DTPOSTED>20250114
  <TRNAMT>-1500.00
  <FITID>2025011400001
  <MEMO>PIX Transfer to John
</STMTTRN>
```

### Excel Spreadsheet

**Buy/Sell Transactions:**
| Column | Field |
|--------|-------|
| 1 | WalletIdentifier (player nickname/ID) |
| 2 | Value (transaction amount) |
| 3 | AssetPool (poker site) |
| 6 | CreatedAt (date/time) |
| 8 | Description |

**Transfer Transactions:**
| Column | Field |
|--------|-------|
| 1 | From (source identifier) |
| 2 | To (destination identifier) |
| 3 | CreatedAt (date/time) |
| 4 | Value (amount) |
| 5 | Description |

---

## API Endpoints

### Import Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/importedtransaction/import/ofx` | Import OFX file |
| POST | `/api/v1/importedtransaction/import/excel` | Import Excel file |
| POST | `/api/v1/importedtransaction/import/excel/buy` | Import buy transactions |
| POST | `/api/v1/importedtransaction/import/excel/sell` | Import sell transactions |
| POST | `/api/v1/importedtransaction/import/excel/transfer` | Import transfers |

### Query Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/importedtransaction` | List all imported transactions |
| GET | `/api/v1/importedtransaction/{id}` | Get by ID |
| GET | `/api/v1/importedtransaction/file/{fileName}/asset-holder/{id}` | Get by file |
| GET | `/api/v1/importedtransaction/unreconciled/asset-holder/{id}` | Get unreconciled |
| GET | `/api/v1/importedtransaction/dashboard/asset-holder/{id}` | Dashboard summary |

### Reconciliation Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/importedtransaction/reconcile` | Reconcile transaction |
| POST | `/api/v1/importedtransaction/find-matches` | Find potential matches |

---

## Error Handling

### Business Exceptions

| Code | Message | When |
|------|---------|------|
| `INVALID_ASSET_HOLDER` | BaseAssetHolder not found or wrong type | Invalid holder ID or type mismatch |
| `NO_NEW_TRANSACTIONS` | No new transactions found in file | All transactions are duplicates |
| `NO_PROCESSABLE_TRANSACTIONS` | No transactions could be processed | All rows failed processing |
| `ALREADY_RECONCILED` | Transaction already reconciled | Attempting to re-reconcile |
| `INVALID_TRANSACTION_TYPE` | Invalid transaction type | Unknown reconciliation type |

### Validation Rules

1. **OFX Import**: BaseAssetHolder must be a Bank
2. **Excel Import**: BaseAssetHolder must be a PokerManager
3. **Duplicate Detection**: Uses `ExternalReferenceId` to prevent duplicate imports
4. **Reconciliation**: Transaction must not already be reconciled

---

## Usage Examples

### Importing OFX File

```csharp
// Request (multipart/form-data)
var content = new MultipartFormDataContent();
content.Add(new StreamContent(ofxFile), "File", "statement.ofx");
content.Add(new StringContent(bankId.ToString()), "BaseAssetHolderId");

var response = await httpClient.PostAsync("/api/v1/importedtransaction/import/ofx", content);
```

### Importing Excel File

```csharp
// Request (multipart/form-data)
var content = new MultipartFormDataContent();
content.Add(new StreamContent(excelFile), "File", "transactions.xlsx");
content.Add(new StringContent(pokerManagerId.ToString()), "BaseAssetHolderId");
content.Add(new StringContent("1"), "ImportType"); // BuyTransactions

var response = await httpClient.PostAsync("/api/v1/importedtransaction/import/excel/buy", content);
```

### Finding and Reconciling

```csharp
// 1. Find matches
var findMatchesRequest = new FindMatchesRequest
{
    ImportedTransactionId = importedTxId,
    DaysTolerance = 3,
    AmountTolerance = 0.01m
};

var matches = await httpClient.PostAsJsonAsync("/api/v1/importedtransaction/find-matches", findMatchesRequest);

// 2. Reconcile with best match
var reconcileRequest = new ReconcileTransactionRequest
{
    ImportedTransactionId = importedTxId,
    BaseTransactionId = bestMatchId,
    TransactionType = ReconciledTransactionType.Fiat,
    Notes = "Auto-matched"
};

await httpClient.PostAsJsonAsync("/api/v1/importedtransaction/reconcile", reconcileRequest);
```

---

## Related Documentation

- [TRANSACTION_INFRASTRUCTURE.md](TRANSACTION_INFRASTRUCTURE.md) - Transaction types
- [ENUMS_AND_TYPE_SYSTEM.md](../07_REFERENCE/ENUMS_AND_TYPE_SYSTEM.md) - Import enums
- [API_REFERENCE.md](../06_API/API_REFERENCE.md) - Complete endpoint documentation

