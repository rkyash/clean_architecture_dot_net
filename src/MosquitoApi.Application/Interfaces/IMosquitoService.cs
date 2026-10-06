using MosquitoApi.Application.Common;
using MosquitoApi.Application.DTOs;

namespace MosquitoApi.Application.Interfaces;

/// <summary>
/// Service contract for mosquito business logic operations
/// </summary>
public interface IMosquitoService
{
    /// <summary>
    /// Gets a mosquito by its unique identifier
    /// </summary>
    /// <param name="id">Mosquito ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Mosquito DTO if found, otherwise failure result</returns>
    Task<Result<MosquitoDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all mosquitoes with pagination
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated result of mosquito DTOs</returns>
    Task<Result<PagedResult<MosquitoDto>>> GetAllAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches mosquitoes based on specified criteria
    /// </summary>
    /// <param name="searchRequest">Search criteria</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated result of matching mosquito DTOs</returns>
    Task<Result<PagedResult<MosquitoDto>>> SearchAsync(MosquitoSearchRequest searchRequest, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new mosquito record
    /// </summary>
    /// <param name="request">Create mosquito request</param>
    /// <param name="userId">ID of the user creating the record</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created mosquito DTO if successful, otherwise failure result</returns>
    Task<Result<MosquitoDto>> CreateAsync(CreateMosquitoRequest request, string? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing mosquito record
    /// </summary>
    /// <param name="id">ID of the mosquito to update</param>
    /// <param name="request">Update mosquito request</param>
    /// <param name="userId">ID of the user updating the record</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated mosquito DTO if successful, otherwise failure result</returns>
    Task<Result<MosquitoDto>> UpdateAsync(Guid id, UpdateMosquitoRequest request, string? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a mosquito record (soft delete)
    /// </summary>
    /// <param name="id">ID of the mosquito to delete</param>
    /// <param name="userId">ID of the user deleting the record</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success result if deleted, otherwise failure result</returns>
    Task<Result> DeleteAsync(Guid id, string? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets comprehensive statistics about mosquito data
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Mosquito statistics DTO</returns>
    Task<Result<MosquitoStatisticsDto>> GetStatisticsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a mosquito exists with the given ID
    /// </summary>
    /// <param name="id">Mosquito ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the mosquito exists</returns>
    Task<Result<bool>> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets mosquitoes within a geographical area
    /// </summary>
    /// <param name="latitude">Center latitude</param>
    /// <param name="longitude">Center longitude</param>
    /// <param name="radiusKm">Search radius in kilometers</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated result of mosquitoes in the area</returns>
    Task<Result<PagedResult<MosquitoDto>>> GetByLocationAsync(
        double latitude, 
        double longitude, 
        double radiusKm, 
        int pageNumber = 1, 
        int pageSize = 20, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates mosquito data without persisting
    /// </summary>
    /// <param name="request">Create mosquito request to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result</returns>
    Task<Result> ValidateAsync(CreateMosquitoRequest request, CancellationToken cancellationToken = default);
}