# AssetGroup Flexible Rename Plan

> **Status:** Deferred - Future Sprint  
> **Created:** January 24, 2026  
> **Priority:** Medium - Improves code clarity but not blocking features  
> **Estimated Effort:** 14-16 hours  
> **Prerequisites:** Self-conversion implementation (Sprint 1) ✅ Completed

---

## Executive Summary

This plan captures the deferred work from the `AssetGroup.Internal` analysis. Sprint 1 implemented the critical business features (self-conversion, query filtering, owner-based separation). This plan documents the remaining naming improvements and behavior layer additions that were deferred due to their breaking-change nature.

---

## Background

### What Was Implemented (Sprint 1)

| Task | Status |
|------|--------|
| Rename `GetInternalWalletToPairWith` → `GetSystemWalletToPairWith` | ✅ Done |
| Add owner filter (`BaseAssetHolderId == null`) | ✅ Done |
| Add `OrderBy` for deterministic results | ✅ Done |
| Create `GetConversionWalletsForManager` method | ✅ Done |
| Add `/conversion-wallets` endpoint | ✅ Done |
| Implement dual-balance logic in `GetBalancesByAssetGroup` | ✅ Done |
| Ensure `AssetPool` navigation in all queries | ✅ Done |
| Update frontend service to use renamed endpoint | ✅ Done |
| Update documentation | ✅ Done |

### What Was Deferred

| Task | Reason for Deferral |
|------|---------------------|
| Rename `AssetGroup.Internal` enum → `Flexible` | Breaking change, 46+ files |
| Rename API endpoint `/internal-wallet` → `/flexible-wallet` | Breaking change |
| Rename `InternalWalletsCheck.tsx` → `FlexibleWalletsCheck.tsx` | Low priority |
| Add behavior layer helpers | Enhancement, not required |
| Add validation per behavior type | Enhancement, not required |

---

## The Naming Problem

### Current Confusing Overlap

| Term | Location | Actual Meaning |
|------|----------|----------------|
| `AssetGroup.Internal` | Enum value 4 | Flexible wallet type (any AssetType) |
| `INTERNAL` mode | Frontend transaction mode | Same-holder transfer |
| `IsInternalTransfer` | Entity property | Both wallets have same owner |

These three concepts are **unrelated** but use similar naming, causing developer confusion.

### Proposed Solution

Rename `AssetGroup.Internal` → `AssetGroup.Flexible` to better describe its technical behavior: wallets that can hold any `AssetType` and bypass metadata validation.

---

## Implementation Plan

### Phase 1: Backend Enum Rename (~4 hours)

**Files to Modify:**

| File | Change |
|------|--------|
| `Domain/Enums/Assets/AssetGroup.cs` | Rename `Internal = 4` → `Flexible = 4` |
| `Domain/Entities/Assets/WalletIdentifier.cs` | Update references |
| `Application/Services/Validation/WalletIdentifierValidationService.cs` | Update references |
| `Application/Services/Assets/WalletIdentifierService.cs` | Update references |
| `Application/Services/Base/BaseAssetHolderService.cs` | Update references |
| `Application/Mappings/AutoMapperProfile.cs` | Update display name |
| `Api/Controllers/v1/Assets/WalletIdentifierController.cs` | Update endpoint route |
| `Api/Controllers/v1/Assets/CompanyAssetPoolController.cs` | Update references |

**Endpoint Changes (Breaking):**

```
# Before
POST /api/v1/WalletIdentifier/internal-wallet

# After
POST /api/v1/WalletIdentifier/flexible-wallet
```

**Note:** The database stores numeric values (4), so no migration is needed.

---

### Phase 2: Frontend Enum Rename (~4 hours)

**Files to Modify:**

| File | Change |
|------|--------|
| `src/shared/types/enums/asset-group.enum.ts` | Rename `Internal: 4` → `Flexible: 4` |
| `src/features/wallets/api/wallet.service.ts` | Update endpoint URL |
| `src/features/wallets/api/wallet.queries.ts` | Update references |
| `src/features/transactions/types/asset-transaction.types.ts` | Update references |
| `src/features/transactions/components/AssetTransactionForm.tsx` | Update references |
| `src/features/transactions/components/FormFields/InternalWalletsCheck.tsx` | **Rename file** |
| `src/features/dashboard/api/dashboard.actions.ts` | Update references |

**Component Rename:**

```
# Before
src/features/transactions/components/FormFields/InternalWalletsCheck.tsx

# After
src/features/transactions/components/FormFields/FlexibleWalletsCheck.tsx
```

---

### Phase 3: Behavior Layer (Optional, ~4 hours)

Add helper classes to detect wallet behavior based on ownership:

**Backend Helper:**

```csharp
// Application/Helpers/FlexibleWalletTypes.cs
public static class FlexibleWalletTypes
{
    /// <summary>
    /// Company-owned wallet for financial categorization (System Operations)
    /// </summary>
    public static bool IsSystemWallet(WalletIdentifier wallet) =>
        wallet.AssetPool?.AssetGroup == AssetGroup.Flexible &&
        wallet.AssetPool?.BaseAssetHolderId == null;

    /// <summary>
    /// PokerManager-owned wallet for self-conversion (dual-balance trigger)
    /// </summary>
    public static bool IsConversionWallet(WalletIdentifier wallet) =>
        wallet.AssetPool?.AssetGroup == AssetGroup.Flexible &&
        wallet.AssetPool?.BaseAssetHolderId != null &&
        IsPokerManager(wallet.AssetPool.BaseAssetHolderId.Value);

    /// <summary>
    /// Entity-owned flexible wallet (general purpose)
    /// </summary>
    public static bool IsFlexibleWallet(WalletIdentifier wallet) =>
        wallet.AssetPool?.AssetGroup == AssetGroup.Flexible;
}
```

**Frontend Constants:**

```typescript
// src/shared/types/wallet-behavior.types.ts
export const FlexibleWalletBehavior = {
  System: 'system',        // Company-owned for financial ops
  Conversion: 'conversion', // PokerManager-owned for dual-balance
  General: 'general',       // Other entity-owned
} as const;

export type FlexibleWalletBehavior = typeof FlexibleWalletBehavior[keyof typeof FlexibleWalletBehavior];

export const getWalletBehavior = (wallet: WalletIdentifier): FlexibleWalletBehavior | null => {
  if (wallet.assetGroup !== AssetGroup.Flexible) return null;
  if (!wallet.baseAssetHolderId) return FlexibleWalletBehavior.System;
  if (wallet.ownerType === 'PokerManager') return FlexibleWalletBehavior.Conversion;
  return FlexibleWalletBehavior.General;
};
```

---

### Phase 4: Documentation Updates (~2 hours)

**Files to Update:**

| File | Update |
|------|--------|
| `ENUMS_AND_TYPE_SYSTEM.md` | Rename Internal → Flexible |
| `ASSET_INFRASTRUCTURE.md` | Update references |
| `INTERNAL_WALLET_TYPE_IMPLEMENTATION.md` | **Rename file** to `FLEXIBLE_WALLET_IMPLEMENTATION.md` |
| `BALANCE_ENDPOINTS.md` | Update references |
| `API_REFERENCE.md` | Update endpoint |
| `00_DOCUMENTATION_INDEX.md` | Update file reference |

---

## Validation Improvements (Optional Enhancement)

Currently, Internal/Flexible wallets have no validation. Consider adding behavior-specific validation:

```csharp
private void ValidateFlexibleWalletSpecific(WalletIdentifier wallet, ValidationResult result)
{
    if (FlexibleWalletTypes.IsSystemWallet(wallet))
    {
        // System wallet: could require Purpose metadata for audit trail
        if (!wallet.HasMetadata("Purpose"))
        {
            result.AddWarning("System wallets should have a Purpose defined");
        }
    }
    else if (FlexibleWalletTypes.IsConversionWallet(wallet))
    {
        // Conversion wallet: validate owner is PokerManager
        // Already enforced by GetConversionWalletsForManager
    }
    else
    {
        // General flexible: may want to restrict creation
        // Consider: only allow for certain entity types
    }
}
```

---

## Migration Considerations

### API Versioning Options

| Option | Approach | Pros | Cons |
|--------|----------|------|------|
| **A: Direct Rename** | Change endpoints, update frontend | Simple | Breaking change |
| **B: Dual Endpoints** | Keep old + add new, deprecate old | Non-breaking | More code |
| **C: API v2** | Version bump, keep v1 | Clean separation | More infrastructure |

**Recommendation:** Option A (Direct Rename) since the frontend is controlled and can be updated simultaneously.

### Rollout Steps

1. Create feature branch
2. Make all backend changes
3. Make all frontend changes
4. Test thoroughly
5. Deploy backend first (brief downtime for endpoint)
6. Deploy frontend immediately after
7. Verify functionality
8. Clean up deprecated code references

---

## Acceptance Criteria

**Phase 1 (Backend):**
- [ ] `AssetGroup.Internal` renamed to `AssetGroup.Flexible` in enum
- [ ] All backend references updated
- [ ] Endpoint `/internal-wallet` renamed to `/flexible-wallet`
- [ ] Unit tests pass

**Phase 2 (Frontend):**
- [ ] `AssetGroup.Internal` renamed to `AssetGroup.Flexible` in enum
- [ ] All frontend references updated
- [ ] `InternalWalletsCheck.tsx` renamed to `FlexibleWalletsCheck.tsx`
- [ ] E2E tests pass

**Phase 3 (Optional - Behavior Layer):**
- [ ] `FlexibleWalletTypes` helper class created
- [ ] `FlexibleWalletBehavior` constants created
- [ ] Helper functions work correctly in tests

**Phase 4 (Documentation):**
- [ ] All documentation files updated
- [ ] `INTERNAL_WALLET_TYPE_IMPLEMENTATION.md` renamed

---

## Risk Mitigation

| Risk | Mitigation |
|------|------------|
| Breaking API consumers | Coordinate frontend deployment with backend |
| Missing references | Use IDE "Find All References" before changes |
| Database issues | None expected - enum values unchanged |
| Test failures | Update test assertions for new names |

---

## Related Documentation

| Document | Relevance |
|----------|-----------|
| [ASSET_INFRASTRUCTURE.md](../03_CORE_SYSTEMS/ASSET_INFRASTRUCTURE.md) | Internal wallet creation use cases |
| [BALANCE_ENDPOINTS.md](../06_API/BALANCE_ENDPOINTS.md) | Self-conversion dual-balance logic |
| [TRANSACTION_INFRASTRUCTURE.md](../03_CORE_SYSTEMS/TRANSACTION_INFRASTRUCTURE.md) | Self-conversion trigger conditions |
| [ENUMS_AND_TYPE_SYSTEM.md](../07_REFERENCE/ENUMS_AND_TYPE_SYSTEM.md) | Naming clarification note |
| [INTERNAL_WALLET_TYPE_IMPLEMENTATION.md](../06_API/INTERNAL_WALLET_TYPE_IMPLEMENTATION.md) | Current implementation guide |

---

## Decision Record

| Date | Decision | Rationale |
|------|----------|-----------|
| Jan 23, 2026 | Defer rename to future sprint | Self-conversion was priority; rename is 14+ hours and breaking |
| Jan 24, 2026 | Created this plan | Preserve deferred work before removing analysis docs |

---

*Last updated: January 24, 2026*
