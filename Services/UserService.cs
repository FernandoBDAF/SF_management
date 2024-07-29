using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SFManagement.Models;
using SFManagement.Settings;

namespace SFManagement.Services
{
    public class UserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly JWT _jwt;

        public UserService(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IOptions<JWT> jwt)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwt = jwt.Value;
        }

        public async Task<ApplicationUser> RegisterAsync(ViewModels.RegisterRequest model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                Name = model.Name,
            };

            var userWithSameEmail = await _userManager.FindByEmailAsync(model.Email);

            if (userWithSameEmail == null)
            {
                var result = await _userManager.CreateAsync(user, model.Password);
                
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, Authorization.default_role.ToString());
                    return user;
                }
                else
                {
                    throw new AppException(result.Errors.ToString());
                }
            }
            else
            {
                throw new AppException($"Email {user.Email} is already registered.");
            }
        }
    }
}
