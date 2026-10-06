using FluentValidation;
using MosquitoApi.Application.DTOs;

namespace MosquitoApi.Application.Validators;

/// <summary>
/// Validator for MosquitoSearchRequest DTO
/// </summary>
public class MosquitoSearchRequestValidator : AbstractValidator<MosquitoSearchRequest>
{
    public MosquitoSearchRequestValidator()
    {
        RuleFor(x => x.Species)
            .IsInEnum().WithMessage("Invalid mosquito species")
            .When(x => x.Species.HasValue);

        RuleFor(x => x.Size)
            .IsInEnum().WithMessage("Invalid mosquito size")
            .When(x => x.Size.HasValue);

        RuleFor(x => x.Location)
            .SetValidator(new SearchLocationRequestValidator())
            .When(x => x.Location != null);

        RuleFor(x => x.RadiusKm)
            .GreaterThan(0).WithMessage("Search radius must be greater than 0km")
            .LessThanOrEqualTo(20000).WithMessage("Search radius cannot exceed 20,000km (half the Earth's circumference)");

        RuleFor(x => x.ObservedFrom)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Observed from date cannot be in the future")
            .When(x => x.ObservedFrom.HasValue);

        RuleFor(x => x.ObservedTo)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Observed to date cannot be in the future")
            .When(x => x.ObservedTo.HasValue);

        RuleFor(x => x)
            .Must(HaveValidDateRange)
            .WithMessage("Observed from date must be earlier than observed to date")
            .When(x => x.ObservedFrom.HasValue && x.ObservedTo.HasValue)
            .WithName("DateRange");

        RuleFor(x => x.MinLengthMm)
            .GreaterThan(0).WithMessage("Minimum length must be greater than 0mm")
            .When(x => x.MinLengthMm.HasValue);

        RuleFor(x => x.MaxLengthMm)
            .GreaterThan(0).WithMessage("Maximum length must be greater than 0mm")
            .When(x => x.MaxLengthMm.HasValue);

        RuleFor(x => x)
            .Must(HaveValidLengthRange)
            .WithMessage("Minimum length must be less than maximum length")
            .When(x => x.MinLengthMm.HasValue && x.MaxLengthMm.HasValue)
            .WithName("LengthRange");

        RuleFor(x => x.MinTemperatureCelsius)
            .InclusiveBetween(-50, 60).WithMessage("Minimum temperature must be between -50°C and 60°C")
            .When(x => x.MinTemperatureCelsius.HasValue);

        RuleFor(x => x.MaxTemperatureCelsius)
            .InclusiveBetween(-50, 60).WithMessage("Maximum temperature must be between -50°C and 60°C")
            .When(x => x.MaxTemperatureCelsius.HasValue);

        RuleFor(x => x)
            .Must(HaveValidTemperatureRange)
            .WithMessage("Minimum temperature must be less than maximum temperature")
            .When(x => x.MinTemperatureCelsius.HasValue && x.MaxTemperatureCelsius.HasValue)
            .WithName("TemperatureRange");

        RuleFor(x => x.MinHumidityPercent)
            .InclusiveBetween(0, 100).WithMessage("Minimum humidity must be between 0% and 100%")
            .When(x => x.MinHumidityPercent.HasValue);

        RuleFor(x => x.MaxHumidityPercent)
            .InclusiveBetween(0, 100).WithMessage("Maximum humidity must be between 0% and 100%")
            .When(x => x.MaxHumidityPercent.HasValue);

        RuleFor(x => x)
            .Must(HaveValidHumidityRange)
            .WithMessage("Minimum humidity must be less than maximum humidity")
            .When(x => x.MinHumidityPercent.HasValue && x.MaxHumidityPercent.HasValue)
            .WithName("HumidityRange");

        RuleFor(x => x.SearchText)
            .MaximumLength(200).WithMessage("Search text cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.SearchText));

        RuleFor(x => x.CreatedBy)
            .MaximumLength(50).WithMessage("Created by filter cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.CreatedBy));

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Country filter cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Country));

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("City filter cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.City));

        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100");

        RuleFor(x => x.SortBy)
            .IsInEnum().WithMessage("Invalid sort field");

        RuleFor(x => x.SortDirection)
            .IsInEnum().WithMessage("Invalid sort direction");
    }

    private static bool HaveValidDateRange(MosquitoSearchRequest request)
    {
        return !request.ObservedFrom.HasValue || 
               !request.ObservedTo.HasValue || 
               request.ObservedFrom.Value <= request.ObservedTo.Value;
    }

    private static bool HaveValidLengthRange(MosquitoSearchRequest request)
    {
        return !request.MinLengthMm.HasValue || 
               !request.MaxLengthMm.HasValue || 
               request.MinLengthMm.Value <= request.MaxLengthMm.Value;
    }

    private static bool HaveValidTemperatureRange(MosquitoSearchRequest request)
    {
        return !request.MinTemperatureCelsius.HasValue || 
               !request.MaxTemperatureCelsius.HasValue || 
               request.MinTemperatureCelsius.Value <= request.MaxTemperatureCelsius.Value;
    }

    private static bool HaveValidHumidityRange(MosquitoSearchRequest request)
    {
        return !request.MinHumidityPercent.HasValue || 
               !request.MaxHumidityPercent.HasValue || 
               request.MinHumidityPercent.Value <= request.MaxHumidityPercent.Value;
    }
}

/// <summary>
/// Validator for SearchLocationRequest DTO
/// </summary>
public class SearchLocationRequestValidator : AbstractValidator<SearchLocationRequest>
{
    public SearchLocationRequestValidator()
    {
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90 degrees");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180 degrees");
    }
}