using System.ComponentModel.DataAnnotations.Schema;

namespace SFManagement.Models.Entities;

public class Member : BaseAssetHolder
{
    public double Share { get; set; }
    
    public DateTime? Birthday { get; set; }
}