using System.ComponentModel.DataAnnotations;

namespace SFManagement.Models.Entities;

public class Address : BaseDomain
{
    [MaxLength(30)] public string? StreetAddress { get; set; }

    public int? Number { get; set; }
    
    [MaxLength(20)] public string? City { get; set; }

    [MaxLength(20)] public string? State { get; set; }
    
    [MaxLength(20)] public string? Country { get; set; }
    
    [Required] [MaxLength(10)] public string Postcode { get; set; }

    [MaxLength(30)] public string? Complement { get; set; }
    
    public Guid? ClientId { get; set; }
    public virtual Client? Client { get; set; }
    
    public Guid? MemberId { get; set; }
    public virtual Member? Member { get; set; }
    
    public Guid? BankId { get; set; }
    public virtual Bank? Bank { get; set; }
    
    public Guid? PokerManagerId { get; set; }
    public virtual PokerManager? PokerManager { get; set; }
}