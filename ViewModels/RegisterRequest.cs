using System.ComponentModel.DataAnnotations;

namespace SFManagement.ViewModels
{
    public class RegisterRequest
    {
        public string? Name { get; set; }
        
        public string? Username { get; set; }
        
        public string? Email { get; set; }

        public string? Password { get; set; }
    }
}
