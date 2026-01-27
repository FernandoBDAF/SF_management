using SFManagement.Application.DTOs.Common;
using SFManagement.Domain.Entities.AssetHolders;

namespace SFManagement.Application.DTOs.Support;

public class AddressRequest
{
    public Guid BaseAssetHolderId { get; set; }
    
    public string Postcode { get; set; } = string.Empty;
}