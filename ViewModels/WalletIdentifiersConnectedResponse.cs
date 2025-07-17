using SFManagement.Enums;
using SFManagement.Models.Entities;
using SFManagement.Enums.AssetInfrastructure;

namespace SFManagement.ViewModels;

public class WalletIdentifiersConnectedResponse
{
    public List<WalletIdentifierGroup> AssetTypeGroups { get; set; } = new();
}

public class WalletIdentifierGroup
{
    public AssetType AssetType { get; set; }
    public List<WalletIdentifierWithAssetHolderResponse> WalletIdentifiers { get; set; } = new();
    public int WalletIdentifierCount => WalletIdentifiers.Count;
    public int AssetHolderCount => WalletIdentifiers.Select(wi => wi.BaseAssetHolderId).Distinct().Count();
}

public class WalletIdentifierWithAssetHolderResponse
{
    public Guid Id { get; set; }
    public string InputForTransactions { get; set; } = string.Empty;
    public AssetType AssetType { get; set; }
    public string? RouteInfo { get; set; }
    public string? IdentifierInfo { get; set; }
    
    // Referral information
    public ReferralInfo? Referral { get; set; }
    
    // Most recent settlement transaction
    public SettlementTransactionSimplifiedResponse? LastSettlementTransaction { get; set; }
    
    // BaseAssetHolder information
    public Guid BaseAssetHolderId { get; set; }
    public string BaseAssetHolderName { get; set; } = string.Empty;
    public AssetHolderType AssetHolderType { get; set; }
}

public class ReferralInfo
{
    public Guid Id { get; set; }
    public Guid AssetHolderId { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public DateTime? ActiveUntil { get; set; }
    public decimal? ParentCommission { get; set; }
} 