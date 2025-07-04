using System.Security.Claims;

namespace SFManagement.Authorization;

public interface IAuth0UserService
{
    string? GetUserId();
    string? GetUserEmail();
    string? GetUserName();
    List<string> GetUserRoles();
    List<string> GetUserPermissions();
    bool IsAuthenticated();
}

public class Auth0UserService : IAuth0UserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Auth0UserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetUserId()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public string? GetUserEmail()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
    }

    public string? GetUserName()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
    }

    public List<string> GetUserRoles()
    {
        var roles = _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role)?.Select(c => c.Value).ToList();
        return roles ?? new List<string>();
    }

    public List<string> GetUserPermissions()
    {
        var permissions = _httpContextAccessor.HttpContext?.User?.FindAll("permissions")?.Select(c => c.Value).ToList();
        return permissions ?? new List<string>();
    }

    public bool IsAuthenticated()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }
}
