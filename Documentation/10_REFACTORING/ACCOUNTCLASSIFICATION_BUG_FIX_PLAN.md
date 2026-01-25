# AccountClassification Bug Fix Plan

> **Status:** Planning  
> **Created:** January 24, 2026  
> **Priority:** High  
> **Discovered During:** Balance System Documentation Review

---

## Table of Contents

- [Executive Summary](#executive-summary)
- [Bug Description](#bug-description)
- [Business Rules (Validated)](#business-rules-validated)
- [Current vs Expected Behavior](#current-vs-expected-behavior)
- [Impact Analysis](#impact-analysis)
- [Proposed Fix](#proposed-fix)
- [Implementation Plan](#implementation-plan)
- [Testing Strategy](#testing-strategy)
- [Related Issues](#related-issues)

---

## Executive Summary

The `DetermineAccountClassificationAsync` method in `TransferService.cs` incorrectly assigns `AccountClassification.ASSET` to **all** PokerManager wallets, regardless of AssetGroup. This causes incorrect balance sign calculations for PokerManager's FiatAssets wallets.

**Root Cause:** Classification is determined by entity type only, not by wallet's AssetGroup.

**Fix:** Classification should consider both entity type AND AssetGroup.

---

## Bug Description

### Location

**File:** `SF_management/Application/Services/Transactions/TransferService.cs`  
**Lines:** 342-351

### Current Code

```csharp
/// <summary>
/// Determines account classification: Banks/PokerManagers = ASSET, Clients/Members = LIABILITY.
/// </summary>
private async Task<AccountClassification> DetermineAccountClassificationAsync(Guid assetHolderId)
{
    var isBank = await _context.Banks.AnyAsync(b => b.BaseAssetHolderId == assetHolderId);
    var isPokerManager = await _context.PokerManagers.AnyAsync(pm => pm.BaseAssetHolderId == assetHolderId);
    
    return (isBank || isPokerManager) ? AccountClassification.ASSET : AccountClassification.LIABILITY;
}
```

### Problem

The method only checks entity type, not the wallet's AssetGroup. This means:
- **All** Bank wallets → ASSET ✅ (correct, banks only have FiatAssets)
- **All** PokerManager wallets → ASSET ❌ (incorrect for FiatAssets)
- **All** Client/Member wallets → LIABILITY ✅ (correct)

---

## Business Rules (Validated)

Based on business analysis, the correct classification rules are:

### PokerManager Wallet Classifications

| AssetGroup | AccountClassification | Rationale |
|------------|----------------------|-----------|
| **PokerAssets** | **ASSET** | Company HOLDS chips via PM |
| **FiatAssets** | **LIABILITY** | Company OWES PM (when PM has positive balance) |
| **Internal** | **ASSET** | Represents PM's external/personal position entering system |
| **Settlements** | **ASSET** | Settlement tracking |

### Why FiatAssets = LIABILITY for PokerManager

When a PokerManager has a positive FiatAssets balance:

1. **Self-Conversion Scenario:**
   - PM deposits own chips (Internal → PokerAssets)
   - Company now holds chips FOR the company
   - Company OWES the PM the BRL equivalent
   - PM's FiatAssets increases → This is a LIABILITY (company owes)

2. **FiatTransaction Scenario:**
   - PM participates in FiatTransactions
   - Acts exactly like a Client
   - Positive balance = company owes PM

### Why Internal = ASSET

The Internal wallet represents the PM's "external" position:
- Chips outside the managed system
- When chips flow FROM Internal TO PokerAssets, they "enter" the company
- The Internal wallet itself is conceptually company-owned for this purpose
- Classification as ASSET ensures correct sign convention

---

## Current vs Expected Behavior

### Classification Matrix

| Entity | AssetGroup | Current | Expected | Status |
|--------|------------|---------|----------|--------|
| Bank | FiatAssets | ASSET | ASSET | ✅ OK |
| PokerManager | PokerAssets | ASSET | ASSET | ✅ OK |
| PokerManager | FiatAssets | ASSET | **LIABILITY** | ❌ BUG |
| PokerManager | Internal | ASSET | ASSET | ✅ OK |
| PokerManager | Settlements | ASSET | ASSET | ✅ OK |
| Client | Any | LIABILITY | LIABILITY | ✅ OK |
| Member | Any | LIABILITY | LIABILITY | ✅ OK |

### Balance Sign Impact

When a PokerManager receives BRL (e.g., from a RECEIPT transaction as the receiver):

**Current (Bug):**
```
PM classified as ASSET
PM receives 5000 BRL
Different classifications (sender is LIABILITY)
Sign inversion may apply incorrectly
```

**Expected (Fixed):**
```
PM's FiatAssets wallet classified as LIABILITY
Same classification as sender (Client = LIABILITY)
No sign inversion needed
Both balances increase correctly
```

---

## Impact Analysis

### Affected Scenarios

1. **PokerManager in FiatTransactions**
   - When PM is sender/receiver of FiatAssetTransaction
   - Balance sign may be calculated incorrectly

2. **Self-Conversion Settlement**
   - After self-conversion, PM's FiatAssets balance exists
   - Later FiatTransactions to settle may have wrong signs

3. **Balance Reports**
   - PM's FiatAssets balance may show incorrect values
   - Affects financial reconciliation

### Scope of Impact

| Area | Impact Level | Notes |
|------|--------------|-------|
| New wallet creation | High | All new PM FiatAssets wallets get wrong classification |
| Existing wallets | Unknown | Need to check existing data |
| Balance calculation | High | Sign convention affected |
| Transaction creation | Medium | New transactions may have wrong signs |

### Data Migration Consideration

Existing PokerManager FiatAssets wallets may have incorrect `AccountClassification`. May need:
1. Query to identify affected wallets
2. Migration script to fix classification
3. Recalculation of balances (or verification they're still correct)

---

## Proposed Fix

### Option A: Simple AssetGroup Check (Recommended)

Modify `DetermineAccountClassificationAsync` to accept AssetType and determine based on both entity type and asset group.

```csharp
/// <summary>
/// Determines account classification based on entity type AND asset group.
/// - Banks: Always ASSET (only hold FiatAssets)
/// - PokerManagers: ASSET for PokerAssets/Internal/Settlements, LIABILITY for FiatAssets
/// - Clients/Members: Always LIABILITY
/// </summary>
private async Task<AccountClassification> DetermineAccountClassificationAsync(
    Guid assetHolderId, 
    AssetType assetType)
{
    var isBank = await _context.Banks.AnyAsync(b => b.BaseAssetHolderId == assetHolderId);
    if (isBank)
        return AccountClassification.ASSET;
    
    var isPokerManager = await _context.PokerManagers.AnyAsync(pm => pm.BaseAssetHolderId == assetHolderId);
    if (isPokerManager)
    {
        var assetGroup = WalletIdentifierValidationService.GetAssetGroupForAssetType(assetType);
        // PM's FiatAssets = LIABILITY (company owes PM)
        // PM's PokerAssets/Internal/Settlements = ASSET (company holds)
        return assetGroup == AssetGroup.FiatAssets 
            ? AccountClassification.LIABILITY 
            : AccountClassification.ASSET;
    }
    
    // Clients and Members are always LIABILITY
    return AccountClassification.LIABILITY;
}
```

### Option B: Lookup-Based Classification

Create a mapping table/configuration for entity type + asset group → classification.

**Pros:** More flexible, easier to change rules
**Cons:** More complex, may be over-engineered

### Recommendation

**Option A** is recommended for simplicity. The logic is clear and matches business rules directly.

---

## Implementation Plan

### Phase 1: Documentation & Analysis

| Task | Status | Notes |
|------|--------|-------|
| Document the bug | ✅ Done | This document |
| Identify affected code | ✅ Done | TransferService.cs:345-350 |
| Document business rules | ✅ Done | See above |
| Check existing data | ⬜ Pending | Query for affected wallets |

### Phase 2: Code Fix

| Task | Status | Notes |
|------|--------|-------|
| Modify `DetermineAccountClassificationAsync` | ⬜ Pending | Add AssetType parameter |
| Update method signature | ⬜ Pending | |
| Update callers | ⬜ Pending | `FindOrCreateWalletAsync` |
| Add unit tests | ⬜ Pending | Test all classification scenarios |

### Phase 3: Data Migration (If Needed)

| Task | Status | Notes |
|------|--------|-------|
| Query affected wallets | ⬜ Pending | PM wallets with FiatAssets + ASSET classification |
| Create migration script | ⬜ Pending | Update to LIABILITY |
| Test migration | ⬜ Pending | Dev environment |
| Execute migration | ⬜ Pending | Production |

### Phase 4: Verification

| Task | Status | Notes |
|------|--------|-------|
| Verify balance calculations | ⬜ Pending | Compare before/after |
| Test FiatTransaction scenarios | ⬜ Pending | PM as sender/receiver |
| Test self-conversion flow | ⬜ Pending | End-to-end |

---

## Testing Strategy

### Unit Tests

```csharp
[Fact]
public async Task DetermineAccountClassification_Bank_ReturnsAsset()
{
    // Banks always ASSET
}

[Fact]
public async Task DetermineAccountClassification_PokerManager_PokerAssets_ReturnsAsset()
{
    // PM PokerAssets = ASSET
}

[Fact]
public async Task DetermineAccountClassification_PokerManager_FiatAssets_ReturnsLiability()
{
    // PM FiatAssets = LIABILITY (this is the bug fix)
}

[Fact]
public async Task DetermineAccountClassification_PokerManager_Internal_ReturnsAsset()
{
    // PM Internal = ASSET
}

[Fact]
public async Task DetermineAccountClassification_Client_ReturnsLiability()
{
    // Clients always LIABILITY
}

[Fact]
public async Task DetermineAccountClassification_Member_ReturnsLiability()
{
    // Members always LIABILITY
}
```

### Integration Tests

1. **PM FiatTransaction as Receiver**
   - Create PM with FiatAssets wallet
   - Create FiatTransaction (Client → PM)
   - Verify balance signs are correct

2. **Self-Conversion with Settlement**
   - PM self-conversion (Internal → PokerAssets)
   - Verify FiatAssets balance created
   - FiatTransaction to settle
   - Verify balances zero out correctly

---

## Related Issues

### Connected to SettlementTransaction Bug

This bug was discovered during the balance system analysis. It's separate from but related to the SettlementTransaction balance bug:

| Bug | Location | Impact |
|-----|----------|--------|
| SettlementTransaction uses AssetAmount | BaseAssetHolderService.cs | Double-counting |
| AccountClassification for PM FiatAssets | TransferService.cs | Wrong sign convention |

Both should be fixed as part of the balance system improvement.

### Documentation to Update After Fix

- `ENTITY_BUSINESS_BEHAVIOR.md` - Update the Entity Categories diagram
- `TRANSACTION_BALANCE_IMPACT.md` - Add AccountClassification rules
- `ENUMS_AND_TYPE_SYSTEM.md` - Update AccountClassification business usage table

---

## Questions for Review

1. **Data Migration:** Are there existing PokerManager FiatAssets wallets that need correction?

2. **CryptoManager (Future):** When implemented, should follow same pattern:
   - CryptoAssets → ASSET
   - FiatAssets → LIABILITY (if PM can have both)

3. **Edge Cases:** Any scenarios where PM's FiatAssets should be ASSET?

---

*Created: January 24, 2026*
*Last Updated: January 24, 2026*
