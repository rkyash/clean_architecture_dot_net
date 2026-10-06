using Microsoft.EntityFrameworkCore;
using MosquitoApi.Domain.Entities;
using MosquitoApi.Infrastructure.Data.Configurations;
using MosquitoApi.Infrastructure.Data.Interceptors;

namespace MosquitoApi.Infrastructure.Data;

/// <summary>
/// Main database context for mosquito business data
/// </summary>
public class MosquitoDbContext : DbContext
{
    private readonly AuditInterceptor _auditInterceptor;

    public MosquitoDbContext(DbContextOptions<MosquitoDbContext> options, AuditInterceptor auditInterceptor)
        : base(options)
    {
        _auditInterceptor = auditInterceptor;
    }

    /// <summary>
    /// Mosquito entities
    /// </summary>
    public DbSet<Mosquito> Mosquitoes => Set<Mosquito>();

    /// <summary>
    /// User entities
    /// </summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>
    /// Token entities
    /// </summary>
    public DbSet<Token> Tokens => Set<Token>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Add the audit interceptor
        optionsBuilder.AddInterceptors(_auditInterceptor);
        
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
        modelBuilder.ApplyConfiguration(new MosquitoConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new TokenConfiguration());

        // Configure schema
        modelBuilder.HasDefaultSchema("mosquito");

        // Global query filters for soft delete
        modelBuilder.Entity<Mosquito>().HasQueryFilter(m => !m.IsDeleted);
        modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);

        // Configure PostgreSQL-specific features
        ConfigurePostgreSqlFeatures(modelBuilder);
    }

    /// <summary>
    /// Configures PostgreSQL-specific features like indexes and constraints
    /// </summary>
    private static void ConfigurePostgreSqlFeatures(ModelBuilder modelBuilder)
    {
        // Add indexes for common queries
        modelBuilder.Entity<Mosquito>()
            .HasIndex(m => m.Species)
            .HasDatabaseName("ix_mosquitoes_species");

        modelBuilder.Entity<Mosquito>()
            .HasIndex(m => m.Size)
            .HasDatabaseName("ix_mosquitoes_size");

        modelBuilder.Entity<Mosquito>()
            .HasIndex(m => m.ObservedAt)
            .HasDatabaseName("ix_mosquitoes_observed_at");

        modelBuilder.Entity<Mosquito>()
            .HasIndex(m => m.IsDiseaseVector)
            .HasDatabaseName("ix_mosquitoes_disease_vector");

        modelBuilder.Entity<Mosquito>()
            .HasIndex(m => m.CreatedAt)
            .HasDatabaseName("ix_mosquitoes_created_at");

        modelBuilder.Entity<Mosquito>()
            .HasIndex(m => m.CreatedBy)
            .HasDatabaseName("ix_mosquitoes_created_by")
            .HasFilter("created_by IS NOT NULL");

        // Composite indexes for common search patterns
        modelBuilder.Entity<Mosquito>()
            .HasIndex(m => new { m.Species, m.Size })
            .HasDatabaseName("ix_mosquitoes_species_size");

        modelBuilder.Entity<Mosquito>()
            .HasIndex(m => new { m.ObservedAt, m.Species })
            .HasDatabaseName("ix_mosquitoes_observed_species");

        // Partial index for non-deleted records
        modelBuilder.Entity<Mosquito>()
            .HasIndex(m => m.Id)
            .HasDatabaseName("ix_mosquitoes_id_active")
            .HasFilter("is_deleted = false");
    }

    /// <summary>
    /// Saves changes with automatic audit trail creation
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update timestamps before saving
        UpdateTimestamps();

        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Saves changes synchronously with automatic audit trail creation
    /// </summary>
    public override int SaveChanges()
    {
        // Update timestamps before saving
        UpdateTimestamps();

        return base.SaveChanges();
    }

    /// <summary>
    /// Updates timestamps for entities being modified
    /// </summary>
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = null;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    // Don't update CreatedAt
                    entry.Property(e => e.CreatedAt).IsModified = false;
                    break;

                case EntityState.Deleted:
                    // Implement soft delete
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }
    }
}