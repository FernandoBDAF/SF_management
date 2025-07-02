using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;
using SFManagement.Models.AssetInfrastructure;
using SFManagement.Models.Support;

namespace SFManagement.Models.Transactions;

public class BaseTransaction : BaseDomain
{
    [Required] public DateTime Date { get; set; }
    
    public Guid? FinancialBehaviorId { get; set; }
    public virtual FinancialBehavior? FinancialBehavior { get; set; }

    public Guid? WalletIdentifierId { get; set; }
    public virtual WalletIdentifier? WalletIdentifier { get; set; }
    
    [Required] public Guid AssetWalletId { get; set; }
    public virtual AssetWallet? AssetWallet { get; set; }
    
    [Required] [Precision(18, 2)] public decimal AssetAmount { get; set; }
    
    [Required] public TransactionDirection TransactionDirection { get; set; }
    
    [MaxLength(50)] public string? Description { get; set; }
    
    public DateTime? ApprovedAt { get; set; }
}