namespace MosquitoApi.Api.Configuration;

/// <summary>
/// Configuration settings for the main database
/// </summary>
public class DatabaseSettings
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "Database";

    /// <summary>
    /// Database connection string
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Maximum retry count for database operations
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// Command timeout in seconds
    /// </summary>
    public int CommandTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Whether to enable sensitive data logging
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; } = false;

    /// <summary>
    /// Whether to enable detailed errors
    /// </summary>
    public bool EnableDetailedErrors { get; set; } = false;

    /// <summary>
    /// Connection pool size
    /// </summary>
    public int PoolSize { get; set; } = 100;

    /// <summary>
    /// Health check timeout in seconds
    /// </summary>
    public int HealthCheckTimeoutSeconds { get; set; } = 5;
}