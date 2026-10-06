namespace MosquitoApi.Api.Middleware;

/// <summary>
/// Middleware for handling correlation IDs to track requests across the system
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeaderName = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Get correlation ID from request header or generate a new one
        var correlationId = GetOrCreateCorrelationId(context);

        // Store correlation ID in HttpContext items for use by other middleware/services
        context.Items["CorrelationId"] = correlationId;

        // Add correlation ID to response headers
        context.Response.Headers.Append(CorrelationIdHeaderName, correlationId);

        // Add correlation ID to logging context
        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }

    /// <summary>
    /// Gets the correlation ID from request headers or creates a new one
    /// </summary>
    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        // First, try to get correlation ID from request headers
        var correlationId = context.Request.Headers[CorrelationIdHeaderName].FirstOrDefault();

        // If not found, try alternative header names
        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = context.Request.Headers["X-Request-ID"].FirstOrDefault();
        }

        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = context.Request.Headers["Request-ID"].FirstOrDefault();
        }

        // If still not found, use the trace identifier
        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = context.TraceIdentifier;
        }

        // If trace identifier is not suitable, generate a new GUID
        if (string.IsNullOrEmpty(correlationId) || correlationId.Length < 8)
        {
            correlationId = Guid.NewGuid().ToString("N")[..8]; // Use first 8 characters for brevity
        }

        return correlationId;
    }
}