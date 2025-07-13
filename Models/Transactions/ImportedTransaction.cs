using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;
using SFManagement.Models.Entities;

namespace SFManagement.Models.Transactions;

/// <summary>
/// Represents a transaction imported from external files (OFX, Excel, CSV, etc.)
/// This replaces the legacy Excel/Ofx transaction models with a unified approach
/// </summary>
public class ImportedTransaction : BaseDomain
{
    /// <summary>
    /// Date of the transaction as recorded in the imported file
    /// </summary>
    [Required] 
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Amount/value of the transaction
    /// </summary>
    [Required] 
    [Precision(18, 2)] 
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Description/memo of the transaction from the imported file
    /// </summary>
    [MaxLength(500)] 
    public string? Description { get; set; }
    
    /// <summary>
    /// External reference ID from the source file (e.g., FitId for OFX, row identifier for Excel)
    /// Used for duplicate detection and reconciliation
    /// </summary>
    [MaxLength(100)] 
    public string? ExternalReferenceId { get; set; }
    
    /// <summary>
    /// The BaseAssetHolder who owns this imported transaction
    /// </summary>
    [Required] 
    public Guid BaseAssetHolderId { get; set; }
    public virtual BaseAssetHolder BaseAssetHolder { get; set; }
    
    /// <summary>
    /// Type of file this transaction was imported from
    /// </summary>
    [Required] 
    public ImportFileType FileType { get; set; }
    
    /// <summary>
    /// Name of the imported file
    /// </summary>
    [Required] 
    [MaxLength(255)] 
    public string FileName { get; set; }
    
    /// <summary>
    /// Hash of the file content for integrity verification
    /// </summary>
    [MaxLength(64)] 
    public string? FileHash { get; set; }
    
    /// <summary>
    /// Size of the imported file in bytes
    /// </summary>
    public long? FileSizeBytes { get; set; }
    
    /// <summary>
    /// Additional metadata from the imported file stored as JSON
    /// Can contain file-specific data like wallet names, nicknames, etc.
    /// </summary>
    public string? FileMetadata { get; set; }
    
    /// <summary>
    /// Status of this imported transaction
    /// </summary>
    [Required] 
    public ImportedTransactionStatus Status { get; set; } = ImportedTransactionStatus.Pending;
    
    /// <summary>
    /// The BaseTransaction that this imported transaction has been reconciled with
    /// When set, this imported transaction is considered validated by external source
    /// </summary>
    public Guid? ReconciledTransactionId { get; set; }
    public virtual BaseTransaction? ReconciledTransaction { get; set; }
    
    /// <summary>
    /// Date when this transaction was reconciled with a BaseTransaction
    /// </summary>
    public DateTime? ReconciledAt { get; set; }
    
    /// <summary>
    /// Notes about the reconciliation process
    /// </summary>
    [MaxLength(500)] 
    public string? ReconciliationNotes { get; set; }
    
    /// <summary>
    /// Date when this transaction was processed/approved
    /// </summary>
    public DateTime? ProcessedAt { get; set; }
    
    /// <summary>
    /// Error message if processing failed
    /// </summary>
    [MaxLength(1000)] 
    public string? ProcessingError { get; set; }
    
    /// <summary>
    /// Checks if this transaction is reconciled with a BaseTransaction
    /// </summary>
    public bool IsReconciled => ReconciledTransactionId.HasValue && ReconciledAt.HasValue;
    
    /// <summary>
    /// Checks if this transaction has been processed successfully
    /// </summary>
    public bool IsProcessed => Status == ImportedTransactionStatus.Processed && ProcessedAt.HasValue;
    
    /// <summary>
    /// Checks if this transaction has processing errors
    /// </summary>
    public bool HasErrors => Status == ImportedTransactionStatus.Failed && !string.IsNullOrEmpty(ProcessingError);
} 