using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SFManagement.Interfaces;

namespace SFManagement.Models.Entities;

public class Member : BaseDomain, IAssetHolder
{
    [Required] public Guid BaseAssetHolderId { get; set; }
    public virtual BaseAssetHolder? BaseAssetHolder { get; set; }
    
    [Precision(18, 4)]
    [Range(0.0, 100.00, ErrorMessage = "Share must be between 0 and 100")]
    public decimal? Share { get; set; }

    [Precision(18, 2)]
    [Range(0.0, float.MaxValue, ErrorMessage = "Salary must be greater than 0")]
    public decimal? Salary { get; set; }
    
    public DateTime? Birthday { get; set; }
    
    /// <summary>
    /// Indicates if the member has an active share (greater than 0)
    /// </summary>
    public bool IsActiveShare => Share > 0;
}