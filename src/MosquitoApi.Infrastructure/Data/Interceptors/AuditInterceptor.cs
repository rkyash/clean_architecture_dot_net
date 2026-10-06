using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MosquitoApi.Domain.Entities;
using MosquitoApi.Domain.Enums;
using MosquitoApi.Domain.Interfaces;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace MosquitoApi.Infrastructure.Data.Interceptors;

/// <summary>
/// EF Core interceptor for automatic audit trail creation
/// </summary>
public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuditInterceptor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context != null)
        {
            await CaptureAuditDataAsync(eventData.Context, cancellationToken);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context != null)
        {
            CaptureAuditData(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    /// <summary>
    /// Captures audit data for all tracked entities
    /// </summary>
    private async Task CaptureAuditDataAsync(DbContext context, CancellationToken cancellationToken = default)
    {
        // Get audit repository from service provider
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetService<IUnitOfWork>();
        
        if (unitOfWork == null) return;

        var auditEntries = new List<AuditEntry>();

        // Capture changes before SaveChanges is called
        foreach (var entry in context.ChangeTracker.Entries())
        {
            // Skip audit logs themselves to prevent infinite loops
            if (entry.Entity is AuditLog) continue;

            // Only audit entities that implement IAuditable or are BaseEntity
            if (entry.Entity is not BaseEntity && entry.Entity is not IAuditable) continue;

            var auditEntry = CreateAuditEntry(entry);
            if (auditEntry != null)
            {
                auditEntries.Add(auditEntry);
            }
        }

        // Create audit log entries
        var auditLogs = auditEntries.Select(CreateAuditLog).ToList();
        
        if (auditLogs.Count > 0)
        {
            await unitOfWork.AuditRepository.AddRangeAsync(auditLogs, cancellationToken);
        }
    }

    /// <summary>
    /// Synchronous version of audit data capture
    /// </summary>
    private void CaptureAuditData(DbContext context)
    {
        // For synchronous operations, we'll use a simpler approach
        // In production, you might want to queue these for background processing
        Task.Run(async () => await CaptureAuditDataAsync(context));
    }

    /// <summary>
    /// Creates an audit entry from an entity entry
    /// </summary>
    private AuditEntry? CreateAuditEntry(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        var entityType = entry.Entity.GetType();
        var entityId = GetEntityId(entry);

        if (string.IsNullOrEmpty(entityId)) return null;

        var auditAction = entry.State switch
        {
            EntityState.Added => AuditAction.Create,
            EntityState.Modified => AuditAction.Update,
            EntityState.Deleted => AuditAction.Delete,
            _ => (AuditAction?)null
        };

        if (auditAction == null) return null;

        var auditEntry = new AuditEntry
        {
            TableName = entityType.Name,
            EntityId = entityId,
            Action = auditAction.Value,
            Entity = entry.Entity,
            OriginalValues = new Dictionary<string, object?>(),
            CurrentValues = new Dictionary<string, object?>(),
            ModifiedProperties = new List<string>()
        };

        // Capture property values
        foreach (var property in entry.Properties)
        {
            if (property.IsTemporary) continue;

            var propertyName = property.Metadata.Name;

            switch (entry.State)
            {
                case EntityState.Added:
                    auditEntry.CurrentValues[propertyName] = property.CurrentValue;
                    break;

                case EntityState.Deleted:
                    auditEntry.OriginalValues[propertyName] = property.OriginalValue;
                    break;

                case EntityState.Modified:
                    if (property.IsModified)
                    {
                        auditEntry.OriginalValues[propertyName] = property.OriginalValue;
                        auditEntry.CurrentValues[propertyName] = property.CurrentValue;
                        auditEntry.ModifiedProperties.Add(propertyName);
                    }
                    break;
            }
        }

        return auditEntry;
    }

    /// <summary>
    /// Creates an audit log from an audit entry
    /// </summary>
    private AuditLog CreateAuditLog(AuditEntry auditEntry)
    {
        var auditLog = new AuditLog
        {
            TableName = auditEntry.TableName,
            EntityId = auditEntry.EntityId,
            Action = auditEntry.Action,
            ActionTimestamp = DateTime.UtcNow
        };

        // Serialize values to JSON
        if (auditEntry.OriginalValues.Count > 0)
        {
            auditLog.OldValues = JsonSerializer.Serialize(auditEntry.OriginalValues, _jsonOptions);
        }

        if (auditEntry.CurrentValues.Count > 0)
        {
            auditLog.NewValues = JsonSerializer.Serialize(auditEntry.CurrentValues, _jsonOptions);
        }

        if (auditEntry.ModifiedProperties.Count > 0)
        {
            var changedFields = auditEntry.ModifiedProperties.ToDictionary(
                prop => prop,
                prop => new
                {
                    Old = auditEntry.OriginalValues.GetValueOrDefault(prop),
                    New = auditEntry.CurrentValues.GetValueOrDefault(prop)
                });

            auditLog.ChangedFields = JsonSerializer.Serialize(changedFields, _jsonOptions);
        }

        // Try to get user context information
        SetUserContext(auditLog, auditEntry.Entity);

        return auditLog;
    }

    /// <summary>
    /// Gets the entity ID from an entity entry
    /// </summary>
    private static string GetEntityId(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        var keyValues = entry.Properties
            .Where(p => p.Metadata.IsPrimaryKey())
            .Select(p => p.CurrentValue)
            .ToArray();

        return keyValues.Length switch
        {
            0 => string.Empty,
            1 => keyValues[0]?.ToString() ?? string.Empty,
            _ => string.Join(",", keyValues.Select(v => v?.ToString() ?? "null"))
        };
    }

    /// <summary>
    /// Sets user context information on the audit log
    /// </summary>
    private void SetUserContext(AuditLog auditLog, object entity)
    {
        // Try to get user information from the entity if it's auditable
        if (entity is IAuditable auditableEntity)
        {
            auditLog.UserId = auditableEntity.CreatedBy ?? auditableEntity.UpdatedBy;
        }

        // Try to get additional context from HTTP context or other sources
        // This would typically be done through dependency injection of IHttpContextAccessor
        // For now, we'll leave these fields to be set by the calling service
    }

    /// <summary>
    /// Internal class to hold audit entry data
    /// </summary>
    private class AuditEntry
    {
        public string TableName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public AuditAction Action { get; set; }
        public object Entity { get; set; } = null!;
        public Dictionary<string, object?> OriginalValues { get; set; } = new();
        public Dictionary<string, object?> CurrentValues { get; set; } = new();
        public List<string> ModifiedProperties { get; set; } = new();
    }
}