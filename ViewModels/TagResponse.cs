using SFManagement.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFManagement.ViewModels
{
    public class TagResponse : BaseResponse
    {
        public string? Description { get; set; }

        public Guid? ParentId { get; set; }

        public virtual TagResponse? Parent { get; set; }

        public virtual List<TagResponse> Children { get; set; } = new List<TagResponse>();
    }
}
