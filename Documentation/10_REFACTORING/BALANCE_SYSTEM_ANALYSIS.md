# Balance System Analysis

> **Status:** Validated Analysis Document  
> **Created:** January 24, 2026  
> **Updated:** January 24, 2026 (post-validation)  
> **Purpose:** Document asset holders, transaction meanings, and balance semantics

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Asset Holders: Who They Are](#asset-holders-who-they-are)
3. [Asset Types and Groups](#asset-types-and-groups)
4. [Transactions: What They Mean](#transactions-what-they-mean)
5. [Balance Calculation: How It Works](#balance-calculation-how-it-works)
6. [Business Flow Examples](#business-flow-examples)
7. [Critical Bugs and Improvements](#critical-bugs-and-improvements)
8. [Deferred Work](#deferred-work)

---

## Executive Summary

SF Management is a financial system for the **poker management business**. The core business model:

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                     THE POKER MANAGEMENT BUSINESS MODEL                      │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  COMPANY ASSET HOLDERS (Hold assets FOR the company)                        │
│  ═══════════════════════════════════════════════════                        │
│                                                                             │
│  PokerManager = Company's Poker Asset Holder                                │
│  • Holds poker chips ON BEHALF OF the company                               │
│  • Facilitates chip ↔ fiat conversions for Clients                          │
│  • The COMPANY earns from spread/rake through them                          │
│  • Positive balance = AccountClassification.ASSET (company owns)            │
│  • Negative balance = AccountClassification.LIABILITY (rare)                │
│                                                                             │
│  Bank = Company's Fiat Asset Holder                                         │
│  • The company's OWN bank account                                           │
│  • Holds fiat currency (BRL, USD) FOR the company                           │
│  • Positive balance = money the company HAS                                 │
│  • Negative balance = company owes the bank (overdraft)                     │
│                                                                             │
│  (Future: CryptoManager = Company's Crypto Asset Holder)                    │
│                                                                             │
│  ───────────────────────────────────────────────────────────────────────    │
│                                                                             │
│  BUSINESS PARTNERS (Transact WITH the company)                              │
│  ═════════════════════════════════════════════                              │
│                                                                             │
│  Client = Poker Player / Customer                                           │
│  • Buys/sells poker chips via PokerManager                                  │
│  • Positive balance = company OWES them                                     │
│  • Negative balance = they OWE the company (credit line)                    │
│                                                                             │
│  Member = Team Member / Business Partner                                    │
│  • Like Client but part of the team                                         │
│  • Has Share % and/or Salary (TBD for financial module)                     │
│  • Transacts exactly like Clients                                           │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Asset Holders: Who They Are

### 1. Client (Poker Player) ✅ VALIDATED

| Aspect | Validated Definition |
|--------|---------------------|
| **Role** | External customer who buys/sells poker chips **via PokerManager** |
| **Attributes** | Name, Tax info (CPF/CNPJ), Birthday |
| **Balance View** | By AssetType (BrazilianReal, PokerStars, etc.) |
| **Typical Wallets** | FiatAssets (BRL), PokerAssets (poker chips) |

**Business Rules:**
- **Positive balance** = Company OWES the client ✓
- **Negative balance** = Client OWES the company (uses credit line) ✓
- Credit limit management needed (personalized per client) - **TODO**

**Transaction Behavior:**
- When **BalanceAs is set**: Only BalanceAs balance is impacted (not PokerAssets)
- When **BalanceAs is NOT set**: Client's balance on the SAME asset is impacted, representing debt in that asset
- **Rate** applies when no BalanceAs: 1000 chips with 5% rate = **-1050** on client balance

---

### 2. Bank (Company's Fiat Holder) ✅ VALIDATED

| Aspect | Validated Definition |
|--------|---------------------|
| **Role** | **Company's own bank account** (not external bank) |
| **Attributes** | Name, Tax info, Bank Code |
| **Balance View** | By AssetType (only fiat: BRL, USD) |
| **Typical Wallets** | FiatAssets only |

**Business Rules:**
- **Positive balance** = Money the company HAS in that bank
- **Negative balance** = Company owes the bank (overdraft, rare)
- **Cannot hold non-fiat assets** ✓

**Transaction Restrictions:**
- Only participates in RECEIPT/PAYMENT transactions
- TRANSFER mode blocks Bank participation (enforced in backend)

---

### 3. Member (Team Member) ✅ VALIDATED

| Aspect | Validated Definition |
|--------|---------------------|
| **Role** | Internal team member, part of company operations |
| **Attributes** | Name, Tax info, **Share (%)**, **Salary**, Birthday |
| **Balance View** | By AssetType (like Client) |
| **Typical Wallets** | FiatAssets, PokerAssets |

**Business Rules:**
- Transacts **exactly like Clients** ✓
- Has `Share %` for profit distribution - **TBD (Financial Module)**
- Has `Salary` - **TBD (Financial Module)**

**Deferred Work:**
- Share % impact on balance/profit calculation
- Salary processing integration
- Detailed documentation when financial module is built

---

### 4. PokerManager (Company's Poker Asset Holder) ✅ VALIDATED

| Aspect | Validated Definition |
|--------|---------------------|
| **Role** | Holds poker chips **ON BEHALF OF the company** |
| **Attributes** | Name, Tax info, ManagerProfitType (Spread or RakeOverrideCommission) |
| **Balance View** | **By AssetGroup** (FiatAssets, PokerAssets, CryptoAssets, Settlements) |
| **Typical Wallets** | PokerAssets, FiatAssets, Internal (for self-conversion) |

**Business Rules:**
- **PokerAssets balance** = Chips held FOR the company ✓
- **Negative PokerAssets** = Company borrowed assets from PM (like a bank, rare)
- **AccountClassification.ASSET** for normal positive balances
- In **FiatTransactions**: Acts like a Client
- In **Conversion Wallet transactions**: Acts like a Client

**Company Profit Sources (via PokerManager):**
- **Spread**: Difference in buy/sell ConversionRate
- **Rate**: Fee percentage on transactions
- **RakeCommission**: % of rake the poker site pays to company

**Self-Conversion Case:** ✅ Correctly described
- Internal wallet → PokerAssets triggers dual-balance impact
- PokerAssets: +chips (holding for company)
- FiatAssets: +BRL (company owes manager)

---

## Asset Types and Groups

### AssetGroup (Categories)

| Group | Value | Purpose | Used By |
|-------|-------|---------|---------|
| FiatAssets | 1 | Real money (BRL, USD) | All entities |
| PokerAssets | 2 | Poker platform credits | All entities |
| CryptoAssets | 3 | Cryptocurrency | All entities |
| Internal | 4 | Flexible wallets (System, Conversion) | Company, PokerManager |
| Settlements | 5 | Settlement records | PokerManager-Client |

### AssetType (Specific Assets)

| Category | Assets |
|----------|--------|
| **Fiat** | BrazilianReal (21), USDollar (22) |
| **Poker** | PokerStars (101), GgPoker (102), YaPoker (103), etc. |
| **Crypto** | Bitcoin (201), Ethereum (202), etc. |

---

## Transactions: What They Mean

### Transaction Type by Asset

| Transaction Type | Asset Category | Purpose |
|------------------|----------------|---------|
| FiatAssetTransaction | Fiat (BRL, USD) | Bank transfers, PIX, cash |
| DigitalAssetTransaction | Poker + Crypto | Chip transfers, crypto movements |
| SettlementTransaction | Settlement | Periodic reconciliation with rake |

### Transaction Modes (Business Flows)

| Mode | Description | Participants | Transaction Type |
|------|-------------|--------------|------------------|
| **SALE** | Client/Member **buys** chips via PokerManager | PM → Client/Member | DigitalAssetTransaction |
| **PURCHASE** | Client/Member **sells** chips via PokerManager | Client/Member → PM | DigitalAssetTransaction |
| **RECEIPT** | Client/Member pays fiat to company via Bank | Client/Member → Bank | FiatAssetTransaction |
| **PAYMENT** | Company pays fiat to Client/Member via Bank | Bank → Client/Member | FiatAssetTransaction |
| **TRANSFER** | Direct transfer between different entities | Any → Any (no Bank) | Either |
| **SELF_TRANSFER** | Movement between same entity's wallets | Self → Self | Either |
| **SYSTEM_OPERATION** | Transaction with System Wallet (financial categorization) | Any ↔ System Wallet | Either |
| **CONVERSION** | PokerManager self-conversion (dual-balance) | Internal ↔ PokerAssets | DigitalAssetTransaction |

> **Note:** The mode previously called "INTERNAL" is renamed to "SELF_TRANSFER" for clarity.
> "Internal" in this context refers to same-holder transfers, not `AssetGroup.Internal` wallets.

### What Each Transaction Really Means

#### SALE (Client BUYS chips via PokerManager) - WITH BalanceAs

```
Business: Client is BUYING chips from the company via PokerManager

Transaction:
├─ Sender: PokerManager's PokerStars wallet
├─ Receiver: Client's PokerStars wallet
├─ Amount: 1000 chips
├─ BalanceAs: BRL
└─ ConversionRate: 5.0

Balance Impact:
├─ PokerManager PokerAssets: -1000 (company's inventory decreased)
├─ Client PokerAssets: NO CHANGE (chips not in their balance!)
└─ Client FiatAssets (BRL): -5000 (owes company 5000 BRL)

Business Meaning:
- Company gave up 1000 chips (via PM)
- Client OWES company 5000 BRL
- The chips are not tracked in client's balance when BalanceAs is set
```

#### SALE (Client BUYS chips via PokerManager) - WITHOUT BalanceAs (Coin Balance)

```
Business: Client is BUYING chips on credit (in the same asset)

Transaction:
├─ Sender: PokerManager's PokerStars wallet
├─ Receiver: Client's PokerStars wallet
├─ Amount: 1000 chips
├─ BalanceAs: null
├─ Rate: 5%
└─ ConversionRate: null

Balance Impact:
├─ PokerManager PokerAssets: -1000 (company's inventory decreased)
└─ Client PokerAssets: -1050 (owes company 1050 chips)

Business Meaning:
- Company gave up 1000 chips (via PM)
- Client owes 1050 chips (1000 + 5% fee) in the SAME asset
- This is the "Coin Balance" scenario
```

#### PURCHASE (Client SELLS chips via PokerManager) - WITH BalanceAs

```
Business: Client is SELLING chips to the company via PokerManager

Transaction:
├─ Sender: Client's PokerStars wallet
├─ Receiver: PokerManager's PokerStars wallet
├─ Amount: 1000 chips
├─ BalanceAs: BRL
└─ ConversionRate: 5.0

Balance Impact:
├─ Client PokerAssets: NO CHANGE (chips not tracked in their balance)
├─ Client FiatAssets (BRL): +5000 (company owes client 5000 BRL)
└─ PokerManager PokerAssets: +1000 (company's inventory increased)

Business Meaning:
- Client gave up 1000 chips (tracked elsewhere, e.g., poker platform)
- Company OWES client 5000 BRL
- PokerManager is now holding 1000 more chips for the company
```

#### RECEIPT (Client PAYS to Bank)

```
Business: Client pays money they owe to the company

Transaction:
├─ Sender: Client's BRL wallet
├─ Receiver: Bank's BRL wallet
├─ Amount: 5000 BRL

Balance Impact:
├─ Client FiatAssets: +5000 (debt reduced / moves toward positive)
└─ Bank FiatAssets: +5000 (company has more money)

Business Meaning:
- If client had -5000, now has 0 (debt cleared)
- Company's bank balance increased by 5000
- BOTH balances go UP in this transaction
```

#### PAYMENT (Bank PAYS to Client)

```
Business: Company pays money owed to the Client

Transaction:
├─ Sender: Bank's BRL wallet
├─ Receiver: Client's BRL wallet
├─ Amount: 5000 BRL

Balance Impact:
├─ Bank FiatAssets: -5000 (company has less money)
└─ Client FiatAssets: -5000 (credit reduced / moves toward negative)

Business Meaning:
- If client had +5000 credit, now has 0 (settled)
- Company's bank balance decreased by 5000
- BOTH balances go DOWN in this transaction
```

---

## Balance Calculation: How It Works

### For Clients, Banks, Members: By AssetType (`GetBalancesByAssetType`)

```
Balance = InitialBalance 
        + FiatTransactions impact
        + DigitalTransactions impact
        + SettlementTransactions impact (⚠️ NEEDS FIX)
```

**FiatTransactions:**
- Standard sender/receiver sign convention
- AccountClassification adjustment when classifications differ

**DigitalTransactions:**
- **With BalanceAs**: `signedAmount * ConversionRate` applied to BalanceAs asset
- **Without BalanceAs**: `signedAmount / ((100 + Rate) / 100)` applied to transaction's AssetType

**SettlementTransactions:** ⚠️ **CRITICAL BUG - See Critical Bugs Section**

### For PokerManager: By AssetGroup (`GetBalancesByAssetGroup`)

```
Balance = InitialBalance 
        + FiatTransactions impact (grouped by AssetGroup)
        + DigitalTransactions impact (with self-conversion logic)
        + SettlementTransactions impact (⚠️ NEEDS FIX)
```

**Key Features:**
- Groups all poker platforms (PokerStars, GgPoker, etc.) into single "PokerAssets" balance
- Groups fiat currencies into single "FiatAssets" balance
- **Internal wallets** are remapped to FiatAssets (if BRL) or PokerAssets (otherwise)
- **Self-Conversion** triggers dual-balance impact (Internal ↔ PokerAssets with BalanceAs)

### The AccountClassification Factor

| Classification | Used By | Sign Convention |
|----------------|---------|-----------------|
| **ASSET** | Banks, PokerManagers (for company assets) | Positive = company owns |
| **LIABILITY** | Clients, Members (typically) | Positive = company owes them |

**When sender and receiver have DIFFERENT classifications:**
- The LIABILITY wallet gets its sign **inverted**
- This ensures both sides see consistent balance changes

---

## Business Flow Examples

### Example 1: Complete Client Lifecycle (Corrected)

```
Day 1: Client João buys 1000 chips (with BalanceAs)
────────────────────────────────────────────────────
Transaction: SALE (PM → João), BalanceAs=BRL, ConversionRate=5.0
- PM PokerAssets: -1000 (company inventory decreased)
- João PokerAssets: NO CHANGE (chips not tracked when BalanceAs is set)
- João FiatAssets (BRL): -5000 (owes company)

Day 2: João pays via PIX
────────────────────────
Transaction: RECEIPT (João → Bank), Amount=5000 BRL
- João FiatAssets: +5000 (debt reduced, -5000 + 5000 = 0)
- Bank FiatAssets: +5000 (company has more money)

Day 3: João wins and sells 2000 chips (with BalanceAs)
──────────────────────────────────────────────────────
Transaction: PURCHASE (João → PM), BalanceAs=BRL, ConversionRate=5.0
- João PokerAssets: NO CHANGE
- João FiatAssets: +10000 (company owes 10000)
- PM PokerAssets: +2000 (company inventory increased)

Day 4: Company pays João
───────────────────────
Transaction: PAYMENT (Bank → João), Amount=10000 BRL
- Bank FiatAssets: -10000 (company has less money)
- João FiatAssets: -10000 (credit reduced, +10000 - 10000 = 0)

Final Balances:
- João: PokerAssets = 0, FiatAssets = 0 (fully settled)
- Bank: FiatAssets = -5000 (net outflow to client)
- PM: PokerAssets = +1000 (net chips acquired for company)
```

### Example 2: Client with Coin Balance (No BalanceAs)

```
Day 1: Client João buys 1000 chips WITHOUT BalanceAs (Rate=5%)
──────────────────────────────────────────────────────────────
Transaction: SALE (PM → João), BalanceAs=null, Rate=5%
- PM PokerAssets: -1000 (company inventory decreased)
- João PokerAssets: -1050 (owes company 1050 chips)

Day 2: João sells 1050 chips back WITHOUT BalanceAs
───────────────────────────────────────────────────
Transaction: PURCHASE (João → PM), BalanceAs=null
- João PokerAssets: +1050 (debt cleared, -1050 + 1050 = 0)
- PM PokerAssets: +1050 (company inventory increased)

Final Balances:
- João: PokerAssets = 0 (debt cleared)
- PM: PokerAssets = +50 (company gained 50 chips via rate)
```

### Example 3: PokerManager Self-Conversion

```
Manager Maria deposits her own 1000 chips at rate 5.0:
──────────────────────────────────────────────────────
Transaction: Internal → PokerAssets (CONVERSION mode)
- Sender: Maria's Internal wallet (PokerStars)
- Receiver: Maria's PokerAssets wallet (PokerStars)
- Amount: 1000
- BalanceAs: BRL
- ConversionRate: 5.0

Balance Impact (DUAL):
- Maria PokerAssets: +1000 (holding for company)
- Maria FiatAssets: +5000 (company owes Maria)

Settlement (later):
- Company pays Maria 5000 BRL via FiatTransaction
- Maria FiatAssets: 0 (settled)
```

---

## Critical Bugs and Improvements

### 🔴 CRITICAL: SettlementTransaction Balance Calculation is WRONG

**Location:** `BaseAssetHolderService.cs` lines 518-536 (GetBalancesByAssetType) and 679-697 (GetBalancesByAssetGroup)

**Current (WRONG) Implementation:**
```csharp
foreach (var tx in settlementTransactions)
{
    var signedAmount = tx.GetSignedAmountForWalletIdentifier(relevantWalletId);
    // ... uses signedAmount (from AssetAmount) for balance
    balances[assetType] += signedAmount;
}
```

**The Problem:**
- Uses `AssetAmount` with sender/receiver sign convention
- `AssetAmount` represents chips that ALREADY flowed via `DigitalAssetTransactions`
- This **DUPLICATES** the chip impact in balance calculation

**The Correct Logic:**

SettlementTransaction fields that impact balance:
- `RakeAmount`: Rake the client pays to the poker site
- `RakeCommission`: % the poker site pays to company (via PokerManager)
- `RakeBack`: % of rake the company returns to client

**Correct Balance Impacts (recorded in BRL - AssetType 21 / FiatAssets):**

| Entity | Impact Formula | Meaning |
|--------|----------------|---------|
| **Client** | `+RakeAmount * (RakeBack / 100)` | Client gets rakeback (BRL balance) |
| **PokerManager** | `-RakeAmount * (RakeCommission / 100)` | Company earns commission (BRL impact on PM) |

**The `AssetAmount` should NOT be used** - it's only for tracking/control purposes.

**Fix Required:**
```csharp
// For Client (GetBalancesByAssetType) - BRL impact
foreach (var tx in settlementTransactions)
{
    if (tx.RakeBack.HasValue && tx.RakeBack.Value > 0)
    {
        var rakebackAmount = tx.RakeAmount * (tx.RakeBack.Value / 100);
        // Add to client's balance (they receive rakeback)
        balances[AssetType.BrazilianReal] += rakebackAmount;
    }
}

// For PokerManager (GetBalancesByAssetGroup) - BRL impact
foreach (var tx in settlementTransactions)
{
    var commissionAmount = tx.RakeAmount * (tx.RakeCommission / 100);
    // Company earns commission (represents outflow from PM perspective)
    if (!balances.ContainsKey(AssetGroup.FiatAssets)) balances[AssetGroup.FiatAssets] = 0;
    balances[AssetGroup.FiatAssets] -= commissionAmount;
}
```

**Implementation Decision: How to Determine Entity Type**

| Option | Approach | Tradeoff |
|--------|----------|----------|
| **A** | Query DB to check if `baseAssetHolderId` is a PokerManager | Explicit but extra DB query per balance call |
| **B** | Infer from wallet's `AssetGroup` (PokerAssets → PM logic) | No extra query but indirect, may not be reliable |
| **C** | Use method context - `GetBalancesByAssetType` = Client/Member, `GetBalancesByAssetGroup` = PokerManager | Simple, already separated, but implicit |

**Recommendation:** Option C is preferred since the calling method already determines the calculation type. This is documented here for review when creating the implementation plan.

---

### 🟡 IMPROVEMENT: Transaction Mode Naming Clarity

**Issue:** The frontend mode "INTERNAL" conflicts with `AssetGroup.Internal` naming.

**Recommendation:** 
- Rename frontend mode `INTERNAL` → `SELF_TRANSFER`
- Add new modes: `SYSTEM_OPERATION`, `CONVERSION`
- This aligns with the work done in `ASSETGROUP_FLEXIBLE_RENAME_PLAN.md`

---

### 🟡 IMPROVEMENT: Credit Limit Management for Clients

**Issue:** Clients can have negative balances (credit lines) but there's no system to manage credit limits.

**Recommendation:**
- Add `CreditLimit` field to Client entity
- Add validation in TransferService to check credit limit before allowing negative balance
- This can be personalized per client (like a credit analysis)

---

### 🔴 CRITICAL: AccountClassification Bug for PokerManager FiatAssets

**Location:** `TransferService.cs` lines 345-350

**Current (WRONG) Implementation:**
```csharp
private async Task<AccountClassification> DetermineAccountClassificationAsync(Guid assetHolderId)
{
    var isBank = await _context.Banks.AnyAsync(b => b.BaseAssetHolderId == assetHolderId);
    var isPokerManager = await _context.PokerManagers.AnyAsync(pm => pm.BaseAssetHolderId == assetHolderId);
    
    return (isBank || isPokerManager) ? AccountClassification.ASSET : AccountClassification.LIABILITY;
}
```

**The Problem:**
- ALL PokerManager wallets are classified as ASSET
- But PM's FiatAssets should be LIABILITY (company OWES PM when positive)
- This affects sign calculations in balance

**Correct Classification by AssetGroup:**

| Entity | AssetGroup | Should Be |
|--------|------------|-----------|
| PokerManager | PokerAssets | ASSET ✅ |
| PokerManager | FiatAssets | **LIABILITY** ❌ Bug |
| PokerManager | Internal | ASSET ✅ |

**See:** [ACCOUNTCLASSIFICATION_BUG_FIX_PLAN.md](./ACCOUNTCLASSIFICATION_BUG_FIX_PLAN.md) for full fix plan.

---

## Deferred Work

### 0. Finance Module - Company Profit from SettlementTransactions (Document for Later)

When implementing the Finance Module, the company profit from settlements should be calculated as:

```
Company Profit = RakeAmount * ((RakeCommission - RakeBack) / 100)
```

**Example:**
- RakeAmount: 1000 chips (what client paid to poker site)
- RakeCommission: 50% (what poker site pays company)
- RakeBack: 10% (what company returns to client)
- **Company Profit**: 1000 * ((50 - 10) / 100) = **400 chips**

This calculation is separate from balance and will be part of the financial reporting module.

---

### 1. Referral System Integration with Balance (Document for Later)

Referrals should impact balance calculation:
- When a client has an active referral, part of their rake/transactions may generate commission for the referrer
- This is NOT currently implemented in balance calculation
- See `REFERRAL_SYSTEM.md` for current implementation

**Future Work:**
- Determine how referral commissions impact balance
- Add referral commission calculation to balance methods
- Document business rules for referral-based balance impact

### 2. Member Financial Module (Document for Later)

Members have `Share %` and `Salary` but these are not yet integrated:

**Share %:**
- Represents profit distribution rights
- Need to define: Share of what? (company profit, rake, etc.)
- Need to define: When is it calculated? (monthly, per transaction, etc.)

**Salary:**
- Need to define: How is it tracked/paid?
- Need to define: Integration with balance module

### 3. ManagerProfitType Implementation (Needs Refactoring)

Two profit models exist but need proper implementation:
- **Spread**: Company earns from difference in buy/sell ConversionRate
- **RakeOverrideCommission**: Company earns share of rake (via SettlementTransaction)

Current code exists but needs refactoring for clarity and correctness.

### 4. Settlement Process for Zeroing Positions (Consider)

Currently not implemented but may be useful:
- Periodic process to zero out client positions
- Would create settlement transactions to reconcile
- Business decision needed on whether to implement

---

## Documentation Updates Needed

Based on this analysis, the following documentation should be updated:

| Document | Update Needed |
|----------|---------------|
| `BALANCE_ENDPOINTS.md` | Add SettlementTransaction correct formula |
| `TRANSACTION_INFRASTRUCTURE.md` | Clarify SALE/PURCHASE balance impacts with BalanceAs |
| `SETTLEMENT_WORKFLOW.md` | Document correct balance impact of RakeAmount, RakeCommission, RakeBack |
| `ENTITY_INFRASTRUCTURE.md` | Clarify Bank is company's own account |
| `BUSINESS_DOMAIN_OVERVIEW.md` | Clarify who earns from spread/rake (company, not PM) |

---

## Implementation Priority

### Immediate (Balance Bug Fix)
1. **Fix SettlementTransaction balance calculation** in `BaseAssetHolderService.cs`
2. **Add unit tests** to verify correct balance calculation

### Short-term (Documentation)
3. Update documentation files listed above
4. Update transaction mode naming for clarity

### Medium-term (Features)
5. Client credit limit management
6. Member financial module design

### Long-term (Deferred)
7. Referral system balance integration
8. ManagerProfitType refactoring
9. Settlement zeroing process (if decided)

---

*Created: January 24, 2026*
*Updated: January 24, 2026 (post-validation)*
*Purpose: Guide for balance system improvements*
