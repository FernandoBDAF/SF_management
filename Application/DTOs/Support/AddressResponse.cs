using SFManagement.Application.DTOs.Common;
using SFManagement.Domain.Entities.Support;

namespace SFManagement.Application.DTOs.Support;

public class AddressResponse : BaseResponse
{
    public Guid BaseAssetHolderId { get; set; }
    
    public string Postcode { get; set; } = string.Empty;
}