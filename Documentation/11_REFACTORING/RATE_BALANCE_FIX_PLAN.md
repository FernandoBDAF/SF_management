# Rate Balance Fix Plan

> **Status:** Implemented  
> **Created:** February 23, 2026  
> **Priority:** High  
> **Affects:** Client/Member balance calculations for transactions with Rate

---

## Table of Contents

- [Problem Statement](#problem-statement)
- [Business Rules Review](#business-rules-review)
- [Root Cause Analysis](#root-cause-analysis)
- [Fix Plan](#fix-plan)
- [UX Approach](#ux-approach)
- [Implementation Steps](#implementation-steps)
- [Testing Scenarios](#testing-scenarios)
- [Related Documentation Updates](#related-documentation-updates)

---

## Problem Statement

When a digital asset transaction has a `Rate` field (embedded fee percentage) but NO `BalanceAs` (coin balance mode), the Client/Member balance is incorrectly calculated using the full `AssetAmount` instead of the Rate-adjusted amount.

### Observed Behavior

Transaction: "Compra 1020 @ 1.00" with 2% Rate
- Manager buys 1020 chips from Client
- Client's statement shows: +R$ 1020.00 (correct - raw value)
- Client's balance impacted by: +1020 chips (WRONG - should be +1000)
- **Missing:** No indication of the 20 chip fee in the description

### Expected Behavior

- Client's statement value: **+1020 chips** (raw transfer amount - correct to show this)
- Client's statement description: Should indicate "Taxa: 20 fichas" so user understands the fee
- Client's balance impact: **+1000 chips** (net after fee deduction)

The 20 chip fee (1020 × 2/102 = 20) is company profit captured in RateFees, not the client's balance. The statement should show the raw amount but explain why only 1000 impacts the balance.

---

## Business Rules Review

### Rate Fee Semantics

The `Rate` field represents an **embedded fee percentage** in the transaction amount:

| Field | Description |
|-------|-------------|
| `AssetAmount` | Total chips transferred (includes the embedded fee) |
| `Rate` | Fee percentage embedded in total (e.g., 2 means 2%) |

### Fee Extraction Formula

```
FeeInChips = AssetAmount × (Rate / (100 + Rate))
NetAmount  = AssetAmount - FeeInChips
           = AssetAmount × (100 / (100 + Rate))
```

**Example with 1020 chips and 2% Rate:**
```
FeeInChips = 1020 × (2 / 102) = 20 chips
NetAmount  = 1020 × (100 / 102) = 1000 chips
```

### Balance Impact Rules

| Entity Type | Balance Impact | Rationale |
|-------------|----------------|-----------|
| **PokerManager** | Full `AssetAmount` | Manager inventory receives all chips including fee; fee captured separately in RateFees profit |
| **Client/Member** | `AssetAmount × 100 / (100 + Rate)` | Client owes/credits only the net amount; fee is company profit |

### When Rate Applies

Rate adjustment applies when:
1. `Rate` field is not null and > 0
2. `BalanceAs` is null (coin balance mode, debt/credit in chips not BRL)

When `BalanceAs` is set, the ConversionRate converts to BRL and Rate doesn't affect balance (the fee is still captured in RateFees but balance shows BRL value).

---

## Root Cause Analysis

### Code Location

**File:** `SF_management/Application/Services/Base/BaseAssetHolderService.cs`

### GetBalancesByAssetType (Client/Member Balance)

**Lines 518-545** - Current implementation:

```csharp
foreach (var tx in digitalTransactions)
{
    var relevantWalletId = walletIdentifierIds.FirstOrDefault(id => 
        tx.SenderWalletIdentifierId == id || tx.ReceiverWalletIdentifierId == id);
    
    var signedAmount = tx.GetSignedAmountForWalletIdentifier(relevantWalletId);
    // ... account classification adjustment ...
    
    if (tx.BalanceAs != null && tx.ConversionRate != null)
    {
        // BalanceAs case - already handled correctly
        balances[tx.BalanceAs.Value] += signedAmount * tx.ConversionRate.Value;
        continue;
    }
    
    // BUG: No Rate adjustment for coin balance mode!
    balances[assetType] += signedAmount;
}
```

### Missing Rate Adjustment

The code should apply:

```csharp
if (tx.Rate.HasValue && tx.Rate.Value > 0)
{
    signedAmount = signedAmount * 100 / (100 + tx.Rate.Value);
}
```

### GetBalancesByAssetGroup (Manager Balance)

**Lines 686-738** - Manager balance correctly does NOT apply Rate for regular transactions (only for self-conversion). This is correct because:
- Manager inventory receives full chips
- Fee is captured separately in RateFees profit calculation

### Statement Response

**Lines 815-841** - `GetTransactionsStatementForAssetHolder` also returns raw `signedAmount` without Rate adjustment:

```csharp
allTransactions.Add(new StatementTransactionResponse
{
    // ...
    AssetAmount = signedAmount,  // Raw amount, no Rate adjustment
    Rate = dat.Rate,             // Rate is available for display
    // ...
});
```

This may need a separate `BalanceImpact` field or Rate-adjusted display.

---

## Fix Plan

### Phase 1: Fix Balance Calculation

Apply Rate adjustment in `GetBalancesByAssetType` for coin balance transactions.

**File:** `BaseAssetHolderService.cs`

**Location:** After the BalanceAs check (around line 537)

```csharp
// After: if (tx.BalanceAs != null && tx.ConversionRate != null) { ... continue; }

// Apply Rate adjustment for coin balance mode (no BalanceAs)
if (tx.Rate.HasValue && tx.Rate.Value > 0)
{
    signedAmount = signedAmount * 100 / (100 + tx.Rate.Value);
}

var assetType = tx.IsReceiver(relevantWalletId) ?
    tx.ReceiverWalletIdentifier!.AssetType :
    tx.SenderWalletIdentifier!.AssetType;

if (!balances.ContainsKey(assetType)) balances[assetType] = 0;
balances[assetType] += signedAmount;
```

### Phase 2: Enhance Statement Display with Fee Information

The statement should continue showing the **raw transaction value** (e.g., 1020 chips), which correctly represents what was transferred. However, users need to understand why only 1000 chips impact their balance.

**Solution:** Add fee information to the statement response and display it in the transaction description.

**Backend:** Add `RateFeeAmount` field to `StatementTransactionResponse`

```csharp
// In StatementTransactionResponse
public decimal? RateFeeAmount { get; set; }  // Computed: AssetAmount × Rate / (100 + Rate)
```

**In GetTransactionsStatementForAssetHolder:**
```csharp
allTransactions.Add(new StatementTransactionResponse
{
    // ... existing fields ...
    AssetAmount = signedAmount,  // Keep raw value
    Rate = dat.Rate,
    RateFeeAmount = dat.Rate.HasValue && dat.Rate.Value > 0
        ? Math.Abs(signedAmount) * dat.Rate.Value / (100 + dat.Rate.Value)
        : null,
    // ...
});
```

**Frontend:** Display fee in transaction description/details

```
Compra 1020 @ 1.00 GgPoker Kapivarig
Taxa: 20 fichas (2%)
```

This helps users understand:
- 1020 chips were transferred
- 20 chips (2% fee) were charged
- Net balance impact: 1000 chips

---

## UX Approach

### Why Show Raw Values in Statement?

The statement should display the **raw transaction value** (not the Rate-adjusted value) because:

1. **Accuracy**: The user did transfer 1020 chips - that's the actual transaction that occurred
2. **Transparency**: Users can see exactly what was transferred vs what was charged as fee
3. **Reconciliation**: Makes it easier to match with external records or receipts

### Fee Indicator

To help users understand why the balance impact differs from the displayed amount:

- Display a "Taxa: X fichas (Y%)" indicator next to transactions with Rate
- This explains the discrepancy between transaction amount and balance impact
- Example: "Compra 1020 GgPoker | Taxa: 20 fichas (2%)"

### User Mental Model

| What User Sees | Meaning |
|----------------|---------|
| Amount: 1020 | "This is what was transferred" |
| Taxa: 20 (2%) | "This portion was a fee" |
| Balance change: +1000 | "Net impact after fee = 1020 - 20" |

---

### Phase 3: Fix Documentation

Update `TRANSACTION_BALANCE_IMPACT.md` to fix the example calculation:

Current (wrong):
```
Without BalanceAs (Rate 5%):
├─ Client PokerAssets: +950 (credit in chips after rate)
```

Should be:
```
Without BalanceAs (Rate 5%):
├─ Client PokerAssets: +952.38 (credit = 1000 / 1.05)
```

Or clarify that AssetAmount is already fee-inclusive:
```
AssetAmount: 1050 (includes 5% fee)
Fee: 1050 × 5/105 = 50
Net to Client: 1000
```

---

## Implementation Steps

### Step 1: Apply Rate Adjustment in Balance Calculation (Backend)

**File:** `SF_management/Application/Services/Base/BaseAssetHolderService.cs`

**Change in GetBalancesByAssetType:**

```csharp
// Process DigitalAssetTransactions
foreach (var tx in digitalTransactions)
{
    var relevantWalletId = walletIdentifierIds.FirstOrDefault(id => 
        tx.SenderWalletIdentifierId == id || tx.ReceiverWalletIdentifierId == id);
    
    var signedAmount = tx.GetSignedAmountForWalletIdentifier(relevantWalletId);
    if (!tx.HaveBothWalletsSameAccountClassification() && tx.IsWalletIdentifierLiability(relevantWalletId))
    {
        signedAmount = -signedAmount;
    }
    
    if (tx.BalanceAs != null && tx.ConversionRate != null)
    {
        if (!balances.ContainsKey(tx.BalanceAs.Value)) balances[tx.BalanceAs.Value] = 0;
        balances[tx.BalanceAs.Value] += signedAmount * tx.ConversionRate.Value;
        continue;
    }
    
    // NEW: Apply Rate adjustment for coin balance mode
    if (tx.Rate.HasValue && tx.Rate.Value > 0)
    {
        signedAmount = signedAmount * 100 / (100 + tx.Rate.Value);
    }
    
    var assetType = tx.IsReceiver(relevantWalletId) ?
        tx.ReceiverWalletIdentifier!.AssetType :
        tx.SenderWalletIdentifier!.AssetType;

    if (!balances.ContainsKey(assetType)) balances[assetType] = 0;
    balances[assetType] += signedAmount;
}
```

### Step 2: Add RateFeeAmount to Statement Response (Backend)

**File:** `SF_management/Application/DTOs/Transactions/StatementTransactionResponse.cs`

Add new field:
```csharp
/// <summary>
/// Computed fee amount when Rate is present: |AssetAmount| × Rate / (100 + Rate)
/// Helps users understand why balance impact differs from transaction amount.
/// </summary>
public decimal? RateFeeAmount { get; set; }
```

**File:** `SF_management/Application/Services/Base/BaseAssetHolderService.cs`

In `GetTransactionsStatementForAssetHolder`, update digital transaction mapping:

```csharp
allTransactions.Add(new StatementTransactionResponse
{
    Id = dat.Id,
    Date = dat.Date,
    Description = dat.Category?.Description,
    AssetAmount = signedAmount,  // Keep raw value
    BalanceAs = dat.BalanceAs,
    ConversionRate = dat.ConversionRate,
    Rate = dat.Rate,
    // NEW: Compute fee amount for display
    RateFeeAmount = (dat.Rate.HasValue && dat.Rate.Value > 0 && dat.BalanceAs == null)
        ? Math.Abs(signedAmount) * dat.Rate.Value / (100 + dat.Rate.Value)
        : null,
    AssetType = dat.SenderWalletIdentifier!.AssetType,
    CounterPartyName = dat.GetCounterPartyName(relevantWalletId),
    WalletIdentifierInput = dat.GetWalletIdentifierInput(relevantWalletId),
    AssetGroup = dat.SenderWalletIdentifier!.AssetGroup
});
```

### Step 3: Display Fee Information in Frontend

**File:** `SF_management-front/src/shared/types/domain/transaction.types.ts`

Add to `SimplifiedTransaction`:
```typescript
rateFeeAmount?: number;
```

**File:** `SF_management-front/src/shared/components/data-display/TransactionTable/DesktopTable.tsx` (and similar for Tablet/Mobile)

Display fee when present:
```tsx
{transaction.rateFeeAmount != null && transaction.rateFeeAmount > 0 && (
  <span className="text-xs text-gray-400 ml-2">
    Taxa: {transaction.rateFeeAmount.toFixed(2)} ({transaction.rate}%)
  </span>
)}
```

### Step 4: Verify Manager Balance (No Change Needed)

Confirm `GetBalancesByAssetGroup` does NOT apply Rate for regular transactions:
- Manager receives full `AssetAmount` in inventory
- Rate fee captured separately in `ProfitCalculationService.CalculateRateFees()`

Only self-conversion applies Rate (existing logic is correct).

### Step 5: Update Documentation

Fix `TRANSACTION_BALANCE_IMPACT.md`:
1. Clarify the Rate formula with correct math
2. Add explicit note about embedded fee semantics
3. Document the difference between Manager and Client Rate handling
4. Explain that statement shows raw value with fee indicator

---

## Testing Scenarios

### Scenario 1: PURCHASE with Rate (Client sells to Manager)

**Setup:**
- Client: Chfs-br
- Manager: Kapivarig
- AssetAmount: 1020 chips
- Rate: 2%
- BalanceAs: null (coin balance)

**Expected After Fix:**
- Manager PokerAssets: +1020 chips (full amount)
- Client balance: +1000 chips (1020 × 100/102 = 1000)

**Verify:**
- Client statement shows transaction with:
  - AssetAmount: +1020 (raw value - what was transferred)
  - RateFeeAmount: 20 (computed fee)
  - Display: "Taxa: 20.00 (2%)" indicator
- Client total balance is correct (+1000 net from this transaction)
- User understands: 1020 transferred, 20 was fee, 1000 credited to balance

### Scenario 2: SALE with Rate (Manager sells to Client)

**Setup:**
- Manager sells 1050 chips to Client
- Rate: 5%
- BalanceAs: null

**Expected:**
- Manager PokerAssets: -1050 chips
- Client balance: -1000 chips (1050 × 100/105 = 1000)

### Scenario 3: Transaction with BalanceAs (BRL mode)

**Setup:**
- AssetAmount: 1000
- Rate: 2%
- BalanceAs: BRL
- ConversionRate: 5.0

**Expected:**
- NO Rate adjustment (BalanceAs mode)
- Client balance: -5000 BRL (1000 × 5.0)
- Rate fee captured in RateFees profit calculation separately

### Scenario 4: Transaction without Rate

**Setup:**
- AssetAmount: 1000
- Rate: null or 0

**Expected:**
- No change from current behavior
- Balance impact = full AssetAmount

---

## Related Documentation Updates

| Document | Update Needed |
|----------|---------------|
| `TRANSACTION_BALANCE_IMPACT.md` | Fix Rate formula example, clarify embedded fee semantics |
| `PROFIT_CALCULATION_SYSTEM.md` | Add note about Rate in balance vs RateFees distinction |
| `ENTITY_BUSINESS_BEHAVIOR.md` | Reference Rate handling for Client/Member |

---

## Rollback Plan

If issues discovered after deployment:
1. Revert the Rate adjustment code change
2. Document specific scenarios causing problems
3. Refine business rules if edge cases found

---

## Files to Modify

| File | Change |
|------|--------|
| `SF_management/Application/Services/Base/BaseAssetHolderService.cs` | Add Rate adjustment in `GetBalancesByAssetType`; Add `RateFeeAmount` to statement response |
| `SF_management/Application/DTOs/Transactions/StatementTransactionResponse.cs` | Add `RateFeeAmount` property |
| `SF_management-front/src/shared/types/domain/transaction.types.ts` | Add `rateFeeAmount` field to SimplifiedTransaction |
| `SF_management-front/src/shared/components/data-display/TransactionTable/*.tsx` | Display fee indicator when `rateFeeAmount` present |
| `SF_management/Documentation/03_CORE_SYSTEMS/TRANSACTION_BALANCE_IMPACT.md` | Fix Rate example, clarify semantics, document statement display |

---

*Created: February 23, 2026*
