using SFManagement.Application.DTOs.Common;
using System.ComponentModel.DataAnnotations;
using SFManagement.Domain.Enums;
using SFManagement.Domain.Enums.ImportedFiles;

namespace SFManagement.Application.DTOs.ImportedTransactions;

/// <summary>
/// Base request model for importing transactions
/// </summary>
public class ImportTransactionRequest
{
    /// <summary>
    /// The BaseAssetHolder who owns these transactions
    /// </summary>
    [Required]
    public Guid BaseAssetHolderId { get; set; }
    
    /// <summary>
    /// The file to import
    /// </summary>
    [Required]
    public required IFormFile File { get; set; }
}

/// <summary>
/// Request model for importing OFX files (Bank transactions)
/// </summary>
public class ImportOfxRequest : ImportTransactionRequest
{
    // OFX files are typically straightforward - no additional parameters needed
}

/// <summary>
/// Request model for importing Excel files (Poker transactions)
/// </summary>
public class ImportExcelRequest : ImportTransactionRequest
{
    /// <summary>
    /// Type of Excel import (Buy, Sell, Transfer)
    /// </summary>
    [Required]
    public ExcelImportType ImportType { get; set; }
    
    /// <summary>
    /// Column mapping for Excel parsing
    /// If not provided, default mapping will be used
    /// </summary>
    public List<ColumnMapping>? ColumnMapping { get; set; }
}

/// <summary>
/// Request model for reconciling an imported transaction with a BaseTransaction
/// </summary>
public class ReconcileTransactionRequest
{
    /// <summary>
    /// The ImportedTransaction to reconcile
    /// </summary>
    [Required]
    public Guid ImportedTransactionId { get; set; }
    
    /// <summary>
    /// The BaseTransaction to reconcile with
    /// </summary>
    [Required]
    public Guid BaseTransactionId { get; set; }
    
    /// <summary>
    /// Type of transaction (Fiat, Digital, Settlement)
    /// </summary>
    [Required]
    public ReconciledTransactionType TransactionType { get; set; }
    
    /// <summary>
    /// Optional notes about the reconciliation
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// Column mapping for Excel file parsing
/// </summary>
public class ColumnMapping
{
    /// <summary>
    /// Column number (1-based)
    /// </summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int Column { get; set; }
    
    /// <summary>
    /// Field name (e.g., "Date", "Amount", "Description")
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Request for finding potential matches for reconciliation
/// </summary>
public class FindMatchesRequest
{
    /// <summary>
    /// The ImportedTransaction to find matches for
    /// </summary>
    [Required]
    public Guid ImportedTransactionId { get; set; }
    
    /// <summary>
    /// Number of days tolerance for date matching (default: 3)
    /// </summary>
    [Range(0, 30)]
    public int DaysTolerance { get; set; } = 3;
    
    /// <summary>
    /// Amount tolerance for matching (default: 0.01)
    /// </summary>
    [Range(0, 1000)]
    public decimal AmountTolerance { get; set; } = 0.01m;
} 