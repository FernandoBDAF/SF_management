# Frontend-Backend API Alignment Review

> **Status:** In Progress  
> **Created:** January 24, 2026  
> **Track:** A  
> **Purpose:** Document API mismatches between frontend services and backend endpoints

---

## Table of Contents

- [Executive Summary](#executive-summary)
- [Methodology](#methodology)
- [Critical Issues](#critical-issues)
- [Gap Analysis](#gap-analysis)
- [Backend Endpoints Not Used](#backend-endpoints-not-used)
- [Frontend Expected Endpoints Missing](#frontend-expected-endpoints-missing)
- [Recommended Actions](#recommended-actions)
- [Implementation Plan](#implementation-plan)

---

## Executive Summary

This document identifies mismatches between frontend API client services and backend endpoints. The goal is NOT to achieve 100% coverage, but to:

1. **Document gaps** for review and decision-making
2. **Fix critical mismatches** that cause failures
3. **Identify outdated endpoints** for potential removal
4. **Highlight missing endpoints** needed by frontend

**Key Findings:**
- 1 critical path mismatch (wallet identifiers)
- 3 backend endpoints not used by frontend (statistics, transfer GET)
- 1 frontend expectation not met (date-filtered balances)

---

## Methodology

### Approach

1. Map all frontend service methods to backend endpoints
2. Identify mismatches (path, method, payload)
3. Categorize gaps:
   - **Critical:** Causes failures or incorrect behavior
   - **Optional:** Nice-to-have, missing feature
   - **Outdated:** Backend endpoint likely no longer needed
   - **Intentional:** Documented decision to not implement

4. Propose actions for each gap

### Scope

**Frontend Services Reviewed:**
- `base-asset-holder.service.ts` (shared base service)
- `client.service.ts`
- `member.service.ts`
- `bank.service.ts`
- `poker-manager.service.ts`
- `transfer.service.ts`
- `wallet.service.ts`
- `category.service.ts`
- `balance.service.ts` (if exists)

**Backend Controllers Reviewed:**
- `BaseAssetHolderController.cs`
- `ClientController.cs`
- `MemberController.cs`
- `BankController.cs`
- `PokerManagerController.cs`
- `TransferController.cs`
- `WalletIdentifierController.cs`
- `CategoryController.cs`
- `CompanyAssetPoolController.cs`

---

## Critical Issues

### 1. Wallet Identifiers Path Mismatch

**Location:** `SF_management-front/src/shared/services/base/base-asset-holder.service.ts`

**Current (WRONG):**
```typescript
// lines 71, 96
async getWalletIdentifiers(id: string): Promise<WalletIdentifier[]> {
  return this.client.get<WalletIdentifier[]>(
    `${this.basePath}/walletidentifiers/${id}`  // ❌ No hyphen
  );
}
```

**Backend Expects:**
```csharp
[HttpGet("{id}/wallet-identifiers")]
public async Task<IActionResult> GetWalletIdentifiers(Guid id)
```

**Impact:** Base service method may fail. Individual services (client, member, etc.) override this method correctly with hyphenated path.

**Action:** ✅ Fix path to `/wallet-identifiers` in base service

---

## Gap Analysis

### Backend Endpoints NOT Used by Frontend

| Endpoint | Controller | Frontend Status | Recommendation |
|----------|-----------|-----------------|----------------|
| `GET /{id}/statistics` | BaseAssetHolderController | Not implemented | **Review:** Is this used? If not, mark deprecated or remove |
| `GET /{id}/client-statistics` | ClientController | Not implemented | **Review:** Is this used? If not, mark deprecated or remove |
| `GET /{id}/member-statistics` | MemberController | Not implemented | **Review:** Is this used? If not, mark deprecated or remove |
| `GET /transfer/{id}?entityType=fiat\|digital` | TransferController | Not implemented | **Optional:** Implement if viewing transfer details is needed |

---

### Frontend Expected Endpoints MISSING in Backend

| Frontend Expectation | Current Backend | Gap | Recommendation |
|---------------------|-----------------|-----|----------------|
| Date-filtered balance | `GET /{id}/balance` returns current balance only | No historical balance support | **Implement:** Add date parameter or new endpoint `/balance/at-date?date=` |
| Profit summary endpoint | Not implemented | No company profit API | **Track C:** Part of Finance Module |
| Member share distribution | Not implemented | No share calculation | **Track C:** Part of Finance Module |
| Client credit status | Not implemented | No credit limit tracking | **Track C:** Part of Finance Module |

---

## Recommended Actions

### Critical (Fix Now)

| Issue | Action | Priority | Effort |
|-------|--------|----------|--------|
| Wallet identifiers path mismatch | Fix frontend path to `/wallet-identifiers` | P0 | 5 min |

### High Priority (Review & Decide)

| Issue | Action | Priority | Effort |
|-------|--------|----------|--------|
| Statistics endpoints unused | Document usage or mark deprecated | P1 | 30 min |
| Date-filtered balance missing | Implement backend endpoint | P1 | 2-4 hours |

### Medium Priority (Optional Features)

| Issue | Action | Priority | Effort |
|-------|--------|----------|--------|
| Transfer GET endpoint | Implement frontend if needed | P2 | 1 hour |

### Low Priority (Future Work - Track C)

| Issue | Action | Priority | Effort |
|-------|--------|----------|--------|
| Profit/share/credit endpoints | Part of Finance Module | P3 | See Track C |

---

## Implementation Plan

### Phase 1: Critical Fixes

**Task 1.1:** Fix wallet identifiers path in base service
- File: `SF_management-front/src/shared/services/base/base-asset-holder.service.ts`
- Change: `/walletidentifiers/` → `/wallet-identifiers/`
- Lines: 71, 96
- Test: Verify wallet identifiers load for all asset holders

### Phase 2: Endpoint Review

**Task 2.1:** Verify statistics endpoints usage
- Search backend for calls to `statistics`, `client-statistics`, `member-statistics`
- Check if any report/dashboard uses them
- Decision: Remove, document, or implement frontend

**Task 2.2:** Date-filtered balance design
- Review `BALANCE_ENDPOINTS.md` for current implementation
- Design new endpoint or parameter for historical balance
- Coordinate with Track C (finance needs this)

### Phase 3: Optional Features

**Task 3.1:** Transfer GET endpoint (if needed)
- Verify if "view transfer details" feature is planned
- Implement `getTransfer(id, entityType)` in `transfer.service.ts`
- Add corresponding frontend page if needed

---

## Cross-References

### Backend Documentation
- [API_REFERENCE.md](../06_API/API_REFERENCE.md) - Complete API overview
- [TRANSACTION_API_ENDPOINTS.md](../06_API/TRANSACTION_API_ENDPOINTS.md) - Transaction endpoints
- [BALANCE_ENDPOINTS.md](../06_API/BALANCE_ENDPOINTS.md) - Balance calculation endpoints
- [COMPANY_ASSET_POOL_ENDPOINTS.md](../06_API/COMPANY_ASSET_POOL_ENDPOINTS.md) - Company wallet endpoints

### Frontend Documentation
- `SF_management-front/documentation/03_CORE_SYSTEMS/BALANCE_DISPLAY_USAGE.md` - Balance consumption
- `SF_management-front/documentation/03_CORE_SYSTEMS/TRANSACTION_SYSTEM.md` - Transaction system

### Related Tracks
- **Track B:** Statement pages use balance/transaction endpoints
- **Track C:** Finance module needs date-filtered balance endpoints

---

## Detailed Endpoint Mapping

### BaseAssetHolder Endpoints

| Method | Backend Route | Frontend Method | Status |
|--------|--------------|-----------------|--------|
| GET | `/{id}` | `getById(id)` | ✅ Aligned |
| GET | `/` | `getAll()` | ✅ Aligned |
| POST | `/` | `create(data)` | ✅ Aligned |
| PUT | `/{id}` | `update(id, data)` | ✅ Aligned |
| DELETE | `/{id}` | `delete(id)` | ✅ Aligned |
| GET | `/{id}/balance` | `getBalance(id)` | ✅ Aligned |
| GET | `/{id}/transactions` | `getTransactions(id)` | ✅ Aligned |
| GET | `/{id}/wallet-identifiers` | `getWalletIdentifiers(id)` | ⚠️ **Path mismatch in base** |
| GET | `/{id}/can-delete` | `canDelete(id)` | ✅ Aligned |
| GET | `/{id}/statistics` | Not implemented | ❓ **Missing frontend** |

### Transfer Endpoints

| Method | Backend Route | Frontend Method | Status |
|--------|--------------|-----------------|--------|
| POST | `/transfer` | `create(data)` | ✅ Aligned |
| GET | `/transfer/{id}?entityType=` | Not implemented | ❓ **Missing frontend** |

### Client-Specific Endpoints

| Method | Backend Route | Frontend Method | Status |
|--------|--------------|-----------------|--------|
| GET | `/{id}/client-statistics` | Not implemented | ❓ **Missing frontend** |

### Member-Specific Endpoints

| Method | Backend Route | Frontend Method | Status |
|--------|--------------|-----------------|--------|
| GET | `/{id}/member-statistics` | Not implemented | ❓ **Missing frontend** |

### PokerManager-Specific Endpoints

| Method | Backend Route | Frontend Method | Status |
|--------|--------------|-----------------|--------|
| GET | `/{id}/wallet-identifiers-connected` | `getConnectedWallets(id)` | ✅ Aligned |
| GET | `/{id}/conversion-wallets` | `getConversionWallets(id)` | ✅ Aligned |
| POST | `/{id}/settlement-by-date` | `createSettlementByDate(id, data)` | ✅ Aligned |

### Category Endpoints

| Method | Backend Route | Frontend Method | Status |
|--------|--------------|-----------------|--------|
| GET | `/category` | `getAll()` | ✅ Aligned |
| GET | `/category/{id}` | `getById(id)` | ✅ Aligned |
| POST | `/category` | `create(data)` | ✅ Aligned |
| PUT | `/category/{id}` | `update(id, data)` | ✅ Aligned |
| DELETE | `/category/{id}` | `delete(id)` | ✅ Aligned |

### Wallet Identifier Endpoints

| Method | Backend Route | Frontend Method | Status |
|--------|--------------|-----------------|--------|
| POST | `/` | `create(data)` | ✅ Aligned |
| GET | `/{id}` | `getById(id)` | ✅ Aligned |
| PUT | `/{id}` | `update(id, data)` | ✅ Aligned |
| DELETE | `/{id}` | `delete(id)` | ✅ Aligned |

### Company Asset Pool Endpoints

| Method | Backend Route | Frontend Method | Status |
|--------|--------------|-----------------|--------|
| GET | `/system-wallet-to-pair-with/{walletId}` | `getSystemWallet(walletId)` | ✅ Aligned |

---

## Action Items

### Immediate

- [ ] Fix wallet identifiers path in `base-asset-holder.service.ts`
- [ ] Test all asset holder wallet identifier fetching

### Review & Decide

- [ ] Investigate statistics endpoints usage (backend grep for calls)
- [ ] Decision: Remove, document, or implement frontend

### Coordinate with Track C

- [ ] Design date-filtered balance endpoint
- [ ] Implement once Track C defines requirements

---

## Testing Checklist

After implementing fixes:

- [ ] Client wallet identifiers load correctly
- [ ] Member wallet identifiers load correctly
- [ ] Bank wallet identifiers load correctly
- [ ] PokerManager wallet identifiers load correctly
- [ ] No 404 errors in browser console for wallet endpoints

---

*Last Updated: January 24, 2026*  
*Managed by: Main Session*  
*Status: Awaiting Track A session start*
