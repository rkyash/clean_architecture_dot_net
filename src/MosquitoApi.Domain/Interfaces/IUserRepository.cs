using MosquitoApi.Domain.Entities;
using MosquitoApi.Domain.Enums;

namespace MosquitoApi.Domain.Interfaces;

/// <summary>
/// Repository interface for User entity operations
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets all users with optional filtering
    /// </summary>
    Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by ID
    /// </summary>
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by email address
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by username
    /// </summary>
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user exists with the given email
    /// </summary>
    Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new user
    /// </summary>
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing user
    /// </summary>
    void Update(User user);

    /// <summary>
    /// Soft deletes a user
    /// </summary>
    void Delete(User user);

    /// <summary>
    /// Gets users by role type
    /// </summary>
    Task<IEnumerable<User>> GetByRoleAsync(RoleType roleType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active users only
    /// </summary>
    Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches users with pagination
    /// </summary>
    Task<(IEnumerable<User> Users, int TotalCount)> SearchAsync(
        string? searchTerm = null,
        RoleType? roleType = null,
        bool? isActive = null,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);
}