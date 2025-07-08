namespace SFManagement.ViewModels;

public class CategoryResponse : BaseResponse
{
    public string? Description { get; set; }

    public Guid? ParentId { get; set; }

    public List<CategoryResponse>? Children { get; set; }
}