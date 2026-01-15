# Business Domain Overview

## Overview

SF Management is a financial management system designed for the poker industry, specifically for managing relationships between poker managers, players (clients), banks, and business members. The system tracks financial transactions across multiple asset types including fiat currencies, poker platform credits, and cryptocurrencies.

---

## Business Context

### The Poker Management Business

Poker managers operate as intermediaries between poker platforms and players. They:
1. **Provide liquidity** - Buying and selling poker chips for players
2. **Manage settlements** - Reconciling player positions periodically
3. **Track commissions** - Earning rake and referral commissions
4. **Handle banking** - Moving money between banks and players

### Key Business Processes

```
Player wants chips → Contact Manager → Manager sells chips → Player pays
Player wins chips → Contact Manager → Player sells chips → Manager pays
                           ↓
                    Settlement Process
                           ↓
                 Calculate net position
                           ↓
              Record rake/commission
```

---

## Domain Entities

### Asset Holders

Entities that can hold assets and transact:

| Entity | Description | Key Use |
|--------|-------------|---------|
| **Client** | Poker players/customers | Buy/sell chips |
| **Bank** | Financial institutions | Hold fiat currency |
| **Member** | Business partners | Revenue sharing |
| **PokerManager** | Operations managers | Manage all transactions |

### Assets

Types of value tracked in the system:

| Asset Group | Examples | Use |
|-------------|----------|-----|
| **FiatAssets** | BRL, USD | Real money |
| **PokerAssets** | PokerStars, GGPoker | Poker chips |
| **CryptoAssets** | Bitcoin, Ethereum | Digital currency |
| **Internal** | Internal entries | Accounting |
| **Settlements** | Settlement records | Period close |

---

## Key Business Rules

### 1. Double-Entry Transactions

Every transaction has a sender and receiver:
- **Sender**: Asset goes out
- **Receiver**: Asset comes in
- Amount balances to zero across the system

### 2. Account Classification

Wallets are classified for accounting:
- **ASSET**: Company owns (increases with credits)
- **LIABILITY**: Company owes (increases with debits)

### 3. Referral Commissions

Referrers earn commission on player wallets:
- One active referral per wallet
- Commission percentage (0-100%)
- Time-bounded relationships

### 4. Settlement Process

Periodic reconciliation:
1. Calculate net position between manager and player
2. Record any rake earned
3. Create settlement transactions
4. Zero out running positions

---

## Financial Flows

### Buying Chips (Player to Manager)

```
Player pays BRL → Bank receives BRL
Manager sends chips → Player receives chips
                          ↓
              DigitalAssetTransaction created
```

### Selling Chips (Player to Manager)

```
Player sends chips → Manager receives chips
Bank sends BRL → Player receives BRL
                          ↓
              FiatAssetTransaction created
```

### Settlement

```
Calculate: Player owes 10,000 chips
Manager has 10,000 chip liability
                    ↓
         Settlement Transaction
                    ↓
         Positions zeroed
         Rake recorded
```

---

## System Benefits

1. **Complete Transaction History** - Every movement tracked
2. **Multi-Asset Support** - Fiat, poker, crypto in one system
3. **Automated Balance Calculation** - Real-time position tracking
4. **Commission Management** - Referral and rake tracking
5. **Import & Reconciliation** - External file integration

---

## Related Documentation

- [ASSET_INFRASTRUCTURE.md](../03_CORE_SYSTEMS/ASSET_INFRASTRUCTURE.md) - Technical asset details
- [TRANSACTION_INFRASTRUCTURE.md](../03_CORE_SYSTEMS/TRANSACTION_INFRASTRUCTURE.md) - Transaction system
- [SETTLEMENT_WORKFLOW.md](../03_CORE_SYSTEMS/SETTLEMENT_WORKFLOW.md) - Settlement process
- [REFERRAL_SYSTEM.md](../03_CORE_SYSTEMS/REFERRAL_SYSTEM.md) - Commission tracking

