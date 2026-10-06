using MosquitoApi.Application.Common;
using MosquitoApi.Application.DTOs.User;

namespace MosquitoApi.Application.Interfaces;

/// <summary>
/// Service interface for user management operations
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Creates a new user in the system
    /// </summary>
    /// <param name="request">User creation request</param>
    /// <param name="createdBy">ID of the user creating this user (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created user details</returns>
    Task<Result<UserDto>> CreateUserAsync(CreateUserRequest request, string? createdBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User details if found</returns>
    Task<Result<UserDto>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their username
    /// </summary>
    /// <param name="username">Username</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User details if found</returns>
    Task<Result<UserDto>> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their email address
    /// </summary>
    /// <param name="email">Email address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User details if found</returns>
    Task<Result<UserDto>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing user
    /// </summary>
    /// <param name="userId">User ID to update</param>
    /// <param name="request">Update request with new values</param>
    /// <param name="updatedBy">ID of the user performing the update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated user details</returns>
    Task<Result<UserDto>> UpdateUserAsync(Guid userId, UpdateUserRequest request, string? updatedBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a user (soft delete)
    /// </summary>
    /// <param name="userId">User ID to delete</param>
    /// <param name="deletedBy">ID of the user performing the deletion</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success result</returns>
    Task<Result> DeleteUserAsync(Guid userId, string? deletedBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Permanently deletes a user from the database
    /// </summary>
    /// <param name="userId">User ID to permanently delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success result</returns>
    Task<Result> PermanentlyDeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Restores a soft-deleted user
    /// </summary>
    /// <param name="userId">User ID to restore</param>
    /// <param name="restoredBy">ID of the user performing the restoration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Restored user details</returns>
    Task<Result<UserDto>> RestoreUserAsync(Guid userId, string? restoredBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for users with filtering and pagination
    /// </summary>
    /// <param name="request">Search criteria and pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of users</returns>
    Task<Result<PagedUserResponse>> SearchUsersAsync(UserSearchRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all users (with pagination)
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Items per page</param>
    /// <param name="includeInactive">Whether to include inactive users</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of users</returns>
    Task<Result<PagedUserResponse>> GetAllUsersAsync(int page = 1, int pageSize = 20, bool includeInactive = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates a user account
    /// </summary>
    /// <param name="userId">User ID to activate</param>
    /// <param name="activatedBy">ID of the user performing the activation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success result</returns>
    Task<Result> ActivateUserAsync(Guid userId, string? activatedBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates a user account
    /// </summary>
    /// <param name="userId">User ID to deactivate</param>
    /// <param name="deactivatedBy">ID of the user performing the deactivation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success result</returns>
    Task<Result> DeactivateUserAsync(Guid userId, string? deactivatedBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a username is available
    /// </summary>
    /// <param name="username">Username to check</param>
    /// <param name="excludeUserId">User ID to exclude from check (for updates)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if username is available</returns>
    Task<bool> IsUsernameAvailableAsync(string username, Guid? excludeUserId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an email is available
    /// </summary>
    /// <param name="email">Email to check</param>
    /// <param name="excludeUserId">User ID to exclude from check (for updates)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if email is available</returns>
    Task<bool> IsEmailAvailableAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets user statistics
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User statistics</returns>
    Task<Result<UserSearchSummary>> GetUserStatisticsAsync(CancellationToken cancellationToken = default);
}