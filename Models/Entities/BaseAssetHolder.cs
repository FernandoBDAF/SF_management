using System.ComponentModel.DataAnnotations;
using SFManagement.Enums;
using SFManagement.Interfaces;
using SFManagement.Models.AssetInfrastructure;
using SFManagement.Models.Support;

namespace SFManagement.Models.Entities;

public class BaseAssetHolder : BaseDomain, IAssetHolder
{
    [Required] [MaxLength(40)] public string Name { get; set; }

    [MaxLength(40)] public string? Email { get; set; }
    
    
    [MaxLength(20)] public string? Cpf { get; set; }
    
    [MaxLength(20)] public string? Cnpj { get; set; }
    public virtual Address? Address { get; set; }
    
    // Navigation properties to specific asset holder types (only one will have a value)
    public virtual Client? Client { get; set; }
    public virtual Bank? Bank { get; set; }
    public virtual Member? Member { get; set; }
    public virtual PokerManager? PokerManager { get; set; }
    
    // Computed property to get the specific asset holder type
    public object? SpecificAssetHolder
    {
        get
        {
            if (Client != null) return Client;
            if (Bank != null) return Bank;
            if (Member != null) return Member;
            if (PokerManager != null) return PokerManager;
            return null;
        }
    }
    
    // Property to get the asset holder type as an enum
    public AssetHolderType AssetHolderType
    {
        get
        {
            if (Client != null) return AssetHolderType.Client;
            if (Bank != null) return AssetHolderType.Bank;
            if (Member != null) return AssetHolderType.Member;
            if (PokerManager != null) return AssetHolderType.PokerManager;
            return AssetHolderType.Unknown;
        }
    }
    
    public virtual ICollection<Referral> Referral { get; set; } = new HashSet<Referral>();
    
    public virtual ICollection<AssetWallet> AssetWallets { get; set; } = new HashSet<AssetWallet>();
    
    public virtual ICollection<WalletIdentifier> WalletIdentifiers { get; set; } = new HashSet<WalletIdentifier>();
    
    public virtual ICollection<InitialBalance> InitialBalances { get; set; } = new HashSet<InitialBalance>();
    
    public virtual ICollection<ContactPhone> ContactPhones { get; set; } = new HashSet<ContactPhone>();
}

// Enum to represent the different asset holder types
public enum AssetHolderType
{
    Unknown = 0,
    Client = 1,
    Bank = 2,
    Member = 3,
    PokerManager = 4
}