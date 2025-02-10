using System.ComponentModel.DataAnnotations;

namespace SFManagement
{
    public class Authorization
    {
        public enum Roles
        {
            [Display(Name = "ALL")]
            ALL,
        }

        
        public const string default_username = "user";
        public const string default_email = "user@secureapi.com";
        public const string default_password = "Pa$$w0rd.";
        public const Roles default_role = Roles.ALL;
    }
}