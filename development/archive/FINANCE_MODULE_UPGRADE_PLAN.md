# Finance Module Upgrade Plan

> **Status:** ✅ Implemented — Phases 1-4 Complete
> **Date:** February 26, 2026
> **Last Updated:** February 27, 2026
> **Scope:** Full-stack (SF_management + SF_management-front)
> **Predecessor:** `FINANCE_MODULE_VISION.md`

---

## Table of Contents

- [1. Overview](#1-overview)
- [2. Current State](#2-current-state)
- [3. Phase 1: Report Page Refactoring (Completed)](#3-phase-1-report-page-refactoring)
- [4. Phase 2: Profit Detail Modals — Backend Endpoints Required](#4-phase-2-profit-detail-modals--backend-endpoints-required)
- [5. Phase 3: New Finance Sub-Modules](#5-phase-3-new-finance-sub-modules)
- [6. Route Structure and Navigation](#6-route-structure-and-navigation)
- [7. RBAC Integration](#7-rbac-integration)
- [8. Backend Architecture for New Modules](#8-backend-architecture-for-new-modules)
- [9. Frontend Architecture for New Modules](#9-frontend-architecture-for-new-modules)
- [10. Implementation Order](#10-implementation-order)
- [11. Open Questions](#11-open-questions)

---

## 1. Overview

The finance module is being upgraded from a single "Planilha Financeira" (financial spreadsheet) page into a full financial reporting suite with four sub-modules:

| Module | Route | Status | Description |
|--------|-------|--------|-------------|
| **Relatório** | `/financeiro/relatorio` | Active | Monthly financial report (renamed from `/planilha`) |
| **Notas Fiscais** | `/financeiro/notas-fiscais` | Placeholder | Invoice generation and tax obligations |
| **Despesas** | `/financeiro/despesas` | Placeholder | Recurring expense management |
| **Consolidado** | `/financeiro/consolidado` | Placeholder | Ledger-based document (Brazilian compliance) |

### Goals

1. Make the Relatório page interactive — profit line items open detail modals
2. Clean up the report layout — remove redundant sections
3. Lay groundwork for invoicing, expenses, and consolidation modules
4. Align with the planned RBAC system (see `SF_management-front/development/RBAC_IMPLEMENTATION_STUDY.md`)

---

## 2. Current State

### 2.1 Backend (SF_management)

#### Existing Finance Endpoints

| Endpoint | Method | Returns |
|----------|--------|---------|
| `/api/v1/finance/profit/summary` | GET | `ProfitSummary` — aggregated totals for 4 revenue sources |
| `/api/v1/finance/profit/by-manager` | GET | `List<ProfitByManager>` — per-manager breakdown |
| `/api/v1/finance/profit/by-source` | GET | `List<ProfitBySource>` — per-source breakdown |
| `/api/v1/finance/profit/direct-income-details` | GET | `DirectIncomeDetailsResponse` — itemized transactions |

All endpoints accept `startDate` and `endDate` query parameters (ISO format).

#### Revenue Sources (Business Logic)

| Source | Service Method | BRL Conversion | Transaction-Level Detail |
|--------|---------------|----------------|--------------------------|
| Direct Income | `CalculateDirectIncome()` | Via AvgRate for non-BRL | ✅ Yes (via `/direct-income-details`) |
| Rake Commission | `CalculateRakeCommission()` | Via AvgRate | ❌ No endpoint |
| Rate Fees | `CalculateRateFees()` | Via AvgRate | ❌ No endpoint |
| Spread Profit | `CalculateSpreadProfit()` | Already BRL | ❌ No endpoint |

#### Key Limitation

**Three of four profit sources lack transaction-level detail endpoints.** The `/by-manager` endpoint provides per-manager breakdowns, but not individual transaction listings. To fully support detail modals, new endpoints are needed (see Phase 2).

### 2.2 Frontend (SF_management-front)

#### Feature Module Structure

```
src/features/finance/
├── api/
│   ├── finance.service.ts       # API client methods
│   └── finance.queries.ts       # React Query hooks (10+ hooks)
├── hooks/
│   └── usePlanilhaData.ts       # Centralized data orchestration
├── types/
│   └── finance.types.ts         # TypeScript interfaces
└── index.ts                     # Public API
```

#### Page Structure (after Phase 1)

```
src/app/(dashboard)/financeiro/
├── relatorio/
│   ├── page.tsx                    # Main report orchestrator
│   └── components/
│       ├── DateSelector.tsx         # Month/Year picker with localStorage
│       ├── ProfitSummaryCard.tsx     # Clickable profit items + modal triggers
│       ├── ProfitDetailDialog.tsx    # Modal for profit detail views
│       ├── BanksTableServer.tsx      # Bank balances wrapper
│       ├── BanksTableView.tsx        # Bank balances table
│       ├── ClientsBalanceServer.tsx  # Client balances wrapper
│       ├── ClientsBalanceView.tsx    # Debtors/Creditors split table
│       ├── ManagersTableClient.tsx   # Manager balances wrapper
│       ├── ManagersTableView.tsx     # Manager balances + profit table
│       ├── ConsolidatedView.tsx      # Assets vs Liabilities cards
│       └── FinancialSummarySection.tsx # Groups managers + consolidated
├── notas-fiscais/
│   └── page.tsx                    # Placeholder
├── despesas/
│   └── page.tsx                    # Placeholder
└── consolidado/
    └── page.tsx                    # Placeholder
```

#### Data Flow

```
DateSelector → DateContext → usePlanilhaData
                                 │
                                 ├── useBanks() + useBanksWithBalances()
                                 ├── useClients() + useClientsWithBalances()
                                 ├── usePokerManagers() + useManagersWithBalances()
                                 ├── useProfitSummary(start, end)
                                 ├── useProfitByManager(start, end)
                                 └── useDirectIncomeDetails(start, end)
                                          │
                                          ▼
                              PlanilhaData interface
                                          │
                                          ▼
                              Components render data
```

---

## 3. Phase 1: Report Page Refactoring

**Status: ✅ Completed**

### 3.1 Route Rename

| Change | Before | After |
|--------|--------|-------|
| Directory | `financeiro/planilha/` | `financeiro/relatorio/` |
| Page title | "Planilha Financeira" | "Relatório Financeiro" |
| NavBar item | `['Planilha']` | `['Relatorio', 'Notas-fiscais', 'Despesas', 'Consolidado']` |
| Route constant | `FINANCE_REPORT: '/financeiro/planilha'` | `FINANCE_REPORT: '/financeiro/relatorio'` |

### 3.2 Interactive Profit Summary

The `ProfitSummaryCard` component now has 4 clickable profit items. Each opens a `ProfitDetailDialog`:

| Profit Item | Modal Content | Data Source |
|-------------|---------------|-------------|
| **Receita Direta** | Income + Expense tables with net result | `directIncomeDetails` (already fetched) |
| **Taxa (Rate Fees)** | Per-manager breakdown | `profitByManager[].rateFees` |
| **Rake Commission** | Per-manager breakdown | `profitByManager[].rakeCommission` |
| **Spread** | Per-manager breakdown | `profitByManager[].spreadProfit` |

### 3.3 Sections Removed from Main Page

| Section | Reason | New Location |
|---------|--------|--------------|
| Receitas table | Moved to Receita Direta modal | `ProfitDetailDialog` |
| Despesas table | Moved to Receita Direta modal | `ProfitDetailDialog` |
| Resultado Direto do Período | Moved to Receita Direta modal | `ProfitDetailDialog` |
| Resultado Total | Redundant with "Total" in ProfitSummaryCard | Removed (total already displayed) |

### 3.4 Final Report Layout

```
┌───────────────────────────────────────────────┐
│ Relatório Financeiro                           │
├───────────────────────────────────────────────┤
│ [DateSelector]                                 │
│                                                │
│ ┌─ Lucro do Período ────────────────────────┐ │
│ │ [Receita Direta] [Rate Fees] [Rake] [Spread] │ Total │
│ │  (clickable)     (clickable) (click) (click)  │       │
│ └───────────────────────────────────────────┘ │
│                                                │
│ ┌─ Bancos ──────────────────────────────────┐ │
│ │ Bank name                          Balance │ │
│ └───────────────────────────────────────────┘ │
│                                                │
│ ┌─ Clientes Devedores ┐ ┌─ Clientes Credores ┐ │
│ │ Client   Balance     │ │ Client   Balance    │ │
│ └─────────────────────┘ └────────────────────┘ │
│                                                │
│ ┌─ Administradoras ─────────────────────────┐ │
│ │ Manager  Chips  Rate  Cash  Profit         │ │
│ └───────────────────────────────────────────┘ │
│                                                │
│ ┌─ Ativos ─────────┐ ┌─ Passivos ──────────┐ │
│ │ R$ X.XXX,XX      │ │ R$ X.XXX,XX         │ │
│ └──────────────────┘ └─────────────────────┘ │
└───────────────────────────────────────────────┘
```

---

## 4. Phase 2: Profit Detail Endpoints

**Status: ✅ Completed**

Three new detail endpoints were added following the `direct-income-details` pattern. The decision to create **new endpoints** (rather than extending existing ones) was based on:
- Existing scalar methods are `private` and called by `GetProfitSummary` — modifying them risks breaking the summary
- Lazy loading: frontend only fetches detail data when a modal is actually opened
- The `DirectIncomeDetails` pattern already establishes the convention

### 4.1 New Endpoints

| Endpoint | Returns | Source Data |
|----------|---------|-------------|
| `GET /profit/rate-fee-details` | `RateFeeDetailsResponse` | `DigitalAssetTransaction` where `Rate != 0` |
| `GET /profit/rake-commission-details` | `RakeCommissionDetailsResponse` | `SettlementTransaction` where `RakeAmount > 0` |
| `GET /profit/spread-details` | `SpreadProfitDetailsResponse` | `DigitalAssetTransaction` SALE with `ConversionRate` |

All accept `startDate` and `endDate` query parameters.

### 4.2 Backend Implementation

**New DTOs** (`Application/DTOs/Finance/ProfitDetailDtos.cs`):

| DTO | Fields per item |
|-----|-----------------|
| `RateFeeItem` | `TransactionId`, `Date`, `ManagerName`, `AssetAmount`, `RatePct`, `FeeChips`, `AvgRate`, `FeeBRL` |
| `RakeCommissionItem` | `SettlementId`, `Date`, `ManagerName`, `RakeAmount`, `RakeCommissionPct`, `RakeBackPct`, `RakeChips`, `AvgRate`, `RakeBRL` |
| `SpreadProfitItem` | `TransactionId`, `Date`, `ManagerName`, `AssetAmount`, `SaleRate`, `AvgRate`, `SpreadBRL` |

**Service methods** follow the same query and iteration patterns as the existing private scalar methods, but capture per-item detail including the AvgRate used for each BRL conversion.

**Controller** endpoints follow the same validation/date-resolution pattern as the existing endpoints.

### 4.3 Frontend Implementation

**Types:** New interfaces in `finance.types.ts` matching the backend DTOs.

**Hooks:** Three new React Query hooks with an `enabled` parameter for lazy loading:
- `useRateFeeDetails(startDate, endDate, enabled)`
- `useRakeCommissionDetails(startDate, endDate, enabled)`
- `useSpreadProfitDetails(startDate, endDate, enabled)`

**Dialog:** `ProfitDetailDialog` renders a loader component per type that only fires the API call when the dialog opens:

| Modal | Columns |
|-------|---------|
| Rate Fees | Data, Administradora, Qtd Fichas, Taxa %, Fee (fichas), Cotação, Valor (R$) |
| Rake Commission | Data, Administradora, Rake, Com. %, RB %, Líq. (fichas), Cotação, Valor (R$) |
| Spread | Data, Administradora, Qtd Fichas, Venda, Custo Médio, Lucro (R$) |

---

## 4B. Bug Fix: DirectIncome Removed from ProfitByManager

**Status: ✅ Completed**

Direct Income is a **system-level metric** — it comes from categorized transactions involving system wallets and is not attributable to any individual manager. The `/profit/by-manager` endpoint was incorrectly including `directIncome` for each manager (with identical values, since the same system-wide calculation was run per manager).

### Changes

**Backend (`ProfitDtos.cs`):**
- Removed `DirectIncome` property from `ProfitByManager` DTO
- Changed `TotalProfit` from a settable property to a computed property: `TotalProfit => RakeCommission + RateFees + SpreadProfit`

**Backend (`ProfitCalculationService.cs`):**
- `GetProfitByManager` now calls `CalculateRakeCommission`, `CalculateRateFees`, and `CalculateSpreadProfit` directly for each manager, instead of calling `GetProfitSummary` (which included Direct Income)

**Frontend (`finance.types.ts`):**
- Removed `directIncome` from `ProfitByManager` interface

**Direct Income is now exclusively served by `/profit/direct-income-details`.**

---

## 4C. Bug Fix: Settlement AssetAmount in Balance for RakeOverrideCommission Managers

**Status: ✅ Completed**

The balance calculation in `BaseAssetHolderService` (used by both `GetBalancesByAssetGroup` and `GetBalancesByAssetType`) previously only accounted for the **rake impact** from SettlementTransactions, ignoring the `AssetAmount` (the actual chips transferred). For `RakeOverrideCommission` managers, this meant the chip flow from settlements was invisible in their balance.

### Business Rule

For `RakeOverrideCommission` managers, settlements represent chip transfers that must be reflected in both asset pools:

| Balance Pool | Signal | Rationale |
|-------------|--------|-----------|
| **PokerAssets** (chips) | receiver +, sender - | Chips are received or sent |
| **FiatAssets** (BRL) | receiver +, sender - | Cash position follows chip flow |

When a manager receives 1000 chips via settlement, both PokerAssets and FiatAssets increase by 1000. When they send chips, both decrease.

### Changes

**`BaseAssetHolderService.GetBalancesByAssetGroup`:**
- Added lookup for `RakeOverrideCommission` manager IDs
- After the existing rake impact, adds `AssetAmount` to `PokerAssets` (normal signal) and `FiatAssets` (inverted signal) for these managers

**`BaseAssetHolderService.GetBalancesByAssetType`:**
- Same fix applied for consistency — uses the wallet's `AssetType` to determine the chip asset type

### Impact

Both the Relatório "Administradoras" table and the individual manager statement page (`/administradoras/{id}/extrato`) use the same balance endpoint, so this fix applies to both views.

---

## 4D. Cotação (AvgRate) for the Administradoras Table

**Status: ✅ Completed**

The "Cotação" column in the Administradoras table was showing 0 for all managers because the balance endpoint does not return AvgRate.

### Solution

**New backend endpoint:** `GET /api/v1/finance/profit/avg-rates?asOfDate=YYYY-MM-DD`

Returns a `Dictionary<Guid, decimal>` mapping each manager's `BaseAssetHolderId` to their AvgRate:
- **RakeOverrideCommission** managers: always `1` (chips valued at face value)
- **Spread** managers: actual AvgRate from `IAvgRateService.GetAvgRateAtDate()`

**Frontend integration:** `usePlanilhaData` now fetches `useManagerAvgRates(asOfDate)` and merges the rate into `ManagerWithBalance.rate` before the data reaches components. This ensures:
- The Administradoras table "Cotação" column shows the correct rate
- The Assets/Liabilities calculation uses the correct rate for chip valuation

---

## 4F. Devedores/Credores: Clients + Members Combined

**Status: ✅ Completed**

Previously the report had "Clientes Devedores" and "Clientes Credores" showing only clients. This was renamed to "Devedores" and "Credores" and now includes **both Clients and Members**.

### Changes

**`usePlanilhaData.ts`:**
- Added `useMembers()` + `useMembersWithBalances()` data fetching
- New `debtors` / `creditors` arrays computed by merging client and member balances into `EntityWithBalance[]` and splitting by sign

**Components:**
- `ClientsBalanceView.tsx` rewritten as `DebtorsCreditorsSection` accepting `debtors` and `creditors` props of type `EntityWithBalance[]`
- `ClientsBalanceServer.tsx` updated to pass the new prop shape
- Headers changed from "Clientes Devedores/Credores" to "Devedores/Credores", column from "Cliente" to "Nome"

---

## 4G. Revised Ativos/Passivos Formulas

**Status: ✅ Completed**

The Assets and Liabilities calculation was rewritten to match the correct financial model:

### Ativos (Assets)

| Source | Rule |
|--------|------|
| Banks | Positive bank balance |
| Devedores | Absolute value of debtor balance (clients + members who owe us) |
| Administradoras (cash) | Absolute value of negative "Saldo em Dinheiro" |
| Administradoras (chips) | `Saldo em Fichas × Cotação` for **Spread** managers only |

### Passivos (Liabilities)

| Source | Rule |
|--------|------|
| Banks | Absolute value of negative bank balance |
| Credores | Full creditor balance (we owe clients + members) |
| Administradoras (cash) | Positive "Saldo em Dinheiro" |
| Administradoras (chips) | `Saldo em Fichas × Cotação` for **RakeOverrideCommission** managers |

**Perspective:** balances are from the company's point of view.
- **Devedores** (negative balance) = they owe the company → **Ativo**
- **Credores** (positive balance) = the company owes them → **Passivo**
- **Administradoras positive cash** = company owes the manager → **Passivo**
- **Administradoras negative cash** = manager owes the company → **Ativo**
- **Spread chips** = company-owned inventory → **Ativo**
- **RakeOverride chips** = chips owed in the ecosystem → **Passivo**

Cotação is displayed with 3 decimal places in the Administradoras table.

---

## 4H. Investigation: Duplicate Balance Services (Deferred)

Two frontend services fetch the same balance data from the same backend endpoint:

| Frontend Service | Method | Backend Endpoint |
|-----------------|--------|-----------------|
| `finance.service.ts` | `getManagerBalance()` | `GET /pokermanager/{id}/balance` |
| `poker-manager.service.ts` | `getBalancesByAssetType()` | `GET /pokermanager/{id}/balance` |

The same duplication exists for clients and banks. This is an unnecessary repetition that should be consolidated. **Deferred for later investigation** — potential approaches:
1. Remove balance methods from `finance.service.ts` and use entity-specific services
2. Create a shared `BalanceService` consumed by both features
3. Keep both but ensure cache keys are shared to avoid duplicate API calls

---

## 5. Phase 3: New Finance Sub-Modules

### 5.1 Notas Fiscais (Invoicing)

**Purpose:** Generate invoices based on profits and create tax obligation records.

#### Backend Requirements

**New Entity: `Invoice`**
```csharp
public class Invoice : BaseEntity
{
    public int InvoiceNumber { get; set; }        // Sequential numbering
    public DateTime IssueDate { get; set; }
    public DateTime ReferenceStart { get; set; }  // Period covered
    public DateTime ReferenceEnd { get; set; }
    public decimal GrossAmount { get; set; }       // From ProfitSummary
    public decimal TaxAmount { get; set; }         // Calculated taxes
    public decimal NetAmount { get; set; }         // Gross - Taxes
    public InvoiceStatus Status { get; set; }      // Draft/Issued/Paid/Cancelled
    public string? Notes { get; set; }

    // Tax breakdown
    public decimal? IRPJ { get; set; }
    public decimal? CSLL { get; set; }
    public decimal? PIS { get; set; }
    public decimal? COFINS { get; set; }
}

public enum InvoiceStatus { Draft, Issued, Paid, Cancelled }
```

**New Endpoints:**
```
POST   /api/v1/finance/invoices           # Create from profit period
GET    /api/v1/finance/invoices           # List with filters
GET    /api/v1/finance/invoices/{id}      # Detail
PUT    /api/v1/finance/invoices/{id}      # Update status/notes
DELETE /api/v1/finance/invoices/{id}      # Soft delete
POST   /api/v1/finance/invoices/{id}/pdf  # Generate PDF
```

**New Service: `IInvoiceService`**
- `GenerateFromPeriod(start, end)` — Creates invoice from ProfitSummary
- `CalculateTaxes(grossAmount)` — Applies tax regime rules
- `GeneratePDF(invoiceId)` — PDF export

#### Frontend Requirements

- `src/features/finance/invoicing/` — New sub-feature module
- Invoice list page with status filters
- Invoice detail/edit page
- "Generate Invoice" flow: select period → preview profit → confirm → create
- PDF download button

### 5.2 Despesas (Expenses)

**Purpose:** Register and track recurring operational expenses.

#### Backend Requirements

**New Entity: `Expense`**
```csharp
public class Expense : BaseEntity
{
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public ExpenseCategory Category { get; set; }  // Operational/Admin/Financial
    public RecurrenceType? Recurrence { get; set; } // Monthly/Quarterly/Annual/None
    public Guid? CategoryId { get; set; }           // Link to existing Category entity
    public string? Notes { get; set; }
}

public enum ExpenseCategory { Operational, Administrative, Financial, Other }
public enum RecurrenceType { Monthly, Quarterly, Annual }
```

**New Endpoints:**
```
POST   /api/v1/finance/expenses            # Create
GET    /api/v1/finance/expenses            # List with date/category filters
PUT    /api/v1/finance/expenses/{id}       # Update
DELETE /api/v1/finance/expenses/{id}       # Soft delete
GET    /api/v1/finance/expenses/summary    # Aggregated by category for a period
```

**Relationship to Direct Income:**
The existing Direct Income system tracks transaction-based expenses (system wallet sends money). The Expense module tracks **operational costs** that may not be transaction-based (rent, utilities, salaries). These are complementary:
- Direct Income expenses = transaction-based, automatically captured
- Expense module = manually registered operational costs

#### Frontend Requirements

- `src/features/finance/expenses/` — New sub-feature module
- Expense list with date range and category filters
- Expense form (create/edit) with recurrence options
- Monthly expense summary view

### 5.3 Consolidado (Consolidation / Ledger)

**Purpose:** Compliance-oriented financial statements per Brazilian law.

#### Backend Requirements

This is the most complex module and requires accounting domain expertise.

**Core Capabilities:**
1. **Plano de Contas** (Chart of Accounts) — Hierarchical account structure
2. **Livro-Razão** (General Ledger) — Double-entry accounting records
3. **DRE** (Income Statement) — Revenue, COGS, Expenses, Profit
4. **Balanço Patrimonial** (Balance Sheet) — Assets, Liabilities, Equity
5. **Fechamento** (Period Closing) — Month/year-end closing process
6. **SPED Export** — Standard Public Digital Bookkeeping System

**New Entities:**
```csharp
public class AccountChart : BaseEntity
{
    public string Code { get; set; }           // e.g., "1.1.01"
    public string Description { get; set; }
    public AccountType Type { get; set; }       // Asset/Liability/Revenue/Expense/Equity
    public Guid? ParentAccountId { get; set; }  // Hierarchical
}

public class LedgerEntry : BaseEntity
{
    public DateTime Date { get; set; }
    public Guid DebitAccountId { get; set; }
    public Guid CreditAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public Guid? SourceTransactionId { get; set; }  // Link to originating transaction
}

public class PeriodClosing : BaseEntity
{
    public int Year { get; set; }
    public int Month { get; set; }
    public DateTime ClosedAt { get; set; }
    public bool IsLocked { get; set; }
}
```

**New Endpoints:**
```
GET    /api/v1/finance/consolidated/balance-sheet    # Balanço Patrimonial
GET    /api/v1/finance/consolidated/income-statement  # DRE
GET    /api/v1/finance/consolidated/ledger           # Livro-Razão entries
POST   /api/v1/finance/consolidated/close-period     # Lock period
GET    /api/v1/finance/consolidated/chart-of-accounts # Account tree
GET    /api/v1/finance/consolidated/export/sped      # SPED file generation
```

#### Frontend Requirements

- `src/features/finance/consolidation/` — New sub-feature module
- Balance Sheet view with drill-down
- Income Statement view
- Ledger entries table with search/filter
- Period closing workflow
- SPED export download

---

## 6. Route Structure and Navigation

### 6.1 Complete Route Map

```
/financeiro/
├── relatorio          # Monthly financial report (profit, balances, assets)
├── notas-fiscais      # Invoice generation and management
├── despesas           # Expense registration and tracking
└── consolidado        # Ledger, DRE, Balance Sheet, SPED
```

### 6.2 NavBar Configuration

```typescript
// Current NavBar structure
const pages = [
  'Dashboard', 'Transacoes', 'Administradoras', 'Bancos',
  'Clientes', 'Membros', 'Cadastros', 'Importacoes', 'Financeiro',
];

const settings = [
  ['Dashboard'],
  ['Lancadas'],
  ['Administradoras'],
  ['Bancos'],
  ['Clientes'],
  ['Membros'],
  ['Rotulos', 'Usuarios'],
  ['Bancos-ofx', 'Fichas-cred'],
  ['Relatorio', 'Notas-fiscais', 'Despesas', 'Consolidado'],  // Updated
];
```

### 6.3 Constants

```typescript
export const ROUTES = {
  // ... existing routes
  FINANCE: '/financeiro',
  FINANCE_REPORT: '/financeiro/relatorio',
  FINANCE_INVOICES: '/financeiro/notas-fiscais',
  FINANCE_EXPENSES: '/financeiro/despesas',
  FINANCE_LEDGER: '/financeiro/consolidado',
} as const;
```

---

## 7. RBAC Integration

The finance module routes must align with the role-based access control system (documented in `RBAC_IMPLEMENTATION_STUDY.md`).

### 7.1 Route Access Matrix

| Route | Admin | Manager | Partner |
|-------|-------|---------|---------|
| `/financeiro/relatorio` | ✅ | ❌ | ✅ (read-only, scoped data) |
| `/financeiro/notas-fiscais` | ✅ | ❌ | ❌ |
| `/financeiro/despesas` | ✅ | ❌ | ❌ |
| `/financeiro/consolidado` | ✅ | ❌ | ✅ (read-only) |

### 7.2 Required Permissions

| Permission | Description | Roles |
|------------|-------------|-------|
| `read:financial_data` | View report and consolidated | admin, partner |
| `read:invoices` | View invoices | admin |
| `create:invoices` | Generate invoices | admin |
| `read:expenses` | View expenses | admin |
| `create:expenses` | Register expenses | admin |
| `update:expenses` | Edit expenses | admin |
| `delete:expenses` | Remove expenses | admin |
| `read:ledger` | View consolidated ledger | admin, partner |
| `close:period` | Lock accounting periods | admin |

### 7.3 Partner Data Scoping

Partners should see only data relevant to their entity. For the Relatório page, this means:
- Profit Summary filtered by their associated `managerId`
- Balance views scoped to their entity
- No access to other managers' data

**Backend:** The `managerId` filter parameter already exists on `/profit/summary`. Extend it to other endpoints.
**Frontend:** The middleware should pass the partner's `managerId` to the API calls.

---

## 8. Backend Architecture for New Modules

### 8.1 Service Layer

```
Application/Services/Finance/
├── ProfitCalculationService.cs      # Existing — add detail methods
├── AvgRateService.cs                # Existing
├── InvoiceService.cs                # New
├── ExpenseService.cs                # New
└── ConsolidationService.cs          # New
```

### 8.2 Controller Layer

```
Api/Controllers/v1/Finance/
├── ProfitController.cs              # Existing — add detail endpoints
├── InvoiceController.cs             # New
├── ExpenseController.cs             # New
└── ConsolidationController.cs       # New
```

### 8.3 Entity Layer

```
Domain/Entities/Finance/
├── Invoice.cs                       # New
├── Expense.cs                       # New
├── AccountChart.cs                  # New
├── LedgerEntry.cs                   # New
└── PeriodClosing.cs                 # New
```

### 8.4 DTO Layer

```
Application/DTOs/Finance/
├── ProfitDtos.cs                    # Existing
├── DirectIncomeDetailsDto.cs        # Existing
├── RateFeeDetailsDto.cs             # New
├── RakeCommissionDetailsDto.cs      # New
├── SpreadDetailsDto.cs              # New
├── InvoiceDtos.cs                   # New
├── ExpenseDtos.cs                   # New
└── ConsolidationDtos.cs             # New
```

---

## 9. Frontend Architecture for New Modules

### 9.1 Feature Module Structure

```
src/features/finance/
├── api/
│   ├── finance.service.ts         # Existing — add detail methods
│   └── finance.queries.ts         # Existing — add detail hooks
├── hooks/
│   └── usePlanilhaData.ts         # Existing (rename later)
├── types/
│   └── finance.types.ts           # Existing — add detail types
├── invoicing/
│   ├── api/
│   │   ├── invoice.service.ts
│   │   └── invoice.queries.ts
│   ├── components/
│   │   ├── InvoiceList.tsx
│   │   ├── InvoiceForm.tsx
│   │   └── InvoicePreview.tsx
│   ├── types/
│   │   └── invoice.types.ts
│   └── index.ts
├── expenses/
│   ├── api/
│   │   ├── expense.service.ts
│   │   └── expense.queries.ts
│   ├── components/
│   │   ├── ExpenseList.tsx
│   │   ├── ExpenseForm.tsx
│   │   └── ExpenseSummary.tsx
│   ├── types/
│   │   └── expense.types.ts
│   └── index.ts
├── consolidation/
│   ├── api/
│   │   ├── consolidation.service.ts
│   │   └── consolidation.queries.ts
│   ├── components/
│   │   ├── BalanceSheet.tsx
│   │   ├── IncomeStatement.tsx
│   │   ├── LedgerEntries.tsx
│   │   └── PeriodClosing.tsx
│   ├── types/
│   │   └── consolidation.types.ts
│   └── index.ts
└── index.ts
```

### 9.2 Shared Components

Components that may be shared across finance modules:
- `DateSelector` — Already exists, could be extracted to shared
- `CurrencyDisplay` — Consistent BRL formatting
- `PeriodSelector` — Month/Year range selection
- `FinanceCard` — Standardized card with title and summary value

---

## 10. Implementation Order

### Priority and Dependencies

```
Phase 1: Report Refactoring (✅ DONE)
    ├── Route rename (/planilha → /relatorio)
    ├── Clickable profit items with modals
    └── Layout cleanup (remove redundant sections)

Phase 2: Profit Detail Endpoints (Next)
    ├── [Backend] Rate Fee Details endpoint
    ├── [Backend] Rake Commission Details endpoint
    ├── [Backend] Spread Details endpoint
    ├── [Frontend] New types + service methods + hooks
    └── [Frontend] Update ProfitDetailDialog for transaction-level data

Phase 3A: Expenses Module
    ├── [Backend] Expense entity + migration
    ├── [Backend] ExpenseService + ExpenseController
    ├── [Frontend] Expense feature module
    └── [Frontend] Expense pages

Phase 3B: Invoicing Module
    ├── [Backend] Invoice entity + migration
    ├── [Backend] InvoiceService + InvoiceController
    ├── [Backend] Tax calculation service
    ├── [Backend] PDF generation service
    ├── [Frontend] Invoice feature module
    └── [Frontend] Invoice pages

Phase 3C: Consolidation Module (requires accounting expertise)
    ├── [Backend] Chart of Accounts + Ledger entities + migration
    ├── [Backend] ConsolidationService
    ├── [Backend] SPED export service
    ├── [Frontend] Consolidation feature module
    └── [Frontend] Consolidation pages

Phase 4: RBAC Integration
    ├── [Backend] Apply [RequirePermission] to new controllers
    ├── [Frontend] Role-based route gating for finance routes
    └── [Frontend] Partner data scoping
```

### Estimated Complexity

| Phase | Backend | Frontend | Total |
|-------|---------|----------|-------|
| Phase 2: Detail Endpoints | Medium (3 endpoints, extract from existing logic) | Small (update dialog, add hooks) | Medium |
| Phase 3A: Expenses | Small (CRUD + simple entity) | Medium (list + form + summary) | Medium |
| Phase 3B: Invoicing | Medium (entity + tax calc + PDF) | Medium (list + form + preview) | Medium-Large |
| Phase 3C: Consolidation | Large (accounting logic, SPED) | Large (multiple views) | Large |

---

## 11. Open Questions

### Tax and Compliance

1. **Tax Regime:** Which tax regime applies? (Simples Nacional, Lucro Presumido, Lucro Real) — This determines tax rates for invoice calculations.
2. **NF-e Integration:** Should invoices integrate with the Brazilian NF-e system (electronic invoice), or are these internal invoices only?
3. **SPED Compliance:** Which SPED modules are required? (ECD, ECF, EFD-Contribuições) — This determines the complexity of the Consolidado module.
4. **Fiscal Year:** Does the company use calendar year (Jan-Dec) or a custom fiscal year?

### Business Logic

5. **Expense Approval:** Is an approval workflow needed, or are expenses admin-only with no approval step?
6. **Recurring Expenses:** Should recurring expenses auto-generate entries, or just show reminders?
7. **Invoice Auto-Generation:** Should invoices be generated automatically at period close, or always manually?
8. **Partner Scoping:** Should partners see the full Relatório, or only data related to their poker manager?

### Architecture

9. **PDF Library:** Which .NET PDF library to use for invoice generation? (QuestPDF, iText, Aspose)
10. **File Storage:** Where to store generated PDFs? (Azure Blob Storage, local filesystem, database binary)
11. **Accounting Standards:** Should the Consolidado module follow CPC (Brazilian GAAP) or simplified accounting?

---

## Related Documentation

| Document | Location | Purpose |
|----------|----------|---------|
| Finance Module Vision | `11_REFACTORING/FINANCE_MODULE_VISION.md` | Original business rules and revenue model |
| RBAC Study | `SF_management-front/development/RBAC_IMPLEMENTATION_STUDY.md` | Role-based access control integration |
| Profit Calculation System | `03_CORE_SYSTEMS/PROFIT_CALCULATION_SYSTEM.md` | Detailed calculation pipeline |
| Finance Deferred Features | `07_REFERENCE/FINANCE_DEFERRED_FEATURES.md` | Future feature backlog |

---

*Document Version: 1.0*
*Last Updated: February 26, 2026*
