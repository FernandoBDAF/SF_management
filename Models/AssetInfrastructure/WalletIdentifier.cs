using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using SFManagement.Data;
using SFManagement.Models.Support;
using SFManagement.Models.Transactions;
using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;

namespace SFManagement.Models.AssetInfrastructure;

public class WalletIdentifier : BaseDomain
{
    [Required] public Guid AssetWalletId { get; set; }
    public virtual AssetWallet AssetWallet { get; set; }

    public WalletType WalletType { get; set; }
    
    [Required, MaxLength(30)]
    public string InputForTransactions { get; set; }
    
    // Store metadata as JSON string in database
    [Column(TypeName = "nvarchar(2000)")]
    public string MetadataJson { get; set; } = "{}";
    
    // Navigation property for code usage - not mapped to database
    [NotMapped]
    public Dictionary<string, string> Metadata
    {
        get => JsonSerializer.Deserialize<Dictionary<string, string>>(MetadataJson) ?? new();
        set => MetadataJson = JsonSerializer.Serialize(value);
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
            BankWalletMetadata.BankName,
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
            PokerWalletMetadata.SiteName,
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
            .ThenInclude(x => x.AssetWallet)
                .ThenInclude(x => x.BaseAssetHolder)
        .Include(x => x.ReceiverWalletIdentifier)
            .ThenInclude(x => x.AssetWallet)
                .ThenInclude(x => x.BaseAssetHolder)
        .ToArray();

        return transactions;
    }
    
    public IEnumerable<FiatAssetTransaction> GetFiatAssetTransactions(DataContext context, bool includeDeleted = false)
    {
        var transactions = context.FiatAssetTransactions
        .Where(x => x.SenderWalletIdentifierId == Id || x.ReceiverWalletIdentifierId == Id && (!x.DeletedAt.HasValue || includeDeleted))
        .Include(x => x.SenderWalletIdentifier)
            .ThenInclude(x => x.AssetWallet)
                .ThenInclude(x => x.BaseAssetHolder)
        .Include(x => x.ReceiverWalletIdentifier)
            .ThenInclude(x => x.AssetWallet)
                .ThenInclude(x => x.BaseAssetHolder)
        .ToArray();

        return transactions;
    }
    
    public IEnumerable<SettlementTransaction> GetSettlementTransactions(DataContext context, bool includeDeleted = false)
    {
        var transactions = context.SettlementTransactions
        .Where(x => x.SenderWalletIdentifierId == Id || x.ReceiverWalletIdentifierId == Id && (!x.DeletedAt.HasValue || includeDeleted))
        .Include(x => x.SenderWalletIdentifier)
            .ThenInclude(x => x.AssetWallet)
                .ThenInclude(x => x.BaseAssetHolder)
        .Include(x => x.ReceiverWalletIdentifier)
            .ThenInclude(x => x.AssetWallet)
                .ThenInclude(x => x.BaseAssetHolder)
        .ToArray();

        return transactions;
    }
}
