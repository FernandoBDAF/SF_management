using System.ComponentModel.DataAnnotations;

namespace SFManagement.Enums.ImportedFiles;

/// <summary>
/// Types of Excel imports supported for poker transactions
/// </summary>
public enum ExcelImportType
{
    [Display(Name = "Buy Transactions", Description = "Purchase transactions from poker platforms")]
    BuyTransactions = 1,
    
    [Display(Name = "Sell Transactions", Description = "Sale transactions from poker platforms")]
    SellTransactions = 2,
    
    [Display(Name = "Transfer Transactions", Description = "Transfer transactions between accounts")]
    TransferTransactions = 3
} 