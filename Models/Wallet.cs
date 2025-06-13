using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;
using SFManagement.Models.Closing;
using SFManagement.Models.Transactions;

namespace SFManagement.Models.Entities;

public class Wallet : BaseDomain
{
    [ForeignKey("AssetHolder")] public Guid AssetHolderId { get; set; }
    
    public AssetType AssetType { get; set; }
    
    [Required] [MaxLength(20)] public string Name { get; set; } = "";
    
    public virtual ICollection<WalletIdentifier> WalletIdentifiers { get; set; } = new List<WalletIdentifier>();
    
    [ForeignKey("InitialBalance")] public Guid? InitialBalanceId { get; set; }
    
    public decimal? DefaultAgreedCommission { get; set; }
}