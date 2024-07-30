using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Models;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<ApplicationUser> RegisterAsync(RegisterRequest model) => await _userService.RegisterAsync(model);

        [AllowAnonymous]
        [HttpPost("token")]
        public async Task<AuthenticationModel> TokenAsync(TokenRequest model) => await _userService.GetTokenAsync(model);
    }
}
