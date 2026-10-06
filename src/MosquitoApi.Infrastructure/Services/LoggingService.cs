using Microsoft.Extensions.Logging;

namespace MosquitoApi.Infrastructure.Services;

/// <summary>
/// Service for centralized logging operations
/// </summary>
public interface ILoggingService
{
    void LogInformation(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogError(Exception exception, string message, params object[] args);
    void LogError(string message, params object[] args);
    void LogDebug(string message, params object[] args);
    void LogTrace(string message, params object[] args);
    void LogCritical(Exception exception, string message, params object[] args);
}

/// <summary>
/// Implementation of centralized logging service
/// </summary>
public class LoggingService : ILoggingService
{
    private readonly ILogger<LoggingService> _logger;

    public LoggingService(ILogger<LoggingService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Logs an informational message
    /// </summary>
    public void LogInformation(string message, params object[] args)
    {
        _logger.LogInformation(message, args);
    }

    /// <summary>
    /// Logs a warning message
    /// </summary>
    public void LogWarning(string message, params object[] args)
    {
        _logger.LogWarning(message, args);
    }

    /// <summary>
    /// Logs an error message with exception
    /// </summary>
    public void LogError(Exception exception, string message, params object[] args)
    {
        _logger.LogError(exception, message, args);
    }

    /// <summary>
    /// Logs an error message without exception
    /// </summary>
    public void LogError(string message, params object[] args)
    {
        _logger.LogError(message, args);
    }

    /// <summary>
    /// Logs a debug message
    /// </summary>
    public void LogDebug(string message, params object[] args)
    {
        _logger.LogDebug(message, args);
    }

    /// <summary>
    /// Logs a trace message
    /// </summary>
    public void LogTrace(string message, params object[] args)
    {
        _logger.LogTrace(message, args);
    }

    /// <summary>
    /// Logs a critical message with exception
    /// </summary>
    public void LogCritical(Exception exception, string message, params object[] args)
    {
        _logger.LogCritical(exception, message, args);
    }
}