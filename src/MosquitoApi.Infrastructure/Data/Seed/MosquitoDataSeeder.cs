using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MosquitoApi.Domain.Entities;
using MosquitoApi.Domain.Enums;
using MosquitoApi.Domain.ValueObjects;

namespace MosquitoApi.Infrastructure.Data.Seed;

/// <summary>
/// Seeds initial data for mosquito entities
/// </summary>
public class MosquitoDataSeeder
{
    private readonly MosquitoDbContext _context;
    private readonly ILogger<MosquitoDataSeeder> _logger;

    public MosquitoDataSeeder(MosquitoDbContext context, ILogger<MosquitoDataSeeder> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Seeds initial mosquito data if database is empty
    /// </summary>
    public async Task SeedAsync()
    {
        try
        {
            // Check if database has any data
            var hasData = await _context.Mosquitoes.AnyAsync();
            if (hasData)
            {
                _logger.LogInformation("Database already has mosquito data. Skipping seeding.");
                return;
            }

            _logger.LogInformation("Seeding initial mosquito data...");

            var mosquitoes = GetSeedData();
            
            await _context.Mosquitoes.AddRangeAsync(mosquitoes);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully seeded {Count} mosquitoes", mosquitoes.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding mosquito data");
            throw;
        }
    }

    /// <summary>
    /// Gets seed data for mosquitoes
    /// </summary>
    private static List<Mosquito> GetSeedData()
    {
        var baseDate = DateTime.UtcNow.AddDays(-30);
        
        return new List<Mosquito>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Species = MosquitoSpecies.AedesAegypti,
                Size = MosquitoSize.Medium,
                Location = new Location(40.7128, -74.0060, "New York City, NY"),
                IsDiseaseVector = true,
                ObservedAt = baseDate.AddDays(1),
                Notes = "Common yellow fever mosquito found in urban area",
                CreatedAt = baseDate.AddDays(1),
                CreatedBy = "system",
                IsDeleted = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Species = MosquitoSpecies.AnophelesGambiae,
                Size = MosquitoSize.Large,
                Location = new Location(34.0522, -118.2437, "Los Angeles, CA"),
                IsDiseaseVector = true,
                ObservedAt = baseDate.AddDays(2),
                Notes = "Primary malaria vector in urban environment",
                CreatedAt = baseDate.AddDays(2),
                CreatedBy = "system",
                IsDeleted = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Species = MosquitoSpecies.CulexPipiens,
                Size = MosquitoSize.Small,
                Location = new Location(41.8781, -87.6298, "Chicago, IL"),
                IsDiseaseVector = false,
                ObservedAt = baseDate.AddDays(3),
                Notes = "Common house mosquito, not typically disease vector",
                CreatedAt = baseDate.AddDays(3),
                CreatedBy = "system",
                IsDeleted = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Species = MosquitoSpecies.AedesAlbopictus,
                Size = MosquitoSize.Medium,
                Location = new Location(29.7604, -95.3698, "Houston, TX"),
                IsDiseaseVector = true,
                ObservedAt = baseDate.AddDays(4),
                Notes = "Asian tiger mosquito, aggressive day biter",
                CreatedAt = baseDate.AddDays(4),
                CreatedBy = "system",
                IsDeleted = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Species = MosquitoSpecies.AnophelesStephensi,
                Size = MosquitoSize.Large,
                Location = new Location(33.4484, -112.0740, "Phoenix, AZ"),
                IsDiseaseVector = true,
                ObservedAt = baseDate.AddDays(5),
                Notes = "Asian malaria mosquito, expanding range",
                CreatedAt = baseDate.AddDays(5),
                CreatedBy = "system",
                IsDeleted = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Species = MosquitoSpecies.CulexQuinquefasciatus,
                Size = MosquitoSize.Medium,
                Location = new Location(39.7392, -104.9903, "Denver, CO"),
                IsDiseaseVector = true,
                ObservedAt = baseDate.AddDays(6),
                Notes = "Southern house mosquito, West Nile virus vector",
                CreatedAt = baseDate.AddDays(6),
                CreatedBy = "system",
                IsDeleted = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Species = MosquitoSpecies.AedesAegypti,
                Size = MosquitoSize.Small,
                Location = new Location(25.7617, -80.1918, "Miami, FL"),
                IsDiseaseVector = true,
                ObservedAt = baseDate.AddDays(7),
                Notes = "Yellow fever mosquito in tropical climate",
                CreatedAt = baseDate.AddDays(7),
                CreatedBy = "system",
                IsDeleted = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Species = MosquitoSpecies.CulexPipiens,
                Size = MosquitoSize.Large,
                Location = new Location(47.6062, -122.3321, "Seattle, WA"),
                IsDiseaseVector = false,
                ObservedAt = baseDate.AddDays(8),
                Notes = "House mosquito adapted to temperate climate",
                CreatedAt = baseDate.AddDays(8),
                CreatedBy = "system",
                IsDeleted = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Species = MosquitoSpecies.AnophelesGambiae,
                Size = MosquitoSize.Medium,
                Location = new Location(37.7749, -122.4194, "San Francisco, CA"),
                IsDiseaseVector = true,
                ObservedAt = baseDate.AddDays(9),
                Notes = "Malaria mosquito in coastal environment",
                CreatedAt = baseDate.AddDays(9),
                CreatedBy = "system",
                IsDeleted = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Species = MosquitoSpecies.AedesAlbopictus,
                Size = MosquitoSize.Small,
                Location = new Location(42.3601, -71.0589, "Boston, MA"),
                IsDiseaseVector = true,
                ObservedAt = baseDate.AddDays(10),
                Notes = "Tiger mosquito expanding range northward",
                CreatedAt = baseDate.AddDays(10),
                CreatedBy = "system",
                IsDeleted = false
            }
        };
    }
}