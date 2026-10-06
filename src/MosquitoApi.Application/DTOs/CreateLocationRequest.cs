using System.ComponentModel.DataAnnotations;

namespace MosquitoApi.Application.DTOs;

/// <summary>
/// Request DTO for creating a new location
/// </summary>
public sealed record CreateLocationRequest
{
    /// <summary>
    /// Latitude coordinate in decimal degrees (-90 to 90)
    /// </summary>
    [Range(-90.0, 90.0, ErrorMessage = "Latitude must be between -90 and 90 degrees")]
    public double Latitude { get; init; }

    /// <summary>
    /// Longitude coordinate in decimal degrees (-180 to 180)
    /// </summary>
    [Range(-180.0, 180.0, ErrorMessage = "Longitude must be between -180 and 180 degrees")]
    public double Longitude { get; init; }

    /// <summary>
    /// Optional address or place name
    /// </summary>
    [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; init; }

    /// <summary>
    /// Optional city name
    /// </summary>
    [MaxLength(100, ErrorMessage = "City name cannot exceed 100 characters")]
    public string? City { get; init; }

    /// <summary>
    /// Optional country name
    /// </summary>
    [MaxLength(100, ErrorMessage = "Country name cannot exceed 100 characters")]
    public string? Country { get; init; }
}