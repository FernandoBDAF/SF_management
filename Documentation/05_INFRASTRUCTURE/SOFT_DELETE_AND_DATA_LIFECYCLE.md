# Soft Delete and Data Lifecycle

## Overview

The SF Management system implements soft delete across all entities, meaning records are never physically deleted from the database. Instead, a `DeletedAt` timestamp is set to mark records as deleted.

---

## BaseDomain

All entities inherit from `BaseDomain`:

```csharp
public class BaseDomain
{
    public Guid Id { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }        // Soft delete marker
    public string? CreatedBy { get; set; }
    public string? LastModifiedBy { get; set; }
    public string? DeletedBy { get; set; }
}
```

---

## Soft Delete Implementation

### BaseService.Delete()

```csharp
public virtual async Task Delete(Guid id)
{
    var obj = await _entity.FirstOrDefaultAsync(x => x.Id == id && !x.DeletedAt.HasValue);
    if (obj != null)
    {
        obj.DeletedAt = DateTime.UtcNow;  // Mark as deleted
        _entity.Update(obj);
        await context.SaveChangesAsync();
    }
}
```

### Automatic Query Filtering

All queries automatically exclude deleted records:

```csharp
public virtual async Task<List<TEntity>> List()
{
    return await _entity
        .Where(x => !x.DeletedAt.HasValue)  // Exclude deleted
        .OrderByDescending(x => x.CreatedAt)
        .ToListAsync();
}
```

---

## Cascade Soft Delete

When deleting an asset holder, related entities are also soft deleted:

```csharp
public async Task<bool> DeleteWithValidation(Guid entityId)
{
    // 1. Soft delete the specific entity (Client, Bank, etc.)
    await Delete(assetHolder.Id);

    // 2. Soft delete the BaseAssetHolder
    baseAssetHolder.DeletedAt = DateTime.UtcNow;
    await context.SaveChangesAsync();
}
```

---

## Deletion Validation

Before deletion, business rules are checked:

```csharp
public async Task<bool> CanDelete(Guid entityId)
{
    // Check for active transactions
    if (await HasActiveTransactions(entityId)) return false;
    
    // Check for non-zero balance
    if (await GetTotalBalance(entityId) != 0) return false;
    
    return true;
}
```

---

## Benefits

1. **Data Recovery** - Records can be restored if needed
2. **Audit Trail** - Complete history preserved
3. **Referential Integrity** - Foreign keys remain valid
4. **Compliance** - Data retention requirements met

---

## Considerations

1. **Query Performance** - Indexes on `DeletedAt` recommended
2. **Storage Growth** - Deleted records consume space
3. **Reporting** - Explicitly filter or include deleted records as needed

---

## Related Documentation

- [DATABASE_SCHEMA.md](../02_ARCHITECTURE/DATABASE_SCHEMA.md) - Schema details
- [AUDIT_SYSTEM.md](AUDIT_SYSTEM.md) - Audit tracking

