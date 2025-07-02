using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SFManagement.Models.AssetInfrastructure;

namespace SFManagement.Models.Support;

public class Referral
{
    [Required] public Guid WalletIdentifierId { get; set; }
    public virtual WalletIdentifier WalletIdentifier { get; set; }
    
    [Required] public Guid AssetHolderId { get; set; }
    
    [Required] public DateTime? ActiveUntil { get; set; }
    
    [Precision(18, 2)] public decimal? ParentCommission { get; set; }
}