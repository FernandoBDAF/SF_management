using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using SFManagement.Data;
using SFManagement.Models.Support;
using SFManagement.Models.Transactions;
using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;
using SFManagement.Enums.WalletsMetadata;


namespace SFManagement.Models.AssetInfrastructure;

public class WalletIdentifier : BaseDomain
{
    public Guid? AssetPoolId { get; set; }
    public virtual AssetPool? AssetPool { get; set; }

    // this is not mapped to the database, but is used to validate the request
    [NotMapped]
    public Guid? BaseAssetHolderId { get; set; }
    
    public AccountClassification AccountClassification { get; set; }

    public WalletType WalletType { get; set; }

    // this is not mapped to the database, but is used to validate the request
    [NotMapped]
    public AssetType? AssetType { get; set; }
    
    // Store metadata as JSON string in database
    [Column(TypeName = "nvarchar(2000)")]
    public string MetadataJson { get; set; } = "{}";
    
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
        if (WalletType == WalletType.PokerWallet)
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
        else if (WalletType == WalletType.BankWallet)
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
        else if (WalletType == WalletType.CryptoWallet)
        {
            if (!string.IsNullOrEmpty(walletAddress))
                metadata[CryptoWalletMetadata.WalletAddress.ToString()] = walletAddress;
            if (!string.IsNullOrEmpty(walletCategory))
                metadata[CryptoWalletMetadata.WalletCategory.ToString()] = walletCategory;
        }
        
        Metadata = metadata;
    }
    
    public virtual Referral? Referral { get; set; }
    
    // Validation methods
    public bool ValidateMetadata()
    {
        return WalletType switch
        {
            WalletType.BankWallet => ValidateBankWalletMetadata(),
            WalletType.PokerWallet => ValidatePokerWalletMetadata(),
            WalletType.CryptoWallet => ValidateCryptoWalletMetadata(),
            _ => false
        };
    }
    
    private bool ValidateBankWalletMetadata()
    {
        var requiredFields = new[]
        {
            BankWalletMetadata.PixKey,
            BankWalletMetadata.AccountType,
            BankWalletMetadata.AccountNumber,
            BankWalletMetadata.RoutingNumber
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
        WalletType == WalletType.BankWallet ? GetMetadataValue(field.ToString()) : null;
    
    public string? GetPokerMetadata(PokerWalletMetadata field) => 
        WalletType == WalletType.PokerWallet ? GetMetadataValue(field.ToString()) : null;
    
    public string? GetCryptoMetadata(CryptoWalletMetadata field) => 
        WalletType == WalletType.CryptoWallet ? GetMetadataValue(field.ToString()) : null;
    
    public void SetBankMetadata(BankWalletMetadata field, string value)
    {
        if (WalletType == WalletType.BankWallet)
            SetMetadataValue(field.ToString(), value);
    }
    
    public void SetPokerMetadata(PokerWalletMetadata field, string value)
    {
        if (WalletType == WalletType.PokerWallet)
            SetMetadataValue(field.ToString(), value);
    }
    
    public void SetCryptoMetadata(CryptoWalletMetadata field, string value)
    {
        if (WalletType == WalletType.CryptoWallet)
            SetMetadataValue(field.ToString(), value);
    }
    
    private string? GetMetadataValue(string key) => 
        Metadata.TryGetValue(key, out var value) ? value : null;
    
    private void SetMetadataValue(string key, string value) =>
        Metadata[key] = value;

    // Transaction query methods
    public IEnumerable<DigitalAssetTransaction> GetDigitalAssetTransactions(DataContext context, bool includeDeleted = false)
    {
        var transactions = context.DigitalAssetTransactions
        .Where(x => x.SenderWalletIdentifierId == Id || x.ReceiverWalletIdentifierId == Id && (!x.DeletedAt.HasValue || includeDeleted))
        .Include(x => x.SenderWalletIdentifier)
            .ThenInclude(x => x.AssetPool)
                .ThenInclude(x => x.BaseAssetHolder)
        .Include(x => x.ReceiverWalletIdentifier)
            .ThenInclude(x => x.AssetPool)
                .ThenInclude(x => x.BaseAssetHolder)
        .ToArray();

        return transactions;
    }
    
    public IEnumerable<FiatAssetTransaction> GetFiatAssetTransactions(DataContext context, bool includeDeleted = false)
    {
        var transactions = context.FiatAssetTransactions
        .Where(x => x.SenderWalletIdentifierId == Id || x.ReceiverWalletIdentifierId == Id && (!x.DeletedAt.HasValue || includeDeleted))
        .Include(x => x.SenderWalletIdentifier)
            .ThenInclude(x => x.AssetPool)
                .ThenInclude(x => x.BaseAssetHolder)
        .Include(x => x.ReceiverWalletIdentifier)
            .ThenInclude(x => x.AssetPool)
                .ThenInclude(x => x.BaseAssetHolder)
        .ToArray();

        return transactions;
    }
    
    public IEnumerable<SettlementTransaction> GetSettlementTransactions(DataContext context, bool includeDeleted = false)
    {
        var transactions = context.SettlementTransactions
        .Where(x => x.SenderWalletIdentifierId == Id || x.ReceiverWalletIdentifierId == Id && (!x.DeletedAt.HasValue || includeDeleted))
        .Include(x => x.SenderWalletIdentifier)
            .ThenInclude(x => x.AssetPool)
                .ThenInclude(x => x.BaseAssetHolder)
        .Include(x => x.ReceiverWalletIdentifier)
            .ThenInclude(x => x.AssetPool)
                .ThenInclude(x => x.BaseAssetHolder)
        .ToArray();

        return transactions;
    }
}
