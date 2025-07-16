using System.ComponentModel.DataAnnotations;

namespace SFManagement.Enums;

/// <summary>
/// Types of transactions that can be reconciled with ImportedTransactions
/// </summary>
public enum ReconciledTransactionType
{
    [Display(Name = "Fiat Transaction", Description = "Fiat asset transaction")]
    Fiat = 1,
    
    [Display(Name = "Digital Transaction", Description = "Digital asset transaction")]
    Digital = 2,
    
    [Display(Name = "Settlement Transaction", Description = "Settlement transaction")]
    Settlement = 3
} 