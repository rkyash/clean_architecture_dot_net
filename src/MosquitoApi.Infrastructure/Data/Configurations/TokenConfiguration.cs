using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MosquitoApi.Domain.Entities;

namespace MosquitoApi.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for Token entity
/// </summary>
public class TokenConfiguration : IEntityTypeConfiguration<Token>
{
    public void Configure(EntityTypeBuilder<Token> builder)
    {
        // Table name
        builder.ToTable("tokens");

        // Primary key
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // Foreign key to User
        builder.Property(t => t.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        // Token value
        builder.Property(t => t.RefreshToken)
            .HasColumnName("refresh_token")
            .HasMaxLength(500)
            .IsRequired();

        // Token expiration
        builder.Property(t => t.ExpiresAt)
            .HasColumnName("expiration_date")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Creation details
        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_date")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(t => t.CreatedByIp)
            .HasColumnName("created_by_ip_address")
            .HasMaxLength(45); // IPv6 max length

        builder.Property(t => t.UserAgent)
            .HasColumnName("created_by_user_agent")
            .HasMaxLength(500);

        // Status flags
        builder.Property(t => t.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        // Revocation details - RevokedAt is stored, IsRevoked is computed
        builder.Property(t => t.RevokedAt)
            .HasColumnName("revoked_date")
            .HasColumnType("timestamp with time zone");

        builder.Property(t => t.RevokedByIp)
            .HasColumnName("revoked_by_ip_address")
            .HasMaxLength(45);

        builder.Property(t => t.RevokedReason)
            .HasColumnName("reason_revoked")
            .HasMaxLength(200);

        builder.Property(t => t.ReplacedByToken)
            .HasColumnName("replaced_by_token")
            .HasMaxLength(500);

        // Computed properties - these are calculated in C# code, not stored in database
        builder.Ignore(t => t.IsExpired);
        builder.Ignore(t => t.IsValid);
        builder.Ignore(t => t.IsRevoked);

        // Unique constraint on refresh token
        builder.HasIndex(t => t.RefreshToken)
            .IsUnique()
            .HasDatabaseName("ix_tokens_refresh_token_unique");

        // Performance indexes
        builder.HasIndex(t => t.UserId)
            .HasDatabaseName("ix_tokens_user_id");

        builder.HasIndex(t => t.ExpiresAt)
            .HasDatabaseName("ix_tokens_expiration_date");

        builder.HasIndex(t => t.CreatedAt)
            .HasDatabaseName("ix_tokens_created_date");

        builder.HasIndex(t => t.IsActive)
            .HasDatabaseName("ix_tokens_is_active");

        builder.HasIndex(t => t.CreatedByIp)
            .HasDatabaseName("ix_tokens_created_by_ip")
            .HasFilter("created_by_ip_address IS NOT NULL");

        // Composite indexes for common queries
        builder.HasIndex(t => new { t.UserId, t.IsActive })
            .HasDatabaseName("ix_tokens_user_active");

        builder.HasIndex(t => new { t.UserId, t.RevokedAt })
            .HasDatabaseName("ix_tokens_user_revoked");

        builder.HasIndex(t => new { t.IsActive, t.ExpiresAt })
            .HasDatabaseName("ix_tokens_active_expiration");

        // Partial indexes for performance optimization
        builder.HasIndex(t => t.Id)
            .HasDatabaseName("ix_tokens_id_active")
            .HasFilter("is_active = true AND revoked_date IS NULL");

        builder.HasIndex(t => t.ExpiresAt)
            .HasDatabaseName("ix_tokens_expiry_cleanup");

        // Index for cleanup operations
        builder.HasIndex(t => new { t.CreatedAt, t.RevokedAt })
            .HasDatabaseName("ix_tokens_cleanup")
            .HasFilter("revoked_date IS NOT NULL");

        // Foreign key relationship (optional - can be configured in User configuration instead)
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}