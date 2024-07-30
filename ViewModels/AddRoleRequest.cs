using System.ComponentModel.DataAnnotations;

namespace SFManagement.ViewModels
{
    public class AddRoleRequest
    {
        public string? Email { get; set; }

        public string? Password { get; set; }

        public string? Role { get; set; }
    }
}
