using System.ComponentModel.DataAnnotations;

namespace SFManagement.Domain.Enums.ImportedFiles;

/// <summary>
/// Status of an imported transaction throughout its lifecycle
/// </summary>
public enum ImportedTransactionStatus
{
    [Display(Name = "Pending", Description = "Transaction imported but not yet processed")]
    Pending = 1,
    
    [Display(Name = "Processing", Description = "Transaction is being processed")]
    Processing = 2,
    
    [Display(Name = "Processed", Description = "Transaction processed successfully")]
    Processed = 3,
    
    [Display(Name = "Reconciled", Description = "Transaction reconciled with a BaseTransaction")]
    Reconciled = 4,
    
    [Display(Name = "Failed", Description = "Transaction processing failed")]
    Failed = 5,
    
    [Display(Name = "Duplicate", Description = "Transaction identified as duplicate")]
    Duplicate = 6,
    
    [Display(Name = "Ignored", Description = "Transaction manually ignored")]
    Ignored = 7,
    
    [Display(Name = "Requires Review", Description = "Transaction requires manual review")]
    RequiresReview = 8
} 