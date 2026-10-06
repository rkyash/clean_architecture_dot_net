using Microsoft.EntityFrameworkCore;
using MosquitoApi.Domain.Entities;
using MosquitoApi.Infrastructure.Data.Configurations;

namespace MosquitoApi.Infrastructure.Data;

/// <summary>
/// Separate database context for audit logging data
/// </summary>
public class AuditDbContext : DbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Audit log entities
    /// </summary>
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Enable sensitive data logging in development
        if (System.Diagnostics.Debugger.IsAttached)
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
        }

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());

        // Configure schema
        modelBuilder.HasDefaultSchema("audit");

        // Configure PostgreSQL-specific features for audit logs
        ConfigurePostgreSqlFeatures(modelBuilder);
    }

    /// <summary>
    /// Configures PostgreSQL-specific features for audit logs
    /// </summary>
    private static void ConfigurePostgreSqlFeatures(ModelBuilder modelBuilder)
    {
        // Add indexes for common audit queries
        modelBuilder.Entity<AuditLog>()
            .HasIndex(a => a.TableName)
            .HasDatabaseName("ix_audit_logs_table_name");

        modelBuilder.Entity<AuditLog>()
            .HasIndex(a => a.EntityId)
            .HasDatabaseName("ix_audit_logs_entity_id");

        modelBuilder.Entity<AuditLog>()
            .HasIndex(a => a.Action)
            .HasDatabaseName("ix_audit_logs_action");

        modelBuilder.Entity<AuditLog>()
            .HasIndex(a => a.UserId)
            .HasDatabaseName("ix_audit_logs_user_id")
            .HasFilter("user_id IS NOT NULL");

        modelBuilder.Entity<AuditLog>()
            .HasIndex(a => a.ActionTimestamp)
            .HasDatabaseName("ix_audit_logs_action_timestamp");

        modelBuilder.Entity<AuditLog>()
            .HasIndex(a => a.CorrelationId)
            .HasDatabaseName("ix_audit_logs_correlation_id")
            .HasFilter("correlation_id IS NOT NULL");

        // Composite indexes for common audit search patterns
        modelBuilder.Entity<AuditLog>()
            .HasIndex(a => new { a.TableName, a.EntityId })
            .HasDatabaseName("ix_audit_logs_table_entity");

        modelBuilder.Entity<AuditLog>()
            .HasIndex(a => new { a.UserId, a.ActionTimestamp })
            .HasDatabaseName("ix_audit_logs_user_timestamp")
            .HasFilter("user_id IS NOT NULL");

        modelBuilder.Entity<AuditLog>()
            .HasIndex(a => new { a.TableName, a.Action, a.ActionTimestamp })
            .HasDatabaseName("ix_audit_logs_table_action_timestamp");

        // Index for correlation ID tracking
        modelBuilder.Entity<AuditLog>()
            .HasIndex(a => new { a.CorrelationId, a.ActionTimestamp })
            .HasDatabaseName("ix_audit_logs_correlation_timestamp")
            .HasFilter("correlation_id IS NOT NULL");
    }

    /// <summary>
    /// Saves changes with automatic timestamp updates
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update timestamps before saving
        UpdateTimestamps();

        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Saves changes synchronously with automatic timestamp updates
    /// </summary>
    public override int SaveChanges()
    {
        // Update timestamps before saving
        UpdateTimestamps();

        return base.SaveChanges();
    }

    /// <summary>
    /// Updates timestamps for audit log entities being added
    /// </summary>
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<AuditLog>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    if (entry.Entity.ActionTimestamp == default)
                    {
                        entry.Entity.ActionTimestamp = DateTime.UtcNow;
                    }
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    // Don't update CreatedAt or ActionTimestamp for audit logs
                    entry.Property(e => e.CreatedAt).IsModified = false;
                    entry.Property(e => e.ActionTimestamp).IsModified = false;
                    break;
            }
        }
    }
}