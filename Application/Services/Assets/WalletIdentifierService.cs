using Microsoft.EntityFrameworkCore;
using SFManagement.Application.DTOs.Assets;
using SFManagement.Application.Services.Base;
using SFManagement.Application.Services.Support;
using SFManagement.Application.Services.Validation;
using SFManagement.Domain.Entities.Assets;
using SFManagement.Domain.Enums;
using SFManagement.Domain.Enums.Assets;
using SFManagement.Domain.Enums.Metadata;
using SFManagement.Infrastructure.Data;

namespace SFManagement.Application.Services.Assets;

public class WalletIdentifierService : BaseService<WalletIdentifier>
{
    private readonly WalletIdentifierValidationService _validationService;
    private readonly AssetPoolService _AssetPoolService;
    private readonly ReferralService _referralService;
    
    public WalletIdentifierService(DataContext context, IHttpContextAccessor httpContextAccessor, 
    AssetPoolService AssetPoolService, ReferralService referralService) : base(context, httpContextAccessor)
    {
        _validationService = new WalletIdentifierValidationService();
        _AssetPoolService = AssetPoolService;
        _referralService = referralService;
    }

    
    public override async Task<WalletIdentifier> Add(WalletIdentifier walletIdentifier)
    {
        AssetPool? assetPool = null;
        
        // Scenario 1: AssetPoolId is provided - use existing AssetPool
        if (walletIdentifier.AssetPoolId != Guid.Empty)
        {
            assetPool = await _AssetPoolService.Get(walletIdentifier.AssetPoolId) ?? walletIdentifier.AssetPool;
            if (assetPool == null)
            {
                throw new ArgumentException($"AssetPool with ID {walletIdentifier.AssetPoolId} not found");
            }
        }
        // Scenario 2: BaseAssetHolderId + AssetType provided - find or create AssetPool with specific AssetGroup
        else if (walletIdentifier.BaseAssetHolderId.HasValue && 
                 !walletIdentifier.BaseAssetHolderId.Equals(Guid.Empty))
        {
            // Validate AssetType is a valid enum value
            if (!Enum.IsDefined(typeof(AssetType), walletIdentifier.AssetType))
            {
                throw new ArgumentException($"Invalid AssetType: {walletIdentifier.AssetType}");
            }
            
            var expectedAssetGroup = WalletIdentifierValidationService.GetAssetGroupForAssetType(walletIdentifier.AssetType);
            
            // Use the expected AssetGroup from AssetType (this ensures consistency)
            assetPool = await _AssetPoolService.GetByBaseAssetHolderAndType(
                walletIdentifier.BaseAssetHolderId.Value, expectedAssetGroup);
            
            if (assetPool == null)
            {
                // Create AssetPool with the correct AssetGroup for the AssetType
                assetPool = new AssetPool
                {
                    BaseAssetHolderId = walletIdentifier.BaseAssetHolderId,
                    AssetGroup = expectedAssetGroup,
                };
                assetPool = await _AssetPoolService.Add(assetPool);
            }
        }
        else
        {
            throw new ArgumentException("Either AssetPoolId or (BaseAssetHolderId + AssetType) must be provided");
        }
        
        // Ensure AssetType is valid
        if (!Enum.IsDefined(typeof(AssetType), walletIdentifier.AssetType))
        {
            throw new ArgumentException($"Invalid AssetType: {walletIdentifier.AssetType}");
        }
        
        // Set the AssetPool reference
        walletIdentifier.AssetPoolId = assetPool.Id;
        walletIdentifier.AssetPool = assetPool; // Set navigation property for validation
        
        // Validate the wallet identifier (including AssetType/AssetGroup compatibility)
        var validationResult = _validationService.ValidateWalletIdentifier(walletIdentifier);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => $"{e.Field}: {e.Message}"));
            throw new ArgumentException($"Validation failed: {errors}");
        }
        
        var result = await base.Add(walletIdentifier);
        
        return result;
    }
    

    public async Task<WalletIdentifier> AddWithAssetGroup(WalletIdentifier walletIdentifier, AssetGroup assetGroup)
    {
        if (walletIdentifier.BaseAssetHolderId == null || walletIdentifier.BaseAssetHolderId == Guid.Empty)
        {
            throw new ArgumentException("BaseAssetHolderId is required");
        }

        // get the asset pool by base asset holder and asset group
        var assetPool = await _AssetPoolService.GetByBaseAssetHolderAndType(
                walletIdentifier.BaseAssetHolderId.Value, assetGroup);

        if (assetPool == null)
        {
            // create the asset pool
            assetPool = new AssetPool
            {
                BaseAssetHolderId = walletIdentifier.BaseAssetHolderId,
                AssetGroup = assetGroup,
            };
            assetPool = await _AssetPoolService.Add(assetPool);
        }

        walletIdentifier.AssetPoolId = assetPool.Id;
        walletIdentifier.AssetPool = assetPool;

        return await Add(walletIdentifier);
    }

    public override async Task<WalletIdentifier> Update(Guid id, WalletIdentifier walletIdentifier)
    {
        // Validate before updating
        var validationResult = _validationService.ValidateWalletIdentifier(walletIdentifier);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => $"{e.Field}: {e.Message}"));
            throw new ArgumentException($"Validation failed: {errors}");
        }
        
        return await base.Update(id, walletIdentifier);
    }
    
    public override async Task<WalletIdentifier?> Get(Guid id)
    {
        return await context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
                .ThenInclude(aw => aw.BaseAssetHolder)
            .Include(wi => wi.Referrals)
            .FirstOrDefaultAsync(wi => wi.Id == id && !wi.DeletedAt.HasValue);
    }
    
    public async Task<List<WalletIdentifier>> GetByAssetPoolId(Guid AssetPoolId)
    {
        return await context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
                .ThenInclude(aw => aw.BaseAssetHolder)
            .Include(wi => wi.Referrals)
            .Where(wi => wi.AssetPoolId == AssetPoolId && !wi.DeletedAt.HasValue)
            .ToListAsync();
    }
    
    public async Task<List<WalletIdentifier>> GetByWalletType(AssetGroup assetGroup)
    {
        return await context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
                .ThenInclude(aw => aw.BaseAssetHolder)
            .Include(wi => wi.Referrals)
            .Where(wi => wi.AssetPool.AssetGroup == assetGroup && !wi.DeletedAt.HasValue)
            .ToListAsync();
    }
    
    public async Task<List<WalletIdentifier>> GetByAssetHolder(Guid assetHolderId)
    {
        return await context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
                .ThenInclude(aw => aw.BaseAssetHolder)
            .Include(wi => wi.Referrals)
            .Where(wi => wi.AssetPool.BaseAssetHolderId == assetHolderId && !wi.DeletedAt.HasValue)
            .ToListAsync();
    }

    public async Task<List<WalletIdentifier>> GetByAssetHolderTypeFiltered(string? assetHolderType, AssetType? assetType, AssetGroup? assetGroup)
    {
        var query = context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
            .ThenInclude(ap => ap.BaseAssetHolder)
            .AsQueryable();

        // Filter by asset holder type
        if (!string.IsNullOrEmpty(assetHolderType))
        {
            query = assetHolderType switch
            {
                "Client" => query.Where(wi => wi.AssetPool.BaseAssetHolder != null && wi.AssetPool.BaseAssetHolder.Client != null),
                "Bank" => query.Where(wi => wi.AssetPool.BaseAssetHolder != null && wi.AssetPool.BaseAssetHolder.Bank != null),
                "Member" => query.Where(wi => wi.AssetPool.BaseAssetHolder != null && wi.AssetPool.BaseAssetHolder.Member != null),
                "PokerManager" => query.Where(wi => wi.AssetPool.BaseAssetHolder != null && wi.AssetPool.BaseAssetHolder.PokerManager != null),
                _ => query.Where(wi => false) // No match
            };
        }

        // Filter by asset type
        if (assetType.HasValue)
        {
            query = query.Where(wi => wi.AssetType == assetType.Value);
        }

        // Filter by asset group (query on AssetPool's AssetGroup)
        if (assetGroup.HasValue)
        {
            query = query.Where(wi => wi.AssetPool.AssetGroup == assetGroup.Value);
        }

        return await query.Where(wi => !wi.DeletedAt.HasValue).ToListAsync();
    }

    public async Task<List<WalletIdentifier>> GetByAssetHolderAndFilters(Guid assetHolderId, AssetType? assetType, AssetGroup? assetGroup)
    {
        var query = context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
            .ThenInclude(ap => ap.BaseAssetHolder)
            .Where(wi => wi.AssetPool.BaseAssetHolderId == assetHolderId);

        // Filter by asset type
        if (assetType.HasValue)
        {
            query = query.Where(wi => wi.AssetType == assetType.Value);
        }

        // Filter by asset group (query on AssetPool's AssetGroup)
        if (assetGroup.HasValue)
        {
            query = query.Where(wi => wi.AssetPool.AssetGroup == assetGroup.Value);
        }

        return await query.Where(wi => !wi.DeletedAt.HasValue).ToListAsync();
    }

    public async Task<List<WalletIdentifier>> GetByAssetHolderAndAssetType(Guid assetHolderId, AssetType assetType)
    {
        return await context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
            .ThenInclude(ap => ap.BaseAssetHolder)
            .Where(wi => wi.AssetPool.BaseAssetHolderId == assetHolderId && wi.AssetType == assetType && !wi.DeletedAt.HasValue)
            .ToListAsync();
    }
    
    public async Task<List<WalletIdentifier>> GetByAssetType(AssetType assetType)
    {
        return await context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
            .ThenInclude(ap => ap.BaseAssetHolder)
            .Where(wi => wi.AssetType == assetType && !wi.DeletedAt.HasValue)
            .ToListAsync();
    }
    
    // Metadata search methods
    public async Task<List<WalletIdentifier>> SearchByMetadata(string metadataKey, string metadataValue)
    {
        return await context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
                .ThenInclude(aw => aw.BaseAssetHolder)
            .Include(wi => wi.Referrals)
            .Where(wi => wi.MetadataJson.Contains($"\"{metadataKey}\":\"{metadataValue}\"") && !wi.DeletedAt.HasValue)
            .ToListAsync();
    }
    
    public async Task<List<WalletIdentifier>> GetBankWalletsByBankName(string bankName)
    {
        return await SearchByMetadata(BankWalletMetadata.BankName.ToString(), bankName);
    }
    
    public async Task<List<WalletIdentifier>> GetCryptoWalletsByExchange(string exchangeName)
    {
        return await SearchByMetadata(CryptoWalletMetadata.ExchangeName.ToString(), exchangeName);
    }
    
    public async Task<List<WalletIdentifier>> GetInternalWallets()
    {
        return await context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
                .ThenInclude(aw => aw!.BaseAssetHolder)
            .Include(wi => wi.Referrals)
            .Where(wi => wi.AssetPool!.AssetGroup == AssetGroup.Internal && !wi.DeletedAt.HasValue)
            .ToListAsync();
    }

    public async Task<WalletIdentifier?> GetSystemWalletToPairWith(Guid walletIdentifierId)
    {
        var walletIdentifier = await context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
            .FirstOrDefaultAsync(wi => wi.Id == walletIdentifierId && !wi.DeletedAt.HasValue);

        if (walletIdentifier == null) return null;

        // Internal wallets can only be paired with other internal wallets of the same asset type
        if (walletIdentifier.AssetGroup == AssetGroup.Internal)
        {
            throw new ArgumentException("Wallet identifier is internal");
        }

        // For non-internal wallets, find an internal wallet of the same asset type
        return await context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
            .ThenInclude(ap => ap!.BaseAssetHolder)
            .Where(wi => wi.AssetPool!.AssetGroup == AssetGroup.Internal &&
                        wi.AssetPool!.BaseAssetHolderId == null &&
                        wi.AssetType == walletIdentifier.AssetType &&
                        !wi.DeletedAt.HasValue)
            .OrderBy(wi => wi.CreatedAt)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets conversion wallets for a PokerManager (used in self-conversion transactions)
    /// </summary>
    public async Task<List<WalletIdentifier>> GetConversionWalletsForManager(Guid managerId)
    {
        var isPokerManager = await context.PokerManagers
            .AnyAsync(pm => pm.BaseAssetHolderId == managerId && !pm.DeletedAt.HasValue);

        if (!isPokerManager)
        {
            throw new ArgumentException("Asset holder is not a PokerManager");
        }

        return await context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
                .ThenInclude(ap => ap!.BaseAssetHolder)
            .Include(wi => wi.Referrals)
            .Where(wi => wi.AssetPool!.AssetGroup == AssetGroup.Internal &&
                        wi.AssetPool!.BaseAssetHolderId == managerId &&
                        !wi.DeletedAt.HasValue)
            .OrderBy(wi => wi.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<List<WalletIdentifier>> GetInternalWalletsByMetadata(string metadataKey, string metadataValue)
    {
        return await context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
                .ThenInclude(aw => aw!.BaseAssetHolder)
            .Include(wi => wi.Referrals)
            .Where(wi => wi.AssetPool!.AssetGroup == AssetGroup.Internal && 
                        wi.MetadataJson.Contains($"\"{metadataKey}\":\"{metadataValue}\"") && 
                        !wi.DeletedAt.HasValue)
            .ToListAsync();
    }
    
    // Validation method
    public ValidationResult ValidateWalletIdentifier(WalletIdentifier walletIdentifier)
    {
        return _validationService.ValidateWalletIdentifier(walletIdentifier);
    }
}