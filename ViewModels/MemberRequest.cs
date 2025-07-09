using System.ComponentModel.DataAnnotations;

namespace SFManagement.ViewModels;

public class MemberRequest : BaseAssetHolderRequest
{
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime? Birthday { get; set; }
    
    [Range(0.0, 1.0, ErrorMessage = "Share must be between 0 and 1")]
    public double? Share { get; set; }
}