# Pre-Production Implementation Plan

> **Created:** February 27, 2026
> **Last Updated:** February 28, 2026
> **Scope:** Both `SF_management` (backend) and `SF_management-front` (frontend)
> **Goal:** Prioritize and sequence remaining implementation work before production launch

---

## Plan Status Assessment

### Already Implemented / Can Be Archived

These plans are fully implemented or their remaining items are negligible. No further action needed.

| Plan | Project | Assessment |
|------|---------|------------|
| `DOCUMENTATION_IMPROVEMENT_STUDY.md` | Both | ✅ Fully implemented — all recommendations applied. Archived. |
| `FINANCE_MODULE_VISION.md` | Backend | ✅ Superseded — Phases 1-4 implemented via `FINANCE_MODULE_UPGRADE_PLAN.md`. Remaining phases (Invoices, Expenses, Ledger) are post-production features. Archived. |
| `TRANSACTION_BUGS_FIX_PLAN.md` | Backend | ⚠️ Issues 1, 3, 4 implemented. Issue 2 (settlement rake commission in planilha) deferred — low impact, cosmetic. Archived; Issue 2 tracked as known issue. |
| `BALANCE_IMPROVEMENTS_PLAN.md` | Backend | ✅ Critical items implemented (date filtering, manager fields, sign documentation). Archived. Remaining items are code quality improvements — deferred to post-production. |
| `CODE_IMPROVEMENT_PLAN.md` | Frontend | ⚠️ Phases 1, 2, 4, 5 done. Phase 3 (remove console.log, empty dirs, commented code) and Phase 6 (split TransactionTable) remain — good pre-production cleanup. **Include in Sprint 1.** |

### Not Relevant Pre-Production

These plans are future features or long-term improvements that provide no benefit before launch. Defer to post-production.

| Plan | Project | Reason to Defer |
|------|---------|-----------------|
| `CONTRACTS_IMPLEMENTATION_PLAN.md` | Backend | New feature (recurring payments). Not needed before production. |
| `TESTING_STRATEGY_PLAN.md` | Backend | Important long-term but won't block production. Better to build tests against stable production code. |
| `CLEAN_ARCHITECTURE_IMPROVEMENT_PLAN.md` | Backend | Refactoring. System works correctly. Risky to restructure before launch. |
| `ASSETGROUP_FLEXIBLE_RENAME_PLAN.md` | Both | ✅ Implemented and archived. |
| `FOLLOWUP_SYSTEM_WALLET_ADMIN_UI.md` | Frontend | Nice-to-have admin UI. Wallets can be managed via API/database directly for now. |
| `BALANCE_IMPROVEMENTS_PLAN.md` (remaining items) | Backend | Typed DTOs, sign metadata, total endpoint — code quality only. Current implementation works correctly. |

### Candidates for Pre-Production Implementation

These plans address real UX issues or code quality that should be resolved while the system has no production users.

| Plan | Project | Why Pre-Production |
|------|---------|-------------------|
| `STATEMENT_REFACTOR_PLAN.md` | Frontend | Standardizes all entity statement pages. Better to unify before users learn inconsistent interfaces. |
| `DATE_SELECTOR_UX_IMPROVEMENT_PLAN.md` | Frontend | Prevents unnecessary API calls. Users will notice the double-fetch behavior immediately. |
| `CODE_IMPROVEMENT_PLAN.md` (Phase 3) | Frontend | Clean console.log/dead code. Professional polish before launch. |
| `FOLLOWUP_WALLET_CREATION_INTERNAL_MODE.md` | Frontend | P1 priority. Extends existing wallet creation to INTERNAL mode. Low effort, high value. |
| `FOLLOWUP_WALLET_CREATION_SALE_PURCHASE_MODE.md` | Frontend | P2 priority. Extends to SALE/PURCHASE modes. Prevents user confusion when wallets are missing. |

---

## Implementation Sprints

### Sprint 1 — Code Quality & Polish (Est. 2-3 days)

**Rationale:** Clean up code before production. Low risk, high polish.

| # | Task | Project | Source Plan | Est. |
|---|------|---------|-------------|------|
| 1.1 | Remove all `console.log` / `console.warn` / `console.error` from production code | Frontend | `CODE_IMPROVEMENT_PLAN.md` Phase 3.1 | 1h |
| 1.2 | Remove empty directories (`features/imports/*/hooks/`) | Frontend | `CODE_IMPROVEMENT_PLAN.md` Phase 3.2 | 30min |
| 1.3 | Remove commented-out code blocks | Frontend | `CODE_IMPROVEMENT_PLAN.md` Phase 3.3 | 1h |
| 1.4 | Remove empty `useEffect` callbacks | Frontend | `CODE_IMPROVEMENT_PLAN.md` Phase 3.4 | 30min |
| 1.5 | Run `yarn build` and fix any warnings/errors | Frontend | — | 1h |
| 1.6 | Verify RBAC behavior for all 3 roles (admin, manager, partner) | Both | — | 2h |
| 1.7 | Verify sign-out works correctly for all roles | Frontend | — | 30min |

**Acceptance:** Clean build with zero warnings, no console output in production, RBAC verified.

---

### Sprint 2 — Statement Display Improvements (Est. 3-4 days)

**Rationale:** All 4 entity statements already use the shared `AssetHolderStatementView`. This sprint fixes display issues and adds entity-specific rendering rules identified during pre-production review with real data.

| # | Task | Project | Source Plan | Est. |
|---|------|---------|-------------|------|
| 2.1 | Hide "@ 1.00" exchange rate — only show when `conversionRate !== 1` | Frontend | `STATEMENT_REFACTOR_PLAN.md` Phase 1 | 1h |
| 2.2 | Rename "Enviado para"/"Recebido de" → "Creditado para"/"Abatido de" | Frontend | `STATEMENT_REFACTOR_PLAN.md` Phase 1 | 30min |
| 2.3 | Add tooltip on hover for Origem column with full wallet details | Frontend | `STATEMENT_REFACTOR_PLAN.md` Phase 1 | 3h |
| 2.4 | Show category name for system wallet transactions (instead of wallet info) | Both | `STATEMENT_REFACTOR_PLAN.md` Phase 1 | 3h |
| 2.5 | Display Rate when present, rename balance labels (Fichas/Reais) | Frontend | `STATEMENT_REFACTOR_PLAN.md` Phase 1 | 1.5h |
| 2.6 | Origem column: show counterparty wallet (not entity's own) for all entities | Both | `STATEMENT_REFACTOR_PLAN.md` Phase 2 | 5h |
| 2.7 | Settlement dual-row display: chips + rakeback rows for managers | Both | `STATEMENT_REFACTOR_PLAN.md` Phase 3 | 6h |

**Acceptance:** Origem shows counterparty info, exchange rate hidden when 1.00, labels corrected, balance badges renamed, settlement shows dual rows for managers, tooltips on wallet info.

---

### ~~Sprint 3 — Wallet Creation UX~~ — Cancelled

After reassessment:
- **INTERNAL mode wallet creation** — Not needed. INTERNAL transactions are admin-only operations. Wallets should be created via admin entity management, not inline.
- **SALE/PURCHASE mode wallet creation** — Cancelled. Inline wallet creation should only be available for **Clients** (safety concern). PokerManager wallets should be set up by admins via `/entidades/administradoras/[mid]/fichas`.
- **DateSelector UX** — Already implemented. Archived.

Both `FOLLOWUP_WALLET_CREATION_INTERNAL_MODE.md` and `FOLLOWUP_WALLET_CREATION_SALE_PURCHASE_MODE.md` have been cancelled and archived.

---

### Sprint 3 — Naming Convention Improvements — Completed

**Rationale:** Pre-production was the ideal time for breaking naming changes — no active API consumers, no data migration needed. This sprint is complete and archived.

| # | Task | Project | Source Plan | Est. |
|---|------|---------|-------------|------|
| 3.1 | Fix 3 DTOs: `DirectIncomeDetailsDto` → `Response`, `AvgRateSnapshot` → `Response`, `CacheStatistics` → `Response` | Backend | `ASSETGROUP_FLEXIBLE_RENAME_PLAN.md` Phase 1 | ✅ Done |
| 3.2 | Fix `AccountClassification` enum: ALL_CAPS → PascalCase (`ASSET` → `Asset`, etc.) | Both | `ASSETGROUP_FLEXIBLE_RENAME_PLAN.md` Phase 2 | ✅ Done |
| 3.3 | Rename `AssetGroup.Internal` → `Flexible` across backend (46+ files) | Backend | `ASSETGROUP_FLEXIBLE_RENAME_PLAN.md` Phase 3 | ✅ Done |
| 3.4 | Rename `AssetGroup.Internal` → `Flexible` across frontend + rename `InternalWalletsCheck.tsx` | Frontend | `ASSETGROUP_FLEXIBLE_RENAME_PLAN.md` Phase 3 | ✅ Done |
| 3.5 | Rename API endpoint `/internal-wallet` → `/flexible-wallet` | Both | `ASSETGROUP_FLEXIBLE_RENAME_PLAN.md` Phase 3 | ✅ Done |
| 3.6 | Create `FlexibleWalletTypes` helper (backend) + `FlexibleWalletBehavior` constants (frontend) | Both | `ASSETGROUP_FLEXIBLE_RENAME_PLAN.md` Phase 4 | ✅ Done |
| 3.7 | Update all documentation references | Both | `ASSETGROUP_FLEXIBLE_RENAME_PLAN.md` Phase 5 | ✅ Done |

**Acceptance:** Met. No remaining `AssetGroup.Internal` references in active code, all DTOs follow `*Response` pattern, `AccountClassification` uses PascalCase, and documentation references were updated.

---

### Sprint 4 — Administration Tools (Est. 2 days)

**Rationale:** Admin tools for system wallet management and internal transactions. Uses existing backend endpoints — frontend-only work. These tools are needed before production so admins can manage system wallets and create administrative transactions without using the API directly.

| # | Task | Project | Source Plan | Est. |
|---|------|---------|-------------|------|
| 4.1 | Create `company-pools` feature module (service, queries, types) | Frontend | `FOLLOWUP_SYSTEM_WALLET_ADMIN_UI.md` Phase 1 | 3h |
| 4.2 | Create System Wallet management page at `/administracao/carteiras-sistema` (pool list, wallet coverage, create pool/wallet modals) | Frontend | `FOLLOWUP_SYSTEM_WALLET_ADMIN_UI.md` Phase 1 | 5h |
| 4.3 | Create `AdminTransactionForm` (simplified wallet selection, no mode restrictions) | Frontend | `FOLLOWUP_SYSTEM_WALLET_ADMIN_UI.md` Phase 2 | 3h |
| 4.4 | Create Internal Transactions page at `/administracao/transacoes-internas` with recent history | Frontend | `FOLLOWUP_SYSTEM_WALLET_ADMIN_UI.md` Phase 2 | 2h |
| 4.5 | Update navigation config and route constants | Frontend | — | 30min |

**Acceptance:** System wallet page shows all company pools with coverage analysis. Admin can create pools and wallets. Internal transaction page allows admin to create transactions between any two wallets. Both pages admin-only.

---

## Plans Explicitly Deferred to Post-Production

| Plan | Project | Reason | Revisit When |
|------|---------|--------|-------------|
| `TESTING_STRATEGY_PLAN.md` | Backend | Build tests against stable production code | After first production month |
| `CLEAN_ARCHITECTURE_IMPROVEMENT_PLAN.md` | Backend | Refactoring risk too high pre-launch | After production stabilization |
| `CONTRACTS_IMPLEMENTATION_PLAN.md` | Backend | New feature, not needed for launch | When business requests it |
| `ASSETGROUP_FLEXIBLE_RENAME_PLAN.md` | Both | Implemented and archived |
| `BALANCE_IMPROVEMENTS_PLAN.md` (remaining) | Backend | Typed DTOs, total endpoint — code quality only | Post-production cleanup sprint |
| ~~`FOLLOWUP_SYSTEM_WALLET_ADMIN_UI.md`~~ | Frontend | **Promoted to Sprint 4** |
| `FOLLOWUP_WALLET_CREATION_RECEIPT_PAYMENT_MODE.md` | Frontend | Cancelled — wallet creation is Client-only | Reassess if needed |
| `TRANSACTION_BUGS_FIX_PLAN.md` Issue 2 | Backend | Cosmetic issue in planilha balance | Post-production bug backlog |

---

## Plans to Archive After Sprints

After completing the sprints above, these plans should be moved to `development/archive/`:

| Plan | Project | Reason |
|------|---------|--------|
| `CODE_IMPROVEMENT_PLAN.md` | Frontend | Will be fully complete after Sprint 1 |
| `STATEMENT_REFACTOR_PLAN.md` | Frontend | Display improvements complete after Sprint 2 |
| `DATE_SELECTOR_UX_IMPROVEMENT_PLAN.md` | Frontend | Already implemented and archived |
| `FOLLOWUP_WALLET_CREATION_INTERNAL_MODE.md` | Frontend | Cancelled and archived — wallet creation is Client-only |
| `FOLLOWUP_WALLET_CREATION_SALE_PURCHASE_MODE.md` | Frontend | Cancelled and archived — wallet creation is Client-only |
| `ASSETGROUP_FLEXIBLE_RENAME_PLAN.md` | Both | Naming improvements complete; archived |
| `FOLLOWUP_SYSTEM_WALLET_ADMIN_UI.md` | Frontend | Admin tools complete after Sprint 4 |

---

## Sprint Dependency Graph

```
Sprint 1 (Code Quality & Polish)
    ↓
Sprint 2 (Statement Display)  ←── Cleaner code from Sprint 1 helps
Sprint 3 (Naming Conventions) ←── Independent, can run in parallel with Sprint 2
Sprint 4 (Admin Tools)        ←── Should run AFTER Sprint 3 (uses renamed AssetGroup.Flexible)
```

**Total estimated effort:** 9-12 days across 4 sprints.

Sprints 2 and 3 are independent and can overlap. Sprint 4 should follow Sprint 3 (uses the new `Flexible` naming).

---

## Risk Assessment

| Risk | Impact | Mitigation |
|------|--------|------------|
| Statement display changes affect all 4 entities | Medium | Test all 4 entity types after each change since they share TransactionTable |
| Settlement dual-row rendering may need backend changes | Medium | Investigate backend response first before frontend work |
| AssetGroup rename affects 46+ files | Medium | Use IDE "Find All References" + grep to catch all occurrences |
| AccountClassification PascalCase may change JSON serialization | Medium | Test frontend enum comparison before/after |
| Admin transaction form complexity | Low | Reuse existing `/transfer` endpoint, simplified wallet selection |

---

## Success Criteria

The system is ready for production when:
1. `yarn build` and `dotnet build` produce zero warnings
2. No `console.log` in production code
3. All 3 RBAC roles verified end-to-end
4. Statement display shows counterparty info, correct labels, tooltips, and settlement dual rows
5. Conversão button accessible from PokerManager statement page
6. No `AssetGroup.Internal` references remain — all renamed to `Flexible`
7. All DTOs follow `*Response` naming pattern
8. `AccountClassification` enum uses PascalCase values
9. System wallet management page at `/administracao/carteiras-sistema`
10. Internal transactions page at `/administracao/transacoes-internas`
