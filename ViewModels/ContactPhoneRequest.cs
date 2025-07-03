namespace SFManagement.ViewModels;

public class ContactPhoneRequest
{
    public int? CountryCode { get; set; }
    
    public int? LocalCode { get; set; }
    
    public string? PhoneNumber { get; set; }
    
    public string? SearchFor { get; set; }
    
    public Guid? BaseAssetHolderId { get; set; }
}