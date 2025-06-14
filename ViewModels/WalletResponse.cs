using SFManagement.Enums;

namespace SFManagement.ViewModels;

public class WalletResponse : BaseResponse
{
    public AssetType? AssetType { get; set; }
    
    public decimal? InitialAssetAmount { get; set; }
    
    public decimal? DefaultAgreedCommission { get; set; }
    
    public WalletIdentifierResponse[]? WalletIdentifiers { get; set; }
    
    public Guid? ClientId { get; set; }
    
    public Guid? MemberId { get; set; }
    
    public Guid? BankId { get; set; }
    
    public Guid? PokerManagerId { get; set; }
}