using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;

namespace SFManagement.Models.Entities;

public class InitialBalance : BaseDomain
{
    [Precision(18, 2)] public decimal Balance { get; set; }
    
    public AssetType BalanceUnit { get; set; }
    
    [Precision(18, 2)] public decimal? ConversionRate { get; set; }
    
    public AssetType? BalanceAs { get; set; }
    
    // Only one of the following relationships bellow should happen, so there is
    // a logic handling this in the service.
    public Guid? ClientId { get; set; }
    public virtual Client? Client { get; set; }
    
    public Guid? MemberId { get; set; }
    public virtual Member? Member { get; set; }
    
    // public Guid? BankId { get; set; }
    // public virtual Bank? Bank { get; set; }
    
    public Guid? PokerManagerId { get; set; }
    public virtual PokerManager? PokerManager { get; set; }
}