using AutoMapper;
using FluentValidation;
using MosquitoApi.Application.Common;
using MosquitoApi.Application.DTOs;
using MosquitoApi.Application.Interfaces;
using MosquitoApi.Domain.Entities;
using MosquitoApi.Domain.Enums;
using MosquitoApi.Domain.Exceptions;
using MosquitoApi.Domain.Interfaces;
using MosquitoApi.Domain.ValueObjects;

namespace MosquitoApi.Application.Services;

/// <summary>
/// Implementation of mosquito business logic service
/// </summary>
public class MosquitoService : IMosquitoService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateMosquitoRequest> _createValidator;
    private readonly IValidator<UpdateMosquitoRequest> _updateValidator;
    private readonly IValidator<MosquitoSearchRequest> _searchValidator;
    private readonly IAuditService _auditService;

    public MosquitoService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreateMosquitoRequest> createValidator,
        IValidator<UpdateMosquitoRequest> updateValidator,
        IValidator<MosquitoSearchRequest> searchValidator,
        IAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _searchValidator = searchValidator;
        _auditService = auditService;
    }

    public async Task<Result<MosquitoDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var mosquito = await _unitOfWork.MosquitoRepository.GetByIdAsync(id, cancellationToken);
            if (mosquito == null)
            {
                return Result<MosquitoDto>.NotFound("Mosquito", id);
            }

            var dto = _mapper.Map<MosquitoDto>(mosquito);
            return Result<MosquitoDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<MosquitoDto>.Failure(ex);
        }
    }

    public async Task<Result<PagedResult<MosquitoDto>>> GetAllAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate pagination parameters
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var mosquitoes = await _unitOfWork.MosquitoRepository.GetAllAsync(pageNumber, pageSize, cancellationToken);
            var totalCount = await _unitOfWork.MosquitoRepository.GetCountAsync(cancellationToken);

            var dtos = _mapper.Map<IEnumerable<MosquitoDto>>(mosquitoes);
            var pagedResult = new PagedResult<MosquitoDto>(dtos, pageNumber, pageSize, totalCount);

            return Result<PagedResult<MosquitoDto>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            return Result<PagedResult<MosquitoDto>>.Failure(ex);
        }
    }

    public async Task<Result<PagedResult<MosquitoDto>>> SearchAsync(MosquitoSearchRequest searchRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate search request
            var validationResult = await _searchValidator.ValidateAsync(searchRequest, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage);
                return Result<PagedResult<MosquitoDto>>.ValidationFailure(errors);
            }

            // Convert search request to domain parameters
            Location? location = null;
            if (searchRequest.Location != null)
            {
                location = new Location(searchRequest.Location.Latitude, searchRequest.Location.Longitude);
            }

            var mosquitoes = await _unitOfWork.MosquitoRepository.SearchAsync(
                species: searchRequest.Species,
                size: searchRequest.Size,
                location: location,
                radiusKm: searchRequest.RadiusKm,
                observedFrom: searchRequest.ObservedFrom,
                observedTo: searchRequest.ObservedTo,
                isDiseaseVector: searchRequest.IsDiseaseVector,
                pageNumber: searchRequest.PageNumber,
                pageSize: searchRequest.PageSize,
                cancellationToken: cancellationToken);

            var totalCount = await _unitOfWork.MosquitoRepository.GetSearchCountAsync(
                species: searchRequest.Species,
                size: searchRequest.Size,
                location: location,
                radiusKm: searchRequest.RadiusKm,
                observedFrom: searchRequest.ObservedFrom,
                observedTo: searchRequest.ObservedTo,
                isDiseaseVector: searchRequest.IsDiseaseVector,
                cancellationToken: cancellationToken);

            var dtos = _mapper.Map<IEnumerable<MosquitoDto>>(mosquitoes);
            var pagedResult = new PagedResult<MosquitoDto>(dtos, searchRequest.PageNumber, searchRequest.PageSize, totalCount);

            return Result<PagedResult<MosquitoDto>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            return Result<PagedResult<MosquitoDto>>.Failure(ex);
        }
    }

    public async Task<Result<MosquitoDto>> CreateAsync(CreateMosquitoRequest request, string? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate request
            var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage);
                return Result<MosquitoDto>.ValidationFailure(errors);
            }

            // Map to domain entity
            var mosquito = _mapper.Map<Mosquito>(request);
            mosquito.CreatedBy = userId;
            mosquito.CreatedAt = DateTime.UtcNow;

            // Domain validation
            var domainErrors = mosquito.Validate();
            if (domainErrors.Count > 0)
            {
                return Result<MosquitoDto>.ValidationFailure(domainErrors);
            }

            // Save to repository
            var createdMosquito = await _unitOfWork.MosquitoRepository.AddAsync(mosquito, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Log audit trail
            await _auditService.LogEntityCreatedAsync(createdMosquito, userId, context: "Created via API", cancellationToken: cancellationToken);

            var dto = _mapper.Map<MosquitoDto>(createdMosquito);
            return Result<MosquitoDto>.Success(dto);
        }
        catch (InvalidMosquitoDataException ex)
        {
            return Result<MosquitoDto>.ValidationFailure(ex.ValidationErrors);
        }
        catch (Exception ex)
        {
            return Result<MosquitoDto>.Failure(ex);
        }
    }

    public async Task<Result<MosquitoDto>> UpdateAsync(Guid id, UpdateMosquitoRequest request, string? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate request
            var validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage);
                return Result<MosquitoDto>.ValidationFailure(errors);
            }

            // Get existing entity
            var existingMosquito = await _unitOfWork.MosquitoRepository.GetByIdAsync(id, cancellationToken);
            if (existingMosquito == null)
            {
                return Result<MosquitoDto>.NotFound("Mosquito", id);
            }

            // Store old values for audit
            var oldMosquito = _mapper.Map<Mosquito>(existingMosquito);

            // Map updates to existing entity
            _mapper.Map(request, existingMosquito);
            existingMosquito.UpdatedBy = userId;
            existingMosquito.UpdatedAt = DateTime.UtcNow;

            // Domain validation
            var domainErrors = existingMosquito.Validate();
            if (domainErrors.Count > 0)
            {
                return Result<MosquitoDto>.ValidationFailure(domainErrors);
            }

            // Save changes
            var updatedMosquito = await _unitOfWork.MosquitoRepository.UpdateAsync(existingMosquito, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Log audit trail
            await _auditService.LogEntityUpdatedAsync(oldMosquito, updatedMosquito, userId, context: "Updated via API", cancellationToken: cancellationToken);

            var dto = _mapper.Map<MosquitoDto>(updatedMosquito);
            return Result<MosquitoDto>.Success(dto);
        }
        catch (InvalidMosquitoDataException ex)
        {
            return Result<MosquitoDto>.ValidationFailure(ex.ValidationErrors);
        }
        catch (Exception ex)
        {
            return Result<MosquitoDto>.Failure(ex);
        }
    }

    public async Task<Result> DeleteAsync(Guid id, string? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get existing entity for audit purposes
            var existingMosquito = await _unitOfWork.MosquitoRepository.GetByIdAsync(id, cancellationToken);
            if (existingMosquito == null)
            {
                return Result.Failure($"Mosquito with ID '{id}' was not found.");
            }

            // Perform soft delete
            var deleted = await _unitOfWork.MosquitoRepository.DeleteAsync(id, cancellationToken);
            if (!deleted)
            {
                return Result.Failure($"Failed to delete mosquito with ID '{id}'.");
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Log audit trail
            await _auditService.LogEntityDeletedAsync(existingMosquito, userId, context: "Deleted via API", cancellationToken: cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex);
        }
    }

    public async Task<Result<MosquitoStatisticsDto>> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var totalCount = await _unitOfWork.MosquitoRepository.GetCountAsync(cancellationToken);
            var speciesStats = await _unitOfWork.MosquitoRepository.GetSpeciesStatisticsAsync(cancellationToken);
            var sizeStats = await _unitOfWork.MosquitoRepository.GetSizeStatisticsAsync(cancellationToken);

            // Get all mosquitoes for detailed statistics (consider caching in production)
            var allMosquitoes = await _unitOfWork.MosquitoRepository.GetAllAsync(1, int.MaxValue, cancellationToken);
            var mosquitoList = allMosquitoes.ToList();

            var statistics = new MosquitoStatisticsDto
            {
                TotalCount = totalCount,
                UniqueSpeciesCount = speciesStats.Count,
                DiseaseVectorCount = mosquitoList.Count(m => m.IsDiseaseVector),
                DiseaseVectorPercentage = totalCount > 0 ? (double)mosquitoList.Count(m => m.IsDiseaseVector) / totalCount * 100 : 0,
                SpeciesBreakdown = CreateSpeciesBreakdown(speciesStats, totalCount),
                SizeBreakdown = CreateSizeBreakdown(sizeStats, totalCount),
                LocationBreakdown = CreateLocationBreakdown(mosquitoList, totalCount),
                TimeBreakdown = CreateTimeBreakdown(mosquitoList),
                Measurements = CreateMeasurementStatistics(mosquitoList),
                Environmental = CreateEnvironmentalStatistics(mosquitoList),
                LatestObservation = mosquitoList.Count > 0 ? mosquitoList.Max(m => m.ObservedAt) : null,
                EarliestObservation = mosquitoList.Count > 0 ? mosquitoList.Min(m => m.ObservedAt) : null
            };

            return Result<MosquitoStatisticsDto>.Success(statistics);
        }
        catch (Exception ex)
        {
            return Result<MosquitoStatisticsDto>.Failure(ex);
        }
    }

    public async Task<Result<bool>> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var exists = await _unitOfWork.MosquitoRepository.ExistsAsync(id, cancellationToken);
            return Result<bool>.Success(exists);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(ex);
        }
    }

    public async Task<Result<PagedResult<MosquitoDto>>> GetByLocationAsync(
        double latitude, 
        double longitude, 
        double radiusKm, 
        int pageNumber = 1, 
        int pageSize = 20, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate parameters
            if (latitude < -90 || latitude > 90)
                return Result<PagedResult<MosquitoDto>>.Failure("Latitude must be between -90 and 90 degrees");

            if (longitude < -180 || longitude > 180)
                return Result<PagedResult<MosquitoDto>>.Failure("Longitude must be between -180 and 180 degrees");

            if (radiusKm <= 0)
                return Result<PagedResult<MosquitoDto>>.Failure("Radius must be greater than 0");

            var location = new Location(latitude, longitude);
            var mosquitoes = await _unitOfWork.MosquitoRepository.GetByLocationAsync(location, radiusKm, cancellationToken);

            // Apply pagination manually since the repository method doesn't support it
            var mosquitoList = mosquitoes.ToList();
            var totalCount = mosquitoList.Count;
            var pagedMosquitoes = mosquitoList
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            var dtos = _mapper.Map<IEnumerable<MosquitoDto>>(pagedMosquitoes);
            var pagedResult = new PagedResult<MosquitoDto>(dtos, pageNumber, pageSize, totalCount);

            return Result<PagedResult<MosquitoDto>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            return Result<PagedResult<MosquitoDto>>.Failure(ex);
        }
    }

    public async Task<Result> ValidateAsync(CreateMosquitoRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage);
                return Result.ValidationFailure(errors);
            }

            // Additional domain validation
            var mosquito = _mapper.Map<Mosquito>(request);
            var domainErrors = mosquito.Validate();
            if (domainErrors.Count > 0)
            {
                return Result.ValidationFailure(domainErrors);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex);
        }
    }

    #region Private Helper Methods

    private static IReadOnlyList<SpeciesStatisticDto> CreateSpeciesBreakdown(Dictionary<MosquitoSpecies, int> speciesStats, int totalCount)
    {
        return speciesStats.Select(kvp => new SpeciesStatisticDto
        {
            Species = kvp.Key,
            SpeciesName = GetSpeciesDisplayName(kvp.Key),
            Count = kvp.Value,
            Percentage = totalCount > 0 ? (double)kvp.Value / totalCount * 100 : 0
        }).ToList().AsReadOnly();
    }

    private static IReadOnlyList<SizeStatisticDto> CreateSizeBreakdown(Dictionary<MosquitoSize, int> sizeStats, int totalCount)
    {
        return sizeStats.Select(kvp => new SizeStatisticDto
        {
            Size = kvp.Key,
            SizeDescription = GetSizeDisplayName(kvp.Key),
            Count = kvp.Value,
            Percentage = totalCount > 0 ? (double)kvp.Value / totalCount * 100 : 0
        }).ToList().AsReadOnly();
    }

    private static IReadOnlyList<LocationStatisticDto> CreateLocationBreakdown(List<Mosquito> mosquitoes, int totalCount)
    {
        return mosquitoes
            .Where(m => !string.IsNullOrEmpty(m.Location.Country))
            .GroupBy(m => m.Location.Country!)
            .Select(g => new LocationStatisticDto
            {
                Country = g.Key,
                Count = g.Count(),
                Percentage = totalCount > 0 ? (double)g.Count() / totalCount * 100 : 0,
                UniqueSpeciesCount = g.Select(m => m.Species).Distinct().Count(),
                DiseaseVectorCount = g.Count(m => m.IsDiseaseVector)
            })
            .OrderByDescending(x => x.Count)
            .ToList()
            .AsReadOnly();
    }

    private static IReadOnlyList<TimeStatisticDto> CreateTimeBreakdown(List<Mosquito> mosquitoes)
    {
        return mosquitoes
            .GroupBy(m => new { m.ObservedAt.Year, m.ObservedAt.Month })
            .Select(g => new TimeStatisticDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Period = $"{g.Key.Year:D4}-{g.Key.Month:D2}",
                Count = g.Count(),
                UniqueSpeciesCount = g.Select(m => m.Species).Distinct().Count()
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToList()
            .AsReadOnly();
    }

    private static MeasurementStatisticsDto CreateMeasurementStatistics(List<Mosquito> mosquitoes)
    {
        if (mosquitoes.Count == 0)
        {
            return new MeasurementStatisticsDto();
        }

        var withWeight = mosquitoes.Where(m => m.WeightMg.HasValue).ToList();

        return new MeasurementStatisticsDto
        {
            AverageLengthMm = mosquitoes.Average(m => m.LengthMm),
            MinLengthMm = mosquitoes.Min(m => m.LengthMm),
            MaxLengthMm = mosquitoes.Max(m => m.LengthMm),
            AverageWeightMg = withWeight.Count > 0 ? withWeight.Average(m => m.WeightMg!.Value) : null,
            WeightDataCount = withWeight.Count
        };
    }

    private static EnvironmentalStatisticsDto CreateEnvironmentalStatistics(List<Mosquito> mosquitoes)
    {
        var withTemperature = mosquitoes.Where(m => m.TemperatureCelsius.HasValue).ToList();
        var withHumidity = mosquitoes.Where(m => m.HumidityPercent.HasValue).ToList();

        return new EnvironmentalStatisticsDto
        {
            AverageTemperatureCelsius = withTemperature.Count > 0 ? withTemperature.Average(m => m.TemperatureCelsius!.Value) : null,
            MinTemperatureCelsius = withTemperature.Count > 0 ? withTemperature.Min(m => m.TemperatureCelsius!.Value) : null,
            MaxTemperatureCelsius = withTemperature.Count > 0 ? withTemperature.Max(m => m.TemperatureCelsius!.Value) : null,
            AverageHumidityPercent = withHumidity.Count > 0 ? withHumidity.Average(m => m.HumidityPercent!.Value) : null,
            MinHumidityPercent = withHumidity.Count > 0 ? withHumidity.Min(m => m.HumidityPercent!.Value) : null,
            MaxHumidityPercent = withHumidity.Count > 0 ? withHumidity.Max(m => m.HumidityPercent!.Value) : null,
            TemperatureDataCount = withTemperature.Count,
            HumidityDataCount = withHumidity.Count
        };
    }

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

    private static string GetSizeDisplayName(MosquitoSize size) => size switch
    {
        MosquitoSize.Small => "Small (< 4mm)",
        MosquitoSize.Medium => "Medium (4-6mm)",
        MosquitoSize.Large => "Large (> 6mm)",
        _ => size.ToString()
    };

    #endregion
}