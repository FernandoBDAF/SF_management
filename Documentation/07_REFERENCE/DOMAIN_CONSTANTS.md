# Domain Constants

## Overview

This document catalogs domain-level constants used throughout the SF Management system. These include system timeline boundaries, authentication claims and roles, permission strings, and cache duration values. Each constant is documented with its source file, value, and the design reasoning behind it.

---

## Table of Contents

1. [SystemImplementation](#systemimplementation)
2. [Auth0 Constants](#auth0-constants)
   - [Custom Claims](#custom-claims)
   - [Roles](#roles)
   - [Permissions](#permissions)
3. [Cache Constants](#cache-constants)
4. [Related Documentation](#related-documentation)

---

## SystemImplementation

**File:** `Domain/Common/SystemImplementation.cs`

```csharp
public static class SystemImplementation
{
    public static readonly DateTime FinanceDataStartDateUtc =
        new(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);
}
```

### FinanceDataStartDateUtc

| Property | Value |
|----------|-------|
| **Value** | `2026-02-01 00:00:00 UTC` |
| **Type** | `DateTime` (Kind = Utc) |
| **Scope** | All finance-related queries |

**Purpose:** Defines the earliest date considered valid for finance logic. All historical financial queries are bounded by this date.

**Used by:**

| Service | Usage |
|---------|-------|
| `AvgRateService` | Backward lookback floor — the iterative AvgRate algorithm stops walking backward when it reaches a month before this date |
| `ProfitCalculationService` | Default `startDate` when profit endpoints are called without a date parameter |
| `ProfitController` | Date parameter defaulting when `startDate` is omitted from API requests |

**Design decision:** The system started tracking financial data from this date. Queries before this date would return inaccurate or incomplete results because the required transaction history does not exist. If a historical data migration extends the earliest data point, this value should be updated accordingly.

**Update guidance:** Changing this value affects all finance API default date ranges and AvgRate lookback depth. Test thoroughly after any change.

---

## Auth0 Constants

### Custom Claims

**File:** `Infrastructure/Authorization/Auth0AuthorizationAttributes.cs`

```csharp
private const string RolesClaim = "https://www.semprefichas.com.br/roles";
```

| Claim | Namespace | Purpose |
|-------|-----------|---------|
| Roles | `https://www.semprefichas.com.br/roles` | Custom Auth0 namespace for role claims in JWT tokens |
| Email | `https://www.semprefichas.com.br/email` | Custom namespace for email claims (fallback from standard) |

The custom namespace is required by Auth0 to avoid conflicts with standard OIDC claims. It is injected into access tokens via an Auth0 Action during login.

Roles are read from **both** the standard `ClaimTypes.Role` and the custom `RolesClaim` to support different token configurations.

### Roles

**File:** `Infrastructure/Authorization/Auth0AuthorizationAttributes.cs`

```csharp
public static class Auth0Roles
{
    public const string Admin = "admin";
    public const string Manager = "manager";
    public const string User = "user";
    public const string Viewer = "viewer";
}
```

| Role | Constant | Status | Description |
|------|----------|--------|-------------|
| `admin` | `Auth0Roles.Admin` | **Active** | Full system access. Auto-bypasses all permission checks. |
| `manager` | `Auth0Roles.Manager` | **Active** | Operational access to entities and transactions with restrictions. |
| `partner` | *(configured in Auth0)* | **Active** | Read-only financial view. Not defined as a code constant — exists as an Auth0 role and maps to `read:financial_data` and entity read permissions. |
| `user` | `Auth0Roles.User` | **Deferred** | Planned for future use. |
| `viewer` | `Auth0Roles.Viewer` | **Deferred** | Planned for future use. |

**Admin auto-bypass:** The `PermissionAuthorizationHandler` checks if the user has the `admin` role before evaluating any permission requirement. If admin, the requirement is automatically satisfied:

```csharp
if (roles.Contains(Auth0Roles.Admin, StringComparer.OrdinalIgnoreCase))
{
    context.Succeed(requirement);
    return Task.CompletedTask;
}
```

### Permissions

**File:** `Infrastructure/Authorization/Auth0AuthorizationAttributes.cs`

All permissions follow the `action:resource` pattern and are defined as constants in the `Auth0Permissions` class.

#### User Management

| Constant | Value |
|----------|-------|
| `Auth0Permissions.ReadUsers` | `"read:users"` |
| `Auth0Permissions.CreateUsers` | `"create:users"` |
| `Auth0Permissions.UpdateUsers` | `"update:users"` |
| `Auth0Permissions.DeleteUsers` | `"delete:users"` |

#### Client Management

| Constant | Value |
|----------|-------|
| `Auth0Permissions.ReadClients` | `"read:clients"` |
| `Auth0Permissions.CreateClients` | `"create:clients"` |
| `Auth0Permissions.UpdateClients` | `"update:clients"` |
| `Auth0Permissions.DeleteClients` | `"delete:clients"` |

#### Member Management

| Constant | Value |
|----------|-------|
| `Auth0Permissions.ReadMembers` | `"read:members"` |
| `Auth0Permissions.CreateMembers` | `"create:members"` |
| `Auth0Permissions.UpdateMembers` | `"update:members"` |
| `Auth0Permissions.DeleteMembers` | `"delete:members"` |

#### Bank Management

| Constant | Value |
|----------|-------|
| `Auth0Permissions.ReadBanks` | `"read:banks"` |
| `Auth0Permissions.CreateBanks` | `"create:banks"` |
| `Auth0Permissions.UpdateBanks` | `"update:banks"` |
| `Auth0Permissions.DeleteBanks` | `"delete:banks"` |

#### Poker Manager Management

| Constant | Value |
|----------|-------|
| `Auth0Permissions.ReadManagers` | `"read:managers"` |
| `Auth0Permissions.CreateManagers` | `"create:managers"` |
| `Auth0Permissions.UpdateManagers` | `"update:managers"` |
| `Auth0Permissions.DeleteManagers` | `"delete:managers"` |

#### Transaction Management

| Constant | Value |
|----------|-------|
| `Auth0Permissions.ReadTransactions` | `"read:transactions"` |
| `Auth0Permissions.CreateTransactions` | `"create:transactions"` |
| `Auth0Permissions.UpdateTransactions` | `"update:transactions"` |
| `Auth0Permissions.DeleteTransactions` | `"delete:transactions"` |

#### Financial Data

| Constant | Value |
|----------|-------|
| `Auth0Permissions.ReadFinancialData` | `"read:financial_data"` |
| `Auth0Permissions.CreateFinancialData` | `"create:financial_data"` |
| `Auth0Permissions.UpdateFinancialData` | `"update:financial_data"` |
| `Auth0Permissions.DeleteFinancialData` | `"delete:financial_data"` |

#### Import Management

| Constant | Value |
|----------|-------|
| `Auth0Permissions.ReadImports` | `"read:imports"` |
| `Auth0Permissions.CreateImports` | `"create:imports"` |
| `Auth0Permissions.DeleteImports` | `"delete:imports"` |

#### Category Management

| Constant | Value |
|----------|-------|
| `Auth0Permissions.ReadCategories` | `"read:categories"` |
| `Auth0Permissions.CreateCategories` | `"create:categories"` |
| `Auth0Permissions.UpdateCategories` | `"update:categories"` |
| `Auth0Permissions.DeleteCategories` | `"delete:categories"` |

#### Wallet Management

| Constant | Value |
|----------|-------|
| `Auth0Permissions.ReadWallets` | `"read:wallets"` |
| `Auth0Permissions.CreateWallets` | `"create:wallets"` |
| `Auth0Permissions.UpdateWallets` | `"update:wallets"` |
| `Auth0Permissions.DeleteWallets` | `"delete:wallets"` |

#### Settlement Management

| Constant | Value |
|----------|-------|
| `Auth0Permissions.ReadSettlements` | `"read:settlements"` |
| `Auth0Permissions.CreateSettlements` | `"create:settlements"` |

#### Other Permissions

| Constant | Value | Status |
|----------|-------|--------|
| `Auth0Permissions.ReadBalances` | `"read:balances"` | Active |
| `Auth0Permissions.ReadDiagnostics` | `"read:diagnostics"` | Active |
| `Auth0Permissions.ReadLedger` | `"read:ledger"` | Planned |

#### Planned Finance Module Permissions

| Constant | Value | Status |
|----------|-------|--------|
| `Auth0Permissions.ReadInvoices` | `"read:invoices"` | Planned |
| `Auth0Permissions.CreateInvoices` | `"create:invoices"` | Planned |
| `Auth0Permissions.UpdateInvoices` | `"update:invoices"` | Planned |
| `Auth0Permissions.DeleteInvoices` | `"delete:invoices"` | Planned |
| `Auth0Permissions.ReadExpenses` | `"read:expenses"` | Planned |
| `Auth0Permissions.CreateExpenses` | `"create:expenses"` | Planned |
| `Auth0Permissions.UpdateExpenses` | `"update:expenses"` | Planned |
| `Auth0Permissions.DeleteExpenses` | `"delete:expenses"` | Planned |

---

## Cache Constants

Cache durations are defined inline within their respective services. This section consolidates all cache TTL values for reference.

### AvgRate Caches

**Service:** `AvgRateService`

| Cache Key Pattern | TTL | Scope | Notes |
|-------------------|-----|-------|-------|
| `AvgRate:{managerId}:{year}:{month}` | 24 hours | Per manager, per completed month | Invalidated cascade from affected month to current on transaction CUD |
| `avgrate.manager-wallet-ids:{managerId}` | 10 minutes | Per manager | Invalidated on wallet create/delete |
| `avgrate.initial-balance:{managerId}` | 10 minutes | Per manager | Invalidated on InitialBalance change |
| `avgrate.first-month:{managerId}:{year}:{month}` | 10 minutes | Per manager, per month | First calculable month detection |

**Current month AvgRate is never cached** — it is always calculated dynamically via `CalculateAvgRateUpToDate()` to reflect the latest transactions.

### Profit Calculation Caches

**Service:** `ProfitCalculationService`

| Cache Key Pattern | TTL | Scope | Notes |
|-------------------|-----|-------|-------|
| `finance.system-wallet-ids` | 10 minutes | Global | System wallets rarely change |
| `profit.rake-manager-ids` | 10 minutes | Global | RakeOverrideCommission manager IDs |
| `profit.spread-manager-ids` | 10 minutes | Global | Spread manager IDs |

### Lookup Caches

**Service:** `CachedLookupService`

| Cache Key | TTL | Scope | Notes |
|-----------|-----|-------|-------|
| `lookups.poker-manager-ids` | 10 minutes | Global | `HashSet<Guid>` for O(1) lookup |

### Cache Duration Summary

| Duration | Used For |
|----------|----------|
| **24 hours** | Completed month AvgRate snapshots (historical data that doesn't change) |
| **10 minutes** | Entity lookups, wallet IDs, system wallet IDs, manager profit type IDs |
| **Not cached** | Current month AvgRate (always dynamic) |
| **60 seconds** | Balance endpoint response cache (`[ResponseCache]` attribute) |

### Cache Invalidation Triggers

```
Transaction CUD (create/update/delete)
    → AvgRate:{managerId}:{year}:{month} (cascade from affected month to current)

PokerManager CUD
    → lookups.poker-manager-ids
    → profit.rake-manager-ids
    → profit.spread-manager-ids

Wallet CUD
    → avgrate.manager-wallet-ids:{managerId}
    → finance.system-wallet-ids
```

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [AUTHENTICATION.md](../05_INFRASTRUCTURE/AUTHENTICATION.md) | Full Auth0 integration, RBAC matrix, authorization handlers |
| [CACHING_STRATEGY.md](../05_INFRASTRUCTURE/CACHING_STRATEGY.md) | Cache key inventory, invalidation patterns, roadmap |
| [PROFIT_CALCULATION_SYSTEM.md](../03_CORE_SYSTEMS/PROFIT_CALCULATION_SYSTEM.md) | AvgRate algorithm and profit calculation pipeline |
| [ENUMS_AND_TYPE_SYSTEM.md](ENUMS_AND_TYPE_SYSTEM.md) | All enumeration values and business meanings |
| [CONFIGURATION_MANAGEMENT.md](../05_INFRASTRUCTURE/CONFIGURATION_MANAGEMENT.md) | Application settings and environment variables |
| [DATETIME_STANDARDS.md](../05_INFRASTRUCTURE/DATETIME_STANDARDS.md) | UTC storage conventions and timezone handling |

---

*Created: February 27, 2026*
