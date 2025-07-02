using SFManagement.Enums;
using SFManagement.Models.Entities;
using SFManagement.Models.Transactions;

namespace SFManagement.ViewModels;

public class AssetWalletResponse : BaseResponse
{
    public AssetType AssetType { get; set; }
    
    public decimal? DefaultAgreedCommission { get; set; }
    
    public string? BaseAssetHolderName { get; set; }
    
    public virtual ICollection<FiatAssetTransaction>? FiatAssetTransactions { get; set; }
    public virtual ICollection<DigitalAssetTransaction>? DigitalAssetTransactions { get; set; }
}
