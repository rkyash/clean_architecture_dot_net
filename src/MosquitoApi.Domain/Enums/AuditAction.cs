namespace MosquitoApi.Domain.Enums;

/// <summary>
/// Enumeration of audit action types for tracking entity changes
/// </summary>
public enum AuditAction
{
    /// <summary>
    /// Entity was created
    /// </summary>
    Create = 1,

    /// <summary>
    /// Entity was updated
    /// </summary>
    Update = 2,

    /// <summary>
    /// Entity was deleted
    /// </summary>
    Delete = 3
}