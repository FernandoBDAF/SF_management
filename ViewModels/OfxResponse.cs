using SFManagement.Models.Transactions;

namespace SFManagement.ViewModels;

/// <summary>
/// Response model for OFX file information with associated transactions
/// </summary>
public class OfxResponse : BaseResponse
{
    /// <summary>
    /// Associated bank information
    /// </summary>
    public BankSummary Bank { get; set; } = new();

    /// <summary>
    /// Original OFX file name
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// File processing statistics
    /// </summary>
    public OfxFileStatistics Statistics { get; set; } = new();

    /// <summary>
    /// List of transactions from this OFX file
    /// </summary>
    public List<OfxTransactionResponse> OfxTransactions { get; set; } = new();

    /// <summary>
    /// File import context
    /// </summary>
    public OfxImportInfo? ImportInfo { get; set; }
}

/// <summary>
/// Bank summary information for OFX responses
/// </summary>
public class BankSummary
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Email { get; set; }
}

/// <summary>
/// OFX file processing statistics
/// </summary>
public class OfxFileStatistics
{
    /// <summary>
    /// Total number of transactions in the file
    /// </summary>
    public int TotalTransactions { get; set; }

    /// <summary>
    /// Number of transactions that were successfully processed
    /// </summary>
    public int ProcessedTransactions { get; set; }

    /// <summary>
    /// Number of transactions that were skipped (duplicates)
    /// </summary>
    public int SkippedTransactions { get; set; }

    /// <summary>
    /// Total value of all transactions (sum of absolute values)
    /// </summary>
    public decimal TotalValue { get; set; }

    /// <summary>
    /// Sum of all positive transactions (credits)
    /// </summary>
    public decimal TotalCredits { get; set; }

    /// <summary>
    /// Sum of all negative transactions (debits)
    /// </summary>
    public decimal TotalDebits { get; set; }

    /// <summary>
    /// Date range of transactions in the file
    /// </summary>
    public DateTime? EarliestTransactionDate { get; set; }
    public DateTime? LatestTransactionDate { get; set; }
}

/// <summary>
/// OFX file import context and metadata
/// </summary>
public class OfxImportInfo
{
    /// <summary>
    /// When the file was imported
    /// </summary>
    public DateTime ImportedAt { get; set; }

    /// <summary>
    /// Who imported the file
    /// </summary>
    public Guid ImportedBy { get; set; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long? FileSizeBytes { get; set; }

    /// <summary>
    /// Import processing status
    /// </summary>
    public string? ProcessingStatus { get; set; }

    /// <summary>
    /// Any import warnings or notes
    /// </summary>
    public List<string> ImportWarnings { get; set; } = new();
}