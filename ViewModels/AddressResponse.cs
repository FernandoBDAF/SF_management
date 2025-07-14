using SFManagement.Models.Support;

namespace SFManagement.ViewModels;

public class AddressResponse : BaseResponse
{
    public Guid BaseAssetHolderId { get; set; }
    
    public string Postcode { get; set; } = string.Empty;
}