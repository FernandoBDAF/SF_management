using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;

namespace SFManagement.Models.Entities;

public class Wallet : BaseDomain
{
    public AssetType AssetType { get; set; }
    
    [Precision(18, 2)] public decimal? InitialAssetAmount { get; set; }
    
    [Precision(18, 2)] public decimal? DefaultAgreedCommission { get; set; }
    
    public virtual ICollection<WalletIdentifier> WalletIdentifiers { get; set; } = new List<WalletIdentifier>();
    
    public Guid? ClientId { get; set; }
    public virtual Client? Client { get; set; }
    
    public Guid? MemberId { get; set; }
    public virtual Member? Member { get; set; }
    
    public Guid? BankId { get; set; }
    public virtual Bank? Bank { get; set; }
    
    public Guid? PokerManagerId { get; set; }
    public virtual PokerManager? PokerManager { get; set; }
}