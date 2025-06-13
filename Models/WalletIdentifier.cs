using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SFManagement.Models.Closing;

namespace SFManagement.Models.Entities;

public class WalletIdentifier : BaseDomain
{
    [ForeignKey("Wallet")] public Guid WalletId { get; set; }

    [ForeignKey("Client")] public Guid AssetHolderId { get; set; }
    
    [Required] [MaxLength(20)] public string? Nickname { get; set; } = string.Empty;
    
    [MaxLength(30)] public string? Email { get; set; }
    
    [MaxLength(20)] public string? Phone { get; set; }
    
    [MaxLength(20)] public string? Pix { get; set; }

    [Required] [MaxLength(20)] public string InputForTransactions { get; set; } = string.Empty;
    
    [Precision(18, 2)] public decimal? DefaultRakeCommission { get; set; }
    
    [ForeignKey("Client")] public Guid? ParentId { get; set; }
    
    [Precision(18, 2)] public decimal? DefaultParentCommission { get; set; }
}