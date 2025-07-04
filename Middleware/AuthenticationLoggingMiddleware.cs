using System.Security.Claims;

namespace SFManagement.Middleware;

public class AuthenticationLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationLoggingMiddleware> _logger;
    private readonly IServiceProvider _serviceProvider;

    public AuthenticationLoggingMiddleware(RequestDelegate next, ILogger<AuthenticationLoggingMiddleware> logger, IServiceProvider serviceProvider)
    {
        _next = next;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var originalPath = context.Request.Path;
        var method = context.Request.Method;
        
        // Log authentication status
        var isAuthenticated = context.User?.Identity?.IsAuthenticated ?? false;
        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userEmail = context.User?.FindFirst(ClaimTypes.Email)?.Value;
        
        if (isAuthenticated)
        {
            _logger.LogInformation("Authenticated request: {Method} {Path} by {UserId} ({UserEmail})", 
                method, originalPath, userId, userEmail);
        }
        else
        {
            _logger.LogInformation("Unauthenticated request: {Method} {Path}", method, originalPath);
        }

        // Log JWT token validation issues
        var authorizationHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer "))
        {
            var token = authorizationHeader.Substring("Bearer ".Length);
            LogTokenInfo(token, context);
        }

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing request: {Method} {Path} by {UserId}", 
                method, originalPath, userId ?? "anonymous");
            throw;
        }
    }

    private void LogTokenInfo(string token, HttpContext context)
    {
        try
        {
            // Basic token validation logging (without decoding sensitive data)
            var tokenParts = token.Split('.');
            if (tokenParts.Length == 3)
            {
                _logger.LogDebug("JWT token received for request: {Method} {Path} - Token length: {TokenLength}", 
                    context.Request.Method, context.Request.Path, token.Length);
                
                // Log token expiration if available
                var payload = tokenParts[1];
                var decodedPayload = DecodeBase64Url(payload);
                if (decodedPayload.Contains("exp"))
                {
                    // Extract expiration time for logging
                    var expMatch = System.Text.RegularExpressions.Regex.Match(decodedPayload, @"""exp"":(\d+)");
                    if (expMatch.Success && long.TryParse(expMatch.Groups[1].Value, out var exp))
                    {
                        var expirationTime = DateTimeOffset.FromUnixTimeSeconds(exp);
                        var timeUntilExpiry = expirationTime - DateTimeOffset.UtcNow;
                        
                        if (timeUntilExpiry.TotalMinutes < 5)
                        {
                            _logger.LogWarning("JWT token expires soon: {ExpirationTime} (in {MinutesUntilExpiry} minutes)", 
                                expirationTime, timeUntilExpiry.TotalMinutes);
                        }
                        else
                        {
                            _logger.LogDebug("JWT token expires at: {ExpirationTime}", expirationTime);
                        }
                    }
                }
            }
            else
            {
                _logger.LogWarning("Invalid JWT token format for request: {Method} {Path}", 
                    context.Request.Method, context.Request.Path);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing JWT token for request: {Method} {Path}", 
                context.Request.Method, context.Request.Path);
        }
    }

    private static string DecodeBase64Url(string base64Url)
    {
        var base64 = base64Url.Replace('-', '+').Replace('_', '/');
        var padding = 4 - (base64.Length % 4);
        if (padding != 4)
        {
            base64 += new string('=', padding);
        }
        
        try
        {
            var bytes = Convert.FromBase64String(base64);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return string.Empty;
        }
    }
}

public static class AuthenticationLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseAuthenticationLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthenticationLoggingMiddleware>();
    }
} 