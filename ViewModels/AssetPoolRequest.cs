using SFManagement.Enums;

namespace SFManagement.ViewModels;

public class AssetPoolRequest
{
    public Guid BaseAssetHolderId { get; set; }
    
    public AssetType AssetType { get; set; }
    
}