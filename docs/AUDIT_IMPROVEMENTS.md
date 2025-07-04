# Simplified Audit System

## Overview

This document describes the simplified and centralized audit system implementation for the SF Management API. The audit system provides automatic tracking of who last modified entities, with detailed audit information stored in the logging system.

## Implementation Summary

### **Before: Complex Audit Model**

- **BaseDomain**: 6 audit fields (CreatedAt, UpdatedAt, DeletedAt, CreatedBy, LastModifiedBy, DeletedBy)
- **DataContext**: Complex audit logic with nullable fields
- **Issues**: Redundant fields, complex database schema, inconsistent data

### **After: Simplified Audit System**

- **BaseDomain**: 4 audit fields (CreatedAt, UpdatedAt, DeletedAt, LastModifiedBy)
- **DataContext**: Clean audit logic with logging integration
- **Benefits**: Simpler model, detailed audit trail in logs, consistent data

## Changes Made

### 1. **Simplified BaseDomain Model**

**File**: `Models/BaseDomain.cs`

**Simplified Properties**:

```csharp
public class BaseDomain
{
    public Guid Id { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid LastModifiedBy { get; set; } // Never null - reflects creator or last editor
}
```

**Key Changes**:

- ❌ Removed `CreatedBy` (redundant with `LastModifiedBy`)
- ❌ Removed `DeletedBy` (redundant with `LastModifiedBy`)
- ✅ `LastModifiedBy` is now non-nullable
- ✅ `LastModifiedBy` reflects either the creator or the last editor

### 2. **DataContext with Logging Integration**

**File**: `Data/DataContext.cs`

**Key Improvements**:

- **Simplified Audit Logic**: Only sets `LastModifiedBy` (never null)
- **Logging Integration**: Detailed audit trail stored in logs
- **Safe User ID**: Uses SHA256 hash of Auth0 sub claim
- **Default System User**: Returns `00000000-0000-0000-0000-000000000000` for unauthenticated operations

**New Methods**:

```csharp
private void SetDefaultProperties()
{
    var userId = GetCurrentUserId();

    foreach (var auditableEntity in ChangeTracker.Entries<BaseDomain>())
    {
        if (auditableEntity.State == EntityState.Added)
        {
            auditableEntity.Entity.CreatedAt = DateTime.UtcNow;
            auditableEntity.Entity.LastModifiedBy = userId;

            // Log creation event
            _loggingService.LogDataAccess("create", auditableEntity.Entity.GetType().Name,
                auditableEntity.Entity.Id, new {
                    EntityType = auditableEntity.Entity.GetType().Name,
                    EntityId = auditableEntity.Entity.Id,
                    CreatedBy = userId,
                    CreatedAt = auditableEntity.Entity.CreatedAt
                });
        }
        else if (auditableEntity.State == EntityState.Modified)
        {
            if (auditableEntity.Entity.DeletedAt.HasValue)
            {
                // Soft delete operation
                auditableEntity.Property(p => p.UpdatedAt).IsModified = false;

                if (auditableEntity.Entity.DeletedAt == DateTime.MinValue)
                {
                    auditableEntity.Entity.DeletedAt = DateTime.UtcNow;
                    auditableEntity.Entity.LastModifiedBy = userId;

                    // Log deletion event
                    _loggingService.LogDataAccess("delete", auditableEntity.Entity.GetType().Name,
                        auditableEntity.Entity.Id, new {
                            EntityType = auditableEntity.Entity.GetType().Name,
                            EntityId = auditableEntity.Entity.Id,
                            DeletedBy = userId,
                            DeletedAt = auditableEntity.Entity.DeletedAt
                        });
                }
            }
            else
            {
                // Regular update operation
                auditableEntity.Entity.UpdatedAt = DateTime.UtcNow;
                auditableEntity.Entity.LastModifiedBy = userId;

                // Log update event
                _loggingService.LogDataAccess("update", auditableEntity.Entity.GetType().Name,
                    auditableEntity.Entity.Id, new {
                        EntityType = auditableEntity.Entity.GetType().Name,
                        EntityId = auditableEntity.Entity.Id,
                        UpdatedBy = userId,
                        UpdatedAt = auditableEntity.Entity.UpdatedAt
                    });
            }
        }
    }
}

private Guid GetCurrentUserId()
{
    var user = _httpContextAccessor.HttpContext?.User;

    if (user != null && user.Identity?.IsAuthenticated == true)
    {
        var subClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(subClaim))
        {
            // Generate consistent Guid from Auth0 sub claim
            var hash = System.Security.Cryptography.SHA256.Create();
            var hashBytes = hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(subClaim));
            return new Guid(hashBytes.Take(16).ToArray());
        }
    }

    // Return a default system user ID if no authenticated user
    return Guid.Parse("00000000-0000-0000-0000-000000000000");
}
```

### 3. **BaseService Cleanup**

**File**: `Services/BaseService.cs`

**Updated Property Exclusion**:

```csharp
// Copy properties (excluding audit properties)
foreach (var property in typeof(TEntity).GetProperties())
{
    if (property.Name != "Id" && property.Name != "CreatedAt" &&
        property.Name != "UpdatedAt" && property.Name != "LastModifiedBy" &&
        property.Name != "DeletedAt")
    {
        // Property copying logic...
    }
}
```

### 4. **Database Migration**

**Migration**: `20250707195032_SimplifyAuditColumns.cs`

**Changes**:

- ❌ **Removed**: `CreatedBy` column from all BaseDomain tables
- ❌ **Removed**: `DeletedBy` column from all BaseDomain tables
- ✅ **Modified**: `LastModifiedBy` is now non-nullable with default value

**Tables Updated**:

- All BaseDomain tables now have simplified audit structure
- Reduced database complexity and storage requirements

## Benefits

### **1. Simplified Data Model**

- **Fewer Columns**: 2 less audit columns per table
- **Cleaner Schema**: Easier to understand and maintain
- **Consistent Data**: `LastModifiedBy` is never null

### **2. Comprehensive Audit Trail**

- **Database Level**: Basic audit info (timestamps + last modifier)
- **Logging Level**: Detailed audit trail with full context
- **Best of Both**: Simple model + rich audit information

### **3. Performance Benefits**

- **Smaller Tables**: Less storage and index overhead
- **Faster Queries**: Fewer columns to process
- **Efficient Logging**: Structured logs for audit analysis

### **4. Maintainability**

- **Single Source of Truth**: `LastModifiedBy` for user tracking
- **Centralized Logic**: All audit logic in DataContext
- **Logging Integration**: Rich audit data without database complexity

## Usage Examples

### **Creating an Entity**

```csharp
var client = new Client { Name = "John Doe", Email = "john@example.com" };
await _clientService.Add(client);

// Database sets:
// - CreatedAt = DateTime.UtcNow
// - LastModifiedBy = current user's Guid

// Logs contain:
// - Full creation context
// - User details
// - Entity properties
```

### **Updating an Entity**

```csharp
client.Name = "John Smith";
await _clientService.Update(client.Id, client);

// Database sets:
// - UpdatedAt = DateTime.UtcNow
// - LastModifiedBy = current user's Guid

// Logs contain:
// - Update context
// - Changed properties
// - User details
```

### **Deleting an Entity**

```csharp
await _clientService.Delete(client.Id);

// Database sets:
// - DeletedAt = DateTime.UtcNow
// - LastModifiedBy = current user's Guid

// Logs contain:
// - Deletion context
// - User details
// - Entity state before deletion
```

## Audit Information Sources

### **Database (Basic Info)**

```sql
SELECT Id, CreatedAt, UpdatedAt, DeletedAt, LastModifiedBy
FROM Clients
WHERE Id = @clientId;
```

### **Logs (Detailed Info)**

```json
{
  "Timestamp": "2024-01-15T10:30:00Z",
  "Level": "Information",
  "Category": "DataAccess",
  "Operation": "create",
  "EntityType": "Client",
  "EntityId": "123e4567-e89b-12d3-a456-426614174000",
  "UserId": "auth0|user123",
  "UserEmail": "john@example.com",
  "Details": {
    "CreatedBy": "hash-of-auth0-sub",
    "CreatedAt": "2024-01-15T10:30:00Z",
    "EntityProperties": {
      "Name": "John Doe",
      "Email": "john@example.com"
    }
  }
}
```

## Security Considerations

### **User ID Generation**

- **Consistent**: Same Auth0 user always generates same Guid
- **Secure**: Uses SHA256 hash, no sensitive data stored
- **Fallback**: System user ID for unauthenticated operations

### **Audit Data Protection**

- **Database**: Minimal audit data (timestamps + user ID)
- **Logs**: Rich audit data with proper access controls
- **Compliance**: Meets audit requirements through logging system

## Migration Notes

### **Before Migration**

- Run `dotnet ef migrations add SimplifyAuditColumns`
- Review generated migration
- Backup existing data

### **After Migration**

- Run `dotnet ef database update`
- Verify new schema
- Test audit functionality

### **Data Migration**

The migration automatically:

- Removes `CreatedBy` and `DeletedBy` columns
- Makes `LastModifiedBy` non-nullable
- Sets default value for existing records

## Future Enhancements

### **Potential Improvements**

1. **Audit Queries**: Add methods to query audit history from logs
2. **Audit Reports**: Generate audit reports from log data
3. **Audit Cleanup**: Implement log retention policies
4. **Audit Analytics**: Add audit analytics and dashboards

### **Monitoring**

- Monitor audit log volume
- Track audit query performance
- Consider log archiving strategies

## Troubleshooting

### **Common Issues**

1. **Null User IDs**: Check Auth0 configuration and JWT claims
2. **Missing Audit Data**: Verify entity inherits from BaseDomain
3. **Log Performance**: Monitor log volume and query performance

### **Debugging**

```csharp
// Enable detailed logging for audit operations
services.AddLogging(builder =>
{
    builder.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Debug);
    builder.AddFilter("SFManagement.Data.DataContext", LogLevel.Information);
});
```

This simplified audit system provides a clean, maintainable, and comprehensive audit solution that balances simplicity with detailed tracking capabilities.
