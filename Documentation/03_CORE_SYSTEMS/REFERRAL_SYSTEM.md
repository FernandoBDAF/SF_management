# Referral System

## Overview

The Referral System tracks commission-based relationships between asset holders and wallet identifiers. When one entity (the referrer) introduces or manages another entity's poker wallet, they can receive a commission on transactions involving that wallet. This system is essential for managing revenue sharing in the poker management business.

---

## Table of Contents

1. [Core Concepts](#core-concepts)
2. [Data Model](#data-model)
3. [Business Rules](#business-rules)
4. [Service Methods](#service-methods)
5. [Usage Examples](#usage-examples)
6. [Integration Points](#integration-points)

---

## Core Concepts

### What is a Referral?

A **Referral** represents a commission relationship where:
- A **Referrer** (BaseAssetHolder) has introduced or is managing a poker player's wallet
- The **Referred** wallet (WalletIdentifier) generates commission for the referrer
- Commission is calculated as a percentage (`ParentCommission`) of transactions

### Key Relationships

```
┌─────────────────────┐
│  BaseAssetHolder    │
│  (Referrer)         │
│                     │
│  e.g., PokerManager │
└──────────┬──────────┘
           │
           │ creates
           │
           ▼
┌─────────────────────┐         ┌─────────────────────┐
│     Referral        │         │   WalletIdentifier  │
│                     │────────▶│   (Referred Wallet) │
│ - ParentCommission  │         │                     │
│ - ActiveFrom        │         │  Owned by different │
│ - ActiveUntil       │         │  BaseAssetHolder    │
└─────────────────────┘         └─────────────────────┘
```

### Example Scenario

1. **PokerManager** "John" manages poker player **Client** "Alice"
2. Alice has a PokerStars wallet (`WalletIdentifier`)
3. John creates a **Referral** on Alice's wallet with 15% commission
4. When Alice wins and transactions occur, John receives 15% commission

---

## Data Model

### Referral Entity

**File**: `Models/Support/Referral.cs`

```csharp
public class Referral : BaseDomain
{
    // The referrer (who receives the commission)
    [Required] 
    public Guid AssetHolderId { get; set; }
    public virtual BaseAssetHolder AssetHolder { get; set; }

    // The wallet being referred (owned by another entity)
    [Required] 
    public Guid WalletIdentifierId { get; set; }
    public virtual WalletIdentifier WalletIdentifier { get; set; }
    
    // Commission percentage (0-100)
    [Precision(18, 4)]
    [Range(0, 100)]
    public decimal? ParentCommission { get; set; }
    
    // Validity period
    public DateTime? ActiveFrom { get; set; }
    public DateTime? ActiveUntil { get; set; }
}
```

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `AssetHolderId` | Guid | The referrer's BaseAssetHolder ID |
| `WalletIdentifierId` | Guid | The wallet receiving the referral |
| `ParentCommission` | decimal? | Commission percentage (0-100) |
| `ActiveFrom` | DateTime? | Start date (null = from beginning) |
| `ActiveUntil` | DateTime? | End date (null = never expires) |

### Helper Properties and Methods

```csharp
// Check if referral is currently active
public bool IsActive => 
    (ActiveFrom == null || DateTime.UtcNow >= ActiveFrom) &&
    (ActiveUntil == null || DateTime.UtcNow <= ActiveUntil) && 
    !DeletedAt.HasValue;

// Check if active at a specific date
public bool IsActiveAt(DateTime date) =>
    (ActiveFrom == null || date >= ActiveFrom) &&
    (ActiveUntil == null || date <= ActiveUntil) && 
    !DeletedAt.HasValue;

// Get the wallet owner (referred party)
public BaseAssetHolder? ReferredAssetHolder => 
    WalletIdentifier?.AssetPool?.BaseAssetHolder;
```

---

## Business Rules

### Rule 1: One Active Referral Per Wallet

Only one referral can be active for a wallet identifier at any given time. When creating a new referral:

```csharp
// Check for existing active referral and deactivate it
var existingActiveReferral = await context.Referrals
    .FirstOrDefaultAsync(r => 
        r.WalletIdentifierId == walletIdentifierId && 
        (r.ActiveFrom == null || r.ActiveFrom <= referralActiveFrom) &&
        (r.ActiveUntil == null || r.ActiveUntil > referralActiveFrom) &&
        !r.DeletedAt.HasValue);

if (existingActiveReferral != null)
{
    // Deactivate by setting ActiveUntil to day before new referral
    existingActiveReferral.ActiveUntil = referralActiveFrom.AddDays(-1);
}
```

### Rule 2: No Self-Referrals

An entity cannot create a referral on their own wallet:

```csharp
if (walletIdentifier.AssetPool.BaseAssetHolderId == referrerAssetHolderId)
    throw new ArgumentException("Cannot create self-referral");
```

### Rule 3: Valid Entities Required

Both the referrer and wallet must exist and not be deleted:

```csharp
var referrer = await context.BaseAssetHolders
    .FirstOrDefaultAsync(bah => bah.Id == referrerAssetHolderId && !bah.DeletedAt.HasValue);

if (referrer == null)
    throw new ArgumentException($"Referrer BaseAssetHolder not found");

var walletIdentifier = await context.WalletIdentifiers
    .FirstOrDefaultAsync(wi => wi.Id == walletIdentifierId && !wi.DeletedAt.HasValue);

if (walletIdentifier == null)
    throw new ArgumentException($"WalletIdentifier not found");
```

### Rule 4: Commission Range

Commission percentage must be between 0 and 100:

```csharp
[Range(0, 100)]
public decimal? ParentCommission { get; set; }
```

---

## Service Methods

### ReferralService

**File**: `Services/ReferralService.cs`

#### CreateReferral

Creates a new referral, automatically deactivating any existing active referral.

```csharp
public async Task<Referral> CreateReferral(
    Guid referrerAssetHolderId, 
    Guid walletIdentifierId, 
    decimal commissionPercentage, 
    DateTime? activeFrom = null, 
    DateTime? activeUntil = null)
```

**Parameters:**
| Parameter | Description |
|-----------|-------------|
| `referrerAssetHolderId` | The BaseAssetHolder who will receive commission |
| `walletIdentifierId` | The wallet to attach the referral to |
| `commissionPercentage` | Commission rate (0-100) |
| `activeFrom` | Optional start date (defaults to now) |
| `activeUntil` | Optional end date (null = never expires) |

#### GetReferralsMadeBy

Returns all referrals created by a specific asset holder:

```csharp
public async Task<List<Referral>> GetReferralsMadeBy(Guid assetHolderId)
```

#### GetReferralsReceivedBy

Returns all referrals on wallets owned by a specific asset holder:

```csharp
public async Task<List<Referral>> GetReferralsReceivedBy(Guid assetHolderId)
```

#### GetActiveReferralForWallet

Returns the currently active referral for a wallet:

```csharp
public async Task<Referral?> GetActiveReferralForWallet(
    Guid walletIdentifierId, 
    DateTime? atDate = null)
```

#### DeactivateActiveReferral

Deactivates the current referral for a wallet:

```csharp
public async Task<bool> DeactivateActiveReferral(
    Guid walletIdentifierId, 
    DateTime deactivationDate)

public async Task<bool> DeactivateActiveReferralNow(Guid walletIdentifierId)
```

---

## Usage Examples

### Creating a New Referral

```csharp
var referralService = serviceProvider.GetRequiredService<ReferralService>();

// Create referral with 15% commission starting now
var referral = await referralService.CreateReferral(
    referrerAssetHolderId: pokerManagerId,
    walletIdentifierId: clientPokerWalletId,
    commissionPercentage: 15m
);

// Create referral with specific date range
var timedReferral = await referralService.CreateReferral(
    referrerAssetHolderId: pokerManagerId,
    walletIdentifierId: clientPokerWalletId,
    commissionPercentage: 10m,
    activeFrom: new DateTime(2025, 1, 1),
    activeUntil: new DateTime(2025, 12, 31)
);
```

### Querying Referrals

```csharp
// Get all referrals a poker manager has created
var myReferrals = await referralService.GetReferralsMadeBy(pokerManagerId);

// Get all referrals on my wallets (who's getting commission from me?)
var referralsOnMe = await referralService.GetReferralsReceivedBy(clientId);

// Get the active referral for a specific wallet
var activeReferral = await referralService.GetActiveReferralForWallet(walletId);
if (activeReferral != null)
{
    Console.WriteLine($"Referrer: {activeReferral.AssetHolder.Name}");
    Console.WriteLine($"Commission: {activeReferral.ParentCommission}%");
}
```

### Ending a Referral Relationship

```csharp
// End referral immediately
await referralService.DeactivateActiveReferralNow(walletId);

// End referral at specific date
await referralService.DeactivateActiveReferral(walletId, new DateTime(2025, 6, 30));
```

---

## Integration Points

### With PokerManager Controller

The referral information is included in wallet responses:

```csharp
// From PokerManagerController.GetWalletIdentifiersFromOthers
walletIdentifierResponses.Add(new WalletIdentifierWithAssetHolderResponse
{
    Id = walletIdentifier.Id,
    // ... other properties
    Referral = walletIdentifier.Referrals.FirstOrDefault() != null 
        ? new ReferralInfo
        {
            Id = walletIdentifier.Referrals.First().Id,
            AssetHolderId = walletIdentifier.Referrals.First().AssetHolderId,
            Name = walletIdentifier.Referrals.First().AssetHolder.Name,
            ActiveUntil = walletIdentifier.Referrals.First().ActiveUntil,
            ParentCommission = walletIdentifier.Referrals.First().ParentCommission
        } 
        : null
});
```

### With BaseAssetHolder

BaseAssetHolder tracks referrals made:

```csharp
public class BaseAssetHolder : BaseDomain
{
    // Collection of referrals made by this asset holder
    public virtual ICollection<Referral> ReferralsMade { get; set; } = [];
}
```

### With WalletIdentifier

WalletIdentifier tracks referrals received:

```csharp
public class WalletIdentifier : BaseDomain
{
    // Collection of referrals on this wallet
    public virtual ICollection<Referral> Referrals { get; set; } = [];
}
```

### With Settlement Transactions

Referral information can be used during settlement calculations:

```csharp
// When creating a settlement, check for active referral
var activeReferral = await referralService.GetActiveReferralForWallet(
    walletIdentifierId, 
    settlementDate);

if (activeReferral != null && activeReferral.ParentCommission.HasValue)
{
    var commission = amount * (activeReferral.ParentCommission.Value / 100);
    // Apply commission to referrer
}
```

---

## Database Schema

```sql
CREATE TABLE Referrals (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    AssetHolderId UNIQUEIDENTIFIER NOT NULL,    -- FK to BaseAssetHolders
    WalletIdentifierId UNIQUEIDENTIFIER NOT NULL, -- FK to WalletIdentifiers
    ParentCommission DECIMAL(18,4),
    ActiveFrom DATETIME2,
    ActiveUntil DATETIME2,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2,
    DeletedAt DATETIME2,
    CreatedBy NVARCHAR(255),
    LastModifiedBy NVARCHAR(255),
    DeletedBy NVARCHAR(255),
    
    FOREIGN KEY (AssetHolderId) REFERENCES BaseAssetHolders(Id),
    FOREIGN KEY (WalletIdentifierId) REFERENCES WalletIdentifiers(Id)
);
```

---

## Best Practices

### 1. Always Check for Active Referral

Before calculating commissions, verify the referral is active:

```csharp
var referral = await referralService.GetActiveReferralForWallet(walletId, transactionDate);
if (referral?.IsActiveAt(transactionDate) == true)
{
    // Apply commission
}
```

### 2. Use Date-Specific Queries

When processing historical data, use the date parameter:

```csharp
// For historical settlement
var referralAtSettlement = await referralService.GetActiveReferralForWallet(
    walletId, 
    settlementDate
);
```

### 3. Handle Referral Transitions

When changing referrers, the old referral is automatically deactivated:

```csharp
// This deactivates old referral and creates new one
await referralService.CreateReferral(newReferrerId, walletId, 12m);
```

### 4. Include Related Data When Needed

Use appropriate includes for navigation properties:

```csharp
var referrals = await context.Referrals
    .Include(r => r.AssetHolder)
    .Include(r => r.WalletIdentifier)
        .ThenInclude(wi => wi.AssetPool)
        .ThenInclude(ap => ap.BaseAssetHolder)
    .ToListAsync();
```

---

## Related Documentation

- [ASSET_INFRASTRUCTURE.md](ASSET_INFRASTRUCTURE.md) - WalletIdentifier details
- [ENTITY_INFRASTRUCTURE.md](ENTITY_INFRASTRUCTURE.md) - BaseAssetHolder details
- [SETTLEMENT_WORKFLOW.md](SETTLEMENT_WORKFLOW.md) - Settlement calculations with commissions

