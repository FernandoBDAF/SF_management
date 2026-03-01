# SF Management Backend - Development Plans

This folder contains development plans, implementation roadmaps, and technical documentation for features being developed or planned.

> **Last Updated:** March 1, 2026

---

## Active Plans

| Document | Status | Description |
|----------|--------|-------------|
| [PRE_PRODUCTION_IMPLEMENTATION_PLAN.md](./PRE_PRODUCTION_IMPLEMENTATION_PLAN.md) | 🔄 **Active** | Unified pre-production plan coordinating both projects |

---

## Planning

Plans that have been drafted but not yet started:

| Document | Status | Description |
|----------|--------|-------------|
| [TESTING_STRATEGY_PLAN.md](./TESTING_STRATEGY_PLAN.md) | 📋 **Planning** | Comprehensive testing strategy |
| [CLEAN_ARCHITECTURE_IMPROVEMENT_PLAN.md](./CLEAN_ARCHITECTURE_IMPROVEMENT_PLAN.md) | 📋 **Planning** | DDD/Clean Architecture improvements |
| [CONTRACTS_IMPLEMENTATION_PLAN.md](./CONTRACTS_IMPLEMENTATION_PLAN.md) | 📋 **Planning** | Recurring payments feature |

---

## Deferred

Plans postponed for future consideration:

| Document | Status | Description |
|----------|--------|-------------|
| *(No deferred plans currently in root)* | — | — |

---

## Archive

Completed or relocated plans are kept in the `archive/` subfolder for reference:

| Document | Status | Description |
|----------|--------|-------------|
| [FINANCE_MODULE_UPGRADE_PLAN.md](./archive/FINANCE_MODULE_UPGRADE_PLAN.md) | ✅ **Implemented** | Finance module upgrade: report refactoring, profit details, balance fixes |
| [RATE_BALANCE_FIX_PLAN.md](./archive/RATE_BALANCE_FIX_PLAN.md) | ✅ **Implemented** | Rate adjustment fix in Client/Member balance |
| [AVGRATE_CALCULATION_FIX_PLAN.md](./archive/AVGRATE_CALCULATION_FIX_PLAN.md) | ✅ **Implemented** | AvgRate calculation fix with transaction ordering |
| [ASSET_POOL_COMPANY_OWNERSHIP_ANALYSIS.md](./archive/ASSET_POOL_COMPANY_OWNERSHIP_ANALYSIS.md) | 📄 **Moved** | Analysis of company-owned asset pools |
| [FINANCE_MODULE_VISION.md](./archive/FINANCE_MODULE_VISION.md) | ✅ **Implemented** | Finance Module vision — superseded by implemented upgrade |
| [TRANSACTION_BUGS_FIX_PLAN.md](./archive/TRANSACTION_BUGS_FIX_PLAN.md) | ✅ **Implemented** | Transaction bugs — Issues 1,3,4 fixed; Issue 2 tracked as known issue |
| [DOCUMENTATION_IMPROVEMENT_STUDY.md](./archive/DOCUMENTATION_IMPROVEMENT_STUDY.md) | ✅ **Implemented** | Documentation audit and improvement recommendations |
| [ENUM_STANDARDIZATION_PLAN.md](./archive/ENUM_STANDARDIZATION_PLAN.md) | ✅ **Implemented** | Enum standardization with `None = 0` and ManagerProfitType migration script |
| [ASSETGROUP_FLEXIBLE_RENAME_PLAN.md](./archive/ASSETGROUP_FLEXIBLE_RENAME_PLAN.md) | ✅ **Implemented** | Naming convention improvements: `AssetGroup.Flexible`, endpoint/component renames, helper layer |
| [BALANCE_IMPROVEMENTS_PLAN.md](./archive/BALANCE_IMPROVEMENTS_PLAN.md) | ✅ **Implemented** | Critical balance improvements completed; remaining items deferred as post-production code quality |

> **Note:** Frontend coordination plans are documented independently in the frontend project.

---

## Related Documentation

| Document | Location | Purpose |
|----------|----------|---------|
| Documentation Index | [Documentation/00_DOCUMENTATION_INDEX.md](../Documentation/00_DOCUMENTATION_INDEX.md) | Main documentation entry point |
| Finance System | [Documentation/03_CORE_SYSTEMS/FINANCE_SYSTEM.md](../Documentation/03_CORE_SYSTEMS/FINANCE_SYSTEM.md) | Implemented finance system |
| Profit Calculation | [Documentation/03_CORE_SYSTEMS/PROFIT_CALCULATION_SYSTEM.md](../Documentation/03_CORE_SYSTEMS/PROFIT_CALCULATION_SYSTEM.md) | Profit calculation pipeline |
| Authentication | [Documentation/05_INFRASTRUCTURE/AUTHENTICATION.md](../Documentation/05_INFRASTRUCTURE/AUTHENTICATION.md) | Auth0 and RBAC documentation |
| Finance Deferred Features | [Documentation/07_REFERENCE/FINANCE_DEFERRED_FEATURES.md](../Documentation/07_REFERENCE/FINANCE_DEFERRED_FEATURES.md) | Future finance features |

---

## Status Legend

| Status | Meaning |
|--------|---------|
| ✅ Implemented | Fully implemented and deployed |
| 🔄 Active | Currently being worked on |
| 📋 Planning | Planned but not yet started |
| ⏸️ Deferred | Postponed for future consideration |
