using SFManagement.Enums;
using SFManagement.Models.Transactions;

namespace SFManagement.ViewModels;

public class DigitalAssetTransactionResponse : BaseTransactionResponse
{
    public decimal? AssetAmount { get; set; }
    
    public TransactionDirection? TransactionDirection { get; set; }
    
    public AssetType? ConvertTo { get; set; }
    
    public decimal? ConversionRate { get; set; }
    
    public decimal? Rate { get; set; }
    
    public decimal? Profit { get; set; }
    
    public virtual ExcelResponse? Excel { get; set; }
}