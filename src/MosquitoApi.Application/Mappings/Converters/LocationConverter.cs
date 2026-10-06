using AutoMapper;
using MosquitoApi.Application.DTOs;
using MosquitoApi.Domain.ValueObjects;

namespace MosquitoApi.Application.Mappings.Converters;

/// <summary>
/// Custom AutoMapper converter for Location value object
/// </summary>
public class LocationToStringConverter : IValueConverter<Location, string>
{
    /// <summary>
    /// Converts Location to string representation
    /// </summary>
    public string Convert(Location sourceMember, ResolutionContext context)
    {
        if (sourceMember == null)
            return string.Empty;

        return $"{sourceMember.Latitude},{sourceMember.Longitude},{sourceMember.Address}";
    }
}

/// <summary>
/// Custom AutoMapper converter from string to Location
/// </summary>
public class StringToLocationConverter : IValueConverter<string, Location>
{
    /// <summary>
    /// Converts string representation to Location value object
    /// </summary>
    public Location Convert(string sourceMember, ResolutionContext context)
    {
        if (string.IsNullOrWhiteSpace(sourceMember))
            return new Location(0, 0, string.Empty);

        var parts = sourceMember.Split(',');
        if (parts.Length < 2)
            return new Location(0, 0, string.Empty);

        if (!double.TryParse(parts[0], out var latitude))
            latitude = 0;

        if (!double.TryParse(parts[1], out var longitude))
            longitude = 0;

        var address = parts.Length > 2 ? parts[2] : string.Empty;

        return new Location(latitude, longitude, address);
    }
}

/// <summary>
/// Converter for Location to LocationDto
/// </summary>
public class LocationToLocationDtoConverter : ITypeConverter<Location, LocationDto>
{
    /// <summary>
    /// Converts Location to LocationDto
    /// </summary>
    public LocationDto Convert(Location source, LocationDto destination, ResolutionContext context)
    {
        if (source == null)
            return new LocationDto();

        return new LocationDto
        {
            Latitude = source.Latitude,
            Longitude = source.Longitude,
            Address = source.Address
        };
    }
}

/// <summary>
/// Converter for LocationDto to Location
/// </summary>
public class LocationDtoToLocationConverter : ITypeConverter<LocationDto, Location>
{
    /// <summary>
    /// Converts LocationDto to Location
    /// </summary>
    public Location Convert(LocationDto source, Location destination, ResolutionContext context)
    {
        if (source == null)
            return new Location(0, 0, string.Empty);

        return new Location(source.Latitude, source.Longitude, source.Address ?? string.Empty);
    }
}