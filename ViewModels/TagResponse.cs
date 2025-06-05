namespace SFManagement.ViewModels;

public class TagResponse : BaseResponse
{
    public string? Description { get; set; }

    public Guid? ParentId { get; set; }

    public List<TagResponse>? Children { get; set; }
}