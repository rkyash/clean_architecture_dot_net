using MosquitoApi.Application.Common;
using MosquitoApi.Domain.Entities;
using MosquitoApi.Domain.Enums;

namespace MosquitoApi.Application.Interfaces;

/// <summary>
/// Service contract for audit logging operations
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Creates an audit log entry
    /// </summary>
    /// <param name="tableName">Name of the table/entity</param>
    /// <param name="entityId">ID of the entity</param>
    /// <param name="action">Type of action performed</param>
    /// <param name="oldValues">JSON representation of old values (for updates)</param>
    /// <param name="newValues">JSON representation of new values</param>
    /// <param name="userId">ID of the user performing the action</param>
    /// <param name="username">Username of the user performing the action</param>
    /// <param name="ipAddress">IP address of the request</param>
    /// <param name="userAgent">User agent string</param>
    /// <param name="correlationId">Correlation ID for tracking related operations</param>
    /// <param name="context">Additional context information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success result if logged, otherwise failure result</returns>
    Task<Result> LogAsync(
        string tableName,
        string entityId,
        AuditAction action,
        string? oldValues = null,
        string? newValues = null,
        string? userId = null,
        string? username = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? correlationId = null,
        string? context = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit trail for a specific entity
    /// </summary>
    /// <param name="entityId">Entity ID</param>
    /// <param name="tableName">Optional table name filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of audit log entries for the entity</returns>
    Task<Result<IEnumerable<AuditLog>>> GetEntityAuditTrailAsync(
        string entityId, 
        string? tableName = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs with pagination and filtering
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
    /// <returns>Paginated result of audit log entries</returns>
    Task<Result<IEnumerable<AuditLog>>> GetAuditLogsAsync(
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
    /// Gets audit statistics
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary with action types and their counts</returns>
    Task<Result<Dictionary<AuditAction, int>>> GetAuditStatisticsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the most active users by audit log count
    /// </summary>
    /// <param name="topCount">Number of top users to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary with user ID/username and activity count</returns>
    Task<Result<Dictionary<string, int>>> GetTopActiveUsersAsync(int topCount = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs by correlation ID for tracking related operations
    /// </summary>
    /// <param name="correlationId">Correlation ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of related audit log entries</returns>
    Task<Result<IEnumerable<AuditLog>>> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs entity creation with automatic serialization
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="entity">Created entity</param>
    /// <param name="userId">User ID</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <param name="context">Additional context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success result if logged</returns>
    Task<Result> LogEntityCreatedAsync<T>(T entity, string? userId = null, string? correlationId = null, string? context = null, CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>
    /// Logs entity update with automatic serialization of old and new values
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="oldEntity">Entity before changes</param>
    /// <param name="newEntity">Entity after changes</param>
    /// <param name="userId">User ID</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <param name="context">Additional context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success result if logged</returns>
    Task<Result> LogEntityUpdatedAsync<T>(T oldEntity, T newEntity, string? userId = null, string? correlationId = null, string? context = null, CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>
    /// Logs entity deletion with automatic serialization
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="entity">Deleted entity</param>
    /// <param name="userId">User ID</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <param name="context">Additional context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success result if logged</returns>
    Task<Result> LogEntityDeletedAsync<T>(T entity, string? userId = null, string? correlationId = null, string? context = null, CancellationToken cancellationToken = default)
        where T : class;
}