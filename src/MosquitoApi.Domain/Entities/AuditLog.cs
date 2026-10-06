using MosquitoApi.Domain.Enums;

namespace MosquitoApi.Domain.Entities;

/// <summary>
/// Entity for tracking audit trail of all system changes
/// </summary>
public class AuditLog : BaseEntity
{
    /// <summary>
    /// The table/entity name that was modified
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Primary key of the modified entity
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// Type of action performed (Create, Update, Delete)
    /// </summary>
    public AuditAction Action { get; set; }

    /// <summary>
    /// JSON representation of the entity before changes (null for Create)
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    /// JSON representation of the entity after changes (null for Delete)
    /// </summary>
    public string? NewValues { get; set; }

    /// <summary>
    /// JSON representation of only the changed fields
    /// </summary>
    public string? ChangedFields { get; set; }

    /// <summary>
    /// User identifier who performed the action
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Username who performed the action
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// IP address from which the action was performed
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent string from the request
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Correlation ID for tracking related operations
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Additional context or metadata about the change
    /// </summary>
    public string? Context { get; set; }

    /// <summary>
    /// Timestamp when the action occurred
    /// </summary>
    public DateTime ActionTimestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a summary description of the audit log entry
    /// </summary>
    /// <returns>Human-readable summary</returns>
    public string GetSummary()
    {
        var actionText = Action switch
        {
            AuditAction.Create => "created",
            AuditAction.Update => "updated",
            AuditAction.Delete => "deleted",
            _ => "modified"
        };

        var userInfo = !string.IsNullOrEmpty(Username) ? Username : UserId ?? "Unknown user";
        
        return $"{userInfo} {actionText} {TableName} record {EntityId} at {ActionTimestamp:yyyy-MM-dd HH:mm:ss} UTC";
    }

    /// <summary>
    /// Validates the audit log entry
    /// </summary>
    /// <returns>List of validation errors</returns>
    public List<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(TableName))
            errors.Add("Table name is required");

        if (string.IsNullOrWhiteSpace(EntityId))
            errors.Add("Entity ID is required");

        if (Action == AuditAction.Update && string.IsNullOrWhiteSpace(OldValues))
            errors.Add("Old values are required for update operations");

        if (Action == AuditAction.Create && !string.IsNullOrWhiteSpace(OldValues))
            errors.Add("Old values should be null for create operations");

        if (Action == AuditAction.Delete && !string.IsNullOrWhiteSpace(NewValues))
            errors.Add("New values should be null for delete operations");

        if (ActionTimestamp > DateTime.UtcNow.AddMinutes(5))
            errors.Add("Action timestamp cannot be significantly in the future");

        return errors;
    }
}