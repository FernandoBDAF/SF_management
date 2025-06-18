namespace SFManagement.ViewModels;

public class MemberRequest : BaseAssetHolderRequest
{
    public DateTime? Birthday { get; set; }
    
    public double? Share { get; set; }
}