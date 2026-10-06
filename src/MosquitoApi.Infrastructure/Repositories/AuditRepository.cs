using Microsoft.EntityFrameworkCore;
using MosquitoApi.Domain.Entities;
using MosquitoApi.Domain.Enums;
using MosquitoApi.Domain.Interfaces;
using MosquitoApi.Infrastructure.Data;

namespace MosquitoApi.Infrastructure.Repositories;

/// <summary>
/// Implementation of audit repository using Entity Framework Core
/// </summary>
public class AuditRepository : IAuditRepository
{
    private readonly AuditDbContext _context;

    public AuditRepository(AuditDbContext context)
    {
        _context = context;
    }

    public async Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetAllAsync(int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .OrderByDescending(a => a.ActionTimestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs.CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetEntityAuditTrailAsync(string entityId, string? tableName = null, CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.Where(a => a.EntityId == entityId);

        if (!string.IsNullOrEmpty(tableName))
        {
            query = query.Where(a => a.TableName == tableName);
        }

        return await query
            .OrderBy(a => a.ActionTimestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetByUserAsync(string userId, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.ActionTimestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetByActionAsync(AuditAction action, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(a => a.Action == action)
            .OrderByDescending(a => a.ActionTimestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetByTableAsync(string tableName, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(a => a.TableName == tableName)
            .OrderByDescending(a => a.ActionTimestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(a => a.ActionTimestamp >= fromDate && a.ActionTimestamp <= toDate)
            .OrderByDescending(a => a.ActionTimestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> SearchAsync(
        string? userId = null,
        string? tableName = null,
        AuditAction? action = null,
        string? entityId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? correlationId = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.AsQueryable();

        // Apply filters
        query = ApplySearchFilters(query, userId, tableName, action, entityId, fromDate, toDate, correlationId);

        // Apply pagination and ordering
        return await query
            .OrderByDescending(a => a.ActionTimestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetSearchCountAsync(
        string? userId = null,
        string? tableName = null,
        AuditAction? action = null,
        string? entityId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.AsQueryable();

        // Apply filters
        query = ApplySearchFilters(query, userId, tableName, action, entityId, fromDate, toDate, correlationId);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<AuditLog> AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        var entry = await _context.AuditLogs.AddAsync(auditLog, cancellationToken);
        return entry.Entity;
    }

    public async Task<IEnumerable<AuditLog>> AddRangeAsync(IEnumerable<AuditLog> auditLogs, CancellationToken cancellationToken = default)
    {
        var auditLogsList = auditLogs.ToList();
        await _context.AuditLogs.AddRangeAsync(auditLogsList, cancellationToken);
        return auditLogsList;
    }

    public async Task<Dictionary<AuditAction, int>> GetActionStatisticsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .GroupBy(a => a.Action)
            .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
    }

    public async Task<Dictionary<string, int>> GetTopActiveUsersAsync(int topCount = 10, CancellationToken cancellationToken = default)
    {
        var results = await _context.AuditLogs
            .Where(a => a.UserId != null)
            .GroupBy(a => a.UserId!)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(topCount)
            .ToListAsync(cancellationToken);

        return results.ToDictionary(x => x.UserId, x => x.Count);
    }

    public async Task<IEnumerable<AuditLog>> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(a => a.CorrelationId == correlationId)
            .OrderBy(a => a.ActionTimestamp)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Applies search filters to the audit log query
    /// </summary>
    private static IQueryable<AuditLog> ApplySearchFilters(
        IQueryable<AuditLog> query,
        string? userId,
        string? tableName,
        AuditAction? action,
        string? entityId,
        DateTime? fromDate,
        DateTime? toDate,
        string? correlationId)
    {
        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(a => a.UserId == userId);
        }

        if (!string.IsNullOrEmpty(tableName))
        {
            query = query.Where(a => a.TableName == tableName);
        }

        if (action.HasValue)
        {
            query = query.Where(a => a.Action == action.Value);
        }

        if (!string.IsNullOrEmpty(entityId))
        {
            query = query.Where(a => a.EntityId == entityId);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(a => a.ActionTimestamp >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(a => a.ActionTimestamp <= toDate.Value);
        }

        if (!string.IsNullOrEmpty(correlationId))
        {
            query = query.Where(a => a.CorrelationId == correlationId);
        }

        return query;
    }
}