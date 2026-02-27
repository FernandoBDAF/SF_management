using Microsoft.EntityFrameworkCore;
using SFManagement.Application.DTOs.Transactions;
using SFManagement.Application.Services.Assets;
using SFManagement.Application.Services.Finance;
using SFManagement.Application.Services.Infrastructure;
using SFManagement.Application.Services.Validation;
using SFManagement.Domain.Entities.Assets;
using SFManagement.Domain.Entities.AssetHolders;
using SFManagement.Domain.Entities.Transactions;
using SFManagement.Domain.Enums;
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
    private readonly IAvgRateService _avgRateService;
    private readonly ICachedLookupService _cachedLookupService;

    public TransferService(
        DataContext context,
        WalletIdentifierService walletIdentifierService,
        IAvgRateService avgRateService,
        ICachedLookupService cachedLookupService)
    {
        _context = context;
        _walletIdentifierService = walletIdentifierService;
        _avgRateService = avgRateService;
        _cachedLookupService = cachedLookupService;
    }

    /// <summary>
    /// Creates a transfer between two asset holders.
    /// </summary>
    public async Task<TransferResponse> TransferAsync(TransferRequest request)
    {
        // Use execution strategy to support SqlServerRetryingExecutionStrategy with transactions
        var strategy = _context.Database.CreateExecutionStrategy();
        
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // 1. Validate asset type
                if (request.AssetType == AssetType.None)
                {
                    throw new BusinessException("AssetType.None is not valid for transfers", TransferErrorCodes.InvalidAssetType);
                }

            // Keep legacy flag validation to reject deprecated behavior.
            // Suppress CS0618 only for this block to avoid warning noise.
#pragma warning disable CS0618
            if (request.CreateWalletsIfMissing)
            {
                throw new BusinessException(
                    "Automatic wallet creation is no longer supported. Create wallets explicitly before initiating transfer.",
                    "WALLETS_CREATION_DEPRECATED");
            }
#pragma warning restore CS0618

            // 2. Determine transaction type
            var assetGroup = WalletIdentifierValidationService.GetAssetGroupForAssetType(request.AssetType);
            var isFiat = assetGroup == AssetGroup.FiatAssets;
            var entityType = isFiat ? "fiat" : "digital";

            // 3. Validate sender exists (or is system/company with null/Guid.Empty)
            var senderAssetHolderId = request.SenderAssetHolderId.GetValueOrDefault();
            var isSenderSystem = !request.SenderAssetHolderId.HasValue || request.SenderAssetHolderId.Value == Guid.Empty;
            BaseAssetHolder? senderAssetHolder = null;
            string senderName = "Company";
            
            if (isSenderSystem)
            {
                // System operation: sender is the company
                // Validate that a sender wallet is provided for system operations
                if (!request.SenderWalletIdentifierId.HasValue)
                {
                    throw new BusinessException(
                        "System operations require a specific sender wallet",
                        TransferErrorCodes.SenderWalletNotFound);
                }
            }
            else
            {
                senderAssetHolder = await _context.BaseAssetHolders
                    .FirstOrDefaultAsync(ah => ah.Id == senderAssetHolderId && !ah.DeletedAt.HasValue)
                    ?? throw new BusinessException(
                        $"Sender asset holder '{senderAssetHolderId}' not found",
                        TransferErrorCodes.SenderNotFound);
                senderName = senderAssetHolder.Name;
            }

            // 4. Validate receiver exists (or is system/company with null/Guid.Empty)
            var receiverAssetHolderId = request.ReceiverAssetHolderId.GetValueOrDefault();
            var isReceiverSystem = !request.ReceiverAssetHolderId.HasValue || request.ReceiverAssetHolderId.Value == Guid.Empty;
            BaseAssetHolder? receiverAssetHolder = null;
            string receiverName = "Company";
            
            if (isReceiverSystem)
            {
                // System operation: receiver is the company
                // Validate that a receiver wallet is provided for system operations
                if (!request.ReceiverWalletIdentifierId.HasValue)
                {
                    throw new BusinessException(
                        "System operations require a specific receiver wallet",
                        TransferErrorCodes.ReceiverWalletNotFound);
                }
            }
            else
            {
                receiverAssetHolder = await _context.BaseAssetHolders
                    .FirstOrDefaultAsync(ah => ah.Id == receiverAssetHolderId && !ah.DeletedAt.HasValue)
                    ?? throw new BusinessException(
                        $"Receiver asset holder '{receiverAssetHolderId}' not found",
                        TransferErrorCodes.ReceiverNotFound);
                receiverName = receiverAssetHolder.Name;
            }

            // 4.1 Validate banks are not involved in TRANSFER mode (inferred)
            var isInternalTransfer = senderAssetHolderId == receiverAssetHolderId;
            if (!isInternalTransfer)
            {
                await ValidateNoBanksInTransferAsync(request);
            }

            // 4.2 Check wallet existence (wallets must exist before transfer)
            var walletError = await CheckWalletsExistAsync(request);
            if (walletError != null)
            {
                throw new WalletMissingException(walletError);
            }

            // 5. Validate category if provided
            if (request.CategoryId.HasValue)
            {
                var categoryExists = await _context.Categories
                    .AnyAsync(c => c.Id == request.CategoryId.Value && !c.DeletedAt.HasValue);
                
                if (!categoryExists)
                {
                    throw new BusinessException(
                        $"Category '{request.CategoryId}' not found",
                        TransferErrorCodes.CategoryNotFound);
                }
            }

            // 6. Get sender wallet
            WalletIdentifier? senderWallet;
            bool senderWalletCreated = false;

            if (request.SenderWalletIdentifierId.HasValue)
            {
                senderWallet = await GetAndValidateWalletAsync(
                    request.SenderWalletIdentifierId.Value,
                    senderAssetHolderId,
                    request.AssetType,
                    "Sender");
            }
            else
            {
                var senderWalletResult = await FindOrCreateWalletAsync(
                    senderAssetHolderId,
                    request.AssetType,
                    false);
                senderWallet = senderWalletResult.Wallet;
                senderWalletCreated = senderWalletResult.Created;
                
                if (senderWallet == null)
                {
                    throw new BusinessException(
                        $"Sender wallet for '{request.AssetType}' not found",
                        TransferErrorCodes.SenderWalletNotFound);
                }
            }

            // 7. Get receiver wallet
            WalletIdentifier? receiverWallet;
            bool receiverWalletCreated = false;

            if (request.ReceiverWalletIdentifierId.HasValue)
            {
                receiverWallet = await GetAndValidateWalletAsync(
                    request.ReceiverWalletIdentifierId.Value,
                    receiverAssetHolderId,
                    request.AssetType,
                    "Receiver");
            }
            else
            {
                var receiverWalletResult = await FindOrCreateWalletAsync(
                    receiverAssetHolderId,
                    request.AssetType,
                    false);
                receiverWallet = receiverWalletResult.Wallet;
                receiverWalletCreated = receiverWalletResult.Created;
                
                if (receiverWallet == null)
                {
                    throw new BusinessException(
                        $"Receiver wallet for '{request.AssetType}' not found",
                        TransferErrorCodes.ReceiverWalletNotFound);
                }
            }

            // 8. Validate not same wallet
            if (senderWallet.Id == receiverWallet.Id)
            {
                throw new BusinessException(
                    "Cannot transfer to the same wallet",
                    TransferErrorCodes.SameSenderReceiverWallet);
            }

            // 9. Validate balance if requested
            if (request.ValidateBalance)
            {
                var balance = await GetBalanceForWalletAsync(senderWallet.Id, isFiat);
                if (balance < request.Amount)
                {
                    throw new BusinessException(
                        $"Insufficient balance. Available: {balance:N2}, Requested: {request.Amount:N2}",
                        TransferErrorCodes.InsufficientBalance);
                }
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

            // 12. Invalidate AvgRate cache for affected PokerManagers
            if (!isFiat)
            {
                await InvalidateAvgRateCacheAsync(senderWallet, receiverWallet, request.Date);
            }

            // 13. Return response
            return new TransferResponse
            {
                TransactionId = transactionId,
                EntityType = entityType,
                AssetType = request.AssetType,
                SenderWalletIdentifierId = senderWallet.Id,
                SenderAssetHolderId = senderAssetHolderId,
                SenderName = senderName,
                ReceiverWalletIdentifierId = receiverWallet.Id,
                ReceiverAssetHolderId = receiverAssetHolderId,
                ReceiverName = receiverName,
                Amount = request.Amount,
                Date = request.Date,
                IsInternalTransfer = isInternalTransfer,
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
                throw new BusinessException($"Transfer failed: {ex.Message}", ex, TransferErrorCodes.TransactionFailed);
            }
        });
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
        {
            throw new BusinessException(
                $"{participantName} wallet '{walletIdentifierId}' not found",
                participantName == "Sender"
                    ? TransferErrorCodes.SenderWalletNotFound
                    : TransferErrorCodes.ReceiverWalletNotFound);
        }
        
        // For system operations (Guid.Empty), the wallet should have null BaseAssetHolderId (company-owned)
        // For regular operations, the wallet must belong to the specified asset holder
        var isSystemOperation = expectedAssetHolderId == Guid.Empty;
        var walletAssetHolderId = wallet.AssetPool?.BaseAssetHolderId;
        
        if (isSystemOperation)
        {
            // System wallet should have null BaseAssetHolderId (company-owned)
            if (walletAssetHolderId.HasValue && walletAssetHolderId.Value != Guid.Empty)
            {
                throw new BusinessException(
                    $"{participantName} wallet is not a system/company wallet",
                    TransferErrorCodes.WalletOwnershipMismatch);
            }
        }
        else
        {
            // Regular wallet must belong to the specified asset holder
            if (walletAssetHolderId != expectedAssetHolderId)
            {
                throw new BusinessException(
                    $"{participantName} wallet does not belong to the specified asset holder",
                    TransferErrorCodes.WalletOwnershipMismatch);
            }
        }
        
        if (wallet.AssetType != expectedAssetType)
        {
            throw new BusinessException(
                $"{participantName} wallet asset type ({wallet.AssetType}) doesn't match request ({expectedAssetType})",
                TransferErrorCodes.AssetTypeMismatch);
        }
        
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
        {
            return (existing, false);
        }
        
        if (!createIfMissing)
        {
            return (null, false);
        }
        
        // Create new wallet (service handles AssetPool creation via [NotMapped] BaseAssetHolderId)
        try
        {
            var newWallet = new WalletIdentifier
            {
                BaseAssetHolderId = assetHolderId,
                AssetType = assetType,
                AccountClassification = await DetermineAccountClassificationAsync(assetHolderId, assetType)
            };
            
            SetDefaultMetadata(newWallet, assetType);
            
            var created = await _walletIdentifierService.Add(newWallet);
            return (created, true);
        }
        catch (Exception ex)
        {
            throw new BusinessException($"Failed to create wallet: {ex.Message}", ex, TransferErrorCodes.WalletCreationFailed);
        }
    }

    /// <summary>
    /// Determines account classification based on entity type and asset group.
    /// </summary>
    private async Task<AccountClassification> DetermineAccountClassificationAsync(
        Guid assetHolderId,
        AssetType assetType)
    {
        var isBank = await _context.Banks.AnyAsync(b => b.BaseAssetHolderId == assetHolderId);
        if (isBank)
        {
            return AccountClassification.ASSET;
        }
        
        var isPokerManager = await _cachedLookupService.IsPokerManagerAsync(assetHolderId);
        if (isPokerManager)
        {
            var assetGroup = WalletIdentifierValidationService.GetAssetGroupForAssetType(assetType);
            return assetGroup == AssetGroup.FiatAssets
                ? AccountClassification.LIABILITY
                : AccountClassification.ASSET;
        }
        
        return AccountClassification.LIABILITY;
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
        decimal incoming;
        decimal outgoing;
        
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
        
        var wallet = await _context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
            .FirstOrDefaultAsync(wi => wi.Id == walletIdentifierId && !wi.DeletedAt.HasValue);

        if (wallet == null)
        {
            throw new BusinessException(
                $"Wallet '{walletIdentifierId}' not found for balance validation",
                TransferErrorCodes.SenderWalletNotFound);
        }

        var baseAssetHolderId = wallet.AssetPool?.BaseAssetHolderId;
        var initialBalance = 0m;

        if (baseAssetHolderId.HasValue)
        {
            initialBalance = await _context.InitialBalances
                .Where(ib => !ib.DeletedAt.HasValue &&
                    ib.BaseAssetHolderId == baseAssetHolderId.Value &&
                    ib.AssetType == wallet.AssetType)
                .SumAsync(ib => (decimal?)ib.Balance) ?? 0;
        }
        
        return initialBalance + incoming - outgoing;
    }

    private async Task<WalletMissingError?> CheckWalletsExistAsync(TransferRequest request)
    {
        var senderAssetHolderId = request.SenderAssetHolderId.GetValueOrDefault();
        var receiverAssetHolderId = request.ReceiverAssetHolderId.GetValueOrDefault();
        var isSenderSystem = !request.SenderAssetHolderId.HasValue || request.SenderAssetHolderId.Value == Guid.Empty;
        var isReceiverSystem = !request.ReceiverAssetHolderId.HasValue || request.ReceiverAssetHolderId.Value == Guid.Empty;
        
        WalletIdentifier? senderWallet = null;
        WalletIdentifier? receiverWallet = null;

        // For system operations with wallet IDs provided, skip wallet existence check
        // For non-system operations without wallet IDs, check if wallet exists
        if (!request.SenderWalletIdentifierId.HasValue && !isSenderSystem)
        {
            senderWallet = await FindWalletByAssetHolderAndType(senderAssetHolderId, request.AssetType);
        }

        if (!request.ReceiverWalletIdentifierId.HasValue && !isReceiverSystem)
        {
            receiverWallet = await FindWalletByAssetHolderAndType(receiverAssetHolderId, request.AssetType);
        }

        var senderMissing = senderWallet == null && !request.SenderWalletIdentifierId.HasValue && !isSenderSystem;
        var receiverMissing = receiverWallet == null && !request.ReceiverWalletIdentifierId.HasValue && !isReceiverSystem;

        if (!senderMissing && !receiverMissing)
        {
            return null;
        }

        var senderInfo = senderMissing
            ? await GetAssetHolderInfoAsync(senderAssetHolderId)
            : (Name: null, Type: null);
        var receiverInfo = receiverMissing
            ? await GetAssetHolderInfoAsync(receiverAssetHolderId)
            : (Name: null, Type: null);

        var assetTypeName = GetAssetTypeName(request.AssetType);

        return new WalletMissingError
        {
            SenderWalletMissing = senderMissing,
            SenderAssetHolderId = senderMissing ? senderAssetHolderId : null,
            SenderAssetHolderName = senderMissing ? senderInfo.Name : null,
            SenderAssetHolderType = senderMissing ? senderInfo.Type : null,
            SenderAssetTypeName = senderMissing ? assetTypeName : null,
            ReceiverWalletMissing = receiverMissing,
            ReceiverAssetHolderId = receiverMissing ? receiverAssetHolderId : null,
            ReceiverAssetHolderName = receiverMissing ? receiverInfo.Name : null,
            ReceiverAssetHolderType = receiverMissing ? receiverInfo.Type : null,
            ReceiverAssetTypeName = receiverMissing ? assetTypeName : null
        };
    }

    private async Task<WalletIdentifier?> FindWalletByAssetHolderAndType(Guid assetHolderId, AssetType assetType)
    {
        var wallets = await _walletIdentifierService.GetByAssetHolderAndAssetType(assetHolderId, assetType);
        return wallets.FirstOrDefault();
    }

    private async Task<(string? Name, string? Type)> GetAssetHolderInfoAsync(Guid assetHolderId)
    {
        var name = await _context.BaseAssetHolders
            .Where(ah => ah.Id == assetHolderId && !ah.DeletedAt.HasValue)
            .Select(ah => ah.Name)
            .FirstOrDefaultAsync();

        var type = await GetAssetHolderTypeAsync(assetHolderId);

        return (name, type);
    }

    private async Task<string?> GetAssetHolderTypeAsync(Guid assetHolderId)
    {
        // System/Company operations use Guid.Empty
        if (assetHolderId == Guid.Empty)
        {
            return "Company";
        }
        
        if (await _context.Clients.AnyAsync(c => c.BaseAssetHolderId == assetHolderId && !c.DeletedAt.HasValue))
        {
            return "Client";
        }

        if (await _context.Members.AnyAsync(m => m.BaseAssetHolderId == assetHolderId && !m.DeletedAt.HasValue))
        {
            return "Member";
        }

        if (await _context.Banks.AnyAsync(b => b.BaseAssetHolderId == assetHolderId && !b.DeletedAt.HasValue))
        {
            return "Bank";
        }

        if (await _cachedLookupService.IsPokerManagerAsync(assetHolderId))
        {
            return "PokerManager";
        }

        return null;
    }

    private static string GetAssetTypeName(AssetType assetType)
    {
        return assetType.ToString();
    }

    private async Task ValidateNoBanksInTransferAsync(TransferRequest request)
    {
        var senderAssetHolderId = request.SenderAssetHolderId.GetValueOrDefault();
        var receiverAssetHolderId = request.ReceiverAssetHolderId.GetValueOrDefault();
        var senderType = await GetAssetHolderTypeAsync(senderAssetHolderId);
        var receiverType = await GetAssetHolderTypeAsync(receiverAssetHolderId);
        
        // Determine if this is a bank transaction (RECEIPT or PAYMENT mode)
        var isSenderBank = senderType == "Bank";
        var isReceiverBank = receiverType == "Bank";
        var isFiatAsset = IsFiatAssetType(request.AssetType);
        
        // RECEIPT mode: Non-bank → Bank (fiat only) - ALLOWED
        // PAYMENT mode: Bank → Non-bank (fiat only) - ALLOWED
        if (isFiatAsset && (isSenderBank || isReceiverBank))
        {
            // This is a valid RECEIPT or PAYMENT transaction
            // Banks can only participate with fiat assets
            if (isSenderBank && isReceiverBank)
            {
                throw new BusinessException(
                    "Bank-to-bank transfers are not allowed.",
                    "BANK_TO_BANK_NOT_ALLOWED");
            }
            
            // Valid bank transaction - allow it
            return;
        }
        
        // TRANSFER mode: Non-bank → Non-bank - Banks not allowed
        if (isSenderBank)
        {
            throw new BusinessException(
                "Banks can only send fiat assets (BRL). Use Payment mode for fiat transactions.",
                "BANK_NOT_ALLOWED_IN_TRANSFER");
        }

        if (isReceiverBank)
        {
            throw new BusinessException(
                "Banks can only receive fiat assets (BRL). Use Receipt mode for fiat transactions.",
                "BANK_NOT_ALLOWED_IN_TRANSFER");
        }
    }
    
    private static bool IsFiatAssetType(AssetType assetType)
    {
        // FiatAssets: AssetTypes 1-20 (see AssetType enum)
        // BrazilianReal = 21 is the main fiat type
        var value = (int)assetType;
        return value >= 1 && value <= 30; // Fiat range (including BRL at 21)
    }

    /// <summary>
    /// Invalidates AvgRate cache for any PokerManagers involved in the transaction.
    /// </summary>
    private async Task InvalidateAvgRateCacheAsync(
        WalletIdentifier senderWallet,
        WalletIdentifier receiverWallet,
        DateTime transactionDate)
    {
        var affectedManagerIds = new HashSet<Guid>();

        if (senderWallet.AssetPool?.BaseAssetHolderId != null &&
            senderWallet.AssetPool.AssetGroup == AssetGroup.PokerAssets)
        {
            var isPokerManager = await _cachedLookupService.IsPokerManagerAsync(
                senderWallet.AssetPool.BaseAssetHolderId.Value);
            if (isPokerManager)
            {
                affectedManagerIds.Add(senderWallet.AssetPool.BaseAssetHolderId.Value);
            }
        }

        if (receiverWallet.AssetPool?.BaseAssetHolderId != null &&
            receiverWallet.AssetPool.AssetGroup == AssetGroup.PokerAssets)
        {
            var isPokerManager = await _cachedLookupService.IsPokerManagerAsync(
                receiverWallet.AssetPool.BaseAssetHolderId.Value);
            if (isPokerManager)
            {
                affectedManagerIds.Add(receiverWallet.AssetPool.BaseAssetHolderId.Value);
            }
        }

        foreach (var managerId in affectedManagerIds)
        {
            await _avgRateService.InvalidateFromDate(managerId, transactionDate);
        }
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

        if (txn == null)
        {
            return null;
        }

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
            CreatedAt = txn.CreatedAt ?? txn.Date
        };
    }
}

