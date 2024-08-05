using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SFManagement.Models;
using SFManagement.Settings;
using SFManagement.ViewModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
                    throw new AppException(string.Join(", ", result.Errors.Select(x => $"{x.Code} - {x.Description}")));
                }
            }
            else
            {
                throw new AppException($"Email {user.Email} is already registered.");
            }
        }

        public async Task<AuthenticationModel> GetTokenAsync(TokenRequest model)
        {
            var authenticationModel = new AuthenticationModel();
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                authenticationModel.IsAuthenticated = false;
                authenticationModel.Message = $"No accounts registered with {model.Email}";

                return authenticationModel;
            }

            if (await _userManager.CheckPasswordAsync(user, model.Password))
            {
                authenticationModel.IsAuthenticated = true;

                JwtSecurityToken jwtSecurityToken = await CreateJwtToken(user);

                authenticationModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
                authenticationModel.Email = user.Email;
                authenticationModel.UserName = user.UserName;

                var roleList = await _userManager.GetRolesAsync(user).ConfigureAwait(false);

                authenticationModel.Roles = roleList.ToList();

                return authenticationModel;
            }

            authenticationModel.IsAuthenticated = false;
            authenticationModel.Message = $"Incorrect credentials for user {user.Email}";

            return authenticationModel;
        }

        public async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            var roleClaims = new List<Claim>();

            for (int i = 0; i < roles.Count; i++)
            {
                roleClaims.Add(new Claim("roles", roles[i]));
            }

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, !string.IsNullOrEmpty(user.Name) ? user.Name : "Generic user"),
                new Claim("uid", user.Id.ToString())
            };

            var symetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signingCredentials = new SigningCredentials(symetricSecurityKey, SecurityAlgorithms.HmacSha256);

            return new JwtSecurityToken(issuer: _jwt.Issuer, audience: _jwt.Audience, claims: claims, expires: DateTime.UtcNow.AddMinutes(_jwt.DurationInMinutes), signingCredentials: signingCredentials);
        }

        public async Task<string> AddRoleAsync(AddRoleRequest model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return $"No accounts registered with {model.Email}";
            }

            if (await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var roleExists = Enum.GetValues(typeof(Authorization.Roles)).Cast<Authorization.Roles>().Any(x => x.ToName<Authorization.Roles>().ToLower() == model.Role.ToLower());

                if (roleExists)
                {
                    var validRole = Enum.GetValues(typeof(Authorization.Roles)).Cast<Authorization.Roles>().Where(x => x.ToString().ToLower() == model.Role.ToLower()).FirstOrDefault();
                    await _userManager.AddToRoleAsync(user, validRole.ToString());
                    return $"Added {model.Role} to user {model.Email}.";
                }
            }

            return $"Incorrect credentials for user {model.Email}.";
        }
    }
}
