# Error Handling

## Overview

The SF Management system uses a structured exception hierarchy and centralized error handling middleware to provide consistent, informative error responses across all API endpoints. This system ensures proper logging, user-friendly messages, and appropriate HTTP status codes.

---

## Table of Contents

1. [Exception Hierarchy](#exception-hierarchy)
2. [Error Handling Middleware](#error-handling-middleware)
3. [HTTP Status Code Mapping](#http-status-code-mapping)
4. [Error Response Format](#error-response-format)
5. [Usage Examples](#usage-examples)
6. [Best Practices](#best-practices)

---

## Exception Hierarchy

```
Exception
└── BusinessException (base for all business errors)
    ├── ValidationException
    ├── EntityNotFoundException
    ├── DuplicateEntityException
    └── BusinessRuleException
```

### BusinessException

Base class for all business logic errors.

**File**: `Domain/Exceptions/BusinessException.cs`

```csharp
public class BusinessException : Exception
{
    public string Code { get; }
    public new object? Data { get; }

    public BusinessException(string message, string code = "BUSINESS_ERROR", object? data = null)
    public BusinessException(string message, Exception innerException, string code, object? data = null)
}
```

| Property | Description |
|----------|-------------|
| `Code` | Machine-readable error code (e.g., "BUSINESS_ERROR") |
| `Data` | Additional context data (optional) |
| `Message` | Human-readable error message |

### ValidationException

For input validation failures.

```csharp
public class ValidationException : BusinessException
{
    public List<ValidationError> ValidationErrors { get; }

    public ValidationException(string message, List<ValidationError> validationErrors)
    public ValidationException(List<ValidationError> validationErrors)
}

public class ValidationError
{
    public string Field { get; set; }
    public string Message { get; set; }
    public string? Code { get; set; }
}
```

**Usage:**
```csharp
throw new ValidationException(new List<ValidationError>
{
    new("Name", "Name is required", "REQUIRED"),
    new("Email", "Invalid email format", "INVALID_FORMAT")
});
```

### EntityNotFoundException

For when a requested entity doesn't exist.

```csharp
public class EntityNotFoundException : BusinessException
{
    public string EntityType { get; }
    public object EntityId { get; }

    public EntityNotFoundException(string entityType, object entityId)
}
```

**Usage:**
```csharp
throw new EntityNotFoundException("Client", clientId);
// Message: "Client with ID {clientId} was not found"
```

### DuplicateEntityException

For unique constraint violations.

```csharp
public class DuplicateEntityException : BusinessException
{
    public string EntityType { get; }
    public string Field { get; }
    public object Value { get; }

    public DuplicateEntityException(string entityType, string field, object value)
}
```

**Usage:**
```csharp
throw new DuplicateEntityException("Client", "GovernmentNumber", "123.456.789-00");
// Message: "Client with GovernmentNumber '123.456.789-00' already exists"
```

### BusinessRuleException

For business rule violations.

```csharp
public class BusinessRuleException : BusinessException
{
    public string RuleName { get; }

    public BusinessRuleException(string ruleName, string message)
}
```

**Usage:**
```csharp
throw new BusinessRuleException(
    "CLIENT_HAS_DEPENDENCIES",
    "Cannot delete client because it has active transactions"
);
```

---

## Error Handling Middleware

The `ErrorHandlerMiddleware` intercepts all unhandled exceptions and converts them to consistent API responses.

**File**: `Api/Middleware/ErrorHandlerMiddleware.cs`

### Registration

```csharp
// Program.cs
app.UseMiddleware<ErrorHandlerMiddleware>();
```

### Processing Flow

```
Request → Controller → Service → Exception thrown
                                       ↓
                            ErrorHandlerMiddleware
                                       ↓
                            GetErrorDetails(exception)
                                       ↓
                            Map to HTTP status code
                                       ↓
                            Log (Error or Warning)
                                       ↓
                            JSON Response
```

### Logging Behavior

| Exception Type | Log Level | Rationale |
|---------------|-----------|-----------|
| `ValidationException` | Warning | Expected user input errors |
| `EntityNotFoundException` | Warning | Expected "not found" cases |
| `DuplicateEntityException` | Warning | Expected constraint violations |
| `BusinessRuleException` | Warning | Expected business logic rejections |
| `BusinessException` | Warning | General business errors |
| `UnauthorizedAccessException` | Error | Security concern |
| `TimeoutException` | Error | Infrastructure issue |
| Unknown exceptions | Error | Unexpected errors |

### Context Captured

The middleware captures comprehensive context for debugging:

```csharp
var errorContext = new
{
    RequestId = requestId,
    Method = method,
    Path = path,
    QueryString = queryString,
    UserId = userId,
    UserEmail = userEmail,
    UserAgent = userAgent,
    IpAddress = ipAddress,
    ExceptionType = exception.GetType().Name,
    ExceptionMessage = exception.Message,
    StackTrace = /* Development only */,
    InnerException = exception.InnerException?.Message,
    Timestamp = DateTime.UtcNow
};
```

---

## HTTP Status Code Mapping

| Exception Type | HTTP Status | Error Code |
|---------------|-------------|------------|
| `ValidationException` | 400 Bad Request | `VALIDATION_ERROR` |
| `EntityNotFoundException` | 404 Not Found | `ENTITY_NOT_FOUND` |
| `DuplicateEntityException` | 409 Conflict | `DUPLICATE_ENTITY` |
| `BusinessRuleException` | 409 Conflict | `BUSINESS_RULE_VIOLATION` |
| `BusinessException` | 400 Bad Request | `BUSINESS_ERROR` |
| `UnauthorizedAccessException` | 401 Unauthorized | `UNAUTHORIZED` |
| `ArgumentException` | 400 Bad Request | `INVALID_ARGUMENT` |
| `KeyNotFoundException` | 404 Not Found | `NOT_FOUND` |
| `TimeoutException` | 408 Request Timeout | `TIMEOUT` |
| All other exceptions | 500 Internal Server Error | `INTERNAL_ERROR` |

---

## Error Response Format

### Standard Error Response

```json
{
  "error": "Human readable error message",
  "code": "ERROR_CODE",
  "requestId": "0HN5QKJI8J9RA:00000001",
  "timestamp": "2025-01-14T12:00:00Z",
  "path": "/api/v1/client",
  "method": "POST"
}
```

### Validation Error Response

```json
{
  "error": "Validation failed",
  "code": "VALIDATION_ERROR",
  "requestId": "0HN5QKJI8J9RA:00000001",
  "timestamp": "2025-01-14T12:00:00Z",
  "path": "/api/v1/client",
  "method": "POST",
  "validationErrors": [
    {
      "field": "Name",
      "message": "Name is required",
      "code": "REQUIRED"
    },
    {
      "field": "Email",
      "message": "Invalid email format",
      "code": "INVALID_FORMAT"
    }
  ]
}
```

### Development Mode Additional Details

In development environment, responses include additional debugging information:

```json
{
  "error": "...",
  "code": "...",
  "details": {
    "exceptionType": "NullReferenceException",
    "stackTrace": "at SFManagement.Services...",
    "innerException": "Object reference not set..."
  }
}
```

---

## Usage Examples

### In Services

```csharp
public async Task<Client> GetClient(Guid id)
{
    var client = await _context.Clients.FindAsync(id);
    
    if (client == null)
        throw new EntityNotFoundException("Client", id);
    
    if (client.DeletedAt.HasValue)
        throw new BusinessException("Client has been deleted", "CLIENT_DELETED");
    
    return client;
}
```

### In Controllers (via BaseAssetHolderController)

```csharp
protected IActionResult HandleValidationException(ValidationException ex)
{
    var problemDetails = new ValidationProblemDetails();
    
    foreach (var error in ex.ValidationErrors)
    {
        if (!problemDetails.Errors.ContainsKey(error.Field))
            problemDetails.Errors[error.Field] = new string[] { };
        
        var errorList = problemDetails.Errors[error.Field].ToList();
        errorList.Add(error.Message);
        problemDetails.Errors[error.Field] = errorList.ToArray();
    }
    
    problemDetails.Title = "Validation Failed";
    problemDetails.Status = StatusCodes.Status400BadRequest;
    problemDetails.Extensions["requestId"] = HttpContext.TraceIdentifier;
    
    return BadRequest(problemDetails);
}
```

### Domain Validation

```csharp
public async Task<Client> AddFromRequest(ClientRequest request)
{
    // Validate using domain service
    var validationResult = await _domainService.ValidateClientCreation(request);
    
    if (!validationResult.IsValid)
    {
        throw new ValidationException(validationResult.Errors);
    }
    
    // Proceed with creation...
}
```

### Business Rules

```csharp
public async Task<bool> DeleteWithValidation(Guid entityId)
{
    var canDelete = await CanDelete(entityId);
    
    if (!canDelete)
    {
        throw new BusinessRuleException(
            $"{typeof(TEntity).Name.ToUpper()}_HAS_DEPENDENCIES",
            $"Cannot delete {typeof(TEntity).Name.ToLower()} because it has active transactions"
        );
    }
    
    // Proceed with deletion...
}
```

---

## Best Practices

### 1. Use Specific Exception Types

```csharp
// Good - specific exception
throw new EntityNotFoundException("Client", id);

// Bad - generic exception
throw new Exception($"Client {id} not found");
```

### 2. Include Error Codes

```csharp
// Good - machine-readable code
throw new BusinessException("Cannot process", "PAYMENT_DECLINED");

// Bad - no code
throw new BusinessException("Cannot process payment");
```

### 3. Provide Context Data

```csharp
throw new BusinessException(
    "Transaction limit exceeded",
    "LIMIT_EXCEEDED",
    new { Limit = 10000, Requested = amount }
);
```

### 4. Don't Expose Sensitive Information

```csharp
// Good - sanitized message
throw new UnauthorizedAccessException("Access denied");

// Bad - exposes internals
throw new Exception($"User {userId} lacks permission {permission}");
```

### 5. Let Middleware Handle Unknown Exceptions

```csharp
// Good - let middleware handle
public async Task DoSomething()
{
    // No try-catch for unexpected errors
    await _service.Process();
}

// Bad - catching and re-throwing
public async Task DoSomething()
{
    try
    {
        await _service.Process();
    }
    catch (Exception ex)
    {
        throw new Exception("Error", ex); // Loss of context
    }
}
```

### 6. Use Validation Collections

```csharp
// Good - all errors at once
var errors = new List<ValidationError>();

if (string.IsNullOrEmpty(request.Name))
    errors.Add(new ValidationError("Name", "Required"));

if (request.Amount < 0)
    errors.Add(new ValidationError("Amount", "Must be positive"));

if (errors.Any())
    throw new ValidationException(errors);

// Bad - throwing on first error
if (string.IsNullOrEmpty(request.Name))
    throw new ValidationException("Name is required");
// User never sees the Amount error
```

---

## Related Documentation

- [LOGGING.md](LOGGING.md) - Logging configuration
- [VALIDATION_SYSTEM.md](VALIDATION_SYSTEM.md) - Input validation
- [API_REFERENCE.md](../06_API/API_REFERENCE.md) - API error responses

