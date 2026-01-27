# Rate Limiting and Performance

## Overview

The SF Management API implements rate limiting and caching strategies to ensure optimal performance and prevent abuse.

---

## Rate Limiting

### Configuration

Rate limiting is configured in `appsettings.json`:

```json
{
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": true,
    "StackBlockedRequests": false,
    "RealIPHeader": "X-Real-IP",
    "ClientIdHeader": "X-ClientId",
    "HttpStatusCode": 429,
    "GeneralRules": [
      {
        "Endpoint": "*",
        "Period": "1m",
        "Limit": 100
      }
    ]
  }
}
```

### Registration

```csharp
// DependencyInjectionExtensions.cs
public static void AddRateLimitServices(this WebApplicationBuilder builder)
{
    builder.Services.Configure<IpRateLimitOptions>(
        builder.Configuration.GetSection("IpRateLimiting"));
    builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
    builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
    builder.Services.AddInMemoryRateLimiting();
}
```

---

## Response Caching

### Endpoint-Level Caching

```csharp
[ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, NoStore = false)]
public virtual async Task<IActionResult> GetBalance(Guid id)
```

### Cache Durations

| Duration | Endpoints |
|----------|-----------|
| 60 sec | Balance, wallet connections |
| 300 sec | Company pool summary |
| 600 sec | Company pool analytics |

---

## Query Optimization

### AsNoTracking for Read Operations

```csharp
return await query.AsNoTracking()
    .Where(x => !x.BaseAssetHolder.DeletedAt.HasValue)
    .ToListAsync();
```

### Selective Includes

Only include necessary navigation properties:

```csharp
.Include(x => x.Category)
.Include(x => x.SenderWalletIdentifier)
    .ThenInclude(wi => wi.AssetPool)
.ToListAsync();
```

---

## Best Practices

1. **Use AsNoTracking** for read-only queries
2. **Limit includes** to required navigation properties
3. **Paginate** large result sets
4. **Cache** expensive calculations
5. **Index** frequently queried columns

---

## Related Documentation

- [API_REFERENCE.md](../06_API/API_REFERENCE.md) - Endpoint documentation
- [SERVICE_LAYER_ARCHITECTURE.md](../02_ARCHITECTURE/SERVICE_LAYER_ARCHITECTURE.md) - Query patterns

