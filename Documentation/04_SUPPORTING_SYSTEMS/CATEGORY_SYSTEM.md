# Category System

## Overview

The Category system provides hierarchical classification for transactions, enabling organized reporting and filtering of financial activities. Categories follow a parent-child tree structure and are used across all transaction types for classification, direct income calculation, and financial reporting.

---

## Data Model

### Category Entity

**File**: `Models/Support/Category.cs`

```csharp
public class Category : BaseDomain
{
    public string Description { get; set; }
    public Guid? ParentId { get; set; }
    public virtual Category? Parent { get; set; }
    public virtual ICollection<Category> Children { get; set; }
}
```

**Inherited from `BaseDomain`**:
- `Id` (Guid) — primary key
- `CreatedAt` (DateTime) — creation timestamp
- `UpdatedAt` (DateTime?) — last modification timestamp
- `DeletedAt` (DateTime?) — soft delete timestamp

### Key Properties

| Property | Type | Description |
|----------|------|-------------|
| `Description` | `string` | Display name of the category |
| `ParentId` | `Guid?` | Reference to the parent category. `null` for root-level categories |
| `Parent` | `Category?` | Navigation property to the parent category |
| `Children` | `ICollection<Category>` | Navigation property to child categories |

---

## Authorization

### Controller: `CategoryController`

**File**: `Controllers/Support/CategoryController.cs`

The category controller uses a two-tier authorization strategy:

#### Class-Level Permission (Read Access)

```csharp
[RequirePermission(Auth0Permissions.ReadCategories)]
public class CategoryController : BaseController<Category>
```

This grants **read access** to any authenticated user with the `ReadCategories` permission. In practice, this includes **admins** and **managers**. Partners do **not** have access to categories.

#### Method-Level Role Restrictions (Write Access)

All write operations require the **Admin** role:

```csharp
[RequireRole(Auth0Roles.Admin)]
public override async Task<ActionResult<Category>> Post([FromBody] Category entity)

[RequireRole(Auth0Roles.Admin)]
public override async Task<ActionResult<Category>> Put(Guid id, [FromBody] Category entity)

[RequireRole(Auth0Roles.Admin)]
public override async Task<ActionResult<Category>> Delete(Guid id)
```

### Authorization Summary

| Role | GET (List/Detail) | POST (Create) | PUT (Update) | DELETE (Remove) |
|------|-------------------|---------------|--------------|-----------------|
| **Admin** | Yes | Yes | Yes | Yes |
| **Manager** | Yes | No | No | No |
| **Partner** | No | No | No | No |

---

## API Endpoints

| Method | Endpoint | Authorization | Description |
|--------|----------|---------------|-------------|
| GET | `/api/v1/category` | `ReadCategories` permission | List all root categories with children (hierarchical) |
| GET | `/api/v1/category/{id}` | `ReadCategories` permission | Get a single category by ID |
| POST | `/api/v1/category` | `Admin` role | Create a new category |
| PUT | `/api/v1/category/{id}` | `Admin` role | Update an existing category |
| DELETE | `/api/v1/category/{id}` | `Admin` role | Soft-delete a category |

---

## Hierarchical Categories

Categories support a parent-child tree structure through the `ParentId` foreign key. A category with `ParentId == null` is a root category, while categories with a `ParentId` are nested under their parent.

### Example Hierarchy

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

### Service: Hierarchical Retrieval

**File**: `Services/Support/CategoryService.cs`

The `List()` method returns only root categories, with their children eagerly loaded via `Include`:

```csharp
public class CategoryService : BaseService<Category>
{
    public override async Task<List<Category>> List()
    {
        return await context.Categories
            .Include(c => c.Children)
            .Where(c => c.ParentId == null && !c.DeletedAt.HasValue)
            .ToListAsync();
    }
}
```

**Key behaviors**:

1. **Root-only query**: The `.Where(c => c.ParentId == null)` clause returns only top-level categories. Child categories are accessed through the `Children` navigation property of each root category.
2. **Soft delete filtering**: The `.Where(!c.DeletedAt.HasValue)` clause excludes any category that has been soft-deleted.
3. **Eager loading**: `.Include(c => c.Children)` ensures child categories are loaded in a single query, avoiding N+1 problems.

### Category Map

The frontend provides a visual "Category Map" view that renders the full category hierarchy as a tree. This allows admins to see the complete structure at a glance and manage parent-child relationships visually. The map relies on the hierarchical data returned by the `List()` endpoint.

---

## Soft Delete

Categories use **soft delete** rather than hard delete. When a category is deleted through the API:

1. The `DeletedAt` timestamp is set on the category entity (via `BaseDomain`).
2. The record remains in the database but is excluded from query results.

### Backend Filtering

The `CategoryService.List()` method explicitly filters out soft-deleted categories:

```csharp
.Where(c => c.ParentId == null && !c.DeletedAt.HasValue)
```

### Frontend Filtering

The frontend applies an additional client-side filter to ensure deleted categories are never displayed:

```typescript
categories.filter(category => !category.deletedAt)
```

This double-layer filtering ensures soft-deleted categories are invisible to users while preserving referential integrity for historical transactions that reference them.

---

## Usage in Transactions

### Optional Classification

Every transaction type includes an optional `CategoryId` field, allowing transactions to be classified under a category:

```csharp
public class BaseTransaction : BaseDomain
{
    public Guid? CategoryId { get; set; }
    public virtual Category? Category { get; set; }
}
```

Since `CategoryId` is nullable, categories are not required — transactions can exist without classification.

### Transaction Classification and Reporting

Categories enable structured financial reporting by grouping transactions. Users can filter transaction lists by category to see, for example, all "Rake Commission" income or all "Bank Fees" expenses across a date range.

### Direct Income Calculation

Categories play a role in **Direct Income** calculations for system wallet transactions. When computing direct income, the system identifies transactions on the system wallet that have specific categories assigned, allowing it to distinguish between different income sources (e.g., rake vs. referral commissions).

### Financial Reporting Filters

Categories serve as a primary filter dimension in financial reports. Reports can be broken down by category to provide granular insight into where money is flowing, enabling stakeholders to analyze profitability by transaction type.

---

## Related Documentation

- [TRANSACTION_INFRASTRUCTURE.md](../03_CORE_SYSTEMS/TRANSACTION_INFRASTRUCTURE.md) — Transaction types and how CategoryId is used across all transaction models
- [AUTHENTICATION.md](../02_ARCHITECTURE/AUTHENTICATION.md) — Auth0 roles, permissions, and the `RequirePermission`/`RequireRole` attribute system
