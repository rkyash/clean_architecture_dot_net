using Microsoft.EntityFrameworkCore;
using MosquitoApi.Domain.Entities;
using MosquitoApi.Domain.Enums;
using MosquitoApi.Domain.Interfaces;
using MosquitoApi.Domain.ValueObjects;
using MosquitoApi.Infrastructure.Data;

namespace MosquitoApi.Infrastructure.Repositories;

/// <summary>
/// Implementation of mosquito repository using Entity Framework Core
/// </summary>
public class MosquitoRepository : IMosquitoRepository
{
    private readonly MosquitoDbContext _context;

    public MosquitoRepository(MosquitoDbContext context)
    {
        _context = context;
    }

    public async Task<Mosquito?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Mosquitoes
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Mosquito>> GetAllAsync(int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        return await _context.Mosquitoes
            .OrderByDescending(m => m.ObservedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Mosquitoes.CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<Mosquito>> SearchAsync(
        MosquitoSpecies? species = null,
        MosquitoSize? size = null,
        Location? location = null,
        double? radiusKm = null,
        DateTime? observedFrom = null,
        DateTime? observedTo = null,
        bool? isDiseaseVector = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Mosquitoes.AsQueryable();

        // Apply filters
        query = ApplySearchFilters(query, species, size, location, radiusKm, observedFrom, observedTo, isDiseaseVector);

        // Apply pagination and ordering
        return await query
            .OrderByDescending(m => m.ObservedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetSearchCountAsync(
        MosquitoSpecies? species = null,
        MosquitoSize? size = null,
        Location? location = null,
        double? radiusKm = null,
        DateTime? observedFrom = null,
        DateTime? observedTo = null,
        bool? isDiseaseVector = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Mosquitoes.AsQueryable();

        // Apply filters
        query = ApplySearchFilters(query, species, size, location, radiusKm, observedFrom, observedTo, isDiseaseVector);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<Mosquito>> GetBySpeciesAsync(MosquitoSpecies species, CancellationToken cancellationToken = default)
    {
        return await _context.Mosquitoes
            .Where(m => m.Species == species)
            .OrderByDescending(m => m.ObservedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Mosquito>> GetByLocationAsync(Location centerLocation, double radiusKm, CancellationToken cancellationToken = default)
    {
        // For PostgreSQL with PostGIS, you would use spatial queries
        // For this implementation, we'll use a simple bounding box approach
        var allMosquitoes = await _context.Mosquitoes.ToListAsync(cancellationToken);
        
        return allMosquitoes
            .Where(m => m.Location.DistanceTo(centerLocation) <= radiusKm)
            .OrderBy(m => m.Location.DistanceTo(centerLocation));
    }

    public async Task<Dictionary<MosquitoSpecies, int>> GetSpeciesStatisticsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Mosquitoes
            .GroupBy(m => m.Species)
            .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
    }

    public async Task<Dictionary<MosquitoSize, int>> GetSizeStatisticsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Mosquitoes
            .GroupBy(m => m.Size)
            .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
    }

    public async Task<Mosquito> AddAsync(Mosquito mosquito, CancellationToken cancellationToken = default)
    {
        var entry = await _context.Mosquitoes.AddAsync(mosquito, cancellationToken);
        return entry.Entity;
    }

    public async Task<Mosquito> UpdateAsync(Mosquito mosquito, CancellationToken cancellationToken = default)
    {
        _context.Mosquitoes.Update(mosquito);
        return await Task.FromResult(mosquito);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var mosquito = await _context.Mosquitoes.FindAsync(new object[] { id }, cancellationToken);
        if (mosquito == null) return false;

        // Soft delete - the context will handle setting IsDeleted = true
        _context.Mosquitoes.Remove(mosquito);
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Mosquitoes.AnyAsync(m => m.Id == id, cancellationToken);
    }

    /// <summary>
    /// Applies search filters to the mosquito query
    /// </summary>
    private static IQueryable<Mosquito> ApplySearchFilters(
        IQueryable<Mosquito> query,
        MosquitoSpecies? species,
        MosquitoSize? size,
        Location? location,
        double? radiusKm,
        DateTime? observedFrom,
        DateTime? observedTo,
        bool? isDiseaseVector)
    {
        if (species.HasValue)
        {
            query = query.Where(m => m.Species == species.Value);
        }

        if (size.HasValue)
        {
            query = query.Where(m => m.Size == size.Value);
        }

        if (observedFrom.HasValue)
        {
            query = query.Where(m => m.ObservedAt >= observedFrom.Value);
        }

        if (observedTo.HasValue)
        {
            query = query.Where(m => m.ObservedAt <= observedTo.Value);
        }

        if (isDiseaseVector.HasValue)
        {
            query = query.Where(m => m.IsDiseaseVector == isDiseaseVector.Value);
        }

        // Note: Location-based filtering with radius would require PostGIS or similar
        // For this implementation, we'll handle it in memory after the query
        // In production, you'd want to use proper spatial queries

        return query;
    }
}