using SFManagement.Application.DTOs.Common;
namespace SFManagement.Application.DTOs.Support;

public class ContactPhoneRequest
{
    public int? CountryCode { get; set; }
    
    public int? AreaCode { get; set; }
    
    public int? PhoneNumber { get; set; }
    
    public string InputPhoneNumber { get; set; } = string.Empty;
    
    public string? SearchFor { get; set; }
    
    public Guid BaseAssetHolderId { get; set; }
}