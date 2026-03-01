# Initial Balances System

## Overview

The Initial Balances system allows setting starting balance values for asset holders, enabling accurate balance calculations when historical transaction data is incomplete or when migrating from another system. Initial balances serve as the baseline from which all subsequent transaction-based balance calculations are derived.

The system supports three modes of operation: setting balances per specific asset type, per asset group, or via a unified auto-detection endpoint. Initial balance configuration also determines how average rate (AvgRate) calculations behave for the given asset holder.

---

## Data Model

### InitialBalance Entity

**File**: `Models/Support/InitialBalance.cs`

```csharp
public class InitialBalance : BaseDomain
{
    public Guid BaseAssetHolderId { get; set; }
    public AssetType AssetType { get; set; }
    public AssetGroup AssetGroup { get; set; }
    public decimal Balance { get; set; }
    public AssetType? BalanceAs { get; set; }
    public decimal? ConversionRate { get; set; }
    public string? Description { get; set; }
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
| `BaseAssetHolderId` | `Guid` | The asset holder this initial balance belongs to |
| `AssetType` | `AssetType` | Specific asset type (e.g., `PokerStars`, `Bet365`). Set to `None` when using AssetGroup mode |
| `AssetGroup` | `AssetGroup` | Asset group (e.g., `PokerAssets`, `BettingAssets`). Set to `None` when using AssetType mode |
| `Balance` | `decimal` | The starting balance amount |
| `BalanceAs` | `AssetType?` | Optional: express this balance as a different asset type (for conversions) |
| `ConversionRate` | `decimal?` | Optional: the conversion factor when `BalanceAs` is specified |
| `Description` | `string?` | Optional note explaining the initial balance entry |

---

## Authorization

### Controller: `InitialBalanceController`

**File**: `Controllers/Support/InitialBalanceController.cs`

The initial balance controller uses a two-tier authorization strategy:

#### Class-Level Permission (Read Access)

```csharp
[RequirePermission(Auth0Permissions.ReadBalances)]
public class InitialBalanceController : BaseController<InitialBalance>
```

This grants **read access** to any authenticated user with the `ReadBalances` permission. In practice, this includes **admins**, **managers**, and **partners** (partners can view balance information relevant to their own data).

#### Method-Level Role Restrictions (Write Access)

All write and validation operations require the **Admin** role:

```csharp
[RequireRole(Auth0Roles.Admin)]
public async Task<ActionResult<InitialBalance>> Post([FromBody] InitialBalance entity)

[RequireRole(Auth0Roles.Admin)]
public async Task<ActionResult> Delete(Guid id)

[RequireRole(Auth0Roles.Admin)]
public async Task<ActionResult<ValidationResult>> Validate([FromBody] InitialBalance entity)
```

### Authorization Summary

| Role | GET (List/Detail) | POST (Create) | DELETE (Remove) | Validate |
|------|-------------------|---------------|-----------------|----------|
| **Admin** | Yes | Yes | Yes | Yes |
| **Manager** | Yes | No | No | No |
| **Partner** | Yes | No | No | No |

---

## API Endpoints

| Method | Endpoint | Authorization | Description |
|--------|----------|---------------|-------------|
| POST | `/api/v1/initialbalance/asset-type` | `Admin` role | Set initial balance for a specific AssetType |
| POST | `/api/v1/initialbalance/asset-group` | `Admin` role | Set initial balance for an entire AssetGroup |
| POST | `/api/v1/initialbalance/unified` | `Admin` role | Auto-detect and set initial balance |
| POST | `/api/v1/initialbalance/validate` | `Admin` role | Validate an initial balance request before creating |
| GET | `/api/v1/initialbalance/asset-holder/{id}` | `ReadBalances` permission | Get all initial balances for an asset holder |
| GET | `/api/v1/initialbalance/asset-holder/{id}/summary` | `ReadBalances` permission | Get summarized initial balance data for an asset holder |
| DELETE | `/api/v1/initialbalance/asset-holder/{id}/asset-type/{type}` | `Admin` role | Remove an initial balance entry |

---

## Three Balance Modes

### 1. By AssetType

Sets an initial balance for a **specific asset** (e.g., PokerStars, Bet365). Use this when you know the exact starting balance for an individual platform or account.

```csharp
await _service.SetInitialBalance(
    baseAssetHolderId,
    AssetType.PokerStars,
    balance: 5000m,
    balanceAs: null,
    conversionRate: null
);
```

**Resulting record**:
- `AssetType = PokerStars`
- `AssetGroup = None`

### 2. By AssetGroup

Sets an initial balance for an **entire group of assets** (e.g., all PokerAssets). Use this when the starting balance is known only at the group level, not broken down by individual asset type.

```csharp
await _service.SetInitialBalanceForAssetGroup(
    baseAssetHolderId,
    AssetGroup.PokerAssets,
    balance: 25000m,
    balanceAs: null,
    conversionRate: null,
    description: "Migration from legacy system"
);
```

**Resulting record**:
- `AssetType = None`
- `AssetGroup = PokerAssets`

### 3. Unified

The unified endpoint **auto-detects** whether the initial balance should be applied at the AssetType or AssetGroup level based on the provided payload. This simplifies client code by providing a single entry point that routes to the appropriate mode internally.

---

## Validation Endpoint

**Endpoint**: `POST /api/v1/initialbalance/validate`

Before creating an initial balance, clients can submit the payload to the validation endpoint to check for issues without persisting any data. This is useful for:

- Verifying that the asset holder exists
- Checking for conflicts with existing initial balances
- Validating conversion rate and `BalanceAs` consistency
- Providing user feedback in forms before final submission

The validation endpoint returns a `ValidationResult` indicating success or a list of validation errors.

---

## AvgRate Integration

The initial balance configuration for an asset holder determines how **AvgRate** (Average Rate) calculations are performed.

When initial balances are set **by AssetType** (individual assets), the AvgRate system uses `PerAssetType` calculation mode, computing separate average rates for each asset type.

When initial balances are set **by AssetGroup**, the AvgRate system uses `Consolidated` calculation mode, computing a single average rate across the entire group.

This distinction is important because it affects how conversion rates and portfolio performance metrics are reported for the asset holder.

---

## Balance Calculation

Initial balances are the **starting point** for all balance calculations in the system. The `BaseAssetHolderService` uses initial balances as the base upon which transaction-derived balances are accumulated.

### GetBalancesByAssetType

```csharp
public async Task<Dictionary<AssetType, decimal>> GetBalancesByAssetType(Guid id)
{
    var balances = new Dictionary<AssetType, decimal>();

    // Step 1: Seed with initial balances
    foreach (var initialBalance in assetHolder.InitialBalances)
    {
        if (initialBalance.AssetType != AssetType.None)
            balances[initialBalance.AssetType] = initialBalance.Balance;
    }

    // Step 2: Add transaction-derived balances on top
    // ...
}
```

### GetBalancesByAssetGroup

```csharp
public async Task<Dictionary<AssetGroup, decimal>> GetBalancesByAssetGroup(Guid id)
{
    var balances = new Dictionary<AssetGroup, decimal>();

    // Step 1: Seed with initial balances at the group level
    foreach (var initialBalance in assetHolder.InitialBalances)
    {
        if (initialBalance.AssetGroup != AssetGroup.None)
            balances[initialBalance.AssetGroup] = initialBalance.Balance;
    }

    // Step 2: Add transaction-derived balances on top
    // ...
}
```

Both methods follow the same pattern: initial balances provide the baseline, and all subsequent deposits, withdrawals, and transfers modify the running total from that starting point. Without initial balances, the system assumes a zero starting balance.

---

## Related Documentation

- [ASSET_INFRASTRUCTURE.md](../03_CORE_SYSTEMS/ASSET_INFRASTRUCTURE.md) — Asset types, asset groups, and the asset holder hierarchy
- [SERVICE_LAYER_ARCHITECTURE.md](../02_ARCHITECTURE/SERVICE_LAYER_ARCHITECTURE.md) — Balance calculation methods in BaseAssetHolderService
- [AUTHENTICATION.md](../02_ARCHITECTURE/AUTHENTICATION.md) — Auth0 roles, permissions, and the `RequirePermission`/`RequireRole` attribute system
