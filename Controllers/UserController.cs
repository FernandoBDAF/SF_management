using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Models;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers;

[ApiController]
[Route("api/v{verion:apiVersion}/[controller]")]
[ApiVersionNeutral]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<List<UserResponse>> GetAsync()
    {
        return await _userService.List();
    }

    [HttpPost("register")]
    public async Task<ApplicationUser> RegisterAsync(RegisterRequest model)
    {
        return await _userService.RegisterAsync(model);
    }

    [AllowAnonymous]
    [HttpPost("token")]
    public async Task<AuthenticationModel> TokenAsync(TokenRequest model)
    {
        return await _userService.GetTokenAsync(model);
    }

    [HttpPost]
    [Route("refresh-token")]
    public async Task<AuthenticationModel> RefreshToken(TokenRequest model)
    {
        return await _userService.RefreshToken(model);
    }

    [AllowAnonymous]
    [HttpPost("addrole")]
    public async Task<string> AddRoleAsync(AddRoleRequest model)
    {
        return await _userService.AddRoleAsync(model);
    }
}