# Audit System

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [DataContext Audit Logic](#datacontext-audit-logic)
- [Audit Information Sources](#audit-information-sources)
- [Querying Audit Data](#querying-audit-data)
- [Best Practices](#best-practices)
- [Related Documentation](#related-documentation)

---

## Overview

The SF Management API implements an automatic audit system that tracks entity lifecycle changes. All domain entities inherit audit capabilities from `BaseDomain`, with the `DataContext` automatically managing timestamps and user tracking during database operations.

> **Note:** For details on the `BaseDomain` model and soft delete implementation, see [SOFT_DELETE_AND_DATA_LIFECYCLE.md](SOFT_DELETE_AND_DATA_LIFECYCLE.md).

---

## Architecture

### Core Components

```
┌─────────────────────────────────────────────────────────────────┐
│                        Application Layer                         │
│  ┌─────────────┐    ┌─────────────┐    ┌──────────────────┐    │
│  │ Controllers │───▶│  Services   │───▶│   DataContext    │    │
│  └─────────────┘    └─────────────┘    └────────┬─────────┘    │
│                                                  │              │
│                                        ┌─────────▼─────────┐   │
│                                        │  LoggingService   │   │
│                                        └───────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

| Component | Responsibility |
|-----------|----------------|
| **BaseDomain** | Abstract base class providing audit properties to all entities |
| **DataContext** | Intercepts save operations to populate audit fields and log events |
| **LoggingService** | Records detailed audit trails including user context and request metadata |

> **Note:** For details on the logging service, see [LOGGING.md](LOGGING.md). For user identification and authentication, see [AUTHENTICATION.md](AUTHENTICATION.md).

---

## DataContext Audit Logic

**File**: `Data/DataContext.cs`

The `DataContext` overrides `SaveChangesAsync` to automatically populate audit fields before persisting changes.

### How It Works

```csharp
public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    SetDefaultProperties();
    return base.SaveChangesAsync(cancellationToken);
}
```

The `SetDefaultProperties` method iterates through tracked `BaseDomain` entities and applies audit logic based on the entity state:

### Entity State Handling

#### On Entity Creation (`EntityState.Added`)

When a new entity is added:
1. `CreatedAt` is set to `DateTime.UtcNow`
2. `LastModifiedBy` is set to the current user's ID
3. A creation event is logged via `ILoggingService`

#### On Entity Modification (`EntityState.Modified`)

When an existing entity is modified:

**For regular updates:**
1. `UpdatedAt` is set to `DateTime.UtcNow`
2. `LastModifiedBy` is set to the current user's ID
3. An update event is logged

**For soft deletes (when `DeletedAt` is being set):**
1. `UpdatedAt` modification is skipped
2. `DeletedAt` is set to `DateTime.UtcNow`
3. `LastModifiedBy` is set to the current user's ID
4. A deletion event is logged

### User Identification

The system derives a consistent `Guid` from the authenticated user's Auth0 `sub` claim using SHA256 hashing. This ensures:

- **Deterministic**: The same Auth0 user always produces the same `Guid`
- **Secure**: The original `sub` claim is hashed, not stored directly
- **Fallback**: System operations without authentication use the zero `Guid`

> **Note:** For details on user identification and the hashing implementation, see [AUTHENTICATION.md](AUTHENTICATION.md#database-integration).

---

## Audit Information Sources

The audit system provides two complementary sources of information:

### Database (Current State)

Query the database for the current audit state of any entity:

```sql
SELECT Id, CreatedAt, UpdatedAt, DeletedAt, LastModifiedBy
FROM Clients
WHERE Id = @clientId;
```

This tells you:
- When the entity was created
- When it was last updated
- Whether it's been deleted (and when)
- Who last modified it

### Logs (Historical Trail)

Logs provide the complete history of all operations:

```json
{
  "Timestamp": "2026-01-14T10:30:00Z",
  "Level": "Information",
  "Message": "Data Access: create on Client 123e4567-e89b-12d3-a456-426614174000 by auth0|user123 (john@example.com)",
  "Properties": {
    "Operation": "create",
    "EntityType": "Client",
    "EntityId": "123e4567-e89b-12d3-a456-426614174000",
    "UserId": "auth0|user123",
    "UserEmail": "john@example.com",
    "Changes": {
      "CreatedBy": "d4735e3a-265e-16ea-ec42-c2c5a9bdc8d8",
      "CreatedAt": "2026-01-14T10:30:00Z"
    }
  }
}
```

This tells you:
- The complete sequence of operations
- Who performed each operation
- What data changed
- Full request context (IP, user agent, etc.)

### Comparison

| Aspect | Database | Logs |
|--------|----------|------|
| Shows | Current state | Complete history |
| Query | SQL | Log aggregator (Seq, etc.) |
| Retention | Until deleted | Based on log retention policy |
| Performance | Fast lookups | Searchable archive |

---

## Querying Audit Data

### Find All Records Modified by a User

```csharp
var userRecords = await _context.Clients
    .Where(c => c.LastModifiedBy == targetUserId)
    .ToListAsync();
```

### Find Recently Modified Records

```csharp
var recentChanges = await _context.Clients
    .Where(c => c.UpdatedAt >= cutoffDate)
    .OrderByDescending(c => c.UpdatedAt)
    .ToListAsync();
```

### Include Soft-Deleted Records

```csharp
var allRecords = await _context.Clients
    .IgnoreQueryFilters() // If global filters are configured
    .ToListAsync();

// Or explicitly:
var includingDeleted = await _context.Clients
    .Where(c => c.DeletedAt.HasValue || !c.DeletedAt.HasValue) // All records
    .ToListAsync();
```

### Security Considerations

- **Consistency**: Same Auth0 user always generates the same deterministic `Guid`
- **Privacy**: Original Auth0 identifiers are hashed using SHA256
- **Traceability**: The mapping from hash to user can be established through Auth0 logs if needed
- **System Operations**: Operations without an authenticated user (background jobs, migrations) use the zero `Guid`: `00000000-0000-0000-0000-000000000000`

---

## Best Practices

1. **Never set audit properties manually** — Let `DataContext` handle all audit fields
2. **Use soft deletes** — Call `Delete()` instead of physically removing records
3. **Query with filters** — Always check `!DeletedAt.HasValue` unless you explicitly need deleted records
4. **Rely on logs for history** — The database stores current state; logs store complete history
5. **Handle unauthenticated operations** — Background jobs will use the system user ID; ensure this is acceptable for your audit requirements

---

## Related Documentation

| Topic | Document |
|-------|----------|
| BaseDomain & Soft Delete | [SOFT_DELETE_AND_DATA_LIFECYCLE.md](SOFT_DELETE_AND_DATA_LIFECYCLE.md) |
| Authentication & User ID | [AUTHENTICATION.md](AUTHENTICATION.md) |
| Logging Service | [LOGGING.md](LOGGING.md) |
| Service Layer | [SERVICE_LAYER_ARCHITECTURE.md](../02_ARCHITECTURE/SERVICE_LAYER_ARCHITECTURE.md) |
| Error Handling | [ERROR_HANDLING.md](ERROR_HANDLING.md) |
