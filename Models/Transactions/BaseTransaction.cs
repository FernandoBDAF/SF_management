using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SFManagement.Models.AssetInfrastructure;
using SFManagement.Models.Support;

namespace SFManagement.Models.Transactions;

public class BaseTransaction : BaseDomain
{
    [Required] public DateTime Date { get; set; }
    
    public Guid? CategoryId { get; set; }
    public virtual Category? Category { get; set; }

    // Sender
    [Required] public Guid SenderWalletIdentifierId { get; set; }
    public virtual WalletIdentifier SenderWalletIdentifier { get; set; }

    // Receiver
    [Required] public Guid ReceiverWalletIdentifierId { get; set; }
    public virtual WalletIdentifier ReceiverWalletIdentifier { get; set; }
    
    [Required] [Precision(18, 2)] public decimal AssetAmount { get; set; }
    
    [MaxLength(50)] public string? Description { get; set; }
    
    public DateTime? ApprovedAt { get; set; }
}