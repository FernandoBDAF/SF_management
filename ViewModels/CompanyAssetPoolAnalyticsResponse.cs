using SFManagement.Enums;

namespace SFManagement.ViewModels;

/// <summary>
/// Analytics response for company asset pools by period
/// </summary>
public class CompanyAssetPoolAnalyticsResponse
{
    /// <summary>
    /// Period information
    /// </summary>
    public AnalyticsPeriod Period { get; set; } = new();
    
    /// <summary>
    /// Summary metrics for the period
    /// </summary>
    public CompanyAnalyticsSummary Summary { get; set; } = new();
    
    /// <summary>
    /// Detailed data by asset pool
    /// </summary>
    public List<CompanyAssetPoolPeriodData> AssetPoolData { get; set; } = new();
}

/// <summary>
/// Period information for analytics
/// </summary>
public class AnalyticsPeriod
{
    public int Year { get; set; }
    public int? Month { get; set; }
    public string PeriodName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDays { get; set; }
}

/// <summary>
/// Summary metrics for company pools in the period
/// </summary>
public class CompanyAnalyticsSummary
{
    /// <summary>
    /// Total number of company pools with activity
    /// </summary>
    public int ActivePoolsCount { get; set; }
    
    /// <summary>
    /// Total balance at the end of the period
    /// </summary>
    public decimal TotalEndingBalance { get; set; }
    
    /// <summary>
    /// Total balance at the start of the period
    /// </summary>
    public decimal TotalStartingBalance { get; set; }
    
    /// <summary>
    /// Net balance change during the period
    /// </summary>
    public decimal NetBalanceChange { get; set; }
    
    /// <summary>
    /// Total transaction count for the period
    /// </summary>
    public int TotalTransactionCount { get; set; }
    
    /// <summary>
    /// Total transaction volume (sum of all transaction amounts)
    /// </summary>
    public decimal TotalTransactionVolume { get; set; }
    
    /// <summary>
    /// Average transaction amount
    /// </summary>
    public decimal AverageTransactionAmount { get; set; }
    
    /// <summary>
    /// Largest single transaction in the period
    /// </summary>
    public decimal LargestTransaction { get; set; }
    
    /// <summary>
    /// Most active asset group by transaction count
    /// </summary>
    public AssetGroup? MostActiveAssetGroup { get; set; }
}

/// <summary>
/// Detailed analytics data for a specific asset pool
/// </summary>
public class CompanyAssetPoolPeriodData
{
    /// <summary>
    /// Asset pool information
    /// </summary>
    public Guid AssetPoolId { get; set; }
    public AssetGroup AssetGroup { get; set; }
    public string AssetGroupName { get; set; } = string.Empty;
    
    /// <summary>
    /// Balance information
    /// </summary>
    public decimal StartingBalance { get; set; }
    public decimal EndingBalance { get; set; }
    public decimal NetBalanceChange { get; set; }
    
    /// <summary>
    /// Transaction metrics
    /// </summary>
    public int TransactionCount { get; set; }
    public decimal TotalTransactionVolume { get; set; }
    public decimal AverageTransactionAmount { get; set; }
    public decimal LargestTransaction { get; set; }
    
    /// <summary>
    /// Transaction breakdown by type
    /// </summary>
    public TransactionTypeBreakdown TransactionBreakdown { get; set; } = new();
    
    /// <summary>
    /// Wallet identifier count for this pool
    /// </summary>
    public int WalletIdentifierCount { get; set; }
    
    /// <summary>
    /// Detailed transactions (if requested)
    /// </summary>
    public List<CompanyPoolTransactionSummary> Transactions { get; set; } = new();
}

/// <summary>
/// Transaction breakdown by transaction type
/// </summary>
public class TransactionTypeBreakdown
{
    public TransactionTypeSummary FiatTransactions { get; set; } = new();
    public TransactionTypeSummary DigitalTransactions { get; set; } = new();
    public TransactionTypeSummary SettlementTransactions { get; set; } = new();
}

/// <summary>
/// Summary for a specific transaction type
/// </summary>
public class TransactionTypeSummary
{
    public int Count { get; set; }
    public decimal TotalVolume { get; set; }
    public decimal AverageAmount { get; set; }
    public decimal LargestAmount { get; set; }
}

/// <summary>
/// Summary information for a transaction involving company pools
/// </summary>
public class CompanyPoolTransactionSummary
{
    public Guid TransactionId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Direction { get; set; } = string.Empty; // "Incoming" or "Outgoing"
    public string? CounterpartyName { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
} 