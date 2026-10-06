namespace MosquitoApi.Domain.ValueObjects;

/// <summary>
/// Value object representing a geographical location
/// </summary>
public sealed record Location
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
    /// Creates a new Location instance
    /// </summary>
    /// <param name="latitude">Latitude in decimal degrees (-90 to 90)</param>
    /// <param name="longitude">Longitude in decimal degrees (-180 to 180)</param>
    /// <param name="address">Optional address</param>
    /// <param name="city">Optional city</param>
    /// <param name="country">Optional country</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when coordinates are out of valid range</exception>
    public Location(double latitude, double longitude, string? address = null, string? city = null, string? country = null)
    {
        if (latitude < -90 || latitude > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90 degrees");

        if (longitude < -180 || longitude > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180 degrees");

        Latitude = latitude;
        Longitude = longitude;
        Address = address?.Trim();
        City = city?.Trim();
        Country = country?.Trim();
    }

    /// <summary>
    /// Calculates approximate distance to another location in kilometers using Haversine formula
    /// </summary>
    /// <param name="other">Other location</param>
    /// <returns>Distance in kilometers</returns>
    public double DistanceTo(Location other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));

        const double earthRadiusKm = 6371;
        var dLat = ToRadians(other.Latitude - Latitude);
        var dLon = ToRadians(other.Longitude - Longitude);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(Latitude)) * Math.Cos(ToRadians(other.Latitude)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadiusKm * c;
    }

    /// <summary>
    /// Returns a string representation of the location
    /// </summary>
    public override string ToString()
    {
        var parts = new List<string> { $"{Latitude:F6}, {Longitude:F6}" };
        
        if (!string.IsNullOrEmpty(Address)) parts.Add(Address);
        if (!string.IsNullOrEmpty(City)) parts.Add(City);
        if (!string.IsNullOrEmpty(Country)) parts.Add(Country);

        return string.Join(" - ", parts);
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}