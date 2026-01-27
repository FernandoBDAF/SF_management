using System.Net;
using System.Security.Claims;
using System.Text.Json;
using SFManagement.Domain.Exceptions;
using SFManagement.Infrastructure.Logging;

namespace SFManagement.Api.Middleware;

public class ErrorHandlerMiddleware
{
    private readonly ILogger<ErrorHandlerMiddleware> _logger;
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;

    public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger, IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception error)
        {
            await HandleExceptionAsync(context, error);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var requestId = context.TraceIdentifier;
        var method = context.Request.Method;
        var path = context.Request.Path;
        var queryString = context.Request.QueryString.ToString();
        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
        var userEmail = context.User?.FindFirst(ClaimTypes.Email)?.Value ?? "unknown";
        var userAgent = context.Request.Headers["User-Agent"].ToString();
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var response = context.Response;
        response.ContentType = "application/json";

        var (statusCode, errorCode, errorMessage, shouldLogAsError) = GetErrorDetails(exception);
        response.StatusCode = statusCode;

        // Create detailed error context
        var errorContext = new
        {
            RequestId = requestId,
            Method = method,
            Path = path,
            QueryString = queryString,
            UserId = userId,
            UserEmail = userEmail,
            UserAgent = userAgent,
            IpAddress = ipAddress,
            ExceptionType = exception.GetType().Name,
            ExceptionMessage = exception.Message,
            StackTrace = _environment.IsDevelopment() ? exception.StackTrace : null,
            InnerException = exception.InnerException?.Message,
            Timestamp = DateTime.UtcNow
        };

        // Log the error with appropriate level
        if (shouldLogAsError)
        {
            _logger.LogError(exception, 
                "Unhandled Exception: {ExceptionType} - {Method} {Path} - User: {UserId} ({UserEmail}) - IP: {IpAddress} - Message: {Message} - RequestId: {RequestId}",
                exception.GetType().Name, method, path, userId, userEmail, ipAddress, exception.Message, requestId);
                
            _logger.LogError("Error Context: {ErrorContext}", JsonSerializer.Serialize(errorContext, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
        }
        else
        {
            _logger.LogWarning(exception,
                "Handled Exception: {ExceptionType} - {Method} {Path} - User: {UserId} ({UserEmail}) - Message: {Message} - RequestId: {RequestId}",
                exception.GetType().Name, method, path, userId, userEmail, exception.Message, requestId);
        }

        // Create response object
        var errorResponse = new
        {
            error = errorMessage,
            code = errorCode,
            requestId = requestId,
            timestamp = DateTime.UtcNow,
            path = path,
            method = method,
            details = _environment.IsDevelopment() ? new
            {
                exceptionType = exception.GetType().Name,
                stackTrace = exception.StackTrace,
                innerException = exception.InnerException?.Message
            } : null
        };

        // Add validation errors if it's a ValidationException
        if (exception is ValidationException validationException)
        {
            var validationResponse = new
            {
                error = errorMessage,
                code = errorCode,
                requestId = requestId,
                timestamp = DateTime.UtcNow,
                path = path,
                method = method,
                validationErrors = validationException.ValidationErrors?.Select(e => new
                {
                    field = e.Field,
                    message = e.Message,
                    code = e.Code
                }).ToArray() ?? Array.Empty<object>(),
                details = _environment.IsDevelopment() ? new
                {
                    exceptionType = exception.GetType().Name,
                    stackTrace = exception.StackTrace
                } : null
            };

            var validationJson = JsonSerializer.Serialize(validationResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = _environment.IsDevelopment()
            });

            await response.WriteAsync(validationJson);
            return;
        }

        var result = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        });

        await response.WriteAsync(result);
    }

    private static (int statusCode, string errorCode, string errorMessage, bool shouldLogAsError) GetErrorDetails(Exception exception)
    {
        return exception switch
        {
            ValidationException validationEx => (
                (int)HttpStatusCode.BadRequest,
                "VALIDATION_ERROR",
                validationEx.Message,
                false // Validation errors are expected, don't log as errors
            ),
            
            EntityNotFoundException notFoundEx => (
                (int)HttpStatusCode.NotFound,
                "ENTITY_NOT_FOUND",
                notFoundEx.Message,
                false // Not found is expected, don't log as errors
            ),
            
            DuplicateEntityException duplicateEx => (
                (int)HttpStatusCode.Conflict,
                "DUPLICATE_ENTITY",
                duplicateEx.Message,
                false // Duplicate entity is expected, don't log as errors
            ),
            
            BusinessRuleException businessRuleEx => (
                (int)HttpStatusCode.Conflict,
                "BUSINESS_RULE_VIOLATION",
                businessRuleEx.Message,
                false // Business rule violations are expected, don't log as errors
            ),
            
            BusinessException businessEx => (
                (int)HttpStatusCode.BadRequest,
                "BUSINESS_ERROR",
                businessEx.Message,
                false // Business errors are expected, don't log as errors
            ),
            
            UnauthorizedAccessException _ => (
                (int)HttpStatusCode.Unauthorized,
                "UNAUTHORIZED",
                "Access denied",
                true // Security issues should be logged as errors
            ),
            
            ArgumentException argEx => (
                (int)HttpStatusCode.BadRequest,
                "INVALID_ARGUMENT",
                argEx.Message,
                false // Argument errors are usually validation issues
            ),
            
            KeyNotFoundException _ => (
                (int)HttpStatusCode.NotFound,
                "NOT_FOUND",
                "The requested resource was not found",
                false // Not found is expected
            ),
            
            TimeoutException _ => (
                (int)HttpStatusCode.RequestTimeout,
                "TIMEOUT",
                "The request timed out",
                true // Timeouts should be logged as errors
            ),
            
            _ => (
                (int)HttpStatusCode.InternalServerError,
                "INTERNAL_ERROR",
                "An unexpected error occurred",
                true // Unknown errors should be logged as errors
            )
        };
    }
}