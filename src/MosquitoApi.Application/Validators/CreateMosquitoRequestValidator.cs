using FluentValidation;
using MosquitoApi.Application.DTOs;
using MosquitoApi.Domain.Enums;

namespace MosquitoApi.Application.Validators;

/// <summary>
/// Validator for CreateMosquitoRequest DTO
/// </summary>
public class CreateMosquitoRequestValidator : AbstractValidator<CreateMosquitoRequest>
{
    public CreateMosquitoRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Mosquito name is required")
            .MaximumLength(200).WithMessage("Mosquito name cannot exceed 200 characters")
            .Must(BeValidName).WithMessage("Mosquito name contains invalid characters");

        RuleFor(x => x.Species)
            .IsInEnum().WithMessage("Invalid mosquito species");

        RuleFor(x => x.Size)
            .IsInEnum().WithMessage("Invalid mosquito size");

        RuleFor(x => x.LengthMm)
            .GreaterThan(0).WithMessage("Length must be greater than 0mm")
            .LessThanOrEqualTo(50).WithMessage("Length cannot exceed 50mm (unrealistic for mosquitoes)");

        RuleFor(x => x.WeightMg)
            .GreaterThan(0).WithMessage("Weight must be greater than 0mg if specified")
            .When(x => x.WeightMg.HasValue);

        RuleFor(x => x.Color)
            .MaximumLength(100).WithMessage("Color description cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Color));

        RuleFor(x => x.Location)
            .NotNull().WithMessage("Location is required")
            .SetValidator(new CreateLocationRequestValidator());

        RuleFor(x => x.ObservedAt)
            .NotEmpty().WithMessage("Observation date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Observation date cannot be in the future")
            .GreaterThanOrEqualTo(new DateTime(1900, 1, 1)).WithMessage("Observation date cannot be before 1900");

        RuleFor(x => x.TemperatureCelsius)
            .InclusiveBetween(-50, 60).WithMessage("Temperature must be between -50°C and 60°C")
            .When(x => x.TemperatureCelsius.HasValue);

        RuleFor(x => x.HumidityPercent)
            .InclusiveBetween(0, 100).WithMessage("Humidity must be between 0% and 100%")
            .When(x => x.HumidityPercent.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.DiseaseTransmission)
            .MaximumLength(500).WithMessage("Disease transmission information cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.DiseaseTransmission));

        // Custom validation for size consistency
        RuleFor(x => x)
            .Must(HaveConsistentSizeAndLength)
            .WithMessage("Size category must be consistent with the actual length")
            .WithName("Size/Length Consistency");
    }

    private static bool BeValidName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        
        // Allow letters, numbers, spaces, hyphens, parentheses, and common punctuation
        return name.All(c => char.IsLetterOrDigit(c) || 
                            char.IsWhiteSpace(c) || 
                            "-().,'".Contains(c));
    }

    private static bool HaveConsistentSizeAndLength(CreateMosquitoRequest request)
    {
        return request.Size switch
        {
            MosquitoSize.Small => request.LengthMm < 4,
            MosquitoSize.Medium => request.LengthMm >= 4 && request.LengthMm <= 6,
            MosquitoSize.Large => request.LengthMm > 6,
            _ => true // Allow unknown sizes
        };
    }
}

/// <summary>
/// Validator for CreateLocationRequest DTO
/// </summary>
public class CreateLocationRequestValidator : AbstractValidator<CreateLocationRequest>
{
    public CreateLocationRequestValidator()
    {
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90 degrees");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180 degrees");

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Address cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Address));

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("City name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.City));

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Country name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Country));
    }
}