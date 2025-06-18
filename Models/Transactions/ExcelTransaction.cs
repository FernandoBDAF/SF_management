using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;

namespace SFManagement.Models.Transactions;

public class ExcelTransaction : BaseDomain
{
    
    public DateTime Date { get; set; }

    [Precision(18, 2)] public decimal Coins { get; set; }
    
    [MaxLength(50)] public string? Description { get; set; }

    public TransactionDirection TransactionDirection { get; set; }

    [MaxLength(40)] public string? ExcelNickname { get; set; }
    
    [MaxLength(40)] public string? ExcelWallet { get; set; }

    public Guid ExcelId { get; set; }
    public virtual Excel Excel { get; set; } = new Excel();
    
    public Guid? DigitalAssetTransactionId { get; set; }
    public virtual DigitalAssetTransaction? DigitalAssetTransaction { get; set; }

}