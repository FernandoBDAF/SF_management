using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SFManagement.Models.Transactions;

public class SettlementTransaction : BaseTransaction
{
    [Precision(18, 4), Required] public decimal Rake { get; set; }
    
    [Precision(18, 4), Required] public decimal RakeCommission { get; set; }
    
    [Precision(18, 4)] public decimal? RakeBack { get; set; }
}