# Authentication & Authorization Documentation

## Overview

This document describes the authentication and authorization implementation for the SF Management API, which uses **Auth0** as the identity provider. The system has been migrated from a custom JWT implementation to Auth0 for enhanced security, scalability, and maintainability.

## Architecture

### Authentication Flow

```
1. Client (Frontend) → Auth0 Login
2. Auth0 → Returns JWT Token
3. Client → API Request with JWT in Authorization Header
4. API → Validates JWT with Auth0
5. API → Extracts user info and permissions from JWT
6. API → Applies authorization policies
7. API → Returns response
```

### Key Components

- **Auth0**: External identity provider handling user authentication
- **JWT Bearer Authentication**: Token-based authentication using Auth0-issued JWTs
- **Custom Authorization Handlers**: Role and permission-based authorization
- **User Context Service**: Extracts user information from JWT claims

## Configuration

### Auth0 Settings

The Auth0 configuration is stored in `appsettings.json`:

```json
{
  "Auth0": {
    "Domain": "your-domain.auth0.com",
    "Audience": "https://your-api-identifier",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret"
  }
}
```

### Configuration Files

- **`Settings/Auth0Settings.cs`**: Strongly-typed configuration class
- **`StartupConfig/DependencyInjectionExtensions.cs`**: Service registration and JWT configuration
- **`Program.cs`**: Application startup configuration

## Authentication Implementation

### JWT Bearer Configuration

The JWT Bearer authentication is configured in `DependencyInjectionExtensions.cs`:

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

The `Auth0UserService` extracts user information from JWT claims:

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

**Usage Example:**

```csharp
[ApiController]
public class SomeController : ControllerBase
{
    private readonly IAuth0UserService _auth0UserService;

    public SomeController(IAuth0UserService auth0UserService)
    {
        _auth0UserService = auth0UserService;
    }

    [HttpGet]
    public IActionResult GetUserInfo()
    {
        var userId = _auth0UserService.GetUserId();
        var userEmail = _auth0UserService.GetUserEmail();
        var userRoles = _auth0UserService.GetUserRoles();

        return Ok(new { userId, userEmail, userRoles });
    }
}
```

## Authorization Implementation

### Authorization Policies

The system defines authorization policies for roles and permissions:

<!-- The permissions follow a resource-based access control (RBAC) pattern: -->

#### Role-Based Policies

- `Role:admin` - Administrator access
- `Role:manager` - Manager access
- `Role:user` - Standard user access
- `Role:viewer` - Read-only access

#### Permission-Based Policies

- `Permission:read:users` - Read user data
- `Permission:create:users` - Create users
- `Permission:update:users` - Update users
- `Permission:delete:users` - Delete users
- `Permission:read:clients` - Read client data
- `Permission:create:clients` - Create clients
- `Permission:update:clients` - Update clients
- `Permission:delete:clients` - Delete clients
- `Permission:read:transactions` - Read transaction data
- `Permission:create:transactions` - Create transactions
- `Permission:update:transactions` - Update transactions
- `Permission:delete:transactions` - Delete transactions
- `Permission:read:financial_data` - Read financial data
- `Permission:create:financial_data` - Create financial data
- `Permission:update:financial_data` - Update financial data
- `Permission:delete:financial_data` - Delete financial data

### Authorization Attributes

Custom authorization attributes are available for easy application:

```csharp
[RequireRole("admin")]
public class AdminController : ControllerBase { }

[RequirePermission("read:users")]
public class UserController : ControllerBase { }
```

### Authorization Handlers

Custom authorization handlers validate roles and permissions from JWT claims. These handlers are the core of the authorization system, implementing the logic that determines whether a user can access specific resources.

#### How Authorization Handlers Work

Authorization handlers follow the **Chain of Responsibility** pattern in ASP.NET Core's authorization system:

```csharp
// 1. Define the requirement
public class RoleRequirement : IAuthorizationRequirement
{
    public string Role { get; }

    public RoleRequirement(string role)
    {
        Role = role;
    }
}

// 2. Implement the handler
public class RoleAuthorizationHandler : AuthorizationHandler<RoleRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
    {
        // Extract roles from JWT claims
        var roles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value);

        // Check if user has the required role
        if (roles.Contains(requirement.Role))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
```

#### Authorization Flow

1. **JWT Token Validation**: Auth0 validates the JWT token and extracts claims
2. **Policy Evaluation**: When `[RequireRole("admin")]` is used, the system looks for a policy named `"Role:admin"`
3. **Handler Execution**: The `RoleAuthorizationHandler` is invoked with the `RoleRequirement`
4. **Claim Extraction**: Handler extracts role claims from the JWT token
5. **Permission Check**: Handler checks if the required role exists in the user's claims
6. **Decision**: Handler calls `context.Succeed()` or leaves the requirement unfulfilled

#### JWT Claims Structure

The JWT token from Auth0 contains claims that the handlers validate:

```json
{
  "sub": "auth0|123456789",
  "email": "user@example.com",
  "name": "John Doe",
  "roles": ["manager", "user"],
  "permissions": [
    "read:clients",
    "create:transactions",
    "update:financial_data"
  ],
  "aud": "https://your-api-identifier",
  "iss": "https://your-domain.auth0.com/",
  "exp": 1640995200
}
```

#### Handler Registration

Handlers are registered in `DependencyInjectionExtensions.cs`:

```csharp
// Add authorization handlers
builder.Services.AddScoped<IAuthorizationHandler, RoleAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

// Define policies that use these handlers
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Role:admin", policy => policy.Requirements.Add(new RoleRequirement(Auth0Roles.Admin)))
    .AddPolicy("Permission:read:users", policy => policy.Requirements.Add(new PermissionRequirement(Auth0Permissions.ReadUsers)));
```

#### Custom Handler Examples

You can create custom handlers for complex authorization logic:

```csharp
public class ClientOwnershipRequirement : IAuthorizationRequirement
{
    public string Operation { get; }

    public ClientOwnershipRequirement(string operation)
    {
        Operation = operation;
    }
}

public class ClientOwnershipHandler : AuthorizationHandler<ClientOwnershipRequirement>
{
    private readonly IAuth0UserService _auth0UserService;
    private readonly IClientService _clientService;

    public ClientOwnershipHandler(IAuth0UserService auth0UserService, IClientService clientService)
    {
        _auth0UserService = auth0UserService;
        _clientService = clientService;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ClientOwnershipRequirement requirement)
    {
        var userId = _auth0UserService.GetUserId();
        var clientId = GetClientIdFromRoute(context); // Extract from route parameters

        // Check if user owns or manages this client
        var canAccess = await _clientService.CanUserAccessClient(userId, clientId, requirement.Operation);

        if (canAccess)
        {
            context.Succeed(requirement);
        }
    }
}
```

#### Debugging Authorization Handlers

Enable detailed logging to debug authorization issues:

```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.AspNetCore.Authorization": "Debug",
      "SFManagement.Authorization": "Debug"
    }
  }
}
```

## Database Integration

### User Context in Data Operations

The `DataContext` automatically extracts user information from JWT claims for audit trails:

```csharp
private void SetDefaultProperties()
{
    var userId = Guid.Empty;
    var user = _httpContextAccessor.HttpContext?.User;

    if (user != null && user.Identity?.IsAuthenticated == true)
    {
        var subClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(subClaim))
        {
            // Convert Auth0 sub claim to Guid for database compatibility
            userId = Guid.Parse(subClaim.Replace("|", "").Substring(0, 32));
        }
    }

    // Apply user context to auditable entities
    foreach (var auditableEntity in ChangeTracker.Entries<BaseDomain>())
    {
        // Set audit properties (CreatedAt, UpdatedAt, etc.)
    }
}
```

## API Usage

### Protected Endpoints

All endpoints require authentication by default. Use the `[AllowAnonymous]` attribute for public endpoints:

```csharp
[AllowAnonymous]
[HttpGet("public")]
public IActionResult PublicEndpoint()
{
    return Ok("This endpoint is public");
}
```

### Role-Based Access

```csharp
[RequireRole(Auth0Roles.Admin)]
[HttpGet("admin-only")]
public IActionResult AdminOnlyEndpoint()
{
    return Ok("Only admins can access this");
}

[RequireRole(Auth0Roles.Manager)]
[HttpGet("manager-only")]
public IActionResult ManagerOnlyEndpoint()
{
    return Ok("Only managers can access this");
}
```

### Permission-Based Access

```csharp
[RequirePermission(Auth0Permissions.ReadUsers)]
[HttpGet("users")]
public async Task<IActionResult> GetUsers()
{
    // Only users with read:users permission can access this
    return Ok(await _userService.GetAllUsers());
}

[RequirePermission(Auth0Permissions.CreateUsers)]
[HttpPost("users")]
public async Task<IActionResult> CreateUser(CreateUserRequest request)
{
    // Only users with create:users permission can access this
    return Ok(await _userService.CreateUser(request));
}
```

### Combining Roles and Permissions

```csharp
[RequireRole(Auth0Roles.Admin)]
[RequirePermission(Auth0Permissions.DeleteUsers)]
[HttpDelete("users/{id}")]
public async Task<IActionResult> DeleteUser(Guid id)
{
    // Requires both admin role AND delete:users permission
    return Ok(await _userService.DeleteUser(id));
}
```

## Frontend Integration

### Obtaining JWT Token

1. **Redirect to Auth0 Login:**

   ```javascript
   const auth0 = new Auth0({
     domain: "your-domain.auth0.com",
     clientId: "your-client-id",
     audience: "https://your-api-identifier",
   });

   auth0.loginWithRedirect();
   ```

2. **Handle Authentication Callback:**
   ```javascript
   auth0.handleRedirectCallback().then((result) => {
     const token = result.accessToken;
     // Store token securely
     localStorage.setItem("auth_token", token);
   });
   ```

### Making Authenticated API Calls

```javascript
const token = localStorage.getItem("auth_token");

fetch("/api/v1/users", {
  headers: {
    Authorization: `Bearer ${token}`,
    "Content-Type": "application/json",
  },
})
  .then((response) => response.json())
  .then((data) => console.log(data));
```

## Security Considerations

### JWT Token Security

- **Token Storage**: Store tokens securely (HttpOnly cookies, secure storage)
- **Token Expiration**: Handle token refresh automatically
- **Token Validation**: Always validate tokens on the server side
- **HTTPS**: Use HTTPS in production for all communications

### Authorization Best Practices

- **Principle of Least Privilege**: Grant minimum required permissions
- **Role Hierarchy**: Design roles with clear hierarchy
- **Permission Granularity**: Use specific permissions rather than broad roles
- **Regular Audits**: Review and update permissions regularly

### Error Handling

```csharp
// Handle authentication errors
if (!_auth0UserService.IsAuthenticated())
{
    return Unauthorized("Authentication required");
}

// Handle authorization errors
if (!_auth0UserService.GetUserPermissions().Contains("read:users"))
{
    return Forbid("Insufficient permissions");
}
```

## Testing

### Unit Testing

```csharp
[Test]
public void Auth0UserService_GetUserId_ReturnsCorrectId()
{
    // Arrange
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, "auth0|123456789")
    };
    var identity = new ClaimsIdentity(claims, "Bearer");
    var principal = new ClaimsPrincipal(identity);

    var httpContext = new DefaultHttpContext { User = principal };
    var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };

    var service = new Auth0UserService(httpContextAccessor);

    // Act
    var userId = service.GetUserId();

    // Assert
    Assert.AreEqual("auth0|123456789", userId);
}
```

### Integration Testing

```csharp
[Test]
public async Task ProtectedEndpoint_WithValidToken_ReturnsSuccess()
{
    // Arrange
    var token = await GetValidAuth0Token();
    var client = _factory.CreateClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    // Act
    var response = await client.GetAsync("/api/v1/protected-endpoint");

    // Assert
    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
}
```

## Migration from Custom JWT

### What Was Removed

- `ApplicationUser` and `ApplicationRole` models
- `UserService` and authentication logic
- `UserController` and authentication endpoints
- Custom JWT configuration and validation
- Identity framework integration
- Authentication-related ViewModels

### What Was Added

- Auth0 configuration and settings
- `Auth0UserService` for user context
- Custom authorization attributes and handlers
- Role and permission constants
- JWT Bearer authentication for Auth0

### Database Changes

- Removed Identity tables (AspNetUsers, AspNetRoles, etc.)
- Updated `DataContext` to use Auth0 claims
- Modified audit trail to use Auth0 user IDs

## Troubleshooting

### Common Issues

1. **JWT Validation Errors**

   - Verify Auth0 domain and audience configuration
   - Check token expiration
   - Ensure correct signing algorithm

2. **Authorization Failures**

   - Verify user has required roles/permissions in Auth0
   - Check JWT claims for correct role/permission values
   - Ensure authorization policies are properly configured

3. **User Context Issues**
   - Verify JWT contains required claims
   - Check `Auth0UserService` implementation
   - Ensure HTTP context is properly configured

### Debugging

Enable detailed logging for authentication:

```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.AspNetCore.Authentication": "Debug",
      "SFManagement.Authorization": "Debug"
    }
  }
}
```

## References

- [Auth0 Documentation](https://auth0.com/docs)
- [ASP.NET Core Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/)
- [JWT Bearer Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)
- [Custom Authorization Handlers](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies)

---

_This documentation should be updated as the authentication system evolves._

## System Growth and Improvement Strategies

### Scalability Considerations

#### 1. **Performance Optimization**

**Current State**: Authorization handlers are simple and fast
**Future Improvements**:

```csharp
// Add caching for frequently accessed permissions
public class CachedPermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IMemoryCache _cache;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var cacheKey = $"permissions_{userId}";

        var permissions = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(5);
            return context.User.FindAll("permissions").Select(c => c.Value).ToList();
        });

        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }
}
```

#### 2. **Dynamic Permission Management**

**Current State**: Permissions are hardcoded in constants
**Future Improvements**:

```csharp
// Database-driven permissions
public class DynamicPermissionService
{
    public async Task<List<string>> GetUserPermissionsAsync(string userId)
    {
        // Fetch from database or external service
        return await _permissionRepository.GetUserPermissionsAsync(userId);
    }
}

// Custom claim transformer
public class PermissionClaimTransformer : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            var permissions = await _permissionService.GetUserPermissionsAsync(userId);
            var identity = principal.Identity as ClaimsIdentity;
            identity?.AddClaims(permissions.Select(p => new Claim("permissions", p)));
        }
        return principal;
    }
}
```

#### 3. **Multi-Tenant Support**

**Future Enhancement**:

```csharp
public class TenantAuthorizationHandler : AuthorizationHandler<TenantRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, TenantRequirement requirement)
    {
        var tenantId = context.User.FindFirst("tenant_id")?.Value;
        var resourceTenantId = GetResourceTenantId(context);

        if (tenantId == resourceTenantId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
```

### Security Enhancements

#### 1. **Advanced Role Hierarchies**

```csharp
public class RoleHierarchyHandler : AuthorizationHandler<RoleRequirement>
{
    private readonly Dictionary<string, List<string>> _roleHierarchy = new()
    {
        ["admin"] = ["manager", "user", "viewer"],
        ["manager"] = ["user", "viewer"],
        ["user"] = ["viewer"],
        ["viewer"] = []
    };

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
    {
        var userRoles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value);
        var effectiveRoles = GetEffectiveRoles(userRoles);

        if (effectiveRoles.Contains(requirement.Role))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }

    private List<string> GetEffectiveRoles(IEnumerable<string> userRoles)
    {
        var effective = new List<string>();
        foreach (var role in userRoles)
        {
            effective.Add(role);
            effective.AddRange(_roleHierarchy.GetValueOrDefault(role, new List<string>()));
        }
        return effective.Distinct().ToList();
    }
}
```

#### 2. **Time-Based Permissions**

```csharp
public class TimeBasedPermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var permissions = context.User.FindAll("permissions");

        foreach (var permission in permissions)
        {
            if (permission.Value == requirement.Permission)
            {
                // Check if permission has time restrictions
                var expiresAt = permission.Properties?.GetValue("expires_at");
                if (expiresAt != null && DateTime.TryParse(expiresAt.ToString(), out var expiry))
                {
                    if (DateTime.UtcNow <= expiry)
                    {
                        context.Succeed(requirement);
                        return Task.CompletedTask;
                    }
                }
                else
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
            }
        }

        return Task.CompletedTask;
    }
}
```

### Integration Enhancements

#### 1. **External Permission Providers**

```csharp
public interface IExternalPermissionProvider
{
    Task<List<string>> GetUserPermissionsAsync(string userId);
    Task<bool> ValidatePermissionAsync(string userId, string permission, string resource);
}

public class ExternalPermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IExternalPermissionProvider _permissionProvider;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var resource = GetResourceFromContext(context);

        var hasPermission = await _permissionProvider.ValidatePermissionAsync(userId, requirement.Permission, resource);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}
```

#### 2. **Audit Trail Integration**

```csharp
public class AuditAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IAuditService _auditService;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var resource = GetResourceFromContext(context);

        // Log authorization attempt
        await _auditService.LogAuthorizationAttemptAsync(userId, requirement.Permission, resource);

        // Continue with normal authorization logic
        var permissions = context.User.FindAll("permissions").Select(c => c.Value);

        if (permissions.Contains(requirement.Permission))
        {
            await _auditService.LogAuthorizationSuccessAsync(userId, requirement.Permission, resource);
            context.Succeed(requirement);
        }
        else
        {
            await _auditService.LogAuthorizationFailureAsync(userId, requirement.Permission, resource);
        }
    }
}
```

## User Information Storage Strategy: Hybrid Approach

### Overview

The hybrid approach leverages Auth0 for authentication and user management while maintaining application-specific user data in your database. This strategy provides the best balance of security, performance, and flexibility.

### What to Store Where

#### **Auth0 Storage (Identity Provider)**

```json
{
  "user_id": "auth0|123456789",
  "email": "user@example.com",
  "name": "John Doe",
  "email_verified": true,
  "roles": ["manager"],
  "permissions": ["read:clients", "create:transactions"],
  "app_metadata": {
    "department": "Finance",
    "employee_id": "EMP001"
  },
  "user_metadata": {
    "preferences": {
      "language": "pt-BR",
      "timezone": "America/Sao_Paulo"
    }
  }
}
```

**Store in Auth0:**

- ✅ **Authentication Data**: Email, password, MFA settings
- ✅ **Profile Information**: Name, email, profile picture
- ✅ **Roles & Permissions**: Global access control
- ✅ **Security Settings**: Login history, device management
- ✅ **Metadata**: Department, employee ID, preferences

#### **Database Storage (Application)**

```csharp
// Example: User-specific application data
public class UserProfile : BaseDomain
{
    public string Auth0UserId { get; set; }  // Link to Auth0
    public string DisplayName { get; set; }
    public string Department { get; set; }
    public List<Guid> ManagedClientIds { get; set; }  // Application-specific
    public UserPreferences Preferences { get; set; }
    public DateTime LastLogin { get; set; }
    public bool IsActive { get; set; }
}

public class UserPreferences
{
    public string DefaultCurrency { get; set; }
    public string DateFormat { get; set; }
    public List<string> FavoriteReports { get; set; }
    public NotificationSettings Notifications { get; set; }
}
```

**Store in Database:**

- ✅ **Application-Specific Data**: User preferences, settings
- ✅ **Business Relationships**: Which clients a user manages
- ✅ **Audit Trail**: Who created/modified what records
- ✅ **Performance Data**: Cached user information
- ✅ **Custom Fields**: Business-specific user attributes

### Implementation Strategy

#### **1. User Context Service Enhancement**

```csharp
public interface IAuth0UserService
{
    string? GetUserId();
    string? GetUserEmail();
    string? GetUserName();
    List<string> GetUserRoles();
    List<string> GetUserPermissions();
    bool IsAuthenticated();

    // New methods for hybrid approach
    Task<UserProfile?> GetUserProfileAsync();
    Task<List<Guid>> GetManagedClientIdsAsync();
    Task<UserPreferences?> GetUserPreferencesAsync();
    Task UpdateUserPreferencesAsync(UserPreferences preferences);
}

public class Auth0UserService : IAuth0UserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserProfileService _userProfileService;
    private readonly IMemoryCache _cache;

    public async Task<UserProfile?> GetUserProfileAsync()
    {
        var auth0UserId = GetUserId();
        if (string.IsNullOrEmpty(auth0UserId))
            return null;

        var cacheKey = $"user_profile_{auth0UserId}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(30);
            return await _userProfileService.GetByAuth0UserIdAsync(auth0UserId);
        });
    }

    public async Task<List<Guid>> GetManagedClientIdsAsync()
    {
        var profile = await GetUserProfileAsync();
        return profile?.ManagedClientIds ?? new List<Guid>();
    }
}
```

#### **2. Database Integration**

```csharp
// In DataContext.cs - Enhanced user context
private async void SetDefaultProperties()
{
    var userId = Guid.Empty;
    var user = _httpContextAccessor.HttpContext?.User;

    if (user != null && user.Identity?.IsAuthenticated == true)
    {
        var auth0UserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(auth0UserId))
        {
            // Get or create user profile in database
            var userProfile = await _userProfileService.GetOrCreateByAuth0UserIdAsync(auth0UserId);
            userId = userProfile.Id;

            // Update last activity
            userProfile.LastLogin = DateTime.UtcNow;
            await _userProfileService.UpdateAsync(userProfile);
        }
    }

    // Apply user context to auditable entities
    foreach (var auditableEntity in ChangeTracker.Entries<BaseDomain>())
    {
        if (auditableEntity.State == EntityState.Added)
        {
            auditableEntity.Entity.CreatedAt = DateTime.UtcNow;
            auditableEntity.Entity.LastModifiedBy = userId;
        }
        else if (auditableEntity.State == EntityState.Modified)
        {
            auditableEntity.Entity.UpdatedAt = DateTime.UtcNow;
            auditableEntity.Entity.LastModifiedBy = userId;
        }
    }
}
```

#### **3. User Profile Management**

```csharp
[ApiController]
[Route("api/v1/user-profile")]
public class UserProfileController : ControllerBase
{
    private readonly IAuth0UserService _auth0UserService;
    private readonly IUserProfileService _userProfileService;

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var profile = await _auth0UserService.GetUserProfileAsync();
        if (profile == null)
        {
            // Create profile if it doesn't exist
            var auth0UserId = _auth0UserService.GetUserId();
            var email = _auth0UserService.GetUserEmail();
            var name = _auth0UserService.GetUserName();

            profile = await _userProfileService.CreateAsync(new UserProfile
            {
                Auth0UserId = auth0UserId,
                DisplayName = name,
                Email = email,
                IsActive = true
            });
        }

        return Ok(profile);
    }

    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences(UserPreferences preferences)
    {
        await _auth0UserService.UpdateUserPreferencesAsync(preferences);
        return Ok();
    }
}
```

### Benefits of Hybrid Approach

#### **1. Security Benefits**

- **Centralized Authentication**: Auth0 handles security best practices
- **No Password Storage**: Sensitive data stays with Auth0
- **Compliance**: Auth0 provides compliance certifications
- **MFA Support**: Built-in multi-factor authentication

#### **2. Performance Benefits**

- **Reduced Database Load**: No need to sync user data
- **Caching**: Cache user profiles for better performance
- **Scalability**: Auth0 scales authentication independently

#### **3. Flexibility Benefits**

- **Custom Fields**: Add business-specific user attributes
- **Easy Migration**: Can change identity providers without data loss
- **Audit Trail**: Track user actions in your system
- **Business Logic**: Implement complex user relationships

### Migration Strategy

#### **Phase 1: Setup Hybrid Infrastructure**

1. Create `UserProfile` table in database
2. Enhance `Auth0UserService` with profile methods
3. Update `DataContext` to use hybrid approach

#### **Phase 2: Gradual Migration**

1. New users automatically get profiles created
2. Existing users get profiles on first login
3. Migrate user preferences from old system

#### **Phase 3: Optimization**

1. Implement caching for user profiles
2. Add user preference management
3. Implement audit trail for user actions

### Example Usage

```csharp
[RequirePermission(Auth0Permissions.ReadClients)]
[HttpGet("clients")]
public async Task<IActionResult> GetClients()
{
    var userProfile = await _auth0UserService.GetUserProfileAsync();
    var managedClientIds = await _auth0UserService.GetManagedClientIdsAsync();

    // Filter clients based on user's managed clients
    var clients = await _clientService.GetClientsByIdsAsync(managedClientIds);

    return Ok(clients);
}

[RequirePermission(Auth0Permissions.CreateTransactions)]
[HttpPost("transactions")]
public async Task<IActionResult> CreateTransaction(TransactionRequest request)
{
    var userProfile = await _auth0UserService.GetUserProfileAsync();

    // Add user context to transaction
    request.CreatedBy = userProfile.Id;
    request.CreatedAt = DateTime.UtcNow;

    var transaction = await _transactionService.CreateAsync(request);
    return Ok(transaction);
}
```

This hybrid approach gives you the best of both worlds: enterprise-grade authentication with Auth0 and flexible, application-specific user management in your database.
