using Microsoft.EntityFrameworkCore;
using MosquitoApi.Application.DTOs.User;
using MosquitoApi.Domain.Entities;
using MosquitoApi.Domain.Enums;
using MosquitoApi.Domain.Interfaces;
using MosquitoApi.Infrastructure.Data;

namespace MosquitoApi.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for User entity
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly MosquitoDbContext _context;

    public UserRepository(MosquitoDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets all users
    /// </summary>
    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .OrderBy(u => u.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets all users with pagination
    /// </summary>
    public async Task<IEnumerable<User>> GetAllAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .OrderBy(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets user by ID
    /// </summary>
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    /// <summary>
    /// Gets user by email
    /// </summary>
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    /// <summary>
    /// Checks if user exists with given email
    /// </summary>
    public async Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AnyAsync(u => u.Email == email, cancellationToken);
    }

    /// <summary>
    /// Gets user by username
    /// </summary>
    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    /// <summary>
    /// Gets user by mobile number
    /// </summary>
    public async Task<User?> GetByMobileAsync(string mobile, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Mobile == mobile, cancellationToken);
    }

    /// <summary>
    /// Gets users by role
    /// </summary>
    public async Task<IEnumerable<User>> GetByRoleAsync(RoleType role, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Where(u => u.RoleType == role)
            .OrderBy(u => u.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets active users only
    /// </summary>
    public async Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Where(u => u.IsActive)
            .OrderBy(u => u.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Searches users by criteria
    /// </summary>
    public async Task<(IEnumerable<User> Users, int TotalCount)> SearchAsync(
        string? searchTerm = null,
        RoleType? roleType = null,
        bool? isActive = null,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchLower = searchTerm.ToLowerInvariant();
            query = query.Where(u => 
                u.FirstName!.ToLower().Contains(searchLower) ||
                u.LastName!.ToLower().Contains(searchLower) ||
                u.Username.ToLower().Contains(searchLower) ||
                u.Email.ToLower().Contains(searchLower));
        }

        if (roleType.HasValue)
            query = query.Where(u => u.RoleType == roleType.Value);

        if (isActive.HasValue)
            query = query.Where(u => u.IsActive == isActive.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .OrderBy(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (users, totalCount);
    }

    /// <summary>
    /// Searches users by extended criteria (kept for backward compatibility)
    /// </summary>
    public async Task<IEnumerable<User>> SearchAsync(
        string? searchTerm = null,
        RoleType? role = null,
        bool? isActive = null,
        bool? isEmailVerified = null,
        DateTime? createdAfter = null,
        DateTime? createdBefore = null,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchLower = searchTerm.ToLowerInvariant();
            query = query.Where(u => 
                u.FirstName!.ToLower().Contains(searchLower) ||
                u.LastName!.ToLower().Contains(searchLower) ||
                u.Username.ToLower().Contains(searchLower) ||
                u.Email.ToLower().Contains(searchLower));
        }

        if (role.HasValue)
            query = query.Where(u => u.RoleType == role.Value);

        if (isActive.HasValue)
            query = query.Where(u => u.IsActive == isActive.Value);

        if (isEmailVerified.HasValue)
            query = query.Where(u => u.IsEmailVerified == isEmailVerified.Value);

        if (createdAfter.HasValue)
            query = query.Where(u => u.CreatedAt >= createdAfter.Value);

        if (createdBefore.HasValue)
            query = query.Where(u => u.CreatedAt <= createdBefore.Value);

        return await query
            .OrderBy(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets count of users matching criteria
    /// </summary>
    public async Task<int> GetCountAsync(
        string? searchTerm = null,
        RoleType? role = null,
        bool? isActive = null,
        bool? isEmailVerified = null,
        DateTime? createdAfter = null,
        DateTime? createdBefore = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchLower = searchTerm.ToLowerInvariant();
            query = query.Where(u => 
                u.FirstName.ToLower().Contains(searchLower) ||
                u.LastName.ToLower().Contains(searchLower) ||
                u.Username.ToLower().Contains(searchLower) ||
                u.Email.ToLower().Contains(searchLower));
        }

        if (role.HasValue)
            query = query.Where(u => u.RoleType == role.Value);

        if (isActive.HasValue)
            query = query.Where(u => u.IsActive == isActive.Value);

        if (isEmailVerified.HasValue)
            query = query.Where(u => u.IsEmailVerified == isEmailVerified.Value);

        if (createdAfter.HasValue)
            query = query.Where(u => u.CreatedAt >= createdAfter.Value);

        if (createdBefore.HasValue)
            query = query.Where(u => u.CreatedAt <= createdBefore.Value);

        return await query.CountAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if email already exists
    /// </summary>
    public async Task<bool> IsEmailExistsAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Users.Where(u => u.Email == email);
        
        if (excludeUserId.HasValue)
            query = query.Where(u => u.Id != excludeUserId.Value);
            
        return await query.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if username already exists
    /// </summary>
    public async Task<bool> IsUsernameExistsAsync(string username, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Users.Where(u => u.Username == username);
        
        if (excludeUserId.HasValue)
            query = query.Where(u => u.Id != excludeUserId.Value);
            
        return await query.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if mobile number already exists
    /// </summary>
    public async Task<bool> IsMobileExistsAsync(string mobile, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Users.Where(u => u.Mobile == mobile);
        
        if (excludeUserId.HasValue)
            query = query.Where(u => u.Id != excludeUserId.Value);
            
        return await query.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Gets active users count
    /// </summary>
    public async Task<int> GetActiveUsersCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .CountAsync(u => u.IsActive, cancellationToken);
    }

    /// <summary>
    /// Gets locked out users
    /// </summary>
    public async Task<IEnumerable<User>> GetLockedOutUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Where(u => u.LockoutEndDateUtc.HasValue && u.LockoutEndDateUtc.Value > DateTime.UtcNow)
            .OrderByDescending(u => u.LockoutEndDateUtc)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets users with failed login attempts
    /// </summary>
    public async Task<IEnumerable<User>> GetUsersWithFailedAttemptsAsync(int minFailedAttempts = 1, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Where(u => u.FailedPasswordAttemptCount >= minFailedAttempts)
            .OrderByDescending(u => u.FailedPasswordAttemptCount)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets users created within date range
    /// </summary>
    public async Task<IEnumerable<User>> GetUsersByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate)
            .OrderBy(u => u.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Adds new user
    /// </summary>
    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        return user;
    }

    /// <summary>
    /// Updates existing user
    /// </summary>
    public void Update(User user)
    {
        _context.Users.Update(user);
    }

    /// <summary>
    /// Deletes user
    /// </summary>
    public void Delete(User user)
    {
        _context.Users.Remove(user);
    }

    /// <summary>
    /// Soft deletes user (marks as inactive)
    /// </summary>
    public void SoftDelete(User user)
    {
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        Update(user);
    }

    /// <summary>
    /// Searches users with advanced filtering and pagination
    /// </summary>
    public async Task<(IEnumerable<User> Items, int TotalCount)> SearchAsync(UserSearchRequest request, CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsQueryable();

        // Apply search term filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLowerInvariant();
            query = query.Where(u => 
                (u.FirstName != null && u.FirstName.ToLower().Contains(searchLower)) ||
                (u.LastName != null && u.LastName.ToLower().Contains(searchLower)) ||
                u.Username.ToLower().Contains(searchLower) ||
                u.Email.ToLower().Contains(searchLower));
        }

        // Apply filters
        if (request.RoleType.HasValue)
            query = query.Where(u => u.RoleType == request.RoleType.Value);

        if (request.GenderType.HasValue)
            query = query.Where(u => u.GenderType == request.GenderType.Value);

        if (request.IsActive.HasValue)
            query = query.Where(u => u.IsActive == request.IsActive.Value);

        if (request.IsEmailVerified.HasValue)
            query = query.Where(u => u.IsEmailVerified == request.IsEmailVerified.Value);

        if (request.IsLocked.HasValue)
        {
            if (request.IsLocked.Value)
                query = query.Where(u => u.LockoutEndDateUtc.HasValue && u.LockoutEndDateUtc > DateTime.UtcNow);
            else
                query = query.Where(u => !u.LockoutEndDateUtc.HasValue || u.LockoutEndDateUtc <= DateTime.UtcNow);
        }

        // Date filters
        if (request.CreatedAfter.HasValue)
            query = query.Where(u => u.CreatedAt >= request.CreatedAfter.Value);

        if (request.CreatedBefore.HasValue)
            query = query.Where(u => u.CreatedAt <= request.CreatedBefore.Value);

        if (request.LastLoginAfter.HasValue)
            query = query.Where(u => u.LastLoginDateUtc.HasValue && u.LastLoginDateUtc >= request.LastLoginAfter.Value);

        if (request.LastLoginBefore.HasValue)
            query = query.Where(u => u.LastLoginDateUtc.HasValue && u.LastLoginDateUtc <= request.LastLoginBefore.Value);

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = request.SortBy.ToLower() switch
        {
            "username" => request.SortDirection.ToLower() == "asc" 
                ? query.OrderBy(u => u.Username) 
                : query.OrderByDescending(u => u.Username),
            "email" => request.SortDirection.ToLower() == "asc" 
                ? query.OrderBy(u => u.Email) 
                : query.OrderByDescending(u => u.Email),
            "firstname" => request.SortDirection.ToLower() == "asc" 
                ? query.OrderBy(u => u.FirstName) 
                : query.OrderByDescending(u => u.FirstName),
            "lastname" => request.SortDirection.ToLower() == "asc" 
                ? query.OrderBy(u => u.LastName) 
                : query.OrderByDescending(u => u.LastName),
            "lastlogin" => request.SortDirection.ToLower() == "asc" 
                ? query.OrderBy(u => u.LastLoginDateUtc) 
                : query.OrderByDescending(u => u.LastLoginDateUtc),
            "role" => request.SortDirection.ToLower() == "asc" 
                ? query.OrderBy(u => u.RoleType) 
                : query.OrderByDescending(u => u.RoleType),
            _ => request.SortDirection.ToLower() == "asc" 
                ? query.OrderBy(u => u.CreatedAt) 
                : query.OrderByDescending(u => u.CreatedAt)
        };

        // Apply pagination
        var users = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return (users, totalCount);
    }
}