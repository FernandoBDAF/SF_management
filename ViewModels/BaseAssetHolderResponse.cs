namespace SFManagement.ViewModels;

public class BaseAssetHolderResponse : BaseResponse
{
    public string Name { get; set; }

    // public ICollection<ContactPhone> PhonesNumbers { get; set; } = new HashSet<ContactPhone>();

    public string? Email { get; set; }
    
    public AddressResponse? Address { get; set; }
    
    public string? Cpf { get; set; }
    
    public string? Cnpj { get; set; }
    
    // public virtual ICollection<InitialBalance> InitialBalances { get; set; } = new HashSet<InitialBalance>();
    //
    // public virtual ICollection<Wallet> Wallets { get; set; } = new HashSet<Wallet>();
    //
    // public virtual ICollection<WalletIdentifier> WalletIdentifiers { get; set; } =  new HashSet<WalletIdentifier>();
}