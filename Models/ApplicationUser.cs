using Microsoft.AspNetCore.Identity;

namespace SFManagement.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string? Name { get; set; }

        public string? Address { get; set; }
    }
}
 