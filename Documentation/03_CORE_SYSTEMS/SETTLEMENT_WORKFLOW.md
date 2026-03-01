# Settlement Workflow

## Overview

The Settlement system handles periodic financial reconciliation between poker managers and their players. Settlements calculate the net position between parties, accounting for transactions, rake, and commissions.

---

## Core Concepts

### What is a Settlement?

A settlement is a periodic reconciliation that:
1. Calculates the net balance between poker manager and player
2. Records the rake paid by the client to the poker site
3. Records the commission the **company earns** (via PokerManager) from the poker site
4. Records the rakeback the **company returns** to the client
5. Groups transactions by settlement date for reporting

> **Important:** The PokerManager acts as an asset holder on behalf of the company. The rake commission represents company revenue, not personal manager income.

### Settlement Transaction

```csharp
public class SettlementTransaction : BaseTransaction
{
    // Total rake the client paid to the poker site (in chips)
    [Required] public decimal RakeAmount { get; set; }
    
    // Percentage of rake the poker site pays to the company
    [Required] public decimal RakeCommission { get; set; }
    
    // Percentage of rake the company returns to the client
    public decimal? RakeBack { get; set; }
}
```

**Field Meanings:**

| Field | Description | Example |
|-------|-------------|---------|
| `RakeAmount` | Total rake client paid to poker site | 1000 chips |
| `RakeCommission` | % of rake poker site pays company | 50% |
| `RakeBack` | % of rake company returns to client | 10% |

> **Note:** `AssetAmount` (from BaseTransaction) represents the chip flow and is used for tracking purposes only. It should NOT be used for balance calculation as those chips already flowed via DigitalAssetTransactions.

---

## Balance Impact

### How SettlementTransactions Affect Balances

⚠️ **Critical:** The `AssetAmount` field should NOT be used for balance calculation. The chips already flowed via `DigitalAssetTransactions`.

| Entity | Balance Impact (BRL) | Formula |
|--------|----------------------|---------|
| **Client** | Receives rakeback | `+RakeAmount × (RakeBack / 100)` |
| **PokerManager** | Company earns commission | `-RakeAmount × (RakeCommission / 100)` |

**Example:**
```
Settlement Transaction:
- RakeAmount: 1000 chips
- RakeCommission: 50%
- RakeBack: 10%

Balance Impacts (BRL):
- Client FiatAssets (BRL): +100 (1000 × 10%)
- PokerManager FiatAssets (BRL): -500 (1000 × 50%)
```

### Settlement AssetAmount (RakeOverrideCommission Exception)

The `SettlementTransaction` has an `AssetAmount` field (inherited from `BaseTransaction`) representing the chip value of the settlement. For most managers, this value is tracking-only. However, for **RakeOverrideCommission** managers, `AssetAmount` impacts both PokerAssets and FiatAssets balances.

**Why:** RakeOverride chips are valued at a 1:1 ratio with BRL, so the chip movement directly corresponds to a fiat value.

**Signal Convention:**
- **Receiver** of the settlement → `+AssetAmount` (positive)
- **Sender** of the settlement → `-AssetAmount` (negative)

| Manager Type | AssetAmount Balance Impact |
|-------------|---------------------------|
| Spread | None (tracking only, chips already flowed via DigitalAssetTransactions) |
| RakeOverrideCommission | Impacts PokerAssets AND FiatAssets (1:1 BRL ratio) |

### Company Profit (Finance Module - TBD)

The company profit from each settlement is calculated as:

```
Company Profit = RakeAmount × ((RakeCommission - RakeBack) / 100)
```

Using the example above:
- Company Profit = 1000 × ((50 - 10) / 100) = **400 chips**

This will be tracked in the Finance Module (to be implemented).

---

## Settlement Process

### 1. Calculate Position

For each player wallet connected to the poker manager:
- Sum all digital asset transactions
- Calculate net position (what player owes/is owed)

### 2. Create Settlement by Date

```csharp
// PokerManagerController
[HttpPost("{assetHolderId}/settlement-by-date")]
public async Task<IActionResult> CreateSettlementTransactionsByDate(
    Guid assetHolderId, 
    [FromBody] SettlementTransactionByDateRequest request)
```

### 3. Record in Settlement Wallets

Settlements use wallets in `AssetGroup.Settlements`:
- Sender: Player's settlement wallet
- Receiver: Manager's settlement wallet
- Or vice versa depending on position

### Settlement Wallet Identifier

Settlements can create dedicated settlement wallet identifiers via:

```http
POST /api/v1/walletidentifier/settlement-wallet
```

Settlement wallets belong to `WalletGroup.Settlement` and are special-purpose wallets used exclusively for tracking settlement-specific transactions. They are distinct from regular PokerAssets or FiatAssets wallets and provide isolated tracking of settlement flows between poker managers and their players.

---

## Service Methods

### SettlementTransactionService

```csharp
// Get settlements grouped by date
public async Task<Dictionary<DateOnly, List<SettlementTransaction>>> GetClosings(
    Guid pokerManagerId)

// Create settlement transactions for a period
public async Task<SettlementTransactionByDateResponse> CreateSettlementTransactionsByDate(
    Guid assetHolderId, 
    SettlementTransactionByDateRequest request)
```

---

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/settlementtransaction` | List all settlements |
| GET | `/api/v1/settlementtransaction/closings/{pokerManagerId}` | Get grouped by date |
| POST | `/api/v1/pokermanager/{id}/settlement-by-date` | Create settlement |

### Response Structure

```json
{
  "closingGroups": [
    {
      "date": "2025-01-14",
      "transactions": [
        {
          "id": "...",
          "assetAmount": 1500.00,
          "rakeAmount": 75.00,
          "settlementDate": "2025-01-14T00:00:00Z",
          "senderWallet": { ... },
          "receiverWallet": { ... }
        }
      ]
    }
  ]
}
```

---

## Integration with Referrals

When processing settlements, check for active referrals:

```csharp
var activeReferral = await _referralService.GetActiveReferralForWallet(
    walletIdentifierId, 
    settlementDate);

if (activeReferral?.ParentCommission != null)
{
    var referralCommission = rakeAmount * (activeReferral.ParentCommission / 100);
    // Record commission for referrer
}
```

---

## Closings (Fechamentos)

### What is a Closing?

A **Closing** (Portuguese: **Fechamento**) represents a **daily batch of settlement transactions** for a poker manager. Rather than viewing settlements individually, closings aggregate all settlements that occurred on a specific date, providing a consolidated view for daily reconciliation.

### Business Purpose

Closings exist to support the poker management workflow:

1. **Daily Reconciliation**: Poker managers settle with players at regular intervals (typically daily or weekly). A closing captures all settlements from a given settlement date.

2. **Grouped Reporting**: Instead of reviewing hundreds of individual settlement transactions, managers can view aggregated totals per day.

3. **Last Settlement Tracking**: The frontend uses closings to show the most recent settlement for each connected wallet, helping managers identify which players need attention.

### How Closings Work

The `GetClosings` method in `SettlementTransactionService`:

1. **Finds wallet identifiers** belonging to the poker manager's asset pools
2. **Queries all settlement transactions** where the poker manager is sender or receiver
3. **Groups by date** (date only, without time component)
4. **Returns ordered dictionary** with most recent dates first

```csharp
public async Task<Dictionary<DateTime, List<SettlementTransaction>>> GetClosings(Guid pokerManagerId)
{
    // Get all wallet identifiers for the poker manager's asset pools
    var walletIdentifierIds = await context.WalletIdentifiers
        .Where(wi => wi.AssetPool.BaseAssetHolderId == pokerManagerId)
        .Select(wi => wi.Id)
        .ToListAsync();

    var transactions = await context.SettlementTransactions
        .Where(st => st.DeletedAt == null && 
                    (walletIdentifierIds.Contains(st.SenderWalletIdentifierId) ||
                     walletIdentifierIds.Contains(st.ReceiverWalletIdentifierId)))
        .ToListAsync();

    return transactions
        .GroupBy(st => st.Date.Date)
        .ToDictionary(
            group => group.Key,
            group => group.ToList()
        );
}
```

### API Endpoint

```http
GET /api/v1/settlementtransaction/closings/{pokerManagerId}
```

Returns settlements grouped by date in descending order (most recent first).

#### Response: `SettlementClosingsGroupedResponse`

```json
{
  "closingGroups": [
    {
      "date": "2026-01-22T00:00:00Z",
      "transactions": [
        {
          "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
          "assetAmount": 1500.00,
          "rakeAmount": 75.00,
          "rakeCommission": 15.00,
          "rakeBack": 10.00,
          "netSettlementAmount": 1420.00
        }
      ],
      "transactionCount": 15,
      "totalAssetAmount": 25000.00,
      "totalRake": 1250.00,
      "totalRakeCommission": 375.00,
      "totalRakeBack": 125.00,
      "totalNetSettlement": 23500.00
    },
    {
      "date": "2026-01-21T00:00:00Z",
      "transactions": [...],
      "transactionCount": 12,
      "totalAssetAmount": 18000.00
    }
  ]
}
```

### Aggregated Metrics

Each `SettlementClosingGroup` provides computed totals:

| Metric | Description |
|--------|-------------|
| `TransactionCount` | Number of settlements on this date |
| `TotalAssetAmount` | Sum of all settlement amounts |
| `TotalRake` | Total rake collected |
| `TotalRakeCommission` | Total commission earned |
| `TotalRakeBack` | Total rakeback paid to players |
| `TotalNetSettlement` | Net amount after all calculations |

### Integration with Wallet Identifiers Connected

The `GET /api/v1/pokermanager/{id}/wallet-identifiers-connected` endpoint uses closings internally to show the **last settlement** for each connected wallet:

```csharp
// PokerManagerController.GetWalletIdentifiersFromOthers
var settlementTransactions = await _settlementTransactionService.GetClosings(id);
var lastSettlementTransactions = settlementTransactions
    .OrderByDescending(st => st.Key)
    .FirstOrDefault().Value;

// For each wallet, find their most recent settlement
var lastSettlementTransaction = lastSettlementTransactions?
    .Where(st => st.SenderWalletIdentifierId == walletIdentifier.Id || 
                 st.ReceiverWalletIdentifierId == walletIdentifier.Id)
    .FirstOrDefault();
```

This allows the frontend to display when each player was last settled.

### Data Flow

```
┌─────────────┐     ┌─────────────────────────────────┐     ┌──────────────┐
│   Frontend  │────▶│ GET /closings/{pokerManagerId}  │────▶│  Controller  │
└─────────────┘     └─────────────────────────────────┘     └──────┬───────┘
                                                                   │
                                                                   ▼
      ┌────────────────────────────────────────────────────────────────┐
      │                 SettlementTransactionService                   │
      │  1. Find poker manager's wallet identifiers                    │
      │  2. Query settlement transactions where manager is party       │
      │  3. Group by Date (day only)                                  │
      │  4. Return Dictionary<DateTime, List<SettlementTransaction>>   │
      └────────────────────────────────────────────────────────────────┘
                                                                   │
                                                                   ▼
      ┌────────────────────────────────────────────────────────────────┐
      │                        Controller                              │
      │  1. Order by date descending (most recent first)              │
      │  2. Map to SettlementClosingsGroupedResponse                   │
      │  3. Each group computes aggregated metrics                     │
      └────────────────────────────────────────────────────────────────┘
```

---

## Statement Display (Extrato)

Settlement transactions appear in asset holder statements (`GET /{entity}/{id}/transactions`) with special handling:

### StatementTransactionResponse Fields

For settlement transactions, the response includes additional rake-related fields:

```csharp
public class StatementTransactionResponse
{
    // ... standard fields ...
    public decimal? RakeAmount { get; set; }
    public decimal? RakeCommission { get; set; }
    public decimal? RakeBack { get; set; }
    public decimal? RakeBackAmount { get; set; }  // Computed: RakeAmount × (RakeBack / 100)
}
```

### Computed Rakeback Amount

The `RakeBackAmount` is computed in the service layer:

```csharp
RakeBackAmount = st.RakeAmount * ((st.RakeBack ?? 0m) / 100m);
```

This represents the actual BRL value a client receives as rakeback from a settlement.

### Frontend Statement Value Logic

In client/member statements, settlement transactions use `RakeBackAmount` as the displayed value (not `AssetAmount`):

| Field | Non-Settlement | Settlement |
|-------|----------------|------------|
| **Valor** | `AssetAmount` (or converted) | `RakeBackAmount` with sign from `AssetAmount` |
| **Origem** | "Venda" / "Compra" | "Fechamento +" / "Fechamento -" |

**Example:**
```
Settlement Transaction:
- RakeAmount: 400
- RakeBack: 50%
- AssetAmount: -200 (negative = client paid chips)

Display:
- Valor: -200 (sign from AssetAmount, value from RakeBackAmount)
- Origem: "Fechamento -"
- Details: "Rakeback Fechamento: R$ 200.00"
```

---

## Related Documentation

- [TRANSACTION_INFRASTRUCTURE.md](TRANSACTION_INFRASTRUCTURE.md) - Transaction base
- [TRANSACTION_RESPONSE_VIEWMODELS.md](TRANSACTION_RESPONSE_VIEWMODELS.md) - Response DTOs including Closing types
- [REFERRAL_SYSTEM.md](REFERRAL_SYSTEM.md) - Commission tracking

