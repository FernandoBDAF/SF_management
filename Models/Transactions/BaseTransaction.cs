using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;
using SFManagement.Models.Entities;

namespace SFManagement.Models.Transactions;

public class BaseTransaction : BaseDomain
{
    [Required] public DateTime Date { get; set; }
    
    public Guid? WalletIdentifierId { get; set; }
    public virtual WalletIdentifier? WalletIdentifier { get; set; }
    
    [Required] public Guid AssetWalletId { get; set; }
    public virtual AssetWallet? AssetWallet { get; set; }
    
    [MaxLength(50)] public string? Description { get; set; }
    
    [Required] [Precision(18, 2)] public decimal AssetAmount { get; set; }
    
    // mudar para sinal?
    [Required] public TransactionDirection TransactionDirection { get; set; }
    
    public Guid? FinancialBehaviorId { get; set; }
    public virtual FinancialBehavior? FinancialBehavior { get; set; }
}