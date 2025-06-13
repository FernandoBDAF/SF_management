using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SFManagement.Models.Entities;

namespace SFManagement.Models.Transactions;

// This class defines operations that transfer assets between two AssetHolders
// One of them will have a wallet that managers an asset's pool
// The other will register in these wallet creating an WalletIdentifier
// The transaction happens between the WI and the W
public class BaseTransaction : BaseDomain
{
    [Required] public DateTime Date { get; set; }
    
    // The one creating the transaction
    [ForeignKey("WalletIdentifier")] public Guid AssetHolderWalletIdentifierId { get; set; }
    
    // The one who owns the asset pool
    [ForeignKey("Wallet")] public Guid AssetHolderWalletId { get; set; }

    [MaxLength(30)] public string? Description { get; set; }
    
    [ForeignKey("Tag")] public Guid? TagId { get; set; }

    public virtual Tag Tag { get; set; } = new();
    
    public DateTime? ApprovedAt { get; set; }

    public Guid? ApprovedBy { get; set; }
}