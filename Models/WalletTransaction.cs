using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;
using SFManagement.Models.Transactions;

namespace SFManagement.Models.Transactions;

public class WalletTransaction : BaseTransaction
{
    [Precision(18, 2)] public decimal? Value { get; set; }

    [Precision(18, 2)] public decimal? ExchangeRate { get; set; }
    
    // This refers to a % charge for exchanging different types of wallet's coins
    [Precision(18, 2)] public decimal? Rate { get; set; }

    public bool IsCoinBalance { get; set; }

    [Precision(18, 2)] public decimal Coins { get; set; }

    [Precision(18, 2)] public decimal? Profit { get; set; }

    public WalletTransactionType WalletTransactionType { get; set; }

    [ForeignKey("Wallet")] public Guid WalletId { get; set; }

    [ForeignKey("Nickname")] public Guid NicknameId { get; set; }

    [ForeignKey("Excel")] public Guid? ExcelId { get; set; }
}