using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MosquitoApi.Domain.Interfaces;
using MosquitoApi.Infrastructure.Data;
using MosquitoApi.Infrastructure.Data.Interceptors;
using MosquitoApi.Infrastructure.Repositories;
using MosquitoApi.Infrastructure.Services;

namespace MosquitoApi.Infrastructure.DependencyInjection;

/// <summary>
/// Extension methods for registering infrastructure layer services
/// </summary>
public static class InfrastructureServiceRegistration
{
    /// <summary>
    /// Registers infrastructure layer services in the dependency injection container
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Register database contexts
        services.AddDatabaseContexts(configuration);

        // Register repositories
        services.AddScoped<IMosquitoRepository, MosquitoRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITokenRepository, TokenRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register infrastructure services
        services.AddSingleton<IDateTimeService, DateTimeService>();
        services.AddScoped<AuditInterceptor>();

        // Register additional services (can be extended)
        services.AddMemoryCache();

        return services;
    }

    /// <summary>
    /// Registers database contexts with connection strings from configuration
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddDatabaseContexts(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Get connection strings
        var mosquitoConnectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection string is not configured");

        var auditConnectionString = configuration.GetConnectionString("AuditConnection")
            ?? configuration.GetConnectionString("DefaultConnection") // Fallback to default connection
            ?? throw new InvalidOperationException("AuditConnection string is not configured");

        // Register main database context
        services.AddDbContext<MosquitoDbContext>((serviceProvider, options) =>
        {
            var auditInterceptor = serviceProvider.GetRequiredService<AuditInterceptor>();
            
            if (mosquitoConnectionString == "InMemory")
            {
                options.UseInMemoryDatabase("MosquitoDatabase");
            }
            else
            {
                options.UseNpgsql(mosquitoConnectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "mosquito");
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null);
                });
            }

            // Configure for development
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // Register audit database context
        services.AddDbContext<AuditDbContext>(options =>
        {
            if (auditConnectionString == "InMemory")
            {
                options.UseInMemoryDatabase("AuditDatabase");
            }
            else
            {
                options.UseNpgsql(auditConnectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "audit");
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null);
                });
            }

            // Configure for development
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        return services;
    }

    /// <summary>
    /// Registers database contexts for testing with in-memory databases
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="databaseName">Name for the in-memory database (optional)</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddInMemoryDatabaseContexts(
        this IServiceCollection services,
        string? databaseName = null)
    {
        var dbName = databaseName ?? Guid.NewGuid().ToString();

        // Register in-memory database contexts for testing
        services.AddDbContext<MosquitoDbContext>((serviceProvider, options) =>
        {
            var auditInterceptor = serviceProvider.GetRequiredService<AuditInterceptor>();
            options.UseInMemoryDatabase($"MosquitoTestDb_{dbName}");
        });

        services.AddDbContext<AuditDbContext>(options =>
        {
            options.UseInMemoryDatabase($"AuditTestDb_{dbName}");
        });

        return services;
    }

    /// <summary>
    /// Configures health checks for the infrastructure services
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddInfrastructureHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var mosquitoConnectionString = configuration.GetConnectionString("DefaultConnection");
        var auditConnectionString = configuration.GetConnectionString("AuditConnection") 
            ?? configuration.GetConnectionString("DefaultConnection");

        var healthChecksBuilder = services.AddHealthChecks();

        if (mosquitoConnectionString != "InMemory")
        {
            healthChecksBuilder
                .AddNpgSql(
                    mosquitoConnectionString!,
                    name: "mosquito-database",
                    tags: new[] { "database", "postgresql", "main" });
        }

        if (auditConnectionString != "InMemory" && auditConnectionString != mosquitoConnectionString)
        {
            healthChecksBuilder
                .AddNpgSql(
                    auditConnectionString!,
                    name: "audit-database",
                    tags: new[] { "database", "postgresql", "audit" });
        }

        return services;
    }

    /// <summary>
    /// Ensures databases are created and migrated
    /// </summary>
    /// <param name="serviceProvider">Service provider</param>
    /// <returns>Task representing the async operation</returns>
    public static async Task EnsureDatabasesCreatedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        // Ensure main database is created and migrated
        var mosquitoContext = scope.ServiceProvider.GetRequiredService<MosquitoDbContext>();
        if (mosquitoContext.Database.IsInMemory())
        {
            await mosquitoContext.Database.EnsureCreatedAsync();
        }
        else
        {
            await mosquitoContext.Database.MigrateAsync();
        }

        // Ensure audit database is created and migrated
        var auditContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        if (auditContext.Database.IsInMemory())
        {
            await auditContext.Database.EnsureCreatedAsync();
        }
        else
        {
            await auditContext.Database.MigrateAsync();
        }
    }

    /// <summary>
    /// Seeds the database with initial data
    /// </summary>
    /// <param name="serviceProvider">Service provider</param>
    /// <returns>Task representing the async operation</returns>
    public static async Task SeedDatabaseAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var mosquitoContext = scope.ServiceProvider.GetRequiredService<MosquitoDbContext>();

        // Check if database already has data
        if (await mosquitoContext.Mosquitoes.AnyAsync())
        {
            return; // Database is already seeded
        }

        // Add seed data here if needed
        // For this implementation, we'll leave it empty as seeding should be done through migrations or separate services
    }
}