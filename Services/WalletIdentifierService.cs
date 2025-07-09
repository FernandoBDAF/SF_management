using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models.AssetInfrastructure;
using SFManagement.Enums;

namespace SFManagement.Services;

public class WalletIdentifierService : BaseService<WalletIdentifier>
{
    private readonly WalletIdentifierValidationService _validationService;
    private readonly AssetWalletService _assetWalletService;
    
    public WalletIdentifierService(DataContext context, IHttpContextAccessor httpContextAccessor, 
    AssetWalletService assetWalletService) : base(context, httpContextAccessor)
    {
        _validationService = new WalletIdentifierValidationService();
        _assetWalletService = assetWalletService;
    }

    
    public override async Task<WalletIdentifier> Add(WalletIdentifier walletIdentifier)
    {
        if (walletIdentifier.AssetWalletId.Equals(Guid.Empty))
        {
            if (walletIdentifier.BaseAssetHolderId.Equals(Guid.Empty) || !walletIdentifier.AssetType.HasValue)
            {
                throw new ArgumentException("AssetWalletId or BaseAssetHolderId and AssetType are required");
            }
            // get the asset wallet
            var assetWallet = await _assetWalletService.
            GetByBaseAssetHolderAndType(walletIdentifier.BaseAssetHolderId!.Value, walletIdentifier.AssetType.Value);
            
            if (assetWallet == null)
            {
                // create the asset wallet
                assetWallet = new AssetWallet
                {
                    BaseAssetHolderId = walletIdentifier.BaseAssetHolderId ?? Guid.Empty,
                    AssetType = walletIdentifier.AssetType.Value,
                };
                assetWallet = await _assetWalletService.Add(assetWallet);
            }

            walletIdentifier.AssetWalletId = assetWallet.Id;
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
            .Include(wi => wi.AssetWallet)
                .ThenInclude(aw => aw.BaseAssetHolder)
            .Include(wi => wi.Referral)
            .FirstOrDefaultAsync(wi => wi.Id == id && !wi.DeletedAt.HasValue);
    }
    
    public async Task<List<WalletIdentifier>> GetByAssetWalletId(Guid assetWalletId)
    {
        return await context.WalletIdentifiers
            .Include(wi => wi.AssetWallet)
                .ThenInclude(aw => aw.BaseAssetHolder)
            .Include(wi => wi.Referral)
            .Where(wi => wi.AssetWalletId == assetWalletId && !wi.DeletedAt.HasValue)
            .ToListAsync();
    }
    
    public async Task<List<WalletIdentifier>> GetByWalletType(WalletType walletType)
    {
        return await context.WalletIdentifiers
            .Include(wi => wi.AssetWallet)
                .ThenInclude(aw => aw.BaseAssetHolder)
            .Include(wi => wi.Referral)
            .Where(wi => wi.WalletType == walletType && !wi.DeletedAt.HasValue)
            .ToListAsync();
    }
    
    public async Task<List<WalletIdentifier>> GetByAssetHolder(Guid assetHolderId)
    {
        return await context.WalletIdentifiers
            .Include(wi => wi.AssetWallet)
                .ThenInclude(aw => aw.BaseAssetHolder)
            .Include(wi => wi.Referral)
            .Where(wi => wi.AssetWallet.BaseAssetHolderId == assetHolderId && !wi.DeletedAt.HasValue)
            .ToListAsync();
    }

    public async Task<List<WalletIdentifier>> GetByAssetHolderTypeFiltered(string? assetHolderType, AssetType? assetType, WalletType? walletType)
    {
        var query = context.WalletIdentifiers
            .Include(wi => wi.AssetWallet)
                .ThenInclude(aw => aw.BaseAssetHolder)
            .Include(wi => wi.Referral)
            .Where(wi => !wi.DeletedAt.HasValue);

        // Filter by asset holder type using navigation properties instead of computed property
        if (!string.IsNullOrEmpty(assetHolderType))
        {
            switch (assetHolderType.ToLower())
            {
                case "client":
                    query = query.Where(wi => wi.AssetWallet.BaseAssetHolder.Client != null);
                    break;
                case "bank":
                    query = query.Where(wi => wi.AssetWallet.BaseAssetHolder.Bank != null);
                    break;
                case "member":
                    query = query.Where(wi => wi.AssetWallet.BaseAssetHolder.Member != null);
                    break;
                case "pokermanager":
                    query = query.Where(wi => wi.AssetWallet.BaseAssetHolder.PokerManager != null);
                    break;
                default:
                    // If unknown type, return empty result
                    return new List<WalletIdentifier>();
            }
        }

        if (assetType.HasValue)
        {
            query = query.Where(wi => wi.AssetWallet.AssetType == assetType.Value);
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
            .Include(wi => wi.AssetWallet)
            .Where(wi => !wi.DeletedAt.HasValue && wi.AssetWallet.BaseAssetHolderId == assetHolderId);

        if (assetType.HasValue)
        {
            query = query.Where(wi => wi.AssetWallet.AssetType == assetType.Value);
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
            .Include(wi => wi.AssetWallet)
                .ThenInclude(aw => aw.BaseAssetHolder)
            .Include(wi => wi.Referral)
            .Where(wi => wi.AssetWallet.BaseAssetHolderId == assetHolderId && wi.AssetWallet.AssetType == assetType && !wi.DeletedAt.HasValue)
            .ToListAsync();
    }
    
    public async Task<List<WalletIdentifier>> GetByAssetType(AssetType assetType)
    {
        return await context.WalletIdentifiers
            .Include(wi => wi.AssetWallet)
                .ThenInclude(aw => aw.BaseAssetHolder)
            .Include(wi => wi.Referral)
            .Where(wi => wi.AssetWallet.AssetType == assetType && !wi.DeletedAt.HasValue)
            .ToListAsync();
    }
    
    // Metadata search methods
    public async Task<List<WalletIdentifier>> SearchByMetadata(string metadataKey, string metadataValue)
    {
        return await context.WalletIdentifiers
            .Include(wi => wi.AssetWallet)
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
    
    // Validation method
    public ValidationResult ValidateWalletIdentifier(WalletIdentifier walletIdentifier)
    {
        return _validationService.ValidateWalletIdentifier(walletIdentifier);
    }
}