namespace MosquitoApi.Domain.Configuration;

/// <summary>
/// Static application settings manager for accessing configuration throughout the application
/// Uses the AppSettings singleton pattern for unified configuration access
/// </summary>
public static class AppSettingsManager
{
    /// <summary>
    /// Gets the current application settings (delegates to AppSettings.Instance)
    /// </summary>
    public static AppSettings Current => AppSettings.Instance;

    /// <summary>
    /// Initializes the application settings with a pre-configured AppSettings instance
    /// This method should be called from the API layer where configuration binding occurs
    /// </summary>
    /// <param name="appSettings">The pre-configured application settings</param>
    public static void Initialize(AppSettings appSettings)
    {
        AppSettings.UpdateInstance(appSettings ?? throw new ArgumentNullException(nameof(appSettings)));
    }

    /// <summary>
    /// Reloads the application settings
    /// </summary>
    /// <param name="appSettings">The updated application settings</param>
    public static void Reload(AppSettings appSettings)
    {
        AppSettings.UpdateInstance(appSettings ?? throw new ArgumentNullException(nameof(appSettings)));
    }

    /// <summary>
    /// Checks if the application settings have been initialized
    /// </summary>
    public static bool IsInitialized => Current.ConnectionStrings != null;

    /// <summary>
    /// Gets connection string by name
    /// </summary>
    /// <param name="name">Connection string name</param>
    /// <returns>Connection string value</returns>
    public static string GetConnectionString(string name)
    {
        return name.ToLowerInvariant() switch
        {
            "default" or "defaultconnection" => Current.ConnectionStrings.DefaultConnection,
            "audit" or "auditconnection" => Current.ConnectionStrings.AuditConnection,
            "redis" or "redisconnection" => Current.ConnectionStrings.RedisConnection ?? string.Empty,
            _ => throw new ArgumentException($"Unknown connection string name: {name}")
        };
    }

    /// <summary>
    /// Gets JWT settings
    /// </summary>
    public static JwtSettings JwtSettings => Current.JwtSettings;

    /// <summary>
    /// Gets database settings
    /// </summary>
    public static ConnectionStrings DatabaseSettings => Current.ConnectionStrings;

    /// <summary>
    /// Gets CORS settings
    /// </summary>
    public static CorsSettings CorsSettings => Current.Cors;

    /// <summary>
    /// Gets rate limiting settings
    /// </summary>
    public static RateLimitingSettings RateLimitingSettings => Current.RateLimiting;

    /// <summary>
    /// Gets application settings
    /// </summary>
    public static ApplicationSettings ApplicationSettings => Current.Application;

    /// <summary>
    /// Gets email settings
    /// </summary>
    public static EmailSettings EmailSettings => Current.Email;

    /// <summary>
    /// Gets file upload settings
    /// </summary>
    public static FileUploadSettings FileUploadSettings => Current.FileUpload;

    /// <summary>
    /// Gets cache settings
    /// </summary>
    public static CacheSettings CacheSettings => Current.Cache;

    /// <summary>
    /// Gets health check settings
    /// </summary>
    public static HealthCheckSettings HealthCheckSettings => Current.HealthChecks;

    /// <summary>
    /// Gets Scalar UI settings
    /// </summary>
    public static ScalarSettings ScalarSettings => Current.Scalar;

    /// <summary>
    /// Gets a configuration value by key with optional default value
    /// </summary>
    /// <param name="key">Configuration key (supports nested keys with colons, e.g., "JwtSettings:SecretKey")</param>
    /// <param name="defaultValue">Default value if key not found</param>
    /// <returns>Configuration value</returns>
    public static string GetValue(string key, string defaultValue = "")
    {
        var keys = key.Split(':');
        object? current = Current;

        foreach (var k in keys)
        {
            if (current == null) return defaultValue;

            var property = current.GetType().GetProperty(k);
            if (property == null) return defaultValue;

            current = property.GetValue(current);
        }

        return current?.ToString() ?? defaultValue;
    }

    /// <summary>
    /// Gets a configuration value by key and converts it to the specified type
    /// </summary>
    /// <typeparam name="T">The type to convert to</typeparam>
    /// <param name="key">Configuration key</param>
    /// <param name="defaultValue">Default value if key not found or conversion fails</param>
    /// <returns>Converted configuration value</returns>
    public static T GetValue<T>(string key, T defaultValue = default!)
    {
        try
        {
            var stringValue = GetValue(key);
            if (string.IsNullOrEmpty(stringValue)) return defaultValue;

            if (typeof(T) == typeof(string)) return (T)(object)stringValue;
            if (typeof(T) == typeof(int)) return (T)(object)int.Parse(stringValue);
            if (typeof(T) == typeof(bool)) return (T)(object)bool.Parse(stringValue);
            if (typeof(T) == typeof(double)) return (T)(object)double.Parse(stringValue);
            if (typeof(T) == typeof(decimal)) return (T)(object)decimal.Parse(stringValue);
            if (typeof(T) == typeof(DateTime)) return (T)(object)DateTime.Parse(stringValue);

            // For enums
            if (typeof(T).IsEnum) return (T)Enum.Parse(typeof(T), stringValue, true);

            return defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Checks if a configuration key exists
    /// </summary>
    /// <param name="key">Configuration key</param>
    /// <returns>True if key exists, false otherwise</returns>
    public static bool HasValue(string key)
    {
        try
        {
            var value = GetValue(key);
            return !string.IsNullOrEmpty(value);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets environment name
    /// </summary>
    public static string Environment => Current.Application.Environment;

    /// <summary>
    /// Checks if running in development environment
    /// </summary>
    public static bool IsDevelopment => Environment.Equals("Development", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Checks if running in staging environment
    /// </summary>
    public static bool IsStaging => Environment.Equals("Staging", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Checks if running in production environment
    /// </summary>
    public static bool IsProduction => Environment.Equals("Production", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets all configuration as a dictionary (for debugging)
    /// </summary>
    /// <returns>Configuration dictionary</returns>
    public static Dictionary<string, string> GetAllSettings()
    {
        var settings = new Dictionary<string, string>();
        
        if (Current == null) return settings;

        // Add connection strings
        settings.Add("ConnectionStrings:DefaultConnection", Current.ConnectionStrings.DefaultConnection);
        settings.Add("ConnectionStrings:AuditConnection", Current.ConnectionStrings.AuditConnection);
        
        // Add JWT settings (excluding sensitive data)
        settings.Add("JwtSettings:Issuer", Current.JwtSettings.Issuer);
        settings.Add("JwtSettings:Audience", Current.JwtSettings.Audience);
        settings.Add("JwtSettings:AccessTokenExpirationMinutes", Current.JwtSettings.AccessTokenExpirationMinutes.ToString());
        settings.Add("JwtSettings:RefreshTokenExpirationDays", Current.JwtSettings.RefreshTokenExpirationDays.ToString());
        
        // Add application settings
        settings.Add("Application:Name", Current.Application.Name);
        settings.Add("Application:Version", Current.Application.Version);
        settings.Add("Application:Environment", Current.Application.Environment);
        
        return settings;
    }
}