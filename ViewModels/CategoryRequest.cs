namespace SFManagement.ViewModels;

public class CategoryRequest
{
    public string? Description { get; set; }

    public Guid? ParentId { get; set; }
}