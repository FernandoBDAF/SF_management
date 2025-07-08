using SFManagement.Authorization;

namespace SFManagement.Services;

public interface ILoggingService
{
    void LogUserAction(string action, string resource, object? data = null, LogLevel level = LogLevel.Information);
    void LogSecurityEvent(string eventType, string details, LogLevel level = LogLevel.Warning);
    void LogDataAccess(string operation, string entityType, Guid? entityId = null, object? changes = null);
    void LogFinancialOperation(string operation, decimal amount, string currency, Guid? clientId = null);
    void LogAuthenticationEvent(string eventType, string userId, bool success, string? reason = null);
    void LogAuthorizationEvent(string resource, string action, bool granted, string? reason = null);
}

public class LoggingService : ILoggingService
{
    private readonly ILogger<LoggingService> _logger;
    private readonly IAuth0UserService _auth0UserService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LoggingService(ILogger<LoggingService> logger, IAuth0UserService auth0UserService, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _auth0UserService = auth0UserService;
        _httpContextAccessor = httpContextAccessor;
    }

    public void LogUserAction(string action, string resource, object? data = null, LogLevel level = LogLevel.Information)
    {
        var userContext = GetUserContext();
        
        _logger.Log(level, "User Action: {Action} on {Resource} by {UserId} ({UserEmail}) - Roles: {UserRoles} - Data: {@Data}",
            action, resource, userContext.UserId, userContext.UserEmail, userContext.UserRoles, data);
    }

    public void LogSecurityEvent(string eventType, string details, LogLevel level = LogLevel.Warning)
    {
        var userContext = GetUserContext();
        var requestInfo = GetRequestInfo();
        
        _logger.Log(level, "Security Event: {EventType} - {Details} - User: {UserId} ({UserEmail}) - IP: {IpAddress} - UserAgent: {UserAgent}",
            eventType, details, userContext.UserId, userContext.UserEmail, requestInfo.IpAddress, requestInfo.UserAgent);
    }

    public void LogDataAccess(string operation, string entityType, Guid? entityId = null, object? changes = null)
    {
        var userContext = GetUserContext();
        
        _logger.LogInformation("Data Access: {Operation} on {EntityType} {EntityId} by {UserId} ({UserEmail}) - Changes: {@Changes}",
            operation, entityType, entityId, userContext.UserId, userContext.UserEmail, changes);
    }

    public void LogFinancialOperation(string operation, decimal amount, string currency, Guid? clientId = null)
    {
        var userContext = GetUserContext();
        
        _logger.LogInformation("Financial Operation: {Operation} - Amount: {Amount} {Currency} - Client: {ClientId} - User: {UserId} ({UserEmail})",
            operation, amount, currency, clientId, userContext.UserId, userContext.UserEmail);
    }

    public void LogAuthenticationEvent(string eventType, string userId, bool success, string? reason = null)
    {
        var requestInfo = GetRequestInfo();
        
        var logLevel = success ? LogLevel.Information : LogLevel.Warning;
        
        _logger.Log(logLevel, "Authentication Event: {EventType} - User: {UserId} - Success: {Success} - Reason: {Reason} - IP: {IpAddress} - UserAgent: {UserAgent}",
            eventType, userId, success, reason ?? "N/A", requestInfo.IpAddress, requestInfo.UserAgent);
    }

    public void LogAuthorizationEvent(string resource, string action, bool granted, string? reason = null)
    {
        var userContext = GetUserContext();
        var requestInfo = GetRequestInfo();
        
        var logLevel = granted ? LogLevel.Information : LogLevel.Warning;
        
        _logger.Log(logLevel, "Authorization Event: {Action} on {Resource} - Granted: {Granted} - User: {UserId} ({UserEmail}) - Roles: {UserRoles} - Reason: {Reason} - IP: {IpAddress}",
            action, resource, granted, userContext.UserId, userContext.UserEmail, userContext.UserRoles, reason ?? "N/A", requestInfo.IpAddress);
    }

    private UserContext GetUserContext()
    {
        return new UserContext
        {
            UserId = _auth0UserService.GetUserId() ?? "anonymous",
            UserEmail = _auth0UserService.GetUserEmail() ?? "unknown",
            UserRoles = string.Join(", ", _auth0UserService.GetUserRoles()),
            UserPermissions = string.Join(", ", _auth0UserService.GetUserPermissions()),
            IsAuthenticated = _auth0UserService.IsAuthenticated()
        };
    }

    private RequestInfo GetRequestInfo()
    {
        var context = _httpContextAccessor.HttpContext;
        return new RequestInfo
        {
            IpAddress = context?.Connection?.RemoteIpAddress?.ToString() ?? "unknown",
            UserAgent = context?.Request?.Headers["User-Agent"].ToString() ?? "unknown",
            Method = context?.Request?.Method ?? "unknown",
            Path = context?.Request?.Path ?? "unknown",
            QueryString = context?.Request?.QueryString.ToString() ?? ""
        };
    }

    private class UserContext
    {
        public string UserId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserRoles { get; set; } = string.Empty;
        public string UserPermissions { get; set; } = string.Empty;
        public bool IsAuthenticated { get; set; }
    }

    private class RequestInfo
    {
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string QueryString { get; set; } = string.Empty;
    }
} 