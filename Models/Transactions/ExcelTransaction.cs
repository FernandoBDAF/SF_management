using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;

namespace SFManagement.Models.Transactions;

public class ExcelTransaction : BaseDomain
{
    
    public DateTime Date { get; set; }

    [Precision(18, 2)] public decimal Coins { get; set; }
    
    public string? Description { get; set; }

    [ForeignKey("Manager")] public Guid ManagerId { get; set; }

    public virtual Manager Manager { get; set; }

    public WalletTransactionType WalletTransactionType { get; set; }

    public string ExcelNickname { get; set; }
    
    public string ExcelWallet { get; set; }

    [ForeignKey("Excel")] public Guid? ExcelId { get; set; }

    public virtual Excel Excel { get; set; }
    
    [ForeignKey("WalletTransaction")] public Guid? WalletTransactionId { get; set; }

    public virtual WalletTransaction WalletTransaction { get; set; } = new();

}