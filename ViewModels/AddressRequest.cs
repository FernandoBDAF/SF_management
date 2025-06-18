using SFManagement.Models.Entities;

namespace SFManagement.ViewModels;

public class AddressRequest
{
    public Guid? BankId { get; set; }
    
    public Guid? ClientId { get; set; }
    
    public Guid? MemberId { get; set; }
    
    public Guid? PokerManagerId { get; set; }
    
    public string? Postcode { get; set; }
    
    public string? StreetAddress { get; set; }

    public int? Number { get; set; }
    
    public string? City { get; set; }

    public string? State { get; set; }
    
    public string? Country { get; set; }

    public string? Complement { get; set; }
}