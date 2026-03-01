# Balance System Improvements Plan

> **Status:** Partially Implemented — Reviewed February 27, 2026
> **Originally Created:** January 24, 2026
> **Scope:** Backend balance API + frontend balance consumption

---

## Implementation Status Review

This plan was created on January 24, 2026. Since then, significant balance work was done as part of the Finance Module Upgrade (February 2026). Below is the current status of each proposed improvement.

### Issue 1: No Date-Filtered Balance Support — ✅ IMPLEMENTED

**What was done:**
- `BaseAssetHolderService.GetBalancesByAssetType(id, asOfDate?)` accepts optional `DateTime? asOfDate`
- `BaseAssetHolderService.GetBalancesByAssetGroup(id, asOfDate?)` accepts optional `DateTime? asOfDate`
- All three transaction types (Fiat, Digital, Settlement) are filtered by `tx.Date <= asOfDate`
- `BaseAssetHolderController.GetBalance(id, asOfDate?)` exposes the `[FromQuery] DateTime? asOfDate` parameter
- `PokerManagerController` overrides with the same parameter for AssetGroup balances

**Frontend:**
- `finance.service.ts` passes `?asOfDate=YYYY-MM-DD` on all balance calls
- `finance.queries.ts` hooks accept optional `date` parameter
- `usePlanilhaData` uses date-filtered balances for the financial report

**No further work needed.**

---

### Issue 2: Response Shape Mismatch — ⚠️ PARTIALLY ADDRESSED

**What was done:**
- The backend still returns `Dictionary<string, decimal>` (enum name → value)
- The frontend `finance.service.ts` handles the transformation:
  - Banks/Clients/Members: extracts `response.BrazilianReal` or sums all values → returns `number`
  - PokerManagers: extracts `response.FiatAssets` and `response.PokerAssets` → returns `{ value, coins, averageRate? }`

**What remains:**
- Backend returns raw dictionaries, not typed DTOs
- The entity-specific services (`client.service.ts`, `bank.service.ts`, etc.) used by extrato pages still call the same endpoint but have their own type expectations (`BalanceResponse`, `BalanceByAssetType[]`)
- There are effectively two parallel balance consumption paths: `finance.service.ts` (for the report) and entity services (for extrato pages)

**Assessment:** The current transformation works correctly. Creating typed DTOs would improve type safety but is **not a functional issue**. This is a code quality improvement, not a bug fix.

---

### Issue 3: Missing Aggregated Values for Poker Managers — ✅ IMPLEMENTED (via workaround)

**What was done:**
- `getManagerBalance()` in `finance.service.ts` maps `FiatAssets → value` and `PokerAssets → coins`
- `averageRate` is NOT returned by the balance endpoint, but is fetched separately via `/api/v1/finance/profit/avg-rates`
- `usePlanilhaData` merges the AvgRate data with manager balances in a `useMemo`

**What remains:**
- The balance endpoint itself does not return `averageRate` — it's fetched from a separate endpoint and merged on the frontend
- A `PokerManagerBalanceResponse` DTO that includes `averageRate` would eliminate this extra call

**Assessment:** Functionally complete. The separate AvgRate fetch works but adds an extra API call per report load.

---

### Issue 4: Inconsistent Sign Convention Documentation — ✅ ADDRESSED

**What was done:**
- The financial report data model (`FINANCIAL_REPORT_DATA_MODEL.md`) documents Ativos/Passivos formulas with explicit sign rules
- `ASSET_VALUATION_RULES.md` documents Cotação and sign conventions
- `TRANSACTION_BALANCE_IMPACT.md` documents balance impact rules including the RakeOverrideCommission special case

**What remains:**
- The API responses themselves don't include sign convention metadata (the plan proposed a `signConvention` field)
- Backend documentation covers the conventions; the API responses do not self-document them

**Assessment:** Documentation is complete. Adding sign metadata to responses is a nice-to-have, not a necessity.

---

### Issue 5: No Single-Value Balance Endpoint — ❌ NOT IMPLEMENTED

**What was proposed:** A `/balance/total` endpoint returning a single aggregated BRL value.

**Current state:** Frontend calculates totals:
- Banks/Clients/Members: `finance.service.ts` extracts `BrazilianReal` or sums all values
- PokerManagers: extracts `FiatAssets` as cash value

**Assessment:** The frontend workaround is adequate. A dedicated endpoint would reduce frontend logic but is not blocking any functionality.

---

## Remaining Improvements (Re-Prioritized)

Given the current implementation state, here is what remains from the original plan:

### Still Valuable Pre-Production

| # | Improvement | Effort | Value | Recommendation |
|---|------------|--------|-------|----------------|
| — | None | — | — | All critical balance issues are resolved |

### Nice-to-Have (Post-Production)

| # | Improvement | Effort | Value | Recommendation |
|---|------------|--------|-------|----------------|
| 1 | Typed balance DTOs replacing `Dictionary<string, decimal>` | Medium | Code quality, type safety | Defer — current transformation works |
| 2 | `PokerManagerBalanceResponse` with embedded `averageRate` | Medium | Eliminates extra API call | Defer — extra call is negligible |
| 3 | Sign convention metadata in API responses | Low | Self-documenting API | Defer — documentation covers this |
| 4 | `/balance/total` endpoint | Low | Simplifies frontend | Defer — frontend handles this fine |
| 5 | Unify duplicate balance service paths (finance.service vs entity services) | Medium | Code cleanup | Defer — both paths work correctly |

---

## Conclusion

**The critical balance improvements (date filtering, manager balance fields, sign documentation) are already implemented.** The remaining items are code quality and API design improvements that don't affect functionality.

**Recommendation:** Remove this plan from the pre-production sprint and defer all remaining items to post-production. Update `PRE_PRODUCTION_IMPLEMENTATION_PLAN.md` accordingly.

---

## Related Documentation

- [FINANCIAL_REPORT_DATA_MODEL.md](../Documentation/03_CORE_SYSTEMS/FINANCIAL_REPORT_DATA_MODEL.md) — Report data model and formulas
- [TRANSACTION_BALANCE_IMPACT.md](../Documentation/03_CORE_SYSTEMS/TRANSACTION_BALANCE_IMPACT.md) — Balance impact rules
- [BALANCE_ENDPOINTS.md](../Documentation/06_API/BALANCE_ENDPOINTS.md) — API endpoint documentation
- [ASSET_VALUATION_RULES.md](../Documentation/08_BUSINESS_RULES/ASSET_VALUATION_RULES.md) — Sign conventions and valuation rules
