using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SFManagement.Models.Entities;

public class ContactPhone : BaseDomain
{
    [Required] public int CountryCode { get; set; }
    
    public int? LocalCode { get; set; }
    
    [Required] [MaxLength(20)] public string PhoneNumber { get; set; }
    
    public string? SearchFor { get; set; }
    
    public Guid? ClientId { get; set; }
    
    public virtual Client? Client { get; set; }
    
    public Guid? MemberId { get; set; }
    
    public virtual Member? Member { get; set; }
    
    public Guid? BankId { get; set; }
    
    public virtual Bank? Bank { get; set; }
    
    public Guid? PokerManagerId { get; set; }
    
    public virtual PokerManager? PokerManager { get; set; }
    
}