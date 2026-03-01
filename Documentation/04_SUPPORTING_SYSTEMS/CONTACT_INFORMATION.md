# Contact Information System

## Overview

The Contact Information system manages addresses and phone numbers for asset holders. It provides two independent controllers — `AddressController` and `ContactPhoneController` — each following standard CRUD patterns with role-based authorization. Contact information is attached to any `BaseAssetHolder` type, meaning clients, banks, members, and managers can all have associated addresses and phone numbers.

---

## Data Models

### Address Entity

**File**: `Models/Support/Address.cs`

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

**Inherited from `BaseDomain`**:
- `Id` (Guid) — primary key
- `CreatedAt` (DateTime) — creation timestamp
- `UpdatedAt` (DateTime?) — last modification timestamp
- `DeletedAt` (DateTime?) — soft delete timestamp

#### Address Properties

| Property | Type | Description |
|----------|------|-------------|
| `BaseAssetHolderId` | `Guid` | The asset holder this address belongs to |
| `Street` | `string` | Street name |
| `Number` | `string` | Street/building number |
| `Complement` | `string?` | Optional apartment, suite, or unit identifier |
| `Neighborhood` | `string` | Neighborhood or district |
| `City` | `string` | City name |
| `State` | `string` | State or province |
| `PostalCode` | `string` | Postal/ZIP code |
| `Country` | `string` | Country name or code |
| `IsPrimary` | `bool` | Whether this is the primary address for the asset holder |

### ContactPhone Entity

**File**: `Models/Support/ContactPhone.cs`

```csharp
public class ContactPhone : BaseDomain
{
    public Guid BaseAssetHolderId { get; set; }
    public string PhoneNumber { get; set; }
    public string? PhoneType { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsWhatsApp { get; set; }
}
```

#### ContactPhone Properties

| Property | Type | Description |
|----------|------|-------------|
| `BaseAssetHolderId` | `Guid` | The asset holder this phone belongs to |
| `PhoneNumber` | `string` | The phone number |
| `PhoneType` | `string?` | Classification: `Mobile`, `Home`, `Work`, etc. |
| `IsPrimary` | `bool` | Whether this is the primary phone for the asset holder |
| `IsWhatsApp` | `bool` | Whether this number is reachable via WhatsApp |

---

## Relationship to BaseAssetHolder

Contact information is linked to the `BaseAssetHolder` abstract entity, which means **any** asset holder type can have addresses and phone numbers:

```
BaseAssetHolder
├── Client         → Addresses, ContactPhones
├── Bank           → Addresses, ContactPhones
├── Member         → Addresses, ContactPhones
└── Manager        → Addresses, ContactPhones
```

The `BaseAssetHolder` entity includes navigation properties for both collections:

```csharp
public abstract class BaseAssetHolder : BaseDomain
{
    // ... other properties ...
    public virtual ICollection<Address> Addresses { get; set; }
    public virtual ICollection<ContactPhone> ContactPhones { get; set; }
}
```

Contact information is automatically included when fetching asset holders:

```csharp
var baseAssetHolder = await context.BaseAssetHolders
    .Include(c => c.Addresses)
    .Include(c => c.ContactPhones)
    .FirstOrDefaultAsync(x => x.Id == id);
```

---

## Primary Contact

Each asset holder can designate **one primary address** and **one primary phone number** through the `IsPrimary` boolean flag.

- When displaying an asset holder's contact summary, the primary address and primary phone are shown by default.
- Multiple addresses and phones can exist per entity, but only one of each should have `IsPrimary = true`.
- The primary flag is managed by the admin when creating or updating contact records.

---

## Authorization

Both controllers follow the same two-tier authorization pattern.

### AddressController

**File**: `Controllers/Support/AddressController.cs`

#### Class-Level Permission (Read Access)

```csharp
[RequirePermission(Auth0Permissions.ReadClients)]
public class AddressController : BaseController<Address>
```

This grants **read access** to any authenticated user with the `ReadClients` permission. In practice, this includes **admins** and **managers**.

#### Method-Level Role Restrictions (Write Access)

All write operations require the **Admin** role:

```csharp
[RequireRole(Auth0Roles.Admin)]
public override async Task<ActionResult<Address>> Post([FromBody] Address entity)

[RequireRole(Auth0Roles.Admin)]
public override async Task<ActionResult<Address>> Put(Guid id, [FromBody] Address entity)

[RequireRole(Auth0Roles.Admin)]
public override async Task<ActionResult<Address>> Delete(Guid id)
```

### ContactPhoneController

**File**: `Controllers/Support/ContactPhoneController.cs`

#### Class-Level Permission (Read Access)

```csharp
[RequirePermission(Auth0Permissions.ReadClients)]
public class ContactPhoneController : BaseController<ContactPhone>
```

Same as AddressController — **admins** and **managers** can read.

#### Method-Level Role Restrictions (Write Access)

```csharp
[RequireRole(Auth0Roles.Admin)]
public override async Task<ActionResult<ContactPhone>> Post([FromBody] ContactPhone entity)

[RequireRole(Auth0Roles.Admin)]
public override async Task<ActionResult<ContactPhone>> Put(Guid id, [FromBody] ContactPhone entity)

[RequireRole(Auth0Roles.Admin)]
public override async Task<ActionResult<ContactPhone>> Delete(Guid id)
```

### Authorization Summary (Both Controllers)

| Role | GET (List/Detail) | POST (Create) | PUT (Update) | DELETE (Remove) |
|------|-------------------|---------------|--------------|-----------------|
| **Admin** | Yes | Yes | Yes | Yes |
| **Manager** | Yes | No | No | No |
| **Partner** | No | No | No | No |

---

## API Endpoints

### Address Endpoints

| Method | Endpoint | Authorization | Description |
|--------|----------|---------------|-------------|
| GET | `/api/v1/address` | `ReadClients` permission | List all addresses |
| GET | `/api/v1/address/{id}` | `ReadClients` permission | Get a single address by ID |
| POST | `/api/v1/address` | `Admin` role | Create a new address |
| PUT | `/api/v1/address/{id}` | `Admin` role | Update an existing address |
| DELETE | `/api/v1/address/{id}` | `Admin` role | Delete an address |

### ContactPhone Endpoints

| Method | Endpoint | Authorization | Description |
|--------|----------|---------------|-------------|
| GET | `/api/v1/contactphone` | `ReadClients` permission | List all contact phones |
| GET | `/api/v1/contactphone/{id}` | `ReadClients` permission | Get a single contact phone by ID |
| POST | `/api/v1/contactphone` | `Admin` role | Create a new contact phone |
| PUT | `/api/v1/contactphone/{id}` | `Admin` role | Update an existing contact phone |
| DELETE | `/api/v1/contactphone/{id}` | `Admin` role | Delete a contact phone |

---

## Service Layer

### AddressService

**File**: `Services/Support/AddressService.cs`

```csharp
public class AddressService : BaseService<Address>
{
    // Inherits standard CRUD from BaseService<Address>
    // - List(), Get(id), Create(entity), Update(id, entity), Delete(id)
}
```

### ContactPhoneService

**File**: `Services/Support/ContactPhoneService.cs`

```csharp
public class ContactPhoneService : BaseService<ContactPhone>
{
    // Inherits standard CRUD from BaseService<ContactPhone>
    // - List(), Get(id), Create(entity), Update(id, entity), Delete(id)
}
```

Both services rely entirely on the `BaseService<T>` CRUD implementation without additional custom logic.

---

## Related Documentation

- [ENTITY_INFRASTRUCTURE.md](../03_CORE_SYSTEMS/ENTITY_INFRASTRUCTURE.md) — BaseAssetHolder hierarchy and entity types (Client, Bank, Member, Manager)
- [AUTHENTICATION.md](../02_ARCHITECTURE/AUTHENTICATION.md) — Auth0 roles, permissions, and the `RequirePermission`/`RequireRole` attribute system
