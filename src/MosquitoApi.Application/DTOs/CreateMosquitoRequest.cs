using MosquitoApi.Domain.Enums;

namespace MosquitoApi.Application.DTOs;

/// <summary>
/// Request DTO for creating a new mosquito record
/// </summary>
public sealed record CreateMosquitoRequest
{
    /// <summary>
    /// Scientific name or common name of the mosquito
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Species classification of the mosquito
    /// </summary>
    public MosquitoSpecies Species { get; init; }

    /// <summary>
    /// Size category of the mosquito
    /// </summary>
    public MosquitoSize Size { get; init; }

    /// <summary>
    /// Length of the mosquito in millimeters
    /// </summary>
    public double LengthMm { get; init; }

    /// <summary>
    /// Weight of the mosquito in milligrams (optional)
    /// </summary>
    public double? WeightMg { get; init; }

    /// <summary>
    /// Color description of the mosquito (optional)
    /// </summary>
    public string? Color { get; init; }

    /// <summary>
    /// Location information where the mosquito was found
    /// </summary>
    public CreateLocationRequest Location { get; init; } = null!;

    /// <summary>
    /// Date and time when the mosquito was observed or collected
    /// </summary>
    public DateTime ObservedAt { get; init; }

    /// <summary>
    /// Temperature in Celsius when the mosquito was observed (optional)
    /// </summary>
    public double? TemperatureCelsius { get; init; }

    /// <summary>
    /// Humidity percentage when the mosquito was observed (optional)
    /// </summary>
    public double? HumidityPercent { get; init; }

    /// <summary>
    /// Additional notes or observations about the mosquito (optional)
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Indicates if the mosquito is a disease vector
    /// </summary>
    public bool IsDiseaseVector { get; init; }

    /// <summary>
    /// List of diseases that this mosquito species can transmit (optional)
    /// </summary>
    public string? DiseaseTransmission { get; init; }
}