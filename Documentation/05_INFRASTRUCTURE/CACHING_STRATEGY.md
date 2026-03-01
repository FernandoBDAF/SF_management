# Caching Strategy

## Overview

The system uses `IMemoryCache` for in-memory caching with metrics tracking via `CacheMetricsService`. Caching is applied strategically to expensive queries — particularly AvgRate calculations and entity lookups.

All caching is in-process memory cache, suitable for the current single-instance deployment.

---

## CacheMetricsService

Wraps `IMemoryCache` with per-category hit/miss tracking. All cached operations go through this service to enable centralized monitoring.

### API

| Method | Description |
|--------|-------------|
| `GetOrCreateAsync<T>(key, factory, duration, category)` | Returns cached value or executes factory, caches result, and tracks hit/miss |
| `Remove(key, category)` | Removes cached entry with logging |
| `GetStatistics()` | Returns per-category hit/miss stats (exposed via DiagnosticsController) |

---

## CachedLookupService

Provides cached entity lookups used across multiple services.

| Cache Key | TTL | Purpose |
|-----------|-----|---------|
| `lookups.poker-manager-ids` | 10 minutes | All PokerManager IDs for quick membership checks |

Used by balance calculation to identify RakeOverrideCommission managers without repeated database queries.

---

## AvgRate Caching Strategy

AvgRate is the most expensive calculation in the system. Caching is applied based on data mutability:

| Data | TTL | Rationale |
|------|-----|-----------|
| Past months | 24 hours | Data is immutable once a month closes |
| Current month | Never cached | Calculated dynamically — active data changes frequently |
| Manager wallet lookups | 10 minutes | Entity data can change |
| Initial balance lookups | 10 minutes | Entity data can change |

### Cache Keys

Cache keys follow the pattern: `avgrate.{managerId}.{year}.{month}`

### Invalidation

`InvalidateFromDate()` clears all cached months from the given date forward. This is triggered when transactions are created or updated that affect PokerManager wallets, ensuring cascade invalidation from the affected month through the present.

---

## Response Caching

ASP.NET response caching is configured at the application level but selectively applied via `[ResponseCache]` attributes on read-heavy GET endpoints.

| Duration | Endpoints |
|----------|-----------|
| 60 seconds | Balance, wallet-identifiers-connected |
| 300 seconds | Company asset pool summary |
| 600 seconds | Company asset pool analytics |

---

## Cache Monitoring

Cache statistics are available via:

```
GET /api/v1/diagnostics/cache-stats
```

This endpoint is admin-only and returns hit/miss rates per category, enabling visibility into cache effectiveness without external tooling.

---

## Design Decisions

| Decision | Rationale |
|----------|-----------|
| Memory cache over distributed cache | Single-instance deployment; no need for Redis complexity |
| Aggressive caching for AvgRate | Most expensive calculation; past-month data is immutable |
| Short TTL for lookup caches | Entity data can change; 10-minute window limits staleness |
| No cache for current-month AvgRate | Active data changes frequently; stale results would be incorrect |

---

## Related Documentation

- [INFRASTRUCTURE_SERVICES.md](INFRASTRUCTURE_SERVICES.md) — Infrastructure service registration and configuration
- [PROFIT_CALCULATION_SYSTEM.md](../03_FEATURES/PROFIT_CALCULATION_SYSTEM.md) — Profit calculation system that relies heavily on caching
