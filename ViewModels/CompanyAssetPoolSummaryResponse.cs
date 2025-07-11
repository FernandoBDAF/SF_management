using SFManagement.Enums;

namespace SFManagement.ViewModels;

/// <summary>
/// Summary response model for company asset pools overview
/// </summary>
public class CompanyAssetPoolSummaryResponse
{
    /// <summary>
    /// Total number of company asset pools
    /// </summary>
    public int TotalPools { get; set; }
    
    /// <summary>
    /// Total balance across all company pools
    /// </summary>
    public decimal TotalBalance { get; set; }
    
    /// <summary>
    /// Breakdown by asset type
    /// </summary>
    public List<CompanyAssetTypeBalance> AssetTypeBalances { get; set; } = new();
    
    /// <summary>
    /// Recent activity summary
    /// </summary>
    public CompanyPoolActivity RecentActivity { get; set; } = new();
}

/// <summary>
/// Balance information for a specific asset type
/// </summary>
public class CompanyAssetTypeBalance
{
    public AssetType AssetType { get; set; }
    public string AssetTypeName { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public int WalletIdentifierCount { get; set; }
    public int TransactionCount { get; set; }
    public DateTime? LastTransactionDate { get; set; }
}

/// <summary>
/// Recent activity information for company pools
/// </summary>
public class CompanyPoolActivity
{
    /// <summary>
    /// Number of transactions in the last 30 days
    /// </summary>
    public int TransactionsLast30Days { get; set; }
    
    /// <summary>
    /// Net balance change in the last 30 days
    /// </summary>
    public decimal BalanceChangeLast30Days { get; set; }
    
    /// <summary>
    /// Most active asset type by transaction count
    /// </summary>
    public AssetType? MostActiveAssetType { get; set; }
    
    /// <summary>
    /// Largest single transaction amount in the last 30 days
    /// </summary>
    public decimal LargestTransactionAmount { get; set; }
} 