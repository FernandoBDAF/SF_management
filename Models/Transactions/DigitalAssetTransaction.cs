using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;
using SFManagement.Models.Transactions;

namespace SFManagement.Models.Transactions;

public class DigitalAssetTransaction : BaseTransaction
{
    [Precision(18, 2)] public decimal AssetAmount { get; set; }
    
    [Required] public TransactionDirection TransactionDirection { get; set; }
    
    public AssetType? ConvertTo { get; set; }
    
    [Precision(18, 2)] public decimal? ConversionRate { get; set; }
    
    // public bool IsAssetBalance { get; set; } // maybe this should be passed to the service
    
    // It is an absolute value that will increase/decrease the AssetAmount in the balance - like a subsequente transaction
    [Precision(18, 2)] public decimal? Rate { get; set; }
    
    [Precision(18, 2)] public decimal? Profit { get; set; }
    
    public Guid? ExcelId { get; set; }
    public virtual Excel? Excel { get; set; }
}