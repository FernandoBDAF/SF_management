# Transaction API Endpoints

## Table of Contents

- [Overview](#overview)
- [Transfer Endpoint (Unified)](#transfer-endpoint-unified)
  - [POST /transfer](#post-apiv1transfer)
  - [GET /transfer/{id}](#get-apiv1transferid)
- [Fiat Asset Transaction Endpoints](#fiat-asset-transaction-endpoints)
- [Digital Asset Transaction Endpoints](#digital-asset-transaction-endpoints)
- [Settlement Transaction Endpoints](#settlement-transaction-endpoints)
- [Deprecated Endpoints](#deprecated-endpoints)
- [Common Request Patterns](#common-request-patterns)
- [Error Handling](#error-handling)
- [Security and Authorization](#security-and-authorization)

---

## Overview

This document provides comprehensive reference for all transaction-related API endpoints. The transaction system supports multiple modes:

| Mode | Entity Type | Endpoint |
|------|-------------|----------|
| SALE / PURCHASE | DigitalAssetTransaction | `/digitalassettransaction` or `/transfer` |
| RECEIPT / PAYMENT | FiatAssetTransaction | `/fiatasettransaction` or `/transfer` |
| TRANSFER | Either (based on asset) | `/transfer` |
| INTERNAL | Either (based on asset) | `/transfer` |
| SETTLEMENT | SettlementTransaction | `/settlementtransaction` |

**Recommended for new implementations:** Use the unified `/transfer` endpoint for all non-settlement transactions.

### February 2026 Status Update

- Automatic wallet creation during transfer is deprecated and explicitly rejected (`WALLETS_CREATION_DEPRECATED`).
- System operations are supported by sending `senderAssetHolderId` and/or `receiverAssetHolderId` as `null` or `Guid.Empty` plus explicit wallet IDs.
- Transfer mode bank rules are enforced on backend; `AssetGroup.Flexible` restriction remains a frontend guardrail (not a backend hard validation).

### Future Improvements

> **📋 TODO: Frontend Transaction Pattern Screening**
> 
> A deep screening of the frontend codebase is needed to identify all transaction creation patterns
> and verify they correctly use the unified `/transfer` endpoint. Currently `/dashboard` has been
> verified to implement correctly. Future documentation reviews should check this note and perform
> the screening if not yet completed.
>
> **Status:** Pending
> **Added:** January 2026

> **🔄 TODO: Endpoint Rename Consideration**
> 
> Consider renaming `/api/v1/transfer` to `/api/v1/asset-transaction` in a future version to:
> 1. Clearly indicate the endpoint's purpose (asset transactions, not just "transfers")
> 2. Distinguish from the TRANSFER transaction mode (which is just one of 7 modes)
> 3. Reserve "transfer" naming for eventual non-asset transactions
>
> **Impact:** Backend endpoint rename + Frontend API calls update + Documentation updates
> **Status:** Deferred
> **Added:** January 2026

---

## Transfer Endpoint (Unified)

**Base Route**: `/api/v1/transfer`

**Controller**: `TransferController`

**Purpose**: Unified endpoint for all asset transactions between asset holders, supporting both Fiat and Digital assets. Despite the name, this handles ALL transaction modes (SALE, PURCHASE, RECEIPT, PAYMENT, TRANSFER, INTERNAL, CONVERSION), not just "transfers".

---

### POST /api/v1/transfer

Creates a transfer between any two asset holders.

**Authorization**: `Permission:create:transactions`

#### Request Body

```json
{
  "senderAssetHolderId": "10828ea8-f3eb-4f0c-492d-08ddc54a1a1b",
  "receiverAssetHolderId": "065fce58-b38b-4003-492f-08ddc54a1a1b",
  "senderWalletIdentifierId": "fa3f2bc0-3e33-473e-949d-08ddc621ef17",
  "receiverWalletIdentifierId": "434d3633-3d85-47b2-8953-08de554e0b11",
  "assetType": 21,
  "amount": 1000.00,
  "date": "2026-01-22T10:00:00Z",
  "categoryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "description": "Payment for services",
  "autoApprove": false,
  "validateBalance": false,
  "balanceAs": null,
  "conversionRate": null,
  "rate": null
}
```

#### Request Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `senderAssetHolderId` | Guid? | ✅ Yes* | - | ID of the sender asset holder. For system operations (company wallets), can be `null` or `Guid.Empty`. When null/empty, `senderWalletIdentifierId` must be provided. |
| `receiverAssetHolderId` | Guid? | ✅ Yes* | - | ID of the receiver asset holder. For system operations (company wallets), can be `null` or `Guid.Empty`. When null/empty, `receiverWalletIdentifierId` must be provided. |
| `assetType` | int | ✅ Yes | - | Asset type enum value (21=BRL, 101=PokerStars, etc.) |
| `amount` | decimal | ✅ Yes | - | Transfer amount (positive) |
| `date` | DateTime | ✅ Yes | - | Transaction date |
| `senderWalletIdentifierId` | Guid? | ❌ No* | null | Specific sender wallet. **Required** for system operations when `senderAssetHolderId` is null/empty. |
| `receiverWalletIdentifierId` | Guid? | ❌ No* | null | Specific receiver wallet. **Required** for system operations when `receiverAssetHolderId` is null/empty. |
| `categoryId` | Guid? | ❌ No | null | Transaction category for classification |
| `description` | string? | ❌ No | null | Transaction description |
| `createWalletsIfMissing` | bool | ❌ No | false | Deprecated. If true, API returns `WALLETS_CREATION_DEPRECATED` |
| `autoApprove` | bool | ❌ No | false | Auto-approve the transaction |
| `validateBalance` | bool | ❌ No | false | Validate sender has sufficient balance |
| `balanceAs` | int? | ❌ No | null | Record balance as different asset (digital only) |
| `conversionRate` | decimal? | ❌ No | null | Conversion rate (digital only) |
| `rate` | decimal? | ❌ No | null | Exchange rate (digital only) |

> **Note on System Operations (*)**  
> For system operations involving company wallets:
> - `senderAssetHolderId` and/or `receiverAssetHolderId` can be `null` or `Guid.Empty` (`00000000-0000-0000-0000-000000000000`)
> - When using null/empty asset holder ID, the corresponding wallet identifier **must** be provided
> - The system will display "Company" as the asset holder name for system participants

#### Success Response (200 OK)

```json
{
  "transactionId": "5fa85f64-5717-4562-b3fc-2c963f66afa6",
  "entityType": "fiat",
  "assetType": 21,
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
  "receiverWalletCreated": false,
  "walletsCreated": false
}
```

#### Business Rules

1. **Mode Inference:**
   - `SenderAssetHolderId == ReceiverAssetHolderId` (and not Guid.Empty) → INTERNAL mode
   - `SenderAssetHolderId != ReceiverAssetHolderId` → TRANSFER mode (or RECEIPT/PAYMENT for bank transactions)
   - System operations (null/Guid.Empty) are treated separately from regular asset holders

2. **Bank Restrictions:**
   - **TRANSFER mode:** Banks cannot be sender or receiver
   - **RECEIPT mode:** Banks can be receiver (fiat assets only)
   - **PAYMENT mode:** Banks can be sender (fiat assets only)
   - **Bank-to-bank transfers:** Not allowed (throws `BANK_TO_BANK_NOT_ALLOWED`)
   - **INTERNAL mode:** Banks allowed (for internal movements)

3. **Wallet Creation:**
   - Wallets must exist before transfer
   - If wallets missing → Returns 400 with `WALLETS_REQUIRED`
   - `CreateWalletsIfMissing` is deprecated; if true → Returns 400 with `WALLETS_CREATION_DEPRECATED`

4. **Asset Type Determination:**
   - Fiat assets (21-22) → Creates `FiatAssetTransaction`
   - Digital assets (101+) → Creates `DigitalAssetTransaction`

5. **Validation:**
   - Sender and receiver must exist
   - Wallets must belong to specified asset holders
   - Wallets must match requested asset type
   - Cannot transfer to the same wallet
   - Balance validation only if `ValidateBalance = true`

#### Error Responses

##### Wallets Required (400)

Returned when one or more required participant wallets do not exist:

```json
{
  "title": "Wallet Creation Required",
  "status": 400,
  "detail": "One or more wallets need to be created to complete this transfer.",
  "extensions": {
    "errorCode": "WALLETS_REQUIRED",
    "walletDetails": {
      "code": "WALLETS_REQUIRED",
      "message": "One or more wallets need to be created to complete this transfer.",
      "senderWalletMissing": true,
      "senderAssetHolderId": "10828ea8-f3eb-4f0c-492d-08ddc54a1a1b",
      "senderAssetHolderName": "Client João Silva",
      "senderAssetHolderType": "Client",
      "senderAssetTypeName": "BrazilianReal",
      "receiverWalletMissing": false,
      "receiverAssetHolderId": null,
      "receiverAssetHolderName": null,
      "receiverAssetHolderType": null,
      "receiverAssetTypeName": null
    }
  }
}
```

##### Wallet Creation Deprecated (400)

Returned when `CreateWalletsIfMissing = true`:

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

##### Bank Not Allowed (400)

Returned when a bank is involved in a TRANSFER mode transaction:

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

##### Insufficient Balance (400)

Returned when `ValidateBalance = true` and sender has insufficient funds:

```json
{
  "title": "Transfer Failed",
  "status": 400,
  "detail": "Insufficient balance. Available: 500.00, Requested: 1000.00",
  "extensions": {
    "errorCode": "INSUFFICIENT_BALANCE"
  }
}
```

#### Use Cases

##### 1. Client sends BRL to Member (Fiat Transfer)

```http
POST /api/v1/transfer
Content-Type: application/json
Authorization: Bearer {token}

{
  "senderAssetHolderId": "client-guid",
  "receiverAssetHolderId": "member-guid",
  "assetType": 21,
  "amount": 500.00,
  "date": "2026-01-22T10:00:00Z"
}
```

→ Creates `FiatAssetTransaction` with `isInternalTransfer: false`

##### 2. Client moves BRL between own wallets (Internal)

```http
POST /api/v1/transfer
Content-Type: application/json
Authorization: Bearer {token}

{
  "senderAssetHolderId": "client-guid",
  "receiverAssetHolderId": "client-guid",
  "senderWalletIdentifierId": "bank-wallet-guid",
  "receiverWalletIdentifierId": "internal-wallet-guid",
  "assetType": 21,
  "amount": 1000.00,
  "date": "2026-01-22T10:00:00Z"
}
```

→ Creates `FiatAssetTransaction` with `isInternalTransfer: true`

##### 3. PokerManager sends chips to Member (Digital Transfer)

```http
POST /api/v1/transfer
Content-Type: application/json
Authorization: Bearer {token}

{
  "senderAssetHolderId": "pokermanager-guid",
  "receiverAssetHolderId": "member-guid",
  "assetType": 101,
  "amount": 5000.00,
  "date": "2026-01-22T10:00:00Z",
  "balanceAs": 21,
  "conversionRate": 1.05,
  "rate": 5.0
}
```

→ Creates `DigitalAssetTransaction` with conversion fields

##### 4. Transfer with wallet creation confirmation

**Step 1: Initial request (wallets don't exist)**

```http
POST /api/v1/transfer
{
  "senderAssetHolderId": "client-guid",
  "receiverAssetHolderId": "member-guid",
  "assetType": 21,
  "amount": 100.00,
  "date": "2026-01-22T10:00:00Z",
  "createWalletsIfMissing": false
}
```

Response (400):
```json
{
  "extensions": {
    "errorCode": "WALLETS_REQUIRED",
    "walletDetails": {
      "senderWalletMissing": true,
      "senderAssetHolderName": "João Silva",
      ...
    }
  }
}
```

**Step 2: User confirms, retry with flag**

```http
POST /api/v1/transfer
{
  "senderAssetHolderId": "client-guid",
  "receiverAssetHolderId": "member-guid",
  "assetType": 21,
  "amount": 100.00,
  "date": "2026-01-22T10:00:00Z",
  "createWalletsIfMissing": true
}
```

Response (200):
```json
{
  "transactionId": "...",
  "senderWalletCreated": true,
  "receiverWalletCreated": false,
  "walletsCreated": true
}
```

---

### GET /api/v1/transfer/{id}

Retrieves a transfer transaction by ID.

**Authorization**: `Permission:read:transactions`

> **Frontend Status:** Currently not used by the frontend. Keep for backend tooling or future UI needs.

#### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `entityType` | string | ❌ No | "fiat" | "fiat" or "digital" |

#### Success Response (200 OK)

```json
{
  "transactionId": "5fa85f64-5717-4562-b3fc-2c963f66afa6",
  "entityType": "fiat",
  "assetType": 21,
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
  "receiverWalletCreated": false,
  "walletsCreated": false
}
```

> **Note:** `senderWalletCreated` and `receiverWalletCreated` are always `false` on GET requests as creation state is not stored.

#### Error Response (404)

```json
{
  "title": "Not Found",
  "status": 404,
  "detail": "Transfer '5fa85f64-5717-4562-b3fc-2c963f66afa6' not found"
}
```

---

## Fiat Asset Transaction Endpoints

**Base Route**: `/api/v1/fiatasettransaction`

Standard CRUD endpoints for fiat currency transactions.

| Method | Route | Description | Response |
|--------|-------|-------------|----------|
| GET | `/` | List all fiat transactions | `List<FiatAssetTransactionResponse>` |
| GET | `/{id}` | Get transaction by ID | `FiatAssetTransactionResponse` |
| POST | `/` | Create transaction | `FiatAssetTransactionResponse` |
| PUT | `/{id}` | **Partial update** transaction | `FiatAssetTransactionResponse` |
| DELETE | `/{id}` | Delete transaction | `204 No Content` |
| GET | `/bank-transactions` | Get bank-related transactions | `TableResponse<FiatAssetTransactionResponse>` |
| GET | `/direct-transactions` | Get non-bank transactions | `TableResponse<FiatAssetTransactionResponse>` |

### Query Parameters for Lists

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `quantity` | int | No | 1000 | Number of records |
| `page` | int | No | 0 | Page number (0-indexed) |

### PUT /api/v1/fiatasettransaction/{id} (Partial Update)

Updates a fiat asset transaction with **partial update semantics**. Only fields that are provided will be updated; other fields remain unchanged.

**Request Body** (`UpdateFiatAssetTransactionRequest`):

```json
{
  "date": "2026-01-22T10:00:00Z",
  "assetAmount": 1500.00,
  "categoryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `date` | DateTime? | No | Transaction date (cannot be in the future) |
| `assetAmount` | decimal? | No | Amount (must be > 0) |
| `categoryId` | Guid? | No | Category ID (send empty GUID to clear) |

**Business Rules:**
- **Approved transactions cannot be updated** - returns 409 Conflict
- Only provided fields are updated; null/missing fields are ignored
- `SenderWalletIdentifierId` and `ReceiverWalletIdentifierId` cannot be changed (intentional)

**Response Codes:**
| Code | Description |
|------|-------------|
| 200 | Success - returns updated transaction |
| 400 | Validation error (future date, negative amount) |
| 404 | Transaction not found |
| 409 | Transaction is already approved |

**Example Error Response (409 Conflict):**
```json
{
  "message": "Cannot update an approved transaction. Remove approval first."
}
```

---

## Digital Asset Transaction Endpoints

**Base Route**: `/api/v1/digitalassettransaction`

Standard CRUD endpoints for digital asset transactions.

| Method | Route | Description | Response |
|--------|-------|-------------|----------|
| GET | `/` | List all digital transactions | `List<DigitalAssetTransactionResponse>` |
| GET | `/{id}` | Get transaction by ID | `DigitalAssetTransactionResponse` |
| POST | `/` | Create transaction | `DigitalAssetTransactionResponse` |
| PUT | `/{id}` | **Partial update** transaction | `DigitalAssetTransactionResponse` |
| DELETE | `/{id}` | Delete transaction | `204 No Content` |
| GET | `/poker-manager-transactions` | Get poker manager transactions | `TableResponse<DigitalAssetTransactionResponse>` |

### PUT /api/v1/digitalassettransaction/{id} (Partial Update)

Updates a digital asset transaction with **partial update semantics**. Only fields that are provided will be updated; other fields remain unchanged.

**Request Body** (`UpdateDigitalAssetTransactionRequest`):

```json
{
  "date": "2026-01-22T10:00:00Z",
  "assetAmount": 5000.00,
  "conversionRate": 5.25,
  "categoryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `date` | DateTime? | No | Transaction date (cannot be in the future) |
| `assetAmount` | decimal? | No | Amount (must be > 0) |
| `conversionRate` | decimal? | No | Conversion rate (must be > 0) |
| `categoryId` | Guid? | No | Category ID (send empty GUID to clear) |

**Business Rules:**
- **Approved transactions cannot be updated** - returns 409 Conflict
- Only provided fields are updated; null/missing fields are ignored
- `SenderWalletIdentifierId` and `ReceiverWalletIdentifierId` cannot be changed (intentional)
- **AvgRate cache is invalidated** after successful update for affected PokerManagers

**Response Codes:**
| Code | Description |
|------|-------------|
| 200 | Success - returns updated transaction |
| 400 | Validation error (future date, negative amount/rate) |
| 404 | Transaction not found |
| 409 | Transaction is already approved |

**Example Error Response (409 Conflict):**
```json
{
  "message": "Cannot update an approved transaction. Remove approval first."
}
```

---

## Settlement Transaction Endpoints

**Base Route**: `/api/v1/settlementtransaction`

Endpoints for poker settlement transactions.

> **Deprecation Note:** `POST /api/v1/settlementtransaction` is disabled. Use  
> `POST /api/v1/pokermanager/{id}/settlement-by-date` to create settlement transactions.

| Method | Route | Description | Response |
|--------|-------|-------------|----------|
| GET | `/` | List all settlements | `List<SettlementTransactionResponse>` |
| GET | `/{id}` | Get settlement by ID | `SettlementTransactionResponse` |
| POST | `/` | Create settlement | `SettlementTransactionResponse` |
| PUT | `/{id}` | Update settlement | `SettlementTransactionResponse` |
| DELETE | `/{id}` | Delete settlement | `204 No Content` |
| GET | `/closings/{pokerManagerId}` | Get closings grouped by date | `SettlementClosingsGroupedResponse` |

---

### GET /api/v1/settlementtransaction/closings/{pokerManagerId}

Retrieves settlement transactions for a poker manager, grouped by settlement date. This endpoint is used for **daily reconciliation** where managers can view all settlements for a given date as a single "closing" (Portuguese: **Fechamento**).

**Authorization**: `Permission:read:transactions`

#### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `pokerManagerId` | Guid | ✅ Yes | ID of the poker manager |

#### Success Response (200 OK)

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
          "signedAssetAmount": -10000.00,
          "rakeAmount": 500.00,
          "rakeCommission": 150.00,
          "rakeBack": 50.00,
          "netSettlementAmount": -9250.00,
          "effectiveCommissionRate": 500.0,
          "transactionType": "Settlement",
          "senderWallet": {
            "id": "...",
            "assetGroup": "Settlements",
            "assetHolder": { "name": "Player João", "assetHolderType": "Client" }
          },
          "receiverWallet": {
            "id": "...",
            "assetGroup": "Settlements",
            "assetHolder": { "name": "Manager ABC", "assetHolderType": "PokerManager" }
          }
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

#### Response Structure

| Field | Type | Description |
|-------|------|-------------|
| `closingGroups` | `SettlementClosingGroup[]` | List of closing groups, ordered by date (most recent first) |

**SettlementClosingGroup:**

| Field | Type | Description |
|-------|------|-------------|
| `date` | DateTime | Settlement date (time component is midnight) |
| `transactions` | `SettlementTransactionResponse[]` | Individual settlements on this date |
| `transactionCount` | int | Number of settlements (computed) |
| `totalAssetAmount` | decimal | Sum of all signed settlement amounts (computed) |
| `totalRake` | decimal | Sum of all rake amounts (computed) |
| `totalRakeCommission` | decimal | Sum of all commission amounts (computed) |
| `totalRakeBack` | decimal? | Sum of all rakeback amounts (computed) |
| `totalNetSettlement` | decimal | Sum of all net settlement amounts (computed) |

#### Business Rules

1. **Date Grouping**: Transactions are grouped by `Date.Date` (date only, no time component)
2. **Ordering**: Closing groups are returned in descending date order (most recent first)
3. **Scope**: Only includes settlements where the poker manager is sender OR receiver
4. **Soft Deletes**: Excludes transactions where `DeletedAt != null`
5. **Signed Amount**: Negative when poker manager is the receiver, positive when sender

#### Use Cases

##### 1. Daily Reconciliation Dashboard

```http
GET /api/v1/settlementtransaction/closings/pokermanager-guid
Authorization: Bearer {token}
```

→ Shows all settlement dates with aggregated totals for quick review

##### 2. Last Settlement Per Wallet

Used internally by `GET /api/v1/pokermanager/{id}/wallet-identifiers-connected` to show the most recent settlement for each connected wallet:

```csharp
var closings = await _settlementTransactionService.GetClosings(pokerManagerId);
var lastClosing = closings.OrderByDescending(c => c.Key).FirstOrDefault().Value;
// Find specific wallet's last settlement from lastClosing
```

#### Integration Notes

This endpoint is closely tied to the "Connected Wallets" feature in the frontend:
- Poker managers can see which players they're connected to
- Each connected wallet shows its last settlement date and amount
- Managers can identify players who haven't settled recently

For detailed information about the Closing concept, see [SETTLEMENT_WORKFLOW.md - Closings Section](../03_CORE_SYSTEMS/SETTLEMENT_WORKFLOW.md#closings-fechamentos).

---

## Deprecated Endpoints

The following endpoints are marked `[Obsolete]` and will be removed in API v2. Use `/api/v1/transfer` instead.

| Controller | Route | Replacement |
|------------|-------|-------------|
| ClientController | `POST /{id}/send-brazilian-real` | `POST /api/v1/transfer` |
| MemberController | `POST /{id}/send-brazilian-real` | `POST /api/v1/transfer` |
| PokerManagerController | `POST /{id}/send-brazilian-real` | `POST /api/v1/transfer` |

### Migration Guide

**Old:**
```http
POST /api/v1/client/{clientId}/send-brazilian-real
{
  "receiverId": "member-guid",
  "amount": 1000.00,
  "date": "2026-01-22"
}
```

**New:**
```http
POST /api/v1/transfer
{
  "senderAssetHolderId": "client-guid",
  "receiverAssetHolderId": "member-guid",
  "assetType": 21,
  "amount": 1000.00,
  "date": "2026-01-22T00:00:00Z"
}
```

---

## Common Request Patterns

### Transaction with Category

```json
{
  "senderAssetHolderId": "sender-guid",
  "receiverAssetHolderId": "receiver-guid",
  "assetType": 21,
  "amount": 500.00,
  "date": "2026-01-22T10:00:00Z",
  "categoryId": "category-guid"
}
```

### Digital Asset with Conversion

```json
{
  "senderAssetHolderId": "pokermanager-guid",
  "receiverAssetHolderId": "client-guid",
  "assetType": 101,
  "amount": 1000.00,
  "date": "2026-01-22T10:00:00Z",
  "balanceAs": 21,
  "conversionRate": 1.05,
  "rate": 5.0
}
```

### System Operation (System Wallet Pairing)

When a transaction uses a company system wallet as counterparty, the system wallet should be resolved by asset type:

```http
GET /api/v1/company/asset-pools/system-wallet-to-pair-with/{walletIdentifierId}
```

Then submit the transfer with the resolved system wallet as sender or receiver.

**System Operation Behavior:**

1. **Asset Holder ID Handling:**
   - Set `senderAssetHolderId` or `receiverAssetHolderId` to `null` or `Guid.Empty` (`00000000-0000-0000-0000-000000000000`) to indicate a system operation
   - The system will automatically use "Company" as the asset holder name

2. **Wallet Requirement:**
   - When using system operations, the corresponding `senderWalletIdentifierId` or `receiverWalletIdentifierId` **must** be provided
   - System wallets are company-owned wallets with `null` `BaseAssetHolderId` in their `AssetPool`

3. **Validation:**
   - System wallets are validated to ensure they have `null` `BaseAssetHolderId` (company-owned)
   - Regular wallets must belong to the specified asset holder

**Example Request (Payment from Company):**
```json
{
  "senderAssetHolderId": null,
  "senderWalletIdentifierId": "company-internal-wallet-guid",
  "receiverAssetHolderId": "client-guid",
  "assetType": 21,
  "amount": 5000.00,
  "date": "2026-01-22T10:00:00Z",
  "categoryId": "category-guid"
}
```

**Example Response:**
```json
{
  "transactionId": "...",
  "senderAssetHolderId": "00000000-0000-0000-0000-000000000000",
  "senderName": "Company",
  "receiverAssetHolderId": "client-guid",
  "receiverName": "Client João Silva",
  ...
}
```

### PokerManager Self-Conversion (Flexible Trigger)

Self-conversion is a DigitalAssetTransaction where a PokerManager uses their **Flexible** wallet as the counterparty to their own PokerAssets wallet. Both `balanceAs` and `conversionRate` must be set.

```json
{
  "senderAssetHolderId": "pokermanager-guid",
  "receiverAssetHolderId": "pokermanager-guid",
  "senderWalletIdentifierId": "internal-wallet-guid",
  "receiverWalletIdentifierId": "poker-wallet-guid",
  "assetType": 101,
  "amount": 1000.00,
  "date": "2026-01-22T10:00:00Z",
  "balanceAs": 21,
  "conversionRate": 5.0
}
```

To list available internal conversion wallets for a PokerManager:

```http
GET /api/v1/pokermanager/{id}/conversion-wallets
```

### Force Balance Validation

```json
{
  "senderAssetHolderId": "sender-guid",
  "receiverAssetHolderId": "receiver-guid",
  "assetType": 21,
  "amount": 1000.00,
  "date": "2026-01-22T10:00:00Z",
  "validateBalance": true
}
```

---

## Error Handling

All transaction endpoints use RFC 7807 Problem Details format for errors.

### Error Codes

| Code | HTTP Status | Description |
|------|-------------|-------------|
| `SENDER_NOT_FOUND` | 400 | Sender asset holder doesn't exist |
| `RECEIVER_NOT_FOUND` | 400 | Receiver asset holder doesn't exist |
| `SENDER_WALLET_NOT_FOUND` | 400 | Sender wallet not found |
| `RECEIVER_WALLET_NOT_FOUND` | 400 | Receiver wallet not found |
| `WALLET_CREATION_FAILED` | 400 | Failed to create wallet |
| `INSUFFICIENT_BALANCE` | 400 | Sender has insufficient funds |
| `INVALID_ASSET_TYPE` | 400 | Asset type is invalid or None |
| `ASSET_TYPE_MISMATCH` | 400 | Wallet asset type doesn't match |
| `WALLET_OWNERSHIP_MISMATCH` | 400 | Wallet doesn't belong to holder |
| `SAME_SENDER_RECEIVER_WALLET` | 400 | Cannot transfer to same wallet |
| `BANK_NOT_ALLOWED_IN_TRANSFER` | 400 | Banks restricted in TRANSFER mode (use RECEIPT/PAYMENT for fiat) |
| `BANK_TO_BANK_NOT_ALLOWED` | 400 | Bank-to-bank transfers are not allowed |
| `WALLETS_REQUIRED` | 400 | Wallets must be created |
| `CATEGORY_NOT_FOUND` | 400 | Category doesn't exist |
| `TRANSACTION_FAILED` | 400 | General transaction error |

### Error Response Format

```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Error Title",
  "status": 400,
  "detail": "Detailed error message",
  "extensions": {
    "requestId": "0HN5QKJI8J9RA:00000001",
    "timestamp": "2026-01-22T12:00:00Z",
    "errorCode": "ERROR_CODE",
    "walletDetails": { ... }
  }
}
```

---

## Security and Authorization

### Required Permissions

| Endpoint | Permission |
|----------|------------|
| `POST /transfer` | `create:transactions` |
| `GET /transfer/{id}` | `read:transactions` |
| `POST /fiatasettransaction` | `create:transactions` |
| `GET /fiatasettransaction` | `read:transactions` |
| `PUT /fiatasettransaction/{id}` | `update:transactions` |
| `DELETE /fiatasettransaction/{id}` | `delete:transactions` |
| Similar for digital/settlement | Corresponding permission |

### Authentication

All endpoints require a valid JWT bearer token from Auth0:

```http
Authorization: Bearer <token>
```

For detailed authentication information, see [AUTHENTICATION.md](../05_INFRASTRUCTURE/AUTHENTICATION.md).

---

## Related Documentation

| Topic | Document |
|-------|----------|
| Transaction Infrastructure | [TRANSACTION_INFRASTRUCTURE.md](../03_CORE_SYSTEMS/TRANSACTION_INFRASTRUCTURE.md) |
| Response DTOs | [TRANSACTION_RESPONSE_VIEWMODELS.md](../03_CORE_SYSTEMS/TRANSACTION_RESPONSE_VIEWMODELS.md) |
| API Reference (Overview) | [API_REFERENCE.md](./API_REFERENCE.md) |
| Authentication | [AUTHENTICATION.md](../05_INFRASTRUCTURE/AUTHENTICATION.md) |
| Error Handling | [ERROR_HANDLING.md](../05_INFRASTRUCTURE/ERROR_HANDLING.md) |
| Enum Definitions | [ENUMS_AND_TYPE_SYSTEM.md](../07_REFERENCE/ENUMS_AND_TYPE_SYSTEM.md) |

---

*Last updated: February 2026*


