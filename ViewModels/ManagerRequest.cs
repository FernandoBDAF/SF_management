using Microsoft.EntityFrameworkCore;

namespace SFManagement.ViewModels
{
    public class ManagerRequest
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }
    }
}