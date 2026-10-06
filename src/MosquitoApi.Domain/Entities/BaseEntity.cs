using MosquitoApi.Domain.Interfaces;

namespace MosquitoApi.Domain.Entities;

/// <summary>
/// Base entity with common properties for all domain entities
/// </summary>
public abstract class BaseEntity : IAuditable
{
    /// <summary>
    /// Unique identifier for the entity
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Timestamp when the entity was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the entity was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Indicates whether the entity is soft deleted
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Timestamp when the entity was soft deleted
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// User identifier who created the entity
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// User identifier who last modified the entity
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// User identifier who deleted the entity (for soft deletes)
    /// </summary>
    public string? DeletedBy { get; set; }
}