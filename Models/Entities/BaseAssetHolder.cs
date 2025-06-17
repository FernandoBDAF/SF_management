using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SFManagement.Models.Transactions;

namespace SFManagement.Models.Entities;

public class BaseAssetHolder : BaseDomain
{
    [Required] [MaxLength(20)] public string Name { get; set; }

    [MaxLength(40)] public string? Email { get; set; }
    
    public virtual Address? Address { get; set; }
    
    [MaxLength(20)] public string? Cpf { get; set; }
    
    [MaxLength(20)] public string? Cnpj { get; set; }
    
    public virtual ICollection<AssetWallet> AssetWallets { get; set; } = new HashSet<AssetWallet>();
    
    public virtual ICollection<WalletIdentifier> WalletIdentifiers { get; set; } = new HashSet<WalletIdentifier>();
}