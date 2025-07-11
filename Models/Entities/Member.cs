using System.ComponentModel.DataAnnotations;
using SFManagement.Interfaces;

namespace SFManagement.Models.Entities;

public class Member : BaseDomain, IAssetHolder
{
    [Required] public Guid BaseAssetHolderId { get; set; }
    
    public virtual BaseAssetHolder? BaseAssetHolder { get; set; }
    
    [Range(0.0, 1.0, ErrorMessage = "Share must be between 0 and 1")]
    public double Share { get; set; }
    
    public DateTime? Birthday { get; set; }
    
    /// <summary>
    /// Indicates if the member has an active share (greater than 0)
    /// </summary>
    public bool IsActiveShare => Share > 0;
}