# Controller Layer Architecture

## Overview

The SF Management API uses a layered controller architecture with generic base controllers that provide common functionality. This design enables consistent API behavior across all entity types while allowing for entity-specific customization.

---

## Table of Contents

1. [Controller Hierarchy](#controller-hierarchy)
2. [BaseApiController](#baseapicontroller)
3. [BaseAssetHolderController](#baseassetholdercontroller)
4. [Entity-Specific Controllers](#entity-specific-controllers)
5. [Route Conventions](#route-conventions)
6. [Response Handling](#response-handling)

---

## Controller Hierarchy

```
ControllerBase (ASP.NET Core)
└── BaseApiController<TEntity, TRequest, TResponse>
    ├── CategoryController
    ├── WalletIdentifierController
    ├── AssetPoolController
    ├── ImportedTransactionController
    ├── FiatAssetTransactionController
    ├── DigitalAssetTransactionController
    ├── SettlementTransactionController
    ├── InitialBalanceController
    │
    └── BaseAssetHolderController<TEntity, TRequest, TResponse>
        ├── ClientController
        ├── BankController
        ├── MemberController
        └── PokerManagerController

ControllerBase (ASP.NET Core)
└── CompanyAssetPoolController (standalone)
```

---

## BaseApiController

Provides standard CRUD operations for all entities.

**File**: `Controllers/BaseApiController.cs`

### Definition

```csharp
public class BaseApiController<TEntity, TRequest, TResponse> : ControllerBase 
    where TEntity : BaseDomain
    where TRequest : class
    where TResponse : BaseResponse
{
    private readonly IMapper _mapper;
    private readonly BaseService<TEntity> _service;

    public BaseApiController(BaseService<TEntity> service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }
}
```

### Standard Endpoints

| Method | Route | Description | Status Codes |
|--------|-------|-------------|--------------|
| GET | `/` | List all entities | 200 |
| GET | `/{id}` | Get single entity | 200, 404 |
| POST | `/` | Create entity | 201, 400 |
| PUT | `/{id}` | Update entity | 200, 400, 404 |
| DELETE | `/{id}` | Delete entity | 204, 404 |

### Implementation

```csharp
[HttpGet]
public virtual async Task<IActionResult> Get()
{
    var entities = await _service.List();
    var response = _mapper.Map<List<TResponse>>(entities);
    return Ok(response);
}

[HttpGet("{id}")]
public virtual async Task<IActionResult> Get(Guid id)
{
    var entity = await _service.Get(id);
    if (entity == null)
        return NotFound();
    
    var response = _mapper.Map<TResponse>(entity);
    return Ok(response);
}

[HttpPost]
public virtual async Task<IActionResult> Post(TRequest model)
{
    var entity = _mapper.Map<TEntity>(model);
    var result = await _service.Add(entity);
    var response = _mapper.Map<TResponse>(result);
    return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
}

[HttpPut("{id}")]
public virtual async Task<IActionResult> Put(Guid id, TRequest model)
{
    var entity = _mapper.Map<TEntity>(model);
    var result = await _service.Update(id, entity);
    var response = _mapper.Map<TResponse>(result);
    return Ok(response);
}

[HttpDelete("{id}")]
public virtual async Task<IActionResult> Delete(Guid id)
{
    await _service.Delete(id);
    return NoContent();
}
```

---

## BaseAssetHolderController

Extends BaseApiController with asset holder-specific functionality.

**File**: `Controllers/BaseAssetHolderController.cs`

### Definition

```csharp
public class BaseAssetHolderController<TEntity, TRequest, TResponse> 
    : BaseApiController<TEntity, TRequest, TResponse>
    where TEntity : BaseDomain, IAssetHolder
    where TRequest : BaseAssetHolderRequest
    where TResponse : BaseResponse
```

### Additional Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/wallet-identifiers` | List wallets by type/group |
| GET | `/{id}/wallet-identifiers` | Get entity's wallets |
| GET | `/{id}/statistics` | Get entity statistics |
| GET | `/{id}/can-delete` | Check deletability |
| GET | `/{id}/balance` | Get balance by asset type |
| GET | `/{id}/transactions` | Get transaction statement |

### Error Handling Methods

The controller provides standardized error handling:

```csharp
protected IActionResult HandleValidationException(ValidationException ex)
protected IActionResult HandleEntityNotFoundException(EntityNotFoundException ex)
protected IActionResult HandleDuplicateEntityException(DuplicateEntityException ex)
protected IActionResult HandleBusinessRuleException(BusinessRuleException ex)
protected IActionResult HandleBusinessException(BusinessException ex)
protected IActionResult HandleGenericException(string operation)
```

### Abstract Methods

Derived controllers must implement these methods:

```csharp
protected virtual async Task<TEntity> CreateEntityFromRequest(TRequest request)
protected virtual async Task<TEntity> UpdateEntityFromRequest(Guid id, TRequest request)
```

---

## Entity-Specific Controllers

### ClientController

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class ClientController : BaseAssetHolderController<Client, ClientRequest, ClientResponse>
{
    // Additional endpoints:
    // GET /{id}/client-statistics
    // POST /{id}/send-brazilian-real
    
    protected override async Task<Client> CreateEntityFromRequest(ClientRequest request)
    {
        return await _clientService.AddFromRequest(request);
    }
}
```

### PokerManagerController

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class PokerManagerController : BaseAssetHolderController<PokerManager, PokerManagerRequest, PokerManagerResponse>
{
    // Additional endpoints:
    // POST /{id}/send-brazilian-real
    // GET /{id}/wallet-identifiers-connected
    // POST /{assetHolderId}/settlement-by-date
    // GET /{id}/balance (overridden - returns by AssetGroup)
}
```

---

## Route Conventions

### Base Route Pattern

```
/api/v{version}/[controller]
```

### Version Configuration

```csharp
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
```

### Common Route Patterns

| Pattern | Example | Description |
|---------|---------|-------------|
| `/{id}` | `/api/v1/client/{id}` | Single resource |
| `/{id}/{action}` | `/api/v1/client/{id}/statistics` | Resource action |
| `/action/{param}` | `/api/v1/importedtransaction/file/{fileName}` | Collection action |
| `/{id}/related` | `/api/v1/pokermanager/{id}/wallet-identifiers` | Related resources |

### Response Caching

```csharp
[ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, NoStore = false)]
public virtual async Task<IActionResult> GetBalance(Guid id)
```

| Duration | Endpoints |
|----------|-----------|
| 60 seconds | Balance, wallet-identifiers-connected |
| 300 seconds | Company asset pool summary |
| 600 seconds | Company asset pool analytics |

---

## Response Handling

### Success Responses

| Method | Success Response |
|--------|------------------|
| GET | 200 OK with entity/list |
| POST | 201 Created with entity and location header |
| PUT | 200 OK with updated entity |
| DELETE | 204 No Content |

### Error Responses (RFC 7807)

```csharp
return NotFound(new ProblemDetails
{
    Title = "Entity Not Found",
    Status = StatusCodes.Status404NotFound,
    Detail = ex.Message,
    Extensions = { 
        ["code"] = ex.Code,
        ["requestId"] = HttpContext.TraceIdentifier,
        ["timestamp"] = DateTime.UtcNow
    }
});
```

### Validation Problem Details

```csharp
var problemDetails = new ValidationProblemDetails();
problemDetails.Errors[error.Field] = new[] { error.Message };
problemDetails.Title = "Validation Failed";
problemDetails.Status = StatusCodes.Status400BadRequest;
return BadRequest(problemDetails);
```

---

## Best Practices

### 1. Override vs Extend

```csharp
// Override when behavior differs completely
public override async Task<IActionResult> GetBalance(Guid id)
{
    // PokerManager uses AssetGroup instead of AssetType
}

// Add new endpoints for additional functionality
[HttpGet("{id}/client-statistics")]
public async Task<IActionResult> GetClientStatistics(Guid id)
```

### 2. Use Structured Logging

```csharp
_logger.LogInformation(
    "Retrieving {EntityType} {EntityId} - RequestId: {RequestId}", 
    entityType, id, requestId);
```

### 3. Return Proper Status Codes

```csharp
// 201 for creates
return CreatedAtAction(nameof(Get), new { id = response.Id }, response);

// 204 for deletes
return NoContent();

// 409 for business conflicts
return Conflict(problemDetails);
```

---

## Related Documentation

- [SERVICE_LAYER_ARCHITECTURE.md](SERVICE_LAYER_ARCHITECTURE.md) - Service layer details
- [API_REFERENCE.md](../06_API/API_REFERENCE.md) - Complete endpoint reference
- [ERROR_HANDLING.md](../05_INFRASTRUCTURE/ERROR_HANDLING.md) - Error handling

