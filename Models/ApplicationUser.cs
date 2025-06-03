using Microsoft.AspNetCore.Identity;

namespace SFManagement.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string? Name { get; set; }

        public string? Address { get; set; }

        public string? RefreshToken { get; set; }

        public DateTime RefreshTokenExpiryTime { get; set; }

        //TODO: ADD CLIENT RELATIONSHIP
    }
}
 