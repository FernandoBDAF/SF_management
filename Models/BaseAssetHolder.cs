using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SFManagement.Models.Transactions;

namespace SFManagement.Models.Entities;

public class BaseAssetHolder : BaseDomain
{
    [Required] [MaxLength(20)] public string Name { get; set; } = "";

    public ICollection<ContactPhone> PhonesNumbers { get; set; } = new HashSet<ContactPhone>();

    [Required] [MaxLength(30)] public string? Email { get; set; }
    
    [ForeignKey("Address")] public Guid? AddressId { get; set; }
    
    public virtual Address? Address { get; set; }
    
    [Required] [MaxLength(20)] public string? Cpf { get; set; }
    
    [Required] [MaxLength(20)] public string? Cnpj { get; set; }
    
    public virtual ICollection<InitialBalance> InitialBalances { get; set; } = new HashSet<InitialBalance>();
    
    public virtual ICollection<Wallet> Wallets { get; set; } = new HashSet<Wallet>();
    
    public virtual ICollection<WalletIdentifier> WalletIdentifiers { get; set; } =  new HashSet<WalletIdentifier>();
}