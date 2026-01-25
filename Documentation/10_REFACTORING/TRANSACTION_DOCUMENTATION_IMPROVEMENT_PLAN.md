# Transaction Documentation Improvement Plan

> **Created:** January 22, 2026  
> **Status:** ✅ Completed (with subsequent updates)  
> **Scope:** Backend transaction documentation enrichment

---

> ⚠️ **HISTORICAL DOCUMENT:** This plan was completed in January 2026. Some content is now outdated:
> 
> **Changes since completion:**
> - `CreateWalletsIfMissing` flag is **deprecated** - automatic wallet creation removed
> - TRANSFER mode now restricted to **Internal wallets only** (AssetGroup 4)
> - Guardrail 1 replaced with explicit wallet creation flow
> 
> For current documentation, see [TRANSACTION_INFRASTRUCTURE.md](../03_CORE_SYSTEMS/TRANSACTION_INFRASTRUCTURE.md).

---

## Overview

This plan enriches the backend transaction documentation to reflect the complete transaction system including the new unified Transfer endpoint, transaction modes, business flows, and the recently implemented guardrails (wallet creation confirmation and bank restrictions).

---

## Current State Analysis

### Existing Documentation

| Document | Location | Content | Gaps |
|----------|----------|---------|------|
| **TRANSACTION_INFRASTRUCTURE.md** | `03_CORE_SYSTEMS/` | Entity models, BaseTransaction helpers | Missing: Transaction modes, TransferService, Guardrails |
| **TRANSACTION_RESPONSE_VIEWMODELS.md** | `03_CORE_SYSTEMS/` | Response DTOs for API | Missing: TransferResponse, WalletMissingError |
| **IMPORTED_TRANSACTIONS.md** | `03_CORE_SYSTEMS/` | OFX/Excel import system | Complete ✅ |
| **SETTLEMENT_WORKFLOW.md** | `03_CORE_SYSTEMS/` | Poker settlements | Complete ✅ |
| **API_REFERENCE.md** | `06_API/` | Basic endpoint list | Missing: Transfer endpoint, Detailed examples |

### What's Missing

| Gap | Impact | Priority |
|-----|--------|----------|
| Transaction Modes conceptual model | Developers don't understand business context | 🔴 High |
| TransferService documentation | New service not documented | 🔴 High |
| Transfer endpoint API reference | New endpoint not documented | 🔴 High |
| Guardrails documentation | Security features not documented | 🟡 Medium |
| Transaction flow diagrams | Visual understanding missing | 🟡 Medium |
| Service layer methods | Business logic not documented | 🟡 Medium |

---

## Improvement Plan

### 1. Update TRANSACTION_INFRASTRUCTURE.md

**Add new sections:**

#### Section A: Transaction Modes and Business Flows

```markdown
## Transaction Modes

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
```

#### Section B: TransferService Documentation

```markdown
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

See `TransferErrorCodes` static class for complete list:
- `SENDER_NOT_FOUND`
- `RECEIVER_NOT_FOUND`
- `SENDER_WALLET_NOT_FOUND`
- `RECEIVER_WALLET_NOT_FOUND`
- `WALLET_CREATION_FAILED`
- `INSUFFICIENT_BALANCE`
- `INVALID_ASSET_TYPE`
- `ASSET_TYPE_MISMATCH`
- `WALLET_OWNERSHIP_MISMATCH`
- `SAME_SENDER_RECEIVER_WALLET`
- `BANK_NOT_ALLOWED_IN_TRANSFER`
- `WALLETS_REQUIRED`
- `TRANSACTION_FAILED`
```

#### Section C: Guardrails Documentation

```markdown
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
  "errorCode": "WALLETS_REQUIRED",
  "walletDetails": {
    "senderWalletMissing": true,
    "senderAssetHolderName": "Client João Silva",
    "senderAssetHolderType": "Client",
    "senderAssetTypeName": "BrazilianReal",
    "receiverWalletMissing": false
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
  "errorCode": "BANK_NOT_ALLOWED_IN_TRANSFER",
  "detail": "Banks cannot be the sender in a transfer. Use Payment mode instead."
}
```
```

---

### 2. Create TRANSACTION_API_ENDPOINTS.md

**New file:** `Documentation/06_API/TRANSACTION_API_ENDPOINTS.md`

**Purpose:** Comprehensive reference for all transaction-related API endpoints with detailed examples, business rules, and validation.

**Table of Contents:**
```markdown
# Transaction API Endpoints

## Overview
## Transfer Endpoint (Unified)
## Fiat Asset Transaction Endpoints
## Digital Asset Transaction Endpoints
## Settlement Transaction Endpoints
## Deprecated Endpoints
## Common Request Patterns
## Error Handling
## Security and Authorization
```

**Key Sections:**

#### Transfer Endpoint Documentation

```markdown
### POST /api/v1/transfer

Creates a transfer between any two asset holders, supporting both Fiat and Digital assets.

**Request Body:**
```json
{
  "senderAssetHolderId": "10828ea8-f3eb-4f0c-492d-08ddc54a1a1b",
  "receiverAssetHolderId": "065fce58-b38b-4003-492f-08ddc54a1a1b",
  "senderWalletIdentifierId": "fa3f2bc0-3e33-473e-949d-08ddc621ef17",  // Optional
  "receiverWalletIdentifierId": "434d3633-3d85-47b2-8953-08de554e0b11", // Optional
  "assetType": 21,  // BrazilianReal
  "amount": 1000.00,
  "date": "2026-01-22T10:00:00Z",
  "categoryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",  // Optional
  "description": "Payment for services",  // Optional
  "createWalletsIfMissing": false,  // Default: false
  "autoApprove": false,  // Default: false
  "validateBalance": false  // Default: false
}
```

**Success Response (200 OK):**
```json
{
  "transactionId": "5fa85f64-5717-4562-b3fc-2c963f66afa6",
  "entityType": "fiat",
  "assetType": "BrazilianReal",
  "senderWalletIdentifierId": "fa3f2bc0-3e33-473e-949d-08ddc621ef17",
  "senderAssetHolderId": "10828ea8-f3eb-4f0c-492d-08ddc54a1a1b",
  "senderName": "Client João Silva",
  "receiverWalletIdentifierId": "434d3633-3d85-47b2-8953-08de554e0b11",
  "receiverAssetHolderId": "065fce58-b38b-4003-492f-08ddc54a1a1b",
  "receiverName": "Member Maria Santos",
  "amount": 1000.00,
  "date": "2026-01-22T10:00:00Z",
  "isInternalTransfer": false,
  "isApproved": false,
  "createdAt": "2026-01-22T10:05:00Z",
  "senderWalletCreated": false,
  "receiverWalletCreated": false
}
```

**Business Rules:**
1. **Mode Inference:**
   - If `SenderAssetHolderId == ReceiverAssetHolderId` → INTERNAL mode
   - If `SenderAssetHolderId != ReceiverAssetHolderId` → TRANSFER mode

2. **Bank Restrictions:**
   - TRANSFER mode: Banks cannot be sender or receiver
   - INTERNAL mode: Banks allowed (for internal movements)

3. **Wallet Creation:**
   - Default `CreateWalletsIfMissing = false` (requires confirmation)
   - If wallets missing and flag is false → Returns 400 with `WALLETS_REQUIRED`
   - If wallets missing and flag is true → Creates wallets and proceeds

4. **Asset Type Determination:**
   - Fiat assets (21-22) → Creates `FiatAssetTransaction`
   - Digital assets (101+) → Creates `DigitalAssetTransaction`

5. **Validation:**
   - Sender and receiver must exist
   - Wallets must belong to specified asset holders
   - Wallets must match requested asset type
   - Cannot transfer to the same wallet
   - Balance validation only if `ValidateBalance = true`

**Error Responses:**

**Wallets Required (400):**
```json
{
  "title": "Wallet Creation Required",
  "status": 400,
  "errorCode": "WALLETS_REQUIRED",
  "walletDetails": {
    "code": "WALLETS_REQUIRED",
    "message": "One or more wallets need to be created to complete this transfer.",
    "senderWalletMissing": true,
    "senderAssetHolderId": "10828ea8-f3eb-4f0c-492d-08ddc54a1a1b",
    "senderAssetHolderName": "Client João Silva",
    "senderAssetHolderType": "Client",
    "senderAssetTypeName": "BrazilianReal",
    "receiverWalletMissing": false
  }
}
```

**Bank Not Allowed (400):**
```json
{
  "title": "Transfer Failed",
  "status": 400,
  "errorCode": "BANK_NOT_ALLOWED_IN_TRANSFER",
  "detail": "Banks cannot be the sender in a transfer. Use Payment mode instead."
}
```

**Use Cases:**

1. **Client sends BRL to Member:**
```bash
POST /api/v1/transfer
{
  "senderAssetHolderId": "client-guid",
  "receiverAssetHolderId": "member-guid",
  "assetType": 21,  # BRL
  "amount": 500.00,
  "date": "2026-01-22T10:00:00Z"
}
# → Creates FiatAssetTransaction
# → isInternalTransfer: false (TRANSFER mode)
```

2. **Client moves BRL between own wallets:**
```bash
POST /api/v1/transfer
{
  "senderAssetHolderId": "client-guid",
  "receiverAssetHolderId": "client-guid",  # Same holder
  "senderWalletIdentifierId": "bank-wallet-guid",
  "receiverWalletIdentifierId": "internal-wallet-guid",
  "assetType": 21,  # BRL
  "amount": 1000.00,
  "date": "2026-01-22T10:00:00Z"
}
# → Creates FiatAssetTransaction
# → isInternalTransfer: true (INTERNAL mode)
```

3. **PokerManager sends chips to Member:**
```bash
POST /api/v1/transfer
{
  "senderAssetHolderId": "pokermanager-guid",
  "receiverAssetHolderId": "member-guid",
  "assetType": 101,  # PokerStars
  "amount": 5000.00,
  "date": "2026-01-22T10:00:00Z",
  "balanceAs": 21,  # Record as BRL
  "conversionRate": 1.05,
  "rate": 5.0
}
# → Creates DigitalAssetTransaction
# → isInternalTransfer: false (TRANSFER mode)
```

### GET /api/v1/transfer/{id}

Retrieves a transfer transaction by ID.

**Query Parameters:**
- `entityType` (string, optional): "fiat" or "digital" (default: "fiat")

**Response (200 OK):**
Same as POST response, but `senderWalletCreated` and `receiverWalletCreated` are always false (creation state not stored).

**Error Response (404):**
```json
{
  "title": "Not Found",
  "status": 404,
  "detail": "Transfer '5fa85f64-5717-4562-b3fc-2c963f66afa6' not found"
}
```
```

---

### 3. Update TRANSACTION_RESPONSE_VIEWMODELS.md

**Add new section:**

```markdown
## Transfer Response

### TransferResponse DTO

Used by the unified transfer endpoint to return transaction results.

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
- **EntityType**: Indicates whether FiatAssetTransaction or DigitalAssetTransaction was created
- **IsInternalTransfer**: Computed from sender/receiver asset holder comparison
- **Wallet Creation Flags**: Only populated on POST, always false on GET

### WalletMissingError DTO

Provides detailed information when wallet creation is required.

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

**Usage:**
Thrown as part of `WalletMissingException`, serialized in `ProblemDetails.Extensions["walletDetails"]`.
```

---

### 4. Update API_REFERENCE.md

**Add to Transaction Controllers section:**

```markdown
### Transfer (Unified Transfer Endpoint)

**Base Route**: `/api/v1/transfer`

**Purpose**: Unified endpoint for all P2P transfers between asset holders.

**Controller**: `TransferController`

**Authorization**: Requires `Permission:create:transactions` for POST, `Permission:read:transactions` for GET

#### Endpoints

| Method | Route | Description | Request | Response |
|--------|-------|-------------|---------|----------|
| POST | `/` | Create transfer | `TransferRequest` | `TransferResponse` |
| GET | `/{id}` | Get transfer by ID | Query: `entityType` | `TransferResponse` |

#### Deprecated Endpoints (Use /transfer Instead)

These endpoints are marked `[Obsolete]` and will be removed in v2:

| Controller | Route | Replacement |
|------------|-------|-------------|
| ClientController | `POST /{id}/send-brazilian-real` | `POST /api/v1/transfer` |
| MemberController | `POST /{id}/send-brazilian-real` | `POST /api/v1/transfer` |
| PokerManagerController | `POST /{id}/send-brazilian-real` | `POST /api/v1/transfer` |

**Migration Guide:**

Old:
```http
POST /api/v1/client/{clientId}/send-brazilian-real
{
  "receiverId": "member-guid",
  "amount": 1000.00,
  "date": "2026-01-22"
}
```

New:
```http
POST /api/v1/transfer
{
  "senderAssetHolderId": "client-guid",
  "receiverAssetHolderId": "member-guid",
  "assetType": 21,
  "amount": 1000.00,
  "date": "2026-01-22"
}
```

For detailed documentation, see [TRANSACTION_API_ENDPOINTS.md](./TRANSACTION_API_ENDPOINTS.md).
```

---

### 5. Update Documentation Index

**File:** `Documentation/00_DOCUMENTATION_INDEX.md`

Add to 03_CORE_SYSTEMS section:
- Update TRANSACTION_INFRASTRUCTURE.md description to mention modes and guardrails
- Update TRANSACTION_RESPONSE_VIEWMODELS.md description to mention TransferResponse

Add to 06_API section:
- New entry for TRANSACTION_API_ENDPOINTS.md

Add to 10_REFACTORING section:
- Keep TRANSFER_ENDPOINT_IMPLEMENTATION_PLAN.md as historical reference
- Add TRANSACTION_GUARDRAILS_IMPLEMENTATION.md

Update totals:
```markdown
| Category | Count |
|----------|-------|
| Core Systems | 8 | (was 7, added TRANSACTION_API_ENDPOINTS)
| Refactoring | 4 | (was 2, added 2 implementation plans)
| **Total** | **36** | (was 32)
```

---

## Implementation Tasks

### Task 1: Update TRANSACTION_INFRASTRUCTURE.md ✅
- [x] Add "Transaction Modes and Business Flows" section
- [x] Add "TransferService" section
- [x] Add "Transaction Guardrails" section
- [x] Update "Related Documentation" with new links

### Task 2: Create TRANSACTION_API_ENDPOINTS.md ✅
- [x] Create file in `06_API/`
- [x] Document Transfer endpoint with examples
- [x] Document all transaction endpoints
- [x] Add migration guide from deprecated endpoints
- [x] Include error responses and validation rules

### Task 3: Update TRANSACTION_RESPONSE_VIEWMODELS.md ✅
- [x] Add TransferResponse documentation
- [x] Add WalletMissingError documentation
- [x] Update AutoMapper notes for new DTOs

### Task 4: Update API_REFERENCE.md ✅
- [x] Add Transfer controller section
- [x] Add deprecation notice for old endpoints
- [x] Link to TRANSACTION_API_ENDPOINTS.md

### Task 5: Update Documentation Index ✅
- [x] Update document descriptions
- [x] Add new files to index
- [x] Update document counts
- [x] Update related links

---

## Files to Create/Modify

| Action | File | Description |
|--------|------|-------------|
| **Create** | `06_API/TRANSACTION_API_ENDPOINTS.md` | Comprehensive transaction API reference |
| **Modify** | `03_CORE_SYSTEMS/TRANSACTION_INFRASTRUCTURE.md` | Add modes, TransferService, guardrails |
| **Modify** | `03_CORE_SYSTEMS/TRANSACTION_RESPONSE_VIEWMODELS.md` | Add TransferResponse, WalletMissingError |
| **Modify** | `06_API/API_REFERENCE.md` | Add Transfer section, deprecation notices |
| **Modify** | `00_DOCUMENTATION_INDEX.md` | Update descriptions and counts |

---

## Success Criteria

- [x] All transaction modes are documented with business context
- [x] TransferService is fully documented with method descriptions
- [x] Transfer endpoint has complete API reference with examples
- [x] Guardrails (wallet creation + bank restriction) are explained
- [x] TransferResponse and WalletMissingError DTOs are documented
- [x] Deprecated endpoints have clear migration path
- [x] All cross-references are updated
- [x] Documentation index reflects new structure

---

## Document Priority

| Document | Priority | Reason |
|----------|----------|--------|
| TRANSACTION_API_ENDPOINTS.md | 🔴 High | New endpoint needs API reference |
| TRANSACTION_INFRASTRUCTURE.md | 🔴 High | Core system understanding |
| TRANSACTION_RESPONSE_VIEWMODELS.md | 🟡 Medium | Completes DTO documentation |
| API_REFERENCE.md | 🟡 Medium | High-level index update |
| Documentation Index | 🟢 Low | Housekeeping |

---

*Ready for implementation*

