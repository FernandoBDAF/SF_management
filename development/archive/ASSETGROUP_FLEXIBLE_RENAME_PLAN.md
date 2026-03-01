# Naming Convention Improvements Plan

> **Status:** Pre-Production — Promoted from Deferred
> **Originally Created:** January 24, 2026
> **Revised:** February 28, 2026
> **Scope:** Both `SF_management` (backend) and `SF_management-front` (frontend)
> **Estimated Effort:** 16-20 hours (2-3 days)

---

## Overview

This plan addresses naming convention inconsistencies across both projects. It was originally scoped to the `AssetGroup.Internal → Flexible` rename, but has been expanded to cover all naming issues identified during a comprehensive audit. Pre-production is the ideal time for these breaking changes since there are no active consumers.

---

## Audit Results

The codebase follows consistent conventions overall (95%+). The following issues were identified:

| # | Issue | Severity | Files Affected |
|---|-------|----------|---------------|
| 1 | `AssetGroup.Internal` should be `Flexible` | Medium | 46+ (backend + frontend) |
| 2 | `AccountClassification` enum uses ALL_CAPS values | Medium | ~15 |
| 3 | 3 DTOs don't follow `*Response` naming pattern | High (consistency) | 3 + consumers |
| 4 | API endpoint `/internal-wallet` should be `/flexible-wallet` | Medium | 2 (backend + frontend) |
| 5 | `InternalWalletsCheck.tsx` component name | Low | 1 + imports |
| 6 | Doc file `INTERNAL_WALLET_TYPE_IMPLEMENTATION.md` | Low | 1 + index refs |

No issues found with: entity names, service names, controller routes, frontend enum alignment, property naming (`ManagerProfitType`, `BaseDomain`), or localization pattern (`PokerManager` in code / "Administradora" in UI).

---

## Phase 1: DTO Naming Fix (Est. 2 hours)

Fix 3 DTOs that don't follow the `*Response` naming convention.

| Current Name | New Name | File |
|-------------|----------|------|
| `DirectIncomeDetailsDto` | `DirectIncomeDetailsResponse` | `Application/Dtos/Finance/ProfitDtos.cs` |
| `AvgRateSnapshot` | `AvgRateSnapshotResponse` | `Application/Dtos/Finance/` or `Application/Services/Finance/AvgRateService.cs` |
| `CacheStatistics` | `CacheStatisticsResponse` | `Application/Services/Infrastructure/CacheMetricsService.cs` |

**Impact:** Rename class + update all references. No API-level breaking change since JSON serialization uses property names, not class names.

**Frontend impact:** Check if `AvgRateSnapshot` is referenced in frontend types. Update if so.

---

## Phase 2: AccountClassification Enum (Est. 2 hours)

Change enum values from ALL_CAPS to PascalCase to match all other enums.

```csharp
// Before
public enum AccountClassification
{
    ASSET = 1,
    LIABILITY = 2,
    EQUITY = 3,
    REVENUE = 4,
    EXPENSE = 5
}

// After
public enum AccountClassification
{
    Asset = 1,
    Liability = 2,
    Equity = 3,
    Revenue = 4,
    Expense = 5
}
```

**Impact:** ~15 files reference these values. Database stores numeric values, so no migration needed.

**Frontend impact:** Check if `AccountClassification` is used in frontend. Update enum file if so.

**Risk:** JSON serialization may return different string values (e.g., `"Asset"` instead of `"ASSET"`). Verify if frontend code compares against string representations.

---

## Phase 3: AssetGroup.Internal → Flexible (Est. 8 hours)

This is the original plan — rename the confusing `Internal` value to `Flexible`.

### The Naming Problem

| Term | Location | Actual Meaning |
|------|----------|----------------|
| `AssetGroup.Internal` | Enum value 4 | Flexible wallet type (any AssetType) |
| `INTERNAL` mode | Frontend transaction mode | Same-holder transfer |
| `IsInternalTransfer` | Entity property | Both wallets have same owner |

Three unrelated concepts sharing similar naming.

### Backend Changes

| File | Change |
|------|--------|
| `Domain/Enums/Assets/AssetGroup.cs` | Rename `Internal = 4` → `Flexible = 4` |
| All services referencing `AssetGroup.Internal` | Update to `AssetGroup.Flexible` |
| `WalletIdentifierController.cs` | Rename endpoint `/internal-wallet` → `/flexible-wallet` |
| `WalletIdentifierService.cs` | Update method references |
| `BaseAssetHolderService.cs` | Update references |
| `CompanyAssetPoolController.cs` | Update references |
| AutoMapper profile | Update display name |

**Database:** No migration needed — stores numeric value `4`.

### Frontend Changes

| File | Change |
|------|--------|
| `src/shared/types/enums/asset-group.enum.ts` | Rename `Internal: 4` → `Flexible: 4` |
| `src/features/wallets/api/wallet.service.ts` | Update endpoint URL |
| `src/features/transactions/types/asset-transaction.types.ts` | Update references |
| `src/features/transactions/components/AssetTransactionForm.tsx` | Update references |
| `src/features/transactions/components/FormFields/InternalWalletsCheck.tsx` | **Rename file** → `FlexibleWalletsCheck.tsx` |
| All other files referencing `AssetGroup.Internal` | Update |

### API Endpoint Change

```
POST /api/v1/WalletIdentifier/internal-wallet  →  POST /api/v1/WalletIdentifier/flexible-wallet
```

---

## Phase 4: Behavior Layer (Est. 3 hours)

Add helper classes to distinguish wallet behaviors based on ownership.

### Backend Helper

```csharp
// Application/Helpers/FlexibleWalletTypes.cs
public static class FlexibleWalletTypes
{
    public static bool IsSystemWallet(WalletIdentifier wallet) =>
        wallet.AssetPool?.AssetGroup == AssetGroup.Flexible &&
        wallet.AssetPool?.BaseAssetHolderId == null;

    public static bool IsConversionWallet(WalletIdentifier wallet) =>
        wallet.AssetPool?.AssetGroup == AssetGroup.Flexible &&
        wallet.AssetPool?.BaseAssetHolderId != null;

    public static bool IsFlexibleWallet(WalletIdentifier wallet) =>
        wallet.AssetPool?.AssetGroup == AssetGroup.Flexible;
}
```

### Frontend Constants

```typescript
// src/shared/types/wallet-behavior.types.ts
export const FlexibleWalletBehavior = {
  System: 'system',
  Conversion: 'conversion',
  General: 'general',
} as const;
```

---

## Phase 5: Documentation Updates (Est. 2 hours)

| File | Update |
|------|--------|
| `Documentation/07_REFERENCE/ENUMS_AND_TYPE_SYSTEM.md` | Rename Internal → Flexible |
| `Documentation/03_CORE_SYSTEMS/ASSET_INFRASTRUCTURE.md` | Update references |
| `Documentation/06_API/INTERNAL_WALLET_TYPE_IMPLEMENTATION.md` | **Rename file** → `FLEXIBLE_WALLET_IMPLEMENTATION.md` |
| `Documentation/06_API/BALANCE_ENDPOINTS.md` | Update references |
| `Documentation/06_API/API_REFERENCE.md` | Update endpoint |
| `Documentation/00_DOCUMENTATION_INDEX.md` | Update file reference |
| Frontend `documentation/07_REFERENCE/TYPES_AND_ENUMS.md` | Update references |

---

## Implementation Strategy

**Pre-production advantage:** No active API consumers, so breaking changes can be deployed with zero risk.

**Order:**
1. Phase 1 (DTO naming) — Independent, can be done first
2. Phase 2 (AccountClassification) — Independent, can be done in parallel with Phase 1
3. Phase 3 (AssetGroup rename) — Largest change, deploy backend + frontend together
4. Phase 4 (Behavior layer) — Depends on Phase 3
5. Phase 5 (Documentation) — Last, after all code changes

---

## Acceptance Criteria

- [ ] All DTOs follow `*Response` naming pattern
- [ ] `AccountClassification` enum values use PascalCase
- [ ] `AssetGroup.Internal` renamed to `AssetGroup.Flexible` (both projects)
- [ ] API endpoint `/internal-wallet` renamed to `/flexible-wallet`
- [ ] `InternalWalletsCheck.tsx` renamed to `FlexibleWalletsCheck.tsx`
- [ ] `FlexibleWalletTypes` helper class created
- [ ] All documentation updated
- [ ] `dotnet build` passes (backend)
- [ ] `yarn build` passes (frontend)
- [ ] No remaining references to `AssetGroup.Internal` in active code

---

## Risk Assessment

| Risk | Impact | Mitigation |
|------|--------|------------|
| Missing a reference during rename | Build failure | Use IDE "Find All References" and `grep` before deploying |
| JSON serialization change for AccountClassification | Frontend parsing error | Check if frontend compares against string enum values |
| Database values unchanged | None | Enum stores numeric values, not strings |
| Frontend/backend deploy timing | Brief API mismatch | Deploy simultaneously (pre-production, no users) |
