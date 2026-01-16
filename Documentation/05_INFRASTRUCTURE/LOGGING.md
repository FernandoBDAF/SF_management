# Logging System

This document describes the logging infrastructure for the SF Management API. The system provides structured logging with user context, security audit trails, and multiple output destinations.

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Configuration](#configuration)
- [Core Components](#core-components)
- [Logging Service](#logging-service)
- [Middleware](#middleware)
- [Log Output Examples](#log-output-examples)
- [Debugging & Troubleshooting](#debugging--troubleshooting)
- [Seq Integration](#seq-integration)
- [Best Practices](#best-practices)
- [Related Documentation](#related-documentation)

---

## Overview

The logging system is built on **Serilog** and provides:

- **Structured logging** - Machine-readable log entries with typed properties
- **User context** - Every log includes the authenticated user's identity
- **Multiple sinks** - Console, file, and Seq outputs
- **Security audit trails** - Authorization decisions are logged
- **Data access tracking** - Database operations are automatically logged

## Architecture

### Logging Stack

```
┌─────────────────────────────────────────────────────────────────┐
│                      SF Management API                          │
├──────────────┬──────────────┬──────────────┬───────────────────┤
│  Controllers │   Services   │ DataContext  │ Auth Handlers     │
└──────┬───────┴──────┬───────┴──────┬───────┴─────────┬─────────┘
       │              │              │                 │
       ▼              ▼              ▼                 ▼
┌─────────────────────────────────────────────────────────────────┐
│                    ILoggingService                              │
│  (User context, request info, structured logging)               │
└─────────────────────────────────────────────────────────────────┘
       │
       ▼
┌─────────────────────────────────────────────────────────────────┐
│                       Serilog                                   │
├─────────────────┬─────────────────┬─────────────────────────────┤
│  Console Sink   │   File Sink     │      Seq Sink               │
│  (Development)  │  (Daily Logs)   │   (Log Server)              │
└─────────────────┴─────────────────┴─────────────────────────────┘
```

### Project Structure

```
SF_management/
├── Infrastructure/
│   └── Logging/
│       └── LoggingService.cs          # Core logging service
├── Api/
│   └── Middleware/
│       ├── AuthenticationLoggingMiddleware.cs
│       ├── ErrorHandlerMiddleware.cs      # Global error handling
│       └── RequestResponseLoggingMiddleware.cs
├── appsettings.json                   # Serilog configuration
└── logs/                              # Daily log files
    └── sf-management-{date}.log
```

---

## Configuration

Serilog is configured in `appsettings.json`. The key settings include:

| Setting | Description |
|---------|-------------|
| `MinimumLevel.Default` | Default log level for the application |
| `MinimumLevel.Override` | Per-namespace log level overrides |
| `WriteTo` | Output destinations (sinks) |
| `Enrich` | Additional properties added to all log entries |
| `Properties` | Static properties added to all entries |

### Log Levels

| Level | Description | Usage |
|-------|-------------|-------|
| `Debug` | Detailed debugging info | Authentication details, token info |
| `Information` | Normal operations | User actions, successful operations |
| `Warning` | Authorization failures | Denied requests, token expiration |
| `Error` | System errors | Unhandled exceptions, timeouts |

### Enabling Detailed Request Logging

The `RequestResponseLoggingMiddleware` is enabled when `EnableDetailedLogging` is true:

```json
{
  "EnableDetailedLogging": true
}
```

This logs full request/response bodies for debugging.

> **Note:** For complete Serilog configuration details, see [CONFIGURATION_MANAGEMENT.md](CONFIGURATION_MANAGEMENT.md#serilog-settings).

---

## Core Components

### ILoggingService Interface

The `ILoggingService` provides structured logging with user context:

```csharp
public interface ILoggingService
{
    void LogUserAction(string action, string resource, object? data = null, 
        LogLevel level = LogLevel.Information);
    
    void LogSecurityEvent(string eventType, string details, 
        LogLevel level = LogLevel.Warning);
    
    void LogDataAccess(string operation, string entityType, 
        Guid? entityId = null, object? changes = null);
    
    void LogFinancialOperation(string operation, decimal amount, 
        string currency, Guid? clientId = null);
    
    void LogAuthenticationEvent(string eventType, string userId, 
        bool success, string? reason = null);
    
    void LogAuthorizationEvent(string resource, string action, 
        bool granted, string? reason = null);
}
```

### User Context

Every log entry automatically includes user context from Auth0:

```csharp
private UserContext GetUserContext()
{
    return new UserContext
    {
        UserId = _auth0UserService.GetUserId() ?? "anonymous",
        UserEmail = _auth0UserService.GetUserEmail() ?? "unknown",
        UserRoles = string.Join(", ", _auth0UserService.GetUserRoles()),
        UserPermissions = string.Join(", ", _auth0UserService.GetUserPermissions()),
        IsAuthenticated = _auth0UserService.IsAuthenticated()
    };
}
```

### Request Context

Security events include request information:

```csharp
private RequestInfo GetRequestInfo()
{
    var context = _httpContextAccessor.HttpContext;
    return new RequestInfo
    {
        IpAddress = context?.Connection?.RemoteIpAddress?.ToString() ?? "unknown",
        UserAgent = context?.Request?.Headers["User-Agent"].ToString() ?? "unknown",
        Method = context?.Request?.Method ?? "unknown",
        Path = context?.Request?.Path ?? "unknown",
        QueryString = context?.Request?.QueryString.ToString() ?? ""
    };
}
```

---

## Logging Service

### Current Usage

The `LoggingService` is currently used in two places:

#### 1. Authorization Handlers

Both `RoleAuthorizationHandler` and `PermissionAuthorizationHandler` log authorization decisions:

```csharp
// In Auth0AuthorizationHandlers.cs
protected override Task HandleRequirementAsync(
    AuthorizationHandlerContext context, 
    RoleRequirement requirement)
{
    var roles = context.User.FindAll(ClaimTypes.Role)
        .Select(c => c.Value).ToList();
    
    if (roles.Contains(requirement.Role))
    {
        _logger.LogInformation(
            "Role authorization granted: User {UserId} ({UserEmail}) has role {RequiredRole}",
            userId, userEmail, requirement.Role);
        
        _loggingService.LogAuthorizationEvent("role", requirement.Role, true);
        context.Succeed(requirement);
    }
    else
    {
        _logger.LogWarning(
            "Role authorization denied: User {UserId} ({UserEmail}) lacks role {RequiredRole}",
            userId, userEmail, requirement.Role);
        
        _loggingService.LogAuthorizationEvent("role", requirement.Role, false,
            $"User has roles: {string.Join(", ", roles)}");
    }
    
    return Task.CompletedTask;
}
```

#### 2. DataContext (Database Operations)

The `DataContext.SetDefaultProperties()` logs all data changes:

```csharp
// In DataContext.cs
private void SetDefaultProperties()
{
    var userId = GetCurrentUserId();

    foreach (var auditableEntity in ChangeTracker.Entries<BaseDomain>())
    {
        if (auditableEntity.State == EntityState.Added)
        {
            auditableEntity.Entity.CreatedAt = DateTime.UtcNow;
            auditableEntity.Entity.LastModifiedBy = userId;
            
            _loggingService.LogDataAccess("create", 
                auditableEntity.Entity.GetType().Name, 
                auditableEntity.Entity.Id, 
                new { 
                    EntityType = auditableEntity.Entity.GetType().Name,
                    EntityId = auditableEntity.Entity.Id,
                    CreatedBy = userId,
                    CreatedAt = auditableEntity.Entity.CreatedAt
                });
        }
        else if (auditableEntity.State == EntityState.Modified)
        {
            // ... similar logging for updates and deletes
        }
    }
}
```

### Available Methods

#### LogDataAccess (Currently Used)

Logs database operations with entity details:

```csharp
_loggingService.LogDataAccess("create", "Client", clientId, new {
    EntityType = "Client",
    EntityId = clientId,
    CreatedBy = userId
});
```

**Output:**
```
[INF] Data Access: create on Client 550e8400-... by auth0|123 (user@example.com) - Changes: {"EntityType":"Client","EntityId":"550e8400-..."}
```

#### LogAuthorizationEvent (Currently Used)

Logs authorization decisions:

```csharp
_loggingService.LogAuthorizationEvent("role", "admin", false, 
    "User has roles: manager, user");
```

**Output:**
```
[WRN] Authorization Event: admin on role - Granted: False - User: auth0|123 (user@example.com) - Roles: manager, user - Reason: User has roles: manager, user - IP: 192.168.1.100
```

#### LogUserAction (Available)

Logs user-initiated actions:

```csharp
_loggingService.LogUserAction("create", "client", request);
```

**Output:**
```
[INF] User Action: create on client by auth0|123 (user@example.com) - Roles: manager - Data: {"Name":"John"}
```

#### LogSecurityEvent (Available)

Logs security-related events:

```csharp
_loggingService.LogSecurityEvent("suspicious_activity", 
    "Multiple failed login attempts");
```

**Output:**
```
[WRN] Security Event: suspicious_activity - Multiple failed login attempts - User: auth0|123 (user@example.com) - IP: 192.168.1.100 - UserAgent: Mozilla/5.0...
```

#### LogFinancialOperation (Available)

Logs financial transactions:

```csharp
_loggingService.LogFinancialOperation("transfer", 1000.00m, "BRL", clientId);
```

**Output:**
```
[INF] Financial Operation: transfer - Amount: 1000.00 BRL - Client: 550e8400-... - User: auth0|123 (user@example.com)
```

#### LogAuthenticationEvent (Available)

Logs authentication events:

```csharp
_loggingService.LogAuthenticationEvent("login", userId, true);
```

**Output:**
```
[INF] Authentication Event: login - User: auth0|123 - Success: True - Reason: N/A - IP: 192.168.1.100 - UserAgent: Mozilla/5.0...
```

---

## Middleware

### AuthenticationLoggingMiddleware

Logs authentication status for every request:

```csharp
// Authenticated request:
_logger.LogInformation(
    "Authenticated request: {Method} {Path} by {UserId} ({UserEmail})", 
    method, originalPath, userId, userEmail);

// Unauthenticated request:
_logger.LogInformation(
    "Unauthenticated request: {Method} {Path}", 
    method, originalPath);
```

**Features:**
- Logs authentication status per request
- Monitors JWT token expiration
- Warns when tokens expire within 5 minutes
- Validates JWT token format

**Registration in `Program.cs`:**
```csharp
app.UseAuthentication();
app.UseAuthenticationLogging(); // After authentication
```

### RequestResponseLoggingMiddleware

Detailed request/response logging for debugging:

**Features:**
- Logs full request details (method, path, headers, body)
- Logs response status and body
- Sanitizes sensitive data (passwords, tokens, secrets)
- Truncates large bodies (>10KB)
- Special handling for 400 errors with validation details

**Sensitive data handling:**
- Headers: `authorization`, `cookie`, `x-api-key`, `x-auth-token`
- Properties: `password`, `token`, `secret`, `key`, `auth`, `credential`

**Registration in `Program.cs`:**
```csharp
if (app.Environment.IsDevelopment() || 
    builder.Configuration.GetValue<bool>("EnableDetailedLogging"))
{
    app.UseRequestResponseLogging();
}
```

> **Note:** For error handling and exception types, see [ERROR_HANDLING.md](ERROR_HANDLING.md).

---

## Log Output Examples

### Authorization Granted

```
[14:30:45 INF] Role authorization granted: User auth0|123456789 (user@example.com) has role admin. User roles: admin, manager
```

### Authorization Denied

```
[14:30:45 WRN] Authorization Event: delete on permission - Granted: False - User: auth0|123456789 (user@example.com) - Roles: manager, user - Reason: User has permissions: read:clients, create:clients - IP: 192.168.1.100
```

### Data Access (Create)

```
[14:30:45 INF] Data Access: create on Client 550e8400-e29b-41d4-a716-446655440000 by auth0|123456789 (user@example.com) - Changes: {"EntityType":"Client","EntityId":"550e8400-e29b-41d4-a716-446655440000","CreatedBy":"abc123...","CreatedAt":"2024-01-15T14:30:45Z"}
```

### Data Access (Update)

```
[14:30:45 INF] Data Access: update on Client 550e8400-e29b-41d4-a716-446655440000 by auth0|123456789 (user@example.com) - Changes: {"EntityType":"Client","EntityId":"550e8400-...","UpdatedBy":"abc123...","UpdatedAt":"2024-01-15T14:30:45Z"}
```

### Data Access (Delete)

```
[14:30:45 INF] Data Access: delete on Client 550e8400-e29b-41d4-a716-446655440000 by auth0|123456789 (user@example.com) - Changes: {"EntityType":"Client","EntityId":"550e8400-...","DeletedBy":"abc123...","DeletedAt":"2024-01-15T14:30:45Z"}
```

### Authenticated Request

```
[14:30:45 INF] Authenticated request: GET /api/v1/clients by auth0|123456789 (user@example.com)
```

### Token Expiration Warning

```
[14:30:45 WRN] JWT token expires soon: 2024-01-15T14:35:45+00:00 (in 4.5 minutes)
```

---

## Debugging & Troubleshooting

### Enable Debug Logging

For detailed authentication/authorization debugging:

```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.AspNetCore.Authentication": "Debug",
      "SFManagement.Infrastructure.Authorization": "Debug",
      "SFManagement.Api.Middleware": "Debug"
    }
  }
}
```

### Useful Log Queries

**Find authorization failures:**
```bash
grep "authorization denied\|Granted: False" logs/sf-management-*.log
```

**Find all errors:**
```bash
grep "\[ERR\]" logs/sf-management-*.log
```

**Find requests by user:**
```bash
grep "auth0|123456789" logs/sf-management-*.log
```

**Find data access operations:**
```bash
grep "Data Access:" logs/sf-management-*.log
```

**Find 400 errors:**
```bash
grep "Bad Request (400)" logs/sf-management-*.log
```

### Common Issues

**Missing User Context:**
- Check if authentication middleware is registered before logging middleware
- Verify JWT token is being passed in requests

**Logs Not Appearing:**
- Check minimum log level configuration
- Verify log file path is writable
- Check Seq server connection (if using)

**Large Log Files:**
- Adjust `retainedFileCountLimit` in configuration
- Increase minimum log level for verbose namespaces

---

## Seq Integration

[Seq](https://datalust.co/seq) is a log aggregation server that provides powerful search and analysis.

### Setup Seq (Docker)

```bash
docker run -d --name seq -e ACCEPT_EULA=Y -p 5341:80 datalust/seq:latest
```

Access the UI at: http://localhost:5341

### Seq Queries

**Authorization failures in last hour:**
```
@Level = 'Warning' and @Message like '%Authorization Event%' and @Timestamp > now() - 1h
```

**Data access by user:**
```
@Message like '%Data Access%' and @Properties['UserId'] = 'auth0|123456789'
```

**All errors:**
```
@Level = 'Error'
```

**Slow operations (if logging duration):**
```
@Properties['Duration'] > 1000
```

### Disabling Seq

If you don't have Seq running, the application will still work. Serilog handles missing sinks gracefully. To remove Seq configuration entirely, remove the Seq entry from `WriteTo` in `appsettings.json`.

---

## Best Practices

### What to Log

✅ **Do log:**
- Authorization decisions (granted and denied)
- Data access operations (create, update, delete)
- Security events (suspicious activity, failed auth)
- Financial operations (for audit compliance)
- Errors with full context

❌ **Don't log:**
- Passwords or secrets
- Full credit card numbers
- Personal identification numbers
- JWT tokens in full
- Large binary data

### Log Levels

- Use **Information** for successful operations
- Use **Warning** for denied requests and expected failures
- Use **Error** for unexpected failures and security issues
- Use **Debug** only for development troubleshooting

### Structured Logging

Always use structured logging with named parameters:

```csharp
// Good - structured
_logger.LogInformation("Created client {ClientId} for user {UserId}", 
    client.Id, userId);

// Bad - string interpolation
_logger.LogInformation($"Created client {client.Id} for user {userId}");
```

Structured logging enables:
- Efficient searching in log aggregators
- Property-based filtering
- Type-safe log analysis

---

## Related Documentation

| Topic | Document |
|-------|----------|
| Configuration | [CONFIGURATION_MANAGEMENT.md](CONFIGURATION_MANAGEMENT.md) |
| Error Handling | [ERROR_HANDLING.md](ERROR_HANDLING.md) |
| Authentication | [AUTHENTICATION.md](AUTHENTICATION.md) |
| Audit System | [AUDIT_SYSTEM.md](AUDIT_SYSTEM.md) |

### External References

- [Serilog Documentation](https://serilog.net/)
- [Seq Documentation](https://docs.datalust.co/docs)
- [ASP.NET Core Logging](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/)
- [Structured Logging Best Practices](https://messagetemplates.org/)
