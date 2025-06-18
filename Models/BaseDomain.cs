namespace SFManagement.Models;

public class BaseDomain
{
    public Guid Id { get; set; }
    
    public DateTime? CreatedAt { get; set; }

    public Guid? CreatorId { get; set; }

    public Guid? EditorId { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? DeleteId { get; set; }

    public DateTime? DeletedAt { get; set; }
    
    public DateTime? ApprovedAt { get; set; }

    public Guid? ApprovedBy { get; set; }
}