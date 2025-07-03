namespace SFManagement.ViewModels;

public class BaseAssetHolderRequest
{
    public Guid? BaseAssetHolderId { get; set; }
    public string? Name { get; set; }

    public string? Email { get; set; }
    
    public string? Cpf { get; set; }
    
    public string? Cnpj { get; set; }
}