using System.ComponentModel.DataAnnotations.Schema;

namespace SFManagement.Models;

public class Member : Client
{
    [ForeignKey("Client")] public Guid? ClientId { get; set; }
    
    public double Share { get; set; }
}