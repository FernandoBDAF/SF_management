using SFManagement.Enums;

namespace SFManagement.ViewModels;

public class DigitalAssetTransactionRequest : BaseTransactionRequest
{
    public decimal? AssetAmount { get; set; }
    
    public TransactionDirection? TransactionDirection { get; set; }
    
    public AssetType? ConvertTo { get; set; }
    
    public decimal? ConversionRate { get; set; }
    
    public decimal? Rate { get; set; }
    
    public decimal? Profit { get; set; }
    
    public Guid? ExcelId { get; set; }
}