using SFManagement.Enums;
using SFManagement.Models.Entities;

namespace SFManagement.ViewModels;

public class AssetWalletResponse : BaseResponse
{
    public AssetType AssetType { get; set; }
    
    public decimal? InitialAssetAmount { get; set; }
    
    public decimal? DefaultAgreedCommission { get; set; }
    
    public WalletIdentifierResponse[]? WalletIdentifiers { get; set; }
    
    public Guid? ClientId { get; set; }
    public string? ClientName { get; set; }
    
    public Guid? MemberId { get; set; }
    public string? MemberName { get; set; }
    
    public Guid? BankId { get; set; }
    public string? BankName { get; set; }
    
    public Guid? PokerManagerId { get; set; }
    public string? PokerManagerName { get; set; }
}