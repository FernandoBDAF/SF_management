# Category System

## Overview

The Category system provides hierarchical classification for transactions, enabling organized reporting and filtering of financial activities.

---

## Data Model

### Category Entity

**File**: `Models/Support/Category.cs`

```csharp
public class Category : BaseDomain
{
    public string Description { get; set; }
    public Guid? ParentId { get; set; }           // Hierarchical support
    public virtual Category? Parent { get; set; }
    public virtual ICollection<Category> Children { get; set; }
}
```

---

## Hierarchical Structure

```
Category
├── Income
│   ├── Poker Winnings
│   ├── Rake Commission
│   └── Referral Commission
├── Expense
│   ├── Player Payments
│   ├── Bank Fees
│   └── Operating Costs
└── Transfer
    ├── Internal Transfer
    └── Settlement
```

---

## Service

### CategoryService

```csharp
public class CategoryService : BaseService<Category>
{
    public override async Task<List<Category>> List()
    {
        // Returns hierarchical structure
        return await context.Categories
            .Include(c => c.Children)
            .Where(c => c.ParentId == null && !c.DeletedAt.HasValue)
            .ToListAsync();
    }
}
```

---

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/category` | List all (hierarchical) |
| GET | `/api/v1/category/{id}` | Get by ID |
| POST | `/api/v1/category` | Create category |
| PUT | `/api/v1/category/{id}` | Update category |
| DELETE | `/api/v1/category/{id}` | Delete category |

---

## Usage in Transactions

Every transaction type has an optional `CategoryId`:

```csharp
public class BaseTransaction : BaseDomain
{
    public Guid? CategoryId { get; set; }
    public virtual Category? Category { get; set; }
}
```

---

## Related Documentation

- [TRANSACTION_INFRASTRUCTURE.md](../03_CORE_SYSTEMS/TRANSACTION_INFRASTRUCTURE.md) - Transaction details

