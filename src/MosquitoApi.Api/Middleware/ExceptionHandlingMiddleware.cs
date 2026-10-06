using MosquitoApi.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace MosquitoApi.Api.Middleware;

/// <summary>
/// Middleware for global exception handling and error response formatting
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next, 
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred while processing the request");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = exception switch
        {
            MosquitoNotFoundException ex => new ErrorResponse
            {
                Title = "Resource Not Found",
                Status = (int)HttpStatusCode.NotFound,
                Detail = ex.Message,
                Instance = context.Request.Path,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                ErrorCode = ex.ErrorCode,
                TraceId = context.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                Details = ex.Details
            },

            InvalidMosquitoDataException ex => new ErrorResponse
            {
                Title = "Validation Failed",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = ex.Message,
                Instance = context.Request.Path,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                ErrorCode = ex.ErrorCode,
                TraceId = context.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                Details = ex.Details,
                ValidationErrors = ex.ValidationErrors.ToList()
            },

            DomainException ex => new ErrorResponse
            {
                Title = "Domain Error",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = ex.Message,
                Instance = context.Request.Path,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                ErrorCode = ex.ErrorCode,
                TraceId = context.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                Details = ex.Details
            },

            ArgumentException ex => new ErrorResponse
            {
                Title = "Invalid Argument",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = ex.Message,
                Instance = context.Request.Path,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                TraceId = context.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            },

            UnauthorizedAccessException => new ErrorResponse
            {
                Title = "Unauthorized",
                Status = (int)HttpStatusCode.Unauthorized,
                Detail = "You are not authorized to access this resource",
                Instance = context.Request.Path,
                Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                TraceId = context.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            },

            _ => new ErrorResponse
            {
                Title = "Internal Server Error",
                Status = (int)HttpStatusCode.InternalServerError,
                Detail = _environment.IsDevelopment() ? exception.Message : "An error occurred while processing your request",
                Instance = context.Request.Path,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                TraceId = context.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                // Only include stack trace in development
                StackTrace = _environment.IsDevelopment() ? exception.StackTrace : null
            }
        };

        response.StatusCode = errorResponse.Status;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        var jsonResponse = JsonSerializer.Serialize(errorResponse, jsonOptions);
        await response.WriteAsync(jsonResponse);
    }
}

/// <summary>
/// Standard error response model following RFC 7807 Problem Details specification
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// A short, human-readable summary of the problem type
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The HTTP status code
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// A human-readable explanation specific to this occurrence of the problem
    /// </summary>
    public string Detail { get; set; } = string.Empty;

    /// <summary>
    /// A URI reference that identifies the specific occurrence of the problem
    /// </summary>
    public string Instance { get; set; } = string.Empty;

    /// <summary>
    /// A URI reference that identifies the problem type
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Application-specific error code
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Trace identifier for request correlation
    /// </summary>
    public string TraceId { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the error occurred
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Additional error details
    /// </summary>
    public Dictionary<string, object>? Details { get; set; }

    /// <summary>
    /// Validation errors (for validation failures)
    /// </summary>
    public List<string>? ValidationErrors { get; set; }

    /// <summary>
    /// Stack trace (development only)
    /// </summary>
    public string? StackTrace { get; set; }
}