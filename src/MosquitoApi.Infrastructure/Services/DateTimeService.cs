namespace MosquitoApi.Infrastructure.Services;

/// <summary>
/// Service for providing date and time functionality (useful for testing)
/// </summary>
public interface IDateTimeService
{
    /// <summary>
    /// Gets the current UTC date and time
    /// </summary>
    DateTime UtcNow { get; }

    /// <summary>
    /// Gets the current local date and time
    /// </summary>
    DateTime Now { get; }

    /// <summary>
    /// Gets today's date
    /// </summary>
    DateTime Today { get; }
}

/// <summary>
/// Default implementation of date time service
/// </summary>
public class DateTimeService : IDateTimeService
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Now => DateTime.Now;
    public DateTime Today => DateTime.Today;
}