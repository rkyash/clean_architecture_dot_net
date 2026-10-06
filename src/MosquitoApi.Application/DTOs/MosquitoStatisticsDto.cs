using MosquitoApi.Domain.Enums;

namespace MosquitoApi.Application.DTOs;

/// <summary>
/// Data Transfer Object for mosquito statistics and analytics
/// </summary>
public sealed record MosquitoStatisticsDto
{
    /// <summary>
    /// Total number of mosquito records
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Number of unique species recorded
    /// </summary>
    public int UniqueSpeciesCount { get; init; }

    /// <summary>
    /// Number of disease vector mosquitoes
    /// </summary>
    public int DiseaseVectorCount { get; init; }

    /// <summary>
    /// Percentage of disease vectors in the dataset
    /// </summary>
    public double DiseaseVectorPercentage { get; init; }

    /// <summary>
    /// Statistics broken down by species
    /// </summary>
    public IReadOnlyList<SpeciesStatisticDto> SpeciesBreakdown { get; init; } = [];

    /// <summary>
    /// Statistics broken down by size category
    /// </summary>
    public IReadOnlyList<SizeStatisticDto> SizeBreakdown { get; init; } = [];

    /// <summary>
    /// Statistics broken down by location (country level)
    /// </summary>
    public IReadOnlyList<LocationStatisticDto> LocationBreakdown { get; init; } = [];

    /// <summary>
    /// Statistics over time (monthly breakdown)
    /// </summary>
    public IReadOnlyList<TimeStatisticDto> TimeBreakdown { get; init; } = [];

    /// <summary>
    /// Average mosquito measurements
    /// </summary>
    public MeasurementStatisticsDto Measurements { get; init; } = null!;

    /// <summary>
    /// Environmental condition statistics
    /// </summary>
    public EnvironmentalStatisticsDto Environmental { get; init; } = null!;

    /// <summary>
    /// Most recent mosquito observation date
    /// </summary>
    public DateTime? LatestObservation { get; init; }

    /// <summary>
    /// Oldest mosquito observation date
    /// </summary>
    public DateTime? EarliestObservation { get; init; }

    /// <summary>
    /// Timestamp when these statistics were generated
    /// </summary>
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Statistics for a specific mosquito species
/// </summary>
public sealed record SpeciesStatisticDto
{
    /// <summary>
    /// Species type
    /// </summary>
    public MosquitoSpecies Species { get; init; }

    /// <summary>
    /// Human-readable species name
    /// </summary>
    public string SpeciesName { get; init; } = string.Empty;

    /// <summary>
    /// Number of observations for this species
    /// </summary>
    public int Count { get; init; }

    /// <summary>
    /// Percentage of total observations
    /// </summary>
    public double Percentage { get; init; }

    /// <summary>
    /// Number of disease vectors in this species
    /// </summary>
    public int DiseaseVectorCount { get; init; }

    /// <summary>
    /// Average length for this species
    /// </summary>
    public double AverageLengthMm { get; init; }
}

/// <summary>
/// Statistics for a specific mosquito size category
/// </summary>
public sealed record SizeStatisticDto
{
    /// <summary>
    /// Size category
    /// </summary>
    public MosquitoSize Size { get; init; }

    /// <summary>
    /// Human-readable size description
    /// </summary>
    public string SizeDescription { get; init; } = string.Empty;

    /// <summary>
    /// Number of observations for this size
    /// </summary>
    public int Count { get; init; }

    /// <summary>
    /// Percentage of total observations
    /// </summary>
    public double Percentage { get; init; }

    /// <summary>
    /// Average length for this size category
    /// </summary>
    public double AverageLengthMm { get; init; }
}

/// <summary>
/// Statistics for a specific location (country)
/// </summary>
public sealed record LocationStatisticDto
{
    /// <summary>
    /// Country name
    /// </summary>
    public string Country { get; init; } = string.Empty;

    /// <summary>
    /// Number of observations in this country
    /// </summary>
    public int Count { get; init; }

    /// <summary>
    /// Percentage of total observations
    /// </summary>
    public double Percentage { get; init; }

    /// <summary>
    /// Number of unique species observed in this country
    /// </summary>
    public int UniqueSpeciesCount { get; init; }

    /// <summary>
    /// Number of disease vectors observed in this country
    /// </summary>
    public int DiseaseVectorCount { get; init; }
}

/// <summary>
/// Statistics for a specific time period
/// </summary>
public sealed record TimeStatisticDto
{
    /// <summary>
    /// Year
    /// </summary>
    public int Year { get; init; }

    /// <summary>
    /// Month (1-12)
    /// </summary>
    public int Month { get; init; }

    /// <summary>
    /// Human-readable period (e.g., "2024-03")
    /// </summary>
    public string Period { get; init; } = string.Empty;

    /// <summary>
    /// Number of observations in this period
    /// </summary>
    public int Count { get; init; }

    /// <summary>
    /// Number of unique species observed in this period
    /// </summary>
    public int UniqueSpeciesCount { get; init; }
}

/// <summary>
/// Statistical measurements for mosquito physical characteristics
/// </summary>
public sealed record MeasurementStatisticsDto
{
    /// <summary>
    /// Average length in millimeters
    /// </summary>
    public double AverageLengthMm { get; init; }

    /// <summary>
    /// Minimum recorded length in millimeters
    /// </summary>
    public double MinLengthMm { get; init; }

    /// <summary>
    /// Maximum recorded length in millimeters
    /// </summary>
    public double MaxLengthMm { get; init; }

    /// <summary>
    /// Average weight in milligrams (for records that have weight data)
    /// </summary>
    public double? AverageWeightMg { get; init; }

    /// <summary>
    /// Number of records that include weight data
    /// </summary>
    public int WeightDataCount { get; init; }
}

/// <summary>
/// Statistical data for environmental conditions
/// </summary>
public sealed record EnvironmentalStatisticsDto
{
    /// <summary>
    /// Average temperature in Celsius (for records that have temperature data)
    /// </summary>
    public double? AverageTemperatureCelsius { get; init; }

    /// <summary>
    /// Minimum recorded temperature in Celsius
    /// </summary>
    public double? MinTemperatureCelsius { get; init; }

    /// <summary>
    /// Maximum recorded temperature in Celsius
    /// </summary>
    public double? MaxTemperatureCelsius { get; init; }

    /// <summary>
    /// Average humidity percentage (for records that have humidity data)
    /// </summary>
    public double? AverageHumidityPercent { get; init; }

    /// <summary>
    /// Minimum recorded humidity percentage
    /// </summary>
    public double? MinHumidityPercent { get; init; }

    /// <summary>
    /// Maximum recorded humidity percentage
    /// </summary>
    public double? MaxHumidityPercent { get; init; }

    /// <summary>
    /// Number of records that include temperature data
    /// </summary>
    public int TemperatureDataCount { get; init; }

    /// <summary>
    /// Number of records that include humidity data
    /// </summary>
    public int HumidityDataCount { get; init; }
}