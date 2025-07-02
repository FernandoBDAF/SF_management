using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;
using SFManagement.Models.Transactions;

namespace SFManagement.Models.Entities;

public class WalletIdentifier : BaseDomain
{
    
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
    
    public AssetType AssetType { get; set; }
    
    [Precision(18, 2)]
    public decimal? DefaultRakeCommission { get; set; }

    [Precision(18, 2)]
    public decimal? DefaultParentCommission { get; set; }
    
    public virtual ICollection<FiatAssetTransaction>? FiatAssetTransactions { get; set; }
    public virtual ICollection<DigitalAssetTransaction>? DigitalAssetTransactions { get; set; }
    
    public Guid AssetHolderId { get; set; }
    public BaseAssetHolder AssetHolder { get; set; }
}