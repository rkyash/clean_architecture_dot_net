using MosquitoApi.Api.Configuration;
using Scalar.AspNetCore;

namespace MosquitoApi.Api.Extensions;

/// <summary>
/// Extensions for configuring Scalar API documentation UI
/// </summary>
public static class ScalarExtensions
{
    /// <summary>
    /// Adds Scalar API documentation services
    /// </summary>
    public static IServiceCollection AddScalarDocumentation(this IServiceCollection services)
    {
        // Scalar services are automatically registered when using MapScalarApiReference
        return services;
    }

    /// <summary>
    /// Uses Scalar API documentation UI with configuration
    /// </summary>
    public static IApplicationBuilder UseScalarDocumentation(this IApplicationBuilder app, IConfiguration configuration)
    {
        var scalarSettings = configuration.GetSection(ScalarSettings.SectionName).Get<ScalarSettings>() ?? new ScalarSettings();
        
        if (!scalarSettings.Enabled)
        {
            return app;
        }

        // Scalar configuration is handled in the endpoint mapping
        return app;
    }

    /// <summary>
    /// Maps the Scalar API reference endpoint
    /// </summary>
    public static IEndpointRouteBuilder MapScalarEndpoint(this IEndpointRouteBuilder endpoints, IConfiguration configuration)
    {
        var scalarSettings = configuration.GetSection(ScalarSettings.SectionName).Get<ScalarSettings>() ?? new ScalarSettings();
        
        if (!scalarSettings.Enabled)
        {
            return endpoints;
        }

        // Map the Scalar UI endpoint
        endpoints.MapScalarApiReference(options =>
        {
            options
                .WithTitle(scalarSettings.Title)
                .WithOpenApiRoutePattern(scalarSettings.OpenApiRoute)
                .WithTheme(GetScalarTheme(scalarSettings.Theme))
                .WithSidebar(scalarSettings.ShowSidebar)
                .WithDarkMode(scalarSettings.DarkMode);
        })
        .WithName("scalar")
        .ExcludeFromDescription();

        return endpoints;
    }

    /// <summary>
    /// Gets the Scalar theme from string configuration
    /// </summary>
    private static ScalarTheme GetScalarTheme(string theme)
    {
        return theme?.ToLowerInvariant() switch
        {
            "light" => ScalarTheme.Default,
            "dark" => ScalarTheme.Moon,
            "purple" => ScalarTheme.Purple,
            "blue" => ScalarTheme.BluePlanet,
            "mars" => ScalarTheme.Mars,
            "kepler" => ScalarTheme.Kepler,
            "saturn" => ScalarTheme.Saturn,
            _ => ScalarTheme.Purple
        };
    }

    /// <summary>
    /// Configures advanced Scalar options
    /// </summary>
    private static void ConfigureAdvancedOptions(ScalarOptions options, ScalarSettings settings)
    {
        // Configure metadata
        if (!string.IsNullOrWhiteSpace(settings.Description))
        {
            // Add description to configuration
            options.WithMetaData("description", settings.Description);
        }

        // Configure favicon
        if (!string.IsNullOrWhiteSpace(settings.FaviconUrl))
        {
            options.WithFavicon(settings.FaviconUrl);
        }

        // Additional advanced configurations can be added here
        // based on the ScalarSettings properties
        
        // Enable/disable features based on settings
        if (!settings.InteractiveMode)
        {
            // Configure for read-only mode if needed
        }

        if (settings.DefaultExpandLevel > 0)
        {
            // Configure default expand level if supported
        }
    }

    /// <summary>
    /// Extension method to add metadata to Scalar options
    /// </summary>
    private static ScalarOptions WithMetaData(this ScalarOptions options, string name, string content)
    {
        // This would need to be implemented based on Scalar's actual API
        // Currently using as a placeholder for future enhancements
        return options;
    }

    /// <summary>
    /// Extension method to add favicon to Scalar options
    /// </summary>
    private static ScalarOptions WithFavicon(this ScalarOptions options, string faviconUrl)
    {
        // This would need to be implemented based on Scalar's actual API
        // Currently using as a placeholder for future enhancements
        return options;
    }
}