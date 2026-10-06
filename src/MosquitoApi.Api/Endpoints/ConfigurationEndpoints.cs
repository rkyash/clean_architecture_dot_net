using Microsoft.AspNetCore.Authorization;
using MosquitoApi.Application.Services;
using MosquitoApi.Domain.Configuration;

namespace MosquitoApi.Api.Endpoints;

/// <summary>
/// Configuration endpoints for testing and debugging configuration access
/// </summary>
public static class ConfigurationEndpoints
{
    /// <summary>
    /// Registers configuration endpoints
    /// </summary>
    public static void RegisterConfigurationEndpoints(this WebApplication app)
    {
        var configGroup = app.MapGroup("api/config")
            .WithTags("Configuration")
            .WithDescription("Configuration access endpoints for testing and debugging")
            .WithOpenApi();

        // Get JWT configuration (Admin only for security)
        configGroup.MapGet("jwt", GetJwtConfigurationAsync)
            .WithName("GetJwtConfiguration")
            .WithSummary("Get JWT configuration")
            .WithDescription("Gets JWT configuration settings (excluding secret key)")
            .Produces<object>(StatusCodes.Status200OK)
            .RequireAuthorization("AdminOnly");

        // Get application configuration
        configGroup.MapGet("app", GetApplicationConfigurationAsync)
            .WithName("GetApplicationConfiguration")
            .WithSummary("Get application configuration")
            .WithDescription("Gets application configuration summary")
            .Produces<object>(StatusCodes.Status200OK)
            .RequireAuthorization("AdminOnly");

        // Get environment information
        configGroup.MapGet("environment", GetEnvironmentInfoAsync)
            .WithName("GetEnvironmentInfo")
            .WithSummary("Get environment information")
            .WithDescription("Gets current environment and feature status")
            .Produces<object>(StatusCodes.Status200OK)
            .RequireAuthorization("AdminOnly");

        // Check if feature is enabled (public endpoint for frontend)
        configGroup.MapGet("features/{featureName}", CheckFeatureAsync)
            .WithName("CheckFeature")
            .WithSummary("Check if feature is enabled")
            .WithDescription("Checks if a specific feature is enabled")
            .Produces<bool>(StatusCodes.Status200OK)
            .AllowAnonymous();

        // Get password policy (public for registration forms)
        configGroup.MapGet("password-policy", GetPasswordPolicyAsync)
            .WithName("GetPasswordPolicy")
            .WithSummary("Get password policy")
            .WithDescription("Gets password complexity requirements")
            .Produces<object>(StatusCodes.Status200OK)
            .AllowAnonymous();

        // Get file upload limits (public for upload forms)
        configGroup.MapGet("upload-limits", GetUploadLimitsAsync)
            .WithName("GetUploadLimits")
            .WithSummary("Get file upload limits")
            .WithDescription("Gets file upload size and type restrictions")
            .Produces<object>(StatusCodes.Status200OK)
            .AllowAnonymous();

        // Get configuration health status
        configGroup.MapGet("health", GetConfigurationHealthAsync)
            .WithName("GetConfigurationHealth")
            .WithSummary("Get configuration health")
            .WithDescription("Gets configuration validation status")
            .Produces<object>(StatusCodes.Status200OK)
            .RequireAuthorization("AdminOnly");

        // Static configuration access demonstration
        configGroup.MapGet("static-demo", GetStaticConfigurationDemoAsync)
            .WithName("GetStaticConfigurationDemo")
            .WithSummary("Static configuration demo")
            .WithDescription("Demonstrates static configuration access without DI")
            .Produces<object>(StatusCodes.Status200OK)
            .RequireAuthorization("AdminOnly");
    }

    /// <summary>
    /// Get JWT configuration endpoint
    /// </summary>
    private static async Task<IResult> GetJwtConfigurationAsync(
        IConfigurationService configurationService,
        CancellationToken cancellationToken)
    {
        var config = await configurationService.GetJwtConfigurationAsync();
        return Results.Ok(config);
    }

    /// <summary>
    /// Get application configuration endpoint
    /// </summary>
    private static async Task<IResult> GetApplicationConfigurationAsync(
        IConfigurationService configurationService,
        CancellationToken cancellationToken)
    {
        var config = await configurationService.GetApplicationConfigurationAsync();
        return Results.Ok(config);
    }

    /// <summary>
    /// Get environment information endpoint
    /// </summary>
    private static async Task<IResult> GetEnvironmentInfoAsync(
        IConfigurationService configurationService,
        CancellationToken cancellationToken)
    {
        var envInfo = await configurationService.GetEnvironmentInfoAsync();
        return Results.Ok(envInfo);
    }

    /// <summary>
    /// Check feature endpoint
    /// </summary>
    private static async Task<IResult> CheckFeatureAsync(
        string featureName,
        CancellationToken cancellationToken)
    {
        var isEnabled = ConfigurationHelper.IsFeatureEnabled(featureName);
        
        return await Task.FromResult(Results.Ok(new
        {
            FeatureName = featureName,
            IsEnabled = isEnabled,
            CheckedAt = DateTime.UtcNow
        }));
    }

    /// <summary>
    /// Get password policy endpoint
    /// </summary>
    private static async Task<IResult> GetPasswordPolicyAsync(CancellationToken cancellationToken)
    {
        var policy = ConfigurationHelper.GetPasswordPolicy();
        
        return await Task.FromResult(Results.Ok(new
        {
            policy.MinimumLength,
            policy.MaximumLength,
            policy.RequireUppercase,
            policy.RequireLowercase,
            policy.RequireDigit,
            policy.RequireSpecialCharacter
        }));
    }

    /// <summary>
    /// Get upload limits endpoint
    /// </summary>
    private static async Task<IResult> GetUploadLimitsAsync(CancellationToken cancellationToken)
    {
        var fileSettings = AppSettingsManager.FileUploadSettings;
        
        return await Task.FromResult(Results.Ok(new
        {
            MaxFileSizeBytes = fileSettings.MaxFileSizeBytes,
            MaxFileSizeMB = fileSettings.MaxFileSizeBytes / (1024 * 1024),
            AllowedExtensions = fileSettings.AllowedExtensions,
            UseCloudStorage = fileSettings.UseCloudStorage,
            UploadPath = fileSettings.UploadPath
        }));
    }

    /// <summary>
    /// Get configuration health endpoint
    /// </summary>
    private static async Task<IResult> GetConfigurationHealthAsync(CancellationToken cancellationToken)
    {
        var health = new
        {
            ConfigurationInitialized = AppSettingsManager.IsInitialized,
            Environment = AppSettingsManager.Environment,
            
            DatabaseConnections = new
            {
                HasDefaultConnection = !string.IsNullOrEmpty(AppSettingsManager.GetConnectionString("default")),
                HasAuditConnection = !string.IsNullOrEmpty(AppSettingsManager.GetConnectionString("audit")),
                HasRedisConnection = !string.IsNullOrEmpty(AppSettingsManager.GetConnectionString("redis"))
            },
            
            Security = new
            {
                JwtConfigured = !string.IsNullOrEmpty(AppSettingsManager.JwtSettings.SecretKey),
                JwtSecretKeyLength = AppSettingsManager.JwtSettings.SecretKey?.Length ?? 0,
                CorsConfigured = AppSettingsManager.CorsSettings.AllowedOrigins.Any()
            },
            
            Features = new
            {
                EmailEnabled = AppSettingsManager.EmailSettings.Enabled,
                CacheEnabled = AppSettingsManager.CacheSettings.Enabled,
                FileUploadEnabled = AppSettingsManager.FileUploadSettings.MaxFileSizeBytes > 0,
                CloudStorageEnabled = AppSettingsManager.FileUploadSettings.UseCloudStorage,
                EmailVerificationRequired = AppSettingsManager.ApplicationSettings.UserAccount.RequireEmailVerification
            },
            
            Limits = new
            {
                MaxPageSize = AppSettingsManager.ApplicationSettings.MaxPageSize,
                DefaultPageSize = AppSettingsManager.ApplicationSettings.DefaultPageSize,
                GlobalRateLimit = AppSettingsManager.RateLimitingSettings.GlobalLimitPerMinute,
                ApiRateLimit = AppSettingsManager.RateLimitingSettings.ApiLimitPerMinute,
                MaxFileUploadSize = AppSettingsManager.FileUploadSettings.MaxFileSizeBytes
            },
            
            CheckedAt = DateTime.UtcNow
        };

        return await Task.FromResult(Results.Ok(health));
    }

    /// <summary>
    /// Static configuration demo endpoint
    /// </summary>
    private static async Task<IResult> GetStaticConfigurationDemoAsync(CancellationToken cancellationToken)
    {
        // This demonstrates how to access configuration from anywhere without DI
        var demo = new
        {
            Message = "This data is accessed using static AppSettingsManager from anywhere in the application",
            
            // Direct property access
            ApplicationName = AppSettingsManager.ApplicationSettings.Name,
            ApplicationVersion = AppSettingsManager.ApplicationSettings.Version,
            Environment = AppSettingsManager.Environment,
            
            // Environment checks
            IsDevelopment = AppSettingsManager.IsDevelopment,
            IsProduction = AppSettingsManager.IsProduction,
            
            // Connection strings
            HasDatabaseConnection = !string.IsNullOrEmpty(ConfigurationHelper.GetConnectionString("default")),
            
            // Feature checks
            EmailEnabled = ConfigurationHelper.IsFeatureEnabled("email"),
            CacheEnabled = ConfigurationHelper.IsFeatureEnabled("cache"),
            
            // Typed value access
            MaxPageSize = ConfigurationHelper.GetValue("Application:MaxPageSize", 20),
            DebugMode = ConfigurationHelper.GetValue("Application:DebugMode", false),
            
            // Complex object access
            PasswordPolicy = ConfigurationHelper.GetPasswordPolicy(),
            TokenExpiration = ConfigurationHelper.GetTokenExpirationSettings(),
            AllowedFileExtensions = ConfigurationHelper.GetAllowedFileExtensions(),
            
            // Custom key access (supports nested keys)
            JwtIssuer = AppSettingsManager.GetValue("JwtSettings:Issuer"),
            RateLimitGlobal = AppSettingsManager.GetValue<int>("RateLimiting:GlobalLimitPerMinute", 100),
            
            AccessedAt = DateTime.UtcNow,
            
            Note = "All this configuration access works from any class, static method, or service without dependency injection"
        };

        return await Task.FromResult(Results.Ok(demo));
    }
}