# Entity Business Behavior

> **Status:** Validated  
> **Created:** January 24, 2026  
> **Purpose:** Define business behavior, balance meanings, and transaction rules for each entity type

---

## Table of Contents

- [Overview](#overview)
- [Entity Categories](#entity-categories)
- [Company Asset Holders](#company-asset-holders)
  - [Bank](#bank)
  - [PokerManager](#pokermanager)
  - [CryptoManager (Future)](#cryptomanager-future)
- [Business Partners](#business-partners)
  - [Client](#client)
  - [Member](#member)
- [Balance Semantics](#balance-semantics)
- [Transaction Participation Rules](#transaction-participation-rules)
- [Special Behaviors](#special-behaviors)
- [Related Documentation](#related-documentation)

---

## Overview

SF Management has two categories of entities based on their relationship with the company:

1. **Company Asset Holders** - Hold assets **on behalf of** the company
2. **Business Partners** - Do business **with** the company

This distinction drives balance meaning, accounting classification, and transaction rules.

---

## Entity Categories

```
┌─────────────────────────────────────────────────────────────────┐
│                    COMPANY ASSET HOLDERS                        │
│            (Hold assets ON BEHALF OF the company)               │
│                                                                 │
│   ┌─────────────┐  ┌─────────────────┐  ┌─────────────────┐    │
│   │    Bank     │  │  PokerManager   │  │ CryptoManager   │    │
│   │ (FiatAssets)│  │ (PokerAssets)   │  │ (CryptoAssets)  │    │
│   │   [ASSET]   │  │    [ASSET]      │  │   [FUTURE]      │    │
│   └─────────────┘  └─────────────────┘  └─────────────────┘    │
│                                                                 │
│   Positive Balance = Company HAS these assets                   │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                     BUSINESS PARTNERS                           │
│              (Do business WITH the company)                     │
│                                                                 │
│   ┌─────────────────────────┐  ┌─────────────────────────┐     │
│   │         Client          │  │         Member          │     │
│   │   (External customer)   │  │   (Internal team)       │     │
│   │      [LIABILITY]        │  │ [LIABILITY] + Share%    │     │
│   └─────────────────────────┘  └─────────────────────────┘     │
│                                                                 │
│   Positive Balance = Company OWES them                          │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                    SYSTEM WALLETS (Company-Owned)               │
│              (Internal Accounting Operations)                    │
│                                                                 │
│   ┌─────────────────────────────────────────────────────────┐  │
│   │  Flexible Wallet with BaseAssetHolderId = NULL          │  │
│   │  AccountClassification: LIABILITY                        │  │
│   │  Purpose: Expenses and Income tracking                   │  │
│   └─────────────────────────────────────────────────────────┘  │
│                                                                 │
│   Used in transactions with Banks/Managers for:                 │
│   - Receita (income): System wallet is SENDER                   │
│   - Despesa (expense): System wallet is RECEIVER                │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                    SPECIAL CASE: PokerManager FiatAssets        │
│                                                                 │
│   When PokerManager has FiatAssets wallet (from self-conversion │
│   or FiatTransactions), it behaves like a Business Partner:     │
│                                                                 │
│   ┌─────────────────────────────────────────────────────┐      │
│   │  PokerManager FiatAssets Wallet                     │      │
│   │  AccountClassification: LIABILITY                   │      │
│   │  Positive Balance = Company OWES the PM             │      │
│   └─────────────────────────────────────────────────────┘      │
└─────────────────────────────────────────────────────────────────┘
```

> **Note:** AccountClassification depends on BOTH entity type AND wallet's AssetGroup. See [ACCOUNTCLASSIFICATION_BUG_FIX_PLAN.md](../10_REFACTORING/ACCOUNTCLASSIFICATION_BUG_FIX_PLAN.md) for details.

---

## Company Asset Holders

These entities hold assets **for the company**. A positive balance means the company **has** those assets.

### Bank

**Purpose:** Holds the company's fiat currency (BRL, USD)

| Attribute | Value |
|-----------|-------|
| AssetGroups | FiatAssets only |
| AccountClassification | Asset |
| Positive Balance | Company HAS money in this bank |
| Negative Balance | Company borrowed from this bank (unlikely) |

**Transaction Restrictions:**

| Mode | Allowed | Notes |
|------|---------|-------|
| RECEIPT | ✅ As receiver | Entity → Bank |
| PAYMENT | ✅ As sender | Bank → Entity |
| TRANSFER | ❌ Never | Cannot participate |
| SELF_TRANSFER | ✅ | Between own wallets |
| SALE/PURCHASE | ❌ Never | Not applicable |

**Key Rules:**
- Cannot hold PokerAssets or CryptoAssets
- Cannot participate in TRANSFER mode (backend enforced)
- Only participates via RECEIPT/PAYMENT modes

---

### PokerManager

**Purpose:** Holds poker chips (PokerAssets) **on behalf of the company**

| Attribute | Value |
|-----------|-------|
| Primary AssetGroup | PokerAssets |
| Secondary AssetGroup | FiatAssets (when participating in FiatTransactions) |
| AccountClassification | Asset (for PokerAssets), Liability (for FiatAssets) |
| Positive PokerAssets | Company HAS chips held by this manager |
| Negative PokerAssets | Company borrowed chips (unlikely) |

**Transaction Participation:**

| Mode | Role | Balance Impact |
|------|------|----------------|
| SALE | Sender | PokerAssets decreases (company sells chips) |
| PURCHASE | Receiver | PokerAssets increases (company buys chips) |
| RECEIPT | As participant | FiatAssets (acts like Client) |
| PAYMENT | As participant | FiatAssets (acts like Client) |
| CONVERSION | Self-conversion | Dual impact (see below) |

**Revenue Sources (Company Earns):**

| Type | Description | How |
|------|-------------|-----|
| Spread | Price difference on buy/sell | Different ConversionRate |
| RakeCommission | Commission from poker site | SettlementTransaction |

> **Important:** The company earns revenue, not the manager personally.

**Special: FiatAssets Behavior**

When a PokerManager participates in FiatTransactions, they act **like a Client**:
- Positive FiatAssets = Company OWES the manager
- This happens when the manager has personal funds involved

### Manager Profit Type

The `ManagerProfitType` property determines the manager's revenue model and affects multiple system behaviors:

| Aspect | Spread | RakeOverrideCommission |
|--------|--------|----------------------|
| **Revenue Source** | Buy/sell rate difference | Percentage of poker room rake |
| **AvgRate (Cotação)** | Weighted average from purchase history | Fixed at 1 (chips = BRL) |
| **Balance Calculation** | Standard (PokerAssets + FiatAssets) | Includes SettlementTransaction.AssetAmount in both PokerAssets and FiatAssets |
| **Profit Formula** | SaleAmount × (SaleRate - AvgRate) | RakeAmount × ((Commission% - RakeBack%) / 100) × AvgRate |
| **Settlement Impact** | Standard rake/commission split | AssetAmount added to chip and fiat balances |

This property is nullable — a manager without a configured profit type will not generate revenue calculations.

---

### CryptoManager (Future)

**Purpose:** Will hold cryptocurrency **on behalf of the company**

| Attribute | Value |
|-----------|-------|
| AssetGroups | CryptoAssets |
| AccountClassification | Asset |
| Positive Balance | Company HAS crypto held by this manager |

> **Note:** Not yet implemented. Will follow PokerManager pattern.

---

## Business Partners

These entities do business **with the company**. A positive balance means the company **owes** them.

### Client

**Purpose:** External customer who buys/sells poker chips via PokerManagers

| Attribute | Value |
|-----------|-------|
| AssetGroups | FiatAssets, PokerAssets, CryptoAssets |
| AccountClassification | Liability |
| Positive Balance | Company OWES the client |
| Negative Balance | Client OWES the company (credit) |

**Transaction Participation:**

| Mode | Flow | Balance Impact |
|------|------|----------------|
| SALE (buys chips) | PM → Client | FiatAssets decreases (owes company) |
| PURCHASE (sells chips) | Client → PM | FiatAssets increases (company owes) |
| RECEIPT | Client → Bank | FiatAssets increases (pays debt) |
| PAYMENT | Bank → Client | FiatAssets decreases (company pays) |
| TRANSFER | Any → Any | Direct P2P (Flexible wallets only) |

**Balance Scenarios:**

```
Scenario 1: Client buys 1000 chips at 5.0 BRL (with BalanceAs)
├─ Client PokerAssets: NO CHANGE (chips not in balance)
└─ Client FiatAssets (BRL): -5000 (owes company)

Scenario 2: Client buys 1000 chips (Coin Balance, no BalanceAs)
├─ Client PokerAssets: -1050 (owes chips + 5% rate)
└─ Client FiatAssets: NO CHANGE

Scenario 3: Client pays 5000 BRL (RECEIPT)
├─ Client FiatAssets: +5000 (debt reduced)
└─ Bank FiatAssets: +5000 (company has money)
```

**Credit Line (Future):**
- Clients can have negative balances (owe company)
- Credit limit per client will be configurable (TBD)

---

### Member

**Purpose:** Internal team member who transacts like a Client + has profit sharing

| Attribute | Value |
|-----------|-------|
| AssetGroups | FiatAssets, PokerAssets, CryptoAssets |
| AccountClassification | Liability |
| Positive Balance | Company OWES the member |
| Negative Balance | Member OWES the company |

**Transaction Behavior:** Identical to Client

**Additional Properties:**

| Property | Purpose | Implementation |
|----------|---------|----------------|
| `Share` | % of company profits | Finance Module (TBD) |
| `Salary` | Fixed periodic payment | Finance Module (TBD) |

> **Note:** Share and Salary implementation is deferred to Finance Module.

---

## Balance Semantics

### Quick Reference

| Entity | AssetGroup | Positive = | Negative = |
|--------|------------|------------|------------|
| Bank | FiatAssets | Company HAS | Company borrowed |
| PokerManager | PokerAssets | Company HAS | Company borrowed |
| PokerManager | FiatAssets | Company OWES PM | PM OWES company |
| Client | Any | Company OWES | Client OWES (credit) |
| Member | Any | Company OWES | Member OWES (credit) |
| System Wallet | Flexible | N/A (internal) | N/A (internal) |

### System Wallet Classification

System wallets are company-owned Flexible wallets used for internal accounting. They interact with entity wallets (Bank, PokerManager) to record company expenses and income.

**Key Properties:**
- `AssetPool.BaseAssetHolderId = null` (no owner entity)
- `AccountClassification = Liability`
- Statement counterparty displays as "Unknown" (use category name instead)

**Transaction Direction Rules:**

| Transaction Type | Entity Type | System Position | Entity Balance |
|------------------|-------------|-----------------|----------------|
| Compra (buy chips) | PokerManager | Sender | +Amount (income) |
| Venda (sell chips) | PokerManager | Receiver | -Amount (expense) |
| Recebimento (receive money) | Bank | Sender | +Amount (income) |
| Pagamento (pay money) | Bank | Receiver | -Amount (expense) |

**Why System Wallets are Liability:**

The Liability classification ensures correct balance signs when transacting with Asset entities:
- When system wallet is **sender** (income): Entity receives → Entity balance UP
- When system wallet is **receiver** (expense): Entity sends → Entity balance DOWN

This follows double-entry accounting: transactions between Asset and Liability accounts trigger sign inversion for the Liability side.

### AccountClassification Effect on Transactions

When sender and receiver have **same** classification:
- Standard sign convention: Sender (-), Receiver (+)

When sender and receiver have **different** classification:
- LIABILITY wallet gets sign inverted
- This normalizes the accounting

---

## Transaction Participation Rules

### By Transaction Mode

| Mode | Bank | PokerManager | Client | Member |
|------|------|--------------|--------|--------|
| SALE | ❌ | ✅ Sender | ✅ Receiver | ✅ Receiver |
| PURCHASE | ❌ | ✅ Receiver | ✅ Sender | ✅ Sender |
| RECEIPT | ✅ Receiver | ✅ * | ✅ Sender | ✅ Sender |
| PAYMENT | ✅ Sender | ✅ * | ✅ Receiver | ✅ Receiver |
| TRANSFER ** | ❌ | ✅ | ✅ | ✅ |
| SELF_TRANSFER | ✅ | ✅ | ✅ | ✅ |
| CONVERSION | ❌ | ✅ | ❌ | ❌ |

\* PokerManager acts like Client in FiatTransactions

\*\* **TRANSFER mode guardrail (application level):** UI limits TRANSFER mode to Flexible wallets (AssetGroup 4). Backend enforces bank restrictions and wallet validity.
- FiatAssets → RECEIPT/PAYMENT with Bank
- PokerAssets → SALE/PURCHASE with PokerManager

### Validation Rules

1. **Bank in TRANSFER:** Backend throws `BANK_NOT_ALLOWED_IN_TRANSFER`
2. **Wallet Existence:** Backend throws `WALLETS_REQUIRED` if missing
3. **Same Wallet:** Backend throws `SAME_SENDER_RECEIVER_WALLET`

---

## Special Behaviors

### 1. PokerManager Self-Conversion

When a PokerManager moves chips between Flexible and PokerAssets wallets with `BalanceAs` set:

```
Flexible → PokerAssets (manager deposits own chips)
├─ PokerAssets: +1000 (company now holds)
└─ FiatAssets: +5000 (company owes manager 5000 BRL)
```

This represents the manager putting personal chips into the managed system.

**Trigger Conditions:**
1. Both wallets belong to same PokerManager
2. One is `AssetGroup.Flexible`, other is `AssetGroup.PokerAssets`
3. `BalanceAs` is set
4. `ConversionRate` is set

### 2. Coin Balance (No BalanceAs)

When a DigitalTransaction has no `BalanceAs`:
- Debt is tracked in the **same** asset type
- `Rate` field applies as fee percentage
- Client's balance in transaction asset is impacted

### 3. Settlement Rake Distribution

SettlementTransactions don't use `AssetAmount` for balance. Impacts are recorded in BRL (AssetType 21 / FiatAssets):

| Entity | Balance Impact |
|--------|----------------|
| Client | `+RakeAmount × (RakeBack / 100)` |
| PokerManager | `-RakeAmount × (RakeCommission / 100)` |

---

## Related Documentation

| Topic | Document |
|-------|----------|
| Entity Models | [ENTITY_INFRASTRUCTURE.md](./ENTITY_INFRASTRUCTURE.md) |
| Balance Calculation | [TRANSACTION_BALANCE_IMPACT.md](./TRANSACTION_BALANCE_IMPACT.md) |
| Transaction System | [TRANSACTION_INFRASTRUCTURE.md](./TRANSACTION_INFRASTRUCTURE.md) |
| Settlement Process | [SETTLEMENT_WORKFLOW.md](./SETTLEMENT_WORKFLOW.md) |
| Enum Definitions | [ENUMS_AND_TYPE_SYSTEM.md](../07_REFERENCE/ENUMS_AND_TYPE_SYSTEM.md) |
| Finance Planning | [FINANCE_MODULE_PLANNING.md](../10_REFACTORING/FINANCE_MODULE_PLANNING.md) |

---

*Last updated: February 23, 2026*
