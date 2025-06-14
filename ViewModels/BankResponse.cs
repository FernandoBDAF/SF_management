using SFManagement.Models.Entities;

namespace SFManagement.ViewModels;

public class BankResponse : BaseAssetHolderResponse
{
    public string? Code { get; set; }
    
    public OfxResponse[]? Ofxs { get; set; }
    
    public InitialBalanceResponse[]? InitialBalances { get; set; }
    
    public ContactPhoneResponse[]? ContactPhones { get; set; }
}