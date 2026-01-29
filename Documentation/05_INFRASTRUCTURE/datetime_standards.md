# DateTime Standards

## Overview

This document defines the standard conventions for handling dates and times throughout the SF Management system. Consistent date/time handling is critical for a financial system operating in Brazil.

---

## Table of Contents

1. [Storage Conventions](#storage-conventions)
2. [API Contracts](#api-contracts)
3. [Date Filtering Semantics](#date-filtering-semantics)
4. [Timezone Handling](#timezone-handling)
5. [Implementation Patterns](#implementation-patterns)
6. [Business Rules](#business-rules)

---

## Storage Conventions

### Database Storage

All `DateTime` fields are stored in **UTC** (Coordinated Universal Time).

| Field Type | Storage Format | Example |
|------------|---------------|---------|
| `DateTime` | UTC | `2026-01-29T14:30:00Z` |
| `DateOnly` | Date only | `2026-01-29` |
| `DateTime?` | Nullable UTC | `null` or `2026-01-29T14:30:00Z` |

### Entity Framework Configuration

```csharp
// BaseDomain audit timestamps - always UTC
public DateTime CreatedAt { get; set; }     // Set via SaveChangesInterceptor
public DateTime? UpdatedAt { get; set; }    // Set via SaveChangesInterceptor
public DateTime? DeletedAt { get; set; }    // Soft delete timestamp

// Transaction dates - always UTC
public DateTime Date { get; set; }
```

### Code Pattern: Always Use UTC

```csharp
// CORRECT: Use DateTime.UtcNow for all timestamps
obj.DeletedAt = DateTime.UtcNow;
obj.UpdatedAt = DateTime.UtcNow;

// INCORRECT: Never use DateTime.Now (uses local timezone)
obj.DeletedAt = DateTime.Now;  // ❌ Don't do this
```

---

## API Contracts

### Request Formats

The API accepts dates in the following formats:

| Format | Example | Usage |
|--------|---------|-------|
| ISO 8601 Date | `2026-01-29` | Date-only parameters (recommended) |
| ISO 8601 DateTime | `2026-01-29T14:30:00Z` | Full datetime with timezone |
| ISO 8601 DateTime (offset) | `2026-01-29T14:30:00-03:00` | Explicit timezone offset |

### Query Parameters

For date-filtered endpoints, use `YYYY-MM-DD` format:

```
GET /api/v1/client/{id}/balance?asOfDate=2026-01-29
GET /api/v1/finance/profit/summary?startDate=2026-01-01&endDate=2026-01-31
GET /api/v1/transactions?startDate=2026-01-01&endDate=2026-01-31
```

### Response Format

All datetime fields in API responses use ISO 8601 format with UTC (`Z` suffix):

```json
{
  "createdAt": "2026-01-29T14:30:00Z",
  "updatedAt": "2026-01-29T15:45:00Z",
  "date": "2026-01-29T00:00:00Z"
}
```

### Model Binding

ASP.NET Core automatically parses date strings to `DateTime`:

```csharp
[HttpGet("balance")]
public async Task<IActionResult> GetBalance(
    Guid id,
    [FromQuery] DateTime? asOfDate = null)  // Accepts YYYY-MM-DD or ISO 8601
```

---

## Date Filtering Semantics

### Range Queries

All date range queries use **inclusive** boundaries:

| Parameter | Semantics | SQL Equivalent |
|-----------|-----------|----------------|
| `startDate` | Inclusive start | `WHERE Date >= startDate` |
| `endDate` | Inclusive end | `WHERE Date <= endDate` |
| `asOfDate` | Up to and including | `WHERE Date <= asOfDate` |

### Implementation Pattern

```csharp
// Date range filtering (both inclusive)
query = query.Where(t => t.Date >= startDate && t.Date <= endDate);

// Balance as-of-date filtering (inclusive)
if (asOfDate.HasValue)
{
    digitalQuery = digitalQuery.Where(dt => dt.Date <= asOfDate.Value);
    fiatQuery = fiatQuery.Where(ft => ft.Date <= asOfDate.Value);
}
```

### Month Boundary Calculation

For monthly reports, calculate boundaries correctly:

```csharp
// Calculate first and last day of a month
var monthStart = new DateTime(year, month, 1);
var monthEnd = monthStart.AddMonths(1).AddDays(-1);

// Or using end of day for the last day
var monthEndDateTime = monthStart.AddMonths(1).AddTicks(-1);

// Query example
var transactions = await query
    .Where(t => t.Date >= monthStart && t.Date <= monthEnd)
    .ToListAsync();
```

---

## Timezone Handling

### Server-Side

The backend operates entirely in **UTC**:

- All timestamps stored as UTC
- All date comparisons in UTC
- No timezone conversions in backend logic

### Client-Side (Frontend)

The frontend is responsible for timezone conversion:

| Direction | Conversion |
|-----------|------------|
| Display → User | Convert UTC to `America/Sao_Paulo` (Brazil) |
| User Input → API | Send as UTC or let API parse local date |

### Frontend Pattern (TypeScript/JavaScript)

```typescript
// Display: Convert UTC to Brazil time
const displayDate = new Date(utcDateString).toLocaleString('pt-BR', {
  timeZone: 'America/Sao_Paulo'
});

// API Request: Send date only (no time component)
const apiDate = date.toISOString().split('T')[0];  // "2026-01-29"

// For month selection
const monthStart = new Date(selectedDate.getFullYear(), selectedDate.getMonth(), 1)
  .toISOString()
  .split('T')[0];  // "2026-01-01"
```

### Brazil Timezone Notes

- **Standard Time:** UTC-3 (Brasília Time - BRT)
- **No Daylight Saving Time:** Brazil abolished DST in 2019
- **IANA Identifier:** `America/Sao_Paulo`

---

## Implementation Patterns

### Creating Timestamps

```csharp
// Audit timestamps (handled by SaveChangesInterceptor)
public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(...)
{
    foreach (var entry in context.ChangeTracker.Entries<BaseDomain>())
    {
        if (entry.State == EntityState.Added)
        {
            entry.Entity.CreatedAt = DateTime.UtcNow;
        }
        
        if (entry.State == EntityState.Modified)
        {
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}

// Manual timestamps
entity.ApprovedAt = DateTime.UtcNow;
```

### Comparing Dates

```csharp
// Check if date is in the past
if (transaction.Date < DateTime.UtcNow.Date)
{
    // Historical transaction
}

// Check if same month
private static bool IsCurrentMonth(int year, int month)
{
    var now = DateTime.UtcNow;
    return year == now.Year && month == now.Month;
}

// Check active period
public bool IsActiveAt(DateTime date) =>
    (ActiveFrom == null || date >= ActiveFrom) &&
    (ActiveUntil == null || date <= ActiveUntil);
```

### Date Arithmetic

```csharp
// Add months (handles varying month lengths)
var nextMonth = currentDate.AddMonths(1);

// Get days in month
var daysInMonth = DateTime.DaysInMonth(year, month);

// First day of month
var firstDay = new DateTime(year, month, 1);

// Last day of month
var lastDay = new DateTime(year, month, daysInMonth);
```

### Cache Keys with Dates

For date-based caching, use consistent key formats:

```csharp
// Monthly cache key
private string GetCacheKey(Guid managerId, int year, int month) 
    => $"AvgRate:{managerId}:{year}:{month:D2}";  // "AvgRate:guid:2026:01"

// Date-based cache key
private string GetCacheKey(Guid entityId, DateTime date)
    => $"Balance:{entityId}:{date:yyyy-MM-dd}";  // "Balance:guid:2026-01-29"
```

---

## Business Rules

### Transaction Date Constraints

| Rule | Implementation |
|------|----------------|
| Future dates | Generally allowed (for scheduling) |
| Past dates | Allowed (for historical corrections) |
| Validation | Transactions dated > 1 year in future may require confirmation |

### Soft Delete Dates

Soft-deleted records retain their `DeletedAt` timestamp permanently:

```csharp
// Soft delete
entity.DeletedAt = DateTime.UtcNow;

// Query excludes soft-deleted
query.Where(x => !x.DeletedAt.HasValue)
```

### Audit Trail

All entities track creation and modification times:

| Field | Set When | Value |
|-------|----------|-------|
| `CreatedAt` | Entity created | `DateTime.UtcNow` |
| `UpdatedAt` | Entity modified | `DateTime.UtcNow` |
| `DeletedAt` | Entity soft-deleted | `DateTime.UtcNow` |

---

## Common Mistakes to Avoid

### 1. Using Local Time

```csharp
// ❌ WRONG: Uses server's local timezone
var now = DateTime.Now;

// ✅ CORRECT: Uses UTC
var now = DateTime.UtcNow;
```

### 2. Inconsistent Date Formats in API

```typescript
// ❌ WRONG: Inconsistent formatting
const date1 = "01/29/2026";       // US format
const date2 = "29/01/2026";       // Brazilian format

// ✅ CORRECT: ISO 8601 format
const date = "2026-01-29";
```

### 3. Timezone-Dependent Comparisons

```csharp
// ❌ WRONG: Compares UTC with potentially local date
if (entity.Date == selectedDate)

// ✅ CORRECT: Compare dates only (ignoring time)
if (entity.Date.Date == selectedDate.Date)
```

### 4. Forgetting End-of-Day for Inclusive Ranges

```csharp
// ❌ WRONG: Misses transactions on the end date after midnight
query.Where(t => t.Date < endDate)

// ✅ CORRECT: Includes entire end date
query.Where(t => t.Date <= endDate)
// Or for datetime precision:
query.Where(t => t.Date < endDate.AddDays(1))
```

---

## Related Documentation

- [AUDIT_SYSTEM.md](AUDIT_SYSTEM.md) - Audit trail and timestamps
- [API_REFERENCE.md](../06_API/API_REFERENCE.md) - API date parameter examples
- [TRANSACTION_INFRASTRUCTURE.md](../03_CORE_SYSTEMS/TRANSACTION_INFRASTRUCTURE.md) - Transaction date handling

---

*Last updated: January 29, 2026*
