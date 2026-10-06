using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MosquitoApi.Domain.Entities;
using MosquitoApi.Domain.Enums;

namespace MosquitoApi.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for User entity
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Table name
        builder.ToTable("users");

        // Primary key
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // Required fields
        builder.Property(u => u.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(250);

        builder.Property(u => u.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(250);

        builder.Property(u => u.Username)
            .HasColumnName("username")
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(u => u.Password)
            .HasColumnName("password")
            .HasMaxLength(250)
            .IsRequired();

        // Optional fields
        builder.Property(u => u.FatherName)
            .HasColumnName("father_name")
            .HasMaxLength(250);

        builder.Property(u => u.CreatedById)
            .HasColumnName("created_by_id")
            .HasMaxLength(250);

        builder.Property(u => u.Mobile)
            .HasColumnName("mobile")
            .HasMaxLength(250);

        builder.Property(u => u.PhotoUrl)
            .HasColumnName("photo_url");

        builder.Property(u => u.Dob)
            .HasColumnName("dob");

        builder.Property(u => u.DateOfBirth)
            .HasColumnName("date_of_birth")
            .HasColumnType("timestamp with time zone");

        // Enums
        builder.Property(u => u.GenderType)
            .HasColumnName("gender_type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(u => u.RoleType)
            .HasColumnName("role_type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue(RoleType.User);

        // Boolean flags
        builder.Property(u => u.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.IsEmailVerified)
            .HasColumnName("is_email_verified")
            .IsRequired()
            .HasDefaultValue(false);

        // Security fields
        builder.Property(u => u.FailedPasswordAttemptCount)
            .HasColumnName("failed_password_attempt_count")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(u => u.LockoutEndDateUtc)
            .HasColumnName("lockout_end_date_utc")
            .HasColumnType("timestamp with time zone");

        builder.Property(u => u.LastLoginDateUtc)
            .HasColumnName("last_login_date_utc")
            .HasColumnType("timestamp with time zone");

        builder.Property(u => u.LastPasswordChangeDateUtc)
            .HasColumnName("last_password_change_date_utc")
            .HasColumnType("timestamp with time zone");

        // Reset tokens
        builder.Property(u => u.ResetId)
            .HasColumnName("reset_id")
            .HasMaxLength(100);

        builder.Property(u => u.ResetIdExpiration)
            .HasColumnName("reset_id_expiration")
            .HasColumnType("timestamp with time zone");

        builder.Property(u => u.EmailVerificationToken)
            .HasColumnName("email_verification_token")
            .HasMaxLength(100);

        // Audit fields (inherited from BaseEntity)
        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone");

        builder.Property(u => u.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(u => u.UpdatedBy)
            .HasColumnName("updated_by");

        builder.Property(u => u.IsDeleted)
            .HasColumnName("is_deleted")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamp with time zone");

        builder.Property(u => u.DeletedBy)
            .HasColumnName("deleted_by");

        // Computed properties - these are calculated in C# code, not stored in database
        builder.Ignore(u => u.FullName);

        // Unique constraints
        builder.HasIndex(u => u.Username)
            .IsUnique()
            .HasDatabaseName("ix_users_username_unique");

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("ix_users_email_unique");

        builder.HasIndex(u => u.Mobile)
            .IsUnique()
            .HasDatabaseName("ix_users_mobile_unique")
            .HasFilter("mobile IS NOT NULL");

        // Performance indexes
        builder.HasIndex(u => u.RoleType)
            .HasDatabaseName("ix_users_role_type");

        builder.HasIndex(u => u.IsActive)
            .HasDatabaseName("ix_users_is_active");

        builder.HasIndex(u => u.IsEmailVerified)
            .HasDatabaseName("ix_users_email_verified");

        builder.HasIndex(u => u.CreatedAt)
            .HasDatabaseName("ix_users_created_at");

        builder.HasIndex(u => u.LastLoginDateUtc)
            .HasDatabaseName("ix_users_last_login");

        builder.HasIndex(u => u.LockoutEndDateUtc)
            .HasDatabaseName("ix_users_lockout_end")
            .HasFilter("lockout_end_date_utc IS NOT NULL");

        // Composite indexes for common queries
        builder.HasIndex(u => new { u.IsActive, u.RoleType })
            .HasDatabaseName("ix_users_active_role");

        builder.HasIndex(u => new { u.IsActive, u.IsEmailVerified })
            .HasDatabaseName("ix_users_active_verified");

        // Partial index for non-deleted active users
        builder.HasIndex(u => u.Id)
            .HasDatabaseName("ix_users_id_active")
            .HasFilter("is_deleted = false AND is_active = true");

        // Full-text search index for names and usernames
        builder.HasIndex(u => new { u.FirstName, u.LastName, u.Username })
            .HasDatabaseName("ix_users_search_names");

        // Relationships - Users can have many tokens
        builder.HasMany<Token>()
            .WithOne()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}