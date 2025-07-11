using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Enums;
using SFManagement.Models.AssetInfrastructure;

namespace SFManagement.Services;

/// <summary>
/// Validation service for AssetPool operations with comprehensive business rules
/// </summary>
public class AssetPoolValidationService
{
    private readonly DataContext _context;

    public AssetPoolValidationService(DataContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Validates AssetPool creation with comprehensive business rules
    /// </summary>
    public async Task<AssetPoolValidationResult> ValidateAssetPoolCreation(AssetPool assetPool)
    {
        var result = new AssetPoolValidationResult();

        // Validate AssetType
        if (!Enum.IsDefined(typeof(AssetType), assetPool.AssetType))
        {
            result.AddError("AssetType", "Invalid AssetType specified", "INVALID_ASSET_TYPE");
            return result; // Early return for critical validation
        }

        if (assetPool.BaseAssetHolderId.HasValue)
        {
            await ValidateAssetHolderPool(assetPool, result);
        }
        else
        {
            await ValidateCompanyPool(assetPool, result);
        }

        return result;
    }

    /// <summary>
    /// Validates asset holder owned pools
    /// </summary>
    private async Task ValidateAssetHolderPool(AssetPool assetPool, AssetPoolValidationResult result)
    {
        // Validate BaseAssetHolder exists
        var assetHolderExists = await _context.BaseAssetHolders
            .AnyAsync(bah => bah.Id == assetPool.BaseAssetHolderId!.Value && !bah.DeletedAt.HasValue);

        if (!assetHolderExists)
        {
            result.AddError("BaseAssetHolderId", $"BaseAssetHolder {assetPool.BaseAssetHolderId} does not exist", "ASSET_HOLDER_NOT_FOUND");
            return;
        }

        // Check for duplicate AssetPool
        var existingPool = await _context.AssetPools
            .FirstOrDefaultAsync(ap => ap.BaseAssetHolderId == assetPool.BaseAssetHolderId &&
                                     ap.AssetType == assetPool.AssetType &&
                                     !ap.DeletedAt.HasValue);

        if (existingPool != null)
        {
            result.AddError("AssetType", $"BaseAssetHolder {assetPool.BaseAssetHolderId} already has an AssetPool for {assetPool.AssetType}", "DUPLICATE_ASSET_POOL");
        }
    }

    /// <summary>
    /// Validates company owned pools with enhanced business rules
    /// </summary>
    private async Task ValidateCompanyPool(AssetPool assetPool, AssetPoolValidationResult result)
    {
        // Check for existing company pool of same type
        var existingCompanyPool = await _context.AssetPools
            .FirstOrDefaultAsync(ap => ap.BaseAssetHolderId == null &&
                                     ap.AssetType == assetPool.AssetType &&
                                     !ap.DeletedAt.HasValue);

        if (existingCompanyPool != null)
        {
            result.AddError("AssetType", $"Company already has an AssetPool for {assetPool.AssetType}. Existing pool ID: {existingCompanyPool.Id}", "DUPLICATE_COMPANY_POOL");
        }

        // Business rule: Validate if company should own this asset type
        await ValidateCompanyAssetTypeOwnership(assetPool.AssetType, result);
    }

    /// <summary>
    /// Validates business rules for company asset type ownership
    /// </summary>
    private async Task ValidateCompanyAssetTypeOwnership(AssetType assetType, AssetPoolValidationResult result)
    {
        // Example business rules - customize based on your requirements
        var restrictedAssetTypes = new AssetType[]
        {
            // Add asset types that should not be company-owned
            // AssetType.PersonalCrypto, // Example
        };

        if (restrictedAssetTypes.Contains(assetType))
        {
            result.AddError("AssetType", $"Asset type {assetType} cannot be owned by the company", "RESTRICTED_COMPANY_ASSET_TYPE");
        }

        // Additional validation: Check if there are related asset holders that should own this type
        var relatedAssetHolders = await _context.BaseAssetHolders
            .Where(bah => !bah.DeletedAt.HasValue)
            .CountAsync();

        if (relatedAssetHolders == 0 && assetType != AssetType.BrazilianReal)
        {
            result.AddWarning("AssetType", $"Creating company pool for {assetType} with no asset holders in system", "NO_ASSET_HOLDERS_WARNING");
        }
    }

    /// <summary>
    /// Validates AssetPool deletion
    /// </summary>
    public async Task<AssetPoolValidationResult> ValidateAssetPoolDeletion(Guid assetPoolId)
    {
        var result = new AssetPoolValidationResult();

        var assetPool = await _context.AssetPools
            .Include(ap => ap.WalletIdentifiers)
            .FirstOrDefaultAsync(ap => ap.Id == assetPoolId && !ap.DeletedAt.HasValue);

        if (assetPool == null)
        {
            result.AddError("AssetPoolId", "AssetPool not found", "ASSET_POOL_NOT_FOUND");
            return result;
        }

        // Check for active wallet identifiers
        var activeWalletIdentifiers = assetPool.WalletIdentifiers.Where(wi => !wi.DeletedAt.HasValue).ToList();
        if (activeWalletIdentifiers.Any())
        {
            result.AddError("WalletIdentifiers", $"Cannot delete AssetPool with {activeWalletIdentifiers.Count} active wallet identifiers", "ACTIVE_WALLET_IDENTIFIERS");
        }

        // Check for transactions
        var walletIdentifierIds = activeWalletIdentifiers.Select(wi => wi.Id).ToList();
        if (walletIdentifierIds.Any())
        {
            var hasTransactions = await _context.FiatAssetTransactions
                .AnyAsync(ft => walletIdentifierIds.Contains(ft.SenderWalletIdentifierId) ||
                              walletIdentifierIds.Contains(ft.ReceiverWalletIdentifierId));

            if (hasTransactions)
            {
                result.AddError("Transactions", "Cannot delete AssetPool with existing transactions", "EXISTING_TRANSACTIONS");
            }
        }

        return result;
    }
}

/// <summary>
/// Validation result for AssetPool operations
/// </summary>
public class AssetPoolValidationResult
{
    public bool IsValid => !Errors.Any();
    public List<AssetPoolValidationError> Errors { get; } = new();
    public List<AssetPoolValidationError> Warnings { get; } = new();

    public void AddError(string field, string message, string code)
    {
        Errors.Add(new AssetPoolValidationError(field, message, code));
    }

    public void AddWarning(string field, string message, string code)
    {
        Warnings.Add(new AssetPoolValidationError(field, message, code));
    }
}

/// <summary>
/// Individual validation error for AssetPool operations
/// </summary>
public class AssetPoolValidationError
{
    public string Field { get; }
    public string Message { get; }
    public string Code { get; }

    public AssetPoolValidationError(string field, string message, string code)
    {
        Field = field;
        Message = message;
        Code = code;
    }
} 