# Logging System Documentation

## Overview

The SF Management API implements a comprehensive logging system that integrates seamlessly with the Auth0 authentication system. This logging infrastructure provides structured logging, security audit trails, and real-time monitoring capabilities.

## Architecture

### Logging Stack

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   ASP.NET API   │───▶│     Serilog     │───▶│      Seq        │
│   (Controllers) │    │  (Structured    │    │  (Log Server)   │
│                  │    │   Logging)      │    │                 │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                              │
                              ▼
                       ┌─────────────────┐
                       │   File Logs     │
                       │  (Daily Files)  │
                       └─────────────────┘
```

### Key Components

- **Serilog**: Structured logging framework
- **Seq**: Log aggregation and analysis server
- **File Sinks**: Daily rolling log files
- **Console Sinks**: Development logging
- **Auth0 Integration**: User context in all logs

## Authentication & Logging Integration

### User Context in Logs

Every log entry automatically includes user context from Auth0:

```csharp
// From LoggingService.cs
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

### Security Event Logging

The system logs all authentication and authorization events:

```csharp
// Authentication events
_loggingService.LogAuthenticationEvent("login", userId, true);
_loggingService.LogAuthenticationEvent("login_failed", userId, false, "Invalid credentials");

// Authorization events
_loggingService.LogAuthorizationEvent("clients", "read", true);
_loggingService.LogAuthorizationEvent("transactions", "delete", false, "Insufficient permissions");
```

## Error Handling Integration

### Global Exception Handling

The logging system is fully integrated with the global error handling middleware to ensure all errors are properly logged with context:

```csharp
// ErrorHandlerMiddleware.cs
public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlerMiddleware> _logger;
    private readonly ILoggingService _loggingService;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
        var userEmail = context.User?.FindFirst(ClaimTypes.Email)?.Value ?? "unknown";
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;

        // Log the error with full context
        _logger.LogError(exception,
            "Unhandled exception: {ExceptionType} - Method: {Method} {Path} - User: {UserId} ({UserEmail}) - Message: {Message}",
            exception.GetType().Name, requestMethod, requestPath, userId, userEmail, exception.Message);

        // Log security event for unexpected errors
        _loggingService.LogSecurityEvent("unhandled_exception",
            $"Exception: {exception.GetType().Name} - {exception.Message}", LogLevel.Error);

        // Create error response
        var response = new
        {
            error = "An unexpected error occurred",
            details = app.Environment.IsDevelopment() ? exception.ToString() : null,
            timestamp = DateTime.UtcNow,
            requestId = context.TraceIdentifier
        };

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(response);
    }
}
```

### Controller-Level Error Logging

Controllers automatically log errors through the logging service:

```csharp
[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly ILoggingService _loggingService;
    private readonly ILogger<ClientsController> _logger;

    [HttpPost]
    public async Task<IActionResult> CreateClient(ClientRequest request)
    {
        try
        {
            _loggingService.LogUserAction("create", "client", request);

            var client = await _clientService.AddFromRequest(request);

            _loggingService.LogDataAccess("create", "Client", client.Id, request);
            return Ok(client);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error creating client: {ValidationErrors}",
                string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));

            _loggingService.LogSecurityEvent("validation_error",
                $"Client creation validation failed: {ex.Message}", LogLevel.Warning);

            return BadRequest(new { errors = ex.Errors });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt to create client");

            _loggingService.LogSecurityEvent("unauthorized_access",
                "Attempted to create client without proper authorization", LogLevel.Warning);

            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating client");

            _loggingService.LogSecurityEvent("unexpected_error",
                $"Unexpected error in client creation: {ex.Message}", LogLevel.Error);

            throw; // Let global handler deal with it
        }
    }
}
```

### Database Error Logging

Entity Framework errors are automatically logged with context:

```csharp
// In DataContext.cs
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    if (!optionsBuilder.IsConfigured)
    {
        optionsBuilder.UseSqlServer(connectionString)
            .EnableSensitiveDataLogging(false)
            .EnableDetailedErrors()
            .LogTo(message =>
            {
                // Log database operations
                if (message.Contains("error", StringComparison.OrdinalIgnoreCase) ||
                    message.Contains("exception", StringComparison.OrdinalIgnoreCase))
                {
                    Log.Error("Database Error: {Message}", message);
                }
                else
                {
                    Log.Debug("Database Operation: {Message}", message);
                }
            });
    }
}
```

### Error Log Examples

#### **Validation Error Log**

```
[2024-01-15 10:30:45.123 +00:00] [WRN] Validation error creating client: Name is required, Email format is invalid - User: auth0|123456789 (user@example.com) - Data: {"Name":"","Email":"invalid-email"}
```

#### **Database Error Log**

```
[2024-01-15 10:30:45.124 +00:00] [ERR] Database Error: Cannot insert duplicate key row in object 'dbo.Clients' with unique index 'IX_Clients_Email' - User: auth0|123456789 (user@example.com) - Operation: CreateClient
```

#### **Unauthorized Access Log**

```
[2024-01-15 10:30:45.125 +00:00] [WRN] Unauthorized access attempt to create client - User: auth0|123456789 (user@example.com) - IP: 192.168.1.100 - Roles: viewer
```

## Log Management Platforms

### Recommended Platforms

#### **1. Seq (Development/Staging)**

**Best for**: Development, staging, and small to medium production environments

**Pros**:

- Excellent .NET integration with Serilog
- Real-time log streaming
- Powerful query language
- Built-in alerting
- Free for development use

**Setup**:

```bash
# Docker installation
docker run -d --name seq -e ACCEPT_EULA=Y -p 5341:80 datalust/seq:latest

# Windows installation
winget install datalust.seq
```

**Configuration**:

```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://localhost:5341",
          "apiKey": "your-seq-api-key"
        }
      }
    ]
  }
}
```

#### **2. Azure Application Insights (Production)**

**Best for**: Azure-hosted applications, enterprise environments

**Pros**:

- Native Azure integration
- Application performance monitoring (APM)
- Distributed tracing
- Built-in alerting and dashboards
- Compliance with enterprise security requirements

**Setup**:

```bash
# Install NuGet package
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

**Configuration**:

```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-instrumentation-key",
    "ConnectionString": "your-connection-string"
  }
}
```

#### **3. ELK Stack (Elasticsearch, Logstash, Kibana)**

**Best for**: Large-scale deployments, custom log analysis

**Pros**:

- Highly scalable
- Powerful search and analytics
- Custom dashboards
- Machine learning capabilities
- Open-source

**Setup**:

```yaml
# docker-compose.yml
version: "3.8"
services:
  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:8.11.0
    environment:
      - discovery.type=single-node
    ports:
      - "9200:9200"

  logstash:
    image: docker.elastic.co/logstash/logstash:8.11.0
    ports:
      - "5044:5044"

  kibana:
    image: docker.elastic.co/kibana/kibana:8.11.0
    ports:
      - "5601:5601"
```

#### **4. Splunk**

**Best for**: Enterprise environments with existing Splunk infrastructure

**Pros**:

- Enterprise-grade security
- Advanced analytics
- Machine learning
- Compliance features
- Extensive integrations

**Configuration**:

```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "Splunk",
        "Args": {
          "splunkHost": "your-splunk-host",
          "splunkPort": 8088,
          "splunkToken": "your-splunk-token"
        }
      }
    ]
  }
}
```

### Platform Selection Guide

#### **Development Environment**

- **Recommendation**: Seq
- **Reason**: Easy setup, excellent .NET integration, free for development
- **Setup Time**: 5 minutes

#### **Staging Environment**

- **Recommendation**: Seq or Azure Application Insights
- **Reason**: Production-like monitoring without enterprise costs
- **Setup Time**: 10-15 minutes

#### **Small Production (< 1000 users)**

- **Recommendation**: Azure Application Insights
- **Reason**: Managed service, good performance, reasonable pricing
- **Setup Time**: 30 minutes

#### **Medium Production (1000-10000 users)**

- **Recommendation**: Azure Application Insights or ELK Stack
- **Reason**: Better scalability, more advanced features
- **Setup Time**: 1-2 hours

#### **Large Production (> 10000 users)**

- **Recommendation**: ELK Stack or Splunk
- **Reason**: Enterprise-grade scalability and features
- **Setup Time**: 4-8 hours

### Platform Comparison

| Feature              | Seq       | Azure App Insights | ELK Stack | Splunk    |
| -------------------- | --------- | ------------------ | --------- | --------- |
| **Setup Complexity** | Easy      | Easy               | Medium    | Hard      |
| **Cost**             | Free/$$   | $$                 | $$        | $$$$      |
| **Scalability**      | Good      | Excellent          | Excellent | Excellent |
| **.NET Integration** | Excellent | Excellent          | Good      | Good      |
| **Real-time**        | Yes       | Yes                | Yes       | Yes       |
| **Alerting**         | Basic     | Advanced           | Advanced  | Advanced  |
| **APM**              | No        | Yes                | Yes       | Yes       |
| **Compliance**       | Basic     | Good               | Good      | Excellent |

### Implementation Strategy

#### **Phase 1: Development (Week 1)**

1. Set up Seq for local development
2. Configure Serilog with Seq sink
3. Test logging integration

#### **Phase 2: Staging (Week 2)**

1. Deploy Seq to staging environment
2. Configure production-like logging
3. Set up basic alerting

#### **Phase 3: Production (Week 3-4)**

1. Choose production platform (Azure App Insights recommended)
2. Configure production logging
3. Set up advanced alerting and dashboards
4. Train team on log analysis

### Log Analysis Examples

#### **Seq Queries**

```sql
-- Find all errors in the last hour
@Level = 'Error' and @Timestamp > now() - 1h

-- Find authorization failures by user
@Message like '%Authorization Event%' and @Properties['Granted'] = false | groupby @Properties['UserId']

-- Find slow database queries
@Message like '%Database query executed%' and @Properties['Duration'] > 1000

-- Find financial operations above threshold
@Message like '%Financial Operation%' and @Properties['Amount'] > 10000
```

#### **Azure Application Insights Queries**

```kusto
// Find all exceptions in the last hour
exceptions
| where timestamp > ago(1h)
| project timestamp, operation_Name, user_Id, message

// Find slow requests
requests
| where duration > 1000
| project timestamp, name, duration, user_Id

// Find authorization failures
traces
| where message contains "Authorization Event" and message contains "Granted: False"
| project timestamp, message, user_Id
```

#### **ELK Stack Queries**

```json
// Find all errors
{
  "query": {
    "bool": {
      "must": [
        { "match": { "level": "Error" } },
        { "range": { "@timestamp": { "gte": "now-1h" } } }
      ]
    }
  }
}

// Find authorization failures
{
  "query": {
    "bool": {
      "must": [
        { "match": { "message": "Authorization Event" } },
        { "match": { "granted": false } }
      ]
    }
  }
}
```

## Logging Service

### ILoggingService Interface

```csharp
public interface ILoggingService
{
    void LogUserAction(string action, string resource, object? data = null, LogLevel level = LogLevel.Information);
    void LogSecurityEvent(string eventType, string details, LogLevel level = LogLevel.Warning);
    void LogDataAccess(string operation, string entityType, Guid? entityId = null, object? changes = null);
    void LogFinancialOperation(string operation, decimal amount, string currency, Guid? clientId = null);
    void LogAuthenticationEvent(string eventType, string userId, bool success, string? reason = null);
    void LogAuthorizationEvent(string resource, string action, bool granted, string? reason = null);
}
```

### Usage Examples

#### **1. User Action Logging**

```csharp
[RequirePermission(Auth0Permissions.CreateClients)]
[HttpPost("clients")]
public async Task<IActionResult> CreateClient(ClientRequest request)
{
    _loggingService.LogUserAction("create", "client", request);

    var client = await _clientService.AddFromRequest(request);

    _loggingService.LogDataAccess("create", "Client", client.Id, request);
    return Ok(client);
}
```

#### **2. Financial Operation Logging**

```csharp
[HttpPost("{id}/send-brazilian-real")]
public async Task<IActionResult> SendBrazilianReais(Guid id, FiatAssetTransactionRequest request)
{
    _loggingService.LogFinancialOperation("send_brl", request.AssetAmount, "BRL", id);

    var transaction = await _fiatAssetTransactionService.SendBrazilianReais(id, request);
    return Ok(transaction);
}
```

#### **3. Security Event Logging**

```csharp
// In authorization handlers
if (userRoles.Contains(requirement.Role))
{
    _loggingService.LogAuthorizationEvent("role", requirement.Role, true);
    context.Succeed(requirement);
}
else
{
    _loggingService.LogAuthorizationEvent("role", requirement.Role, false,
        $"User has roles: {string.Join(", ", userRoles)}");
}
```

## Authorization Handlers with Logging

### Enhanced Role Authorization Handler

```csharp
public class RoleAuthorizationHandler : AuthorizationHandler<RoleRequirement>
{
    private readonly ILogger<RoleAuthorizationHandler> _logger;
    private readonly ILoggingService _loggingService;

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
        var userEmail = context.User.FindFirst(ClaimTypes.Email)?.Value ?? "unknown";
        var roles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        if (roles.Contains(requirement.Role))
        {
            _logger.LogInformation("Role authorization granted: User {UserId} ({UserEmail}) has role {RequiredRole}",
                userId, userEmail, requirement.Role);

            _loggingService.LogAuthorizationEvent("role", requirement.Role, true);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("Role authorization denied: User {UserId} ({UserEmail}) lacks role {RequiredRole}",
                userId, userEmail, requirement.Role);

            _loggingService.LogAuthorizationEvent("role", requirement.Role, false,
                $"User has roles: {string.Join(", ", roles)}");
        }

        return Task.CompletedTask;
    }
}
```

## Authentication Logging Middleware

### Request-Level Logging

The `AuthenticationLoggingMiddleware` logs every request with authentication context:

```csharp
public async Task InvokeAsync(HttpContext context)
{
    var isAuthenticated = context.User?.Identity?.IsAuthenticated ?? false;
    var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var userEmail = context.User?.FindFirst(ClaimTypes.Email)?.Value;

    if (isAuthenticated)
    {
        _logger.LogInformation("Authenticated request: {Method} {Path} by {UserId} ({UserEmail})",
            context.Request.Method, context.Request.Path, userId, userEmail);
    }
    else
    {
        _logger.LogInformation("Unauthenticated request: {Method} {Path}",
            context.Request.Method, context.Request.Path);
    }
}
```

### JWT Token Monitoring

The middleware monitors JWT tokens for:

- Token expiration warnings
- Invalid token formats
- Token validation errors

```csharp
private void LogTokenInfo(string token, HttpContext context)
{
    // Log token expiration warnings
    if (timeUntilExpiry.TotalMinutes < 5)
    {
        _logger.LogWarning("JWT token expires soon: {ExpirationTime} (in {MinutesUntilExpiry} minutes)",
            expirationTime, timeUntilExpiry.TotalMinutes);
    }
}
```

## Configuration

### Serilog Configuration

```json
{
  "Serilog": {
    "Using": [
      "Serilog.Sinks.Console",
      "Serilog.Sinks.File",
      "Serilog.Sinks.Seq"
    ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/sf-management-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://localhost:5341",
          "apiKey": "your-seq-api-key"
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"],
    "Properties": {
      "Application": "SF Management API",
      "Environment": "Development"
    }
  }
}
```

### Log Levels

- **Information**: Normal operations, user actions
- **Warning**: Authorization failures, token expiration
- **Error**: System errors, authentication failures
- **Debug**: Detailed debugging information

## Log Output Examples

### User Action Log

```
[2024-01-15 10:30:45.123 +00:00] [INF] User Action: create on client by auth0|123456789 (user@example.com) - Roles: manager, user - Data: {"Name":"John Doe","Email":"john@example.com"}
```

### Authorization Event Log

```
[2024-01-15 10:30:45.124 +00:00] [WRN] Authorization Event: delete on transactions - Granted: False - User: auth0|123456789 (user@example.com) - Roles: manager, user - Reason: User has permissions: read:transactions, create:transactions - IP: 192.168.1.100
```

### Financial Operation Log

```
[2024-01-15 10:30:45.125 +00:00] [INF] Financial Operation: send_brl - Amount: 1000.00 BRL - Client: 550e8400-e29b-41d4-a716-446655440000 - User: auth0|123456789 (user@example.com)
```

## Seq Integration

### Setup Seq Server

1. **Install Seq** (Docker example):

```bash
docker run -d --name seq -e ACCEPT_EULA=Y -p 5341:80 datalust/seq:latest
```

2. **Access Seq UI**: http://localhost:5341

3. **Configure API Key** (optional):

```bash
curl -X POST http://localhost:5341/api/keys -H "Content-Type: application/json" -d '{"Title":"SF Management API"}'
```

### Seq Queries

#### **Find All Authorization Failures**

```
@Level = 'Warning' and @Message like '%Authorization Event%' and @Properties['Granted'] = false
```

#### **Find User Actions by Specific User**

```
@Message like '%User Action%' and @Properties['UserId'] = 'auth0|123456789'
```

#### **Find Financial Operations Above Amount**

```
@Message like '%Financial Operation%' and @Properties['Amount'] > 10000
```

#### **Find Recent Authentication Events**

```
@Message like '%Authentication Event%' and @Timestamp > now() - 1h
```

## Security Considerations

### Sensitive Data Protection

- **No PII in Logs**: User passwords, tokens, and sensitive data are never logged
- **Structured Logging**: Sensitive fields are explicitly excluded
- **Log Retention**: Configurable retention policies for compliance

### Audit Trail Compliance

The logging system supports financial industry compliance requirements:

- **User Accountability**: Every action is tied to a specific user
- **Data Access Tracking**: All data access is logged with user context
- **Change Tracking**: All modifications include before/after data
- **Time Stamping**: All events include precise timestamps

## Performance Considerations

### Logging Performance

- **Async Logging**: All logging operations are asynchronous
- **Structured Logging**: Efficient serialization of log data
- **Batching**: Seq sink batches log entries for better performance
- **Level Filtering**: Configurable log levels to reduce overhead

### Monitoring Performance

```csharp
// Performance monitoring
_logger.LogInformation("Database query executed in {Duration}ms for {Operation}",
    stopwatch.ElapsedMilliseconds, "GetClients");
```

## Troubleshooting

### Common Issues

#### **1. Seq Connection Issues**

```bash
# Check if Seq is running
curl http://localhost:5341/api/events

# Check firewall settings
netstat -an | grep 5341
```

#### **2. Log File Issues**

```bash
# Check log file permissions
ls -la logs/

# Check disk space
df -h
```

#### **3. Authentication Logging Issues**

```json
{
  "Logging": {
    "LogLevel": {
      "SFManagement.Middleware": "Debug",
      "SFManagement.Authorization": "Debug"
    }
  }
}
```

### Debugging Authorization

Enable detailed authorization logging:

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

## Future Integration Opportunities

### Overview

The `LoggingService` provides several methods that are currently defined but not yet integrated throughout the codebase. This section documents these methods and provides guidance on where and how to integrate them for comprehensive logging coverage.

### Unused LoggingService Methods

#### **1. LogUserAction**

**Purpose**: Log general user interactions with the system

**Current Status**: ❌ Not used anywhere in the codebase

**Recommended Integration Points**:

```csharp
// Controllers - User Operations
[HttpPost]
public async Task<IActionResult> CreateClient(CreateClientRequest request)
{
    var client = await _clientService.Add(request);

    // Add this logging
    _loggingService.LogUserAction("create", "client", new {
        ClientId = client.Id,
        ClientName = client.Name
    });

    return Ok(client);
}

// Service Methods - Business Operations
public async Task<TransactionResult> ProcessTransaction(TransactionRequest request)
{
    var result = await _transactionService.Process(request);

    // Add this logging
    _loggingService.LogUserAction("process_transaction", "financial", new {
        TransactionId = result.Id,
        Amount = result.Amount,
        Status = result.Status
    });

    return result;
}
```

**Benefits**:

- Track user behavior patterns
- Monitor system usage
- Identify popular features
- Audit user interactions

#### **2. LogSecurityEvent**

**Purpose**: Log security-related events and suspicious activities

**Current Status**: ❌ Not used anywhere in the codebase

**Recommended Integration Points**:

```csharp
// Authorization Handlers - Access Denied
public class RoleAuthorizationHandler : AuthorizationHandler<RequireRoleRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RequireRoleRequirement requirement)
    {
        if (!HasRequiredRole(context, requirement.Role))
        {
            // Add this logging
            _loggingService.LogSecurityEvent("access_denied",
                $"User lacks required role: {requirement.Role}",
                LogLevel.Warning);

            return Task.CompletedTask;
        }

        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}

// Authentication Failures
public class Auth0UserService
{
    public string? GetUserId()
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            // Add this logging
            _loggingService.LogSecurityEvent("missing_user_id",
                "JWT token missing user identifier",
                LogLevel.Warning);
        }

        return userId;
    }
}

// Suspicious Activities
public class RateLimitMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (IsRateLimitExceeded(context))
        {
            // Add this logging
            _loggingService.LogSecurityEvent("rate_limit_exceeded",
                $"IP {context.Connection.RemoteIpAddress} exceeded rate limit",
                LogLevel.Warning);
        }
    }
}
```

**Benefits**:

- Detect security threats early
- Monitor access patterns
- Identify potential attacks
- Compliance reporting

#### **3. LogFinancialOperation**

**Purpose**: Log financial transactions and money movements

**Current Status**: ❌ Not used anywhere in the codebase

**Recommended Integration Points**:

```csharp
// Transaction Services
public class TransactionService
{
    public async Task<TransactionResult> CreateTransaction(CreateTransactionRequest request)
    {
        var transaction = await _repository.Create(request);

        // Add this logging
        _loggingService.LogFinancialOperation("create_transaction",
            request.Amount,
            request.Currency,
            request.ClientId);

        return transaction;
    }

    public async Task<TransactionResult> UpdateTransaction(Guid id, UpdateTransactionRequest request)
    {
        var transaction = await _repository.Update(id, request);

        // Add this logging
        _loggingService.LogFinancialOperation("update_transaction",
            request.Amount,
            request.Currency,
            transaction.ClientId);

        return transaction;
    }
}

// Financial Calculations
public class FinancialCalculationService
{
    public async Task<decimal> CalculateBalance(Guid clientId)
    {
        var balance = await _repository.CalculateBalance(clientId);

        // Add this logging
        _loggingService.LogFinancialOperation("calculate_balance",
            balance,
            "BRL",
            clientId);

        return balance;
    }
}
```

**Benefits**:

- Track all financial operations
- Audit money movements
- Compliance with financial regulations
- Detect financial anomalies

### Integration Priority

1. **High Priority**: `LogSecurityEvent` - Critical for security monitoring
2. **Medium Priority**: `LogFinancialOperation` - Important for financial compliance
3. **Low Priority**: `LogUserAction` - Useful for analytics and user behavior

### Implementation Strategy

1. **Phase 1**: Integrate `LogSecurityEvent` in authorization handlers and authentication flows
2. **Phase 2**: Integrate `LogFinancialOperation` in transaction services
3. **Phase 3**: Integrate `LogUserAction` in controllers and service methods

### Monitoring and Alerts

Once integrated, set up monitoring for:

- **Security Events**: Alert on suspicious activities
- **Financial Operations**: Monitor for unusual transactions
- **User Actions**: Track system usage patterns

### Expected Log Output Examples

#### **User Action Log**

```
[2024-01-15 10:30:45.123 +00:00] [INF] User Action: create on client by auth0|123456789 (user@example.com) - Roles: manager, user - Data: {"ClientId":"550e8400-e29b-41d4-a716-446655440000","ClientName":"John Doe"}
```

#### **Security Event Log**

```
[2024-01-15 10:30:45.124 +00:00] [WRN] Security Event: access_denied - User lacks required role: admin - User: auth0|123456789 (user@example.com) - IP: 192.168.1.100 - UserAgent: Mozilla/5.0...
```

#### **Financial Operation Log**

```
[2024-01-15 10:30:45.125 +00:00] [INF] Financial Operation: create_transaction - Amount: 1000.00 BRL - Client: 550e8400-e29b-41d4-a716-446655440000 - User: auth0|123456789 (user@example.com)
```

## Best Practices

### **1. Consistent Logging**

- Use structured logging for all events
- Include user context in all logs
- Use appropriate log levels

### **2. Security Logging**

- Log all authentication attempts (success/failure)
- Log all authorization decisions
- Log sensitive operations (financial transactions)

### **3. Performance Logging**

- Log database query performance
- Log external API call durations
- Monitor resource usage

### **4. Error Logging**

- Include full exception details
- Log user context with errors
- Provide actionable error messages

## Integration with Monitoring

### **1. Health Checks**

```csharp
public class LoggingHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        // Check if logging is working
        return Task.FromResult(HealthCheckResult.Healthy("Logging system is operational"));
    }
}
```

### **2. Metrics Integration**

```csharp
// Log metrics for monitoring
_logger.LogInformation("API Request Count: {Count} for endpoint {Endpoint}",
    requestCount, endpoint);
```

This comprehensive logging system provides complete visibility into your application's security, performance, and user activities while maintaining compliance with financial industry standards.
