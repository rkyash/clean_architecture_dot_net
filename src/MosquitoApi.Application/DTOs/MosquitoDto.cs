using MosquitoApi.Domain.Enums;

namespace MosquitoApi.Application.DTOs;

/// <summary>
/// Data Transfer Object for mosquito response data
/// </summary>
public sealed record MosquitoDto
{
    /// <summary>
    /// Unique identifier for the mosquito
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Scientific name or common name of the mosquito
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Species classification of the mosquito
    /// </summary>
    public MosquitoSpecies Species { get; init; }

    /// <summary>
    /// Human-readable species name
    /// </summary>
    public string SpeciesName { get; init; } = string.Empty;

    /// <summary>
    /// Size category of the mosquito
    /// </summary>
    public MosquitoSize Size { get; init; }

    /// <summary>
    /// Human-readable size description
    /// </summary>
    public string SizeDescription { get; init; } = string.Empty;

    /// <summary>
    /// Length of the mosquito in millimeters
    /// </summary>
    public double LengthMm { get; init; }

    /// <summary>
    /// Weight of the mosquito in milligrams
    /// </summary>
    public double? WeightMg { get; init; }

    /// <summary>
    /// Color description of the mosquito
    /// </summary>
    public string? Color { get; init; }

    /// <summary>
    /// Location information where the mosquito was found
    /// </summary>
    public LocationDto Location { get; init; } = null!;

    /// <summary>
    /// Date and time when the mosquito was observed or collected
    /// </summary>
    public DateTime ObservedAt { get; init; }

    /// <summary>
    /// Temperature in Celsius when the mosquito was observed
    /// </summary>
    public double? TemperatureCelsius { get; init; }

    /// <summary>
    /// Humidity percentage when the mosquito was observed
    /// </summary>
    public double? HumidityPercent { get; init; }

    /// <summary>
    /// Additional notes or observations about the mosquito
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Indicates if the mosquito is a disease vector
    /// </summary>
    public bool IsDiseaseVector { get; init; }

    /// <summary>
    /// List of diseases that this mosquito species can transmit
    /// </summary>
    public string? DiseaseTransmission { get; init; }

    /// <summary>
    /// Risk assessment based on species and environmental factors
    /// </summary>
    public string RiskAssessment { get; init; } = string.Empty;

    /// <summary>
    /// Indicates if mosquito is likely to be active based on conditions
    /// </summary>
    public bool IsLikelyActive { get; init; }

    /// <summary>
    /// User identifier who created the record
    /// </summary>
    public string? CreatedBy { get; init; }

    /// <summary>
    /// User identifier who last modified the record
    /// </summary>
    public string? ModifiedBy { get; init; }

    /// <summary>
    /// Timestamp when the record was created
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Timestamp when the record was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; init; }
}