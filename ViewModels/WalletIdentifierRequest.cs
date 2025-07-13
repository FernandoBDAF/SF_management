using SFManagement.Enums;

namespace SFManagement.ViewModels;

public class WalletIdentifierRequest
{
    /// <summary>
    /// SCENARIO 1: Provide AssetPoolId to use an existing AssetPool
    /// SCENARIO 2: Provide BaseAssetHolderId + AssetType to find/create AssetPool automatically
    /// </summary>
    public Guid? AssetPoolId { get; set; }
    
    /// <summary>
    /// SCENARIO 2: BaseAssetHolder ID - used with AssetType to find/create appropriate AssetPool
    /// </summary>
    public Guid? BaseAssetHolderId { get; set; }
    
    /// <summary>
    /// REQUIRED: The specific asset type (PokerStars, Bitcoin, BrazilianReal, etc.)
    /// This determines which AssetGroup the AssetPool should have
    /// </summary>
    public AssetType AssetType { get; set; }
    
    public AccountClassification AccountClassification { get; set; }

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
    
    // JSON metadata (alternative to individual fields)
    public string? MetadataJson { get; set; }
}