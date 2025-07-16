# Company Asset Pool Endpoints Documentation

## Overview

This document describes the specialized endpoints for managing company-owned asset pools. These endpoints provide a clean, dedicated API for handling company assets separate from asset holder pools.

## Why Dedicated View Models?

Creating specific view models for company asset pools was essential for several reasons:

### **1. Clear Ownership Semantics**

- **CompanyAssetPoolRequest**: Excludes `BaseAssetHolderId` since it's always null for company pools
- **CompanyAssetPoolResponse**: Always shows "Company" as owner, making ownership explicit
- Prevents confusion between company and asset holder pools

### **2. Enhanced Validation**

- Business-specific validation rules for company pools
- Prevents accidental creation of orphaned pools
- Validates asset type restrictions for company ownership

### **3. Rich Metrics & Analytics**

- Company-specific metrics like transaction counts, balances, and activity
- Summary endpoints for dashboard views
- Performance-optimized for company pool operations

### **4. Type Safety**

- Compile-time guarantees that company pools are handled correctly
- Prevents null reference exceptions in mapping
- Clear API contracts for consumers

## API Endpoints

### **Base URL**

```
/api/v{version}/company/asset-pools
```

### **1. Get All Company Asset Pools**

```http
GET /api/v1/company/asset-pools
```

**Response:**

```json
[
  {
    "id": "guid",
    "assetType": "BrazilianReal",
    "ownerName": "Company",
    "baseAssetHolderId": null,
    "currentBalance": 50000.00,
    "walletIdentifierCount": 5,
    "transactionCount": 150,
    "createdAt": "2024-01-01T00:00:00Z",
    "lastTransactionDate": "2024-01-15T10:30:00Z",
    "description": "Main company BRL pool",
    "businessJustification": "Central liquidity management",
    "walletIdentifiers": [...]
  }
]
```

### **2. Get Specific Company Asset Pool**

```http
GET /api/v1/company/asset-pools/{assetType}
```

**Parameters:**

- `assetType`: The asset type (BrazilianReal, Dollar, Bitcoin, etc.)

**Response:** Same as above, single object

### **3. Create Company Asset Pool**

```http
POST /api/v1/company/asset-pools
```

**Request Body:**

```json
{
  "assetType": "BrazilianReal",
  "description": "Main company BRL pool",
  "initialBalance": 100000.0,
  "businessJustification": "Central liquidity management for operations"
}
```

**Response:** `201 Created` with created pool details

### **4. Get Company Pools Summary**

```http
GET /api/v1/company/asset-pools/summary
```

**Response:**

```json
{
  "totalPools": 3,
  "totalBalance": 250000.0,
  "assetTypeBalances": [
    {
      "assetType": "BrazilianReal",
      "assetTypeName": "BrazilianReal",
      "balance": 150000.0,
      "walletIdentifierCount": 8,
      "transactionCount": 200,
      "lastTransactionDate": "2024-01-15T10:30:00Z"
    }
  ],
  "recentActivity": {
    "transactionsLast30Days": 45,
    "balanceChangeLast30Days": 25000.0,
    "mostActiveAssetType": "BrazilianReal",
    "largestTransactionAmount": 50000.0
  }
}
```

**Features:**

- Cached for 5 minutes for performance
- Comprehensive activity metrics
- Breakdown by asset type

### **5. Delete Company Asset Pool**

```http
DELETE /api/v1/company/asset-pools/{assetType}
```

**Response:** `204 No Content` on success

**Validation:**

- Checks for active wallet identifiers
- Validates no existing transactions
- Business rule compliance

### **6. Get Company Asset Pool Analytics**

```http
GET /api/v1/company/asset-pools/analytics?year=2024&month=1&includeTransactions=true&transactionLimit=50
```

**Query Parameters:**

- `year` (required): Year for analytics (2020-2050)
- `month` (optional): Month for analytics (1-12). If omitted, returns yearly data
- `includeTransactions` (optional): Include transaction details (default: true)
- `transactionLimit` (optional): Max transactions per pool (1-1000, default: 100)

**Response:**

```json
{
  "period": {
    "year": 2024,
    "month": 1,
    "periodName": "January 2024",
    "startDate": "2024-01-01T00:00:00Z",
    "endDate": "2024-01-31T23:59:59Z",
    "totalDays": 31
  },
  "summary": {
    "activePoolsCount": 3,
    "totalEndingBalance": 750000.0,
    "totalStartingBalance": 500000.0,
    "netBalanceChange": 250000.0,
    "totalTransactionCount": 145,
    "totalTransactionVolume": 2500000.0,
    "averageTransactionAmount": 17241.38,
    "largestTransaction": 100000.0,
    "mostActiveAssetType": "BrazilianReal"
  },
  "assetPoolData": [
    {
      "assetPoolId": "guid",
      "assetType": "BrazilianReal",
      "assetTypeName": "BrazilianReal",
      "startingBalance": 300000.0,
      "endingBalance": 450000.0,
      "netBalanceChange": 150000.0,
      "transactionCount": 89,
      "totalTransactionVolume": 1800000.0,
      "averageTransactionAmount": 20224.72,
      "largestTransaction": 100000.0,
      "transactionBreakdown": {
        "fiatTransactions": {
          "count": 65,
          "totalVolume": 1200000.0,
          "averageAmount": 18461.54,
          "largestAmount": 100000.0
        },
        "digitalTransactions": {
          "count": 15,
          "totalVolume": 400000.0,
          "averageAmount": 26666.67,
          "largestAmount": 80000.0
        },
        "settlementTransactions": {
          "count": 9,
          "totalVolume": 200000.0,
          "averageAmount": 22222.22,
          "largestAmount": 50000.0
        }
      },
      "walletIdentifierCount": 8,
      "transactions": [
        {
          "transactionId": "guid",
          "transactionType": "Fiat",
          "date": "2024-01-15T10:30:00Z",
          "amount": 50000.0,
          "direction": "Incoming",
          "counterpartyName": "Client ABC",
          "description": "Monthly payment",
          "category": "Revenue"
        }
      ]
    }
  ]
}
```

**Features:**

- Cached for 10 minutes for performance
- Comprehensive period analysis with starting/ending balances
- Transaction breakdown by type (Fiat, Digital, Settlement)
- Detailed transaction summaries with counterparty information
- Flexible period selection (monthly or yearly)

## View Models

### **CompanyAssetPoolRequest**

```csharp
public class CompanyAssetPoolRequest
{
    [Required]
    public AssetType AssetType { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public decimal? InitialBalance { get; set; }

    [MaxLength(1000)]
    public string? BusinessJustification { get; set; }
}
```

**Key Features:**

- No `BaseAssetHolderId` field (always null for company pools)
- Business justification for audit trail
- Optional initial balance for setup

### **CompanyAssetPoolResponse**

```csharp
public class CompanyAssetPoolResponse : BaseResponse
{
    public AssetType AssetType { get; set; }
    public string OwnerName => "Company"; // Always "Company"
    public Guid? BaseAssetHolderId => null; // Always null
    public decimal CurrentBalance { get; set; }
    public int WalletIdentifierCount { get; set; }
    public int TransactionCount { get; set; }
    public new DateTime CreatedAt { get; set; }
    public DateTime? LastTransactionDate { get; set; }
    public string? Description { get; set; }
    public string? BusinessJustification { get; set; }
    public List<WalletIdentifierResponse> WalletIdentifiers { get; set; }
}
```

**Key Features:**

- Read-only properties that enforce company ownership
- Rich metrics calculated at runtime
- Includes related wallet identifiers

### **CompanyAssetPoolSummaryResponse**

```csharp
public class CompanyAssetPoolSummaryResponse
{
    public int TotalPools { get; set; }
    public decimal TotalBalance { get; set; }
    public List<CompanyAssetTypeBalance> AssetTypeBalances { get; set; }
    public CompanyPoolActivity RecentActivity { get; set; }
}
```

**Key Features:**

- Dashboard-ready summary data
- Asset type breakdown
- Recent activity metrics (30-day window)

## Validation & Business Rules

### **AssetPoolValidationService**

The dedicated validation service provides:

#### **Company Pool Validation**

- Prevents duplicate company pools for same asset type
- Validates business rules for company asset ownership
- Checks asset type restrictions

#### **Deletion Validation**

- Ensures no active wallet identifiers
- Validates no existing transactions
- Business rule compliance checks

#### **Configurable Business Rules**

```csharp
// Example: Restrict certain asset types from company ownership
var restrictedAssetTypes = new AssetType[]
{
    // AssetType.PersonalCrypto, // Example restriction
};
```

### **Error Handling**

#### **Validation Errors (400 Bad Request)**

```json
{
  "title": "Validation Failed",
  "status": 400,
  "errors": {
    "AssetType": ["Asset type BrazilianReal cannot be owned by the company"]
  },
  "requestId": "0HNDVHGQTNTTP:00000001"
}
```

#### **Business Rule Violations (409 Conflict)**

```json
{
  "title": "Business Rule Violation",
  "detail": "Company already has an AssetPool for BrazilianReal",
  "status": 409,
  "requestId": "0HNDVHGQTNTTP:00000001",
  "assetType": "BrazilianReal"
}
```

## Service Architecture

### **Enhanced AssetPoolService**

New methods added for company pool management:

```csharp
// Company pool creation with validation
Task<AssetPool> CreateCompanyAssetPool(AssetType assetType, string? description, string? businessJustification)

// Rich metrics for company pools
Task<AssetPool?> GetCompanyAssetPoolWithMetrics(AssetType assetType)

// Comprehensive summary with activity data
Task<CompanyAssetPoolSummaryResponse> GetCompanyAssetPoolSummary()
```

### **Dependency Injection**

```csharp
// Added to DI container
builder.Services.AddScoped<AssetPoolValidationService>();
```

### **AutoMapper Configuration**

```csharp
// Company-specific mappings
CreateMap<CompanyAssetPoolRequest, AssetPool>()
    .ForMember(dest => dest.BaseAssetHolderId, opt => opt.MapFrom(src => (Guid?)null));

CreateMap<AssetPool, CompanyAssetPoolResponse>()
    .ForMember(dest => dest.CurrentBalance, opt => opt.Ignore()) // Set manually
    .ForMember(dest => dest.TransactionCount, opt => opt.Ignore()); // Set manually
```

## Performance Considerations

### **Caching**

- Summary endpoint cached for 5 minutes
- Balance calculations optimized with strategic includes
- Transaction count queries use efficient aggregations

### **Database Optimization**

- Indexes on `BaseAssetHolderId` and `AssetType` for fast company pool queries
- Optimized includes for related entities
- Efficient pagination for large datasets

## Security & Authorization

### **Access Control**

- Company pool endpoints require appropriate permissions
- Audit logging for all company pool operations
- Request tracing for debugging and monitoring

### **Validation**

- Comprehensive input validation
- Business rule enforcement
- SQL injection prevention through parameterized queries

## Usage Examples

### **Create Company BRL Pool**

```bash
curl -X POST /api/v1/company/asset-pools \
  -H "Content-Type: application/json" \
  -d '{
    "assetType": "BrazilianReal",
    "description": "Main company BRL liquidity pool",
    "initialBalance": 500000.00,
    "businessJustification": "Central treasury management for operational expenses"
  }'
```

### **Get Company Pool Summary**

```bash
curl -X GET /api/v1/company/asset-pools/summary \
  -H "Accept: application/json"
```

### **Check Specific Asset Type**

```bash
curl -X GET /api/v1/company/asset-pools/BrazilianReal \
  -H "Accept: application/json"
```

## Integration Points

### **Transaction System**

- Company pools integrate seamlessly with existing transaction infrastructure
- WalletIdentifiers can belong to company pools
- Balance calculations include company pool transactions

### **Reporting & Analytics**

- Summary data ready for dashboard integration
- Metrics suitable for financial reporting
- Activity tracking for compliance and auditing

### **Asset Management**

- Clear separation between company and asset holder assets
- Centralized company asset visibility
- Simplified company liquidity management

## Benefits

### **For Developers**

- Type-safe company pool operations
- Clear separation of concerns
- Comprehensive validation and error handling
- Rich documentation and examples

### **For Business Users**

- Clear company asset visibility
- Centralized company pool management
- Rich metrics and activity tracking
- Audit trail for compliance

### **For System Administration**

- Robust validation prevents data integrity issues
- Comprehensive logging for debugging
- Performance-optimized for scale
- Secure and authorized access

## Enhanced AssetPool Request Validation

### **Preventing Accidental Company Pools**

The regular `AssetPoolRequest` has been enhanced to prevent accidental creation of company pools:

```csharp
public class AssetPoolRequest
{
    [Required(ErrorMessage = "BaseAssetHolderId is required. For company pools, use the CompanyAssetPoolController.")]
    public Guid BaseAssetHolderId { get; set; }

    [Required]
    public AssetType AssetType { get; set; }
}
```

**Enhanced AutoMapper Validation:**

```csharp
CreateMap<AssetPoolRequest, AssetPool>()
    .AfterMap((src, dest, context) =>
    {
        if (dest.BaseAssetHolderId == Guid.Empty)
        {
            throw new ArgumentException("BaseAssetHolderId cannot be empty. For company pools, use the CompanyAssetPoolController.");
        }
        if (!dest.BaseAssetHolderId.HasValue)
        {
            throw new ArgumentException("BaseAssetHolderId is required. For company pools, use the CompanyAssetPoolController.");
        }
    });
```

**Benefits:**

- **Clear Separation**: Forces developers to use the correct controller for each pool type
- **Validation**: Prevents `Guid.Empty` and null `BaseAssetHolderId` values
- **Error Messages**: Provides clear guidance on which controller to use
- **Type Safety**: Compile-time and runtime validation
