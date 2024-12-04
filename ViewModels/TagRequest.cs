namespace SFManagement.ViewModels
{
    public class TagRequest : BaseResponse
    {
        public string? Description { get; set; }

        public Guid? ParentId { get; set; }
    }
}
