using SFManagement.Enums;
using SFManagement.Models.Support;

namespace SFManagement.ViewModels;

public class BaseAssetHolderResponse : BaseResponse
{
    public Guid? BaseAssetHolderId { get; set; }
    public string? Name { get; set; }
    
    public TaxEntityType TaxEntityType { get; set; }

    public string GovernmentNumber { get; set; }
    
    public AddressResponse? Address { get; set; }

    public List<AssetPoolResponse> AssetPools { get; set; } = new List<AssetPoolResponse>();
    
    // Remove redundant collections - these should be accessed through separate endpoints
    // Collections like AssetPools, WalletIdentifiers, etc. create circular references
    // and performance issues in responses
}