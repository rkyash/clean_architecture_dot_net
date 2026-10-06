namespace MosquitoApi.Api.Configuration;

/// <summary>
/// Configuration settings for the audit database
/// </summary>
public class AuditDatabaseSettings
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "AuditDatabase";

    /// <summary>
    /// Audit database connection string
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Maximum retry count for audit database operations
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// Command timeout in seconds
    /// </summary>
    public int CommandTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Whether to enable sensitive data logging for audit operations
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; } = false;

    /// <summary>
    /// Whether to enable detailed errors for audit operations
    /// </summary>
    public bool EnableDetailedErrors { get; set; } = false;

    /// <summary>
    /// Connection pool size for audit database
    /// </summary>
    public int PoolSize { get; set; } = 50;

    /// <summary>
    /// Health check timeout in seconds
    /// </summary>
    public int HealthCheckTimeoutSeconds { get; set; } = 5;

    /// <summary>
    /// Retention period for audit logs in days
    /// </summary>
    public int AuditLogRetentionDays { get; set; } = 365;

    /// <summary>
    /// Whether to enable automatic cleanup of old audit logs
    /// </summary>
    public bool EnableAutomaticCleanup { get; set; } = true;
}