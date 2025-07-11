using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models.AssetInfrastructure;
using SFManagement.Enums;
using SFManagement.Enums.WalletsMetadata;

namespace SFManagement.Services;

public class WalletIdentifierService : BaseService<WalletIdentifier>
{
    private readonly WalletIdentifierValidationService _validationService;
    private readonly AssetPoolService _AssetPoolService;
    
    public WalletIdentifierService(DataContext context, IHttpContextAccessor httpContextAccessor, 
    AssetPoolService AssetPoolService) : base(context, httpContextAccessor)
    {
        _validationService = new WalletIdentifierValidationService();
        _AssetPoolService = AssetPoolService;
    }

    
    public override async Task<WalletIdentifier> Add(WalletIdentifier walletIdentifier)
    {
        if (walletIdentifier.AssetPoolId.Equals(Guid.Empty) || walletIdentifier.AssetPoolId.Equals(null))
        {
            if (walletIdentifier.BaseAssetHolderId.Equals(Guid.Empty) ||
                walletIdentifier.BaseAssetHolderId.Equals(null) ||
                !walletIdentifier.AssetType.HasValue)
            {
                throw new ArgumentException("AssetPoolId or BaseAssetHolderId and AssetType are required");
            }
            
            // get the asset pool
            var assetPool = await _AssetPoolService.
            GetByBaseAssetHolderAndType(walletIdentifier.BaseAssetHolderId!.Value, walletIdentifier.AssetType.Value);
            
            if (assetPool == null)
            {
                // create the asset pool - FIXED: properly handle null BaseAssetHolderId
                assetPool = new AssetPool
                {
                    BaseAssetHolderId = walletIdentifier.BaseAssetHolderId, // Keep null if it's null
                    AssetType = walletIdentifier.AssetType.Value,
                };
                assetPool = await _AssetPoolService.Add(assetPool);
            }

            walletIdentifier.AssetPoolId = assetPool.Id;
        }
        // Validate before adding
        var validationResult = _validationService.ValidateWalletIdentifier(walletIdentifier);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => $"{e.Field}: {e.Message}"));
            throw new ArgumentException($"Validation failed: {errors}");
        }
        
        return await base.Add(walletIdentifier);
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
            .Include(wi => wi.Referral)
            .FirstOrDefaultAsync(wi => wi.Id == id && !wi.DeletedAt.HasValue);
    }
    
    public async Task<List<WalletIdentifier>> GetByAssetPoolId(Guid AssetPoolId)
    {
        return await context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
                .ThenInclude(aw => aw.BaseAssetHolder)
            .Include(wi => wi.Referral)
            .Where(wi => wi.AssetPoolId == AssetPoolId && !wi.DeletedAt.HasValue)
            .ToListAsync();
    }
    
    public async Task<List<WalletIdentifier>> GetByWalletType(WalletType walletType)
    {
        return await context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
                .ThenInclude(aw => aw.BaseAssetHolder)
            .Include(wi => wi.Referral)
            .Where(wi => wi.WalletType == walletType && !wi.DeletedAt.HasValue)
            .ToListAsync();
    }
    
    public async Task<List<WalletIdentifier>> GetByAssetHolder(Guid assetHolderId)
    {
        return await context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
                .ThenInclude(aw => aw.BaseAssetHolder)
            .Include(wi => wi.Referral)
            .Where(wi => wi.AssetPool.BaseAssetHolderId == assetHolderId && !wi.DeletedAt.HasValue)
            .ToListAsync();
    }

    public async Task<List<WalletIdentifier>> GetByAssetHolderTypeFiltered(string? assetHolderType, AssetType? assetType, WalletType? walletType)
    {
        var query = context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
                .ThenInclude(aw => aw.BaseAssetHolder)
            .Include(wi => wi.Referral)
            .Where(wi => !wi.DeletedAt.HasValue);

        // Filter by asset holder type using navigation properties instead of computed property
        if (!string.IsNullOrEmpty(assetHolderType))
        {
            switch (assetHolderType.ToLower())
            {
                case "client":
                    query = query.Where(wi => wi.AssetPool.BaseAssetHolder.Client != null);
                    break;
                case "bank":
                    query = query.Where(wi => wi.AssetPool.BaseAssetHolder.Bank != null);
                    break;
                case "member":
                    query = query.Where(wi => wi.AssetPool.BaseAssetHolder.Member != null);
                    break;
                case "pokermanager":
                    query = query.Where(wi => wi.AssetPool.BaseAssetHolder.PokerManager != null);
                    break;
                default:
                    // If unknown type, return empty result
                    return new List<WalletIdentifier>();
            }
        }

        if (assetType.HasValue)
        {
            query = query.Where(wi => wi.AssetPool.AssetType == assetType.Value);
        }

        if (walletType.HasValue)
        {
            query = query.Where(wi => wi.WalletType == walletType.Value);
        }

        return await query
            .OrderBy(wi => wi.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<WalletIdentifier>> GetByAssetHolderAndFilters(Guid assetHolderId, AssetType? assetType, WalletType? walletType)
    {
        var query = context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
            .Where(wi => !wi.DeletedAt.HasValue && wi.AssetPool.BaseAssetHolderId == assetHolderId);

        if (assetType.HasValue)
        {
            query = query.Where(wi => wi.AssetPool.AssetType == assetType.Value);
        }

        if (walletType.HasValue)
        {
            query = query.Where(wi => wi.WalletType == walletType.Value);
        }

        return await query
            .OrderBy(wi => wi.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<WalletIdentifier>> GetByAssetHolderAndAssetType(Guid assetHolderId, AssetType assetType)
    {
        return await context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
                .ThenInclude(aw => aw.BaseAssetHolder)
            .Include(wi => wi.Referral)
            .Where(wi => wi.AssetPool.BaseAssetHolderId == assetHolderId && wi.AssetPool.AssetType == assetType && !wi.DeletedAt.HasValue)
            .ToListAsync();
    }
    
    public async Task<List<WalletIdentifier>> GetByAssetType(AssetType assetType)
    {
        return await context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
                .ThenInclude(aw => aw.BaseAssetHolder)
            .Include(wi => wi.Referral)
            .Where(wi => wi.AssetPool.AssetType == assetType && !wi.DeletedAt.HasValue)
            .ToListAsync();
    }
    
    // Metadata search methods
    public async Task<List<WalletIdentifier>> SearchByMetadata(string metadataKey, string metadataValue)
    {
        return await context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
                .ThenInclude(aw => aw.BaseAssetHolder)
            .Include(wi => wi.Referral)
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
                .ThenInclude(aw => aw.BaseAssetHolder)
            .Include(wi => wi.Referral)
            .Where(wi => wi.WalletType == WalletType.Internal && !wi.DeletedAt.HasValue)
            .ToListAsync();
    }

    public async Task<WalletIdentifier?> GetInternalWalletToPairWith(Guid walletIdentifierId)
    {
        var walletIdentifier = await Get(walletIdentifierId);
        if (walletIdentifier == null)
        {
            throw new ArgumentException("Wallet identifier not found");
        }

        if (walletIdentifier.WalletType == WalletType.Internal)
        {
            throw new ArgumentException("Wallet identifier is internal");
        }

        return await context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
                .ThenInclude(aw => aw.BaseAssetHolder)
            .Include(wi => wi.Referral)
            .FirstOrDefaultAsync(wi => wi.WalletType == WalletType.Internal && 
            !wi.DeletedAt.HasValue 
            && wi.AssetPool.AssetType == walletIdentifier.AssetPool.AssetType);
    }
    
    public async Task<List<WalletIdentifier>> GetInternalWalletsByMetadata(string metadataKey, string metadataValue)
    {
        return await context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
                .ThenInclude(aw => aw.BaseAssetHolder)
            .Include(wi => wi.Referral)
            .Where(wi => wi.WalletType == WalletType.Internal && 
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