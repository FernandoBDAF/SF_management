using System.ComponentModel.DataAnnotations.Schema;

namespace SFManagement.Models
{
    public class Tag : BaseDomain
    {
        public string? Description { get; set; }

        [ForeignKey("Parent")]
        public Guid? ParentId { get; set; }
        
        public virtual Tag? Parent { get; set; }

        public virtual List<Tag> Children { get; set; } = new List<Tag>();
    }
}