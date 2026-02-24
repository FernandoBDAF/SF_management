# Transaction Bugs Fix Plan

> **Status:** In progress (Issues 1 and 3 implemented, post-implementation tuning applied)  
> **Created:** February 23, 2026  
> **Scope:** Three bugs identified during finance module validation  
> **Priority:** High (Issues 1 and 3), Medium (Issue 2 - deferred investigation)

---

## Table of Contents

- [Executive Summary](#executive-summary)
- [Issue 1: System Wallet Position Inverted](#issue-1-system-wallet-position-inverted)
- [Issue 2: Settlement Rake Commission Missing in Planilha](#issue-2-settlement-rake-commission-missing-in-planilha)
- [Issue 3: Rakeback Missing from Client/Member Statements](#issue-3-rakeback-missing-from-clientmember-statements)
- [Implementation Order](#implementation-order)
- [Testing Checklist](#testing-checklist)

---

## Executive Summary

During validation of the finance module (planilha) and settlement closing flows, three bugs were identified:

| #   | Issue                                                                  | Severity | Status                                |
| --- | ---------------------------------------------------------------------- | -------- | ------------------------------------- |
| 1   | System wallet position inverted in SALE/PURCHASE/RECEIPT/PAYMENT modes | High     | Implemented                           |
| 2   | Settlement rake commission shows zero in planilha balance              | Medium   | Deferred - needs deeper investigation |
| 3   | Rakeback not displayed in client/member statement transactions         | High     | Implemented + fine-tuned              |

---

## Issue 1: System Wallet Position Inverted

### Problem

When using "Operacao com Conta Sistema" in non-TRANSFER modes, the system wallet can be placed on the wrong side of the transaction depending on business perspective. After validation with real flows, SALE/PURCHASE needed a mode-specific adjustment for poker manager operations.

### Evidence (Screenshots)

| Mode                      | Current Behavior            | Expected Behavior           |
| ------------------------- | --------------------------- | --------------------------- |
| **Compra** (PURCHASE)     | System = Origem (sender)    | System = Destino (receiver) |
| **Venda** (SALE)          | System = Destino (receiver) | System = Origem (sender)    |
| **Recebimento** (RECEIPT) | System = Origem (sender)    | System = Destino (receiver) |
| **Pagamento** (PAYMENT)   | System = Destino (receiver) | System = Origem (sender)    |

### Business Logic

The system wallet represents the company. Its position should reflect the validated finance perspective per mode:

- **PURCHASE** (PM buys chips): company receives chips -> system = **receiver**
- **SALE** (PM sells chips): company sends chips -> system = **sender**
- **RECEIPT** (money into bank): Company receives money -> system = **receiver**
- **PAYMENT** (money from bank): Company sends money -> system = **sender**

### Root Cause

File: `SF_management-front/src/features/transactions/components/FormFields/SystemOperationCheck.tsx`

```typescript
// CURRENT (WRONG)
function getDefaultSystemPosition(
  mode: TransactionMode,
): "sender" | "receiver" {
  switch (mode) {
    case "SALE":
      return "sender"; // Should be: receiver
    case "PURCHASE":
      return "receiver"; // Should be: sender
    case "RECEIPT":
      return "sender"; // Should be: receiver
    case "PAYMENT":
      return "receiver"; // Should be: sender
    default:
      return "receiver";
  }
}
```

### Fix Plan

#### Step 1: Invert position mapping

File: `SystemOperationCheck.tsx`

```typescript
// FIXED
function getDefaultSystemPosition(
  mode: TransactionMode,
): "sender" | "receiver" {
  switch (mode) {
    case "SALE":
      return "sender";
    case "PURCHASE":
      return "receiver";
    case "RECEIPT":
      return "receiver";
    case "PAYMENT":
      return "sender";
    default:
      return "receiver";
  }
}
```

#### Step 2: Handle fixed-side conflict

When the system wallet lands on the same side as the "fixed" entity (creator), the creator must be auto-moved to the other side.

File: `AssetTransactionForm.tsx`

In `handleSystemOperationChange`, after setting the system wallet:

```typescript
// Auto-set creator on the non-system side when system takes the fixed side
if (position === "sender" && config.senderFixed) {
  form.setValue("receiverAssetHolderId", creatorAssetHolderId);
} else if (position === "receiver" && config.receiverFixed) {
  form.setValue("senderAssetHolderId", creatorAssetHolderId);
}
```

#### Step 3: Update ParticipantSelector fixed props

The non-system ParticipantSelector must show the creator as fixed when the system took the creator's original fixed side.

```typescript
// Compute effective fixed state
const senderFixedToCreator = shouldHideReceiver && config.receiverFixed;
const receiverFixedToCreator = shouldHideSender && config.senderFixed;

// Sender ParticipantSelector
isFixed={config.senderFixed && !shouldHideSender || senderFixedToCreator}

// Receiver ParticipantSelector
isFixed={config.receiverFixed && !shouldHideReceiver || receiverFixedToCreator}
```

### Files Modified

| File                                                                                           | Change                                                            |
| ---------------------------------------------------------------------------------------------- | ----------------------------------------------------------------- |
| `SF_management-front/src/features/transactions/components/FormFields/SystemOperationCheck.tsx` | Invert `getDefaultSystemPosition` mapping                         |
| `SF_management-front/src/features/transactions/components/AssetTransactionForm.tsx`            | Auto-set creator on non-system side; update effective fixed props |

### Impact Analysis

- All system operation transactions in SALE/PURCHASE/RECEIPT/PAYMENT modes will have corrected sender/receiver direction
- Balance calculations in planilha will reflect the correct sign for system wallet transactions
- No backend changes needed - the fix is purely frontend transaction creation logic
- Existing transactions already created with wrong direction cannot be auto-corrected (manual fix needed if important)

### Post-implementation regression and fix

After the direction fix, a UX regression appeared: toggling `Operacao com Conta Sistema` could fail with `Selecione primeiro` even when one wallet was already selected in the form.

Root cause:

- `handleSystemOperationChange` only attempted to resolve the system wallet from the **opposite side** wallet.
- In `RECEIPT/PAYMENT`, that side is often empty at toggle time (fixed-side wallet is selected first), so the toggle was immediately reverted.
- The auto-selection path used mode-based wallet lists and could miss valid wallets for the selected reference entity.

Fix applied in `AssetTransactionForm.tsx`:

- Resolve candidate reference wallet from all known wallet sources (`bank`, `poker`, `client fiat`, `member fiat`) for the selected reference entity.
- Prefer same `assetType` when available.
- Fallback to the already selected wallet on the other side before blocking.
- Keep the safeguard toast only when no wallet can be inferred from either side.

### Latest adjustment after poker manager validation

Validated with finance outcome and persisted transactions:

- `Venda` + system wallet must keep system as `Origem` (sender).
- `Compra` + system wallet must keep system as `Destino` (receiver).
- Temporary inversion (`SALE -> receiver`, `PURCHASE -> sender`) caused incorrect finance interpretation for digital transactions.

Fix applied:

- Updated `getDefaultSystemPosition` for poker modes only:
  - `SALE -> sender`
  - `PURCHASE -> receiver`
- Kept bank modes unchanged:
  - `RECEIPT -> receiver`
  - `PAYMENT -> sender`

### Additional post-implementation regression and fix (Poker Manager system operations)

Symptom:

- In `SALE`/`PURCHASE` with `Operacao com Conta Sistema`, clicking `Confirmar` did not create the transaction when `Taxa de Conversao` was not filled.

Root cause:

- Digital modes still treated conversion fields as active in payload shaping (`balanceAs` + `conversionRate`) while system-operation flow should bypass conversion requirements.
- UI still rendered conversion-related controls in system-operation state for digital modes, creating a mismatch between expected and required inputs.

Fix applied in `AssetTransactionForm.tsx`:

- Hide conversion section (`CoinBalanceCheck` + `BrlValueDisplay`) whenever `isSystemOperation` is enabled.
- In `buildTransferRequest`, force conversion bypass for system operations in conversion-enabled modes:
  - `conversionRate = undefined`
  - `balanceAs = undefined`
- Keep regular conversion behavior unchanged for non-system digital transactions.

---

## Issue 2: Settlement Rake Commission Missing in Planilha

> **Status:** Deferred - needs deeper investigation

### Problem

After creating a settlement closing, the rake commission does not appear in the poker manager's balance on the finance module (planilha). The closings page correctly shows the values (e.g., Rake Total = 400, Lucro = 80), but the planilha shows zero commission impact.

### What We Know

The planilha calls `GET /pokermanager/{id}/balance?asOfDate=...` which runs `GetBalancesByAssetGroup`. The settlement processing block (lines 740-762 of `BaseAssetHolderService.cs`) has correct formula:

```csharp
var balanceImpact = isPokerManager
    ? -(tx.RakeAmount * (tx.RakeCommission / 100m))
    : tx.RakeAmount * ((tx.RakeBack ?? 0m) / 100m);
```

### Suspected Causes

1. **Settlement wallet not in `walletIdentifierIds`**: The query only includes wallets owned by the poker manager. If the settlement wallet's `AssetPool.BaseAssetHolderId` doesn't match, the transaction is excluded from the query.

2. **`AssetPool.BaseAssetHolderId` is null**: Causes the `continue` at line 748-751 to skip the transaction entirely.

3. **`FirstOrDefault` returns `Guid.Empty`**: If no matching wallet ID is found, `walletIdentifiers.First(...)` at line 746 may throw or match incorrectly.

### Investigation Plan (for later)

1. Add temporary logging to `GetBalancesByAssetGroup` to trace settlement transaction processing
2. Verify settlement wallet ownership via database query
3. Check if `AssetPool` is properly loaded (eager loading)
4. Compare `walletIdentifierIds` with settlement transaction wallet IDs

### Files to Investigate

| File                                                                                | Area                           |
| ----------------------------------------------------------------------------------- | ------------------------------ |
| `SF_management/Application/Services/Base/BaseAssetHolderService.cs` (lines 740-762) | Settlement balance calculation |
| `SF_management/Application/Services/Base/BaseAssetHolderService.cs` (lines 595-600) | Wallet identifier query        |
| Settlement transaction creation flow                                                | Verify wallet ownership        |

---

## Issue 3: Rakeback Missing from Client/Member Statements

### Problem

Client and member statement (extrato) pages show settlement transactions but do not display the rakeback information. The balance total correctly includes rakeback, but individual transaction rows in the statement don't show the breakdown.

### Root Cause

#### Backend: DTO missing fields

File: `SF_management/Application/DTOs/Transactions/StatementTransactionResponse.cs`

The DTO has no fields for `RakeAmount`, `RakeCommission`, or `RakeBack`.

#### Backend: Fields not mapped

File: `SF_management/Application/Services/Base/BaseAssetHolderService.cs` (lines 871-891)

Settlement transactions are mapped to `StatementTransactionResponse` without rakeback fields:

```csharp
allTransactions.Add(new StatementTransactionResponse
{
    Id = st.Id,
    Date = st.Date,
    AssetAmount = st.GetSignedAmountForWalletIdentifier(relevantWalletId),
    // ... standard fields ...
    // MISSING: RakeAmount, RakeCommission, RakeBack
});
```

#### Frontend: Types and UI missing fields

- `SimplifiedTransaction` type has no rakeback fields
- `TransactionTable` component does not render rakeback

### Fix Plan

#### Step 1: Add fields to backend DTO

File: `StatementTransactionResponse.cs`

```csharp
public decimal? RakeAmount { get; set; }
public decimal? RakeCommission { get; set; }
public decimal? RakeBack { get; set; }
public decimal? RakeBackAmount { get; set; }
```

#### Step 2: Map fields in service

File: `BaseAssetHolderService.cs` (settlement transaction mapping)

```csharp
allTransactions.Add(new StatementTransactionResponse
{
    // ... existing fields ...
    RakeAmount = st.RakeAmount,
    RakeCommission = st.RakeCommission,
    RakeBack = st.RakeBack,
    RakeBackAmount = st.RakeAmount * ((st.RakeBack ?? 0m) / 100m),
});
```

#### Step 3: Add fields to frontend type

File: `SF_management-front/src/shared/types/domain/transaction.types.ts`

```typescript
export interface SimplifiedTransaction {
  // ... existing fields ...
  rakeAmount?: number;
  rakeCommission?: number;
  rakeBack?: number;
  rakeBackAmount?: number;
}
```

#### Step 4: Display and use correct settlement statement value

Files:

- `SF_management-front/src/shared/components/data-display/TransactionTable/utils.ts`
- `SF_management-front/src/shared/components/data-display/AssetHolderStatementView/index.tsx`
- `SF_management-front/src/shared/components/data-display/TransactionTable/DesktopTable.tsx`
- `SF_management-front/src/shared/components/data-display/TransactionTable/TabletTable.tsx`
- `SF_management-front/src/shared/components/data-display/TransactionTable/MobileCards.tsx`

Fine-tuning rules applied for settlement rows in client/member statements:

1. Use **rakeback amount** (`RakeBackAmount`) as the statement **Valor**, not `AssetAmount`.
2. Compute settlement value sign from `AssetAmount` sign and apply it to `RakeBackAmount`.
3. In **Origem**, replace `Venda`/`Compra` with `Fechamento` and append sign (`-` or `+`).
4. Keep settlement details explicit in description area (`Rakeback Fechamento: R$ X.XX`).

Example:

- `RakeAmount = 400`, `RakeBack = 50%` -> `RakeBackAmount = 200`
- if `AssetAmount` is negative in statement context -> `Valor = -200` and `Origem = Fechamento -`

### Files Modified

| File                                                                                        | Change                                                                                       |
| ------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------- |
| `SF_management/Application/DTOs/Transactions/StatementTransactionResponse.cs`               | Add `RakeAmount`, `RakeCommission`, `RakeBack`, `RakeBackAmount`                             |
| `SF_management/Application/Services/Base/BaseAssetHolderService.cs`                         | Map settlement fields and compute `RakeBackAmount`                                           |
| `SF_management-front/src/shared/types/domain/transaction.types.ts`                          | Add optional settlement fields to `SimplifiedTransaction`                                    |
| `SF_management-front/src/shared/components/data-display/TransactionTable/utils.ts`          | Add settlement helpers (`isSettlementTransaction`, `getRakeBackAmount`, `getStatementValue`) |
| `SF_management-front/src/shared/components/data-display/AssetHolderStatementView/index.tsx` | Use settlement statement value in filters/sorting                                            |
| `SF_management-front/src/shared/components/data-display/TransactionTable/DesktopTable.tsx`  | Use settlement value in Valor; label Origem as `Fechamento +/-`                              |
| `SF_management-front/src/shared/components/data-display/TransactionTable/TabletTable.tsx`   | Same settlement behavior as desktop                                                          |
| `SF_management-front/src/shared/components/data-display/TransactionTable/MobileCards.tsx`   | Same settlement behavior as desktop                                                          |

### Impact Analysis

- Backend: New nullable fields in DTO - backward compatible (no breaking change)
- Frontend: Optional fields - existing code unaffected
- Only settlement transactions have settlement-specific fields populated
- Client/member statement Valor now reflects rakeback amount impact (instead of settlement asset amount)
- Settlement Origem semantics are clearer (`Fechamento +/-` instead of `Venda`/`Compra`)

---

## Implementation Order

```
Issue 1: System Wallet Position (Frontend only)
├── 1. Fix getDefaultSystemPosition mapping
├── 2. Handle fixed-side conflict in AssetTransactionForm
├── 3. Update ParticipantSelector effective fixed props
└── 4. Test all four modes with system operation

Issue 3: Rakeback in Statements (Backend + Frontend)
├── 1. Add fields to StatementTransactionResponse DTO
├── 2. Map fields in GetTransactionsStatementForAssetHolder (+ compute RakeBackAmount)
├── 3. Add fields to SimplifiedTransaction frontend type
├── 4. Add settlement value helpers in TransactionTable utils
├── 5. Use settlement value in statement sort/filter
└── 6. Display settlement rows as "Fechamento +/-" using rakeback amount

Issue 2: Rake Commission in Planilha (Deferred)
└── Investigate and fix later
```

---

## Testing Checklist

### Issue 1

- [ ] SALE + system op: System is Origem (sender), PM is Destino (receiver)
- [ ] PURCHASE + system op: PM is Origem (sender), System is Destino (receiver)
- [ ] RECEIPT + system op: Entity is Origem (sender), System is Destino (receiver)
- [ ] PAYMENT + system op: System is Origem (sender), Entity is Destino (receiver)
- [ ] Creator entity auto-fills on non-system side
- [ ] Planilha balance reflects correct sign after transactions

### Issue 3

- [ ] Client statement settlement rows show `Valor = RakeBackAmount` (not `AssetAmount`)
- [ ] Member statement settlement rows show `Valor = RakeBackAmount` (not `AssetAmount`)
- [ ] Settlement `Origem` shows `Fechamento` with explicit sign (`-`/`+`)
- [ ] Settlement detail line shows computed rakeback amount (`RakeAmount * RakeBack%`)
- [ ] Non-settlement transactions keep existing behavior
- [ ] Balance totals remain unchanged (rakeback was already included in balances)

---

_Created: February 23, 2026_
