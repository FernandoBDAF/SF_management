using SFManagement.Enums;

namespace SFManagement.ViewModels;

/// <summary>
/// Response model for imported transactions
/// </summary>
public class ImportedTransactionResponse : BaseResponse
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public string? ExternalReferenceId { get; set; }
    public Guid BaseAssetHolderId { get; set; }
    public string BaseAssetHolderName { get; set; } = string.Empty;
    public ImportFileType FileType { get; set; }
    public string FileTypeName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string? FileHash { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? FileMetadata { get; set; }
    public ImportedTransactionStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public ReconciledTransactionType? ReconciledTransactionType { get; set; }
    public Guid? ReconciledTransactionId { get; set; }
    public DateTime? ReconciledAt { get; set; }
    public string? ReconciliationNotes { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ProcessingError { get; set; }
    public bool IsReconciled { get; set; }
    public bool IsProcessed { get; set; }
    public bool HasErrors { get; set; }
}

/// <summary>
/// Summary response for import operations
/// </summary>
public class ImportSummaryResponse
{
    public string FileName { get; set; } = string.Empty;
    public ImportFileType FileType { get; set; }
    public string FileTypeName { get; set; } = string.Empty;
    public Guid BaseAssetHolderId { get; set; }
    public string BaseAssetHolderName { get; set; } = string.Empty;
    public int TotalTransactions { get; set; }
    public int ProcessedTransactions { get; set; }
    public int FailedTransactions { get; set; }
    public int DuplicateTransactions { get; set; }
    public int ReconciledTransactions { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime ImportedAt { get; set; }
    public bool Success => FailedTransactions == 0 && ProcessedTransactions > 0;
}

/// <summary>
/// Response for reconciliation operations
/// </summary>
public class ReconciliationResponse
{
    public Guid ImportedTransactionId { get; set; }
    public Guid BaseTransactionId { get; set; }
    public DateTime ReconciledAt { get; set; }
    public string? Notes { get; set; }
    public ImportedTransactionResponse ImportedTransaction { get; set; } = new();
    public BaseTransactionSummary BaseTransaction { get; set; } = new();
}

/// <summary>
/// Summary information about a BaseTransaction for reconciliation
/// </summary>
public class BaseTransactionSummary
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public decimal AssetAmount { get; set; }
    public string? Description { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public AssetType AssetType { get; set; }
    public string AssetTypeName { get; set; } = string.Empty;
}

/// <summary>
/// Response for potential match finding
/// </summary>
public class PotentialMatchesResponse
{
    public ImportedTransactionResponse ImportedTransaction { get; set; } = new();
    public List<PotentialMatch> Matches { get; set; } = new();
    public int TotalMatches { get; set; }
}

/// <summary>
/// Information about a potential match for reconciliation
/// </summary>
public class PotentialMatch
{
    public ReconciledTransactionType TransactionType { get; set; }
    public Guid TransactionId { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public double MatchScore { get; set; }
    public int DaysDifference { get; set; }
    public decimal AmountDifference { get; set; }
    public List<string> MatchReasons { get; set; } = new();
}

/// <summary>
/// Response for file-based transaction grouping
/// </summary>
public class FileTransactionsResponse
{
    public string FileName { get; set; } = string.Empty;
    public ImportFileType FileType { get; set; }
    public string FileTypeName { get; set; } = string.Empty;
    public string? FileHash { get; set; }
    public long? FileSizeBytes { get; set; }
    public Guid BaseAssetHolderId { get; set; }
    public string BaseAssetHolderName { get; set; } = string.Empty;
    public List<ImportedTransactionResponse> Transactions { get; set; } = new();
    public int TotalTransactions { get; set; }
    public int ReconciledTransactions { get; set; }
    public int PendingTransactions { get; set; }
    public int FailedTransactions { get; set; }
    public DateTime? FirstTransactionDate { get; set; }
    public DateTime? LastTransactionDate { get; set; }
    public decimal TotalAmount { get; set; }
}

/// <summary>
/// Dashboard summary for imported transactions
/// </summary>
public class ImportedTransactionDashboard
{
    public Guid BaseAssetHolderId { get; set; }
    public string BaseAssetHolderName { get; set; } = string.Empty;
    public int TotalImportedTransactions { get; set; }
    public int PendingReconciliation { get; set; }
    public int ReconciledTransactions { get; set; }
    public int FailedTransactions { get; set; }
    public int FilesImported { get; set; }
    public DateTime? LastImportDate { get; set; }
    public List<FileTypeSummary> FileTypeSummaries { get; set; } = new();
    public List<RecentImport> RecentImports { get; set; } = new();
}

/// <summary>
/// Summary by file type
/// </summary>
public class FileTypeSummary
{
    public ImportFileType FileType { get; set; }
    public string FileTypeName { get; set; } = string.Empty;
    public int TransactionCount { get; set; }
    public int FileCount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime? LastImportDate { get; set; }
}

/// <summary>
/// Recent import summary
/// </summary>
public class RecentImport
{
    public string FileName { get; set; } = string.Empty;
    public ImportFileType FileType { get; set; }
    public int TransactionCount { get; set; }
    public DateTime ImportedAt { get; set; }
    public ImportedTransactionStatus Status { get; set; }
} 