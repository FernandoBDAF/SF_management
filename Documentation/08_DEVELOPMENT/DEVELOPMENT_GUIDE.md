# Development Guide

## Overview

This guide provides essential information for developers working on the SF Management system, covering setup, conventions, and common tasks.

---

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- SQL Server (local or remote)
- Visual Studio 2022 / VS Code / Rider
- Auth0 account (for authentication)

### Initial Setup

```bash
# Clone the repository
git clone <repository-url>
cd SF_management

# Restore packages
dotnet restore

# Apply database migrations
dotnet ef database update

# Run the application
dotnet run
```

### Configuration

Copy `appsettings.Development.json.example` to `appsettings.Development.json` and configure:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SFManagement;..."
  },
  "Auth0": {
    "Domain": "your-tenant.auth0.com",
    "Audience": "your-api-identifier"
  }
}
```

---

## Project Structure

```
SF_management/
├── Controllers/
│   ├── BaseApiController.cs
│   ├── BaseAssetHolderController.cs
│   └── v1/                          # Versioned controllers
├── Services/
│   ├── BaseService.cs
│   ├── BaseAssetHolderService.cs
│   └── BaseTransactionService.cs
├── Models/
│   ├── BaseDomain.cs
│   ├── Entities/                    # Asset holders
│   ├── Transactions/                # Transaction types
│   └── AssetInfrastructure/         # Asset models
├── ViewModels/                      # Request/Response models
├── Enums/                           # Enumerations
├── Data/
│   └── DataContext.cs               # EF Core context
├── Exceptions/                      # Custom exceptions
├── Middleware/                      # Custom middleware
├── Authorization/                   # Auth policies
└── Documentation/                   # This documentation
```

---

## Development Patterns

### Adding a New Entity

1. **Create the model** in `Models/`
2. **Add DbSet** to `DataContext.cs`
3. **Create service** extending `BaseService<T>`
4. **Create controller** extending `BaseApiController<T,Req,Res>`
5. **Add ViewModels** for request/response
6. **Register service** in `DependencyInjectionExtensions.cs`
7. **Add AutoMapper mappings** in `AutoMapperProfile.cs`

### Adding a New Endpoint

1. Add method to the appropriate controller
2. Use appropriate HTTP verb attributes
3. Add `[ProducesResponseType]` attributes
4. Implement logging with `ILogger`
5. Handle exceptions appropriately

### Creating a Migration

```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

---

## Coding Conventions

### Naming

| Type | Convention | Example |
|------|------------|---------|
| Classes | PascalCase | `ClientService` |
| Methods | PascalCase | `GetBalancesByAssetType` |
| Variables | camelCase | `baseAssetHolder` |
| Constants | UPPER_SNAKE | `DEFAULT_PAGE_SIZE` |
| Interfaces | IPrefix | `IAssetHolderDomainService` |

### Async/Await

- Always use `async/await` for database operations
- Suffix async methods with `Async` when public

### Error Handling

- Use custom exceptions (`BusinessException`, `ValidationException`)
- Let `ErrorHandlerMiddleware` handle responses
- Log at appropriate levels

---

## Testing

### API Testing

Use Swagger UI at `/swagger` for interactive API testing.

### Authentication Testing

1. Get token from Auth0
2. Click "Authorize" in Swagger
3. Enter: `Bearer <your-token>`

---

## Common Tasks

### Checking Balance Calculations

Balance methods are in `BaseAssetHolderService`:
- `GetBalancesByAssetType()` - Per asset type
- `GetBalancesByAssetGroup()` - Per asset group

### Working with Transactions

All transaction services extend `BaseTransactionService<T>`:
- `FiatAssetTransactionService`
- `DigitalAssetTransactionService`
- `SettlementTransactionService`

### Adding Validation

1. Create `FluentValidation` validator for requests
2. Add domain validation in `IAssetHolderDomainService`
3. Add service-level validation as needed

---

## Debugging Tips

1. **Check RequestId** in error responses for log correlation
2. **Use SQL Profiler** to view generated queries
3. **Enable Development mode** for detailed error responses
4. **Check EF logging** for query performance

---

## Related Documentation

- [SERVICE_LAYER_ARCHITECTURE.md](../02_ARCHITECTURE/SERVICE_LAYER_ARCHITECTURE.md)
- [CONTROLLER_LAYER_ARCHITECTURE.md](../02_ARCHITECTURE/CONTROLLER_LAYER_ARCHITECTURE.md)
- [ERROR_HANDLING.md](../05_INFRASTRUCTURE/ERROR_HANDLING.md)
- [API_REFERENCE.md](../06_API/API_REFERENCE.md)

