using SFManagement.Models.Entities;

namespace SFManagement.ViewModels;

public class AddressRequest
{
    public Guid BaseAssetHolderId { get; set; }
    
    public string Postcode { get; set; } = string.Empty;
}