using System.Diagnostics;

namespace MosquitoApi.Api.Middleware;

/// <summary>
/// Middleware for logging HTTP requests and responses
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip logging for health checks and static files
        if (ShouldSkipLogging(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? context.TraceIdentifier;

        // Log request
        _logger.LogInformation(
            "HTTP Request {Method} {Path} started. CorrelationId: {CorrelationId}, UserAgent: {UserAgent}, RemoteIP: {RemoteIP}",
            context.Request.Method,
            context.Request.Path.Value,
            correlationId,
            context.Request.Headers.UserAgent.FirstOrDefault(),
            GetClientIpAddress(context));

        // Capture original response body stream
        var originalBodyStream = context.Response.Body;

        try
        {
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            stopwatch.Stop();

            // Log response
            _logger.LogInformation(
                "HTTP Response {Method} {Path} completed in {ElapsedMilliseconds}ms. Status: {StatusCode}, CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path.Value,
                stopwatch.ElapsedMilliseconds,
                context.Response.StatusCode,
                correlationId);

            // Log slow requests as warnings
            if (stopwatch.ElapsedMilliseconds > 5000) // 5 seconds
            {
                _logger.LogWarning(
                    "Slow request detected: {Method} {Path} took {ElapsedMilliseconds}ms. CorrelationId: {CorrelationId}",
                    context.Request.Method,
                    context.Request.Path.Value,
                    stopwatch.ElapsedMilliseconds,
                    correlationId);
            }

            // Copy response back to original stream
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex,
                "HTTP Request {Method} {Path} failed after {ElapsedMilliseconds}ms. CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path.Value,
                stopwatch.ElapsedMilliseconds,
                correlationId);

            throw;
        }
        finally
        {
            // Restore original response body stream
            context.Response.Body = originalBodyStream;
        }
    }

    /// <summary>
    /// Determines if logging should be skipped for the given path
    /// </summary>
    private static bool ShouldSkipLogging(PathString path)
    {
        var pathValue = path.Value?.ToLowerInvariant() ?? string.Empty;

        return pathValue.StartsWith("/health") ||
               pathValue.StartsWith("/metrics") ||
               pathValue.StartsWith("/favicon.ico") ||
               pathValue.StartsWith("/swagger") ||
               pathValue.StartsWith("/scalar") ||
               pathValue.StartsWith("/openapi") ||
               pathValue.Contains("ping");
    }

    /// <summary>
    /// Gets the client IP address from the HTTP context
    /// </summary>
    private static string GetClientIpAddress(HttpContext context)
    {
        // Check for forwarded headers first (load balancer, proxy, etc.)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // X-Forwarded-For can contain multiple IPs, take the first one
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fall back to connection remote IP
        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}