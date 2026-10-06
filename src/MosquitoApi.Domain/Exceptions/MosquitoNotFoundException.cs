namespace MosquitoApi.Domain.Exceptions;

/// <summary>
/// Exception thrown when a requested mosquito is not found
/// </summary>
public class MosquitoNotFoundException : DomainException
{
    /// <summary>
    /// The ID of the mosquito that was not found
    /// </summary>
    public Guid MosquitoId { get; }

    /// <summary>
    /// Initializes a new instance of the MosquitoNotFoundException class
    /// </summary>
    /// <param name="mosquitoId">ID of the mosquito that was not found</param>
    public MosquitoNotFoundException(Guid mosquitoId)
        : base($"Mosquito with ID '{mosquitoId}' was not found.", "MOSQUITO_NOT_FOUND", 
               new Dictionary<string, object> { { "MosquitoId", mosquitoId } })
    {
        MosquitoId = mosquitoId;
    }

    /// <summary>
    /// Initializes a new instance of the MosquitoNotFoundException class with a custom message
    /// </summary>
    /// <param name="mosquitoId">ID of the mosquito that was not found</param>
    /// <param name="message">Custom error message</param>
    public MosquitoNotFoundException(Guid mosquitoId, string message)
        : base(message, "MOSQUITO_NOT_FOUND",
               new Dictionary<string, object> { { "MosquitoId", mosquitoId } })
    {
        MosquitoId = mosquitoId;
    }

    /// <summary>
    /// Initializes a new instance of the MosquitoNotFoundException class with an inner exception
    /// </summary>
    /// <param name="mosquitoId">ID of the mosquito that was not found</param>
    /// <param name="innerException">Inner exception</param>
    public MosquitoNotFoundException(Guid mosquitoId, Exception innerException)
        : base($"Mosquito with ID '{mosquitoId}' was not found.", "MOSQUITO_NOT_FOUND", innerException,
               new Dictionary<string, object> { { "MosquitoId", mosquitoId } })
    {
        MosquitoId = mosquitoId;
    }
}