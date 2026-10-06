using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MosquitoApi.Domain.Entities;
using MosquitoApi.Domain.Enums;
using MosquitoApi.Domain.ValueObjects;
using System.Text.Json;

namespace MosquitoApi.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for Mosquito entity
/// </summary>
public class MosquitoConfiguration : IEntityTypeConfiguration<Mosquito>
{
    public void Configure(EntityTypeBuilder<Mosquito> builder)
    {
        // Table configuration
        builder.ToTable("mosquitoes");

        // Primary key
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .HasColumnName("id")
            .ValueGeneratedNever(); // We generate GUIDs in the domain

        // Basic properties
        builder.Property(m => m.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(m => m.Species)
            .HasColumnName("species")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(m => m.Size)
            .HasColumnName("size")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(m => m.LengthMm)
            .HasColumnName("length_mm")
            .HasColumnType("decimal(6,2)")
            .IsRequired();

        builder.Property(m => m.WeightMg)
            .HasColumnName("weight_mg")
            .HasColumnType("decimal(8,3)");

        builder.Property(m => m.Color)
            .HasColumnName("color")
            .HasMaxLength(100);

        // Location as JSON column (PostgreSQL JSONB)
        builder.Property(m => m.Location)
            .HasColumnName("location")
            .HasColumnType("jsonb")
            .HasConversion(
                location => JsonSerializer.Serialize(new
                {
                    latitude = location.Latitude,
                    longitude = location.Longitude,
                    address = location.Address,
                    city = location.City,
                    country = location.Country
                }, (JsonSerializerOptions?)null),
                json => DeserializeLocation(json))
            .IsRequired();

        // Observation properties
        builder.Property(m => m.ObservedAt)
            .HasColumnName("observed_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(m => m.TemperatureCelsius)
            .HasColumnName("temperature_celsius")
            .HasColumnType("decimal(5,2)");

        builder.Property(m => m.HumidityPercent)
            .HasColumnName("humidity_percent")
            .HasColumnType("decimal(5,2)");

        builder.Property(m => m.Notes)
            .HasColumnName("notes")
            .HasMaxLength(1000);

        // Disease properties
        builder.Property(m => m.IsDiseaseVector)
            .HasColumnName("is_disease_vector")
            .IsRequired();

        builder.Property(m => m.DiseaseTransmission)
            .HasColumnName("disease_transmission")
            .HasMaxLength(500);

        // Audit properties
        builder.Property(m => m.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(50);

        builder.Property(m => m.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(50);

        builder.Property(m => m.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(m => m.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone");

        // Soft delete properties
        builder.Property(m => m.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(m => m.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamp with time zone");

        // Configure enum value mappings for better database readability
        ConfigureEnumMappings(builder);
    }

    /// <summary>
    /// Configures enum value mappings for species and size
    /// </summary>
    private static void ConfigureEnumMappings(EntityTypeBuilder<Mosquito> builder)
    {
        builder.Property(m => m.Species)
            .HasConversion(
                species => (int)species,
                value => (MosquitoSpecies)value);

        builder.Property(m => m.Size)
            .HasConversion(
                size => (int)size,
                value => (MosquitoSize)value);
    }

    /// <summary>
    /// Helper method for deserializing Location from JSON
    /// </summary>
    private static Location DeserializeLocation(string json)
    {
        var locationJson = JsonSerializer.Deserialize<LocationJson>(json, (JsonSerializerOptions?)null);
        if (locationJson == null)
            throw new InvalidOperationException("Failed to deserialize location");
        
        return new Location(locationJson.Latitude, locationJson.Longitude, 
            locationJson.Address, locationJson.City, locationJson.Country);
    }

    /// <summary>
    /// Helper class for JSON serialization/deserialization of Location
    /// </summary>
    private class LocationJson
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
    }
}