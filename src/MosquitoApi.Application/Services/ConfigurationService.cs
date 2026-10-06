using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MosquitoApi.Domain.Configuration;

namespace MosquitoApi.Application.Services;

/// <summary>
/// Service to demonstrate various ways to access configuration throughout the application
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Gets JWT configuration information
    /// </summary>
    /// <returns>JWT settings summary</returns>
    Task<object> GetJwtConfigurationAsync();

    /// <summary>
    /// Gets application configuration summary
    /// </summary>
    /// <returns>Configuration summary</returns>
    Task<object> GetApplicationConfigurationAsync();

    /// <summary>
    /// Validates if email is enabled
    /// </summary>
    /// <returns>True if email is enabled</returns>
    bool IsEmailEnabled();

    /// <summary>
    /// Gets maximum file upload size
    /// </summary>
    /// <returns>Maximum file size in bytes</returns>
    long GetMaxFileUploadSize();

    /// <summary>
    /// Gets current environment information
    /// </summary>
    /// <returns>Environment information</returns>
    Task<object> GetEnvironmentInfoAsync();
}

/// <summary>
/// Implementation of configuration service showing different ways to access settings
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly ILogger<ConfigurationService> _logger;
    private readonly AppSettings _appSettings;
    private readonly JwtSettings _jwtSettings;
    private readonly EmailSettings _emailSettings;
    private readonly IOptionsMonitor<ApplicationSettings> _applicationSettingsMonitor;

    public ConfigurationService(
        ILogger<ConfigurationService> logger,
        AppSettings appSettings,
        JwtSettings jwtSettings,
        EmailSettings emailSettings,
        IOptionsMonitor<ApplicationSettings> applicationSettingsMonitor)
    {
        _logger = logger;
        _appSettings = appSettings;
        _jwtSettings = jwtSettings;
        _emailSettings = emailSettings;
        _applicationSettingsMonitor = applicationSettingsMonitor;
    }

    /// <summary>
    /// Method 1: Using injected strongly-typed settings
    /// </summary>
    public async Task<object> GetJwtConfigurationAsync()
    {
        _logger.LogInformation("Getting JWT configuration using injected settings");

        return await Task.FromResult(new
        {
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            AccessTokenExpirationMinutes = _jwtSettings.AccessTokenExpirationMinutes,
            RefreshTokenExpirationDays = _jwtSettings.RefreshTokenExpirationDays,
            ValidateIssuer = _jwtSettings.ValidateIssuer,
            ValidateAudience = _jwtSettings.ValidateAudience,
            ValidateLifetime = _jwtSettings.ValidateLifetime,
            ValidateIssuerSigningKey = _jwtSettings.ValidateIssuerSigningKey,
            ClockSkewMinutes = _jwtSettings.ClockSkewMinutes,
            // Note: We don't expose the SecretKey for security reasons
            HasSecretKey = !string.IsNullOrEmpty(_jwtSettings.SecretKey)
        });
    }

    /// <summary>
    /// Method 2: Using AppSettings injected as a whole
    /// </summary>
    public async Task<object> GetApplicationConfigurationAsync()
    {
        _logger.LogInformation("Getting application configuration using injected AppSettings");

        return await Task.FromResult(new
        {
            Application = new
            {
                _appSettings.Application.Name,
                _appSettings.Application.Version,
                _appSettings.Application.Environment,
                _appSettings.Application.DebugMode,
                _appSettings.Application.MaxPageSize,
                _appSettings.Application.DefaultPageSize
            },
            RateLimiting = new
            {
                _appSettings.RateLimiting.GlobalLimitPerMinute,
                _appSettings.RateLimiting.ApiLimitPerMinute,
                _appSettings.RateLimiting.SearchLimitPerMinute,
                _appSettings.RateLimiting.AuthLimitPerMinute
            },
            HealthChecks = new
            {
                _appSettings.HealthChecks.CheckIntervalSeconds,
                _appSettings.HealthChecks.FailureNotificationsEnabled,
                _appSettings.HealthChecks.TimeoutSeconds
            }
        });
    }

    /// <summary>
    /// Method 3: Using static AppSettingsManager (can be used anywhere)
    /// </summary>
    public bool IsEmailEnabled()
    {
        _logger.LogInformation("Checking if email is enabled using static AppSettingsManager");

        // This can be called from anywhere in the application without DI
        return AppSettingsManager.EmailSettings.Enabled;
    }

    /// <summary>
    /// Method 4: Using static AppSettingsManager with specific property access
    /// </summary>
    public long GetMaxFileUploadSize()
    {
        _logger.LogInformation("Getting max file upload size using static AppSettingsManager");

        // Direct access to nested property
        return AppSettingsManager.FileUploadSettings.MaxFileSizeBytes;
    }

    /// <summary>
    /// Method 5: Using IOptionsMonitor for settings that might change at runtime
    /// </summary>
    public async Task<object> GetEnvironmentInfoAsync()
    {
        _logger.LogInformation("Getting environment info using IOptionsMonitor");

        var currentSettings = _applicationSettingsMonitor.CurrentValue;

        return await Task.FromResult(new
        {
            // From IOptionsMonitor (reactive to changes)
            ApplicationName = currentSettings.Name,
            ApplicationVersion = currentSettings.Version,
            Environment = currentSettings.Environment,
            DebugMode = currentSettings.DebugMode,

            // From static manager
            IsDevelopment = AppSettingsManager.IsDevelopment,
            IsStaging = AppSettingsManager.IsStaging,
            IsProduction = AppSettingsManager.IsProduction,

            // Connection status
            HasDatabaseConnection = !string.IsNullOrEmpty(AppSettingsManager.GetConnectionString("default")),
            HasAuditConnection = !string.IsNullOrEmpty(AppSettingsManager.GetConnectionString("audit")),
            HasRedisConnection = !string.IsNullOrEmpty(AppSettingsManager.GetConnectionString("redis")),

            // Feature flags
            EmailEnabled = _emailSettings.Enabled,
            CacheEnabled = AppSettingsManager.CacheSettings.Enabled,
            FileUploadEnabled = AppSettingsManager.FileUploadSettings.MaxFileSizeBytes > 0,

            // Security settings
            JwtConfigured = !string.IsNullOrEmpty(AppSettingsManager.JwtSettings.SecretKey),
            CorsConfigured = AppSettingsManager.CorsSettings.AllowedOrigins.Any(),

            // Performance settings
            MaxPageSize = currentSettings.MaxPageSize,
            DefaultPageSize = currentSettings.DefaultPageSize,
            GlobalRateLimit = AppSettingsManager.RateLimitingSettings.GlobalLimitPerMinute
        });
    }
}

/// <summary>
/// Example of a utility class that uses AppSettingsManager statically
/// This can be used anywhere without dependency injection
/// </summary>
public static class ConfigurationHelper
{
    /// <summary>
    /// Gets a connection string by name
    /// </summary>
    /// <param name="name">Connection string name</param>
    /// <returns>Connection string</returns>
    public static string GetConnectionString(string name)
    {
        return AppSettingsManager.GetConnectionString(name);
    }

    /// <summary>
    /// Checks if a feature is enabled
    /// </summary>
    /// <param name="featureName">Feature name</param>
    /// <returns>True if feature is enabled</returns>
    public static bool IsFeatureEnabled(string featureName)
    {
        return featureName.ToLowerInvariant() switch
        {
            "email" => AppSettingsManager.EmailSettings.Enabled,
            "cache" => AppSettingsManager.CacheSettings.Enabled,
            "cloudstorage" => AppSettingsManager.FileUploadSettings.UseCloudStorage,
            "emailverification" => AppSettingsManager.ApplicationSettings.UserAccount.RequireEmailVerification,
            "debugmode" => AppSettingsManager.ApplicationSettings.DebugMode,
            _ => false
        };
    }

    /// <summary>
    /// Gets a configuration value with type conversion
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    /// <param name="key">Configuration key</param>
    /// <param name="defaultValue">Default value</param>
    /// <returns>Configuration value</returns>
    public static T GetValue<T>(string key, T defaultValue = default!)
    {
        return AppSettingsManager.GetValue(key, defaultValue);
    }

    /// <summary>
    /// Gets all allowed file extensions for uploads
    /// </summary>
    /// <returns>List of allowed extensions</returns>
    public static List<string> GetAllowedFileExtensions()
    {
        return AppSettingsManager.FileUploadSettings.AllowedExtensions;
    }

    /// <summary>
    /// Checks if current environment is development
    /// </summary>
    /// <returns>True if development environment</returns>
    public static bool IsDevelopmentEnvironment()
    {
        return AppSettingsManager.IsDevelopment;
    }

    /// <summary>
    /// Gets JWT token expiration settings
    /// </summary>
    /// <returns>Token expiration info</returns>
    public static (int AccessTokenMinutes, int RefreshTokenDays) GetTokenExpirationSettings()
    {
        var jwtSettings = AppSettingsManager.JwtSettings;
        return (jwtSettings.AccessTokenExpirationMinutes, jwtSettings.RefreshTokenExpirationDays);
    }

    /// <summary>
    /// Gets password policy settings
    /// </summary>
    /// <returns>Password settings</returns>
    public static PasswordSettings GetPasswordPolicy()
    {
        return AppSettingsManager.ApplicationSettings.Password;
    }

    /// <summary>
    /// Gets user account policy settings
    /// </summary>
    /// <returns>User account settings</returns>
    public static UserAccountSettings GetUserAccountPolicy()
    {
        return AppSettingsManager.ApplicationSettings.UserAccount;
    }
}