# Enum Standardization Plan

> **Status:** Implemented  
> **Created:** February 2026  
> **Implemented:** February 2026  

---

## Overview

This plan standardizes all domain enums to follow the `None = 0` pattern, ensuring consistency across the codebase and preventing issues with default enum values.

## Current State Analysis

### Enums WITH `None/Unknown = 0` (Already Compliant)

| Enum | Zero Value | Current Range |
|------|------------|---------------|
| `AssetGroup` | `None = 0` | 0-5 |
| `AssetType` | `None = 0` | 0, 21-22, 101-108, 201-206 |
| `AssetHolderType` | `Unknown = 0` | 0-4 |

### Enums WITHOUT `None = 0` (Need Standardization)

| Enum | Current Range | DB Tables Affected | Data Migration Required |
|------|---------------|-------------------|------------------------|
| **`ManagerProfitType`** | 0-1 | `PokerManagers` | **YES** |
| `AccountClassification` | 1-5 | `WalletIdentifiers` | **YES** |
| `TaxEntityType` | 1-3 | `BaseAssetHolders` | **YES** |
| `ReconciledTransactionType` | 1-3 | `ImportedTransactions` | **YES** |
| `ImportedTransactionStatus` | 1-8 | `ImportedTransactions` | **YES** |
| `ImportFileType` | 1-8 | `ImportedFiles` | **YES** |
| `ExcelImportType` | 1-3 | None (used in code logic) | No |

---

## Phase 1: ManagerProfitType Migration (Priority)

### 1.1 Current Definition

```csharp
public enum ManagerProfitType
{
    Spread = 0,
    RakeOverrideCommission = 1
}
```

### 1.2 New Definition

```csharp
public enum ManagerProfitType
{
    None = 0,
    Spread = 1,
    RakeOverrideCommission = 2
}
```

### 1.3 Database Impact

**Table:** `PokerManagers`
**Column:** `ManagerProfitType` (int, nullable)

**Current Data:**
- `0` = Spread
- `1` = RakeOverrideCommission
- `NULL` = Not set

**After Migration:**
- `0` = None (should not exist if column is properly managed)
- `1` = Spread
- `2` = RakeOverrideCommission
- `NULL` = Not set

### 1.4 SQL Migration Script

```sql
-- ManagerProfitType Migration: Shift values +1
-- Current: Spread=0, RakeOverrideCommission=1
-- New: None=0, Spread=1, RakeOverrideCommission=2

BEGIN TRANSACTION;

-- Verify current state
SELECT 
    ManagerProfitType,
    COUNT(*) as Count
FROM PokerManagers
WHERE DeletedAt IS NULL
GROUP BY ManagerProfitType;

-- Update existing values (shift +1)
UPDATE PokerManagers
SET ManagerProfitType = ManagerProfitType + 1
WHERE ManagerProfitType IS NOT NULL;

-- Verify migration
SELECT 
    ManagerProfitType,
    COUNT(*) as Count
FROM PokerManagers
WHERE DeletedAt IS NULL
GROUP BY ManagerProfitType;

COMMIT;
-- ROLLBACK; -- Use if verification fails
```

### 1.5 Backend Code Changes

**Files to update:**

1. **`Domain/Enums/ManagerProfitType.cs`** - Add `None = 0`, shift others
2. **`Application/Services/Finance/ProfitCalculationService.cs`** - Uses `ManagerProfitType.Spread` and `ManagerProfitType.RakeOverrideCommission` (no changes needed, references enum names)
3. **`Application/Services/Base/BaseAssetHolderService.cs`** - Uses `ManagerProfitType.RakeOverrideCommission` (no changes needed)
4. **`Application/Services/Finance/AvgRateService.cs`** - Uses `ManagerProfitType.Spread` (no changes needed)

### 1.6 Frontend Code Changes

**Files to update:**

1. **`src/shared/types/enums/manager-profit-type.enum.ts`**
   ```typescript
   export const ManagerProfitType = {
     None: 0,
     Spread: 1,
     RakeOverrideCommission: 2,
   } as const;
   ```

2. **`src/features/finance/hooks/usePlanilhaData.ts`** - Uses `ManagerProfitType.RakeOverrideCommission` (no changes needed, references enum names)

---

## Phase 2: Other Enums Standardization

### 2.1 AccountClassification

**Current:**
```csharp
public enum AccountClassification
{
    Asset = 1,
    Liability = 2,
    Equity = 3,
    Revenue = 4,
    Expense = 5
}
```

**New:**
```csharp
public enum AccountClassification
{
    None = 0,
    Asset = 1,
    Liability = 2,
    Equity = 3,
    Revenue = 4,
    Expense = 5
}
```

**SQL Migration:**
```sql
-- AccountClassification: No data migration needed
-- Just adding None = 0, existing values stay the same (1-5)
-- Verify no rows have AccountClassification = 0 (shouldn't exist)
SELECT COUNT(*) FROM WalletIdentifiers WHERE AccountClassification = 0;
```

**Impact:** Low - just adding `None = 0` without shifting existing values.

### 2.2 TaxEntityType

**Current:**
```csharp
public enum TaxEntityType
{
    CPF = 1,
    CNPJ = 2,
    CNPJ_Not_Taxable = 3
}
```

**New:**
```csharp
public enum TaxEntityType
{
    None = 0,
    CPF = 1,
    CNPJ = 2,
    CNPJ_Not_Taxable = 3
}
```

**SQL Migration:**
```sql
-- TaxEntityType: No data migration needed
-- Just adding None = 0, existing values stay the same (1-3)
-- Verify no rows have TaxEntityType = 0 (shouldn't exist)
SELECT COUNT(*) FROM BaseAssetHolders WHERE TaxEntityType = 0;
```

**Impact:** Low - just adding `None = 0` without shifting.

### 2.3 ImportedFiles Enums

These enums start at 1 and can have `None = 0` added without data migration:

- `ReconciledTransactionType` (1-3)
- `ImportedTransactionStatus` (1-8)
- `ImportFileType` (1-8)
- `ExcelImportType` (1-3)

**SQL Verification (run for each):**
```sql
-- Verify no ImportedTransactions have ReconciledTransactionType = 0
SELECT COUNT(*) FROM ImportedTransactions WHERE ReconciledTransactionType = 0;

-- Verify no ImportedTransactions have Status = 0
SELECT COUNT(*) FROM ImportedTransactions WHERE Status = 0;

-- Verify no ImportedFiles have FileType = 0
SELECT COUNT(*) FROM ImportedFiles WHERE FileType = 0;
```

---

## Implementation Order

### Sprint 1: ManagerProfitType (Breaking Change)

| Step | Task | Risk |
|------|------|------|
| 1 | Backup database | - |
| 2 | Run SQL migration to shift values +1 | **High** |
| 3 | Deploy backend with new enum values | **High** |
| 4 | Deploy frontend with new enum values | **High** |
| 5 | Verify application functionality | - |

**Important:** Steps 2-4 must happen in the same deployment window.

### Sprint 2: Other Enums (Non-Breaking)

| Step | Task | Risk |
|------|------|------|
| 1 | Run SQL verification queries | - |
| 2 | Update backend enum definitions | Low |
| 3 | Update frontend enum definitions (if applicable) | Low |
| 4 | Update documentation | - |

---

## Complete SQL Script

```sql
-- ============================================
-- ENUM STANDARDIZATION MIGRATION SCRIPT
-- ============================================
-- Run this script BEFORE deploying the new code
-- ============================================

-- 1. BACKUP CHECK
-- Ensure you have a recent database backup before proceeding

-- 2. PRE-MIGRATION VERIFICATION
PRINT 'Pre-Migration State:';

PRINT 'ManagerProfitType distribution:';
SELECT 
    ManagerProfitType,
    COUNT(*) as Count
FROM PokerManagers
WHERE DeletedAt IS NULL
GROUP BY ManagerProfitType;

PRINT 'Checking for invalid enum values (should all be 0):';
SELECT 'AccountClassification=0' as Check, COUNT(*) as Count FROM WalletIdentifiers WHERE AccountClassification = 0
UNION ALL
SELECT 'TaxEntityType=0', COUNT(*) FROM BaseAssetHolders WHERE TaxEntityType = 0;

-- 3. MANAGER PROFIT TYPE MIGRATION
BEGIN TRANSACTION;

PRINT 'Migrating ManagerProfitType...';

UPDATE PokerManagers
SET ManagerProfitType = ManagerProfitType + 1
WHERE ManagerProfitType IS NOT NULL;

PRINT 'ManagerProfitType migration complete.';

-- 4. POST-MIGRATION VERIFICATION
PRINT 'Post-Migration State:';
SELECT 
    ManagerProfitType,
    COUNT(*) as Count
FROM PokerManagers
WHERE DeletedAt IS NULL
GROUP BY ManagerProfitType;

-- COMMIT or ROLLBACK based on verification
-- COMMIT;
-- ROLLBACK;

-- ============================================
-- After successful migration, deploy new code:
-- - Backend enum changes
-- - Frontend enum changes
-- ============================================
```

---

## Rollback Script

If issues are found after migration:

```sql
-- ROLLBACK: Revert ManagerProfitType to original values
BEGIN TRANSACTION;

UPDATE PokerManagers
SET ManagerProfitType = ManagerProfitType - 1
WHERE ManagerProfitType IS NOT NULL AND ManagerProfitType > 0;

COMMIT;
```

**Note:** This rollback only works if no new managers were created with `ManagerProfitType = 1` (Spread) after the migration.

---

## Frontend Sync Requirement

The frontend enum at `src/shared/types/enums/manager-profit-type.enum.ts` must be updated simultaneously with the backend and database:

```typescript
// BEFORE
export const ManagerProfitType = {
  Spread: 0,
  RakeOverrideCommission: 1,
} as const;

// AFTER
export const ManagerProfitType = {
  None: 0,
  Spread: 1,
  RakeOverrideCommission: 2,
} as const;
```

The `getManagerProfitTypeDisplayName` function should also handle the `None` case:

```typescript
export function getManagerProfitTypeDisplayName(type: ManagerProfitType): string {
  switch (type) {
    case ManagerProfitType.None:
      return 'Não definido';
    case ManagerProfitType.Spread:
      return 'Spread';
    case ManagerProfitType.RakeOverrideCommission:
      return 'Rake Commission';
    default:
      return 'Desconhecido';
  }
}
```

---

## Documentation Updates Required

After implementation, update:

1. `SF_management/Documentation/07_REFERENCE/ENUMS_AND_TYPE_SYSTEM.md`
2. `SF_management-front/documentation/07_REFERENCE/TYPES_AND_ENUMS.md`
3. Any business logic documentation referencing enum values

---

## Risk Assessment

| Change | Risk Level | Mitigation |
|--------|------------|------------|
| `ManagerProfitType` shift | **High** | Coordinated deploy, backup, rollback script |
| `AccountClassification` add `None` | **Low** | Values unchanged, just adding 0 |
| `TaxEntityType` add `None` | **Low** | Values unchanged, just adding 0 |
| ImportedFiles enums | **Low** | Values unchanged, just adding 0 |

---

## Checklist

### Pre-Deployment
- [ ] Database backup created
- [ ] SQL migration script tested in staging
- [ ] Backend code changes ready
- [ ] Frontend code changes ready
- [ ] Deployment window scheduled

### Deployment
- [ ] Run SQL migration script
- [ ] Verify migration results
- [ ] Deploy backend
- [ ] Deploy frontend
- [ ] Run smoke tests

### Post-Deployment
- [x] Verify PokerManager CRUD operations
- [x] Verify financial reports using profit types
- [x] Verify settlement processing
- [x] Update documentation

---

## Implementation Summary

### Completed: February 2026

#### Backend Changes

1. **`Domain/Enums/ManagerProfitType.cs`**
   - Added `None = 0`
   - Shifted `Spread` to `1` and `RakeOverrideCommission` to `2`

2. **`Domain/Enums/Assets/AccountClassification.cs`**
   - Added `None = 0` (existing values 1-5 unchanged)

3. **`Domain/Enums/TaxEntityType.cs`**
   - Added `None = 0` (existing values 1-3 unchanged)

4. **`Domain/Enums/ImportedFiles/ReconciledTransactionType.cs`**
   - Added `None = 0` (existing values 1-3 unchanged)

5. **`Domain/Enums/ImportedFiles/ImportedTransactionStatus.cs`**
   - Added `None = 0` (existing values 1-8 unchanged)

6. **`Domain/Enums/ImportedFiles/ImportFileType.cs`**
   - Added `None = 0` (existing values 1-8 unchanged)

7. **`Domain/Enums/ImportedFiles/ExcelImportType.cs`**
   - Added `None = 0` (existing values 1-3 unchanged)

#### Frontend Changes

1. **`src/shared/types/enums/manager-profit-type.enum.ts`**
   - Added `None: 0`
   - Updated `Spread: 1` and `RakeOverrideCommission: 2`
   - Updated `getManagerProfitTypeDisplayName()` to handle `None` case

2. **`src/shared/types/enums/tax-entity-type.enum.ts`**
   - Added `None: 0` (existing values unchanged)

#### SQL Migration

Created `development/ENUM_STANDARDIZATION_MIGRATION.sql`:
- Pre-migration verification queries
- `ManagerProfitType` value shift (+1) for existing records
- Post-migration verification queries
- Rollback script included

#### Documentation Updates

- Updated `Documentation/07_REFERENCE/ENUMS_AND_TYPE_SYSTEM.md`
- Updated `SF_management-front/documentation/07_REFERENCE/TYPES_AND_ENUMS.md`

#### Deployment Notes

The `ManagerProfitType` migration required coordinated deployment:
1. Database backup
2. SQL migration to shift values +1
3. Backend code deployment
4. Frontend code deployment

All other enum changes (adding `None = 0` without shifting) were low-risk and deployed with normal releases.
