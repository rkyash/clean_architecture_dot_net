using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MosquitoApi.Application.DTOs.User;
using MosquitoApi.Application.Interfaces;
using System.Security.Claims;

namespace MosquitoApi.Api.Endpoints;

/// <summary>
/// User management endpoints using minimal APIs
/// </summary>
public static class UserEndpoints
{
    /// <summary>
    /// Registers user management endpoints
    /// </summary>
    public static void RegisterUserEndpoints(this WebApplication app)
    {
        var userGroup = app.MapGroup("api/users")
            .WithTags("User Management")
            .WithDescription("User creation, update, deletion and search endpoints")
            .WithOpenApi();

        // Create user endpoint
        userGroup.MapPost("", CreateUserAsync)
            .WithName("CreateUser")
            .WithSummary("Create new user")
            .WithDescription("Creates a new user in the system")
            .Produces<UserDto>(StatusCodes.Status201Created)
            .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict)
            .RequireAuthorization("AdminOnly");

        // Get user by ID endpoint
        userGroup.MapGet("{id:guid}", GetUserByIdAsync)
            .WithName("GetUserById")
            .WithSummary("Get user by ID")
            .WithDescription("Retrieves a user by their unique identifier")
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        // Get current user endpoint
        userGroup.MapGet("me", GetCurrentUserAsync)
            .WithName("GetCurrentUser")
            .WithSummary("Get current user")
            .WithDescription("Retrieves the currently authenticated user's profile")
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        // Get user by username endpoint
        userGroup.MapGet("username/{username}", GetUserByUsernameAsync)
            .WithName("GetUserByUsername")
            .WithSummary("Get user by username")
            .WithDescription("Retrieves a user by their username")
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        // Get user by email endpoint
        userGroup.MapGet("email/{email}", GetUserByEmailAsync)
            .WithName("GetUserByEmail")
            .WithSummary("Get user by email")
            .WithDescription("Retrieves a user by their email address")
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .RequireAuthorization("AdminOnly");

        // Update user endpoint
        userGroup.MapPut("{id:guid}", UpdateUserAsync)
            .WithName("UpdateUser")
            .WithSummary("Update user")
            .WithDescription("Updates an existing user's information")
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict)
            .RequireAuthorization();

        // Update current user endpoint
        userGroup.MapPut("me", UpdateCurrentUserAsync)
            .WithName("UpdateCurrentUser")
            .WithSummary("Update current user")
            .WithDescription("Updates the currently authenticated user's information")
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict)
            .RequireAuthorization();

        // Delete user endpoint (soft delete)
        userGroup.MapDelete("{id:guid}", DeleteUserAsync)
            .WithName("DeleteUser")
            .WithSummary("Delete user")
            .WithDescription("Soft deletes a user (marks as deleted)")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .RequireAuthorization("AdminOnly");

        // Permanently delete user endpoint
        userGroup.MapDelete("{id:guid}/permanent", PermanentlyDeleteUserAsync)
            .WithName("PermanentlyDeleteUser")
            .WithSummary("Permanently delete user")
            .WithDescription("Permanently removes a user from the database")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .RequireAuthorization("SuperAdminOnly");

        // Restore user endpoint
        userGroup.MapPost("{id:guid}/restore", RestoreUserAsync)
            .WithName("RestoreUser")
            .WithSummary("Restore deleted user")
            .WithDescription("Restores a soft-deleted user")
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .RequireAuthorization("AdminOnly");

        // Search users endpoint
        userGroup.MapPost("search", SearchUsersAsync)
            .WithName("SearchUsers")
            .WithSummary("Search users")
            .WithDescription("Searches users with advanced filtering and pagination")
            .Produces<PagedUserResponse>(StatusCodes.Status200OK)
            .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .RequireAuthorization();

        // Get all users endpoint
        userGroup.MapGet("", GetAllUsersAsync)
            .WithName("GetAllUsers")
            .WithSummary("Get all users")
            .WithDescription("Retrieves paginated list of all users")
            .Produces<PagedUserResponse>(StatusCodes.Status200OK)
            .RequireAuthorization();

        // Activate user endpoint
        userGroup.MapPost("{id:guid}/activate", ActivateUserAsync)
            .WithName("ActivateUser")
            .WithSummary("Activate user")
            .WithDescription("Activates a user account")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .RequireAuthorization("AdminOnly");

        // Deactivate user endpoint
        userGroup.MapPost("{id:guid}/deactivate", DeactivateUserAsync)
            .WithName("DeactivateUser")
            .WithSummary("Deactivate user")
            .WithDescription("Deactivates a user account")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .RequireAuthorization("AdminOnly");

        // Check username availability endpoint
        userGroup.MapGet("username/{username}/availability", CheckUsernameAvailabilityAsync)
            .WithName("CheckUsernameAvailability")
            .WithSummary("Check username availability")
            .WithDescription("Checks if a username is available for registration")
            .Produces<bool>(StatusCodes.Status200OK)
            .AllowAnonymous();

        // Check email availability endpoint
        userGroup.MapGet("email/{email}/availability", CheckEmailAvailabilityAsync)
            .WithName("CheckEmailAvailability")
            .WithSummary("Check email availability")
            .WithDescription("Checks if an email is available for registration")
            .Produces<bool>(StatusCodes.Status200OK)
            .AllowAnonymous();

        // Get user statistics endpoint
        userGroup.MapGet("statistics", GetUserStatisticsAsync)
            .WithName("GetUserStatistics")
            .WithSummary("Get user statistics")
            .WithDescription("Retrieves system user statistics")
            .Produces<UserSearchSummary>(StatusCodes.Status200OK)
            .RequireAuthorization("AdminOnly");
    }

    /// <summary>
    /// Create user endpoint
    /// </summary>
    private static async Task<IResult> CreateUserAsync(
        [FromBody] CreateUserRequest request,
        IUserService userService,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var createdBy = context.User.FindFirst("user_id")?.Value;
        var result = await userService.CreateUserAsync(request, createdBy, cancellationToken);

        if (result.IsSuccess)
        {
            return Results.Created($"/api/users/{result.Value.Id}", result.Value);
        }

        if (result.Details?.ContainsKey("ValidationErrors") == true && result.Details["ValidationErrors"] is List<string> validationErrors)
        {
            return Results.ValidationProblem(validationErrors.ToDictionary(e => e, e => new[] { e }));
        }

        return Results.Problem(result.Error, statusCode: StatusCodes.Status409Conflict);
    }

    /// <summary>
    /// Get user by ID endpoint
    /// </summary>
    private static async Task<IResult> GetUserByIdAsync(
        Guid id,
        IUserService userService,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        // Users can only view their own profile unless they're admin
        var currentUserId = context.User.FindFirst("user_id")?.Value;
        var isAdmin = context.User.IsInRole("Admin") || context.User.IsInRole("SuperAdmin");

        if (!isAdmin && currentUserId != id.ToString())
        {
            return Results.Forbid();
        }

        var result = await userService.GetUserByIdAsync(id, cancellationToken);

        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        return Results.Problem(result.Error, statusCode: StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Get current user endpoint
    /// </summary>
    private static async Task<IResult> GetCurrentUserAsync(
        IUserService userService,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var userIdClaim = context.User.FindFirst("user_id")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Problem("Invalid user token", statusCode: StatusCodes.Status401Unauthorized);
        }

        var result = await userService.GetUserByIdAsync(userId, cancellationToken);

        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        return Results.Problem(result.Error, statusCode: StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Get user by username endpoint
    /// </summary>
    private static async Task<IResult> GetUserByUsernameAsync(
        string username,
        IUserService userService,
        CancellationToken cancellationToken)
    {
        var result = await userService.GetUserByUsernameAsync(username, cancellationToken);

        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        return Results.Problem(result.Error, statusCode: StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Get user by email endpoint
    /// </summary>
    private static async Task<IResult> GetUserByEmailAsync(
        string email,
        IUserService userService,
        CancellationToken cancellationToken)
    {
        var result = await userService.GetUserByEmailAsync(email, cancellationToken);

        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        return Results.Problem(result.Error, statusCode: StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Update user endpoint
    /// </summary>
    private static async Task<IResult> UpdateUserAsync(
        Guid id,
        [FromBody] UpdateUserRequest request,
        IUserService userService,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        // Users can only update their own profile unless they're admin
        var currentUserId = context.User.FindFirst("user_id")?.Value;
        var isAdmin = context.User.IsInRole("Admin") || context.User.IsInRole("SuperAdmin");

        if (!isAdmin && currentUserId != id.ToString())
        {
            return Results.Forbid();
        }

        // Non-admin users cannot change role
        if (!isAdmin && request.RoleType.HasValue)
        {
            request.RoleType = null;
        }

        var updatedBy = context.User.FindFirst("user_id")?.Value;
        var result = await userService.UpdateUserAsync(id, request, updatedBy, cancellationToken);

        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        if (result.Details?.ContainsKey("ValidationErrors") == true && result.Details["ValidationErrors"] is List<string> validationErrors)
        {
            return Results.ValidationProblem(validationErrors.ToDictionary(e => e, e => new[] { e }));
        }

        return Results.Problem(result.Error, statusCode: result.Error.Contains("not found") ? StatusCodes.Status404NotFound : StatusCodes.Status409Conflict);
    }

    /// <summary>
    /// Update current user endpoint
    /// </summary>
    private static async Task<IResult> UpdateCurrentUserAsync(
        [FromBody] UpdateUserRequest request,
        IUserService userService,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var userIdClaim = context.User.FindFirst("user_id")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Problem("Invalid user token", statusCode: StatusCodes.Status401Unauthorized);
        }

        // Users cannot change their own role or active status
        request.RoleType = null;
        request.IsActive = null;

        var result = await userService.UpdateUserAsync(userId, request, userIdClaim, cancellationToken);

        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        if (result.Details?.ContainsKey("ValidationErrors") == true && result.Details["ValidationErrors"] is List<string> validationErrors)
        {
            return Results.ValidationProblem(validationErrors.ToDictionary(e => e, e => new[] { e }));
        }

        return Results.Problem(result.Error, statusCode: StatusCodes.Status409Conflict);
    }

    /// <summary>
    /// Delete user endpoint (soft delete)
    /// </summary>
    private static async Task<IResult> DeleteUserAsync(
        Guid id,
        IUserService userService,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var deletedBy = context.User.FindFirst("user_id")?.Value;
        var result = await userService.DeleteUserAsync(id, deletedBy, cancellationToken);

        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        return Results.Problem(result.Error, statusCode: StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Permanently delete user endpoint
    /// </summary>
    private static async Task<IResult> PermanentlyDeleteUserAsync(
        Guid id,
        IUserService userService,
        CancellationToken cancellationToken)
    {
        var result = await userService.PermanentlyDeleteUserAsync(id, cancellationToken);

        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        return Results.Problem(result.Error, statusCode: StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Restore user endpoint
    /// </summary>
    private static async Task<IResult> RestoreUserAsync(
        Guid id,
        IUserService userService,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var restoredBy = context.User.FindFirst("user_id")?.Value;
        var result = await userService.RestoreUserAsync(id, restoredBy, cancellationToken);

        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        return Results.Problem(result.Error, statusCode: StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Search users endpoint
    /// </summary>
    private static async Task<IResult> SearchUsersAsync(
        [FromBody] UserSearchRequest request,
        IUserService userService,
        CancellationToken cancellationToken)
    {
        var result = await userService.SearchUsersAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        if (result.Details?.ContainsKey("ValidationErrors") == true && result.Details["ValidationErrors"] is List<string> validationErrors)
        {
            return Results.ValidationProblem(validationErrors.ToDictionary(e => e, e => new[] { e }));
        }

        return Results.Problem(result.Error, statusCode: StatusCodes.Status400BadRequest);
    }

    /// <summary>
    /// Get all users endpoint
    /// </summary>
    private static async Task<IResult> GetAllUsersAsync(
        IUserService userService,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool includeInactive = false)
    {
        var result = await userService.GetAllUsersAsync(page, pageSize, includeInactive, cancellationToken);

        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        return Results.Problem(result.Error, statusCode: StatusCodes.Status400BadRequest);
    }

    /// <summary>
    /// Activate user endpoint
    /// </summary>
    private static async Task<IResult> ActivateUserAsync(
        Guid id,
        IUserService userService,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var activatedBy = context.User.FindFirst("user_id")?.Value;
        var result = await userService.ActivateUserAsync(id, activatedBy, cancellationToken);

        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        return Results.Problem(result.Error, statusCode: StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Deactivate user endpoint
    /// </summary>
    private static async Task<IResult> DeactivateUserAsync(
        Guid id,
        IUserService userService,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var deactivatedBy = context.User.FindFirst("user_id")?.Value;
        var result = await userService.DeactivateUserAsync(id, deactivatedBy, cancellationToken);

        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        return Results.Problem(result.Error, statusCode: StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Check username availability endpoint
    /// </summary>
    private static async Task<IResult> CheckUsernameAvailabilityAsync(
        string username,
        IUserService userService,
        CancellationToken cancellationToken,
        [FromQuery] Guid? excludeUserId = null)
    {
        var isAvailable = await userService.IsUsernameAvailableAsync(username, excludeUserId, cancellationToken);
        return Results.Ok(isAvailable);
    }

    /// <summary>
    /// Check email availability endpoint
    /// </summary>
    private static async Task<IResult> CheckEmailAvailabilityAsync(
        string email,
        IUserService userService,
        CancellationToken cancellationToken,
        [FromQuery] Guid? excludeUserId = null)
    {
        var isAvailable = await userService.IsEmailAvailableAsync(email, excludeUserId, cancellationToken);
        return Results.Ok(isAvailable);
    }

    /// <summary>
    /// Get user statistics endpoint
    /// </summary>
    private static async Task<IResult> GetUserStatisticsAsync(
        IUserService userService,
        CancellationToken cancellationToken)
    {
        var result = await userService.GetUserStatisticsAsync(cancellationToken);

        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        return Results.Problem(result.Error, statusCode: StatusCodes.Status500InternalServerError);
    }
}