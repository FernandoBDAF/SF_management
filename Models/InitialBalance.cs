using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;

namespace SFManagement.Models.Entities;

public class InitialBalance
{
    [ForeignKey("Wallet")] public Guid? WalletId { get; set; }

    [ForeignKey("AssetHolder")] public Guid? AssetHolderId { get; set; }
    
    [Precision(18, 2)] public decimal Balance { get; set; }
    
    public AssetType BalanceUnit { get; set; }
    
    [Precision(18, 2)] public decimal? ConversionRate { get; set; }
    
    public AssetType? ConvertTo { get; set; }
}