# Entity Infrastructure

## Table of Contents

- [Overview](#overview)
- [Core Concepts](#core-concepts)
- [Domain Models](#domain-models)
  - [BaseAssetHolder](#baseasetholder)
  - [Client](#client)
  - [Bank](#bank)
  - [Member](#member)
  - [PokerManager](#pokermanager)
  - [IAssetHolder Interface](#iassetholder-interface)
- [Request/Response Models](#requestresponse-models)
- [API Usage Examples](#api-usage-examples)
- [Entity Relationships Diagram](#entity-relationships-diagram)
- [Related Documentation](#related-documentation)

---

## Overview

This document describes the entity infrastructure for **SF Management**, explaining how asset holders are modeled, validated, and managed through the system. The architecture uses a unified base entity pattern with specialized types for different business contexts.

---

## Core Concepts

### The BaseAssetHolder Pattern

All entities that can own assets in the system inherit from a common `BaseAssetHolder` entity. This provides:

- **Unified identity** - Common fields for name, tax information, and government ID
- **Polymorphic access** - One ID can represent a Client, Bank, Member, or PokerManager
- **Shared collections** - Asset pools, addresses, contacts, and initial balances

### Entity Hierarchy

```
BaseAssetHolder (common attributes)
    ├── Client (external customers)
    ├── Bank (financial institutions)
    ├── Member (internal team with profit sharing)
    └── PokerManager (poker site agents)
```

Each specialized entity has a one-to-one relationship with `BaseAssetHolder` and only one type can be active at a time (mutually exclusive).

---

## Domain Models

### BaseAssetHolder

The foundation entity that holds common attributes for all asset holder types:

```csharp
public class BaseAssetHolder : BaseDomain
{
    [Required] [MaxLength(32)] public string Name { get; set; }
    [Required] public TaxEntityType TaxEntityType { get; set; }
    [Required] [MaxLength(20)] public string GovernmentNumber { get; set; }
    
    // Navigation properties (only one will have a value)
    public virtual Client? Client { get; set; }
    public virtual Bank? Bank { get; set; }
    public virtual Member? Member { get; set; }
    public virtual PokerManager? PokerManager { get; set; }
    
    // Computed property for asset holder type
    public AssetHolderType AssetHolderType { get; }
    
    // Access the specific entity
    public object? SpecificAssetHolder { get; }
    
    // Validates exactly one type is set
    [NotMapped]
    public bool HasSingleEntityType { get; }
    
    // Related collections
    public virtual ICollection<AssetPool> AssetPools { get; set; }
    public virtual ICollection<InitialBalance> InitialBalances { get; set; }
    public virtual ICollection<Address> Addresses { get; set; }
    public virtual ICollection<ContactPhone> ContactPhones { get; set; }
    public virtual ICollection<ImportedTransaction> ImportedTransactions { get; set; }
    
    // Referral relationship
    public Guid? ReferrerId { get; set; }
    public virtual BaseAssetHolder? Referrer { get; set; }
    public virtual ICollection<Referral> ReferralsMade { get; set; }
}
```

**Key Features:**

| Property | Purpose |
|----------|---------|
| `TaxEntityType` | CPF (individual), CNPJ (company), or CNPJ_Not_Taxable |
| `GovernmentNumber` | The actual tax ID number |
| `AssetHolderType` | Computed: Client, Bank, Member, PokerManager, or Unknown |
| `HasSingleEntityType` | Validation: ensures exactly one type is populated |

### Client

Represents external customers who use the platform's services:

```csharp
public class Client : BaseDomain, IAssetHolder
{
    [Required] public Guid BaseAssetHolderId { get; set; }
    public virtual BaseAssetHolder? BaseAssetHolder { get; set; }
    
    public DateTime? Birthday { get; set; }
    
    // Computed from Birthday
    public int? Age { get; }
}
```

### Bank

Represents banking institutions:

```csharp
public class Bank : BaseDomain, IAssetHolder
{
    [Required] public Guid BaseAssetHolderId { get; set; }
    public virtual BaseAssetHolder? BaseAssetHolder { get; set; }
    
    [Required] [MaxLength(10)] public string Code { get; set; }
}
```

### Member

Represents internal team members with profit-sharing arrangements:

```csharp
public class Member : BaseDomain, IAssetHolder
{
    [Required] public Guid BaseAssetHolderId { get; set; }
    public virtual BaseAssetHolder? BaseAssetHolder { get; set; }
    
    [Precision(7, 4)]
    [Range(0.0000, 100.0000)]
    public decimal? Share { get; set; }  // Percentage (0-100)

    [Precision(18, 2)]
    public decimal? Salary { get; set; }
    
    public DateTime? Birthday { get; set; }
    
    // Computed: Share > 0
    public bool IsActiveShare { get; }
}
```

### PokerManager

Represents poker site managers/agents:

```csharp
public class PokerManager : BaseDomain, IAssetHolder
{
    [Required] public Guid BaseAssetHolderId { get; set; }
    public virtual BaseAssetHolder? BaseAssetHolder { get; set; }
    
    public ManagerProfitType? ManagerProfitType { get; set; }  // Spread or RakeOverrideCommission
}
```

### IAssetHolder Interface

All specialized entities implement this interface:

```csharp
public interface IAssetHolder
{
    Guid BaseAssetHolderId { get; set; }
    BaseAssetHolder? BaseAssetHolder { get; set; }
}
```

---

## Request/Response Models

### BaseAssetHolderRequest

Common fields for creating/updating any asset holder:

```csharp
public class BaseAssetHolderRequest
{
    public Guid? BaseAssetHolderId { get; set; }
    
    [Required]
    [StringLength(40, MinimumLength = 1)]
    public string Name { get; set; }
    
    [Required]
    public TaxEntityType TaxEntityType { get; set; }

    [Required]
    [MaxLength(20)]
    public string GovernmentNumber { get; set; }

    // Optional referrer for establishing referral relationships
    public Guid? ReferrerId { get; set; }
}
```

### Specialized Request Examples

```csharp
public class ClientRequest : BaseAssetHolderRequest
{
    public DateTime? Birthday { get; set; }
}

public class MemberRequest : BaseAssetHolderRequest
{
    [Range(0.0000, 100.0000)]
    public decimal? Share { get; set; }
    
    public decimal? Salary { get; set; }
    public DateTime? Birthday { get; set; }
}

public class BankRequest : BaseAssetHolderRequest
{
    [Required]
    [MaxLength(10)]
    public string Code { get; set; }
}
```

---

## API Usage Examples

### Create a Client

```http
POST /api/v1/Client
Content-Type: application/json

{
  "name": "João Silva",
  "taxEntityType": "CPF",
  "governmentNumber": "123.456.789-00",
  "birthday": "1990-05-15"
}
```

### Create a Member with Share

```http
POST /api/v1/Member
Content-Type: application/json

{
  "name": "Maria Santos",
  "taxEntityType": "CPF",
  "governmentNumber": "987.654.321-00",
  "share": 15.5000,
  "salary": 5000.00,
  "birthday": "1985-03-20"
}
```

### Get Client Balance

```http
GET /api/v1/Client/{id}/balance
```

Response:
```json
{
  "BrazilianReal": 15000.00,
  "PokerStars": 2500.00,
  "Bitcoin": 0.05
}
```

### Get Transaction Statement

```http
GET /api/v1/Client/{id}/transactions
```

Response:
```json
{
  "assetHolderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "assetHolderName": "João Silva",
  "transactions": [
    {
      "id": "...",
      "date": "2025-01-14T10:30:00Z",
      "amount": 1000.00,
      "direction": "Incoming",
      "counterpartyName": "Company",
      "transactionType": "Fiat"
    }
  ]
}
```

### Check Deletion Feasibility

```http
GET /api/v1/Client/{id}/can-delete
```

Response:
```json
{
  "canDelete": false,
  "reason": "Asset holder has active transactions"
}
```

---

## Entity Relationships Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                        BaseAssetHolder                          │
│  Name, TaxEntityType, GovernmentNumber                          │
│  ┌────────┬────────┬────────┬─────────────┐                     │
│  │ Client │  Bank  │ Member │PokerManager │  (1:1 exclusive)    │
│  └────────┴────────┴────────┴─────────────┘                     │
└───────────────────────┬─────────────────────────────────────────┘
                        │
        ┌───────────────┼───────────────┬───────────────┐
        │               │               │               │
        ▼               ▼               ▼               ▼
┌───────────────┐ ┌───────────┐ ┌───────────────┐ ┌───────────┐
│  AssetPools   │ │ Addresses │ │ ContactPhones │ │ Referrals │
└───────────────┘ └───────────┘ └───────────────┘ └───────────┘
        │
        ▼
┌───────────────────┐
│ WalletIdentifiers │
└───────────────────┘
        │
        ▼
┌───────────────────┐
│   Transactions    │
└───────────────────┘
```

---

## Summary

The entity infrastructure provides:

1. **Unified Base Entity** - `BaseAssetHolder` with common fields and collections
2. **Specialized Types** - Client, Bank, Member, PokerManager with type-specific attributes
3. **Generic Services** - `BaseAssetHolderService<TEntity>` with factory pattern
4. **Domain Validation** - `IAssetHolderDomainService` for centralized business logic
5. **Generic Controllers** - `BaseAssetHolderController` with standard endpoints
6. **Structured Exceptions** - Typed exceptions with detailed error information
7. **Strategy Pattern** - Efficient type handling without runtime reflection

This architecture enables:
- Adding new asset holder types with minimal code
- Consistent API patterns across all entity types
- Centralized validation and business rules
- Type-safe operations with compile-time checking
- Comprehensive error handling with detailed responses

---

## Related Documentation

For detailed information on specific aspects of the entity infrastructure, refer to:

| Topic | Document |
|-------|----------|
| Service Layer Patterns | [SERVICE_LAYER_ARCHITECTURE.md](../02_ARCHITECTURE/SERVICE_LAYER_ARCHITECTURE.md) |
| Controller Patterns | [CONTROLLER_LAYER_ARCHITECTURE.md](../02_ARCHITECTURE/CONTROLLER_LAYER_ARCHITECTURE.md) |
| Validation System | [VALIDATION_SYSTEM.md](../05_INFRASTRUCTURE/VALIDATION_SYSTEM.md) |
| Error Handling | [ERROR_HANDLING.md](../05_INFRASTRUCTURE/ERROR_HANDLING.md) |
| Database Schema | [DATABASE_SCHEMA.md](../02_ARCHITECTURE/DATABASE_SCHEMA.md) |
| API Endpoints | [API_REFERENCE.md](../06_API/API_REFERENCE.md) |
| Enum Definitions | [ENUMS_AND_TYPE_SYSTEM.md](../07_REFERENCE/ENUMS_AND_TYPE_SYSTEM.md) |
| Asset Infrastructure | [ASSET_INFRASTRUCTURE.md](./ASSET_INFRASTRUCTURE.md) |
