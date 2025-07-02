using SFManagement.Models.Entities;

namespace SFManagement.ViewModels;

public class ClientResponse : BaseAssetHolderResponse
{
    public DateTime? Birthday { get; set; }
    
    public WalletIdentifierResponse[]? WalletIdentifiers { get; set; }
    
    public AssetWalletResponse[]? AssetWallets { get; set; }
    
    public InitialBalanceResponse[]? InitialBalances { get; set; }
    
    public ContactPhoneResponse[]? ContactPhones { get; set; }
}