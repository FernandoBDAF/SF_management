using SFManagement.Models.Entities;

namespace SFManagement.ViewModels;

public class MemberResponse : BaseAssetHolderResponse
{
    public DateTime? Birthday { get; set; }
    
    public double? Share { get; set; }
    
    public AssetWalletResponse[]? Wallets { get; set; }
    
    public WalletIdentifierResponse[]? WalletIdentifiers { get; set; }
    
    public InitialBalanceResponse[]? InitialBalances { get; set; }
    
    public ContactPhoneResponse[]? ContactPhones { get; set; }
}