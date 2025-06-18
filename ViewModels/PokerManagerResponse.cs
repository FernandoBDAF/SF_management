using SFManagement.Models.Entities;

namespace SFManagement.ViewModels;

public class PokerManagerResponse : BaseAssetHolderResponse
{
    public ExcelResponse[]? Excels { get; set; }
    
    public InitialBalanceResponse[]? InitialBalances { get; set; }
    
    public ContactPhoneResponse[]? ContactPhones { get; set; }
    
    public AssetWalletResponse[]? AssetWallets { get; set; }
    
    public WalletIdentifierResponse[]? WalletIdentifiers { get; set; }
}