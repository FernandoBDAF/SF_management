using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;

namespace SFManagement.Models.Transactions;

public class ExcelTransaction : BaseDomain
{
    
    public DateTime Date { get; set; }

    [Precision(18, 2)] public decimal Coins { get; set; }
    
    [MaxLength(30)] public string? Description { get; set; }

    public WalletTransactionType WalletTransactionType { get; set; }

    [MaxLength(30)] public string? ExcelNickname { get; set; }
    
    [MaxLength(30)] public string? ExcelWallet { get; set; }

    [ForeignKey("Excel")] public Guid ExcelId { get; set; }
    
    [ForeignKey("DigitalAssetTransaction")] public Guid? WalletTransactionId { get; set; }

}