using System.Text.Json;
using MosquitoApi.Application.Common;
using MosquitoApi.Application.Interfaces;
using MosquitoApi.Domain.Entities;
using MosquitoApi.Domain.Enums;
using MosquitoApi.Domain.Interfaces;

namespace MosquitoApi.Application.Services;

/// <summary>
/// Implementation of audit logging service
/// </summary>
public class AuditService : IAuditService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuditService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        
        // Configure JSON serialization options for audit logging
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    public async Task<Result> LogAsync(
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
        CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLog = new AuditLog
            {
                TableName = tableName,
                EntityId = entityId,
                Action = action,
                OldValues = oldValues,
                NewValues = newValues,
                UserId = userId,
                Username = username,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                CorrelationId = correlationId,
                Context = context,
                ActionTimestamp = DateTime.UtcNow
            };

            // Validate audit log entry
            var validationErrors = auditLog.Validate();
            if (validationErrors.Count > 0)
            {
                return Result.ValidationFailure(validationErrors);
            }

            // Detect changed fields for updates
            if (action == AuditAction.Update && !string.IsNullOrEmpty(oldValues) && !string.IsNullOrEmpty(newValues))
            {
                auditLog.ChangedFields = DetectChangedFields(oldValues, newValues);
            }

            await _unitOfWork.AuditRepository.AddAsync(auditLog, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            // Log the exception but don't throw - audit failures shouldn't break the main operation
            // In a production system, you might want to log this to a separate error logging system
            return Result.Failure($"Audit logging failed: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<AuditLog>>> GetEntityAuditTrailAsync(
        string entityId, 
        string? tableName = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLogs = await _unitOfWork.AuditRepository.GetEntityAuditTrailAsync(entityId, tableName, cancellationToken);
            return Result<IEnumerable<AuditLog>>.Success(auditLogs);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<AuditLog>>.Failure(ex);
        }
    }

    public async Task<Result<IEnumerable<AuditLog>>> GetAuditLogsAsync(
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
        try
        {
            // Validate pagination parameters
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 1000) pageSize = 50;

            var auditLogs = await _unitOfWork.AuditRepository.SearchAsync(
                userId: userId,
                tableName: tableName,
                action: action,
                entityId: entityId,
                fromDate: fromDate,
                toDate: toDate,
                correlationId: correlationId,
                pageNumber: pageNumber,
                pageSize: pageSize,
                cancellationToken: cancellationToken);

            return Result<IEnumerable<AuditLog>>.Success(auditLogs);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<AuditLog>>.Failure(ex);
        }
    }

    public async Task<Result<Dictionary<AuditAction, int>>> GetAuditStatisticsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var statistics = await _unitOfWork.AuditRepository.GetActionStatisticsAsync(cancellationToken);
            return Result<Dictionary<AuditAction, int>>.Success(statistics);
        }
        catch (Exception ex)
        {
            return Result<Dictionary<AuditAction, int>>.Failure(ex);
        }
    }

    public async Task<Result<Dictionary<string, int>>> GetTopActiveUsersAsync(int topCount = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            if (topCount < 1) topCount = 10;
            if (topCount > 100) topCount = 100;

            var topUsers = await _unitOfWork.AuditRepository.GetTopActiveUsersAsync(topCount, cancellationToken);
            return Result<Dictionary<string, int>>.Success(topUsers);
        }
        catch (Exception ex)
        {
            return Result<Dictionary<string, int>>.Failure(ex);
        }
    }

    public async Task<Result<IEnumerable<AuditLog>>> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                return Result<IEnumerable<AuditLog>>.Failure("Correlation ID is required");
            }

            var auditLogs = await _unitOfWork.AuditRepository.GetByCorrelationIdAsync(correlationId, cancellationToken);
            return Result<IEnumerable<AuditLog>>.Success(auditLogs);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<AuditLog>>.Failure(ex);
        }
    }

    public async Task<Result> LogEntityCreatedAsync<T>(T entity, string? userId = null, string? correlationId = null, string? context = null, CancellationToken cancellationToken = default)
        where T : class
    {
        try
        {
            var tableName = GetTableName<T>();
            var entityId = GetEntityId(entity);
            var newValues = SerializeEntity(entity);

            return await LogAsync(
                tableName: tableName,
                entityId: entityId,
                action: AuditAction.Create,
                oldValues: null,
                newValues: newValues,
                userId: userId,
                correlationId: correlationId,
                context: context,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to log entity creation: {ex.Message}");
        }
    }

    public async Task<Result> LogEntityUpdatedAsync<T>(T oldEntity, T newEntity, string? userId = null, string? correlationId = null, string? context = null, CancellationToken cancellationToken = default)
        where T : class
    {
        try
        {
            var tableName = GetTableName<T>();
            var entityId = GetEntityId(newEntity);
            var oldValues = SerializeEntity(oldEntity);
            var newValues = SerializeEntity(newEntity);

            return await LogAsync(
                tableName: tableName,
                entityId: entityId,
                action: AuditAction.Update,
                oldValues: oldValues,
                newValues: newValues,
                userId: userId,
                correlationId: correlationId,
                context: context,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to log entity update: {ex.Message}");
        }
    }

    public async Task<Result> LogEntityDeletedAsync<T>(T entity, string? userId = null, string? correlationId = null, string? context = null, CancellationToken cancellationToken = default)
        where T : class
    {
        try
        {
            var tableName = GetTableName<T>();
            var entityId = GetEntityId(entity);
            var oldValues = SerializeEntity(entity);

            return await LogAsync(
                tableName: tableName,
                entityId: entityId,
                action: AuditAction.Delete,
                oldValues: oldValues,
                newValues: null,
                userId: userId,
                correlationId: correlationId,
                context: context,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to log entity deletion: {ex.Message}");
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Gets the table name for an entity type
    /// </summary>
    private static string GetTableName<T>() where T : class
    {
        var type = typeof(T);
        return type.Name;
    }

    /// <summary>
    /// Gets the entity ID from an entity instance
    /// </summary>
    private static string GetEntityId<T>(T entity) where T : class
    {
        if (entity == null) return string.Empty;

        // Try to get Id property using reflection
        var idProperty = entity.GetType().GetProperty("Id");
        if (idProperty != null)
        {
            var id = idProperty.GetValue(entity);
            return id?.ToString() ?? string.Empty;
        }

        // Fallback to hash code if no Id property
        return entity.GetHashCode().ToString();
    }

    /// <summary>
    /// Serializes an entity to JSON for audit purposes
    /// </summary>
    private string SerializeEntity<T>(T entity) where T : class
    {
        if (entity == null) return string.Empty;

        try
        {
            return JsonSerializer.Serialize(entity, _jsonOptions);
        }
        catch (Exception)
        {
            // If serialization fails, return a basic representation
            return $"{{\"Type\":\"{entity.GetType().Name}\",\"ToString\":\"{entity}\"}}";
        }
    }

    /// <summary>
    /// Detects changed fields between old and new JSON values
    /// </summary>
    private string DetectChangedFields(string oldValues, string newValues)
    {
        try
        {
            var changedFields = new Dictionary<string, object>();

            using var oldDoc = JsonDocument.Parse(oldValues);
            using var newDoc = JsonDocument.Parse(newValues);

            CompareJsonElements(oldDoc.RootElement, newDoc.RootElement, changedFields, string.Empty);

            return JsonSerializer.Serialize(changedFields, _jsonOptions);
        }
        catch (Exception)
        {
            // If comparison fails, return empty object
            return "{}";
        }
    }

    /// <summary>
    /// Recursively compares JSON elements to find changes
    /// </summary>
    private static void CompareJsonElements(JsonElement oldElement, JsonElement newElement, Dictionary<string, object> changedFields, string path)
    {
        if (oldElement.ValueKind != newElement.ValueKind)
        {
            changedFields[path] = new { Old = GetJsonValue(oldElement), New = GetJsonValue(newElement) };
            return;
        }

        switch (oldElement.ValueKind)
        {
            case JsonValueKind.Object:
                CompareJsonObjects(oldElement, newElement, changedFields, path);
                break;
            case JsonValueKind.Array:
                CompareJsonArrays(oldElement, newElement, changedFields, path);
                break;
            default:
                if (!JsonElementsEqual(oldElement, newElement))
                {
                    changedFields[path] = new { Old = GetJsonValue(oldElement), New = GetJsonValue(newElement) };
                }
                break;
        }
    }

    /// <summary>
    /// Compares JSON objects
    /// </summary>
    private static void CompareJsonObjects(JsonElement oldObj, JsonElement newObj, Dictionary<string, object> changedFields, string basePath)
    {
        var oldProperties = oldObj.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);
        var newProperties = newObj.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);

        // Check for changed and removed properties
        foreach (var (key, value) in oldProperties)
        {
            var currentPath = string.IsNullOrEmpty(basePath) ? key : $"{basePath}.{key}";
            
            if (newProperties.TryGetValue(key, out var newValue))
            {
                CompareJsonElements(value, newValue, changedFields, currentPath);
            }
            else
            {
                // Property was removed
                changedFields[currentPath] = new { Old = GetJsonValue(value), New = (object?)null };
            }
        }

        // Check for added properties
        foreach (var (key, value) in newProperties)
        {
            if (!oldProperties.ContainsKey(key))
            {
                var currentPath = string.IsNullOrEmpty(basePath) ? key : $"{basePath}.{key}";
                changedFields[currentPath] = new { Old = (object?)null, New = GetJsonValue(value) };
            }
        }
    }

    /// <summary>
    /// Compares JSON arrays
    /// </summary>
    private static void CompareJsonArrays(JsonElement oldArray, JsonElement newArray, Dictionary<string, object> changedFields, string path)
    {
        var oldItems = oldArray.EnumerateArray().ToArray();
        var newItems = newArray.EnumerateArray().ToArray();

        if (oldItems.Length != newItems.Length)
        {
            changedFields[path] = new 
            { 
                Old = oldItems.Select(GetJsonValue).ToArray(), 
                New = newItems.Select(GetJsonValue).ToArray() 
            };
            return;
        }

        for (var i = 0; i < oldItems.Length; i++)
        {
            CompareJsonElements(oldItems[i], newItems[i], changedFields, $"{path}[{i}]");
        }
    }

    /// <summary>
    /// Checks if two JSON elements are equal
    /// </summary>
    private static bool JsonElementsEqual(JsonElement element1, JsonElement element2)
    {
        if (element1.ValueKind != element2.ValueKind) return false;

        return element1.ValueKind switch
        {
            JsonValueKind.String => element1.GetString() == element2.GetString(),
            JsonValueKind.Number => element1.GetDecimal() == element2.GetDecimal(),
            JsonValueKind.True or JsonValueKind.False => element1.GetBoolean() == element2.GetBoolean(),
            JsonValueKind.Null => true,
            _ => element1.GetRawText() == element2.GetRawText()
        };
    }

    /// <summary>
    /// Gets the value of a JSON element as an object
    /// </summary>
    private static object? GetJsonValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.GetDecimal(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.GetRawText()
        };
    }

    #endregion
}