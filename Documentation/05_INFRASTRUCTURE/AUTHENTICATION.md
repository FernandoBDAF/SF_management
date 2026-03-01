# Authentication & Authorization

This document describes the authentication and authorization system for the SF Management API. The system uses **Auth0** as the identity provider for secure, standards-based authentication with **Role-Based Access Control (RBAC)**.

> **Last Updated:** February 27, 2026
> **Status:** Implemented — RBAC fully operational

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Configuration](#configuration)
- [Core Components](#core-components)
- [Role-Based Access Control (RBAC)](#role-based-access-control-rbac)
- [Authorization System](#authorization-system)
- [Controller Authorization Matrix](#controller-authorization-matrix)
- [Using Authentication in Code](#using-authentication-in-code)
- [Database Integration](#database-integration)
- [Frontend Integration](#frontend-integration)
- [Security Best Practices](#security-best-practices)
- [Troubleshooting](#troubleshooting)
- [Related Documentation](#related-documentation)

---

## Overview

The SF Management API uses **Auth0** as an external identity provider to handle user authentication. This approach provides:

- **Enterprise-grade security** - Auth0 handles password storage, MFA, and security best practices
- **Scalability** - Authentication scales independently from the application
- **Compliance** - Built-in support for security standards and certifications
- **Flexibility** - Easy integration with social logins, SSO, and enterprise identity providers

## Architecture

### Authentication Flow

```
┌──────────────┐     ┌─────────────┐     ┌──────────────────┐
│   Frontend   │────▶│   Auth0     │────▶│  SF Management   │
│   Client     │◀────│   Login     │◀────│      API         │
└──────────────┘     └─────────────┘     └──────────────────┘
       │                    │                     │
       │  1. Login Request  │                     │
       │───────────────────▶│                     │
       │                    │                     │
       │  2. JWT Token      │                     │
       │◀───────────────────│                     │
       │                    │                     │
       │         3. API Request with Bearer Token │
       │─────────────────────────────────────────▶│
       │                    │                     │
       │                    │  4. Validate JWT    │
       │                    │◀────────────────────│
       │                    │                     │
       │         5. API Response                  │
       │◀─────────────────────────────────────────│
```

**Step-by-step:**

1. The frontend client initiates login through Auth0
2. Auth0 authenticates the user and returns a JWT token
3. The client includes the JWT in the `Authorization` header for API requests
4. The API validates the JWT against Auth0's public keys
5. If valid, the API processes the request and returns a response

### Project Structure

```
SF_management/
├── Authorization/
│   ├── Auth0AuthorizationAttributes.cs  # Custom attributes and constants
│   ├── Auth0AuthorizationHandlers.cs    # Policy handlers
│   └── Auth0UserService.cs              # User context service
├── Middleware/
│   └── AuthenticationLoggingMiddleware.cs
├── Settings/
│   └── Auth0Settings.cs                 # Configuration class
├── StartupConfig/
│   └── DependencyInjectionExtensions.cs # Service registration
└── Data/
    └── DataContext.cs                   # Database integration
```

---

## Configuration

Auth0 settings are configured in `appsettings.json`:

```json
{
  "Auth0": {
    "Domain": "your-tenant.auth0.com",
    "Audience": "https://your-api-identifier",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret"
  }
}
```

| Setting | Description |
|---------|-------------|
| `Domain` | Your Auth0 tenant domain |
| `Audience` | The API identifier registered in Auth0 |
| `ClientId` | Application client ID |
| `ClientSecret` | Application client secret (keep secure) |

> **Security Note:** Never commit secrets to version control. Use environment variables or a secrets manager for production deployments.
>
> **Note:** For complete configuration details including environment variables and User Secrets, see [CONFIGURATION_MANAGEMENT.md](CONFIGURATION_MANAGEMENT.md#auth0-settings).

---

## Core Components

### JWT Bearer Authentication

JWT Bearer authentication is configured in `DependencyInjectionExtensions.cs`:

```csharp
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.Authority = $"https://{builder.Configuration["Auth0:Domain"]}/";
    o.Audience = builder.Configuration["Auth0:Audience"];
    o.RequireHttpsMetadata = false;
    o.SaveToken = true;
});
```

### User Context Service

The `Auth0UserService` extracts user information from JWT claims and makes it available throughout the application:

```csharp
public interface IAuth0UserService
{
    string? GetUserId();
    string? GetUserEmail();
    string? GetUserName();
    List<string> GetUserRoles();
    List<string> GetUserPermissions();
    bool IsAuthenticated();
}
```

**Implementation details:**

| Method | Claim Source |
|--------|--------------|
| `GetUserId()` | `ClaimTypes.NameIdentifier` (Auth0 `sub` claim) |
| `GetUserEmail()` | `ClaimTypes.Email` or custom claim `https://www.semprefichas.com.br/email` |
| `GetUserName()` | `ClaimTypes.Name` |
| `GetUserRoles()` | `ClaimTypes.Role` |
| `GetUserPermissions()` | `permissions` claim |

**Usage in a controller:**

```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class ExampleController : ControllerBase
{
    private readonly IAuth0UserService _auth0UserService;

    public ExampleController(IAuth0UserService auth0UserService)
    {
        _auth0UserService = auth0UserService;
    }

    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        if (!_auth0UserService.IsAuthenticated())
            return Unauthorized();

        return Ok(new
        {
            UserId = _auth0UserService.GetUserId(),
            Email = _auth0UserService.GetUserEmail(),
            Name = _auth0UserService.GetUserName(),
            Roles = _auth0UserService.GetUserRoles(),
            Permissions = _auth0UserService.GetUserPermissions()
        });
    }
}
```

---

## Authorization System

### Default Security Policy

By default, **all endpoints require authentication**. This is enforced through a fallback policy:

```csharp
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
        .RequireAssertion(context =>
        {
            // Allow OPTIONS requests (CORS preflight) without authentication
            if (context.Resource is HttpContext httpContext && 
                httpContext.Request.Method == "OPTIONS")
            {
                return true;
            }
            // Require authentication for all other requests
            return context.User.Identity?.IsAuthenticated == true;
        })
        .Build());
```

**Exceptions:**
- `OPTIONS` requests (CORS preflight) are allowed without authentication
- The `/health` endpoint is marked with `AllowAnonymous`

---

## Role-Based Access Control (RBAC)

The system implements a comprehensive RBAC model with three active roles and fine-grained permissions.

### Roles

| Role | Constant | Description | Scope |
|------|----------|-------------|-------|
| `admin` | `Auth0Roles.Admin` | Full system access | Everything — auto-bypasses all permission checks |
| `manager` | `Auth0Roles.Manager` | Operational access | `/central/*`, `/entidades/*` with restrictions |
| `partner` | `Auth0Roles.Partner` | Read-only financial view | `/financeiro/relatorio`, `/financeiro/consolidado` |

> **Note:** `user` and `viewer` roles exist in the codebase (`Auth0Roles.User`, `Auth0Roles.Viewer`) but are deferred for future implementation. No permissions or policies are currently assigned to these roles.

### Admin Auto-Bypass

The `admin` role automatically passes **all** permission checks. This is implemented in `PermissionAuthorizationHandler`:

```csharp
// Admin bypass - admin role auto-succeeds any permission requirement
var roles = context.User.FindAll(ClaimTypes.Role)
    .Concat(context.User.FindAll(RolesClaim))
    .Select(c => c.Value)
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToList();

if (roles.Contains(Auth0Roles.Admin, StringComparer.OrdinalIgnoreCase))
{
    context.Succeed(requirement);
    return Task.CompletedTask;
}
```

### Custom Roles Claim

Roles are read from both the standard `ClaimTypes.Role` and a custom Auth0 namespace:

```csharp
private const string RolesClaim = "https://www.semprefichas.com.br/roles";
```

This namespace is set via an Auth0 Action that injects roles into the access token during login.

### Permissions

Permissions follow an `action:resource` pattern and are managed in Auth0 Dashboard.

| Resource | Permissions |
|----------|-------------|
| Users | `read:users`, `create:users`, `update:users`, `delete:users` |
| Clients | `read:clients`, `create:clients`, `update:clients`, `delete:clients` |
| Members | `read:members`, `create:members`, `update:members`, `delete:members` |
| Banks | `read:banks`, `create:banks`, `update:banks`, `delete:banks` |
| Poker Managers | `read:managers`, `create:managers`, `update:managers`, `delete:managers` |
| Transactions | `read:transactions`, `create:transactions`, `update:transactions`, `delete:transactions` |
| Imports | `read:imports`, `create:imports`, `delete:imports` |
| Categories | `read:categories`, `create:categories`, `update:categories`, `delete:categories` |
| Wallets | `read:wallets`, `create:wallets`, `update:wallets`, `delete:wallets` |
| Settlements | `read:settlements`, `create:settlements` |
| Balances | `read:balances` |
| Financial Data | `read:financial_data` |
| Diagnostics | `read:diagnostics` |
| Invoices *(planned)* | `read:invoices`, `create:invoices`, `update:invoices`, `delete:invoices` |
| Expenses *(planned)* | `read:expenses`, `create:expenses`, `update:expenses`, `delete:expenses` |
| Ledger *(planned)* | `read:ledger`, `create:ledger_entry`, `close:period` |

### Role-to-Permission Mapping

**Admin:** All permissions (plus auto-bypass)

**Manager:**
- `read:clients`, `create:clients`, `update:clients`, `delete:clients` (full CRUD on clients)
- `read:members`, `read:banks`, `read:managers` (read-only on other entities)
- `read:transactions`, `create:transactions`, `update:transactions`, `delete:transactions`
- `read:wallets`, `create:wallets`
- `read:settlements`, `create:settlements`
- `read:categories`, `read:balances`

**Partner:**
- `read:financial_data`, `read:ledger`
- `read:banks`, `read:clients`, `read:managers`, `read:members`
- `read:balances`

**Permission constants are available in `Auth0Permissions`:**

```csharp
public static class Auth0Permissions
{
    // User management
    public const string ReadUsers = "read:users";
    public const string CreateUsers = "create:users";
    public const string UpdateUsers = "update:users";
    public const string DeleteUsers = "delete:users";
    
    // Client management
    public const string ReadClients = "read:clients";
    public const string CreateClients = "create:clients";
    public const string UpdateClients = "update:clients";
    public const string DeleteClients = "delete:clients";

    // Member management
    public const string ReadMembers = "read:members";
    public const string CreateMembers = "create:members";
    public const string UpdateMembers = "update:members";
    public const string DeleteMembers = "delete:members";

    // Bank management
    public const string ReadBanks = "read:banks";
    public const string CreateBanks = "create:banks";
    public const string UpdateBanks = "update:banks";
    public const string DeleteBanks = "delete:banks";

    // Poker manager management
    public const string ReadManagers = "read:managers";
    public const string CreateManagers = "create:managers";
    public const string UpdateManagers = "update:managers";
    public const string DeleteManagers = "delete:managers";
    
    // Transaction management
    public const string ReadTransactions = "read:transactions";
    public const string CreateTransactions = "create:transactions";
    public const string UpdateTransactions = "update:transactions";
    public const string DeleteTransactions = "delete:transactions";
    
    // Financial data
    public const string ReadFinancialData = "read:financial_data";

    // Categories
    public const string ReadCategories = "read:categories";
    public const string CreateCategories = "create:categories";
    public const string UpdateCategories = "update:categories";
    public const string DeleteCategories = "delete:categories";

    // Wallets
    public const string ReadWallets = "read:wallets";
    public const string CreateWallets = "create:wallets";
    public const string UpdateWallets = "update:wallets";
    public const string DeleteWallets = "delete:wallets";

    // Settlements
    public const string ReadSettlements = "read:settlements";
    public const string CreateSettlements = "create:settlements";

    // Balances
    public const string ReadBalances = "read:balances";

    // Diagnostics
    public const string ReadDiagnostics = "read:diagnostics";

    // Ledger
    public const string ReadLedger = "read:ledger";

    // Imports
    public const string ReadImports = "read:imports";
    public const string CreateImports = "create:imports";
    public const string DeleteImports = "delete:imports";
}
```

### Authorization Handlers

Two custom authorization handlers validate roles and permissions from JWT claims:

**RoleAuthorizationHandler:**

```csharp
public class RoleAuthorizationHandler : AuthorizationHandler<RoleRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        RoleRequirement requirement)
    {
        var roles = context.User.FindAll(ClaimTypes.Role)
            .Select(c => c.Value).ToList();
        
        if (roles.Contains(requirement.Role))
        {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}
```

**PermissionAuthorizationHandler:**

```csharp
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        PermissionRequirement requirement)
    {
        var permissions = context.User.FindAll("permissions")
            .Select(c => c.Value).ToList();
        
        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}
```

Both handlers include comprehensive logging for security auditing.

> **Note:** For details on authorization logging, see [LOGGING.md](LOGGING.md#authorization-event-logging).

---

## Controller Authorization Matrix

This matrix shows the authorization requirements for each controller. Controllers use either `[RequirePermission]` (permission-based) or `[RequireRole]` (role-based) attributes.

| Controller | Endpoints | Admin | Manager | Partner | Attribute |
|------------|-----------|:-----:|:-------:|:-------:|-----------|
| **ClientController** | GET | ✅ | ✅ | ✅ | `[RequirePermission(ReadClients)]` |
| **ClientController** | POST, PUT, DELETE | ✅ | ✅ | ❌ | `[RequirePermission(CreateClients)]` etc. |
| **MemberController** | GET | ✅ | ✅ | ✅ | `[RequirePermission(ReadMembers)]` |
| **MemberController** | POST, PUT, DELETE | ✅ | ❌ | ❌ | `[RequireRole(Admin)]` |
| **BankController** | GET | ✅ | ✅ | ✅ | `[RequirePermission(ReadBanks)]` |
| **BankController** | POST, PUT, DELETE | ✅ | ❌ | ❌ | `[RequireRole(Admin)]` |
| **PokerManagerController** | GET | ✅ | ✅ | ✅ | `[RequirePermission(ReadManagers)]` |
| **PokerManagerController** | POST, PUT, DELETE | ✅ | ❌ | ❌ | `[RequireRole(Admin)]` |
| **CategoryController** | GET | ✅ | ✅ | ❌ | `[RequirePermission(ReadCategories)]` |
| **CategoryController** | POST, PUT, DELETE | ✅ | ❌ | ❌ | `[RequireRole(Admin)]` |
| **InitialBalanceController** | GET | ✅ | ✅ | ✅ | `[RequirePermission(ReadBalances)]` |
| **InitialBalanceController** | POST, DELETE | ✅ | ❌ | ❌ | `[RequireRole(Admin)]` |
| **CompanyAssetPoolController** | GET | ✅ | ✅ | ❌ | `[RequirePermission(ReadWallets)]` |
| **CompanyAssetPoolController** | POST, DELETE | ✅ | ❌ | ❌ | `[RequireRole(Admin)]` |
| **FiatAssetTransactionController** | All | ✅ | ✅ | ❌ | `[RequirePermission(CreateTransactions)]` |
| **DigitalAssetTransactionController** | All | ✅ | ✅ | ❌ | `[RequirePermission(CreateTransactions)]` |
| **TransferController** | POST | ✅ | ✅ | ❌ | `[RequirePermission(CreateTransactions)]` |
| **SettlementTransactionController** | All | ✅ | ✅ | ❌ | `[RequirePermission(CreateSettlements)]` |
| **WalletIdentifierController** | GET, POST | ✅ | ✅ | ❌ | `[RequirePermission(ReadWallets)]` |
| **WalletIdentifierController** | PUT, DELETE | ✅ | ❌ | ❌ | `[RequireRole(Admin)]` |
| **ProfitController** | All | ✅ | ❌ | ✅ | `[RequirePermission(ReadFinancialData)]` |
| **ImportedTransactionController** | All | ✅ | ❌ | ❌ | `[RequireRole(Admin)]` |
| **DiagnosticsController** | All | ✅ | ❌ | ❌ | `[RequireRole(Admin)]` |

### Authorization Pattern

The system uses a split authorization pattern:

1. **Class-level permission** for read access (e.g., `[RequirePermission(ReadCategories)]`)
2. **Method-level admin restriction** for write operations (e.g., `[RequireRole(Admin)]` on POST/PUT/DELETE)

Example from `CategoryController`:

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[RequirePermission(Auth0Permissions.ReadCategories)]  // Class-level: read access
public class CategoryController : BaseApiController<...>
{
    public override async Task<IActionResult> Get() { /* ... */ }

    [RequireRole(Auth0Roles.Admin)]  // Method-level: admin only
    public override Task<IActionResult> Post(CategoryRequest model) { /* ... */ }

    [RequireRole(Auth0Roles.Admin)]
    public override Task<IActionResult> Put(Guid id, CategoryRequest model) { /* ... */ }

    [RequireRole(Auth0Roles.Admin)]
    public override Task<IActionResult> Delete(Guid id) { /* ... */ }
}
```

---

## Using Authentication in Code

### Authorization Attributes

Apply authorization requirements using custom attributes:

**Require a specific role:**

```csharp
[RequireRole(Auth0Roles.Admin)]
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteResource(Guid id)
{
    // Only admins can access this endpoint
    return Ok(await _service.DeleteAsync(id));
}
```

**Require a specific permission:**

```csharp
[RequirePermission(Auth0Permissions.ReadClients)]
[HttpGet]
public async Task<IActionResult> GetClients()
{
    // Only users with read:clients permission can access
    return Ok(await _clientService.GetAllAsync());
}
```

**Combine role and permission requirements:**

```csharp
[RequireRole(Auth0Roles.Manager)]
[RequirePermission(Auth0Permissions.DeleteTransactions)]
[HttpDelete("transactions/{id}")]
public async Task<IActionResult> DeleteTransaction(Guid id)
{
    // Requires BOTH manager role AND delete:transactions permission
    return Ok(await _transactionService.DeleteAsync(id));
}
```

### Public Endpoints

To create a public endpoint, use the `[AllowAnonymous]` attribute:

```csharp
[AllowAnonymous]
[HttpGet("health")]
public IActionResult HealthCheck()
{
    return Ok(new { status = "healthy" });
}
```

### Programmatic Authorization Checks

Check authorization in code using `IAuth0UserService`:

```csharp
public async Task<IActionResult> ConditionalAction()
{
    var permissions = _auth0UserService.GetUserPermissions();
    
    if (permissions.Contains(Auth0Permissions.UpdateFinancialData))
    {
        // Perform sensitive operation
        return Ok(await PerformSensitiveUpdate());
    }
    
    return Forbid("Insufficient permissions for this operation");
}
```

---

## Database Integration

### Audit Trail

The `DataContext` automatically tracks who creates or modifies records using the `LastModifiedBy` property on `BaseDomain`.

### User ID Conversion

Auth0 user IDs (e.g., `auth0|123456789`) are converted to GUIDs for database storage using a consistent SHA256 hash:

```csharp
private Guid GetCurrentUserId()
{
    var user = _httpContextAccessor.HttpContext?.User;
    
    if (user != null && user.Identity?.IsAuthenticated == true)
    {
        var subClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(subClaim))
        {
            // Generate consistent Guid from Auth0 sub claim
            var hash = SHA256.Create();
            var hashBytes = hash.ComputeHash(Encoding.UTF8.GetBytes(subClaim));
            return new Guid(hashBytes.Take(16).ToArray());
        }
    }
    
    // Return default system user ID if no authenticated user
    return Guid.Empty;
}
```

This ensures:
- The same Auth0 user always maps to the same GUID
- Audit trails can track changes back to specific users
- The conversion is deterministic and reversible for debugging

> **Note:** For complete details on audit tracking, see [AUDIT_SYSTEM.md](AUDIT_SYSTEM.md).

---

## Frontend Integration

### Obtaining a JWT Token

Use the Auth0 SDK to authenticate users:

```javascript
import { Auth0Client } from '@auth0/auth0-spa-js';

const auth0 = new Auth0Client({
    domain: 'your-tenant.auth0.com',
    clientId: 'your-client-id',
    authorizationParams: {
        audience: 'https://your-api-identifier'
    }
});

// Redirect to Auth0 login
await auth0.loginWithRedirect();

// After login, get the access token
const token = await auth0.getAccessTokenSilently();
```

### Making Authenticated API Calls

Include the JWT in the `Authorization` header:

```javascript
const token = await auth0.getAccessTokenSilently();

const response = await fetch('/api/v1/clients', {
    headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
    }
});

const data = await response.json();
```

### Handling Token Expiration

Implement token refresh logic:

```javascript
async function apiRequest(url, options = {}) {
    try {
        const token = await auth0.getAccessTokenSilently();
        
        const response = await fetch(url, {
            ...options,
            headers: {
                ...options.headers,
                'Authorization': `Bearer ${token}`
            }
        });
        
        if (response.status === 401) {
            // Token expired or invalid, re-authenticate
            await auth0.loginWithRedirect();
        }
        
        return response;
    } catch (error) {
        if (error.error === 'login_required') {
            await auth0.loginWithRedirect();
        }
        throw error;
    }
}
```

---

## Security Best Practices

### Token Security

- **Store tokens securely** - Use `httpOnly` cookies or secure storage, never `localStorage` in production
- **Validate tokens server-side** - Never trust client-side token validation alone
- **Use HTTPS** - Always use HTTPS in production to protect tokens in transit
- **Handle expiration gracefully** - Implement silent refresh or prompt users to re-authenticate

### Authorization Best Practices

- **Principle of least privilege** - Grant minimum required permissions
- **Use specific permissions** - Prefer `read:clients` over broad `admin` role when possible
- **Audit authorization decisions** - Log both grants and denials for security analysis
- **Review permissions regularly** - Periodically audit user roles and permissions in Auth0

### Error Handling

Return appropriate HTTP status codes without leaking sensitive information:

```csharp
// Authentication failed (no valid token)
return Unauthorized();

// Authorization failed (valid token, insufficient permissions)
return Forbid();

// Don't expose internal details
// Bad: return Unauthorized("User auth0|123 not found in database");
// Good: return Unauthorized();
```

---

## Troubleshooting

### Common Issues

**401 Unauthorized on all requests:**
- Verify Auth0 `Domain` and `Audience` in configuration
- Check that the token hasn't expired
- Ensure the token is being sent in the `Authorization: Bearer {token}` header

**403 Forbidden after successful authentication:**
- User is authenticated but lacks required role/permission
- Check the JWT claims to verify roles and permissions are present
- Ensure roles/permissions are configured correctly in Auth0

**User email is null:**
- The email claim may be under a custom namespace
- Check for `https://www.semprefichas.com.br/email` custom claim
- Verify Auth0 rules/actions are adding the email to the token

**Inconsistent user IDs in audit trail:**
- The same Auth0 user should always generate the same GUID
- If GUIDs differ, check if the `sub` claim format changed

### Debugging JWT Tokens

Decode and inspect JWT tokens at [jwt.io](https://jwt.io) to verify:

1. **Header** - Algorithm and token type
2. **Payload** - Contains claims including:
   - `sub` - User identifier
   - `email` - User email
   - `roles` - User roles array
   - `permissions` - User permissions array
   - `exp` - Expiration timestamp
   - `aud` - Audience (should match your API identifier)
   - `iss` - Issuer (should match your Auth0 domain)
3. **Signature** - Validates token integrity

---

## Related Documentation

| Topic | Document |
|-------|----------|
| Configuration Details | [CONFIGURATION_MANAGEMENT.md](CONFIGURATION_MANAGEMENT.md) |
| Audit System | [AUDIT_SYSTEM.md](AUDIT_SYSTEM.md) |
| Logging | [LOGGING.md](LOGGING.md) |
| Error Handling | [ERROR_HANDLING.md](ERROR_HANDLING.md) |
| Frontend RBAC | Frontend RBAC implementation is documented independently in the frontend project. |

### External References

- [Auth0 Documentation](https://auth0.com/docs)
- [ASP.NET Core Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/)
- [JWT Bearer Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)
- [Custom Authorization Handlers](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies)
