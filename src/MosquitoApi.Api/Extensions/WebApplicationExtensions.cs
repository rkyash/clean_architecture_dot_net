using MosquitoApi.Api.Endpoints;
using MosquitoApi.Api.Middleware;
using Scalar.AspNetCore;

namespace MosquitoApi.Api.Extensions;

/// <summary>
/// Extension methods for configuring the web application pipeline and endpoints
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configures the HTTP request pipeline
    /// </summary>
    /// <param name="app">Web application</param>
    /// <returns>Task representing the async operation</returns>
    public static async Task<WebApplication> ConfigurePipelineAsync(this WebApplication app)
    {
        // Add custom middleware in the correct order
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        // Add security headers
        app.UseSecurityHeaders();

        // Configure OpenAPI/Swagger in development
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            // Configure Scalar UI (modern alternative to Swagger UI)
            app.MapScalarApiReference(options => options
                .WithTitle("Mosquito App API")
                .WithTheme(ScalarTheme.Kepler)
                .WithDarkMode()
                .AddPreferredSecuritySchemes("Bearer")
                .WithLayout(ScalarLayout.Modern)
                .WithModels(false));
        }

        // Add CORS
        app.UseCors(app.Environment.IsDevelopment() ? "Development" : "DefaultPolicy");

        // Add rate limiting
        app.UseRateLimiter();

        // Add HTTPS redirection
        app.UseHttpsRedirection();

        // Add authentication and authorization
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }

    /// <summary>
    /// Configures API endpoints
    /// </summary>
    /// <param name="app">Web application</param>
    /// <returns>Web application for chaining</returns>
    public static WebApplication ConfigureEndpoints(this WebApplication app)
    {
        // Map health check endpoints
        app.MapHealthCheckEndpoints();

        // Map mosquito endpoints
        app.MapMosquitoEndpoints();

        // Map authentication endpoints
        app.RegisterAuthEndpoints();

        // Map user management endpoints
        app.RegisterUserEndpoints();

        // Map configuration endpoints
        app.RegisterConfigurationEndpoints();

        // Map audit endpoints (optional)
        // app.MapAuditEndpoints();

        return app;
    }

    /// <summary>
    /// Adds security headers middleware
    /// </summary>
    private static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            // Add security headers
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            
            // Add Content Security Policy
            context.Response.Headers.Append("Content-Security-Policy", 
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                "style-src 'self' 'unsafe-inline'; " +
                "img-src 'self' data: https:; " +
                "font-src 'self' data:; " +
                "connect-src 'self'");

            await next();
        });
    }
}