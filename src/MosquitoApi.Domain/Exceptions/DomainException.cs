namespace MosquitoApi.Domain.Exceptions;

/// <summary>
/// Base exception for domain-related errors
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>
    /// Error code for categorizing the exception
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Additional details about the error
    /// </summary>
    public Dictionary<string, object>? Details { get; }

    /// <summary>
    /// Initializes a new instance of the DomainException class
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="errorCode">Error code</param>
    /// <param name="details">Additional error details</param>
    protected DomainException(string message, string errorCode, Dictionary<string, object>? details = null)
        : base(message)
    {
        ErrorCode = errorCode;
        Details = details;
    }

    /// <summary>
    /// Initializes a new instance of the DomainException class with an inner exception
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="errorCode">Error code</param>
    /// <param name="innerException">Inner exception</param>
    /// <param name="details">Additional error details</param>
    protected DomainException(string message, string errorCode, Exception innerException, Dictionary<string, object>? details = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        Details = details;
    }

    /// <summary>
    /// Returns a string representation of the exception including error code and details
    /// </summary>
    public override string ToString()
    {
        var result = $"[{ErrorCode}] {Message}";
        
        if (Details?.Count > 0)
        {
            var detailStrings = Details.Select(kvp => $"{kvp.Key}: {kvp.Value}");
            result += $" | Details: {string.Join(", ", detailStrings)}";
        }

        if (InnerException != null)
        {
            result += $" | Inner Exception: {InnerException.Message}";
        }

        return result;
    }
}