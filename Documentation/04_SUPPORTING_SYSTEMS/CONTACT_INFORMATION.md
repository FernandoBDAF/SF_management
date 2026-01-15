# Contact Information System

## Overview

The Contact Information system manages addresses and phone numbers for asset holders, providing multiple contact options per entity.

---

## Data Models

### Address

```csharp
public class Address : BaseDomain
{
    public Guid BaseAssetHolderId { get; set; }
    public string Street { get; set; }
    public string Number { get; set; }
    public string? Complement { get; set; }
    public string Neighborhood { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
    public bool IsPrimary { get; set; }
}
```

### ContactPhone

```csharp
public class ContactPhone : BaseDomain
{
    public Guid BaseAssetHolderId { get; set; }
    public string PhoneNumber { get; set; }
    public string? PhoneType { get; set; }        // Mobile, Home, Work
    public bool IsPrimary { get; set; }
    public bool IsWhatsApp { get; set; }
}
```

---

## Relationships

```
BaseAssetHolder
├── Addresses (1:N)
└── ContactPhones (1:N)
```

Each asset holder can have multiple addresses and phone numbers, with one of each marked as primary.

---

## Service Methods

### AddressService

```csharp
public class AddressService : BaseService<Address>
{
    // Inherits standard CRUD from BaseService
}
```

### ContactPhoneService

```csharp
public class ContactPhoneService : BaseService<ContactPhone>
{
    // Inherits standard CRUD from BaseService
}
```

---

## Usage in Entity Retrieval

Contact information is automatically included when fetching asset holders:

```csharp
var baseAssetHolder = await context.BaseAssetHolders
    .Include(c => c.Addresses)
    .Include(c => c.ContactPhones)
    .FirstOrDefaultAsync(x => x.Id == id);
```

---

## Related Documentation

- [ENTITY_INFRASTRUCTURE.md](../03_CORE_SYSTEMS/ENTITY_INFRASTRUCTURE.md) - Entity details

