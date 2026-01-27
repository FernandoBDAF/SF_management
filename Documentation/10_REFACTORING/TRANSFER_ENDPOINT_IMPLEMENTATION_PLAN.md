# Transfer Endpoint Implementation Plan

> **Version:** 1.1  
> **Last Updated:** January 22, 2026  
> **Status:** ✅ Implemented (with subsequent changes)  
> **Based On:** SF_management-front/documentation/03_CORE_SYSTEMS/TRANSACTION_SYSTEM_ANALYSIS.md

---

> ⚠️ **HISTORICAL DOCUMENT:** This plan was implemented in January 2026. Some details are now outdated:
> 
> **Changes since implementation:**
> - `CreateWalletsIfMissing` flag is **deprecated** - automatic wallet creation removed
> - TRANSFER mode now restricted to **Internal wallets only** (AssetGroup 4)
> - Wallet creation is now explicit via dedicated API before transfer
> 
> For current behavior, see:
> - [TRANSACTION_INFRASTRUCTURE.md](../03_CORE_SYSTEMS/TRANSACTION_INFRASTRUCTURE.md)
> - [TRANSACTION_API_ENDPOINTS.md](../06_API/TRANSACTION_API_ENDPOINTS.md)

---

## Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Implementation Phases](#implementation-phases)
4. [Phase 1: DTOs](#phase-1-dtos)
5. [Phase 2: TransferService](#phase-2-transferservice)
6. [Phase 3: TransferController](#phase-3-transfercontroller)
7. [Phase 4: FluentValidation](#phase-4-fluentvalidation)
8. [Phase 5: DI Registration](#phase-5-di-registration)
9. [Phase 6: Testing](#phase-6-testing)
10. [Deprecation Plan](#deprecation-plan)
11. [Technical Notes](#technical-notes)
12. [File Summary](#file-summary)
13. [Execution Checklist](#execution-checklist)

---

## Overview

### Purpose

Create a unified `POST /api/v1/transfer` endpoint that handles all asset transfers between any two asset holders, replacing the scattered `send-brazilian-real` endpoints.

### Goals

1. Single, flexible transfer endpoint for all asset types
2. Support both Fiat (BRL, USD) and Digital (Poker, Crypto) asset transfers
3. Auto-creation of wallets when needed
4. Optional specific wallet selection for advanced use cases
5. Consistent with existing codebase patterns

### Prerequisites

- Understanding of the wallet/asset pool hierarchy: `BaseAssetHolder → AssetPool → WalletIdentifier`
- Familiarity with the existing transaction entities: `FiatAssetTransaction`, `DigitalAssetTransaction`
- Review of `TRANSACTION_SYSTEM_ANALYSIS.md`

---

## Architecture

### Domain Model

```
BaseAssetHolder (Client, Member, Bank, PokerManager)
    └── AssetPool (grouped by AssetGroup: Fiat, Poker, Crypto)
            └── WalletIdentifier (specific asset type: BRL, PokerStars, Bitcoin)
                    └── Transactions (FiatAssetTransaction or DigitalAssetTransaction)
```

### API Usage Modes

**Simple Mode** - System auto-selects/creates wallets:
```json
{
    "senderAssetHolderId": "guid",
    "receiverAssetHolderId": "guid",
    "assetType": 21,
    "amount": 1000.00,
    "date": "2026-01-22T10:00:00Z",
    "createWalletsIfMissing": true
}
```

**Advanced Mode** - User specifies wallets:
```json
{
    "senderAssetHolderId": "guid",
    "receiverAssetHolderId": "guid",
    "senderWalletIdentifierId": "wallet-guid",
    "receiverWalletIdentifierId": "wallet-guid",
    "assetType": 21,
    "amount": 1000.00,
    "date": "2026-01-22T10:00:00Z"
}
```

### Transaction Type Determination

| Asset Type | Asset Group | Transaction Entity |
|------------|-------------|-------------------|
| BrazilianReal, USDollar | FiatAssets | `FiatAssetTransaction` |
| PokerStars, GgPoker, etc. | PokerAssets | `DigitalAssetTransaction` |
| Bitcoin, Ethereum, etc. | CryptoAssets | `DigitalAssetTransaction` |

---

## Implementation Phases

| Phase | Description | Files | Time |
|-------|-------------|-------|------|
| **1** | DTOs | 3 files | 30 min |
| **2** | TransferService | 1 file | 1.5 hours |
| **3** | TransferController | 1 file | 30 min |
| **4** | FluentValidation | 1 file | 20 min |
| **5** | DI Registration | 1 file (modify) | 10 min |
| **6** | Testing | - | 2 hours |
| **Total** | | **7 files** | **~5 hours** |

---

## Phase 1: DTOs

### 1.1 TransferRequest.cs

**File:** `Application/DTOs/Transactions/TransferRequest.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using SFManagement.Domain.Enums.Assets;

namespace SFManagement.Application.DTOs.Transactions;

/// <summary>
/// Request DTO for creating a transfer between any two asset holders.
/// Supports both Fiat and Digital asset transfers.
/// </summary>
public class TransferRequest
{
    // === Required: Participants ===
    
    /// <summary>
    /// The asset holder sending the assets.
    /// </summary>
    [Required(ErrorMessage = "SenderAssetHolderId is required")]
    public Guid SenderAssetHolderId { get; set; }
    
    /// <summary>
    /// The asset holder receiving the assets.
    /// </summary>
    [Required(ErrorMessage = "ReceiverAssetHolderId is required")]
    public Guid ReceiverAssetHolderId { get; set; }
    
    // === Optional: Specific Wallet Selection ===
    
    /// <summary>
    /// Optional: Specific sender wallet to use.
    /// If provided, must belong to SenderAssetHolderId and match AssetType.
    /// </summary>
    public Guid? SenderWalletIdentifierId { get; set; }
    
    /// <summary>
    /// Optional: Specific receiver wallet to use.
    /// If provided, must belong to ReceiverAssetHolderId and match AssetType.
    /// </summary>
    public Guid? ReceiverWalletIdentifierId { get; set; }
    
    // === Required: Asset Specification ===
    
    /// <summary>
    /// The type of asset being transferred.
    /// Determines whether to create Fiat or Digital transaction.
    /// </summary>
    [Required(ErrorMessage = "AssetType is required")]
    public AssetType AssetType { get; set; }
    
    // === Required: Transaction Details ===
    
    /// <summary>
    /// The amount to transfer. Must be greater than 0.
    /// </summary>
    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
    
    /// <summary>
    /// The date of the transaction.
    /// </summary>
    [Required(ErrorMessage = "Date is required")]
    public DateTime Date { get; set; }
    
    // === Optional: Transaction Details ===
    
    /// <summary>
    /// Optional category for the transaction.
    /// </summary>
    public Guid? CategoryId { get; set; }
    
    /// <summary>
    /// Optional description.
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }
    
    // === Optional: Digital Asset Specific ===
    
    /// <summary>
    /// For digital transactions: record balance as this asset type.
    /// </summary>
    public AssetType? BalanceAs { get; set; }
    
    /// <summary>
    /// For digital transactions: conversion rate.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "ConversionRate must be non-negative")]
    public decimal? ConversionRate { get; set; }
    
    /// <summary>
    /// For digital transactions: rate/fee percentage.
    /// </summary>
    [Range(0, 100, ErrorMessage = "Rate must be between 0 and 100")]
    public decimal? Rate { get; set; }
    
    // === Options ===
    
    /// <summary>
    /// If true, creates wallets if they don't exist.
    /// Ignored when specific wallet IDs are provided.
    /// Default: true
    /// </summary>
    public bool CreateWalletsIfMissing { get; set; } = true;
    
    /// <summary>
    /// If true, auto-approves the transaction.
    /// Default: false
    /// </summary>
    public bool AutoApprove { get; set; } = false;
    
    /// <summary>
    /// If true, validates sender has sufficient balance.
    /// Default: false
    /// </summary>
    public bool ValidateBalance { get; set; } = false;
}
```

### 1.2 TransferResponse.cs

**File:** `Application/DTOs/Transactions/TransferResponse.cs`

```csharp
using SFManagement.Domain.Enums.Assets;

namespace SFManagement.Application.DTOs.Transactions;

/// <summary>
/// Response DTO for a completed transfer transaction.
/// </summary>
public class TransferResponse
{
    // Transaction identification
    public Guid TransactionId { get; set; }
    public string EntityType { get; set; } = string.Empty; // "fiat" or "digital"
    public AssetType AssetType { get; set; }
    
    // Sender details
    public Guid SenderWalletIdentifierId { get; set; }
    public Guid SenderAssetHolderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    
    // Receiver details
    public Guid ReceiverWalletIdentifierId { get; set; }
    public Guid ReceiverAssetHolderId { get; set; }
    public string ReceiverName { get; set; } = string.Empty;
    
    // Transaction details
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public bool IsInternalTransfer { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Wallet creation indicators
    public bool SenderWalletCreated { get; set; }
    public bool ReceiverWalletCreated { get; set; }
    public bool WalletsCreated => SenderWalletCreated || ReceiverWalletCreated;
}
```

### 1.3 TransferError.cs

**File:** `Application/DTOs/Transactions/TransferError.cs`

```csharp
namespace SFManagement.Application.DTOs.Transactions;

/// <summary>
/// Error details for transfer operations.
/// </summary>
public class TransferError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Field { get; set; }
    public Dictionary<string, object>? Details { get; set; }
}

/// <summary>
/// Error codes for transfer operations.
/// </summary>
public static class TransferErrorCodes
{
    public const string SenderNotFound = "SENDER_NOT_FOUND";
    public const string ReceiverNotFound = "RECEIVER_NOT_FOUND";
    public const string SenderWalletNotFound = "SENDER_WALLET_NOT_FOUND";
    public const string ReceiverWalletNotFound = "RECEIVER_WALLET_NOT_FOUND";
    public const string WalletCreationFailed = "WALLET_CREATION_FAILED";
    public const string InsufficientBalance = "INSUFFICIENT_BALANCE";
    public const string InvalidAssetType = "INVALID_ASSET_TYPE";
    public const string AssetTypeMismatch = "ASSET_TYPE_MISMATCH";
    public const string WalletOwnershipMismatch = "WALLET_OWNERSHIP_MISMATCH";
    public const string InvalidAmount = "INVALID_AMOUNT";
    public const string CategoryNotFound = "CATEGORY_NOT_FOUND";
    public const string TransactionFailed = "TRANSACTION_FAILED";
    public const string SameSenderReceiverWallet = "SAME_SENDER_RECEIVER_WALLET";
}
```

---

## Phase 2: TransferService

**File:** `Application/Services/Transactions/TransferService.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using SFManagement.Application.DTOs.Transactions;
using SFManagement.Application.Services.Assets;
using SFManagement.Application.Services.Validation;
using SFManagement.Domain.Entities.Assets;
using SFManagement.Domain.Entities.Transactions;
using SFManagement.Domain.Enums.Assets;
using SFManagement.Domain.Exceptions;
using SFManagement.Infrastructure.Data;

namespace SFManagement.Application.Services.Transactions;

/// <summary>
/// Service for unified transfer operations between asset holders.
/// </summary>
public class TransferService
{
    private readonly DataContext _context;
    private readonly WalletIdentifierService _walletIdentifierService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TransferService(
        DataContext context,
        WalletIdentifierService walletIdentifierService,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _walletIdentifierService = walletIdentifierService;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Creates a transfer between two asset holders.
    /// </summary>
    public async Task<TransferResponse> TransferAsync(TransferRequest request)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // 1. Validate asset type
            if (request.AssetType == AssetType.None)
            {
                throw new BusinessException(TransferErrorCodes.InvalidAssetType, 
                    "AssetType.None is not valid for transfers");
            }

            // 2. Determine transaction type
            var assetGroup = WalletIdentifierValidationService.GetAssetGroupForAssetType(request.AssetType);
            var isFiat = assetGroup == AssetGroup.FiatAssets;
            var entityType = isFiat ? "fiat" : "digital";

            // 3. Validate sender exists
            var senderAssetHolder = await _context.BaseAssetHolders
                .FirstOrDefaultAsync(ah => ah.Id == request.SenderAssetHolderId && !ah.DeletedAt.HasValue)
                ?? throw new BusinessException(TransferErrorCodes.SenderNotFound,
                    $"Sender asset holder '{request.SenderAssetHolderId}' not found");

            // 4. Validate receiver exists
            var receiverAssetHolder = await _context.BaseAssetHolders
                .FirstOrDefaultAsync(ah => ah.Id == request.ReceiverAssetHolderId && !ah.DeletedAt.HasValue)
                ?? throw new BusinessException(TransferErrorCodes.ReceiverNotFound,
                    $"Receiver asset holder '{request.ReceiverAssetHolderId}' not found");

            // 5. Validate category if provided
            if (request.CategoryId.HasValue)
            {
                var categoryExists = await _context.Categories
                    .AnyAsync(c => c.Id == request.CategoryId.Value && !c.DeletedAt.HasValue);
                
                if (!categoryExists)
                    throw new BusinessException(TransferErrorCodes.CategoryNotFound,
                        $"Category '{request.CategoryId}' not found");
            }

            // 6. Get sender wallet
            WalletIdentifier senderWallet;
            bool senderWalletCreated = false;

            if (request.SenderWalletIdentifierId.HasValue)
            {
                senderWallet = await GetAndValidateWalletAsync(
                    request.SenderWalletIdentifierId.Value,
                    request.SenderAssetHolderId,
                    request.AssetType,
                    "Sender");
            }
            else
            {
                (senderWallet, senderWalletCreated) = await FindOrCreateWalletAsync(
                    request.SenderAssetHolderId,
                    request.AssetType,
                    request.CreateWalletsIfMissing);
                
                if (senderWallet == null)
                    throw new BusinessException(TransferErrorCodes.SenderWalletNotFound,
                        $"Sender wallet for '{request.AssetType}' not found");
            }

            // 7. Get receiver wallet
            WalletIdentifier receiverWallet;
            bool receiverWalletCreated = false;

            if (request.ReceiverWalletIdentifierId.HasValue)
            {
                receiverWallet = await GetAndValidateWalletAsync(
                    request.ReceiverWalletIdentifierId.Value,
                    request.ReceiverAssetHolderId,
                    request.AssetType,
                    "Receiver");
            }
            else
            {
                (receiverWallet, receiverWalletCreated) = await FindOrCreateWalletAsync(
                    request.ReceiverAssetHolderId,
                    request.AssetType,
                    request.CreateWalletsIfMissing);
                
                if (receiverWallet == null)
                    throw new BusinessException(TransferErrorCodes.ReceiverWalletNotFound,
                        $"Receiver wallet for '{request.AssetType}' not found");
            }

            // 8. Validate not same wallet
            if (senderWallet.Id == receiverWallet.Id)
                throw new BusinessException(TransferErrorCodes.SameSenderReceiverWallet,
                    "Cannot transfer to the same wallet");

            // 9. Validate balance if requested
            if (request.ValidateBalance)
            {
                var balance = await GetBalanceForWalletAsync(senderWallet.Id, isFiat);
                if (balance < request.Amount)
                    throw new BusinessException(TransferErrorCodes.InsufficientBalance,
                        $"Insufficient balance. Available: {balance:N2}, Requested: {request.Amount:N2}");
            }

            // 10. Create transaction
            Guid transactionId;
            DateTime createdAt = DateTime.UtcNow;
            DateTime? approvedAt = request.AutoApprove ? DateTime.UtcNow : null;

            if (isFiat)
            {
                var fiatTransaction = new FiatAssetTransaction
                {
                    SenderWalletIdentifierId = senderWallet.Id,
                    ReceiverWalletIdentifierId = receiverWallet.Id,
                    AssetAmount = request.Amount,
                    Date = request.Date,
                    CategoryId = request.CategoryId,
                    ApprovedAt = approvedAt,
                    CreatedAt = createdAt
                };
                await _context.FiatAssetTransactions.AddAsync(fiatTransaction);
                transactionId = fiatTransaction.Id;
            }
            else
            {
                var digitalTransaction = new DigitalAssetTransaction
                {
                    SenderWalletIdentifierId = senderWallet.Id,
                    ReceiverWalletIdentifierId = receiverWallet.Id,
                    AssetAmount = request.Amount,
                    Date = request.Date,
                    CategoryId = request.CategoryId,
                    BalanceAs = request.BalanceAs,
                    ConversionRate = request.ConversionRate,
                    Rate = request.Rate,
                    ApprovedAt = approvedAt,
                    CreatedAt = createdAt
                };
                await _context.DigitalAssetTransactions.AddAsync(digitalTransaction);
                transactionId = digitalTransaction.Id;
            }

            // 11. Save and commit
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // 12. Return response
            return new TransferResponse
            {
                TransactionId = transactionId,
                EntityType = entityType,
                AssetType = request.AssetType,
                SenderWalletIdentifierId = senderWallet.Id,
                SenderAssetHolderId = request.SenderAssetHolderId,
                SenderName = senderAssetHolder.Name,
                ReceiverWalletIdentifierId = receiverWallet.Id,
                ReceiverAssetHolderId = request.ReceiverAssetHolderId,
                ReceiverName = receiverAssetHolder.Name,
                Amount = request.Amount,
                Date = request.Date,
                IsInternalTransfer = request.SenderAssetHolderId == request.ReceiverAssetHolderId,
                IsApproved = request.AutoApprove,
                CreatedAt = createdAt,
                SenderWalletCreated = senderWalletCreated,
                ReceiverWalletCreated = receiverWalletCreated
            };
        }
        catch (BusinessException)
        {
            await transaction.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new BusinessException(TransferErrorCodes.TransactionFailed,
                $"Transfer failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Validates a provided wallet belongs to expected asset holder and matches asset type.
    /// </summary>
    private async Task<WalletIdentifier> GetAndValidateWalletAsync(
        Guid walletIdentifierId,
        Guid expectedAssetHolderId,
        AssetType expectedAssetType,
        string participantName)
    {
        var wallet = await _context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
            .FirstOrDefaultAsync(wi => wi.Id == walletIdentifierId && !wi.DeletedAt.HasValue);
        
        if (wallet == null)
            throw new BusinessException(
                participantName == "Sender" ? TransferErrorCodes.SenderWalletNotFound : TransferErrorCodes.ReceiverWalletNotFound,
                $"{participantName} wallet '{walletIdentifierId}' not found");
        
        if (wallet.AssetPool?.BaseAssetHolderId != expectedAssetHolderId)
            throw new BusinessException(TransferErrorCodes.WalletOwnershipMismatch,
                $"{participantName} wallet does not belong to the specified asset holder");
        
        if (wallet.AssetType != expectedAssetType)
            throw new BusinessException(TransferErrorCodes.AssetTypeMismatch,
                $"{participantName} wallet asset type ({wallet.AssetType}) doesn't match request ({expectedAssetType})");
        
        return wallet;
    }

    /// <summary>
    /// Finds existing wallet or creates new one if allowed.
    /// Uses WalletIdentifierService which handles AssetPool creation automatically.
    /// </summary>
    private async Task<(WalletIdentifier? Wallet, bool Created)> FindOrCreateWalletAsync(
        Guid assetHolderId,
        AssetType assetType,
        bool createIfMissing)
    {
        // Find existing
        var existingWallets = await _walletIdentifierService
            .GetByAssetHolderAndAssetType(assetHolderId, assetType);
        
        if (existingWallets.FirstOrDefault() is { } existing)
            return (existing, false);
        
        if (!createIfMissing)
            return (null, false);
        
        // Create new wallet (service handles AssetPool creation via [NotMapped] BaseAssetHolderId)
        try
        {
            var newWallet = new WalletIdentifier
            {
                BaseAssetHolderId = assetHolderId,
                AssetType = assetType,
                AccountClassification = await DetermineAccountClassificationAsync(assetHolderId)
            };
            
            SetDefaultMetadata(newWallet, assetType);
            
            var created = await _walletIdentifierService.Add(newWallet);
            return (created, true);
        }
        catch (Exception ex)
        {
            throw new BusinessException(TransferErrorCodes.WalletCreationFailed,
                $"Failed to create wallet: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Determines account classification: Banks/PokerManagers = ASSET, Clients/Members = LIABILITY.
    /// </summary>
    private async Task<AccountClassification> DetermineAccountClassificationAsync(Guid assetHolderId)
    {
        var isBank = await _context.Banks.AnyAsync(b => b.BaseAssetHolderId == assetHolderId);
        var isPokerManager = await _context.PokerManagers.AnyAsync(pm => pm.BaseAssetHolderId == assetHolderId);
        
        return (isBank || isPokerManager) ? AccountClassification.ASSET : AccountClassification.LIABILITY;
    }

    /// <summary>
    /// Sets default metadata for new wallet based on asset type.
    /// </summary>
    private static void SetDefaultMetadata(WalletIdentifier wallet, AssetType assetType)
    {
        var assetGroup = WalletIdentifierValidationService.GetAssetGroupForAssetType(assetType);
        
        switch (assetGroup)
        {
            case AssetGroup.FiatAssets:
                wallet.SetMetadataFromFields(bankName: "Auto-created", pixKey: "pending");
                break;
            case AssetGroup.PokerAssets:
                wallet.SetMetadataFromFields(inputForTransactions: "auto-created");
                break;
            case AssetGroup.CryptoAssets:
                wallet.SetMetadataFromFields(walletAddress: "pending", walletCategory: "auto-created");
                break;
        }
    }

    /// <summary>
    /// Gets current balance for a wallet.
    /// </summary>
    private async Task<decimal> GetBalanceForWalletAsync(Guid walletIdentifierId, bool isFiat)
    {
        decimal incoming, outgoing;
        
        if (isFiat)
        {
            incoming = await _context.FiatAssetTransactions
                .Where(t => !t.DeletedAt.HasValue && t.ReceiverWalletIdentifierId == walletIdentifierId)
                .SumAsync(t => t.AssetAmount);
            outgoing = await _context.FiatAssetTransactions
                .Where(t => !t.DeletedAt.HasValue && t.SenderWalletIdentifierId == walletIdentifierId)
                .SumAsync(t => t.AssetAmount);
        }
        else
        {
            incoming = await _context.DigitalAssetTransactions
                .Where(t => !t.DeletedAt.HasValue && t.ReceiverWalletIdentifierId == walletIdentifierId)
                .SumAsync(t => t.AssetAmount);
            outgoing = await _context.DigitalAssetTransactions
                .Where(t => !t.DeletedAt.HasValue && t.SenderWalletIdentifierId == walletIdentifierId)
                .SumAsync(t => t.AssetAmount);
        }
        
        var initialBalance = await _context.InitialBalances
            .Where(ib => ib.WalletIdentifierId == walletIdentifierId && !ib.DeletedAt.HasValue)
            .SumAsync(ib => (decimal?)ib.Amount) ?? 0;
        
        return initialBalance + incoming - outgoing;
    }

    /// <summary>
    /// Gets a transfer by ID.
    /// </summary>
    public async Task<TransferResponse?> GetTransferAsync(Guid transactionId, bool isFiat)
    {
        BaseTransaction? txn = isFiat
            ? await _context.FiatAssetTransactions
                .Include(t => t.SenderWalletIdentifier).ThenInclude(w => w!.AssetPool).ThenInclude(ap => ap!.BaseAssetHolder)
                .Include(t => t.ReceiverWalletIdentifier).ThenInclude(w => w!.AssetPool).ThenInclude(ap => ap!.BaseAssetHolder)
                .FirstOrDefaultAsync(t => t.Id == transactionId && !t.DeletedAt.HasValue)
            : await _context.DigitalAssetTransactions
                .Include(t => t.SenderWalletIdentifier).ThenInclude(w => w!.AssetPool).ThenInclude(ap => ap!.BaseAssetHolder)
                .Include(t => t.ReceiverWalletIdentifier).ThenInclude(w => w!.AssetPool).ThenInclude(ap => ap!.BaseAssetHolder)
                .FirstOrDefaultAsync(t => t.Id == transactionId && !t.DeletedAt.HasValue);

        if (txn == null) return null;

        return new TransferResponse
        {
            TransactionId = txn.Id,
            EntityType = isFiat ? "fiat" : "digital",
            AssetType = txn.SenderWalletIdentifier?.AssetType ?? AssetType.None,
            SenderWalletIdentifierId = txn.SenderWalletIdentifierId,
            SenderAssetHolderId = txn.SenderWalletIdentifier?.AssetPool?.BaseAssetHolderId ?? Guid.Empty,
            SenderName = txn.SenderWalletIdentifier?.AssetPool?.BaseAssetHolder?.Name ?? "Unknown",
            ReceiverWalletIdentifierId = txn.ReceiverWalletIdentifierId,
            ReceiverAssetHolderId = txn.ReceiverWalletIdentifier?.AssetPool?.BaseAssetHolderId ?? Guid.Empty,
            ReceiverName = txn.ReceiverWalletIdentifier?.AssetPool?.BaseAssetHolder?.Name ?? "Unknown",
            Amount = txn.AssetAmount,
            Date = txn.Date,
            IsInternalTransfer = txn.IsInternalTransfer,
            IsApproved = txn.ApprovedAt.HasValue,
            CreatedAt = txn.CreatedAt
        };
    }
}
```

---

## Phase 3: TransferController

**File:** `Api/Controllers/v1/Transactions/TransferController.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Application.DTOs.Transactions;
using SFManagement.Application.Services.Transactions;
using SFManagement.Domain.Exceptions;

namespace SFManagement.Api.Controllers.v1.Transactions;

/// <summary>
/// Unified transfer operations between asset holders.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class TransferController : ControllerBase
{
    private readonly TransferService _transferService;
    private readonly ILogger<TransferController> _logger;

    public TransferController(TransferService transferService, ILogger<TransferController> logger)
    {
        _transferService = transferService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a transfer between any two asset holders.
    /// </summary>
    /// <remarks>
    /// Supports both simple mode (auto-select wallets) and advanced mode (specify wallet IDs).
    /// 
    /// **Transaction Types:**
    /// - Fiat assets (BrazilianReal=21, USDollar=22) → FiatAssetTransaction
    /// - Digital assets (PokerStars=101, Bitcoin=201, etc.) → DigitalAssetTransaction
    /// </remarks>
    [HttpPost]
    [Authorize(Policy = "Permission:create:transactions")]
    [ProducesResponseType(typeof(TransferResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
    {
        try
        {
            _logger.LogInformation(
                "Transfer: {Sender} -> {Receiver}, AssetType={AssetType}, Amount={Amount}",
                request.SenderAssetHolderId, request.ReceiverAssetHolderId,
                request.AssetType, request.Amount);

            var response = await _transferService.TransferAsync(request);

            _logger.LogInformation(
                "Transfer completed: TxnId={TxnId}, Type={Type}, Internal={IsInternal}",
                response.TransactionId, response.EntityType, response.IsInternalTransfer);

            return Ok(response);
        }
        catch (BusinessException ex)
        {
            _logger.LogWarning(ex, "Transfer failed: {Code} - {Message}", ex.ErrorCode, ex.Message);

            return BadRequest(new ProblemDetails
            {
                Title = "Transfer Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Extensions =
                {
                    ["errorCode"] = ex.ErrorCode,
                    ["errors"] = new[] { new TransferError { Code = ex.ErrorCode ?? "UNKNOWN", Message = ex.Message } }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during transfer");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Gets a transfer by ID.
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Policy = "Permission:read:transactions")]
    [ProducesResponseType(typeof(TransferResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransfer(Guid id, [FromQuery] string entityType = "fiat")
    {
        var response = await _transferService.GetTransferAsync(id, entityType.ToLowerInvariant() == "fiat");

        if (response == null)
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = $"Transfer '{id}' not found",
                Status = StatusCodes.Status404NotFound
            });

        return Ok(response);
    }
}
```

---

## Phase 4: FluentValidation

**File:** `Application/Validators/Transactions/TransferRequestValidator.cs`

```csharp
using FluentValidation;
using SFManagement.Application.DTOs.Transactions;
using SFManagement.Domain.Enums.Assets;

namespace SFManagement.Application.Validators.Transactions;

public class TransferRequestValidator : AbstractValidator<TransferRequest>
{
    public TransferRequestValidator()
    {
        // Required fields
        RuleFor(x => x.SenderAssetHolderId)
            .NotEmpty().WithMessage("SenderAssetHolderId is required");

        RuleFor(x => x.ReceiverAssetHolderId)
            .NotEmpty().WithMessage("ReceiverAssetHolderId is required");

        RuleFor(x => x.AssetType)
            .NotEqual(AssetType.None).WithMessage("AssetType must be valid (not None)")
            .IsInEnum().WithMessage("AssetType must be a valid enum value");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0")
            .PrecisionScale(18, 2, true).WithMessage("Amount: max 18 digits, 2 decimal places");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)).WithMessage("Date cannot be more than 1 day in future");

        // Optional fields
        RuleFor(x => x.Description)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.ConversionRate)
            .GreaterThanOrEqualTo(0).When(x => x.ConversionRate.HasValue);

        RuleFor(x => x.Rate)
            .InclusiveBetween(0, 100).When(x => x.Rate.HasValue);

        // Optional wallet IDs
        RuleFor(x => x.SenderWalletIdentifierId)
            .NotEqual(Guid.Empty).When(x => x.SenderWalletIdentifierId.HasValue)
            .WithMessage("SenderWalletIdentifierId must be valid if provided");

        RuleFor(x => x.ReceiverWalletIdentifierId)
            .NotEqual(Guid.Empty).When(x => x.ReceiverWalletIdentifierId.HasValue)
            .WithMessage("ReceiverWalletIdentifierId must be valid if provided");
    }
}
```

---

## Phase 5: DI Registration

**File:** `Api/Configuration/DependencyInjectionExtensions.cs`

Add in `AddScopedServices` method:

```csharp
// Transaction services (existing)
builder.Services.AddScoped<BaseTransactionService<FiatAssetTransaction>, FiatAssetTransactionService>();
builder.Services.AddScoped<FiatAssetTransactionService>();
builder.Services.AddScoped<BaseTransactionService<DigitalAssetTransaction>, DigitalAssetTransactionService>();
builder.Services.AddScoped<DigitalAssetTransactionService>();
builder.Services.AddScoped<BaseTransactionService<SettlementTransaction>, SettlementTransactionService>();
builder.Services.AddScoped<SettlementTransactionService>();
builder.Services.AddScoped<BaseService<ImportedTransaction>, ImportedTransactionService>();
builder.Services.AddScoped<ImportedTransactionService>();

// NEW: Transfer service
builder.Services.AddScoped<TransferService>();
```

---

## Phase 6: Testing

### Unit Tests

| Test | Description |
|------|-------------|
| `TransferAsync_FiatTransfer_CreatesTransaction` | BRL transfer creates FiatAssetTransaction |
| `TransferAsync_DigitalTransfer_CreatesTransaction` | PokerStars transfer creates DigitalAssetTransaction |
| `TransferAsync_InternalTransfer_SetsFlag` | Same sender/receiver sets IsInternalTransfer |
| `TransferAsync_WithAutoApprove_SetsApprovedAt` | AutoApprove flag works |
| `TransferAsync_WithWalletCreation_CreatesWallets` | CreateWalletsIfMissing works |
| `TransferAsync_WithSpecificWalletIds_UsesProvided` | Optional wallet IDs respected |
| `TransferAsync_InvalidSender_ThrowsException` | Validates sender exists |
| `TransferAsync_InvalidReceiver_ThrowsException` | Validates receiver exists |
| `TransferAsync_WalletMismatch_ThrowsException` | Validates wallet ownership |
| `TransferAsync_InsufficientBalance_ThrowsException` | ValidateBalance flag works |
| `TransferAsync_SameWallet_ThrowsException` | Cannot transfer to same wallet |

### Manual Testing Checklist

| Test | Request | Expected |
|------|---------|----------|
| Fiat transfer | BRL Client→Member | FiatAssetTransaction created |
| Digital transfer | PokerStars Member→Member | DigitalAssetTransaction created |
| Internal transfer | Same asset holder | `IsInternalTransfer: true` |
| Auto wallet creation | Missing wallet | Wallet created, transfer succeeds |
| Specific wallet | Provide wallet IDs | Uses exact wallets |
| Validation error | Invalid sender | 400 with error details |

---

## Deprecation Plan

### Immediate: Add [Obsolete] Attribute

**ClientController.cs:**
```csharp
[Obsolete("Use POST /api/v1/transfer instead. Will be removed in v2.")]
[HttpPost("{id}/send-brazilian-real")]
public async Task<IActionResult> SendBrazilianReais(...)
```

**MemberController.cs:**
```csharp
[Obsolete("Use POST /api/v1/transfer instead. Will be removed in v2.")]
[HttpPost("{id}/send-brazilian-real")]
public async Task<IActionResult> SendBrazilianReais(...)
```

### Future v2: Remove Endpoints

1. Remove deprecated endpoints
2. Remove `SendBrazilianReais` method from `FiatAssetTransactionService`
3. Update documentation

---

## Technical Notes

### WalletIdentifier.BaseAssetHolderId

The `WalletIdentifier` entity has a `[NotMapped]` property `BaseAssetHolderId`:

```csharp
// WalletIdentifier.cs
[NotMapped]
public Guid? BaseAssetHolderId { get; set; }
```

This property is **not stored in the database** but is used by `WalletIdentifierService.Add()` to find or create the appropriate `AssetPool`. This allows creating wallets by specifying only the asset holder ID and asset type, with the service handling the intermediate `AssetPool` layer.

### AccountClassification

- **Banks, PokerManagers**: `AccountClassification.ASSET` (company's holdings)
- **Clients, Members**: `AccountClassification.LIABILITY` (company owes them)

The service determines this automatically based on the asset holder type.

### Wallet Auto-Creation Metadata

Auto-created wallets have placeholder metadata:

| Asset Group | Metadata |
|-------------|----------|
| FiatAssets | `bankName: "Auto-created", pixKey: "pending"` |
| PokerAssets | `inputForTransactions: "auto-created"` |
| CryptoAssets | `walletAddress: "pending", walletCategory: "auto-created"` |

### TransferResponse Behavior Notes

- `SenderWalletCreated` and `ReceiverWalletCreated` are only populated for `POST /transfer`.
- `GET /transfer/{id}` returns these fields as `false` because historical creation state is not stored.

### Description Field Note

- `TransferRequest.Description` is accepted but **not persisted** because `BaseTransaction` has no `Description` field.
- Adding persistence would require a schema change and migrations.

---

## File Summary

| Action | File | Description |
|--------|------|-------------|
| **Create** | `Application/DTOs/Transactions/TransferRequest.cs` | Request DTO |
| **Create** | `Application/DTOs/Transactions/TransferResponse.cs` | Response DTO |
| **Create** | `Application/DTOs/Transactions/TransferError.cs` | Error codes |
| **Create** | `Application/Services/Transactions/TransferService.cs` | Business logic |
| **Create** | `Api/Controllers/v1/Transactions/TransferController.cs` | API endpoint |
| **Create** | `Application/Validators/Transactions/TransferRequestValidator.cs` | Validation |
| **Modify** | `Api/Configuration/DependencyInjectionExtensions.cs` | DI registration |
| **Modify** | `Api/Controllers/v1/AssetHolders/ClientController.cs` | [Obsolete] |
| **Modify** | `Api/Controllers/v1/AssetHolders/MemberController.cs` | [Obsolete] |

---

## Execution Checklist

### Phase 1: DTOs
- [ ] Create `TransferRequest.cs`
- [ ] Create `TransferResponse.cs`
- [ ] Create `TransferError.cs`
- [ ] `dotnet build` - verify no errors

### Phase 2: TransferService
- [ ] Create `TransferService.cs`
- [ ] `dotnet build` - verify no errors

### Phase 3: TransferController
- [ ] Create `TransferController.cs`
- [ ] `dotnet build` - verify no errors

### Phase 4: FluentValidation
- [ ] Create `TransferRequestValidator.cs`
- [ ] `dotnet build` - verify no errors

### Phase 5: DI Registration
- [ ] Update `DependencyInjectionExtensions.cs`
- [ ] `dotnet build` - verify no errors
- [ ] `dotnet run` - verify app starts
- [ ] Verify `/transfer` appears in Swagger

### Phase 6: Testing
- [ ] Test Fiat transfer (BRL)
- [ ] Test Digital transfer (PokerStars)
- [ ] Test Internal transfer
- [ ] Test wallet auto-creation
- [ ] Test specific wallet IDs
- [ ] Test validation errors
- [ ] Test authorization

### Phase 7: Deprecation
- [ ] Add `[Obsolete]` to `ClientController.SendBrazilianReais`
- [ ] Add `[Obsolete]` to `MemberController.SendBrazilianReais`

### Final Verification
- [ ] `dotnet build` - 0 errors
- [ ] `dotnet run` - Application starts
- [ ] Swagger UI shows `/transfer` endpoint
- [ ] All test cases pass

---

*Document Version: 1.1 | Last Updated: January 22, 2026*

---

## Implementation Review

> **Reviewed:** January 22, 2026  
> **Reviewer:** Code Review  
> **Status:** ✅ Implementation Complete - Minor Observations

### Review Summary

The backend implementation has been completed successfully. All planned files were created and the solution builds without errors.

### Files Verified

| File | Status | Notes |
|------|--------|-------|
| `Application/DTOs/Transactions/TransferRequest.cs` | ✅ Complete | All fields implemented correctly |
| `Application/DTOs/Transactions/TransferResponse.cs` | ✅ Complete | Includes split wallet creation flags |
| `Application/DTOs/Transactions/TransferError.cs` | ✅ Complete | All error codes defined |
| `Application/Services/Transactions/TransferService.cs` | ✅ Complete | Full implementation with all features |
| `Api/Controllers/v1/Transactions/TransferController.cs` | ✅ Complete | POST and GET endpoints |
| `Application/Validators/Transactions/TransferRequestValidator.cs` | ✅ Complete | FluentValidation rules |
| `Api/Configuration/DependencyInjectionExtensions.cs` | ✅ Updated | TransferService registered |
| `Api/Controllers/v1/AssetHolders/ClientController.cs` | ✅ Updated | [Obsolete] attribute added |
| `Api/Controllers/v1/AssetHolders/MemberController.cs` | ✅ Updated | [Obsolete] attribute added |
| `Api/Controllers/v1/AssetHolders/PokerManagerController.cs` | ✅ Updated | [Obsolete] attribute added |

### Build Verification

```
dotnet build --no-restore
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### Implementation Quality Assessment

#### ✅ Correctly Implemented

1. **Database Transaction Handling**
   - Uses `BeginTransactionAsync()` with proper rollback on errors
   - Atomic operation for wallet creation + transaction

2. **Wallet Auto-Creation**
   - Correctly uses `WalletIdentifierService.Add()` with `[NotMapped] BaseAssetHolderId`
   - Service handles AssetPool creation automatically
   - Proper account classification determination (ASSET vs LIABILITY)

3. **Optional Wallet ID Support**
   - Both `SenderWalletIdentifierId` and `ReceiverWalletIdentifierId` implemented
   - Validation ensures wallet belongs to correct asset holder
   - Validation ensures wallet matches requested asset type

4. **Balance Validation**
   - Correctly queries InitialBalance by `BaseAssetHolderId` and `AssetType`
   - Properly calculates: `initialBalance + incoming - outgoing`

5. **BusinessException Usage**
   - Correct constructor signature: `(message, code)` and `(message, exception, code)`
   - All error codes from `TransferErrorCodes` used consistently

6. **Deprecation**
   - `[Obsolete]` attribute added to all three `send-brazilian-real` endpoints
   - Clear migration message pointing to `/api/v1/transfer`

#### ⚠️ Minor Observations (Non-Blocking)

1. **Description Field Not Persisted**
   
   The `TransferRequest.Description` field is accepted but not saved to the transaction entity:
   
   ```csharp
   // TransferService.cs line ~163
   var fiatTransaction = new FiatAssetTransaction
   {
       // ... other fields
       // Description is NOT set here
   };
   ```
   
   **Impact:** Low - Description is optional and may not be supported by `BaseTransaction`
   
   **Recommendation:** Either:
   - Remove `Description` from `TransferRequest` if not supported
   - Or add `Description` property to transaction entities if needed

2. **GetTransferAsync Returns Incomplete Data**
   
   The `GetTransferAsync` method doesn't populate `SenderWalletCreated` and `ReceiverWalletCreated`:
   
   ```csharp
   // TransferService.cs line ~420
   return new TransferResponse
   {
       // ...
       // SenderWalletCreated and ReceiverWalletCreated are not set (default false)
   };
   ```
   
   **Impact:** Low - These flags are only meaningful at creation time
   
   **Recommendation:** Document that these flags are only populated on POST, not GET

3. **Logging in Controller Only**
   
   Logging is only in the controller, not in the service. For debugging production issues, service-level logging could be helpful.
   
   **Impact:** Low - Controller logging is sufficient for most cases
   
   **Recommendation:** Consider adding structured logging in `TransferService` for:
   - Wallet creation events
   - Balance validation results

### Review Decisions & Actions

The following actions are applied based on the review:

1. **Description Field Persistence**: **Not implemented**
   - **Reason:** `BaseTransaction` has no `Description` field. Persisting this would require a schema change and migrations.
   - **Decision:** Keep `Description` in `TransferRequest` for forward compatibility and potential future UI needs, but document that it is not stored.

2. **Wallet Creation Flags on GET**: **Documented**
   - **Reason:** `SenderWalletCreated` and `ReceiverWalletCreated` are only meaningful at creation time.
   - **Decision:** Documented behavior that GET responses do not populate these flags (default `false`).

3. **Service-Level Logging**: **Not implemented**
   - **Reason:** Controller logging is sufficient for current observability and avoids duplicating logs.
   - **Decision:** Defer until there is a production need for more granular tracing.

#### 🔍 Code Quality Notes

1. **Clean Separation of Concerns**
   - Controller handles HTTP concerns only
   - Service handles all business logic
   - DTOs are well-documented with XML comments

2. **Defensive Programming**
   - Null checks throughout
   - Proper use of nullable types
   - Transaction rollback on any exception

3. **Consistent Error Handling**
   - All business errors use `BusinessException` with codes
   - Controller converts to `ProblemDetails` for API response

### Test Recommendations

Before deploying to production, verify:

| Test Case | Expected Result |
|-----------|-----------------|
| Fiat transfer (BRL) between Client → Member | FiatAssetTransaction created |
| Digital transfer (PokerStars) between Member → Member | DigitalAssetTransaction created |
| Internal transfer (same asset holder) | `IsInternalTransfer: true` |
| Auto wallet creation | Wallet + AssetPool created |
| Specific wallet IDs | Uses exact wallets provided |
| Wallet ownership mismatch | 400 with `WALLET_OWNERSHIP_MISMATCH` |
| Asset type mismatch | 400 with `ASSET_TYPE_MISMATCH` |
| Insufficient balance (with `ValidateBalance: true`) | 400 with `INSUFFICIENT_BALANCE` |
| Same wallet transfer | 400 with `SAME_SENDER_RECEIVER_WALLET` |
| Invalid sender | 400 with `SENDER_NOT_FOUND` |
| Swagger UI | `/transfer` endpoint visible with documentation |

### Conclusion

The implementation is **production-ready**. The minor observations are non-blocking and can be addressed in future iterations if needed.

**Next Steps:**
1. ✅ Backend implementation complete
2. ⏳ Frontend implementation (see `TRANSACTION_REFACTOR_IMPLEMENTATION.md`)
3. ⏳ Integration testing
4. ⏳ Remove deprecated endpoints in v2

---

*Review completed: January 22, 2026*
