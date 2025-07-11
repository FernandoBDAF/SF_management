using SFManagement.Enums;

namespace SFManagement.ViewModels;

public class WalletIdentifierRequest
{
    // validation: AssetPoolId or BaseAssetHolderId+ AssetType are required
    public Guid? AssetPoolId { get; set; }
    
    public Guid BaseAssetHolderId { get; set; }

    public AssetType? AssetType { get; set; }

    public AccountClassification AccountClassification { get; set; }

    public WalletType WalletType { get; set; }
    
    public decimal? DefaultParentCommission { get; set; }

    public string? MetadataJson { get; set; }
    
    // Metadata fields - these will be used to construct MetadataJson if provided
    public string? InputForTransactions { get; set; }
    public string? PlayerNickname { get; set; }
    public string? PlayerEmail { get; set; }
    public string? AccountStatus { get; set; }
    
    // Bank wallet specific fields
    public string? BankName { get; set; }
    public string? AccountNumber { get; set; }
    public string? RoutingNumber { get; set; }
    public string? PixKey { get; set; }
    public string? AccountType { get; set; }
    
    // Crypto wallet specific fields
    public string? WalletAddress { get; set; }
    public string? WalletCategory { get; set; }
    
}