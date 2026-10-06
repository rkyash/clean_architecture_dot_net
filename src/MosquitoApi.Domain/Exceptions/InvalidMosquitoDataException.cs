namespace MosquitoApi.Domain.Exceptions;

/// <summary>
/// Exception thrown when mosquito data fails domain validation rules
/// </summary>
public class InvalidMosquitoDataException : DomainException
{
    /// <summary>
    /// List of validation errors that caused the exception
    /// </summary>
    public IReadOnlyList<string> ValidationErrors { get; }

    /// <summary>
    /// The field or property that caused the validation error (if applicable)
    /// </summary>
    public string? FieldName { get; }

    /// <summary>
    /// Initializes a new instance of the InvalidMosquitoDataException class
    /// </summary>
    /// <param name="validationErrors">List of validation errors</param>
    public InvalidMosquitoDataException(IEnumerable<string> validationErrors)
        : base(CreateMessage(validationErrors), "INVALID_MOSQUITO_DATA",
               new Dictionary<string, object> { { "ValidationErrors", validationErrors.ToList() } })
    {
        ValidationErrors = validationErrors.ToList().AsReadOnly();
    }

    /// <summary>
    /// Initializes a new instance of the InvalidMosquitoDataException class for a specific field
    /// </summary>
    /// <param name="fieldName">Name of the field that failed validation</param>
    /// <param name="errorMessage">Validation error message</param>
    public InvalidMosquitoDataException(string fieldName, string errorMessage)
        : base($"Validation failed for field '{fieldName}': {errorMessage}", "INVALID_MOSQUITO_DATA",
               new Dictionary<string, object> 
               { 
                   { "FieldName", fieldName },
                   { "ValidationErrors", new List<string> { errorMessage } }
               })
    {
        FieldName = fieldName;
        ValidationErrors = new List<string> { errorMessage }.AsReadOnly();
    }

    /// <summary>
    /// Initializes a new instance of the InvalidMosquitoDataException class with a single error
    /// </summary>
    /// <param name="errorMessage">Validation error message</param>
    public InvalidMosquitoDataException(string errorMessage)
        : base(errorMessage, "INVALID_MOSQUITO_DATA",
               new Dictionary<string, object> { { "ValidationErrors", new List<string> { errorMessage } } })
    {
        ValidationErrors = new List<string> { errorMessage }.AsReadOnly();
    }

    /// <summary>
    /// Initializes a new instance of the InvalidMosquitoDataException class with an inner exception
    /// </summary>
    /// <param name="validationErrors">List of validation errors</param>
    /// <param name="innerException">Inner exception</param>
    public InvalidMosquitoDataException(IEnumerable<string> validationErrors, Exception innerException)
        : base(CreateMessage(validationErrors), "INVALID_MOSQUITO_DATA", innerException,
               new Dictionary<string, object> { { "ValidationErrors", validationErrors.ToList() } })
    {
        ValidationErrors = validationErrors.ToList().AsReadOnly();
    }

    /// <summary>
    /// Creates a formatted error message from multiple validation errors
    /// </summary>
    /// <param name="validationErrors">Collection of validation errors</param>
    /// <returns>Formatted error message</returns>
    private static string CreateMessage(IEnumerable<string> validationErrors)
    {
        var errors = validationErrors.ToList();
        
        if (errors.Count == 0)
            return "Mosquito data validation failed";
            
        if (errors.Count == 1)
            return $"Mosquito data validation failed: {errors[0]}";
            
        return $"Mosquito data validation failed with {errors.Count} errors: {string.Join("; ", errors)}";
    }

    /// <summary>
    /// Gets a formatted string of all validation errors
    /// </summary>
    /// <returns>Formatted validation errors</returns>
    public string GetValidationErrorsString()
    {
        return ValidationErrors.Count switch
        {
            0 => "No validation errors",
            1 => ValidationErrors[0],
            _ => string.Join(Environment.NewLine, ValidationErrors.Select((error, index) => $"{index + 1}. {error}"))
        };
    }
}