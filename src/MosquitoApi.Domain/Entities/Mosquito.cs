using MosquitoApi.Domain.Enums;
using MosquitoApi.Domain.Interfaces;
using MosquitoApi.Domain.ValueObjects;

namespace MosquitoApi.Domain.Entities;

/// <summary>
/// Main mosquito entity representing a mosquito specimen with business rules
/// </summary>
public class Mosquito : BaseEntity, IAuditable
{
    /// <summary>
    /// Scientific name or common name of the mosquito
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Species classification of the mosquito
    /// </summary>
    public MosquitoSpecies Species { get; set; }

    /// <summary>
    /// Size category of the mosquito
    /// </summary>
    public MosquitoSize Size { get; set; }

    /// <summary>
    /// Length of the mosquito in millimeters
    /// </summary>
    public double LengthMm { get; set; }

    /// <summary>
    /// Weight of the mosquito in milligrams
    /// </summary>
    public double? WeightMg { get; set; }

    /// <summary>
    /// Color description of the mosquito
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Geographical location where the mosquito was found
    /// </summary>
    public Location Location { get; set; } = null!;

    /// <summary>
    /// Date and time when the mosquito was observed or collected
    /// </summary>
    public DateTime ObservedAt { get; set; }

    /// <summary>
    /// Temperature in Celsius when the mosquito was observed
    /// </summary>
    public double? TemperatureCelsius { get; set; }

    /// <summary>
    /// Humidity percentage when the mosquito was observed
    /// </summary>
    public double? HumidityPercent { get; set; }

    /// <summary>
    /// Additional notes or observations about the mosquito
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Indicates if the mosquito is a disease vector
    /// </summary>
    public bool IsDiseaseVector { get; set; }

    /// <summary>
    /// List of diseases that this mosquito species can transmit
    /// </summary>
    public string? DiseaseTransmission { get; set; }

    // Note: CreatedBy, UpdatedBy, and DeletedBy are inherited from BaseEntity

    /// <summary>
    /// Validates the mosquito entity according to business rules
    /// </summary>
    /// <returns>List of validation errors, empty if valid</returns>
    public List<string> Validate()
    {
        var errors = new List<string>();

        // Name validation
        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Mosquito name is required");
        else if (Name.Length > 200)
            errors.Add("Mosquito name cannot exceed 200 characters");

        // Length validation
        if (LengthMm <= 0)
            errors.Add("Length must be greater than 0 mm");
        else if (LengthMm > 50)
            errors.Add("Length cannot exceed 50 mm (unrealistic for mosquitoes)");

        // Weight validation
        if (WeightMg.HasValue && WeightMg <= 0)
            errors.Add("Weight must be greater than 0 mg if specified");

        // Size consistency validation
        ValidateSizeConsistency(errors);

        // Location validation
        if (Location == null)
            errors.Add("Location is required");

        // Observation date validation
        if (ObservedAt > DateTime.UtcNow)
            errors.Add("Observation date cannot be in the future");
        else if (ObservedAt < new DateTime(1900, 1, 1))
            errors.Add("Observation date cannot be before 1900");

        // Temperature validation
        if (TemperatureCelsius.HasValue && (TemperatureCelsius < -50 || TemperatureCelsius > 60))
            errors.Add("Temperature must be between -50°C and 60°C");

        // Humidity validation
        if (HumidityPercent.HasValue && (HumidityPercent < 0 || HumidityPercent > 100))
            errors.Add("Humidity must be between 0% and 100%");

        // Notes length validation
        if (Notes?.Length > 1000)
            errors.Add("Notes cannot exceed 1000 characters");

        return errors;
    }

    /// <summary>
    /// Validates that the size category matches the actual length
    /// </summary>
    private void ValidateSizeConsistency(List<string> errors)
    {
        switch (Size)
        {
            case MosquitoSize.Small when LengthMm >= 4:
                errors.Add("Small mosquitoes should be less than 4mm in length");
                break;
            case MosquitoSize.Medium when LengthMm < 4 || LengthMm > 6:
                errors.Add("Medium mosquitoes should be between 4-6mm in length");
                break;
            case MosquitoSize.Large when LengthMm <= 6:
                errors.Add("Large mosquitoes should be greater than 6mm in length");
                break;
        }
    }

    /// <summary>
    /// Determines if this mosquito is likely to be active based on environmental conditions
    /// </summary>
    /// <returns>True if conditions favor mosquito activity</returns>
    public bool IsLikelyActive()
    {
        // Mosquitoes are generally more active in warm, humid conditions
        var temperatureOk = !TemperatureCelsius.HasValue || 
                           (TemperatureCelsius >= 15 && TemperatureCelsius <= 35);
        
        var humidityOk = !HumidityPercent.HasValue || 
                        HumidityPercent >= 60;

        return temperatureOk && humidityOk;
    }

    /// <summary>
    /// Gets a risk assessment based on species and environmental factors
    /// </summary>
    /// <returns>Risk level description</returns>
    public string GetRiskAssessment()
    {
        var riskFactors = 0;

        // Disease vector species increase risk
        if (IsDiseaseVector && Species is MosquitoSpecies.AedesAegypti or MosquitoSpecies.AedesAlbopictus or MosquitoSpecies.AnophelesGambiae)
            riskFactors += 3;
        else if (IsDiseaseVector)
            riskFactors += 2;

        // Environmental conditions favorable for mosquito activity
        if (IsLikelyActive())
            riskFactors += 1;

        // Large mosquitoes may be more concerning
        if (Size == MosquitoSize.Large)
            riskFactors += 1;

        return riskFactors switch
        {
            >= 4 => "High Risk - Disease vector in favorable conditions",
            3 => "Moderate-High Risk - Disease vector present",
            2 => "Moderate Risk - Some concerning factors",
            1 => "Low-Moderate Risk - Monitor conditions",
            _ => "Low Risk - Standard precautions recommended"
        };
    }
}