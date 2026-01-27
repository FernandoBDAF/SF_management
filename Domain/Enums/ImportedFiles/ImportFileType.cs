using System.ComponentModel.DataAnnotations;

namespace SFManagement.Domain.Enums.ImportedFiles;

/// <summary>
/// Types of files that can be imported to create transactions
/// </summary>
public enum ImportFileType
{
    [Display(Name = "OFX File", Description = "Open Financial Exchange file format")]
    Ofx = 1,
    
    [Display(Name = "Excel File", Description = "Microsoft Excel spreadsheet")]
    Excel = 2,
    
    [Display(Name = "CSV File", Description = "Comma-separated values file")]
    Csv = 3,
    
    [Display(Name = "Bank Statement", Description = "Bank statement in various formats")]
    BankStatement = 4,
    
    [Display(Name = "Poker Transaction Export", Description = "Poker platform transaction export")]
    PokerExport = 5,
    
    [Display(Name = "Crypto Exchange Export", Description = "Cryptocurrency exchange transaction export")]
    CryptoExport = 6,
    
    [Display(Name = "Manual Entry", Description = "Manually entered transaction data")]
    Manual = 7,
    
    [Display(Name = "API Import", Description = "Transaction imported via API")]
    Api = 8
} 