using SFManagement.Enums;

namespace SFManagement.ViewModels;

public class AssetWalletRequest
{
    public AssetType? AssetType { get; set; }
    
    // public decimal? InitialAssetAmount { get; set; }
    
    public decimal? DefaultAgreedCommission { get; set; }
    
    public Guid? BaseAssetHolderId { get; set; }
}