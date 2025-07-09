using SFManagement.Enums;

namespace SFManagement.ViewModels;

public class AssetWalletRequest
{
    public Guid BaseAssetHolderId { get; set; }
    
    public AssetType AssetType { get; set; }
    
}