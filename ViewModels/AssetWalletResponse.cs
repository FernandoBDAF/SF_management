using SFManagement.Enums;
using SFManagement.Models.Entities;
using SFManagement.Models.Transactions;

namespace SFManagement.ViewModels;

public class AssetWalletResponse : BaseResponse
{
    public AssetType AssetType { get; set; }
    
    public decimal? DefaultAgreedCommission { get; set; }
    
    public Guid? ClientId { get; set; }
    public string? ClientName { get; set; }
    
    public Guid? MemberId { get; set; }
    public string? MemberName { get; set; }
    
    public Guid? BankId { get; set; }
    public string? BankName { get; set; }
    
    public Guid? PokerManagerId { get; set; }
    public string? PokerManagerName { get; set; }
    
    public virtual ICollection<FiatAssetTransaction>? FiatAssetTransactions { get; set; }
    public virtual ICollection<DigitalAssetTransaction>? DigitalAssetTransactions { get; set; }
}
