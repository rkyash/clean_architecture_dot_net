namespace MosquitoApi.Domain.Interfaces;

/// <summary>
/// Interface for entities that support audit tracking
/// </summary>
public interface IAuditable
{
    /// <summary>
    /// User identifier who created the entity
    /// </summary>
    string? CreatedBy { get; set; }

    /// <summary>
    /// User identifier who last modified the entity
    /// </summary>
    string? UpdatedBy { get; set; }

    /// <summary>
    /// Timestamp when the entity was created
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the entity was last modified
    /// </summary>
    DateTime? UpdatedAt { get; set; }
}