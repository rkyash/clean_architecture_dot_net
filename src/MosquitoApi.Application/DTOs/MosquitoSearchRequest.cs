using MosquitoApi.Domain.Enums;

namespace MosquitoApi.Application.DTOs;

/// <summary>
/// Request DTO for searching mosquitoes with various filters
/// </summary>
public sealed record MosquitoSearchRequest
{
    /// <summary>
    /// Optional species filter
    /// </summary>
    public MosquitoSpecies? Species { get; init; }

    /// <summary>
    /// Optional size filter
    /// </summary>
    public MosquitoSize? Size { get; init; }

    /// <summary>
    /// Optional location-based search
    /// </summary>
    public SearchLocationRequest? Location { get; init; }

    /// <summary>
    /// Search radius in kilometers when location is specified (default: 10km)
    /// </summary>
    public double RadiusKm { get; init; } = 10.0;

    /// <summary>
    /// Optional start date for observation period filter
    /// </summary>
    public DateTime? ObservedFrom { get; init; }

    /// <summary>
    /// Optional end date for observation period filter
    /// </summary>
    public DateTime? ObservedTo { get; init; }

    /// <summary>
    /// Optional filter for disease vectors
    /// </summary>
    public bool? IsDiseaseVector { get; init; }

    /// <summary>
    /// Optional minimum length filter in millimeters
    /// </summary>
    public double? MinLengthMm { get; init; }

    /// <summary>
    /// Optional maximum length filter in millimeters
    /// </summary>
    public double? MaxLengthMm { get; init; }

    /// <summary>
    /// Optional minimum temperature filter in Celsius
    /// </summary>
    public double? MinTemperatureCelsius { get; init; }

    /// <summary>
    /// Optional maximum temperature filter in Celsius
    /// </summary>
    public double? MaxTemperatureCelsius { get; init; }

    /// <summary>
    /// Optional minimum humidity filter as percentage
    /// </summary>
    public double? MinHumidityPercent { get; init; }

    /// <summary>
    /// Optional maximum humidity filter as percentage
    /// </summary>
    public double? MaxHumidityPercent { get; init; }

    /// <summary>
    /// Optional text search in mosquito name, notes, or disease transmission
    /// </summary>
    public string? SearchText { get; init; }

    /// <summary>
    /// Optional filter by creator user ID
    /// </summary>
    public string? CreatedBy { get; init; }

    /// <summary>
    /// Optional filter by country
    /// </summary>
    public string? Country { get; init; }

    /// <summary>
    /// Optional filter by city
    /// </summary>
    public string? City { get; init; }

    /// <summary>
    /// Page number for pagination (1-based, default: 1)
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Number of items per page (default: 20, max: 100)
    /// </summary>
    public int PageSize { get; init; } = 20;

    /// <summary>
    /// Sort field for ordering results
    /// </summary>
    public MosquitoSortField SortBy { get; init; } = MosquitoSortField.ObservedAt;

    /// <summary>
    /// Sort direction for ordering results
    /// </summary>
    public SortDirection SortDirection { get; init; } = SortDirection.Descending;
}

/// <summary>
/// Request DTO for location-based search parameters
/// </summary>
public sealed record SearchLocationRequest
{
    /// <summary>
    /// Center latitude for location-based search
    /// </summary>
    public double Latitude { get; init; }

    /// <summary>
    /// Center longitude for location-based search
    /// </summary>
    public double Longitude { get; init; }
}

/// <summary>
/// Enumeration of available sort fields for mosquito search
/// </summary>
public enum MosquitoSortField
{
    /// <summary>
    /// Sort by observation date
    /// </summary>
    ObservedAt,

    /// <summary>
    /// Sort by mosquito name
    /// </summary>
    Name,

    /// <summary>
    /// Sort by species
    /// </summary>
    Species,

    /// <summary>
    /// Sort by size
    /// </summary>
    Size,

    /// <summary>
    /// Sort by length
    /// </summary>
    LengthMm,

    /// <summary>
    /// Sort by creation date
    /// </summary>
    CreatedAt,

    /// <summary>
    /// Sort by last update date
    /// </summary>
    UpdatedAt
}

/// <summary>
/// Enumeration of sort directions
/// </summary>
public enum SortDirection
{
    /// <summary>
    /// Ascending order (A to Z, 1 to 9, earliest to latest)
    /// </summary>
    Ascending,

    /// <summary>
    /// Descending order (Z to A, 9 to 1, latest to earliest)
    /// </summary>
    Descending
}