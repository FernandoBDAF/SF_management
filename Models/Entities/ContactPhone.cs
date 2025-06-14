using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SFManagement.Models.Entities;

public class ContactPhone : BaseDomain
{
    public int? CountryCode { get; set; }
    
    public int? LocalCode { get; set; }
    
    [Required] [MaxLength(20)] public string PhoneNumber { get; set; }
    
    [MaxLength(30)] public string? SearchFor { get; set; }
    
    // The relationships require that the service has a logic to garanteed only one of the 4 classes bellow
    // Also to avoid dependency loop .....
    public Guid? ClientId { get; set; }
    public virtual Client? Client { get; set; }
    
    public Guid? MemberId { get; set; }
    public virtual Member? Member { get; set; }
    
    public Guid? BankId { get; set; }
    public virtual Bank? Bank { get; set; }
    
    public Guid? PokerManagerId { get; set; }
    public virtual PokerManager? PokerManager { get; set; }
}