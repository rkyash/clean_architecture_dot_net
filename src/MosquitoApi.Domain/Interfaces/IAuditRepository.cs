using MosquitoApi.Domain.Entities;
using MosquitoApi.Domain.Enums;

namespace MosquitoApi.Domain.Interfaces;

/// <summary>
/// Repository contract for audit log data access operations
/// </summary>
public interface IAuditRepository
{
    /// <summary>
    /// Gets an audit log entry by its unique identifier
    /// </summary>
    /// <param name="id">Audit log ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Audit log entry or null if not found</returns>
    Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all audit logs with optional pagination
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of audit logs</returns>
    Task<IEnumerable<AuditLog>> GetAllAsync(int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total count of audit logs
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total count</returns>
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs for a specific entity
    /// </summary>
    /// <param name="entityId">Entity ID to get audit trail for</param>
    /// <param name="tableName">Optional table name filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of audit logs for the entity</returns>
    Task<IEnumerable<AuditLog>> GetEntityAuditTrailAsync(string entityId, string? tableName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs by user
    /// </summary>
    /// <param name="userId">User ID to filter by</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of audit logs for the user</returns>
    Task<IEnumerable<AuditLog>> GetByUserAsync(string userId, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs by action type
    /// </summary>
    /// <param name="action">Action type to filter by</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of audit logs for the action type</returns>
    Task<IEnumerable<AuditLog>> GetByActionAsync(AuditAction action, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs by table name
    /// </summary>
    /// <param name="tableName">Table name to filter by</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of audit logs for the table</returns>
    Task<IEnumerable<AuditLog>> GetByTableAsync(string tableName, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs within a date range
    /// </summary>
    /// <param name="fromDate">Start date (inclusive)</param>
    /// <param name="toDate">End date (inclusive)</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of audit logs within the date range</returns>
    Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches audit logs by multiple criteria
    /// </summary>
    /// <param name="userId">Optional user ID filter</param>
    /// <param name="tableName">Optional table name filter</param>
    /// <param name="action">Optional action type filter</param>
    /// <param name="entityId">Optional entity ID filter</param>
    /// <param name="fromDate">Optional start date filter</param>
    /// <param name="toDate">Optional end date filter</param>
    /// <param name="correlationId">Optional correlation ID filter</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of matching audit logs</returns>
    Task<IEnumerable<AuditLog>> SearchAsync(
        string? userId = null,
        string? tableName = null,
        AuditAction? action = null,
        string? entityId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? correlationId = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets count of audit logs matching search criteria
    /// </summary>
    /// <param name="userId">Optional user ID filter</param>
    /// <param name="tableName">Optional table name filter</param>
    /// <param name="action">Optional action type filter</param>
    /// <param name="entityId">Optional entity ID filter</param>
    /// <param name="fromDate">Optional start date filter</param>
    /// <param name="toDate">Optional end date filter</param>
    /// <param name="correlationId">Optional correlation ID filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of matching audit logs</returns>
    Task<int> GetSearchCountAsync(
        string? userId = null,
        string? tableName = null,
        AuditAction? action = null,
        string? entityId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? correlationId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new audit log entry
    /// </summary>
    /// <param name="auditLog">Audit log entry to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The added audit log entry</returns>
    Task<AuditLog> AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple audit log entries in a batch
    /// </summary>
    /// <param name="auditLogs">Collection of audit log entries to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The added audit log entries</returns>
    Task<IEnumerable<AuditLog>> AddRangeAsync(IEnumerable<AuditLog> auditLogs, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets statistics about audit log activity
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary with action type as key and count as value</returns>
    Task<Dictionary<AuditAction, int>> GetActionStatisticsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the most active users by audit log count
    /// </summary>
    /// <param name="topCount">Number of top users to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary with user ID as key and activity count as value</returns>
    Task<Dictionary<string, int>> GetTopActiveUsersAsync(int topCount = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs by correlation ID for tracking related operations
    /// </summary>
    /// <param name="correlationId">Correlation ID to search by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of related audit logs</returns>
    Task<IEnumerable<AuditLog>> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default);
}