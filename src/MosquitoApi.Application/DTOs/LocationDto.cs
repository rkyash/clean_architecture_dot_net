namespace MosquitoApi.Application.DTOs;

/// <summary>
/// Data transfer object for location information
/// </summary>
public record LocationDto
{
    /// <summary>
    /// Latitude coordinate in decimal degrees
    /// </summary>
    public double Latitude { get; init; }

    /// <summary>
    /// Longitude coordinate in decimal degrees
    /// </summary>
    public double Longitude { get; init; }

    /// <summary>
    /// Optional address or place name
    /// </summary>
    public string? Address { get; init; }

    /// <summary>
    /// Optional city name
    /// </summary>
    public string? City { get; init; }

    /// <summary>
    /// Optional country name
    /// </summary>
    public string? Country { get; init; }

    /// <summary>
    /// Formatted location string
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Default constructor
    /// </summary>
    public LocationDto() { }

    /// <summary>
    /// Constructor with parameters
    /// </summary>
    public LocationDto(double latitude, double longitude, string? address = null, string? city = null, string? country = null)
    {
        Latitude = latitude;
        Longitude = longitude;
        Address = address;
        City = city;
        Country = country;
        DisplayName = $"{latitude:F6}, {longitude:F6}";
        
        if (!string.IsNullOrEmpty(address))
            DisplayName += $" - {address}";
        if (!string.IsNullOrEmpty(city))
            DisplayName += $", {city}";
        if (!string.IsNullOrEmpty(country))
            DisplayName += $", {country}";
    }
}