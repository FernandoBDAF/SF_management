# Multi-Track Progress Control

> **Status:** In Progress  
> **Created:** January 24, 2026  
> **Purpose:** Central coordination document for 3 parallel improvement tracks

---

## Overview

This document tracks progress across 3 parallel workstreams improving the SF Management system:

| Track | Focus | Document | Status |
|-------|-------|----------|--------|
| **A** | Frontend-Backend API Alignment | [FRONTEND_BACKEND_ALIGNMENT.md](./FRONTEND_BACKEND_ALIGNMENT.md) | 🟡 In Progress |
| **B** | Statement Page Refactor | [STATEMENT_REFACTOR_PLAN.md](../../SF_management-front/development/STATEMENT_REFACTOR_PLAN.md) | 🟡 In Progress |
| **C** | Finance Module Review & Roadmap | [FINANCE_CURRENT_STATE.md](./FINANCE_CURRENT_STATE.md) | 🟡 In Progress |

---

## Track A: Frontend-Backend API Alignment

**Objective:** Document all mismatches between frontend services and backend endpoints; identify gaps for review.

**Key Insight:** Frontend doesn't need to implement ALL backend endpoints. Some may be outdated or unnecessary. The goal is to document discrepancies so we can decide whether to:
- Remove/update backend endpoint
- Implement frontend support
- Document as intentional gap

**Deliverables:**
- [x] Create `FRONTEND_BACKEND_ALIGNMENT.md`
- [ ] Complete API mapping
- [ ] Categorize gaps (critical, optional, outdated)
- [ ] Propose actions per gap

**Reference Documents:**
- [API_REFERENCE.md](../06_API/API_REFERENCE.md)
- [TRANSACTION_API_ENDPOINTS.md](../06_API/TRANSACTION_API_ENDPOINTS.md)
- [BALANCE_ENDPOINTS.md](../06_API/BALANCE_ENDPOINTS.md)

---

## Track B: Statement Page Refactor

**Objective:** Unify statement pages for all asset holders; improve structure, UX, and code quality while preserving business rules.

**Key Insight:** Backend business rules must drive the implementation. Statement calculation logic is deeply documented in:
- [BALANCE_SYSTEM_ANALYSIS.md](./BALANCE_SYSTEM_ANALYSIS.md)
- [TRANSACTION_BALANCE_IMPACT.md](../03_CORE_SYSTEMS/TRANSACTION_BALANCE_IMPACT.md)
- [ENTITY_BUSINESS_BEHAVIOR.md](../03_CORE_SYSTEMS/ENTITY_BUSINESS_BEHAVIOR.md)

**Focus:** AssetHolder statements only (Clients, Members, Banks, PokerManagers). Other statement types are out of scope.

**Deliverables:**
- [x] Create `STATEMENT_REFACTOR_PLAN.md`
- [ ] Document current architecture
- [ ] Identify pain points
- [ ] Design unified architecture
- [ ] Create implementation roadmap

**Reference Documents:**
- Backend: [BALANCE_ENDPOINTS.md](../06_API/BALANCE_ENDPOINTS.md)
- Frontend: `SF_management-front/documentation/03_CORE_SYSTEMS/BALANCE_DISPLAY_USAGE.md`

---

## Track C: Finance Module Review & Roadmap

**Objective:** Document current finance (financeiro) implementation state; create comprehensive roadmap for refactor and expansion.

**Key Insight:** Frontend has `/financeiro/planilha` with most UX/features for the first version. Backend may be missing corresponding infrastructure. The financial statement view should guide backend API design.

**Deliverables:**
- [x] Create `FINANCE_CURRENT_STATE.md`
- [ ] Document current implementation (backend + frontend)
- [ ] Identify gaps (date-filtered balances, profit tracking, etc.)
- [ ] Create phased implementation plan
- [ ] Update `FINANCE_MODULE_PLANNING.md` with findings

**Reference Documents:**
- [FINANCE_MODULE_PLANNING.md](./FINANCE_MODULE_PLANNING.md)
- [SETTLEMENT_WORKFLOW.md](../03_CORE_SYSTEMS/SETTLEMENT_WORKFLOW.md)
- [BALANCE_SYSTEM_ANALYSIS.md](./BALANCE_SYSTEM_ANALYSIS.md)

---

## Session Management

This document is managed by the **main session**.

**Parallel sessions** work on Tracks A, B, and C independently:
- Each session references its track document
- Updates progress in its track document
- Reports back to main session for coordination

**Main session responsibilities:**
- Update this document as tracks progress
- Coordinate cross-track dependencies
- Update `00_DOCUMENTATION_INDEX.md` when complete

---

## Cross-Track Dependencies

### Track A → Track B
Statement pages consume balance/transaction endpoints. Any API changes from Track A may impact Track B.

**Action:** Track B must review Track A findings before finalizing refactor plan.

### Track A → Track C
Finance module depends on balance endpoints. Date-filtered balance gaps identified in Track C impact Track A.

**Action:** Track A must document date-filtered balance endpoints for Track C implementation.

### Track C → Track B
Settlement balance display in statements depends on correct calculation (Track C fix).

**Action:** Track B should wait for settlement balance bug fix completion before finalizing statement balance logic.

---

## Progress Log

### January 24, 2026

**Track A:**
- Created base document
- Identified wallet identifiers path mismatch
- Identified missing statistics endpoints
- Status: Ready for full API mapping

**Track B:**
- Created base document
- Surveyed current statement architecture
- Identified 4 different implementations (2 shared, 2 custom)
- Status: Ready for pain point analysis

**Track C:**
- Created base document
- Identified missing date-filtered balance endpoints
- Identified profit tracking gap
- Status: Ready for deep backend/frontend scan

---

## Next Steps

1. **Track A Session:** Complete API endpoint mapping; categorize gaps; propose actions
2. **Track B Session:** Document current architecture; design unified solution; create implementation plan
3. **Track C Session:** Survey backend/frontend finance implementation; create phased roadmap; update FINANCE_MODULE_PLANNING.md
4. **Main Session:** Review progress; coordinate dependencies; update index

---

*Last Updated: January 24, 2026*
