using SFManagement.Enums;

namespace SFManagement.ViewModels;

/// <summary>
/// Response model for individual OFX transaction information
/// </summary>
public class OfxTransactionResponse : BaseResponse
{
    /// <summary>
    /// Transaction date from OFX file
    /// </summary>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Transaction amount (can be positive or negative)
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// Transaction description/memo from OFX
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Unique transaction identifier from OFX file
    /// </summary>
    public string FitId { get; set; } = string.Empty;

    /// <summary>
    /// Associated OFX file information
    /// </summary>
    public OfxFileSummary OfxFile { get; set; } = new();

    /// <summary>
    /// Bank information for this transaction
    /// </summary>
    public BankSummary Bank { get; set; } = new();

    /// <summary>
    /// Related fiat asset transaction (if linked)
    /// </summary>
    public FiatAssetTransactionSummary? LinkedTransaction { get; set; }

    /// <summary>
    /// Transaction classification information
    /// </summary>
    public OfxTransactionClassification Classification { get; set; } = new();

    /// <summary>
    /// Transaction direction (Income/Expense based on value)
    /// </summary>
    public TransactionDirection Direction => Value >= 0 ? TransactionDirection.Income : TransactionDirection.Expense;

    /// <summary>
    /// Absolute value of the transaction
    /// </summary>
    public decimal AbsoluteValue => Math.Abs(Value);
}

/// <summary>
/// OFX file summary for transaction responses
/// </summary>
public class OfxFileSummary
{
    public Guid Id { get; set; }
    public string? FileName { get; set; }
    public DateTime ImportedAt { get; set; }
}

/// <summary>
/// Summary of linked fiat asset transaction
/// </summary>
public class FiatAssetTransactionSummary
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public decimal AssetAmount { get; set; }
    public string? Description { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public bool IsInternalTransfer { get; set; }
    public string? CategoryName { get; set; }
}

/// <summary>
/// Transaction classification and metadata
/// </summary>
public class OfxTransactionClassification
{
    /// <summary>
    /// Whether this transaction has been processed/linked
    /// </summary>
    public bool IsProcessed { get; set; }

    /// <summary>
    /// Detected transaction type based on description
    /// </summary>
    public string? DetectedType { get; set; }

    /// <summary>
    /// Confidence level of the classification (0-100)
    /// </summary>
    public int? ClassificationConfidence { get; set; }

    /// <summary>
    /// Suggested category based on description analysis
    /// </summary>
    public string? SuggestedCategory { get; set; }

    /// <summary>
    /// Whether this appears to be a PIX transaction
    /// </summary>
    public bool IsPotentialPix { get; set; }

    /// <summary>
    /// Whether this appears to be a recurring transaction
    /// </summary>
    public bool IsPotentialRecurring { get; set; }

    /// <summary>
    /// Additional processing notes
    /// </summary>
    public List<string> ProcessingNotes { get; set; } = new();
}