# Balance System Improvements Plan

> **Status:** Proposed  
> **Last Updated:** January 24, 2026  
> **Scope:** Backend balance API improvements based on frontend-backend analysis

---

## Executive Summary

This document outlines improvements needed in the backend balance system based on a cross-analysis of:
- Frontend balance consumption patterns (`SF_management-front/documentation/03_CORE_SYSTEMS/BALANCE_DISPLAY_USAGE.md`)
- Backend balance implementation (`SF_management/Documentation/06_API/BALANCE_ENDPOINTS.md`)

---

## Current State Analysis

### What the Backend Provides

| Endpoint | Response Type | Grouping |
|----------|---------------|----------|
| `GET /api/v1/bank/{id}/balance` | `Dictionary<string, decimal>` | By AssetType |
| `GET /api/v1/client/{id}/balance` | `Dictionary<string, decimal>` | By AssetType |
| `GET /api/v1/member/{id}/balance` | `Dictionary<string, decimal>` | By AssetType |
| `GET /api/v1/pokermanager/{id}/balance` | `Dictionary<string, decimal>` | By AssetGroup |

### What the Frontend Expects

| Location | Expected Shape | Current Issue |
|----------|---------------|---------------|
| **Extrato (Client/Member)** | `{ balance: number }` | Backend returns `Dictionary<AssetType, decimal>` |
| **Extrato (Bank/Manager)** | `BalanceByAssetType[]` | Backend returns `Dictionary`, not array |
| **Planilha** | Date-filtered balance with `{ value, coins, averageRate }` | No date filter support; no coins/rate fields |

### Legacy Code (Commented Out)

The `BalanceResponse` class exists but is entirely commented out:
```csharp
// Application/DTOs/Assets/BalanceResponse.cs
public class BalanceResponse
{
    // public decimal Value { get; set; }
    // public decimal Coins { get; set; }
    // public decimal AverateRate { get; set; }
}
```

The `BalanceRequest` has a `Date` field but it's never used:
```csharp
public class BalanceRequest
{
    public DateTime? Date { get; set; }
}
```

---

## Identified Issues

### 1. No Date-Filtered Balance Support

**Problem:** The frontend planilha requires balances calculated up to a specific date. The backend does not support this.

**Impact:** The planilha cannot show historical balances or balance snapshots.

**Current Workaround:** Frontend may be calculating balances client-side or using stale data.

### 2. Response Shape Mismatch

**Problem:** Backend returns `Dictionary<string, decimal>`. Frontend expects different shapes for different contexts.

**Impact:** 
- Extrato pages must transform data before display
- Type safety is lost
- Risk of key mismatches between frontend and backend

### 3. Missing Aggregated Values for Poker Managers

**Problem:** Poker managers need `coins` and `averageRate` in addition to balance values. The backend does not compute or return these.

**Impact:** Frontend must calculate these values separately or use hardcoded defaults.

### 4. Inconsistent Sign Convention Documentation

**Problem:** The `AccountClassification` adjustment logic is complex and not explicitly documented for frontend consumers.

**Impact:** Frontend may misinterpret balance signs, leading to incorrect display (e.g., showing debts as credits).

### 5. No Single-Value Balance Endpoint

**Problem:** Clients and members often need a single total balance (e.g., in BRL), but the endpoint returns per-asset-type breakdown.

**Impact:** Frontend must aggregate manually.

---

## Proposed Improvements

### Priority 1: Date-Filtered Balance Endpoint

**Recommendation:** Add date support to existing balance endpoints.

```csharp
[HttpGet("{id}/balance")]
public async Task<IActionResult> GetBalance(Guid id, [FromQuery] DateTime? asOf = null)
{
    var balances = await _service.GetBalancesByAssetType(id, asOf);
    return Ok(balances);
}
```

**Service Change:**
```csharp
public async Task<Dictionary<AssetType, decimal>> GetBalancesByAssetType(
    Guid baseAssetHolderId, 
    DateTime? asOf = null)
{
    // Filter transactions by date if asOf is provided
    var cutoffDate = asOf ?? DateTime.UtcNow;
    
    // ... existing logic with date filter ...
}
```

**Effort:** Medium  
**Risk:** Low (backward compatible - default is current behavior)

---

### Priority 2: Typed Balance DTOs

**Recommendation:** Create typed DTOs instead of returning raw dictionaries.

```csharp
// Application/DTOs/Assets/BalanceResponse.cs
public class AssetHolderBalanceResponse
{
    public Guid AssetHolderId { get; set; }
    public DateTime CalculatedAt { get; set; }
    public DateTime? AsOfDate { get; set; }
    public List<BalanceEntry> Balances { get; set; } = new();
    public decimal TotalInBRL { get; set; }
}

public class BalanceEntry
{
    public string Key { get; set; } = string.Empty;  // AssetType or AssetGroup name
    public int KeyValue { get; set; }                // Enum value
    public decimal Balance { get; set; }
    public decimal? PendingBalance { get; set; }
}
```

**Effort:** Medium  
**Risk:** Medium (breaking change - requires frontend update)

---

### Priority 3: Poker Manager Extended Balance

**Recommendation:** Add poker manager-specific fields to balance response.

```csharp
public class PokerManagerBalanceResponse : AssetHolderBalanceResponse
{
    public decimal TotalCoins { get; set; }
    public decimal AverageRate { get; set; }
    public decimal CashBalance { get; set; }
    public decimal ChipsBalance { get; set; }
}
```

**Effort:** High  
**Risk:** Medium (new fields, backward compatible if added to existing response)

---

### Priority 4: Sign Convention Documentation & Helpers

**Recommendation:** Add explicit sign convention to response and create frontend-friendly helpers.

```csharp
public class BalanceEntry
{
    // ... existing fields ...
    
    /// <summary>
    /// Positive = asset holder has this amount (they own it or are owed it)
    /// Negative = asset holder owes this amount (liability)
    /// </summary>
    public string SignConvention { get; set; } = "standard";
    
    /// <summary>
    /// Account classification affecting this balance
    /// </summary>
    public AccountClassification? AccountClassification { get; set; }
}
```

**Effort:** Low  
**Risk:** Low

---

### Priority 5: Single Total Balance Endpoint

**Recommendation:** Add a simplified endpoint for getting a single aggregated balance.

```csharp
[HttpGet("{id}/balance/total")]
[ProducesResponseType(typeof(TotalBalanceResponse), StatusCodes.Status200OK)]
public async Task<IActionResult> GetTotalBalance(
    Guid id, 
    [FromQuery] AssetType? inAssetType = null,
    [FromQuery] DateTime? asOf = null)
{
    var total = await _service.GetTotalBalance(id, inAssetType ?? AssetType.BrazilianReal, asOf);
    return Ok(new TotalBalanceResponse { Balance = total, AssetType = inAssetType ?? AssetType.BrazilianReal });
}
```

**Effort:** Low  
**Risk:** Low

---

## Implementation Roadmap

### Phase 1: Quick Wins (Low Risk)

1. **Document sign conventions** in API response comments
2. **Add `asOf` query parameter** to existing endpoints (backward compatible)
3. **Clean up legacy code** - remove or restore `BalanceResponse` and `BalanceRequest`

### Phase 2: Type Safety (Breaking Change)

1. **Create typed DTOs** for balance responses
2. **Coordinate with frontend** for migration
3. **Deprecate dictionary response** with transition period

### Phase 3: Extended Features

1. **Implement poker manager extended balance** with coins and rate
2. **Add total balance endpoint** for simplified consumption
3. **Add balance caching improvements** if needed

---

## API Contract Proposal

### New Balance Response (v2)

```json
// GET /api/v2/bank/{id}/balance?asOf=2026-01-24
{
  "assetHolderId": "77ba107b-6221-4e85-4928-08ddc54a1a1b",
  "calculatedAt": "2026-01-24T12:00:00Z",
  "asOfDate": "2026-01-24T00:00:00Z",
  "balances": [
    {
      "key": "BrazilianReal",
      "keyValue": 21,
      "balance": -71853.36,
      "pendingBalance": null,
      "signConvention": "standard"
    }
  ],
  "totalInBRL": -71853.36
}
```

### Poker Manager Extended Response

```json
// GET /api/v2/pokermanager/{id}/balance
{
  "assetHolderId": "...",
  "calculatedAt": "2026-01-24T12:00:00Z",
  "asOfDate": null,
  "balances": [
    { "key": "FiatAssets", "keyValue": 1, "balance": 25000.00 },
    { "key": "PokerAssets", "keyValue": 2, "balance": 8500.00 }
  ],
  "totalInBRL": 25000.00,
  "totalCoins": 8500.00,
  "cashBalance": 25000.00,
  "chipsBalance": 8500.00,
  "averageRate": 5.45
}
```

---

## Frontend Alignment Checklist

After backend improvements, frontend should:

- [ ] Update `useClientBalance`, `useMemberBalance` to handle new DTO shape
- [ ] Update `useBankBalance`, `usePokerManagerBalance` to handle new DTO shape
- [ ] Update planilha to use `asOf` query parameter
- [ ] Remove any client-side balance calculations duplicating backend logic
- [ ] Update types to match new `AssetHolderBalanceResponse` structure

---

## Questions for Review

1. **Should date filtering be mandatory or optional?**
   - Recommendation: Optional, defaulting to "now"

2. **Should we version the API (v2) or update in place?**
   - Recommendation: New v2 endpoints to avoid breaking existing consumers

3. **What is the source of truth for `averageRate`?**
   - Need to clarify calculation method for poker managers

4. **Should pending balances be included?**
   - Some transactions may be "pending approval" - should these affect balance?

---

## Related Documentation

- `SF_management/Documentation/06_API/BALANCE_ENDPOINTS.md` - Current implementation details
- `SF_management-front/documentation/03_CORE_SYSTEMS/BALANCE_DISPLAY_USAGE.md` - Frontend consumption patterns

---

*Last updated: January 24, 2026*
