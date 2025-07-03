using SFManagement.Models.AssetInfrastructure;
using SFManagement.Models.Support;

namespace SFManagement.ViewModels;

public class BaseAssetHolderResponse : BaseResponse
{
    public Guid? BaseAssetHolderId { get; set; }
    public string? Name { get; set; }

    public string? Email { get; set; }
    
    public AddressResponse? Address { get; set; }
    
    public string? Cpf { get; set; }
    
    public string? Cnpj { get; set; }
    
    public virtual ICollection<Referral> Referrals { get; set; } = new HashSet<Referral>();
    
    public virtual ICollection<AssetWallet> AssetWallets { get; set; } = new HashSet<AssetWallet>();
    
    public virtual ICollection<WalletIdentifier> WalletIdentifiers { get; set; } = new HashSet<WalletIdentifier>();
    
    public virtual ICollection<InitialBalance> InitialBalances { get; set; } = new HashSet<InitialBalance>();
    
    public virtual ICollection<ContactPhone> ContactPhones { get; set; } = new HashSet<ContactPhone>();
}