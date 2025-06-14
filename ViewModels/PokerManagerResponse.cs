using SFManagement.Enums;
using SFManagement.Models.Entities;
using SFManagement.Models.Transactions;

namespace SFManagement.ViewModels;

public class PokerManagerResponse : BaseAssetHolderResponse
{
    public Wallet[]? Wallets { get; set; }
    
    public WalletIdentifierResponse[]? WalletIdentifiers { get; set; }
    
    public InitialBalanceResponse[]? InitialBalances { get; set; }
    
    public ContactPhoneResponse[]? ContactPhones { get; set; }
    
    public ExcelResponse[]? Excels { get; set; }
}