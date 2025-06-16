using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;
using SFManagement.Models.Entities;

namespace SFManagement.Models.Transactions;

// This class defines operations that transfer assets between two AssetHolders
// One of them will have a assetWallet that managers an asset's pool
// The other will register in these assetWallet creating an WalletIdentifier
// The transaction happens between the WI and the W
public class BaseTransaction : BaseDomain
{
    [Required] public DateTime Date { get; set; }
    
    // The one creating the transaction - AssetHolderWalletIdentifierId
    public Guid WalletIdentifierId { get; set; }
    
    // The one who owns the asset pool (reference for Direction) - AssetHolderWalletId
    public Guid AssetWalletId { get; set; }
    
    [MaxLength(50)] public string? Description { get; set; }
    
    [Precision(18, 2)] public decimal AssetAmount { get; set; }
    
    [Required] public TransactionDirection TransactionDirection { get; set; }
    
    public Guid? TagId { get; set; }
    public virtual Tag? Tag { get; set; }
    
    // add this two to the base data table (audit Table) so all tables will refer to that
    public DateTime? ApprovedAt { get; set; }

    public Guid? ApprovedBy { get; set; }
}