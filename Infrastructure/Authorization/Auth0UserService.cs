using SFManagement.Infrastructure.Logging;
using System.Security.Claims;

namespace SFManagement.Infrastructure.Authorization;

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
    private const string RolesClaim = "https://www.semprefichas.com.br/roles";
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
        var email = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
        
        // If email is not in standard claim, try custom Auth0 claim
        if (string.IsNullOrEmpty(email))
        {
            email = _httpContextAccessor.HttpContext?.User?.FindFirst("https://www.semprefichas.com.br/email")?.Value;
        }
        
        return email;
    }

    public string? GetUserName()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
    }

    public List<string> GetUserRoles()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user is null)
        {
            return new List<string>();
        }

        var mappedRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value);
        var namespacedRoles = user.FindAll(RolesClaim).Select(c => c.Value);

        return mappedRoles
            .Concat(namespacedRoles)
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
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
