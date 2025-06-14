namespace SFManagement.ViewModels;

public class ContactPhoneResponse
{
    public int? CountryCode { get; set; }
    
    public int? LocalCode { get; set; }
    
    public string? PhoneNumber { get; set; }
    
    public string? SearchFor { get; set; }
    
    public Guid? ClientId { get; set; }
    
    public Guid? MemberId { get; set; }
    
    public Guid? BankId { get; set; }
    
    public Guid? PokerManagerId { get; set; }
}