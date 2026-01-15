# Initial Balances System

## Overview

The Initial Balances system allows setting starting balance values for asset holders, enabling accurate balance calculations when historical transaction data is incomplete or when migrating from another system.

---

## Data Model

### InitialBalance Entity

**File**: `Models/Support/InitialBalance.cs`

```csharp
public class InitialBalance : BaseDomain
{
    public Guid BaseAssetHolderId { get; set; }
    public AssetType AssetType { get; set; }      // For specific asset type
    public AssetGroup AssetGroup { get; set; }    // For asset group level
    public decimal Balance { get; set; }
    public AssetType? BalanceAs { get; set; }     // Convert to different type
    public decimal? ConversionRate { get; set; }  // Conversion factor
    public string? Description { get; set; }
}
```

---

## Usage Modes

### 1. By AssetType

Set balance for a specific asset (e.g., PokerStars):

```csharp
await _service.SetInitialBalance(
    baseAssetHolderId,
    AssetType.PokerStars,
    balance: 5000m,
    balanceAs: null,
    conversionRate: null
);
```

### 2. By AssetGroup

Set balance for an entire group (e.g., all Poker assets):

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

---

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/initialbalance/asset-type` | Set by AssetType |
| POST | `/api/v1/initialbalance/asset-group` | Set by AssetGroup |
| POST | `/api/v1/initialbalance/unified` | Auto-detect type |
| GET | `/api/v1/initialbalance/asset-holder/{id}` | Get all for holder |
| GET | `/api/v1/initialbalance/asset-holder/{id}/summary` | Get summary |
| DELETE | `/api/v1/initialbalance/asset-holder/{id}/asset-type/{type}` | Remove |

---

## Balance Calculations

Initial balances are included in balance calculations:

```csharp
public async Task<Dictionary<AssetType, decimal>> GetBalancesByAssetType(Guid id)
{
    var balances = new Dictionary<AssetType, decimal>();

    // Include initial balances
    foreach (var initialBalance in assetHolder.InitialBalances)
    {
        if (initialBalance.AssetType != AssetType.None)
            balances[initialBalance.AssetType] = initialBalance.Balance;
    }

    // Add transaction balances
    // ...
}
```

---

## Related Documentation

- [ASSET_INFRASTRUCTURE.md](../03_CORE_SYSTEMS/ASSET_INFRASTRUCTURE.md) - Asset system
- [SERVICE_LAYER_ARCHITECTURE.md](../02_ARCHITECTURE/SERVICE_LAYER_ARCHITECTURE.md) - Balance methods

