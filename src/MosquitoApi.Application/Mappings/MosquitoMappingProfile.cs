using AutoMapper;
using MosquitoApi.Application.DTOs;
using MosquitoApi.Application.DTOs.Auth;
using MosquitoApi.Domain.Entities;
using MosquitoApi.Domain.Enums;
using MosquitoApi.Domain.ValueObjects;

namespace MosquitoApi.Application.Mappings;

/// <summary>
/// AutoMapper profile for mapping between mosquito domain entities and DTOs
/// </summary>
public class MosquitoMappingProfile : Profile
{
    public MosquitoMappingProfile()
    {
        ConfigureMosquitoMappings();
        ConfigureLocationMappings();
        ConfigureEnumMappings();
        ConfigureUserMappings();
    }

    private void ConfigureMosquitoMappings()
    {
        // Domain Entity to DTO
        CreateMap<Mosquito, MosquitoDto>()
            .ForMember(dest => dest.SpeciesName, opt => opt.MapFrom(src => GetSpeciesDisplayName(src.Species)))
            .ForMember(dest => dest.SizeDescription, opt => opt.MapFrom(src => GetSizeDisplayName(src.Size)))
            .ForMember(dest => dest.RiskAssessment, opt => opt.MapFrom(src => src.GetRiskAssessment()))
            .ForMember(dest => dest.IsLikelyActive, opt => opt.MapFrom(src => src.IsLikelyActive()))
            .ForMember(dest => dest.Location, opt => opt.MapFrom<LocationConverter>());

        // Create Request to Domain Entity
        CreateMap<CreateMosquitoRequest, Mosquito>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Will be set by domain
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Will be set by domain
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Location, opt => opt.MapFrom<CreateLocationConverter>());

        // Update Request to Domain Entity (for updating existing entity)
        CreateMap<UpdateMosquitoRequest, Mosquito>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Preserve existing ID
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Preserve creation info
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Location, opt => opt.MapFrom<UpdateLocationConverter>());
    }

    private void ConfigureLocationMappings()
    {
        // Location Value Object to DTO
        CreateMap<Location, LocationDto>()
            .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.ToString()));

        // Create Location Request to Value Object
        CreateMap<CreateLocationRequest, Location>()
            .ConstructUsing(src => new Location(src.Latitude, src.Longitude, src.Address, src.City, src.Country));

        // Update Location Request to Value Object
        CreateMap<UpdateLocationRequest, Location>()
            .ConstructUsing(src => new Location(src.Latitude, src.Longitude, src.Address, src.City, src.Country));
    }

    private void ConfigureEnumMappings()
    {
        // Configure enum to string mappings for better API responses
        CreateMap<MosquitoSpecies, string>().ConvertUsing(src => GetSpeciesDisplayName(src));
        CreateMap<MosquitoSize, string>().ConvertUsing(src => GetSizeDisplayName(src));
    }

    /// <summary>
    /// Gets human-readable display name for mosquito species
    /// </summary>
    private static string GetSpeciesDisplayName(MosquitoSpecies species) => species switch
    {
        MosquitoSpecies.AedesAegypti => "Aedes aegypti (Yellow fever mosquito)",
        MosquitoSpecies.AedesAlbopictus => "Aedes albopictus (Asian tiger mosquito)",
        MosquitoSpecies.AnophelesGambiae => "Anopheles gambiae (Principal malaria vector)",
        MosquitoSpecies.AnophelesStephensi => "Anopheles stephensi (Asian malaria mosquito)",
        MosquitoSpecies.CulexPipiens => "Culex pipiens (Common house mosquito)",
        MosquitoSpecies.CulexQuinquefasciatus => "Culex quinquefasciatus (Southern house mosquito)",
        MosquitoSpecies.Unknown => "Unknown species",
        _ => species.ToString()
    };

    /// <summary>
    /// Gets human-readable display name for mosquito size
    /// </summary>
    private static string GetSizeDisplayName(MosquitoSize size) => size switch
    {
        MosquitoSize.Small => "Small (< 4mm)",
        MosquitoSize.Medium => "Medium (4-6mm)",
        MosquitoSize.Large => "Large (> 6mm)",
        _ => size.ToString()
    };

    /// <summary>
    /// Configures user-related mappings
    /// </summary>
    private void ConfigureUserMappings()
    {
        // User Entity to UserDto
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.GenderType))
            .ForMember(dest => dest.LastLoginAt, opt => opt.MapFrom(src => src.LastLoginDateUtc));
    }
}

/// <summary>
/// Custom converter for mapping Location value object to LocationDto
/// </summary>
public class LocationConverter : IValueResolver<Mosquito, MosquitoDto, LocationDto>
{
    public LocationDto Resolve(Mosquito source, MosquitoDto destination, LocationDto destMember, ResolutionContext context)
    {
        if (source.Location == null) return null!;

        return new LocationDto
        {
            Latitude = source.Location.Latitude,
            Longitude = source.Location.Longitude,
            Address = source.Location.Address,
            City = source.Location.City,
            Country = source.Location.Country,
            DisplayName = source.Location.ToString()
        };
    }
}

/// <summary>
/// Custom converter for mapping CreateLocationRequest to Location value object
/// </summary>
public class CreateLocationConverter : IValueResolver<CreateMosquitoRequest, Mosquito, Location>
{
    public Location Resolve(CreateMosquitoRequest source, Mosquito destination, Location destMember, ResolutionContext context)
    {
        if (source.Location == null) return null!;

        return new Location(
            source.Location.Latitude,
            source.Location.Longitude,
            source.Location.Address,
            source.Location.City,
            source.Location.Country
        );
    }
}

/// <summary>
/// Custom converter for mapping UpdateLocationRequest to Location value object
/// </summary>
public class UpdateLocationConverter : IValueResolver<UpdateMosquitoRequest, Mosquito, Location>
{
    public Location Resolve(UpdateMosquitoRequest source, Mosquito destination, Location destMember, ResolutionContext context)
    {
        if (source.Location == null) return null!;

        return new Location(
            source.Location.Latitude,
            source.Location.Longitude,
            source.Location.Address,
            source.Location.City,
            source.Location.Country
        );
    }
}