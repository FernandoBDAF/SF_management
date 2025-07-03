using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;
using SFManagement.Models.Entities;
using SFManagement.Models.Support;
using SFManagement.Models.Transactions;

namespace SFManagement.Models.AssetInfrastructure;

public class WalletIdentifier : BaseDomain
{
    [Required] public Guid BaseAssetHolderId { get; set; }
    public virtual BaseAssetHolder BaseAssetHolder { get; set; }
    
    public AssetType AssetType { get; set; }
    
    // Nickname, Routing Number, Agencia
    [MaxLength(40)]
    public string? RouteInfo { get; set; }
    
    // Account Number, email, Conta
    [MaxLength(40)]
    public string? IdentifierInfo { get; set; }
    
    // Account Type, pix, poupanca
    [MaxLength(40)]
    public string? DescriptiveInfo { get; set; }
    
    // Name, 
    [MaxLength(50)]
    public string? ExtraInfo { get; set; }
    
    // PIX, PokerManager input, etc...
    [Required, MaxLength(30)]
    public string InputForTransactions { get; set; }
    
    public virtual Referral? Referral { get; set; }
    
    public virtual ICollection<FiatAssetTransaction> FiatAssetTransactions { get; set; } = new HashSet<FiatAssetTransaction>();
    
    public virtual ICollection<DigitalAssetTransaction> DigitalAssetTransactions { get; set; } = new HashSet<DigitalAssetTransaction>();
    
    public virtual ICollection<SettlementTransaction> SettlementTransactions { get; set; } = new HashSet<SettlementTransaction>();
}