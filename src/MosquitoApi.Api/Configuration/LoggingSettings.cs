namespace MosquitoApi.Api.Configuration;

/// <summary>
/// Configuration settings for logging
/// </summary>
public class LoggingSettings
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "Logging";

    /// <summary>
    /// Minimum log level
    /// </summary>
    public string MinimumLevel { get; set; } = "Information";

    /// <summary>
    /// Whether to log to console
    /// </summary>
    public bool LogToConsole { get; set; } = true;

    /// <summary>
    /// Whether to log to file
    /// </summary>
    public bool LogToFile { get; set; } = true;

    /// <summary>
    /// Log file path pattern
    /// </summary>
    public string LogFilePath { get; set; } = "logs/mosquito-api-.log";

    /// <summary>
    /// Log file rolling interval (Day, Hour, etc.)
    /// </summary>
    public string RollingInterval { get; set; } = "Day";

    /// <summary>
    /// Number of log files to retain
    /// </summary>
    public int RetainedFileCountLimit { get; set; } = 7;

    /// <summary>
    /// Whether to log to database
    /// </summary>
    public bool LogToDatabase { get; set; } = false;

    /// <summary>
    /// Database connection string for logging (if LogToDatabase is true)
    /// </summary>
    public string DatabaseConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Database table name for logs
    /// </summary>
    public string DatabaseTableName { get; set; } = "logs";

    /// <summary>
    /// Whether to include scopes in log output
    /// </summary>
    public bool IncludeScopes { get; set; } = true;

    /// <summary>
    /// Console log output template
    /// </summary>
    public string ConsoleOutputTemplate { get; set; } = "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj} {Properties:j}{NewLine}{Exception}";

    /// <summary>
    /// File log output template
    /// </summary>
    public string FileOutputTemplate { get; set; } = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {CorrelationId} {SourceContext}: {Message:lj} {Properties:j}{NewLine}{Exception}";
}