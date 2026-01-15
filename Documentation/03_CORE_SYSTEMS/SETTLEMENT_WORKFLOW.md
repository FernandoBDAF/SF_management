# Settlement Workflow

## Overview

The Settlement system handles periodic financial reconciliation between poker managers and their players. Settlements calculate the net position between parties, accounting for transactions, rake, and commissions.

---

## Core Concepts

### What is a Settlement?

A settlement is a periodic reconciliation that:
1. Calculates the net balance between poker manager and player
2. Records the rake (commission) earned by the manager
3. Creates settlement transactions to zero out positions
4. Groups transactions by settlement date for reporting

### Settlement Transaction

```csharp
public class SettlementTransaction : BaseTransaction
{
    public decimal? RakeAmount { get; set; }       // Commission
    public DateTime? SettlementDate { get; set; }  // Settlement period end
}
```

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

## Related Documentation

- [TRANSACTION_INFRASTRUCTURE.md](TRANSACTION_INFRASTRUCTURE.md) - Transaction base
- [REFERRAL_SYSTEM.md](REFERRAL_SYSTEM.md) - Commission tracking

