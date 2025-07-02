namespace SFManagement.Models;

public class BaseDomain
{
    public Guid Id { get; set; }
    
    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Guid? LastModifiedBy { get; set; }
}