namespace SFManagement.ViewModels;

public class BaseAssetHolderResponse : BaseResponse
{
    public string? Name { get; set; }

    public string? Email { get; set; }
    
    public AddressResponse? Address { get; set; }
    
    public string? Cpf { get; set; }
    
    public string? Cnpj { get; set; }
}