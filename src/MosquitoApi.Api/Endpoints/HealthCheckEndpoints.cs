using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace MosquitoApi.Api.Endpoints;

/// <summary>
/// Extension methods for mapping health check endpoints
/// </summary>
public static class HealthCheckEndpoints
{
    /// <summary>
    /// Maps health check endpoints
    /// </summary>
    /// <param name="app">Web application</param>
    /// <returns>Web application for chaining</returns>
    public static WebApplication MapHealthCheckEndpoints(this WebApplication app)
    {
        // Basic health check endpoint
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = WriteHealthCheckResponse
        })
        .WithName("HealthCheck")
        .WithTags("Health")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Health Check";
            operation.Description = "Returns the health status of the application and its dependencies";
            return operation;
        });

        // Readiness probe endpoint
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready") || check.Tags.Contains("database"),
            ResponseWriter = WriteHealthCheckResponse
        })
        .WithName("ReadinessCheck")
        .WithTags("Health")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Readiness Check";
            operation.Description = "Returns the readiness status of the application (database connectivity, etc.)";
            return operation;
        });

        // Liveness probe endpoint
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false, // Only include checks that are explicitly tagged for liveness
            ResponseWriter = WriteHealthCheckResponse
        })
        .WithName("LivenessCheck")
        .WithTags("Health")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Liveness Check";
            operation.Description = "Returns the liveness status of the application";
            return operation;
        });

        // Detailed health check endpoint (development only)
        if (app.Environment.IsDevelopment())
        {
            app.MapHealthChecks("/health/detailed", new HealthCheckOptions
            {
                ResponseWriter = WriteDetailedHealthCheckResponse,
                AllowCachingResponses = false
            })
            .WithName("DetailedHealthCheck")
            .WithTags("Health", "Development")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Detailed Health Check";
                operation.Description = "Returns detailed health status of all application components (Development only)";
                return operation;
            });
        }

        return app;
    }

    /// <summary>
    /// Writes a simple health check response
    /// </summary>
    private static async Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            duration = report.TotalDuration.TotalMilliseconds
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }

    /// <summary>
    /// Writes a detailed health check response with individual check results
    /// </summary>
    private static async Task WriteDetailedHealthCheckResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            duration = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                duration = entry.Value.Duration.TotalMilliseconds,
                description = entry.Value.Description,
                data = entry.Value.Data,
                tags = entry.Value.Tags,
                exception = entry.Value.Exception?.Message
            })
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}