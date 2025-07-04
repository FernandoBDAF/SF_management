using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SFManagement.Models.AssetInfrastructure;
using SFManagement.Models.Entities;

namespace SFManagement.Models.Support;

public class Referral : BaseDomain
{
    [Required] public Guid WalletIdentifierId { get; set; }
    public virtual WalletIdentifier WalletIdentifier { get; set; }
    
    [Required] public Guid AssetHolderId { get; set; }
    public virtual BaseAssetHolder AssetHolder { get; set; }
    
    [Required] public DateTime? ActiveUntil { get; set; }
    
    [Precision(18, 2)] public decimal? ParentCommission { get; set; }
}