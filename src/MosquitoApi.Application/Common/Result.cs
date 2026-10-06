namespace MosquitoApi.Application.Common;

/// <summary>
/// Represents the result of an operation that can succeed or fail
/// </summary>
public sealed class Result
{
    /// <summary>
    /// Indicates whether the operation was successful
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indicates whether the operation failed
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Error message if the operation failed
    /// </summary>
    public string Error { get; }

    /// <summary>
    /// Additional error details
    /// </summary>
    public Dictionary<string, object>? Details { get; }

    private Result(bool isSuccess, string error, Dictionary<string, object>? details = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        Details = details;
    }

    /// <summary>
    /// Creates a successful result
    /// </summary>
    /// <returns>Successful result</returns>
    public static Result Success() => new(true, string.Empty);

    /// <summary>
    /// Creates a failed result with an error message
    /// </summary>
    /// <param name="error">Error message</param>
    /// <param name="details">Additional error details</param>
    /// <returns>Failed result</returns>
    public static Result Failure(string error, Dictionary<string, object>? details = null) => new(false, error, details);

    /// <summary>
    /// Creates a failed result from an exception
    /// </summary>
    /// <param name="exception">Exception that caused the failure</param>
    /// <returns>Failed result</returns>
    public static Result Failure(Exception exception) => new(false, exception.Message, 
        new Dictionary<string, object> { { "ExceptionType", exception.GetType().Name } });

    /// <summary>
    /// Creates a failed result from multiple validation errors
    /// </summary>
    /// <param name="errors">Collection of validation errors</param>
    /// <returns>Failed result</returns>
    public static Result ValidationFailure(IEnumerable<string> errors)
    {
        var errorList = errors.ToList();
        var message = errorList.Count == 1 
            ? errorList[0] 
            : $"Validation failed with {errorList.Count} errors";
        
        return new(false, message, new Dictionary<string, object> { { "ValidationErrors", errorList } });
    }

    /// <summary>
    /// Implicitly converts a Result to bool based on success status
    /// </summary>
    /// <param name="result">Result to convert</param>
    public static implicit operator bool(Result result) => result.IsSuccess;
}

/// <summary>
/// Represents the result of an operation that can succeed with a value or fail
/// </summary>
/// <typeparam name="T">Type of the success value</typeparam>
public sealed class Result<T>
{
    /// <summary>
    /// Indicates whether the operation was successful
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indicates whether the operation failed
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// The value if the operation was successful
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Error message if the operation failed
    /// </summary>
    public string Error { get; }

    /// <summary>
    /// Additional error details
    /// </summary>
    public Dictionary<string, object>? Details { get; }

    private Result(bool isSuccess, T? value, string error, Dictionary<string, object>? details = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        Details = details;
    }

    /// <summary>
    /// Creates a successful result with a value
    /// </summary>
    /// <param name="value">Success value</param>
    /// <returns>Successful result</returns>
    public static Result<T> Success(T value) => new(true, value, string.Empty);

    /// <summary>
    /// Creates a failed result with an error message
    /// </summary>
    /// <param name="error">Error message</param>
    /// <param name="details">Additional error details</param>
    /// <returns>Failed result</returns>
    public static Result<T> Failure(string error, Dictionary<string, object>? details = null) => 
        new(false, default, error, details);

    /// <summary>
    /// Creates a failed result from an exception
    /// </summary>
    /// <param name="exception">Exception that caused the failure</param>
    /// <returns>Failed result</returns>
    public static Result<T> Failure(Exception exception) => 
        new(false, default, exception.Message, 
            new Dictionary<string, object> { { "ExceptionType", exception.GetType().Name } });

    /// <summary>
    /// Creates a failed result from multiple validation errors
    /// </summary>
    /// <param name="errors">Collection of validation errors</param>
    /// <returns>Failed result</returns>
    public static Result<T> ValidationFailure(IEnumerable<string> errors)
    {
        var errorList = errors.ToList();
        var message = errorList.Count == 1 
            ? errorList[0] 
            : $"Validation failed with {errorList.Count} errors";
        
        return new(false, default, message, new Dictionary<string, object> { { "ValidationErrors", errorList } });
    }

    /// <summary>
    /// Creates a "not found" result
    /// </summary>
    /// <param name="entity">Name of the entity that was not found</param>
    /// <param name="identifier">Identifier that was searched for</param>
    /// <returns>Failed result indicating not found</returns>
    public static Result<T> NotFound(string entity, object identifier) =>
        new(false, default, $"{entity} with identifier '{identifier}' was not found.",
            new Dictionary<string, object> { { "Entity", entity }, { "Identifier", identifier } });

    /// <summary>
    /// Maps the value of this result to a different type if successful
    /// </summary>
    /// <typeparam name="TResult">Target type</typeparam>
    /// <param name="mapper">Function to map the value</param>
    /// <returns>New result with mapped value</returns>
    public Result<TResult> Map<TResult>(Func<T, TResult> mapper)
    {
        if (IsFailure)
            return Result<TResult>.Failure(Error, Details);

        try
        {
            var mappedValue = mapper(Value!);
            return Result<TResult>.Success(mappedValue);
        }
        catch (Exception ex)
        {
            return Result<TResult>.Failure(ex);
        }
    }

    /// <summary>
    /// Executes an action on the value if the result is successful
    /// </summary>
    /// <param name="action">Action to execute</param>
    /// <returns>The original result</returns>
    public Result<T> OnSuccess(Action<T> action)
    {
        if (IsSuccess)
        {
            action(Value!);
        }
        return this;
    }

    /// <summary>
    /// Executes an action if the result is a failure
    /// </summary>
    /// <param name="action">Action to execute with error message</param>
    /// <returns>The original result</returns>
    public Result<T> OnFailure(Action<string> action)
    {
        if (IsFailure)
        {
            action(Error);
        }
        return this;
    }

    /// <summary>
    /// Returns the value if successful, otherwise returns the default value
    /// </summary>
    /// <param name="defaultValue">Default value to return on failure</param>
    /// <returns>Value or default</returns>
    public T GetValueOrDefault(T defaultValue = default!) =>
        IsSuccess ? Value! : defaultValue;

    /// <summary>
    /// Implicitly converts a Result&lt;T&gt; to bool based on success status
    /// </summary>
    /// <param name="result">Result to convert</param>
    public static implicit operator bool(Result<T> result) => result.IsSuccess;

    /// <summary>
    /// Implicitly converts a value to a successful Result&lt;T&gt;
    /// </summary>
    /// <param name="value">Value to wrap</param>
    public static implicit operator Result<T>(T value) => Success(value);
}