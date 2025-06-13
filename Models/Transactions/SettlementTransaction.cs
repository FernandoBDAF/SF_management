using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SFManagement.Models.Entities;

namespace SFManagement.Models.Transactions;

public class SettlementTransaction : BaseTransaction
{
    [Required] public DateTime StartDate { get; set; }
    
    [Precision(18, 2)] public decimal? Balance { get; set; }
    
    [Precision(18, 2)] public decimal? Rake { get; set; }
    
    [Precision(18, 2)] public decimal? RakeCommission { get; set; }
    
    [Precision(18, 2)] public decimal? ParentCommission { get; set; }
    
    public decimal? AgreedCommission { get; set; } 
    
    public DateTime? DoneAt { get; set; }
    
    public DateTime? CalculatedAt { get; set; }
    
    // This is the parent
    public Guid? ClientId { get; set; }
    public virtual Client? Client { get; set; }
}