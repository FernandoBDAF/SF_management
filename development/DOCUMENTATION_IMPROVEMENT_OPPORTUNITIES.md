# Documentation Improvement Opportunities

> **Created:** March 1, 2026  
> **Purpose:** Track documentation gaps and improvement needs across both projects  
> **Scope:** Backend (SF_management) and Frontend (SF_management-front)

---

## Overview

This document consolidates documentation improvement opportunities identified during the pre-production wrap-up review. Items are categorized by priority and project.

---

## Backend Documentation (SF_management/Documentation)

### High Priority

| Document | Issue | Recommended Action |
|----------|-------|-------------------|
| `03_CORE_SYSTEMS/TRANSACTION_BALANCE_IMPACT.md` | Contains "Known Issue" warning about SettlementTransaction bug that references a non-existent `10_REFACTORING/BALANCE_SYSTEM_ANALYSIS.md` | Either create the referenced analysis document or update the reference to point to existing documentation |
| `06_API/BALANCE_ENDPOINTS.md` | Contains "CRITICAL BUG (Known Issue)" warning that may be outdated | Verify if issue is still valid; update or remove warning accordingly |

### Medium Priority

| Document | Issue | Recommended Action |
|----------|-------|-------------------|
| `06_API/BALANCE_ENDPOINTS.md` | Inconsistent dates - header says "January 24, 2026", footer says "February 27, 2026" | Align dates to reflect actual last update |
| `07_REFERENCE/FINANCE_DEFERRED_FEATURES.md` | "Deferred Feature 5: ManagerProfitType Refactoring" - enum standardization portion is now complete | Add note clarifying that `None = 0` standardization is implemented; only the rename to `CompanyRevenueSource` remains deferred |

### Low Priority

| Document | Issue | Recommended Action |
|----------|-------|-------------------|
| Multiple documents | Many documents lack explicit "Last Updated" dates | Consider adding consistent date tracking to all documents |
| `05_INFRASTRUCTURE/DATETIME_STANDARDS.md` | Last updated January 29, 2026 - older than most recent changes | Review for accuracy with current implementation |

---

## Frontend Documentation (SF_management-front/documentation)

### High Priority

| Document | Issue | Recommended Action |
|----------|-------|-------------------|
| `07_REFERENCE/TRANSACTION_FEATURE_GUIDE.md` | Last updated January 23, 2026 - missing recent CONVERSION mode and statement display changes | Update to document: (1) CONVERSION mode entity restriction to `creatorAssetHolderId` only, (2) Internal/conversion transaction statement display format |

### Medium Priority

| Document | Issue | Recommended Action |
|----------|-------|-------------------|
| `04_COMPONENTS/DATA_DISPLAY.md` | TransactionTable helpers (`isTransferTransaction`, `isInternalTransaction`, `getOriginRateDisplay`, etc.) not fully documented | Add section documenting new utility functions in `TransactionTable/utils.ts` |
| `03_CORE_SYSTEMS/TRANSACTION_SYSTEM.md` | Footer shows "March 2026" while header shows "February 27, 2026" | Align dates |

### Low Priority - January 2026 Documents

The following documents were last updated in January 2026 and may need review for alignment with route reorganization and RBAC implementation:

| Document | Potential Issue |
|----------|----------------|
| `02_ARCHITECTURE/CLEAN_ARCHITECTURE.md` | May reference old route structure |
| `02_ARCHITECTURE/API_INTEGRATION.md` | May need updated examples |
| `02_ARCHITECTURE/STATE_MANAGEMENT.md` | Review for current patterns |
| `04_COMPONENTS/FORM_SYSTEM.md` | Review for transaction form updates |
| `05_INFRASTRUCTURE/ENVIRONMENT_CONFIG.md` | Verify env vars are current |
| `05_INFRASTRUCTURE/API_PROXY.md` | Review for accuracy |
| `05_INFRASTRUCTURE/LOGGING_STRATEGY.md` | Review for current implementation |
| `06_DEVELOPMENT/ARCHITECTURAL_GUARDRAILS.md` | Verify rules match current ESLint config |

---

## Cross-Project Observations

### Documentation Independence

Both projects' documentation should be independent since developers may only have access to one project. Current state is generally good, but some areas reference the other project without sufficient context:

1. **Backend** `TRANSACTION_INFRASTRUCTURE.md` references frontend files (`SystemOperationCheck.tsx`) - acceptable for context but ensure backend logic is self-documented
2. **Frontend** `TRANSACTION_SYSTEM.md` references backend services - acceptable for understanding flow

### Naming Consistency

`AssetGroup.Flexible` rename work (from `Internal`) is complete and archived. Remaining naming consistency work is limited to normal documentation maintenance as new features are implemented.

---

## Recommended Next Steps

### Immediate (Next Documentation Sprint)

1. Update `TRANSACTION_FEATURE_GUIDE.md` with CONVERSION mode changes
2. Resolve broken reference in `TRANSACTION_BALANCE_IMPACT.md`
3. Verify and update `BALANCE_ENDPOINTS.md` known issue status

### Short-Term

1. Add TransactionTable utility function documentation to `DATA_DISPLAY.md`
2. Align all header/footer dates in documents
3. Review January 2026 frontend docs for route/RBAC alignment

### Long-Term

1. Establish consistent "Last Updated" format across all documents
2. Create automated documentation freshness checks

---

## Status Legend

| Priority | Meaning |
|----------|---------|
| **High** | Documentation is incorrect or references missing content |
| **Medium** | Documentation is accurate but incomplete or has minor issues |
| **Low** | Documentation could be improved but is not blocking |

---

*This document should be updated as improvements are made and new opportunities are identified.*
