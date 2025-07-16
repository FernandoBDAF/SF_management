using SFManagement.Enums;

namespace SFManagement.ViewModels;

public class DigitalAssetTransactionRequest : BaseTransactionRequest
{
    public AssetType? BalanceAs { get; set; }
    
    public decimal? ConversionRate { get; set; }
    
    public decimal? Rate { get; set; }
    
    public Guid? ExcelId { get; set; }
}