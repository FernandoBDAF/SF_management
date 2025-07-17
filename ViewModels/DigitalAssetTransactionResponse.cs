using SFManagement.Enums;
using SFManagement.Models.Transactions;
using SFManagement.Enums.AssetInfrastructure;

namespace SFManagement.ViewModels;

public class DigitalAssetTransactionResponse : BaseTransactionResponse
{
    /// <summary>
    /// Asset type to convert to (for conversion transactions)
    /// </summary>
    public AssetType? ConvertTo { get; set; }
    
    /// <summary>
    /// Conversion rate used (for conversion transactions)
    /// </summary>
    public decimal? ConversionRate { get; set; }
    
    /// <summary>
    /// Exchange rate at time of transaction
    /// </summary>
    public decimal? Rate { get; set; }
    
    /// <summary>
    /// Profit/loss from the transaction
    /// </summary>
    public decimal? Profit { get; set; }
    
    /// <summary>
    /// Excel file information (for imported transactions)
    /// </summary>
    public ExcelTransactionSummary? Excel { get; set; }
    
    /// <summary>
    /// Poker-specific transaction details
    /// </summary>
    public PokerTransactionInfo? PokerInfo { get; set; }
    
    /// <summary>
    /// Crypto-specific transaction details
    /// </summary>
    public CryptoTransactionInfo? CryptoInfo { get; set; }
    
    /// <summary>
    /// Conversion details (if this is a conversion transaction)
    /// </summary>
    public ConversionDetails? ConversionDetails { get; set; }
}

/// <summary>
/// Excel import summary for digital asset transactions
/// </summary>
public class ExcelTransactionSummary
{
    public Guid Id { get; set; }
    public string? FileName { get; set; }
    public string? FileType { get; set; }
    public string? PokerManagerName { get; set; }
    public DateTime ImportedAt { get; set; }
}

/// <summary>
/// Poker-specific transaction information
/// </summary>
public class PokerTransactionInfo
{
    public string? PlayerNickname { get; set; }
    public string? PlayerEmail { get; set; }
    public string? AccountStatus { get; set; }
    public string? PokerSite { get; set; }
    public string? TableInfo { get; set; }
    public string? GameType { get; set; }
}

/// <summary>
/// Crypto-specific transaction information
/// </summary>
public class CryptoTransactionInfo
{
    public string? WalletAddress { get; set; }
    public string? WalletCategory { get; set; }
    public string? NetworkType { get; set; }
    public string? TransactionHash { get; set; }
    public int? Confirmations { get; set; }
    public decimal? NetworkFee { get; set; }
}

/// <summary>
/// Conversion transaction details
/// </summary>
public class ConversionDetails
{
    public AssetType FromAsset { get; set; }
    public AssetType ToAsset { get; set; }
    public decimal FromAmount { get; set; }
    public decimal ToAmount { get; set; }
    public decimal ExchangeRate { get; set; }
    public decimal? Fee { get; set; }
    public string? ExchangeUsed { get; set; }
}