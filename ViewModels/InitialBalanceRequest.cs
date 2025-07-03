using SFManagement.Enums;

namespace SFManagement.ViewModels;

public class InitialBalanceRequest
{
    public decimal? Balance { get; set; }
    
    public AssetType? BalanceUnit { get; set; }
    
    public decimal? ConversionRate { get; set; }
    
    public AssetType? ConvertTo { get; set; }
    
    public Guid? BaseAssetHolderId { get; set; }
}