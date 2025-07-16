using SFManagement.Enums;
using SFManagement.Models.Transactions;

namespace SFManagement.ViewModels;

public class WalletIdentifierResponse : BaseResponse
{
    public AccountClassification AccountClassification { get; set; }

    public AssetType AssetType { get; set; }

    public AssetGroup AssetGroup { get; set; }

    public Guid AssetPoolId { get; set; }
    
    public Guid BaseAssetHolderId { get; set; }

    // public Guid? ReferralId { get; set; }
    
    public string? BaseAssetHolderName { get; set; }

    public decimal? DefaultParentCommission { get; set; }

    // Metadata fields - these will be used to construct MetadataJson if provided
    public string? InputForTransactions { get; set; }
    public string? PlayerNickname { get; set; }
    public string? PlayerPhone { get; set; }
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