namespace SFManagement.Models;

public class Address
{

    public string? StreetAddress { get; set; }

    public int? Number { get; set; }
    
    public string? City { get; set; }

    public string? State { get; set; }
    
    public string? Country { get; set; }
    
    // ["xxxxx-xxx"]
    public string Postcode { get; set; }

    public string? Complement { get; set; }
    
    public virtual List<Client> Client { get; set; } = new();

}