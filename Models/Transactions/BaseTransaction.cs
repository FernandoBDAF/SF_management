using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;
using SFManagement.Models.Entities;

namespace SFManagement.Models.Transactions;

public class BaseTransaction : BaseDomain
{
    [Required] public DateTime Date { get; set; }
    
    [Required] public Guid WalletIdentifierId { get; set; }
    
    [Required] public Guid AssetWalletId { get; set; }
    
    [MaxLength(50)] public string? Description { get; set; }
    
    [Required] [Precision(18, 2)] public decimal AssetAmount { get; set; }
    
    // mudar para sinal?
    [Required] public TransactionDirection TransactionDirection { get; set; }
    
    public Guid? TagId { get; set; }
    public virtual Tag? Tag { get; set; }
}