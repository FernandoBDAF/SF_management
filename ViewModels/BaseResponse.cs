namespace SFManagement.ViewModels
{
    public class BaseResponse
    {
        public Guid Id { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }
        
        public Guid? CreatorId { get; set; }
    }
}
