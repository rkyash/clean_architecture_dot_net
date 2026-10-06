namespace MosquitoApi.Domain.Configuration;

/// <summary>
/// Main application settings container with singleton pattern
/// </summary>
public sealed class AppSettings
{
    private static readonly Lazy<AppSettings> _instance = new(() => new AppSettings());
    public static AppSettings Instance => _instance.Value;

    // Public constructor to allow configuration binding, but singleton pattern is still preferred via Instance
    public AppSettings() { }
    /// <summary>
    /// Database connection strings
    /// </summary>
    public ConnectionStrings ConnectionStrings { get; set; } = new();

    /// <summary>
    /// Logging configuration
    /// </summary>
    public LoggingSettings Logging { get; set; } = new();

    /// <summary>
    /// Serilog configuration
    /// </summary>
    public SerilogSettings Serilog { get; set; } = new();

    /// <summary>
    /// JWT authentication settings
    /// </summary>
    public JwtSettings JwtSettings { get; set; } = new();

    /// <summary>
    /// CORS configuration
    /// </summary>
    public CorsSettings Cors { get; set; } = new();

    /// <summary>
    /// Scalar UI configuration
    /// </summary>
    public ScalarSettings Scalar { get; set; } = new();

    /// <summary>
    /// Health checks configuration
    /// </summary>
    public HealthCheckSettings HealthChecks { get; set; } = new();

    /// <summary>
    /// Rate limiting configuration
    /// </summary>
    public RateLimitingSettings RateLimiting { get; set; } = new();

    /// <summary>
    /// Allowed hosts configuration
    /// </summary>
    public string AllowedHosts { get; set; } = "*";

    /// <summary>
    /// Application-specific settings
    /// </summary>
    public ApplicationSettings Application { get; set; } = new();

    /// <summary>
    /// Email configuration
    /// </summary>
    public EmailSettings Email { get; set; } = new();

    /// <summary>
    /// File upload settings
    /// </summary>
    public FileUploadSettings FileUpload { get; set; } = new();

    /// <summary>
    /// Cache settings
    /// </summary>
    public CacheSettings Cache { get; set; } = new();

    /// <summary>
    /// Updates the singleton instance with new configuration values
    /// Note: Binding happens at the API layer to avoid domain dependencies
    /// </summary>
    /// <param name="configuredInstance">Pre-configured AppSettings instance</param>
    public static void UpdateInstance(AppSettings configuredInstance)
    {
        // Copy all properties from the configured instance
        var instance = Instance;
        
        // Copy all configuration sections
        instance.ConnectionStrings = configuredInstance.ConnectionStrings;
        instance.Logging = configuredInstance.Logging;
        instance.Serilog = configuredInstance.Serilog;
        instance.JwtSettings = configuredInstance.JwtSettings;
        instance.Cors = configuredInstance.Cors;
        instance.Scalar = configuredInstance.Scalar;
        instance.HealthChecks = configuredInstance.HealthChecks;
        instance.RateLimiting = configuredInstance.RateLimiting;
        instance.AllowedHosts = configuredInstance.AllowedHosts;
        instance.Application = configuredInstance.Application;
        instance.Email = configuredInstance.Email;
        instance.FileUpload = configuredInstance.FileUpload;
        instance.Cache = configuredInstance.Cache;
    }
}

/// <summary>
/// Database connection strings configuration
/// </summary>
public class ConnectionStrings
{
    /// <summary>
    /// Main database connection string
    /// </summary>
    public string DefaultConnection { get; set; } = string.Empty;

    /// <summary>
    /// Audit database connection string
    /// </summary>
    public string AuditConnection { get; set; } = string.Empty;

    /// <summary>
    /// Redis cache connection string (optional)
    /// </summary>
    public string? RedisConnection { get; set; }
}

/// <summary>
/// Logging configuration
/// </summary>
public class LoggingSettings
{
    /// <summary>
    /// Log levels configuration
    /// </summary>
    public Dictionary<string, string> LogLevel { get; set; } = new();
}

/// <summary>
/// Serilog configuration
/// </summary>
public class SerilogSettings
{
    /// <summary>
    /// Serilog assemblies to use
    /// </summary>
    public List<string> Using { get; set; } = new();

    /// <summary>
    /// Minimum log level configuration
    /// </summary>
    public MinimumLevelSettings MinimumLevel { get; set; } = new();

    /// <summary>
    /// Write to targets configuration
    /// </summary>
    public List<WriteToSettings> WriteTo { get; set; } = new();

    /// <summary>
    /// Enrichers configuration
    /// </summary>
    public List<string> Enrich { get; set; } = new();
}

/// <summary>
/// Serilog minimum level settings
/// </summary>
public class MinimumLevelSettings
{
    /// <summary>
    /// Default minimum level
    /// </summary>
    public string Default { get; set; } = "Information";

    /// <summary>
    /// Override settings for specific namespaces
    /// </summary>
    public Dictionary<string, string> Override { get; set; } = new();
}

/// <summary>
/// Serilog write to settings
/// </summary>
public class WriteToSettings
{
    /// <summary>
    /// Sink name (Console, File, etc.)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Sink arguments
    /// </summary>
    public Dictionary<string, object> Args { get; set; } = new();
}

/// <summary>
/// JWT authentication settings
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// JWT secret key for signing tokens
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// JWT issuer
    /// </summary>
    public string Issuer { get; set; } = "MosquitoApi";

    /// <summary>
    /// JWT audience
    /// </summary>
    public string Audience { get; set; } = "MosquitoApi.Users";

    /// <summary>
    /// Access token expiration in minutes
    /// </summary>
    public int AccessTokenExpirationMinutes { get; set; } = 15;

    /// <summary>
    /// Refresh token expiration in days
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;

    /// <summary>
    /// Whether to validate issuer
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;

    /// <summary>
    /// Whether to validate audience
    /// </summary>
    public bool ValidateAudience { get; set; } = true;

    /// <summary>
    /// Whether to validate token lifetime
    /// </summary>
    public bool ValidateLifetime { get; set; } = true;

    /// <summary>
    /// Whether to validate issuer signing key
    /// </summary>
    public bool ValidateIssuerSigningKey { get; set; } = true;

    /// <summary>
    /// Clock skew in minutes
    /// </summary>
    public int ClockSkewMinutes { get; set; } = 5;
}

/// <summary>
/// CORS configuration
/// </summary>
public class CorsSettings
{
    /// <summary>
    /// Allowed origins for CORS
    /// </summary>
    public List<string> AllowedOrigins { get; set; } = new();

    /// <summary>
    /// Allowed methods for CORS
    /// </summary>
    public List<string> AllowedMethods { get; set; } = new() { "GET", "POST", "PUT", "DELETE", "OPTIONS" };

    /// <summary>
    /// Allowed headers for CORS
    /// </summary>
    public List<string> AllowedHeaders { get; set; } = new() { "*" };

    /// <summary>
    /// Whether to allow credentials
    /// </summary>
    public bool AllowCredentials { get; set; } = true;
}

/// <summary>
/// Scalar UI configuration
/// </summary>
public class ScalarSettings
{
    /// <summary>
    /// API documentation title
    /// </summary>
    public string Title { get; set; } = "Mosquito API Documentation";

    /// <summary>
    /// API description
    /// </summary>
    public string Description { get; set; } = "A comprehensive API for managing mosquito data with Clean Architecture";

    /// <summary>
    /// API version
    /// </summary>
    public string Version { get; set; } = "v1.0.0";

    /// <summary>
    /// UI theme
    /// </summary>
    public string Theme { get; set; } = "purple";

    /// <summary>
    /// Whether to show sidebar
    /// </summary>
    public bool ShowSidebar { get; set; } = true;

    /// <summary>
    /// Whether to hide download button
    /// </summary>
    public bool HideDownloadButton { get; set; } = false;

    /// <summary>
    /// Whether to use dark mode
    /// </summary>
    public bool DarkMode { get; set; } = true;

    /// <summary>
    /// UI layout
    /// </summary>
    public string Layout { get; set; } = "modern";

    /// <summary>
    /// Custom CSS path
    /// </summary>
    public string? CustomCss { get; set; }

    /// <summary>
    /// Server configurations
    /// </summary>
    public List<ServerSettings> Servers { get; set; } = new();
}

/// <summary>
/// Server settings for API documentation
/// </summary>
public class ServerSettings
{
    /// <summary>
    /// Server URL
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Server description
    /// </summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Health checks configuration
/// </summary>
public class HealthCheckSettings
{
    /// <summary>
    /// Health check interval in seconds
    /// </summary>
    public int CheckIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Whether failure notifications are enabled
    /// </summary>
    public bool FailureNotificationsEnabled { get; set; } = true;

    /// <summary>
    /// Timeout for health checks in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}

/// <summary>
/// Rate limiting configuration
/// </summary>
public class RateLimitingSettings
{
    /// <summary>
    /// Global rate limit per minute
    /// </summary>
    public int GlobalLimitPerMinute { get; set; } = 100;

    /// <summary>
    /// API rate limit per minute
    /// </summary>
    public int ApiLimitPerMinute { get; set; } = 50;

    /// <summary>
    /// Search rate limit per minute
    /// </summary>
    public int SearchLimitPerMinute { get; set; } = 20;

    /// <summary>
    /// Authentication rate limit per minute
    /// </summary>
    public int AuthLimitPerMinute { get; set; } = 10;
}

/// <summary>
/// Application-specific settings
/// </summary>
public class ApplicationSettings
{
    /// <summary>
    /// Application name
    /// </summary>
    public string Name { get; set; } = "Mosquito API";

    /// <summary>
    /// Application version
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Environment name
    /// </summary>
    public string Environment { get; set; } = "Development";

    /// <summary>
    /// Whether debug mode is enabled
    /// </summary>
    public bool DebugMode { get; set; } = true;

    /// <summary>
    /// Maximum page size for pagination
    /// </summary>
    public int MaxPageSize { get; set; } = 100;

    /// <summary>
    /// Default page size for pagination
    /// </summary>
    public int DefaultPageSize { get; set; } = 20;

    /// <summary>
    /// Password complexity settings
    /// </summary>
    public PasswordSettings Password { get; set; } = new();

    /// <summary>
    /// User account settings
    /// </summary>
    public UserAccountSettings UserAccount { get; set; } = new();
}

/// <summary>
/// Password complexity settings
/// </summary>
public class PasswordSettings
{
    /// <summary>
    /// Minimum password length
    /// </summary>
    public int MinimumLength { get; set; } = 6;

    /// <summary>
    /// Maximum password length
    /// </summary>
    public int MaximumLength { get; set; } = 100;

    /// <summary>
    /// Whether uppercase letters are required
    /// </summary>
    public bool RequireUppercase { get; set; } = true;

    /// <summary>
    /// Whether lowercase letters are required
    /// </summary>
    public bool RequireLowercase { get; set; } = true;

    /// <summary>
    /// Whether digits are required
    /// </summary>
    public bool RequireDigit { get; set; } = true;

    /// <summary>
    /// Whether special characters are required
    /// </summary>
    public bool RequireSpecialCharacter { get; set; } = true;
}

/// <summary>
/// User account settings
/// </summary>
public class UserAccountSettings
{
    /// <summary>
    /// Maximum failed login attempts before lockout
    /// </summary>
    public int MaxFailedLoginAttempts { get; set; } = 5;

    /// <summary>
    /// Account lockout duration in minutes
    /// </summary>
    public int LockoutDurationMinutes { get; set; } = 30;

    /// <summary>
    /// Whether email verification is required
    /// </summary>
    public bool RequireEmailVerification { get; set; } = true;

    /// <summary>
    /// Email verification token expiration in hours
    /// </summary>
    public int EmailVerificationTokenExpirationHours { get; set; } = 24;

    /// <summary>
    /// Password reset token expiration in hours
    /// </summary>
    public int PasswordResetTokenExpirationHours { get; set; } = 1;
}

/// <summary>
/// Email configuration
/// </summary>
public class EmailSettings
{
    /// <summary>
    /// SMTP server host
    /// </summary>
    public string SmtpHost { get; set; } = string.Empty;

    /// <summary>
    /// SMTP server port
    /// </summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>
    /// SMTP username
    /// </summary>
    public string SmtpUsername { get; set; } = string.Empty;

    /// <summary>
    /// SMTP password
    /// </summary>
    public string SmtpPassword { get; set; } = string.Empty;

    /// <summary>
    /// Whether to use SSL
    /// </summary>
    public bool UseSsl { get; set; } = true;

    /// <summary>
    /// From email address
    /// </summary>
    public string FromEmail { get; set; } = string.Empty;

    /// <summary>
    /// From display name
    /// </summary>
    public string FromName { get; set; } = "Mosquito API";

    /// <summary>
    /// Whether email is enabled
    /// </summary>
    public bool Enabled { get; set; } = false;
}

/// <summary>
/// File upload settings
/// </summary>
public class FileUploadSettings
{
    /// <summary>
    /// Maximum file size in bytes
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024; // 10MB

    /// <summary>
    /// Allowed file extensions
    /// </summary>
    public List<string> AllowedExtensions { get; set; } = new() { ".jpg", ".jpeg", ".png", ".gif", ".pdf" };

    /// <summary>
    /// Upload directory path
    /// </summary>
    public string UploadPath { get; set; } = "uploads";

    /// <summary>
    /// Whether to use cloud storage
    /// </summary>
    public bool UseCloudStorage { get; set; } = false;

    /// <summary>
    /// Cloud storage connection string
    /// </summary>
    public string? CloudStorageConnectionString { get; set; }
}

/// <summary>
/// Cache settings
/// </summary>
public class CacheSettings
{
    /// <summary>
    /// Whether caching is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Default cache expiration in minutes
    /// </summary>
    public int DefaultExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Cache provider type (Memory, Redis, etc.)
    /// </summary>
    public string Provider { get; set; } = "Memory";

    /// <summary>
    /// Redis-specific settings
    /// </summary>
    public RedisCacheSettings? Redis { get; set; }
}

/// <summary>
/// Redis cache settings
/// </summary>
public class RedisCacheSettings
{
    /// <summary>
    /// Redis instance name
    /// </summary>
    public string InstanceName { get; set; } = "MosquitoApi";

    /// <summary>
    /// Redis database number
    /// </summary>
    public int Database { get; set; } = 0;

    /// <summary>
    /// Connection timeout in seconds
    /// </summary>
    public int ConnectTimeoutSeconds { get; set; } = 5;
}