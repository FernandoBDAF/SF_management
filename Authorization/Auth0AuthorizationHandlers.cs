using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using SFManagement.Services;
using System.Security.Claims;

namespace SFManagement.Authorization;

public class RoleAuthorizationHandler : AuthorizationHandler<RoleRequirement>
{
    private readonly ILogger<RoleAuthorizationHandler> _logger;
    private readonly ILoggingService _loggingService;

    public RoleAuthorizationHandler(ILogger<RoleAuthorizationHandler> logger, ILoggingService loggingService)
    {
        _logger = logger;
        _loggingService = loggingService;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
        var userEmail = context.User.FindFirst(ClaimTypes.Email)?.Value ?? "unknown";
        var roles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        
        if (roles.Contains(requirement.Role))
        {
            _logger.LogInformation("Role authorization granted: User {UserId} ({UserEmail}) has role {RequiredRole}. User roles: {UserRoles}", 
                userId, userEmail, requirement.Role, string.Join(", ", roles));
            
            _loggingService.LogAuthorizationEvent("role", requirement.Role, true);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("Role authorization denied: User {UserId} ({UserEmail}) lacks role {RequiredRole}. User roles: {UserRoles}", 
                userId, userEmail, requirement.Role, string.Join(", ", roles));
            
            _loggingService.LogAuthorizationEvent("role", requirement.Role, false, $"User has roles: {string.Join(", ", roles)}");
        }
        
        return Task.CompletedTask;
    }
}

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ILogger<PermissionAuthorizationHandler> _logger;
    private readonly ILoggingService _loggingService;

    public PermissionAuthorizationHandler(ILogger<PermissionAuthorizationHandler> logger, ILoggingService loggingService)
    {
        _logger = logger;
        _loggingService = loggingService;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
        var userEmail = context.User.FindFirst(ClaimTypes.Email)?.Value ?? "unknown";
        var permissions = context.User.FindAll("permissions").Select(c => c.Value).ToList();
        
        if (permissions.Contains(requirement.Permission))
        {
            _logger.LogInformation("Permission authorization granted: User {UserId} ({UserEmail}) has permission {RequiredPermission}", 
                userId, userEmail, requirement.Permission);
            
            _loggingService.LogAuthorizationEvent("permission", requirement.Permission, true);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("Permission authorization denied: User {UserId} ({UserEmail}) lacks permission {RequiredPermission}. User permissions: {UserPermissions}", 
                userId, userEmail, requirement.Permission, string.Join(", ", permissions));
            
            _loggingService.LogAuthorizationEvent("permission", requirement.Permission, false, $"User has permissions: {string.Join(", ", permissions)}");
        }
        
        return Task.CompletedTask;
    }
}

public class RoleRequirement : IAuthorizationRequirement
{
    public string Role { get; }
    
    public RoleRequirement(string role)
    {
        Role = role;
    }
}

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }
    
    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
} 