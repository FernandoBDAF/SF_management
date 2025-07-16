using SFManagement.Enums;
using SFManagement.Models.Entities;
using SFManagement.Models.Transactions;

namespace SFManagement.ViewModels;

public class AssetPoolResponse : BaseResponse
{
    public AssetGroup AssetGroup { get; set; }
    
    public decimal? DefaultAgreedCommission { get; set; }
    
    public Guid BaseAssetHolderId { get; set; }
    
    public string? BaseAssetHolderName { get; set; }
    
    public List<WalletIdentifierResponse> WalletIdentifiers { get; set; } = new List<WalletIdentifierResponse>();
}
