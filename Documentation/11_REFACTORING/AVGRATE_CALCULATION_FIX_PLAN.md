# AvgRate Calculation Fix Plan

**Created**: 2026-02-23  
**Status**: Implemented  
**Priority**: High  
**Affects**: Profit calculation accuracy (RateFees, SpreadProfit)
**Implemented on**: 2026-02-23

---

## Problem Statement

Two issues were discovered in the AvgRate calculation logic:

### Issue 1: Inconsistent Receive Logic Between Past and Current Month

The `CalculateSingleMonth()` method (used for past months) and `CalculateAvgRateUpToDate()` method (used for current month) have **different** logic for handling incoming chip transactions without `ConversionRate`.

**Past month logic (`CalculateSingleMonth`):**
```csharp
if (isReceiving && tx.ConversionRate.HasValue)  // Only counts receives WITH ConversionRate
{
    totalChips += tx.AssetAmount;
    totalCost += tx.AssetAmount * tx.ConversionRate.Value;
}
```

**Current month logic (`CalculateAvgRateUpToDate`):**
```csharp
if (isReceiving)  // Counts ALL receives, uses currentAvgRate as fallback
{
    var currentAvgRate = totalChips > 0 ? totalCost / totalChips : 0;
    var receivePrice = tx.ConversionRate ?? currentAvgRate;
    totalChips += tx.AssetAmount;
    totalCost += tx.AssetAmount * receivePrice;
}
```

**Impact**: The same transaction would produce different AvgRate results depending on whether it's in the current month or a past month. This breaks the consistency guarantee.

---

### Issue 2: Transaction Order Sensitivity Within a Day

Transactions registered out of chronological order within the same day can cause incorrect AvgRate calculations. 

**Example scenario:**
1. Manager has 900 chips at AvgRate 5.0
2. Two transactions occur on Feb 20:
   - 22:40 - SELL 1000 chips (registered first in DB)
   - 22:41 - BUY 1020 chips with 2% Rate (registered second)
3. Actual business chronology: the buy happened before the sell (client returned chips, then manager forwarded them)

**Current behavior:**
1. Process sell first → balance goes negative → clamp to 0 chips, 0 cost
2. Process buy with no ConversionRate → `receivePrice = currentAvgRate = 0/0 = 0`
3. Result: 1020 chips at cost 0 → AvgRate = 0
4. RateFees = 20 chips × 0 = **0 BRL** (should be ~100 BRL)

**Root cause**: The algorithm processes transactions by `Date` then `CreatedAt`, but `CreatedAt` reflects registration time, not business occurrence time within the day.

---

## Solution Design

### Fix 1: Synchronize Past Month Logic with Current Month Logic

Update `CalculateSingleMonth()` to use the same receive pricing logic as `CalculateAvgRateUpToDate()`:

```csharp
if (isReceiving)
{
    var currentAvgRate = totalChips > 0 ? totalCost / totalChips : 0;
    var receivePrice = tx.ConversionRate ?? currentAvgRate;
    totalChips += tx.AssetAmount;
    totalCost += tx.AssetAmount * receivePrice;
}
```

This ensures consistent behavior regardless of whether the transaction is in the current month or a past month.

---

### Fix 2: Process Receives Before Sends Within Each Day

Change the transaction processing order to:
1. **Group transactions by Date**
2. **Within each day, process ALL receives first, then ALL sends**
3. This ensures incoming chips update the AvgRate before any outgoing chips are deducted

**New ordering logic:**
```csharp
foreach (var tx in transactions
    .OrderBy(t => t.Date.Date)           // Group by day
    .ThenBy(t => IsReceive(t) ? 0 : 1)   // Receives first (0), then sends (1)
    .ThenBy(t => t.CreatedAt))           // Within each group, by registration time
{
    // Process transaction...
}
```

**Rationale**: In real business operations, it's impossible for a manager to sell chips they don't have. If chips were sold, they must have been acquired first (either from InitialBalance or a prior buy). Processing receives before sends within a day aligns with this business reality regardless of DB registration order.

---

### Fix 3: Add Negative Balance Detection as Error Condition

Since negative chip balances are impossible in practice, treat them as data integrity errors:

```csharp
if (totalChips < 0)
{
    _logger.LogError(
        "CRITICAL: Negative chip balance detected for manager {ManagerId} at {Year}-{Month}. " +
        "TxId={TransactionId}, Amount={Amount}, ResultingChips={Chips}. " +
        "This indicates data inconsistency - transactions may be missing or duplicated.",
        pokerManagerId, year, month, tx.Id, tx.AssetAmount, totalChips);
    
    throw new BusinessException(
        $"Negative chip balance detected for manager {pokerManagerId}. " +
        "Please review transaction history for data integrity issues.");
}
```

**Note**: During development/testing, you may want to log + clamp instead of throwing. The throw behavior should be enabled once data integrity is confirmed.

---

## Implementation Steps

### Phase 1: Fix Logic Inconsistency

**File**: `Application/Services/Finance/AvgRateService.cs`

1. [x] Update `CalculateSingleMonth()` receive logic (lines 340-346) to match `CalculateAvgRateUpToDate()` pattern
2. [x] Verify the obsolete `CalculateMonthlySnapshot()` method has the same fix (or remove it)

**Estimated effort**: 15 minutes

---

### Phase 2: Implement Receives-First Ordering

**File**: `Application/Services/Finance/AvgRateService.cs`

1. [x] Create helper ordering helpers to classify receive/send direction
2. [x] Update `CalculateSingleMonth()` transaction ordering to use receives-first logic
3. [x] Update `CalculateAvgRateUpToDate()` transaction ordering to use receives-first logic
4. [x] Keep `GetTransactionsForMonth()` as retrieval-only and centralize ordering in processing flow

**Estimated effort**: 30 minutes

---

### Phase 3: Add Negative Balance Error Handling

**File**: `Application/Services/Finance/AvgRateService.cs`

1. [x] Replace clamp-to-zero logic with error logging + throw
2. [x] Add similar check in `CalculateAvgRateUpToDate()`
3. [x] Keep fail-fast behavior without config toggle (explicitly chosen for data integrity)

**Estimated effort**: 20 minutes

---

### Phase 4: Invalidate Caches and Test

1. [ ] Clear all AvgRate caches after deployment (operational step for rollout)
2. [ ] Test with the problematic transactions (pending live validation):
   - Transaction `eaec1b53` (plain transfer, no Rate)
   - Transaction `d1493b65` (1020 chips with 2% Rate)
3. [ ] Verify RateFees now calculates correctly (~100 BRL for 20 chips × AvgRate 5)
4. [ ] Verify SpreadProfit calculations are unaffected

**Estimated effort**: 30 minutes

---

### Phase 5: Update Documentation

1. [x] Update `PROFIT_CALCULATION_SYSTEM.md` to document the receives-first ordering
2. [x] Update `ASSET_VALUATION_RULES.md` to note the ordering guarantee
3. [x] Add known limitation: if actual business order matters within a day, manual timestamp adjustment may be needed

**Estimated effort**: 15 minutes

---

## Code Changes Summary

### AvgRateService.cs - CalculateSingleMonth()

**Before:**
```csharp
foreach (var tx in transactions.OrderBy(t => t.Date).ThenBy(t => t.CreatedAt))
{
    // ...
    if (isReceiving && tx.ConversionRate.HasValue)
    {
        totalChips += tx.AssetAmount;
        totalCost += tx.AssetAmount * tx.ConversionRate.Value;
    }
    else if (isSending)
    {
        // ... clamp to zero ...
    }
}
```

**After:**
```csharp
foreach (var tx in transactions
    .OrderBy(t => t.Date.Date)
    .ThenBy(t => walletIds.Contains(t.ReceiverWalletIdentifierId) ? 0 : 1)  // Receives first
    .ThenBy(t => t.CreatedAt))
{
    // ...
    if (isReceiving)
    {
        var currentAvgRate = totalChips > 0 ? totalCost / totalChips : 0;
        var receivePrice = tx.ConversionRate ?? currentAvgRate;
        totalChips += tx.AssetAmount;
        totalCost += tx.AssetAmount * receivePrice;
    }
    else if (isSending)
    {
        if (totalChips > 0)
        {
            var proportion = tx.AssetAmount / totalChips;
            totalCost -= totalCost * proportion;
            totalChips -= tx.AssetAmount;
        }
        
        if (totalChips < 0)
        {
            _logger.LogError("CRITICAL: Negative balance for {ManagerId}...", ...);
            throw new BusinessException("Negative chip balance detected...");
        }
    }
}
```

---

## Testing Scenarios

### Scenario A: Rate-based buy after inventory depletion
- Start: 900 chips @ 5.0
- Day 1: Sell 900 chips (balance → 0)
- Day 2: Buy 1020 chips with 2% Rate, no ConversionRate
- **Expected**: Receives-first doesn't help here (different days), but fallback to current AvgRate (0) means no cost basis. This is a known limitation when inventory fully depletes.

### Scenario B: Same-day sell and buy registered out of order
- Start: 900 chips @ 5.0
- Day 1: Buy 1020 chips (registered at 22:41) + Sell 1000 chips (registered at 22:40)
- **Expected with fix**: Process buy first → 1920 chips, then sell → 920 chips. AvgRate preserved.

### Scenario C: Transaction with ConversionRate
- Any receive with ConversionRate should use that rate regardless of ordering.
- **Expected**: No change in behavior, ConversionRate always takes precedence.

---

## Rollback Plan

If issues are discovered after deployment:
1. Revert the code changes
2. Clear AvgRate caches
3. Re-enable clamp-to-zero behavior temporarily
4. Investigate specific transaction sequences causing problems

---

## Related Files

- `Application/Services/Finance/AvgRateService.cs` - Main changes
- `Application/Services/Finance/ProfitCalculationService.cs` - Consumes AvgRate
- `Documentation/03_CORE_SYSTEMS/PROFIT_CALCULATION_SYSTEM.md` - Documentation update
- `Documentation/08_BUSINESS_RULES/ASSET_VALUATION_RULES.md` - Documentation update
