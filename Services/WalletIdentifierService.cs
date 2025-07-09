using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models.AssetInfrastructure;
using SFManagement.Enums;

namespace SFManagement.Services;

public class WalletIdentifierService : BaseService<WalletIdentifier>
{
    private readonly WalletIdentifierValidationService _validationService;
    
    public WalletIdentifierService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context, httpContextAccessor)
    {
        _validationService = new WalletIdentifierValidationService();
    }
    
    public override async Task<WalletIdentifier> Add(WalletIdentifier walletIdentifier)
    {
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
    
    public async Task<List<WalletIdentifier>> GetPokerWalletsBySite(string siteName)
    {
        return await SearchByMetadata(PokerWalletMetadata.SiteName.ToString(), siteName);
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