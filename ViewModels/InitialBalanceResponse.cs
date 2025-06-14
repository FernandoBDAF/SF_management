using SFManagement.Enums;

namespace SFManagement.ViewModels;

public class InitialBalanceResponse : BaseResponse
{
    public decimal? Balance { get; set; }
    
    public AssetType? BalanceUnit { get; set; }
    
    public decimal? ConversionRate { get; set; }
    
    public AssetType? ConvertTo { get; set; }
    
    public Guid? ClientId { get; set; }
    
    public Guid? MemberId { get; set; }
    
    public Guid? BankId { get; set; }
    
    public Guid? PokerManagerId { get; set; }
}