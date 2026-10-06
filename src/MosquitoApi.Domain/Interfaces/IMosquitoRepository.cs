using MosquitoApi.Domain.Entities;
using MosquitoApi.Domain.Enums;
using MosquitoApi.Domain.ValueObjects;

namespace MosquitoApi.Domain.Interfaces;

/// <summary>
/// Repository contract for mosquito data access operations
/// </summary>
public interface IMosquitoRepository
{
    /// <summary>
    /// Gets a mosquito by its unique identifier
    /// </summary>
    /// <param name="id">Mosquito ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Mosquito entity or null if not found</returns>
    Task<Mosquito?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all mosquitoes with optional pagination
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of mosquitoes</returns>
    Task<IEnumerable<Mosquito>> GetAllAsync(int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total count of mosquitoes
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total count</returns>
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches mosquitoes by various criteria
    /// </summary>
    /// <param name="species">Optional species filter</param>
    /// <param name="size">Optional size filter</param>
    /// <param name="location">Optional location-based search</param>
    /// <param name="radiusKm">Search radius in kilometers when location is specified</param>
    /// <param name="observedFrom">Optional start date for observation period</param>
    /// <param name="observedTo">Optional end date for observation period</param>
    /// <param name="isDiseaseVector">Optional filter for disease vectors</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of matching mosquitoes</returns>
    Task<IEnumerable<Mosquito>> SearchAsync(
        MosquitoSpecies? species = null,
        MosquitoSize? size = null,
        Location? location = null,
        double? radiusKm = null,
        DateTime? observedFrom = null,
        DateTime? observedTo = null,
        bool? isDiseaseVector = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets count of mosquitoes matching search criteria
    /// </summary>
    /// <param name="species">Optional species filter</param>
    /// <param name="size">Optional size filter</param>
    /// <param name="location">Optional location-based search</param>
    /// <param name="radiusKm">Search radius in kilometers when location is specified</param>
    /// <param name="observedFrom">Optional start date for observation period</param>
    /// <param name="observedTo">Optional end date for observation period</param>
    /// <param name="isDiseaseVector">Optional filter for disease vectors</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of matching mosquitoes</returns>
    Task<int> GetSearchCountAsync(
        MosquitoSpecies? species = null,
        MosquitoSize? size = null,
        Location? location = null,
        double? radiusKm = null,
        DateTime? observedFrom = null,
        DateTime? observedTo = null,
        bool? isDiseaseVector = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets mosquitoes by species
    /// </summary>
    /// <param name="species">Species to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of mosquitoes of the specified species</returns>
    Task<IEnumerable<Mosquito>> GetBySpeciesAsync(MosquitoSpecies species, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets mosquitoes within a geographical area
    /// </summary>
    /// <param name="centerLocation">Center point of the search area</param>
    /// <param name="radiusKm">Search radius in kilometers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of mosquitoes within the specified area</returns>
    Task<IEnumerable<Mosquito>> GetByLocationAsync(Location centerLocation, double radiusKm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets statistics about mosquito species distribution
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary with species as key and count as value</returns>
    Task<Dictionary<MosquitoSpecies, int>> GetSpeciesStatisticsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets statistics about mosquito size distribution
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary with size as key and count as value</returns>
    Task<Dictionary<MosquitoSize, int>> GetSizeStatisticsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new mosquito to the repository
    /// </summary>
    /// <param name="mosquito">Mosquito entity to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The added mosquito entity</returns>
    Task<Mosquito> AddAsync(Mosquito mosquito, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing mosquito in the repository
    /// </summary>
    /// <param name="mosquito">Mosquito entity to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated mosquito entity</returns>
    Task<Mosquito> UpdateAsync(Mosquito mosquito, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a mosquito from the repository (soft delete)
    /// </summary>
    /// <param name="id">ID of the mosquito to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the mosquito was deleted, false if not found</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a mosquito exists with the given ID
    /// </summary>
    /// <param name="id">Mosquito ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the mosquito exists</returns>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}