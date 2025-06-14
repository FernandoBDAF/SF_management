using SFManagement.Models.Entities;

namespace SFManagement.ViewModels;

public class PokerManagerResponse : BaseAssetHolderResponse
{
    public ExcelResponse[]? Excels { get; set; }
    
    public InitialBalanceResponse[]? InitialBalances { get; set; }
    
    public ContactPhoneResponse[]? ContactPhones { get; set; }
    
    public WalletResponse[]? Wallets { get; set; }
    
    public WalletIdentifierResponse[]? WalletIdentifiers { get; set; }
}