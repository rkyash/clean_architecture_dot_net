using Microsoft.Extensions.Options;
using MosquitoApi.Domain.Configuration;

namespace MosquitoApi.Api.Extensions;

/// <summary>
/// Extension methods for configuration setup
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Adds application settings to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration instance</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddAppSettings(this IServiceCollection services, IConfiguration configuration)
    {
        // Create and bind AppSettings instance (similar to your Bind pattern)
        var appSettings = new AppSettings();
        configuration.Bind(appSettings);
        
        // Initialize both the singleton and static manager patterns
        AppSettings.UpdateInstance(appSettings);  // Your singleton pattern
        AppSettingsManager.Initialize(appSettings); // Existing static manager pattern

        // Register the AppSettings as a singleton for dependency injection
        services.Configure<AppSettings>(configuration);
        services.AddSingleton(provider => provider.GetRequiredService<IOptions<AppSettings>>().Value);

        // Register individual settings sections for more granular injection
        services.Configure<ConnectionStrings>(configuration.GetSection(nameof(ConnectionStrings)));
        services.Configure<JwtSettings>(configuration.GetSection(nameof(JwtSettings)));
        services.Configure<CorsSettings>(configuration.GetSection(nameof(CorsSettings)));
        services.Configure<ScalarSettings>(configuration.GetSection(nameof(ScalarSettings)));
        services.Configure<HealthCheckSettings>(configuration.GetSection(nameof(HealthCheckSettings)));
        services.Configure<RateLimitingSettings>(configuration.GetSection(nameof(RateLimitingSettings)));
        services.Configure<ApplicationSettings>(configuration.GetSection(nameof(ApplicationSettings)));
        services.Configure<EmailSettings>(configuration.GetSection(nameof(EmailSettings)));
        services.Configure<FileUploadSettings>(configuration.GetSection(nameof(FileUploadSettings)));
        services.Configure<CacheSettings>(configuration.GetSection(nameof(CacheSettings)));

        // Register individual settings as singletons
        services.AddSingleton(provider => provider.GetRequiredService<IOptions<ConnectionStrings>>().Value);
        services.AddSingleton(provider => provider.GetRequiredService<IOptions<JwtSettings>>().Value);
        services.AddSingleton(provider => provider.GetRequiredService<IOptions<CorsSettings>>().Value);
        services.AddSingleton(provider => provider.GetRequiredService<IOptions<ScalarSettings>>().Value);
        services.AddSingleton(provider => provider.GetRequiredService<IOptions<HealthCheckSettings>>().Value);
        services.AddSingleton(provider => provider.GetRequiredService<IOptions<RateLimitingSettings>>().Value);
        services.AddSingleton(provider => provider.GetRequiredService<IOptions<ApplicationSettings>>().Value);
        services.AddSingleton(provider => provider.GetRequiredService<IOptions<EmailSettings>>().Value);
        services.AddSingleton(provider => provider.GetRequiredService<IOptions<FileUploadSettings>>().Value);
        services.AddSingleton(provider => provider.GetRequiredService<IOptions<CacheSettings>>().Value);

        return services;
    }

    /// <summary>
    /// Gets a strongly-typed configuration section
    /// </summary>
    /// <typeparam name="T">Configuration section type</typeparam>
    /// <param name="configuration">Configuration instance</param>
    /// <param name="sectionName">Section name (optional, uses type name if not provided)</param>
    /// <returns>Configuration section instance</returns>
    public static T GetSection<T>(this IConfiguration configuration, string? sectionName = null) where T : new()
    {
        sectionName ??= typeof(T).Name;
        var section = new T();
        configuration.GetSection(sectionName).Bind(section);
        return section;
    }

    /// <summary>
    /// Validates that required configuration values are present
    /// </summary>
    /// <param name="configuration">Configuration instance</param>
    /// <exception cref="InvalidOperationException">Thrown when required configuration is missing</exception>
    public static void ValidateConfiguration(this IConfiguration configuration)
    {
        var errors = new List<string>();

        // Validate connection strings
        var defaultConnection = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(defaultConnection))
        {
            errors.Add("DefaultConnection connection string is required");
        }

        var auditConnection = configuration.GetConnectionString("AuditConnection");
        if (string.IsNullOrEmpty(auditConnection))
        {
            errors.Add("AuditConnection connection string is required");
        }

        // Validate JWT settings
        var jwtSecretKey = configuration["JwtSettings:SecretKey"];
        if (string.IsNullOrEmpty(jwtSecretKey))
        {
            errors.Add("JwtSettings:SecretKey is required");
        }
        else if (jwtSecretKey.Length < 32)
        {
            errors.Add("JwtSettings:SecretKey must be at least 32 characters long");
        }

        var jwtIssuer = configuration["JwtSettings:Issuer"];
        if (string.IsNullOrEmpty(jwtIssuer))
        {
            errors.Add("JwtSettings:Issuer is required");
        }

        var jwtAudience = configuration["JwtSettings:Audience"];
        if (string.IsNullOrEmpty(jwtAudience))
        {
            errors.Add("JwtSettings:Audience is required");
        }

        // Validate application settings
        var appName = configuration["Application:Name"];
        if (string.IsNullOrEmpty(appName))
        {
            errors.Add("Application:Name is required");
        }

        if (errors.Any())
        {
            throw new InvalidOperationException($"Configuration validation failed:\n{string.Join("\n", errors)}");
        }
    }

    /// <summary>
    /// Gets environment-specific configuration file name
    /// </summary>
    /// <param name="environmentName">Environment name</param>
    /// <returns>Configuration file name</returns>
    public static string GetAppSettingsFileName(string environmentName)
    {
        return string.IsNullOrEmpty(environmentName) 
            ? "appsettings.json" 
            : $"appsettings.{environmentName}.json";
    }

    /// <summary>
    /// Adds configuration from multiple sources
    /// </summary>
    /// <param name="builder">Configuration builder</param>
    /// <param name="environment">Environment name</param>
    /// <param name="basePath">Base path for configuration files</param>
    /// <returns>Configuration builder for chaining</returns>
    public static IConfigurationBuilder AddAppConfiguration(
        this IConfigurationBuilder builder, 
        string environment, 
        string? basePath = null)
    {
        if (!string.IsNullOrEmpty(basePath))
        {
            builder.SetBasePath(basePath);
        }

        // Add base appsettings.json
        builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        // Add environment-specific appsettings
        if (!string.IsNullOrEmpty(environment))
        {
            builder.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);
        }

        // Add user secrets in development
        if (environment?.Equals("Development", StringComparison.OrdinalIgnoreCase) == true)
        {
            builder.AddUserSecrets<Program>();
        }

        // Add environment variables
        builder.AddEnvironmentVariables("MOSQUITOAPI_");

        // Add command line arguments if available
        var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
        if (args.Length > 0)
        {
            builder.AddCommandLine(args);
        }

        return builder;
    }

    /// <summary>
    /// Extension method to bind configuration directly to AppSettings singleton (your preferred pattern)
    /// </summary>
    /// <param name="appSettings">AppSettings type</param>
    /// <param name="configuration">Configuration instance</param>
    public static void Bind(this AppSettings appSettings, IConfiguration configuration)
    {
        // Create temporary instance and bind configuration
        var tempSettings = new AppSettings();
        configuration.Bind(tempSettings);
        
        // Update the singleton instance
        AppSettings.UpdateInstance(tempSettings);
    }

    /// <summary>
    /// Static method to bind configuration directly (alternative approach)
    /// </summary>
    /// <param name="configuration">Configuration instance</param>
    public static void BindAppSettings(this IConfiguration configuration)
    {
        var tempSettings = new AppSettings();
        configuration.Bind(tempSettings);
        AppSettings.UpdateInstance(tempSettings);
    }

    /// <summary>
    /// Logs configuration summary
    /// </summary>
    /// <param name="configuration">Configuration instance</param>
    /// <param name="logger">Logger instance</param>
    public static void LogConfigurationSummary(this IConfiguration configuration, ILogger logger)
    {
        try
        {
            logger.LogInformation("=== Configuration Summary ===");
            logger.LogInformation("Environment: {Environment}", 
                configuration["ASPNETCORE_ENVIRONMENT"] ?? "Not Set");
            
            logger.LogInformation("Application Name: {AppName}", 
                configuration["Application:Name"] ?? "Not Set");
            
            logger.LogInformation("Application Version: {AppVersion}", 
                configuration["Application:Version"] ?? "Not Set");
            
            logger.LogInformation("JWT Issuer: {JwtIssuer}", 
                configuration["JwtSettings:Issuer"] ?? "Not Set");
            
            logger.LogInformation("Default Database: {DbStatus}", 
                string.IsNullOrEmpty(configuration.GetConnectionString("DefaultConnection")) ? "Not Configured" : "Configured");
            
            logger.LogInformation("Audit Database: {AuditDbStatus}", 
                string.IsNullOrEmpty(configuration.GetConnectionString("AuditConnection")) ? "Not Configured" : "Configured");
            
            logger.LogInformation("Rate Limiting: Global={GlobalLimit}, API={ApiLimit}", 
                configuration["RateLimiting:GlobalLimitPerMinute"] ?? "Not Set",
                configuration["RateLimiting:ApiLimitPerMinute"] ?? "Not Set");
            
            logger.LogInformation("CORS Origins: {CorsOrigins}", 
                configuration["Cors:AllowedOrigins"] ?? "Not Set");
            
            logger.LogInformation("=============================");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to log configuration summary");
        }
    }
}