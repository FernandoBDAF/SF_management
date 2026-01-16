using SFManagement.Application.DTOs.Common;
using SFManagement.Application.DTOs.Assets;
using SFManagement.Application.DTOs.Support;
using System.ComponentModel.DataAnnotations;

namespace SFManagement.Application.DTOs.AssetHolders;

public class MemberRequest : BaseAssetHolderRequest
{
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime? Birthday { get; set; }
    
    [Range(0.0, 100.00, ErrorMessage = "Share must be between 0 and 100")]
    public decimal? Share { get; set; }

    [Range(0.0, float.MaxValue, ErrorMessage = "Salary must be greater than 0")]
    public decimal? Salary { get; set; }
}