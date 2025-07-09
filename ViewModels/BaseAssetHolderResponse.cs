using SFManagement.Models.Support;

namespace SFManagement.ViewModels;

public class BaseAssetHolderResponse : BaseResponse
{
    public Guid? BaseAssetHolderId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Cpf { get; set; }
    public string? Cnpj { get; set; }
    
    public AddressResponse? Address { get; set; }

    public List<AssetWalletResponse> AssetWallets { get; set; } = new List<AssetWalletResponse>();
    
    // Remove redundant collections - these should be accessed through separate endpoints
    // Collections like AssetWallets, WalletIdentifiers, etc. create circular references
    // and performance issues in responses
}