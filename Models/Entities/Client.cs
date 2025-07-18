using System.ComponentModel.DataAnnotations;
using SFManagement.Interfaces;

namespace SFManagement.Models.Entities;

public class Client : BaseDomain, IAssetHolder
{
    [Required] public Guid BaseAssetHolderId { get; set; }
    
    public virtual BaseAssetHolder? BaseAssetHolder { get; set; }
    
    public DateTime? Birthday { get; set; }
    
    /// <summary>
    /// Calculates the age of the client based on their birthday
    /// </summary>
    public int? Age => Birthday.HasValue ? 
        DateTime.Now.Year - Birthday.Value.Year - 
        (DateTime.Now.DayOfYear < Birthday.Value.DayOfYear ? 1 : 0) : null;
}