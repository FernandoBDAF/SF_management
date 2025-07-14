using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using SFManagement.Models.Support;
using SFManagement.Enums;
using SFManagement.Enums.WalletsMetadata;


namespace SFManagement.Models.AssetInfrastructure;

public class WalletIdentifier : BaseDomain
{
    public Guid AssetPoolId { get; set; }
    public virtual AssetPool AssetPool { get; set; }
    
    /// <summary>
    /// Referrals for this WalletIdentifier
    /// Multiple referrals are allowed for the same wallet identifier because
    /// different referrers can refer the same wallet with different commission rates and dates
    /// </summary>
    public virtual ICollection<Referral> Referrals { get; set; } = new HashSet<Referral>();
    
    public AccountClassification AccountClassification { get; set; }

    public AssetType AssetType { get; set; }
    
    // Store metadata as JSON string in database
    [Column(TypeName = "nvarchar(2000)")]
    public string MetadataJson { get; set; } = "{}";
    
    // this is not mapped to the database, but is used to validate the request
    [NotMapped]
    public Guid? BaseAssetHolderId { get; set; }

    // Navigation property for code usage - not mapped to database
    [NotMapped]
    public Dictionary<string, string> Metadata
    {
        get
        {
            try
            {
                if (string.IsNullOrWhiteSpace(MetadataJson) || MetadataJson == "{}")
                    return new Dictionary<string, string>();
                    
                return JsonSerializer.Deserialize<Dictionary<string, string>>(MetadataJson) ?? new Dictionary<string, string>();
            }
            catch (JsonException)
            {
                // If JSON is invalid, return empty dictionary and log the issue
                // This prevents the application from crashing due to malformed JSON
                return new Dictionary<string, string>();
            }
        }
        set => MetadataJson = JsonSerializer.Serialize(value);
    }
    
    // Helper property to determine AssetGroup based on AssetType
    [NotMapped]
    public AssetGroup AssetGroup
    {
        get
        {
            // Use the validation service method for consistency
            return Services.WalletIdentifierValidationService.GetAssetGroupForAssetType(AssetType);
        }
    }
    
    /// <summary>
    /// Validates that this WalletIdentifier's AssetType is compatible with its AssetPool's AssetGroup
    /// </summary>
    public bool IsAssetTypeCompatibleWithAssetPool()
    {
        if (AssetPool == null)
            return false;
            
        return Services.WalletIdentifierValidationService.ValidateAssetTypeCompatibility(AssetType, AssetPool.AssetGroup);
    }
    
    /// <summary>
    /// Helper method to set metadata from individual fields
    /// </summary>
    public void SetMetadataFromFields(string? inputForTransactions = null, 
                                    string? playerNickname = null, 
                                    string? playerEmail = null,
                                    string? accountStatus = null,
                                    string? accountNumber = null,
                                    string? routingNumber = null,
                                    string? walletAddress = null,
                                    string? walletCategory = null,
                                    string? pixKey = null,
                                    string? accountType = null)
    {
        var metadata = new Dictionary<string, string>();
        
        // Add fields based on wallet type
        if (AssetGroup == AssetGroup.PokerAssets)
        {
            if (!string.IsNullOrEmpty(inputForTransactions))
                metadata[PokerWalletMetadata.InputForTransactions.ToString()] = inputForTransactions;
            if (!string.IsNullOrEmpty(playerNickname))
                metadata[PokerWalletMetadata.PlayerNickname.ToString()] = playerNickname;
            if (!string.IsNullOrEmpty(playerEmail))
                metadata[PokerWalletMetadata.PlayerEmail.ToString()] = playerEmail;
            if (!string.IsNullOrEmpty(accountStatus))
                metadata[PokerWalletMetadata.AccountStatus.ToString()] = accountStatus;
        }
        else if (AssetGroup == AssetGroup.FiatAssets)
        {
            if (!string.IsNullOrEmpty(pixKey))
                metadata[BankWalletMetadata.PixKey.ToString()] = pixKey;
            if (!string.IsNullOrEmpty(accountType))
                metadata[BankWalletMetadata.AccountType.ToString()] = accountType;
            if (!string.IsNullOrEmpty(accountNumber))
                metadata[BankWalletMetadata.AccountNumber.ToString()] = accountNumber;
            if (!string.IsNullOrEmpty(routingNumber))
                metadata[BankWalletMetadata.RoutingNumber.ToString()] = routingNumber;
        }
        else if (AssetGroup == AssetGroup.CryptoAssets)
        {
            if (!string.IsNullOrEmpty(walletAddress))
                metadata[CryptoWalletMetadata.WalletAddress.ToString()] = walletAddress;
            if (!string.IsNullOrEmpty(walletCategory))
                metadata[CryptoWalletMetadata.WalletCategory.ToString()] = walletCategory;
        }
        else if (AssetGroup == AssetGroup.Internal)
        {
            // Internal wallets can have optional metadata, but none is required
            // For now, we don't set any specific metadata for internal wallets
        }
        
        Metadata = metadata;
    }
    
    // Validation methods
    public bool ValidateMetadata()
    {
        return AssetGroup switch
        {
            AssetGroup.FiatAssets => ValidateBankWalletMetadata(),
            AssetGroup.PokerAssets => ValidatePokerWalletMetadata(),
            AssetGroup.CryptoAssets => ValidateCryptoWalletMetadata(),
            AssetGroup.Internal => true, // Internal wallets require no metadata validation
            _ => false
        };
    }
    
    private bool ValidateBankWalletMetadata()
    {
        var requiredFields = new[]
        {
            BankWalletMetadata.PixKey,
            // BankWalletMetadata.AccountType,
            // BankWalletMetadata.AccountNumber,
            // BankWalletMetadata.RoutingNumber
        };
        
        return requiredFields.All(field => 
            Metadata.ContainsKey(field.ToString()) && 
            !string.IsNullOrEmpty(Metadata[field.ToString()]));
    }
    
    private bool ValidatePokerWalletMetadata()
    {
        var requiredFields = new[]
        {
            PokerWalletMetadata.InputForTransactions
        };
        
        return requiredFields.All(field => 
            Metadata.ContainsKey(field.ToString()) && 
            !string.IsNullOrEmpty(Metadata[field.ToString()]));
    }
    
    private bool ValidateCryptoWalletMetadata()
    {
        var requiredFields = new[]
        {
            CryptoWalletMetadata.WalletAddress,
            CryptoWalletMetadata.WalletCategory
        };
        
        return requiredFields.All(field => 
            Metadata.ContainsKey(field.ToString()) && 
            !string.IsNullOrEmpty(Metadata[field.ToString()]));
    }
    
    // Type-safe metadata accessors
    public string? GetBankMetadata(BankWalletMetadata field) => 
        AssetGroup == AssetGroup.FiatAssets ? GetMetadataValue(field.ToString()) : null;
    
    public string? GetPokerMetadata(PokerWalletMetadata field) => 
        AssetGroup == AssetGroup.PokerAssets ? GetMetadataValue(field.ToString()) : null;
    
    public string? GetCryptoMetadata(CryptoWalletMetadata field) => 
        AssetGroup == AssetGroup.CryptoAssets ? GetMetadataValue(field.ToString()) : null;
    
    public string? GetInternalMetadata(string field) => 
        AssetGroup == AssetGroup.Internal ? GetMetadataValue(field) : null;
    
    public void SetBankMetadata(BankWalletMetadata field, string value)
    {
        if (AssetGroup == AssetGroup.FiatAssets)
            SetMetadataValue(field.ToString(), value);
    }
    
    public void SetPokerMetadata(PokerWalletMetadata field, string value)
    {
        if (AssetGroup == AssetGroup.PokerAssets)
            SetMetadataValue(field.ToString(), value);
    }
    
    public void SetCryptoMetadata(CryptoWalletMetadata field, string value)
    {
        if (AssetGroup == AssetGroup.CryptoAssets)
            SetMetadataValue(field.ToString(), value);
    }
    
    public void SetInternalMetadata(string field, string value)
    {
        if (AssetGroup == AssetGroup.Internal)
            SetMetadataValue(field, value);
    }
    
    private string? GetMetadataValue(string key) => 
        Metadata.TryGetValue(key, out var value) ? value : null;
    
    private void SetMetadataValue(string key, string value) =>
        Metadata[key] = value;
}
