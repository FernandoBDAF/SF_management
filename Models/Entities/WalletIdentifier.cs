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
    
    // Only one of the following relationships bellow should happen, so there is
    // a logic handling this in the service.
    public Guid? BankId { get; set; }
    public virtual Bank? Bank { get; set; }
    public Guid? ClientId { get; set; }
    public virtual Client? Client { get; set; }

    public Guid? MemberId { get; set; }
    public virtual Member? Member { get; set; }

    public Guid? PokerManagerId { get; set; }
    public virtual PokerManager? PokerManager { get; set; }
}