using Microsoft.EntityFrameworkCore;

namespace SFManagement.ViewModels
{
    public class ClosingManagerRequest
    {
        public Guid ManagerId { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }
    }
}
