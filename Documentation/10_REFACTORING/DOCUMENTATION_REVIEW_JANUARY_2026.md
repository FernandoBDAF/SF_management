# Documentation Review - January 2026

> **Status:** Comprehensive Review  
> **Created:** January 24, 2026  
> **Purpose:** Identify documentation issues, gaps, and improvements before balance system refactoring

---

## Executive Summary

This review identifies documentation inaccuracies, gaps, and missing documents based on validated business rules. The issues are categorized by priority and document.

### Key Findings

| Category | Count | Priority |
|----------|-------|----------|
| Critical Bugs (Code) | 1 | 🔴 Immediate |
| Business Rule Errors | 8 | 🟡 High |
| Incomplete Explanations | 12 | 🟡 High |
| Missing Documents | 3 | 🟢 Medium |
| Deferred Items | 5 | 🔵 Future |

---

## Table of Contents

1. [Critical Code Bug](#critical-code-bug)
2. [Document-by-Document Review](#document-by-document-review)
3. [Missing Documents](#missing-documents)
4. [Implementation Decisions to Document](#implementation-decisions-to-document)
5. [Deferred Work Documentation](#deferred-work-documentation)
6. [Action Plan](#action-plan)

---

## Critical Code Bug

### 🔴 SettlementTransaction Balance Calculation (BaseAssetHolderService.cs)

**Location:** Lines 518-536 (`GetBalancesByAssetType`) and 679-697 (`GetBalancesByAssetGroup`)

**Current Wrong Code:**
```csharp
foreach (var tx in settlementTransactions)
{
    var signedAmount = tx.GetSignedAmountForWalletIdentifier(relevantWalletId);
    balances[assetType] += signedAmount;  // WRONG - uses AssetAmount
}
```

**Problem:** 
- Uses `AssetAmount` (from `BaseTransaction`) for balance
- `AssetAmount` represents chips that ALREADY flowed via `DigitalAssetTransactions`
- This causes **double-counting** of chip movements

**Correct Logic:**

| Entity | Balance Impact | Field Used |
|--------|---------------|------------|
| **Client** | `+RakeAmount * (RakeBack / 100)` | RakeBack |
| **PokerManager** | `-RakeAmount * (RakeCommission / 100)` | RakeCommission |

**Finance Module Impact (Document for Later):**
- Company profit = `RakeAmount * ((RakeCommission - RakeBack) / 100)`

**Implementation Decision Needed:**

How to determine if we're calculating for Client vs PokerManager:

| Option | Approach | Tradeoff |
|--------|----------|----------|
| A | Check if `baseAssetHolderId` is a PokerManager | Extra DB query per call |
| B | Use wallet's `AssetGroup` to infer | Indirect, may not be reliable |
| C | Method context already determines | `GetBalancesByAssetType` = non-PM, `GetBalancesByAssetGroup` = PM |

**Recommendation:** Document Option C as the approach - the calling method already determines behavior.

---

## Document-by-Document Review

### 01_BUSINESS/BUSINESS_DOMAIN_OVERVIEW.md

| Line | Issue | Type | Fix |
|------|-------|------|-----|
| 13 | "Poker managers operate as intermediaries" | ❌ Misleading | "PokerManagers hold assets ON BEHALF OF the company" |
| 17 | "Track commissions - Earning rake" | ❌ Wrong | "The COMPANY earns rake/commission via PokerManager" |
| 41-45 | Asset Holder descriptions vague | ⚠️ Incomplete | Add clarifications below |
| 95-123 | Financial flows incomplete | ⚠️ Incomplete | Add BalanceAs impact |

**Corrections Needed:**

```markdown
## Domain Entities

### Asset Holders

| Entity | Description | Key Use | Balance Meaning |
|--------|-------------|---------|-----------------|
| **Client** | Poker players who buy/sell chips **via PokerManager** | Buy/sell chips | Positive = company OWES them |
| **Bank** | **Company's own bank accounts** | Hold company's fiat | Positive = money company HAS |
| **Member** | Team members with Share%/Salary (TBD for financial module) | Like clients + profit sharing | Positive = company OWES them |
| **PokerManager** | **Holds poker assets ON BEHALF OF company** | Company's poker inventory | Positive = company's inventory |
```

---

### 03_CORE_SYSTEMS/ENTITY_INFRASTRUCTURE.md

| Line | Issue | Type | Fix |
|------|-------|------|-----|
| 103-118 | Client description missing balance semantics | ⚠️ Incomplete | Add positive/negative meaning |
| 119-132 | Bank says "banking institutions" | ❌ Wrong | "Company's own bank account" |
| 133-156 | Member has Share/Salary without explanation | ⚠️ Incomplete | Add TBD note for financial module |
| 157-169 | PokerManager missing business context | ⚠️ Incomplete | Add "holds for company" and ManagerProfitType |

**Add section: Business Behavior by Entity Type**

```markdown
## Business Behavior by Entity Type

### Company Asset Holders
These entities hold assets **on behalf of the company**:

| Entity | AssetGroups | Balance Meaning | AccountClassification |
|--------|-------------|-----------------|----------------------|
| **Bank** | FiatAssets only | Positive = company HAS money | ASSET |
| **PokerManager** | PokerAssets, FiatAssets | Positive = company's inventory | ASSET |
| *(Future: CryptoManager)* | CryptoAssets | Positive = company's inventory | ASSET |

### Business Partners
These entities do business **with** the company:

| Entity | Balance Meaning | AccountClassification | Credit Behavior |
|--------|-----------------|----------------------|-----------------|
| **Client** | Positive = company OWES them | LIABILITY | Can go negative (credit line) |
| **Member** | Positive = company OWES them | LIABILITY | Can go negative (credit line) |

### Special Behaviors

**PokerManager in FiatTransactions**: Acts like a Client (balance = what company owes/is owed by PM)

**PokerManager with Conversion Wallet**: Acts like a Client for self-conversion (dual-balance impact)
```

---

### 03_CORE_SYSTEMS/SETTLEMENT_WORKFLOW.md

| Line | Issue | Type | Fix |
|------|-------|------|-----|
| 17 | "Records the rake earned by the manager" | ❌ Wrong | "Records rake, with company earning commission" |
| 22-27 | SettlementTransaction model incomplete | ❌ Wrong | Add RakeCommission, RakeBack fields |
| All | Missing balance impact explanation | ⚠️ Missing | Add complete section |
| All | Missing company profit formula | ⚠️ Missing | Add finance module section |

**Add section: SettlementTransaction Fields and Balance Impact**

```markdown
## SettlementTransaction Fields

```csharp
public class SettlementTransaction : BaseTransaction
{
    // From BaseTransaction: AssetAmount (chips that flowed - for tracking only)
    
    [Required] public decimal RakeAmount { get; set; }      // Total rake client paid to poker site
    [Required] public decimal RakeCommission { get; set; }  // % of rake poker site pays company
    public decimal? RakeBack { get; set; }                  // % of rake company returns to client
}
```

### Balance Impact

**⚠️ Important:** `AssetAmount` should NOT be used for balance calculation - it represents chips that already flowed via DigitalAssetTransactions.

| Entity | Balance Impact | Formula |
|--------|---------------|---------|
| **Client** | Receives rakeback | `+RakeAmount * (RakeBack / 100)` |
| **PokerManager** | Company earns commission | `-RakeAmount * (RakeCommission / 100)` |

### Company Profit (Finance Module - TBD)

```
Company Profit per Settlement = RakeAmount * ((RakeCommission - RakeBack) / 100)
```

Example:
- RakeAmount: 1000 chips
- RakeCommission: 50%
- RakeBack: 10%
- Company Profit: 1000 * ((50 - 10) / 100) = 400 chips
```

---

### 06_API/BALANCE_ENDPOINTS.md

| Line | Issue | Type | Fix |
|------|-------|------|-----|
| 163 | "Settlement Transactions - SettlementTransaction records" | ❌ Wrong | Add warning about correct formula |
| All | Missing SettlementTransaction correct formula | ⚠️ Missing | Add section |
| All | Missing current bug warning | ⚠️ Missing | Add warning |

**Add section: SettlementTransaction Balance Logic**

```markdown
## SettlementTransaction Balance Logic

⚠️ **IMPORTANT BUG:** Current implementation incorrectly uses `AssetAmount` for settlement balance.

### Correct Balance Calculation

| Method | Entity Type | Formula |
|--------|-------------|---------|
| `GetBalancesByAssetType` | Client/Member | `+RakeAmount * (RakeBack / 100)` |
| `GetBalancesByAssetGroup` | PokerManager | `-RakeAmount * (RakeCommission / 100)` |

The `AssetAmount` field represents chips that already flowed via `DigitalAssetTransactions` and should NOT be used for balance to avoid double-counting.

See [BALANCE_SYSTEM_ANALYSIS.md](../10_REFACTORING/BALANCE_SYSTEM_ANALYSIS.md) for implementation plan.
```

---

### 07_REFERENCE/ENUMS_AND_TYPE_SYSTEM.md

| Line | Issue | Type | Fix |
|------|-------|------|-----|
| 159-166 | AccountClassification missing business context | ⚠️ Incomplete | Add usage by entity type |
| 479-482 | ManagerProfitType descriptions incomplete | ⚠️ Incomplete | Add detailed explanations |

**Update AccountClassification section:**

```markdown
### AccountClassification - Business Usage

| Value | Used By | Balance Meaning |
|-------|---------|-----------------|
| ASSET | Banks, PokerManagers | Positive = company OWNS this |
| LIABILITY | Clients, Members | Positive = company OWES this |

**Transaction Behavior:**
- When both wallets have SAME classification: Standard sign convention
- When classifications DIFFER: LIABILITY wallet gets sign inverted
```

**Update ManagerProfitType section:**

```markdown
### ManagerProfitType - Business Meaning

| Value | Name | Description | How Company Profits |
|-------|------|-------------|---------------------|
| 0 | Spread | Profit from price spread | Different ConversionRate on buy vs sell |
| 1 | RakeOverrideCommission | Commission from poker site | RakeCommission % in SettlementTransaction |

**Note:** The COMPANY earns these profits, using the PokerManager as the asset holder.
```

---

### 03_CORE_SYSTEMS/TRANSACTION_INFRASTRUCTURE.md

| Line | Issue | Type | Fix |
|------|-------|------|-----|
| 310-314 | SALE/PURCHASE descriptions | ⚠️ Imprecise | "Client buys/sells via PokerManager" |
| All | Missing BalanceAs vs Coin Balance explanation | ⚠️ Missing | Add section |
| All | Rate vs ConversionRate unclear | ⚠️ Missing | Add clarification |

**Add section: BalanceAs vs Coin Balance Scenarios**

```markdown
## Transaction Scenarios: BalanceAs vs Coin Balance

### With BalanceAs (Standard)

When `BalanceAs` and `ConversionRate` are set:
- Client's balance in the **BalanceAs asset** is impacted
- Client's balance in the **transaction asset** is NOT impacted
- PokerManager's balance in transaction asset IS impacted

**Example: Client buys 1000 chips at 5.0 BRL**
```
Transaction: PM → Client (PokerStars)
BalanceAs: BRL, ConversionRate: 5.0

Impacts:
- PM PokerAssets: -1000 (company inventory decreased)
- Client PokerAssets: NO CHANGE
- Client BRL: -5000 (owes company)
```

### Without BalanceAs (Coin Balance)

When `BalanceAs` is null:
- Client's balance in the **transaction asset** is impacted
- Represents debt in the SAME asset type
- `Rate` applies as a fee percentage

**Example: Client buys 1000 chips with 5% rate**
```
Transaction: PM → Client (PokerStars)
BalanceAs: null, Rate: 5%

Impacts:
- PM PokerAssets: -1000
- Client PokerAssets: -1050 (owes 1000 + 5% fee in chips)
```

### Rate vs ConversionRate

| Field | Purpose | Applied To |
|-------|---------|------------|
| `ConversionRate` | Currency exchange rate | BalanceAs calculation |
| `Rate` | Fee/discount percentage | Coin Balance scenarios |
```

---

## Missing Documents

### 1. 📄 ENTITY_BUSINESS_BEHAVIOR.md (New Document)

**Location:** `03_CORE_SYSTEMS/ENTITY_BUSINESS_BEHAVIOR.md`

**Purpose:** Document business behavior of each entity type, who earns, who owes, balance meanings

**Contents:**
- Company Asset Holders vs Business Partners distinction
- Balance meaning for each entity
- AccountClassification usage
- Special behaviors (PM in FiatTransactions, Conversion Wallet)
- Future entity types (CryptoManager)

---

### 2. 📄 TRANSACTION_BALANCE_IMPACT.md (New Document)

**Location:** `03_CORE_SYSTEMS/TRANSACTION_BALANCE_IMPACT.md`

**Purpose:** Single source of truth for how each transaction type impacts balances

**Contents:**
- FiatTransaction balance rules
- DigitalTransaction with BalanceAs
- DigitalTransaction without BalanceAs (Coin Balance)
- SettlementTransaction balance rules (correct formula)
- Self-Conversion dual-balance rules
- Examples for each scenario

---

### 3. 📄 FINANCE_MODULE_PLANNING.md (New Document)

**Location:** `10_REFACTORING/FINANCE_MODULE_PLANNING.md`

**Purpose:** Document deferred financial features and business rules

**Contents:**
- Company profit calculation (from Spread, RakeCommission)
- Member Share % implementation
- Member Salary implementation
- Referral commission integration
- Credit limit management
- ManagerProfitType implementation details

---

## Implementation Decisions to Document

### Decision 1: SettlementTransaction Balance Calculation

**Context:** Need to determine Client vs PokerManager for correct formula

**Options:**

| Option | Approach | Pros | Cons |
|--------|----------|------|------|
| A | Query DB to check if holder is PokerManager | Explicit, clear | Extra DB call, performance |
| B | Infer from wallet's AssetGroup | No extra query | Indirect, potentially unreliable |
| C | Method context determines | Already separated | Implicit, may break if refactored |

**Recommendation:** Option C - `GetBalancesByAssetType` is for non-PM, `GetBalancesByAssetGroup` is for PM

**Document this decision in:** `BALANCE_SYSTEM_ANALYSIS.md`

---

### Decision 2: Client Credit Limit

**Context:** Clients can have negative balances (owe company)

**Future Feature:**
- Add `CreditLimit` field to Client entity
- Validate in TransferService before allowing negative balance
- Personalized per client

**Document this decision in:** `FINANCE_MODULE_PLANNING.md`

---

## Deferred Work Documentation

### Items to Document for Future Reference

| Item | Current State | Future Work | Document In |
|------|---------------|-------------|-------------|
| **Referral Balance Impact** | Referrals exist but don't affect balance | Integrate referral commission into balance calculation | FINANCE_MODULE_PLANNING.md |
| **Member Share %** | Field exists, unused | Define: share of what, when calculated | FINANCE_MODULE_PLANNING.md |
| **Member Salary** | Field exists, unused | Define: how tracked/paid | FINANCE_MODULE_PLANNING.md |
| **ManagerProfitType** | Code exists, needs refactoring | Clean implementation of Spread vs RakeOverride | FINANCE_MODULE_PLANNING.md |
| **Settlement Zeroing** | Not implemented | Consider periodic position zeroing | FINANCE_MODULE_PLANNING.md |
| **CryptoManager** | Not implemented | Future entity like PokerManager for crypto | ENTITY_BUSINESS_BEHAVIOR.md |

---

## Action Plan

### Phase 1: Critical Fixes (Immediate) ✅ COMPLETED

1. ✅ **Document the SettlementTransaction bug** - Done in BALANCE_SYSTEM_ANALYSIS.md
2. ✅ **Add implementation decision for balance calculation** - Done in BALANCE_SYSTEM_ANALYSIS.md (Options A/B/C)
3. ✅ **Add finance module impact to documentation** - Done in BALANCE_SYSTEM_ANALYSIS.md + FINANCE_MODULE_PLANNING.md

### Phase 1.5: New Critical Bug (Discovered During Review)

4. ⬜ **AccountClassification Bug** - PokerManager FiatAssets wallets incorrectly classified as ASSET instead of LIABILITY
   - See [ACCOUNTCLASSIFICATION_BUG_FIX_PLAN.md](./ACCOUNTCLASSIFICATION_BUG_FIX_PLAN.md) for full plan

### Phase 2: Document Corrections ✅ COMPLETED

| Document | Priority | Status |
|----------|----------|--------|
| SETTLEMENT_WORKFLOW.md | High | ✅ Fixed |
| BALANCE_ENDPOINTS.md | High | ✅ Fixed |
| BUSINESS_DOMAIN_OVERVIEW.md | High | ✅ Fixed |
| ENTITY_INFRASTRUCTURE.md | High | ✅ Fixed |
| TRANSACTION_INFRASTRUCTURE.md | Medium | ✅ Fixed |
| ENUMS_AND_TYPE_SYSTEM.md | Medium | ✅ Fixed |

### Phase 3: Create Missing Documents ✅ COMPLETED

| Document | Priority | Status |
|----------|----------|--------|
| ENTITY_BUSINESS_BEHAVIOR.md | High | ✅ Created |
| TRANSACTION_BALANCE_IMPACT.md | High | ✅ Created |
| FINANCE_MODULE_PLANNING.md | Medium | ✅ Created |

### Phase 4: Code Fixes (After Documentation)

1. Fix SettlementTransaction balance calculation in BaseAssetHolderService.cs
2. Add unit tests for balance calculation
3. Verify existing balance calculations are correct

---

## Summary

### What We Must Do Now

1. ✅ **Complete documentation corrections** listed above
2. ✅ **Create missing documents** for entity behavior and balance impact
3. ✅ **Document deferred work** in FINANCE_MODULE_PLANNING.md

### What We Document for Later

1. Referral system integration with balance
2. Member Share/Salary implementation  
3. Client credit limit management
4. ManagerProfitType refactoring
5. Company profit tracking (Finance Module)

---

## Post-Review Updates

### Wallet Creation UX Refactor (January 24, 2026)

A separate session implemented the Wallet Creation UX Refactor plan. The following documentation was updated to reflect these changes:

| Document | Updates |
|----------|---------|
| `TRANSACTION_INFRASTRUCTURE.md` | Updated guardrails section, deprecated `CreateWalletsIfMissing`, added AssetGroup restriction |
| `API_REFERENCE.md` | Updated features list, added deprecation notice |
| `ENTITY_BUSINESS_BEHAVIOR.md` | Updated TRANSFER mode to note Internal-only restriction |
| `TRANSACTION_BALANCE_IMPACT.md` | Updated TRANSFER example with restriction note |
| `TRANSFER_ENDPOINT_IMPLEMENTATION_PLAN.md` | Added historical document notice |
| `TRANSACTION_DOCUMENTATION_IMPROVEMENT_PLAN.md` | Added historical document notice |

**Key Changes:**
- `createWalletsIfMissing` flag is **deprecated** - backend rejects if true
- TRANSFER mode restricted to **Internal wallets only** (AssetGroup 4)
- Wallet creation is now explicit via dedicated API before transfer

---

*Created: January 24, 2026*
*Last Updated: January 24, 2026*
*Purpose: Guide documentation improvements before balance system refactoring*
