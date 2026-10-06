using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MosquitoApi.Domain.Entities;
using MosquitoApi.Domain.Enums;

namespace MosquitoApi.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for AuditLog entity
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        // Table configuration
        builder.ToTable("audit_logs");

        // Primary key
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasColumnName("id")
            .ValueGeneratedNever(); // We generate GUIDs in the domain

        // Basic audit properties
        builder.Property(a => a.TableName)
            .HasColumnName("table_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.EntityId)
            .HasColumnName("entity_id")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.Action)
            .HasColumnName("action")
            .HasConversion<int>()
            .IsRequired();

        // JSON data columns (PostgreSQL JSONB for better performance)
        builder.Property(a => a.OldValues)
            .HasColumnName("old_values")
            .HasColumnType("jsonb");

        builder.Property(a => a.NewValues)
            .HasColumnName("new_values")
            .HasColumnType("jsonb");

        builder.Property(a => a.ChangedFields)
            .HasColumnName("changed_fields")
            .HasColumnType("jsonb");

        // User information
        builder.Property(a => a.UserId)
            .HasColumnName("user_id")
            .HasMaxLength(50);

        builder.Property(a => a.Username)
            .HasColumnName("username")
            .HasMaxLength(100);

        // Request information
        builder.Property(a => a.IpAddress)
            .HasColumnName("ip_address")
            .HasMaxLength(45); // IPv6 addresses can be up to 45 characters

        builder.Property(a => a.UserAgent)
            .HasColumnName("user_agent")
            .HasMaxLength(500);

        // Tracking information
        builder.Property(a => a.CorrelationId)
            .HasColumnName("correlation_id")
            .HasMaxLength(100);

        builder.Property(a => a.Context)
            .HasColumnName("context")
            .HasMaxLength(500);

        // Timestamp properties
        builder.Property(a => a.ActionTimestamp)
            .HasColumnName("action_timestamp")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone");

        // Soft delete properties (though audit logs should rarely be deleted)
        builder.Property(a => a.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(a => a.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamp with time zone");

        // Configure enum value mappings
        ConfigureEnumMappings(builder);

        // Add constraints
        ConfigureConstraints(builder);
    }

    /// <summary>
    /// Configures enum value mappings for audit actions
    /// </summary>
    private static void ConfigureEnumMappings(EntityTypeBuilder<AuditLog> builder)
    {
        builder.Property(a => a.Action)
            .HasConversion(
                action => (int)action,
                value => (AuditAction)value);
    }

    /// <summary>
    /// Configures database constraints
    /// </summary>
    private static void ConfigureConstraints(EntityTypeBuilder<AuditLog> builder)
    {
        // Check constraint to ensure action timestamp is not in the future
        builder.HasCheckConstraint(
            "ck_audit_logs_action_timestamp_not_future",
            "action_timestamp <= CURRENT_TIMESTAMP");

        // Check constraint to ensure created_at is not in the future
        builder.HasCheckConstraint(
            "ck_audit_logs_created_at_not_future",
            "created_at <= CURRENT_TIMESTAMP");

        // Check constraint for valid action values
        builder.HasCheckConstraint(
            "ck_audit_logs_valid_action",
            "action IN (1, 2, 3)"); // Create, Update, Delete

        // Check constraint to ensure old_values is null for create operations
        builder.HasCheckConstraint(
            "ck_audit_logs_create_no_old_values",
            "(action = 1 AND old_values IS NULL) OR action != 1");

        // Check constraint to ensure new_values is null for delete operations
        builder.HasCheckConstraint(
            "ck_audit_logs_delete_no_new_values",
            "(action = 3 AND new_values IS NULL) OR action != 3");
    }
}