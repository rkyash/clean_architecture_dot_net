using AutoMapper;
using Microsoft.Extensions.Logging;
using MosquitoApi.Application.Common;
using MosquitoApi.Application.DTOs.User;
using MosquitoApi.Application.Interfaces;
using MosquitoApi.Domain.Entities;
using MosquitoApi.Domain.Interfaces;
using BCrypt.Net;
using MosquitoApi.Domain.Enums;

namespace MosquitoApi.Application.Services;

/// <summary>
/// Service implementation for user management operations
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<UserDto>> CreateUserAsync(CreateUserRequest request, string? createdBy = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating new user with username: {Username}", request.Username);

            // Check if username or email already exists
            var existingUser = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
            if (existingUser != null)
            {
                return Result<UserDto>.Failure("Username is already taken");
            }

            existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (existingUser != null)
            {
                return Result<UserDto>.Failure("Email address is already registered");
            }

            // Map request to User entity
            var user = _mapper.Map<User>(request);
            
            // Hash password
            user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password, BCrypt.Net.BCrypt.GenerateSalt(12));
            
            // Set audit fields
            user.CreatedBy = createdBy;
            user.CreatedAt = DateTime.UtcNow;

            // Generate email verification token if requested
            if (request.SendEmailVerification)
            {
                user.EmailVerificationToken = GenerateEmailVerificationToken();
            }

            // Add user to repository
            await _userRepository.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User created successfully with ID: {UserId}", user.Id);

            // Map to DTO and return
            var userDto = _mapper.Map<UserDto>(user);
            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user with username: {Username}", request.Username);
            return Result<UserDto>.Failure("An error occurred while creating the user");
        }
    }

    public async Task<Result<UserDto>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return Result<UserDto>.Failure("User not found");
            }

            var userDto = _mapper.Map<UserDto>(user);
            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with ID: {UserId}", userId);
            return Result<UserDto>.Failure("An error occurred while retrieving the user");
        }
    }

    public async Task<Result<UserDto>> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByUsernameAsync(username, cancellationToken);
            if (user == null)
            {
                return Result<UserDto>.Failure("User not found");
            }

            var userDto = _mapper.Map<UserDto>(user);
            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with username: {Username}", username);
            return Result<UserDto>.Failure("An error occurred while retrieving the user");
        }
    }

    public async Task<Result<UserDto>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user == null)
            {
                return Result<UserDto>.Failure("User not found");
            }

            var userDto = _mapper.Map<UserDto>(user);
            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with email: {Email}", email);
            return Result<UserDto>.Failure("An error occurred while retrieving the user");
        }
    }

    public async Task<Result<UserDto>> UpdateUserAsync(Guid userId, UpdateUserRequest request, string? updatedBy = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating user with ID: {UserId}", userId);

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return Result<UserDto>.Failure("User not found");
            }

            // Check for username/email conflicts
            if (!string.IsNullOrEmpty(request.Username) && request.Username != user.Username)
            {
                var existingUser = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
                if (existingUser != null && existingUser.Id != userId)
                {
                    return Result<UserDto>.Failure("Username is already taken");
                }
            }

            if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
            {
                var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
                if (existingUser != null && existingUser.Id != userId)
                {
                    return Result<UserDto>.Failure("Email address is already registered");
                }
            }

            // Apply updates
            _mapper.Map(request, user);
            
            // Set audit fields
            user.UpdatedBy = updatedBy;
            user.UpdatedAt = DateTime.UtcNow;

            // Update repository
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User updated successfully with ID: {UserId}", userId);

            var userDto = _mapper.Map<UserDto>(user);
            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user with ID: {UserId}", userId);
            return Result<UserDto>.Failure("An error occurred while updating the user");
        }
    }

    public async Task<Result> DeleteUserAsync(Guid userId, string? deletedBy = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Soft deleting user with ID: {UserId}", userId);

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return Result.Failure("User not found");
            }

            if (user.IsDeleted)
            {
                return Result.Failure("User is already deleted");
            }

            // Soft delete
            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            user.DeletedBy = deletedBy;
            user.IsActive = false;

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User soft deleted successfully with ID: {UserId}", userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user with ID: {UserId}", userId);
            return Result.Failure("An error occurred while deleting the user");
        }
    }

    public async Task<Result> PermanentlyDeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Permanently deleting user with ID: {UserId}", userId);

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return Result.Failure("User not found");
            }

            _userRepository.Delete(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User permanently deleted with ID: {UserId}", userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error permanently deleting user with ID: {UserId}", userId);
            return Result.Failure("An error occurred while permanently deleting the user");
        }
    }

    public async Task<Result<UserDto>> RestoreUserAsync(Guid userId, string? restoredBy = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Restoring user with ID: {UserId}", userId);

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return Result<UserDto>.Failure("User not found");
            }

            if (!user.IsDeleted)
            {
                return Result<UserDto>.Failure("User is not deleted");
            }

            // Restore
            user.IsDeleted = false;
            user.DeletedAt = null;
            user.DeletedBy = null;
            user.IsActive = true;
            user.UpdatedBy = restoredBy;
            user.UpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User restored successfully with ID: {UserId}", userId);

            var userDto = _mapper.Map<UserDto>(user);
            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring user with ID: {UserId}", userId);
            return Result<UserDto>.Failure("An error occurred while restoring the user");
        }
    }

    public async Task<Result<PagedUserResponse>> SearchUsersAsync(UserSearchRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // For now, use the basic search method until we can properly refactor
            var users = await _userRepository.SearchAsync(
                request.SearchTerm,
                request.RoleType,
                request.IsActive,
                request.Page,
                request.PageSize,
                cancellationToken);

            var userDtos = _mapper.Map<List<UserDto>>(users.Users);

            var response = new PagedUserResponse
            {
                Users = userDtos,
                CurrentPage = request.Page,
                PageSize = request.PageSize,
                TotalCount = users.TotalCount,
                TotalPages = (int)Math.Ceiling((double)users.TotalCount / request.PageSize),
                Summary = await CalculateUserSummary(users.Users)
            };

            return Result<PagedUserResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users");
            return Result<PagedUserResponse>.Failure("An error occurred while searching users");
        }
    }

    public async Task<Result<PagedUserResponse>> GetAllUsersAsync(int page = 1, int pageSize = 20, bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var searchRequest = new UserSearchRequest
        {
            Page = page,
            PageSize = pageSize,
            IsActive = includeInactive ? null : true
        };

        return await SearchUsersAsync(searchRequest, cancellationToken);
    }

    public async Task<Result> ActivateUserAsync(Guid userId, string? activatedBy = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return Result.Failure("User not found");
            }

            user.IsActive = true;
            user.UpdatedBy = activatedBy;
            user.UpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating user with ID: {UserId}", userId);
            return Result.Failure("An error occurred while activating the user");
        }
    }

    public async Task<Result> DeactivateUserAsync(Guid userId, string? deactivatedBy = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return Result.Failure("User not found");
            }

            user.IsActive = false;
            user.UpdatedBy = deactivatedBy;
            user.UpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user with ID: {UserId}", userId);
            return Result.Failure("An error occurred while deactivating the user");
        }
    }

    public async Task<bool> IsUsernameAvailableAsync(string username, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByUsernameAsync(username, cancellationToken);
            return user == null || (excludeUserId.HasValue && user.Id == excludeUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking username availability: {Username}", username);
            return false;
        }
    }

    public async Task<bool> IsEmailAvailableAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            return user == null || (excludeUserId.HasValue && user.Id == excludeUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email availability: {Email}", email);
            return false;
        }
    }

    public async Task<Result<UserSearchSummary>> GetUserStatisticsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var allUsers = await _userRepository.GetAllAsync(cancellationToken);
            var summary = await CalculateUserSummary(allUsers);
            return Result<UserSearchSummary>.Success(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating user statistics");
            return Result<UserSearchSummary>.Failure("An error occurred while calculating user statistics");
        }
    }

    private async Task<UserSearchSummary> CalculateUserSummary(IEnumerable<User> users)
    {
        return await Task.FromResult(new UserSearchSummary
        {
            ActiveUsers = users.Count(u => u.IsActive),
            InactiveUsers = users.Count(u => !u.IsActive),
            EmailVerifiedUsers = users.Count(u => u.IsEmailVerified),
            LockedUsers = users.Count(u => u.LockoutEndDateUtc.HasValue && u.LockoutEndDateUtc > DateTime.UtcNow),
            UsersByRole = users.GroupBy(u => u.RoleType.ToString())
                              .ToDictionary(g => g.Key, g => g.Count())
        });
    }

    private static string GenerateEmailVerificationToken()
    {
        return Guid.NewGuid().ToString("N");
    }
}