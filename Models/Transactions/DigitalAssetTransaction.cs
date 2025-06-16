using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;
using SFManagement.Models.Transactions;

namespace SFManagement.Models.Transactions;

public class DigitalAssetTransaction : BaseTransaction
{
    // change name: BalanceAs
    public AssetType? ConvertTo { get; set; } = AssetType.BrazilianReal;
    
    [Precision(18, 2)] public decimal? ConversionRate { get; set; }
    
    [Precision(18, 2)] public decimal? Rate { get; set; }
    
    [Precision(18, 2)] public decimal? Profit { get; set; }
    
    public Guid? ExcelId { get; set; }
    public virtual Excel? Excel { get; set; }
}