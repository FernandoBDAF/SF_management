# SF Management - Build Warnings Cleanup Plan

## Table of Contents

- [Overview](#overview)
- [Warning Summary](#warning-summary)
- [Phase 1: Remove Unused Fields](#phase-1-remove-unused-fields)
- [Phase 2: Fix Null Reference Warnings in EnumExtensions](#phase-2-fix-null-reference-warnings-in-enumextensions)
- [Phase 3: Fix Non-Nullable Property Warnings](#phase-3-fix-non-nullable-property-warnings)
- [Phase 4: Update Deprecated APIs](#phase-4-update-deprecated-apis)
- [Phase 5: Fix AutoMapper Null Dereference](#phase-5-fix-automapper-null-dereference)
- [Phase 6: Fix Service Layer Null Warnings](#phase-6-fix-service-layer-null-warnings)
- [Phase 7: Fix Controller Warnings](#phase-7-fix-controller-warnings)
- [Phase 8: Fix Miscellaneous Warnings](#phase-8-fix-miscellaneous-warnings)
- [Phase 9: Optional Suppressions](#phase-9-optional-suppressions)
- [Verification](#verification)

---

## Overview

This document provides step-by-step instructions to resolve all 116 remaining build warnings in the SF Management codebase.

**Current State:** 116 warnings (0 errors)  
**Target State:** 0 warnings  
**Estimated Total Time:** 2-3 hours

### Priority Levels

| Priority | Description | Warnings | Time |
|----------|-------------|----------|------|
| 🔴 Critical | Unused code, hiding members | 5 | 10 min |
| 🟡 High | Deprecated APIs | 19 | 45 min |
| 🟢 Medium | Null reference warnings | 85+ | 1-2 hours |
| ⚪ Low | Style/async warnings | 10 | 20 min |

---

## Warning Summary

| Code | Description | Count | Phase |
|------|-------------|-------|-------|
| CS0169 | Field never used | 2 | 1 |
| CS0108 | Member hides inherited member | 2 | 1 |
| CS0168 | Variable declared but never used | 1 | 1 |
| CS8602 | Possible null dereference | 35 | 2, 5, 6 |
| CS8604 | Possible null argument | 6 | 2, 6 |
| CS8603 | Possible null reference return | 8 | 2, 6 |
| CS8618 | Non-nullable property uninitialized | 20 | 3 |
| CS0618 | Obsolete API (FluentValidation) | 2 | 4 |
| CS0618 | Obsolete API (HasCheckConstraint) | 17 | 4 |
| CS8601 | Possible null assignment | 4 | 6, 7 |
| CS1998 | Async method lacks await | 8 | 7 |
| CS8509 | Non-exhaustive switch | 1 | 8 |
| CS8981 | Lowercase type name (migration) | 2 | 9 |
| SYSLIB0051 | Obsolete serialization | 1 | 9 |
| CS9107 | Parameter captured | 1 | 9 |

---

## Phase 1: Remove Unused Fields

**Priority:** 🔴 Critical  
**Time:** 10 minutes  
**Warnings Fixed:** 5 (CS0169, CS0108, CS0168)

### 1.1 BankController.cs - Remove Unused Field

**File:** `Api/Controllers/v1/AssetHolders/BankController.cs`  
**Warnings:** CS0108 (line 20), CS0169 (line 20), CS8618 (line 22)

```csharp
// BEFORE:
public class BankController : BaseAssetHolderController<Bank, BankRequest, BankResponse>
{
    private readonly BankService _bankService;
    private readonly WalletIdentifierService _walletIdentifierService;  // REMOVE THIS

    public BankController(BankService service, IMapper mapper, 
        ILogger<BaseAssetHolderController<Bank, BankRequest, BankResponse>> logger,
        WalletIdentifierService walletIdentifierService)  // Keep parameter for base
        : base(service, walletIdentifierService, mapper, logger)
    {
        _bankService = service;
        // Don't assign _walletIdentifierService - REMOVE any assignment
    }
}

// AFTER:
public class BankController : BaseAssetHolderController<Bank, BankRequest, BankResponse>
{
    private readonly BankService _bankService;

    public BankController(BankService service, IMapper mapper, 
        ILogger<BaseAssetHolderController<Bank, BankRequest, BankResponse>> logger,
        WalletIdentifierService walletIdentifierService)
        : base(service, walletIdentifierService, mapper, logger)
    {
        _bankService = service;
    }
}
```

### 1.2 MemberController.cs - Remove Unused Field

**File:** `Api/Controllers/v1/AssetHolders/MemberController.cs`  
**Warnings:** CS0108 (line 21), CS0169 (line 21), CS8618 (line 24)

```csharp
// BEFORE:
public class MemberController : BaseAssetHolderController<Member, MemberRequest, MemberResponse>
{
    private readonly MemberService _memberService;
    private readonly FiatAssetTransactionService _fiatAssetTransactionService;
    private readonly WalletIdentifierService _walletIdentifierService;  // REMOVE THIS

    public MemberController(
        MemberService service, 
        FiatAssetTransactionService fiatAssetTransactionService, 
        IMapper mapper,
        ILogger<BaseAssetHolderController<Member, MemberRequest, MemberResponse>> logger,
        WalletIdentifierService walletIdentifierService)
        : base(service, walletIdentifierService, mapper, logger)
    {
        _memberService = service;
        _fiatAssetTransactionService = fiatAssetTransactionService;
        // Don't assign _walletIdentifierService - REMOVE any assignment
    }
}

// AFTER:
public class MemberController : BaseAssetHolderController<Member, MemberRequest, MemberResponse>
{
    private readonly MemberService _memberService;
    private readonly FiatAssetTransactionService _fiatAssetTransactionService;

    public MemberController(
        MemberService service, 
        FiatAssetTransactionService fiatAssetTransactionService, 
        IMapper mapper,
        ILogger<BaseAssetHolderController<Member, MemberRequest, MemberResponse>> logger,
        WalletIdentifierService walletIdentifierService)
        : base(service, walletIdentifierService, mapper, logger)
    {
        _memberService = service;
        _fiatAssetTransactionService = fiatAssetTransactionService;
    }
}
```

### 1.3 ImportedTransactionService.cs - Fix Unused Exception Variable

**File:** `Application/Services/Transactions/ImportedTransactionService.cs`  
**Warning:** CS0168 (line 146)

```csharp
// BEFORE:
catch (Exception ex)
{
    // ex is never used
    var failedTransaction = new ImportedTransaction { ... };
}

// AFTER - Option A (discard the exception):
catch (Exception)
{
    var failedTransaction = new ImportedTransaction { ... };
}

// AFTER - Option B (log the exception):
catch (Exception ex)
{
    // Log the exception for debugging
    Console.WriteLine($"Error processing row: {ex.Message}");
    var failedTransaction = new ImportedTransaction { ... };
}
```

---

## Phase 2: Fix Null Reference Warnings in EnumExtensions

**Priority:** 🟢 Medium  
**Time:** 15 minutes  
**Warnings Fixed:** 13 (CS8602, CS8603, CS8604)

**File:** `Application/Common/Extensions/EnumExtensions.cs`

The entire file needs to be rewritten with proper null handling:

```csharp
using System.ComponentModel.DataAnnotations;

namespace SFManagement.Application.Common.Extensions;

public static class EnumHelper
{
    public static int ToOrder<T>(this Enum e) where T : struct, Enum
    {
        var field = typeof(T).GetField(e.ToString());
        if (field == null) return 0;
        
        var attributes = field.GetCustomAttributes(typeof(DisplayAttribute), false) as DisplayAttribute[];
        return attributes?.FirstOrDefault()?.Order ?? 0;
    }

    public static string? ToDescription<T>(this Enum e) where T : struct, Enum
    {
        var field = typeof(T).GetField(e.ToString());
        if (field == null) return null;
        
        var attributes = field.GetCustomAttributes(typeof(DisplayAttribute), false) as DisplayAttribute[];
        return attributes?.FirstOrDefault()?.Description;
    }

    public static string? ToShortName<T>(this Enum e) where T : struct, Enum
    {
        var field = typeof(T).GetField(e.ToString());
        if (field == null) return null;
        
        var attributes = field.GetCustomAttributes(typeof(DisplayAttribute), false) as DisplayAttribute[];
        return attributes?.FirstOrDefault()?.ShortName;
    }

    public static string? ToGroup<T>(this Enum e) where T : struct, Enum
    {
        var field = typeof(T).GetField(e.ToString());
        if (field == null) return null;
        
        var attributes = field.GetCustomAttributes(typeof(DisplayAttribute), false) as DisplayAttribute[];
        return attributes?.FirstOrDefault()?.GroupName;
    }

    public static string ToName<T>(this Enum e) where T : struct, Enum
    {
        var field = typeof(T).GetField(e.ToString());
        if (field == null) return e.ToString();

        var attributes = field.GetCustomAttributes(typeof(DisplayAttribute), false) as DisplayAttribute[];
        return attributes?.FirstOrDefault()?.Name ?? e.ToString();
    }
}
```

---

## Phase 3: Fix Non-Nullable Property Warnings

**Priority:** 🟢 Medium  
**Time:** 30 minutes  
**Warnings Fixed:** 20 (CS8618)

### 3.1 Domain Entities

#### BaseAssetHolder.cs (lines 13, 17)

```csharp
// BEFORE:
[Required] [MaxLength(32)] public string Name { get; set; }
[Required] [MaxLength(20)] [Column(TypeName = "varchar(20)")] public string GovernmentNumber { get; set; }

// AFTER:
[Required] [MaxLength(32)] public string Name { get; set; } = string.Empty;
[Required] [MaxLength(20)] [Column(TypeName = "varchar(20)")] public string GovernmentNumber { get; set; } = string.Empty;
```

#### Bank.cs (line 15)

```csharp
// BEFORE:
[Required] [MaxLength(10)] [Column(TypeName = "varchar(10)")] public string Code { get; set; }

// AFTER:
[Required] [MaxLength(10)] [Column(TypeName = "varchar(10)")] public string Code { get; set; } = string.Empty;
```

#### Category.cs (line 11)

```csharp
// BEFORE:
public string Description { get; set; }

// AFTER:
public string Description { get; set; } = string.Empty;
```

#### BaseTransaction.cs (lines 22, 26)

```csharp
// BEFORE:
public WalletIdentifier SenderWalletIdentifier { get; set; }
public WalletIdentifier ReceiverWalletIdentifier { get; set; }

// AFTER (make nullable since these are navigation properties):
public virtual WalletIdentifier? SenderWalletIdentifier { get; set; }
public virtual WalletIdentifier? ReceiverWalletIdentifier { get; set; }
```

#### ImportedTransaction.cs (lines 46, 59)

```csharp
// BEFORE:
public BaseAssetHolder BaseAssetHolder { get; set; }
public string FileName { get; set; }

// AFTER:
public virtual BaseAssetHolder? BaseAssetHolder { get; set; }
public string FileName { get; set; } = string.Empty;
```

#### Referral.cs (lines 21, 29)

```csharp
// BEFORE:
public BaseAssetHolder AssetHolder { get; set; }
public WalletIdentifier WalletIdentifier { get; set; }

// AFTER:
public virtual BaseAssetHolder? AssetHolder { get; set; }
public virtual WalletIdentifier? WalletIdentifier { get; set; }
```

#### WalletIdentifier.cs (line 17)

```csharp
// BEFORE:
public AssetPool AssetPool { get; set; }

// AFTER:
public virtual AssetPool? AssetPool { get; set; }
```

### 3.2 DTOs

#### BaseAssetHolderRequest.cs (lines 15, 21)

```csharp
// BEFORE:
public string Name { get; set; }
public string GovernmentNumber { get; set; }

// AFTER:
public required string Name { get; set; }
public required string GovernmentNumber { get; set; }
```

#### BaseAssetHolderResponse.cs (line 16)

```csharp
// BEFORE:
public string GovernmentNumber { get; set; }

// AFTER:
public string GovernmentNumber { get; set; } = string.Empty;
```

#### BankRequest.cs (line 12)

```csharp
// BEFORE:
public string Code { get; set; }

// AFTER:
public required string Code { get; set; }
```

#### ImportTransferTransactionRequest.cs (line 6)

```csharp
// BEFORE:
public IFormFile File { get; set; }

// AFTER:
public required IFormFile File { get; set; }
```

#### ImportedTransactionRequest.cs (line 23)

```csharp
// BEFORE:
public IFormFile File { get; set; }

// AFTER:
public required IFormFile File { get; set; }
```

#### ImportBuySellTransactionsRequest.cs (line 6)

```csharp
// BEFORE:
public IFormFile File { get; set; }

// AFTER:
public required IFormFile File { get; set; }
```

#### StatementAssetHolderWithTransactions.cs (line 9)

```csharp
// BEFORE:
public string Name { get; set; }

// AFTER:
public string Name { get; set; } = string.Empty;
```

---

## Phase 4: Update Deprecated APIs

**Priority:** 🟡 High  
**Time:** 45 minutes  
**Warnings Fixed:** 19 (CS0618)

### 4.1 FluentValidation Registration

**File:** `Program.cs` (lines 50-53)

```csharp
// BEFORE (deprecated):
builder.Services.AddFluentValidation(fv =>
{
    fv.RegisterValidatorsFromAssemblyContaining<BankRequestValidator>();
});

// AFTER (new API):
builder.Services.AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<BankRequestValidator>();
```

**Note:** Add using directive if needed:
```csharp
using FluentValidation;
using FluentValidation.AspNetCore;
```

### 4.2 HasCheckConstraint API (17 instances)

**File:** `Infrastructure/Data/DataContext.cs`

#### Helper Method (add to DataContext class)

```csharp
/// <summary>
/// Helper method to add check constraints using the new API
/// </summary>
private static void ConfigureCheckConstraint<TEntity>(
    ModelBuilder modelBuilder, 
    string tableName,
    string constraintName, 
    string sql) where TEntity : class
{
    modelBuilder.Entity<TEntity>()
        .ToTable(tableName, t => t.HasCheckConstraint(constraintName, sql));
}
```

#### Convert Each HasCheckConstraint Call

Pattern for conversion:

```csharp
// BEFORE (deprecated):
modelBuilder.Entity<Bank>()
    .HasCheckConstraint("CK_Bank_SharePercentage", 
        "[SharePercentage] >= 0 AND [SharePercentage] <= 100");

// AFTER (new API):
modelBuilder.Entity<Bank>()
    .ToTable("Banks", t => t.HasCheckConstraint("CK_Bank_SharePercentage", 
        "[SharePercentage] >= 0 AND [SharePercentage] <= 100"));
```

#### Complete List of Constraints to Update

**Lines 171, 176, 181, 186, 191 (Asset Holder constraints):**
```csharp
modelBuilder.Entity<Bank>()
    .ToTable("Banks", t => t.HasCheckConstraint("CK_Bank_SharePercentage", 
        "[SharePercentage] >= 0 AND [SharePercentage] <= 100"));

modelBuilder.Entity<Client>()
    .ToTable("Clients", t => t.HasCheckConstraint("CK_Client_Constraint", "..."));

modelBuilder.Entity<Member>()
    .ToTable("Members", t => t.HasCheckConstraint("CK_Member_Constraint", "..."));

modelBuilder.Entity<PokerManager>()
    .ToTable("PokerManagers", t => t.HasCheckConstraint("CK_PokerManager_Constraint1", "..."));

modelBuilder.Entity<PokerManager>()
    .ToTable("PokerManagers", t => t.HasCheckConstraint("CK_PokerManager_Constraint2", "..."));
```

**Lines 465-524 (Transaction constraints):**
Convert all FiatAssetTransaction and DigitalAssetTransaction constraints using the same pattern.

---

## Phase 5: Fix AutoMapper Null Dereference

**Priority:** 🟢 Medium  
**Time:** 20 minutes  
**Warnings Fixed:** 21 (CS8602)

**File:** `Application/Mappings/AutoMapperProfile.cs` (lines 344-381)

Add null-conditional operators in all mappings that access navigation properties:

```csharp
// BEFORE:
CreateMap<Bank, BankResponse>()
    .ForMember(dest => dest.BaseAssetHolderId, opt => opt.MapFrom(src => src.BaseAssetHolder.Id))
    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.BaseAssetHolder.Name))
    .ForMember(dest => dest.GovernmentNumber, opt => opt.MapFrom(src => src.BaseAssetHolder.GovernmentNumber))
    .ForMember(dest => dest.TaxEntityType, opt => opt.MapFrom(src => src.BaseAssetHolder.TaxEntityType))
    .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.BaseAssetHolder.Addresses.FirstOrDefault()));

// AFTER:
CreateMap<Bank, BankResponse>()
    .ForMember(dest => dest.BaseAssetHolderId, opt => opt.MapFrom(src => 
        src.BaseAssetHolder != null ? src.BaseAssetHolder.Id : Guid.Empty))
    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => 
        src.BaseAssetHolder != null ? src.BaseAssetHolder.Name : string.Empty))
    .ForMember(dest => dest.GovernmentNumber, opt => opt.MapFrom(src => 
        src.BaseAssetHolder != null ? src.BaseAssetHolder.GovernmentNumber : string.Empty))
    .ForMember(dest => dest.TaxEntityType, opt => opt.MapFrom(src => 
        src.BaseAssetHolder != null ? src.BaseAssetHolder.TaxEntityType : default))
    .ForMember(dest => dest.Address, opt => opt.MapFrom(src => 
        src.BaseAssetHolder != null ? src.BaseAssetHolder.Addresses.FirstOrDefault() : null));
```

Apply the same pattern to:
- `CreateMap<Client, ClientResponse>()` (lines 353-359)
- `CreateMap<Member, MemberResponse>()` (lines 363-371)
- `CreateMap<PokerManager, PokerManagerResponse>()` (lines 375-381)

---

## Phase 6: Fix Service Layer Null Warnings

**Priority:** 🟢 Medium  
**Time:** 30 minutes  
**Warnings Fixed:** 15 (CS8602, CS8601, CS8603)

### 6.1 BaseAssetHolderService.cs (lines 91, 92, 813)

```csharp
// Add null checks before accessing navigation properties
// Line 91-92:
if (entity?.SomeProperty != null)
{
    var value = entity.SomeProperty.NestedProperty;
}

// Line 813 - ensure return value is not null:
return result ?? throw new InvalidOperationException("Entity not found");
```

### 6.2 PokerManagerService.cs (lines 66, 87)

```csharp
// Use null-conditional operators:
var value = entity?.Property?.SubProperty ?? defaultValue;
```

### 6.3 ClientReferralService.cs (lines 44, 50, 96, 99, 114, 117)

```csharp
// Add null checks for navigation properties:
if (referral?.AssetHolder != null)
{
    // Process
}

// Or use null-conditional:
var name = referral?.AssetHolder?.Name ?? "Unknown";
```

### 6.4 CategoryService.cs (line 44)

```csharp
// BEFORE:
return result;  // May return null

// AFTER:
public async Task<Category?> Get(Guid id)
{
    return await context.Categories.FirstOrDefaultAsync(c => c.Id == id);
}
// Or throw if null is not expected:
return result ?? throw new KeyNotFoundException($"Category {id} not found");
```

### 6.5 RequestResponseLoggingMiddleware.cs (lines 244, 254, 286, 292)

```csharp
// Return empty string instead of null:
// Lines 244, 254:
return sanitizedValue ?? string.Empty;

// Lines 286, 292:
var value = someNullableValue ?? string.Empty;
```

### 6.6 DependencyInjectionExtensions.cs (line 221)

```csharp
// BEFORE:
.AddSqlServer(configuration.GetConnectionString("DefaultConnection"))

// AFTER:
.AddSqlServer(configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("DefaultConnection string not configured"))
```

---

## Phase 7: Fix Controller Warnings

**Priority:** 🟢 Medium  
**Time:** 20 minutes  
**Warnings Fixed:** 10 (CS1998, CS8601)

### 7.1 Async Methods Without Await (CS1998)

#### BaseAssetHolderController.cs (lines 520, 528)

```csharp
// BEFORE:
protected virtual async Task<IActionResult> SomeMethod()
{
    return Ok(result);  // No await
}

// AFTER - Option A (if async is not needed):
protected virtual IActionResult SomeMethod()
{
    return Ok(result);
}

// AFTER - Option B (if signature must stay async):
protected virtual Task<IActionResult> SomeMethod()
{
    return Task.FromResult<IActionResult>(Ok(result));
}
```

#### ClientController.cs (lines 94, 115)

Apply the same pattern as above.

#### CompanyAssetPoolController.cs (lines 425, 439)

Apply the same pattern as above.

#### AssetHolderDomainService.cs (line 44)

Apply the same pattern as above.

### 7.2 PokerManagerController.cs (line 109)

```csharp
// Add null check or use null-conditional:
var value = someObject?.Property ?? defaultValue;
```

---

## Phase 8: Fix Miscellaneous Warnings

**Priority:** 🟢 Medium  
**Time:** 10 minutes  
**Warnings Fixed:** 1 (CS8509)

### 8.1 Non-exhaustive Switch (CS8509)

**File:** `Application/Services/Validation/WalletIdentifierValidationService.cs` (line 73)

```csharp
// BEFORE:
private static AssetGroup GetExpectedAssetGroup(AssetType assetType)
{
    return assetType switch
    {
        AssetType.BrazilianReal or AssetType.USDollar => AssetGroup.FiatAssets,
        AssetType.PokerStars or AssetType.GgPoker or ... => AssetGroup.PokerAssets,
        AssetType.Bitcoin or AssetType.Ethereum or ... => AssetGroup.CryptoAssets,
        // Missing: AssetType.None
    };
}

// AFTER:
private static AssetGroup GetExpectedAssetGroup(AssetType assetType)
{
    return assetType switch
    {
        AssetType.None => throw new ArgumentException($"AssetType.None is not valid", nameof(assetType)),
        AssetType.BrazilianReal or AssetType.USDollar => AssetGroup.FiatAssets,
        AssetType.PokerStars or AssetType.GgPoker or AssetType.YaPoker or 
            AssetType.AmericasCardRoom or AssetType.SupremaPoker or 
            AssetType.AstroPayICash or AssetType.LuxonPoker => AssetGroup.PokerAssets,
        AssetType.Bitcoin or AssetType.Ethereum or AssetType.Litecoin or 
            AssetType.Ripple or AssetType.BitcoinCash or AssetType.Stellar => AssetGroup.CryptoAssets,
        _ => throw new ArgumentOutOfRangeException(nameof(assetType), $"Unknown AssetType: {assetType}")
    };
}
```

---

## Phase 9: Optional Suppressions

**Priority:** ⚪ Low  
**Time:** 5 minutes  
**Warnings Suppressed:** 4

Some warnings are intentionally suppressed rather than fixed:

### 9.1 Migration File Warning (CS8981)

**File:** `Infrastructure/Data/Migrations/20250717172118_fix share precision.cs`

This is auto-generated code. Add to `.editorconfig`:

```ini
[**/Migrations/*.cs]
dotnet_diagnostic.CS8981.severity = none
```

Or add pragma in the file:
```csharp
#pragma warning disable CS8981
public partial class fixshareprecision : Migration
#pragma warning restore CS8981
```

### 9.2 Obsolete Serialization Constructor (SYSLIB0051)

**File:** `Domain/Exceptions/BusinessException.cs` (line 28)

```csharp
#pragma warning disable SYSLIB0051
protected BusinessException(SerializationInfo info, StreamingContext context)
    : base(info, context) { }
#pragma warning restore SYSLIB0051
```

Or remove the constructor entirely if binary serialization is not used.

### 9.3 Parameter Captured (CS9107)

**File:** `Api/Controllers/v1/Assets/InitialBalanceController.cs` (line 18)

This warning is informational and safe to suppress:

```csharp
#pragma warning disable CS9107
public InitialBalanceController(InitialBalanceService service, IMapper mapper)
    : base(service, mapper)
#pragma warning restore CS9107
{
    _initialBalanceService = service;
}
```

---

## Verification

### Build Verification

```bash
cd "/Users/fernandobarroso/Local Repo/Sempre Fichas/SF_management"

# Clean build
dotnet clean
dotnet restore
dotnet build

# Expected output: Build succeeded with 0 warning(s)
```

### Run Application

```bash
dotnet run

# Verify:
# - Application starts without errors
# - Swagger UI accessible at https://localhost:7078
# - Health check passes at /health
```

---

## Execution Checklist

| Phase | Task | Status |
|-------|------|--------|
| 1 | Remove `_walletIdentifierService` from BankController | ⬜ |
| 1 | Remove `_walletIdentifierService` from MemberController | ⬜ |
| 1 | Fix unused `ex` variable in ImportedTransactionService | ⬜ |
| 2 | Rewrite EnumExtensions with null handling | ⬜ |
| 3 | Fix CS8618 in BaseAssetHolder (Name, GovernmentNumber) | ⬜ |
| 3 | Fix CS8618 in Bank (Code) | ⬜ |
| 3 | Fix CS8618 in Category (Description) | ⬜ |
| 3 | Fix CS8618 in BaseTransaction (navigation props) | ⬜ |
| 3 | Fix CS8618 in ImportedTransaction | ⬜ |
| 3 | Fix CS8618 in Referral | ⬜ |
| 3 | Fix CS8618 in WalletIdentifier | ⬜ |
| 3 | Fix CS8618 in DTOs (Request/Response classes) | ⬜ |
| 4 | Update FluentValidation registration | ⬜ |
| 4 | Update HasCheckConstraint calls (17 instances) | ⬜ |
| 5 | Fix AutoMapper null dereference (4 mappings) | ⬜ |
| 6 | Fix BaseAssetHolderService null warnings | ⬜ |
| 6 | Fix PokerManagerService null warnings | ⬜ |
| 6 | Fix ClientReferralService null warnings | ⬜ |
| 6 | Fix CategoryService null return | ⬜ |
| 6 | Fix RequestResponseLoggingMiddleware null returns | ⬜ |
| 6 | Fix DependencyInjectionExtensions null connection string | ⬜ |
| 7 | Fix async methods without await (8 methods) | ⬜ |
| 7 | Fix PokerManagerController null assignment | ⬜ |
| 8 | Fix non-exhaustive switch in WalletIdentifierValidationService | ⬜ |
| 9 | Suppress CS8981 for migration files | ⬜ |
| 9 | Suppress SYSLIB0051 for serialization constructor | ⬜ |
| 9 | Suppress CS9107 for InitialBalanceController | ⬜ |
| ✓ | Build verification | ⬜ |
| ✓ | Runtime verification | ⬜ |

---

## Summary

| Phase | Description | Warnings Fixed | Time |
|-------|-------------|----------------|------|
| Phase 1 | Remove unused fields | 5 | 10 min |
| Phase 2 | Fix EnumExtensions | 13 | 15 min |
| Phase 3 | Fix non-nullable properties | 20 | 30 min |
| Phase 4 | Update deprecated APIs | 19 | 45 min |
| Phase 5 | Fix AutoMapper null warnings | 21 | 20 min |
| Phase 6 | Fix service layer null warnings | 15 | 30 min |
| Phase 7 | Fix controller warnings | 10 | 20 min |
| Phase 8 | Fix switch warning | 1 | 10 min |
| Phase 9 | Suppressions | 4 | 5 min |
| **Total** | | **108+** | **~3 hours** |

*Note: Some warnings may share root causes, so fixing one may resolve multiple warnings.*

---

*Last Updated: January 2026*
