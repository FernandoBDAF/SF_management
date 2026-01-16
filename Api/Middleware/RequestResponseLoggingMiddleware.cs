using SFManagement.Infrastructure.Logging;
using SFManagement.Domain.Exceptions;
using System.Text;
using System.Text.Json;

namespace SFManagement.Api.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = context.TraceIdentifier;
        var method = context.Request.Method;
        var path = context.Request.Path;
        var queryString = context.Request.QueryString.ToString();
        
        // Enable request body buffering to read it multiple times
        context.Request.EnableBuffering();
        
        // Read request body
        var requestBody = await ReadRequestBodyAsync(context.Request);
        
        // Log request details
        LogRequestDetails(context, requestId, method, path, queryString, requestBody);
        
        // Capture response
        var originalResponseBodyStream = context.Response.Body;
        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;
        
        try
        {
            await _next(context);
        }
        finally
        {
            // Log response details
            await LogResponseDetailsAsync(context, requestId, method, path, responseBodyStream, originalResponseBodyStream);
        }
    }

    private async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        try
        {
            request.Body.Position = 0;
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            return body;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read request body");
            return "[Unable to read request body]";
        }
    }

    private void LogRequestDetails(HttpContext context, string requestId, string method, string path, string queryString, string requestBody)
    {
        var userId = context.User?.Identity?.Name ?? "anonymous";
        var userAgent = context.Request.Headers["User-Agent"].ToString();
        var contentType = context.Request.ContentType ?? "none";
        var contentLength = context.Request.ContentLength ?? 0;
        
        // Get relevant headers (excluding sensitive ones)
        var headers = context.Request.Headers
            .Where(h => !IsSensitiveHeader(h.Key))
            .ToDictionary(h => h.Key, h => h.Value.ToString());

        var requestInfo = new
        {
            RequestId = requestId,
            Method = method,
            Path = path,
            QueryString = queryString,
            UserId = userId,
            UserAgent = userAgent,
            ContentType = contentType,
            ContentLength = contentLength,
            Headers = headers,
            Body = SanitizeRequestBody(requestBody, contentType)
        };

        _logger.LogInformation("Request: {RequestInfo}", JsonSerializer.Serialize(requestInfo, new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }

    private async Task LogResponseDetailsAsync(HttpContext context, string requestId, string method, string path, 
        MemoryStream responseBodyStream, Stream originalResponseBodyStream)
    {
        var statusCode = context.Response.StatusCode;
        var contentType = context.Response.ContentType ?? "none";
        
        // Read response body
        responseBodyStream.Position = 0;
        var responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();
        
        // Copy response back to original stream
        responseBodyStream.Position = 0;
        await responseBodyStream.CopyToAsync(originalResponseBodyStream);
        
        var responseInfo = new
        {
            RequestId = requestId,
            Method = method,
            Path = path,
            StatusCode = statusCode,
            ContentType = contentType,
            ContentLength = responseBodyStream.Length,
            Body = SanitizeResponseBody(responseBody, contentType, statusCode)
        };

        var logLevel = statusCode >= 400 ? LogLevel.Warning : LogLevel.Information;
        var logMessage = statusCode >= 400 ? "Error Response" : "Response";
        
        _logger.Log(logLevel, "{LogMessage}: {ResponseInfo}", logMessage, JsonSerializer.Serialize(responseInfo, new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));

        // Special handling for 400 errors
        if (statusCode == 400)
        {
            LogBadRequestDetails(context, requestId, method, path, responseBody);
        }
    }

    private void LogBadRequestDetails(HttpContext context, string requestId, string method, string path, string responseBody)
    {
        var userId = context.User?.Identity?.Name ?? "anonymous";
        var userEmail = context.User?.Claims?.FirstOrDefault(c => c.Type == "email")?.Value ?? "unknown";
        
        // Parse validation errors from response if possible
        var validationErrors = ExtractValidationErrors(responseBody);
        
        var badRequestInfo = new
        {
            RequestId = requestId,
            Method = method,
            Path = path,
            UserId = userId,
            UserEmail = userEmail,
            ValidationErrors = validationErrors,
            FullResponse = responseBody,
            Timestamp = DateTime.UtcNow
        };

        _logger.LogError("Bad Request (400) Details: {BadRequestInfo}", JsonSerializer.Serialize(badRequestInfo, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }

    private static bool IsSensitiveHeader(string headerName)
    {
        var sensitiveHeaders = new[] { "authorization", "cookie", "x-api-key", "x-auth-token" };
        return sensitiveHeaders.Contains(headerName.ToLowerInvariant());
    }

    private static string SanitizeRequestBody(string body, string contentType)
    {
        if (string.IsNullOrEmpty(body))
            return "[Empty]";

        if (body.Length > 10000)
            return $"[Body too large: {body.Length} characters]";

        // Remove sensitive fields from JSON
        if (contentType?.Contains("application/json") == true)
        {
            return SanitizeJsonBody(body);
        }

        return body;
    }

    private static string SanitizeResponseBody(string body, string contentType, int statusCode)
    {
        if (string.IsNullOrEmpty(body))
            return "[Empty]";

        if (body.Length > 10000)
            return $"[Body too large: {body.Length} characters]";

        // For error responses, include full body for debugging
        if (statusCode >= 400)
            return body;

        // For successful responses, truncate if needed
        if (body.Length > 1000)
            return body.Substring(0, 1000) + "... [truncated]";

        return body;
    }

    private static string SanitizeJsonBody(string jsonBody)
    {
        try
        {
            var jsonDoc = JsonDocument.Parse(jsonBody);
            var sanitized = SanitizeJsonElement(jsonDoc.RootElement);
            return JsonSerializer.Serialize(sanitized, new JsonSerializerOptions { WriteIndented = false });
        }
        catch
        {
            return jsonBody; // Return original if parsing fails
        }
    }

    private static object SanitizeJsonElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                var obj = new Dictionary<string, object>();
                foreach (var prop in element.EnumerateObject())
                {
                    if (IsSensitiveProperty(prop.Name))
                        obj[prop.Name] = "[REDACTED]";
                    else
                        obj[prop.Name] = SanitizeJsonElement(prop.Value);
                }
                return obj;
            
            case JsonValueKind.Array:
                return element.EnumerateArray().Select(SanitizeJsonElement).ToArray();
            
            case JsonValueKind.String:
                return element.GetString() ?? string.Empty;
            
            case JsonValueKind.Number:
                return element.GetDecimal();
            
            case JsonValueKind.True:
            case JsonValueKind.False:
                return element.GetBoolean();
            
            case JsonValueKind.Null:
                return null;
            
            default:
                return element.ToString();
        }
    }

    private static bool IsSensitiveProperty(string propertyName)
    {
        var sensitiveProps = new[] { "password", "token", "secret", "key", "auth", "credential" };
        return sensitiveProps.Any(prop => propertyName.ToLowerInvariant().Contains(prop));
    }

    private static Dictionary<string, object> ExtractValidationErrors(string responseBody)
    {
        try
        {
            var jsonDoc = JsonDocument.Parse(responseBody);
            var errors = new Dictionary<string, object>();

            // Check for ASP.NET Core ValidationProblemDetails format
            if (jsonDoc.RootElement.TryGetProperty("errors", out var errorsElement))
            {
                foreach (var error in errorsElement.EnumerateObject())
                {
                    errors[error.Name] = error.Value.EnumerateArray().Select(e => e.GetString() ?? string.Empty).ToArray();
                }
            }
            
            // Check for custom error format
            if (jsonDoc.RootElement.TryGetProperty("error", out var errorElement))
            {
                errors["error"] = errorElement.GetString() ?? string.Empty;
            }

            // Check for detail property
            if (jsonDoc.RootElement.TryGetProperty("detail", out var detailElement))
            {
                errors["detail"] = detailElement.GetString() ?? string.Empty;
            }

            return errors;
        }
        catch
        {
            return new Dictionary<string, object> { { "raw", responseBody } };
        }
    }
}

public static class RequestResponseLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestResponseLoggingMiddleware>();
    }
} 