using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MosquitoApi.Application.DTOs.Auth;
using MosquitoApi.Application.Interfaces;

namespace MosquitoApi.Api.Endpoints;

/// <summary>
/// Authentication endpoints using minimal APIs
/// </summary>
public static class AuthEndpoints
{
    /// <summary>
    /// Registers authentication endpoints
    /// </summary>
    public static void RegisterAuthEndpoints(this WebApplication app)
    {
        var authGroup = app.MapGroup("api/auth")
            .WithTags("Authentication")
            .WithDescription("Authentication and authorization endpoints")
            .WithOpenApi();

        // Login endpoint
        authGroup.MapPost("login", LoginAsync)
            .WithName("Login")
            .WithSummary("User login")
            .WithDescription("Authenticates a user with email and password")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .AllowAnonymous();

        // Refresh token endpoint
        authGroup.MapPost("refresh", RefreshTokenAsync)
            .WithName("RefreshToken")
            .WithSummary("Refresh access token")
            .WithDescription("Refreshes an access token using a refresh token")
            .Produces<RefreshTokenResponse>(StatusCodes.Status200OK)
            .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .AllowAnonymous();

        // Logout endpoint
        authGroup.MapPost("logout", LogoutAsync)
            .WithName("Logout")
            .WithSummary("User logout")
            .WithDescription("Logs out a user by revoking their refresh token")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        // Logout from all devices endpoint
        authGroup.MapPost("logout-all", LogoutAllDevicesAsync)
            .WithName("LogoutAllDevices")
            .WithSummary("Logout from all devices")
            .WithDescription("Logs out a user from all devices by revoking all their tokens")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        // Forgot password endpoint
        authGroup.MapPost("forgot-password", ForgotPasswordAsync)
            .WithName("ForgotPassword")
            .WithSummary("Request password reset")
            .WithDescription("Sends a password reset email to the user")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        // Reset password endpoint
        authGroup.MapPost("reset-password", ResetPasswordAsync)
            .WithName("ResetPassword")
            .WithSummary("Reset password")
            .WithDescription("Resets user password using a reset token")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        // Change password endpoint
        authGroup.MapPost("change-password", ChangePasswordAsync)
            .WithName("ChangePassword")
            .WithSummary("Change password")
            .WithDescription("Changes user password (requires current password)")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        // Verify email endpoint
        authGroup.MapPost("verify-email", VerifyEmailAsync)
            .WithName("VerifyEmail")
            .WithSummary("Verify email address")
            .WithDescription("Verifies a user's email address using a verification token")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        // Resend email verification endpoint
        authGroup.MapPost("resend-verification", ResendEmailVerificationAsync)
            .WithName("ResendEmailVerification")
            .WithSummary("Resend email verification")
            .WithDescription("Resends email verification to the user")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        // Check account status endpoint
        authGroup.MapGet("account-status/{email}", GetAccountStatusAsync)
            .WithName("GetAccountStatus")
            .WithSummary("Check account status")
            .WithDescription("Checks if an account is locked or active")
            .Produces<AccountStatusResponse>(StatusCodes.Status200OK)
            .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        // Unlock account endpoint (Admin only)
        authGroup.MapPost("unlock-account/{userId:guid}", UnlockAccountAsync)
            .WithName("UnlockAccount")
            .WithSummary("Unlock user account")
            .WithDescription("Unlocks a user account (Admin only)")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
            .RequireAuthorization("AdminOnly");
    }

    /// <summary>
    /// User login endpoint
    /// </summary>
    private static async Task<IResult> LoginAsync(
        [FromBody] LoginRequest request,
        IAuthService authService,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        var userAgent = context.Request.Headers.UserAgent.ToString();

        var result = await authService.LoginAsync(request, ipAddress, userAgent, cancellationToken);

        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        if (result.Details?.ContainsKey("ValidationErrors") == true && result.Details["ValidationErrors"] is List<string> validationErrors)
        {
            return Results.ValidationProblem(validationErrors.ToDictionary(e => e, e => new[] { e }));
        }

        return Results.Problem(result.Error, statusCode: StatusCodes.Status401Unauthorized);
    }

    /// <summary>
    /// Refresh token endpoint
    /// </summary>
    private static async Task<IResult> RefreshTokenAsync(
        [FromBody] RefreshTokenRequest request,
        IAuthService authService,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        var userAgent = context.Request.Headers.UserAgent.ToString();

        var result = await authService.RefreshTokenAsync(request, ipAddress, userAgent, cancellationToken);

        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        if (result.Details?.ContainsKey("ValidationErrors") == true && result.Details["ValidationErrors"] is List<string> validationErrors)
        {
            return Results.ValidationProblem(validationErrors.ToDictionary(e => e, e => new[] { e }));
        }

        return Results.Problem(result.Error, statusCode: StatusCodes.Status401Unauthorized);
    }

    /// <summary>
    /// Logout endpoint
    /// </summary>
    private static async Task<IResult> LogoutAsync(
        [FromBody] LogoutRequest request,
        IAuthService authService,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();

        var result = await authService.LogoutAsync(request.RefreshToken, ipAddress, cancellationToken);

        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        if (result.Details?.ContainsKey("ValidationErrors") == true && result.Details["ValidationErrors"] is List<string> validationErrors)
        {
            return Results.ValidationProblem(validationErrors.ToDictionary(e => e, e => new[] { e }));
        }

        return Results.Problem(result.Error, statusCode: StatusCodes.Status400BadRequest);
    }

    /// <summary>
    /// Logout from all devices endpoint
    /// </summary>
    private static async Task<IResult> LogoutAllDevicesAsync(
        IAuthService authService,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var userIdClaim = context.User.FindFirst("user_id")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Problem("Invalid user token", statusCode: StatusCodes.Status401Unauthorized);
        }

        var ipAddress = context.Connection.RemoteIpAddress?.ToString();

        var result = await authService.LogoutAllDevicesAsync(userId, ipAddress, cancellationToken);

        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        return Results.Problem(result.Error, statusCode: StatusCodes.Status400BadRequest);
    }

    /// <summary>
    /// Forgot password endpoint
    /// </summary>
    private static async Task<IResult> ForgotPasswordAsync(
        [FromBody] ForgotPasswordRequest request,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.ForgotPasswordAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        if (result.Details?.ContainsKey("ValidationErrors") == true && result.Details["ValidationErrors"] is List<string> validationErrors)
        {
            return Results.ValidationProblem(validationErrors.ToDictionary(e => e, e => new[] { e }));
        }

        return Results.Problem(result.Error, statusCode: StatusCodes.Status400BadRequest);
    }

    /// <summary>
    /// Reset password endpoint
    /// </summary>
    private static async Task<IResult> ResetPasswordAsync(
        [FromBody] ResetPasswordRequest request,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.ResetPasswordAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        if (result.Details?.ContainsKey("ValidationErrors") == true && result.Details["ValidationErrors"] is List<string> validationErrors)
        {
            return Results.ValidationProblem(validationErrors.ToDictionary(e => e, e => new[] { e }));
        }

        return Results.Problem(result.Error, statusCode: StatusCodes.Status400BadRequest);
    }

    /// <summary>
    /// Change password endpoint
    /// </summary>
    private static async Task<IResult> ChangePasswordAsync(
        [FromBody] ChangePasswordRequest request,
        IAuthService authService,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var userIdClaim = context.User.FindFirst("user_id")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Problem("Invalid user token", statusCode: StatusCodes.Status401Unauthorized);
        }

        var result = await authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword, cancellationToken);

        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        if (result.Details?.ContainsKey("ValidationErrors") == true && result.Details["ValidationErrors"] is List<string> validationErrors)
        {
            return Results.ValidationProblem(validationErrors.ToDictionary(e => e, e => new[] { e }));
        }

        return Results.Problem(result.Error, statusCode: StatusCodes.Status400BadRequest);
    }

    /// <summary>
    /// Verify email endpoint
    /// </summary>
    private static async Task<IResult> VerifyEmailAsync(
        [FromBody] VerifyEmailRequest request,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.VerifyEmailAsync(request.Email, request.Token, cancellationToken);

        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        if (result.Details?.ContainsKey("ValidationErrors") == true && result.Details["ValidationErrors"] is List<string> validationErrors)
        {
            return Results.ValidationProblem(validationErrors.ToDictionary(e => e, e => new[] { e }));
        }

        return Results.Problem(result.Error, statusCode: StatusCodes.Status400BadRequest);
    }

    /// <summary>
    /// Resend email verification endpoint
    /// </summary>
    private static async Task<IResult> ResendEmailVerificationAsync(
        [FromBody] ResendEmailVerificationRequest request,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.ResendEmailVerificationAsync(request.Email, cancellationToken);

        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        if (result.Details?.ContainsKey("ValidationErrors") == true && result.Details["ValidationErrors"] is List<string> validationErrors)
        {
            return Results.ValidationProblem(validationErrors.ToDictionary(e => e, e => new[] { e }));
        }

        return Results.Problem(result.Error, statusCode: StatusCodes.Status400BadRequest);
    }

    /// <summary>
    /// Get account status endpoint
    /// </summary>
    private static async Task<IResult> GetAccountStatusAsync(
        string email,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        var isLocked = await authService.IsAccountLockedAsync(email, cancellationToken);

        var response = new AccountStatusResponse
        {
            Email = email,
            IsLocked = isLocked
        };

        return Results.Ok(response);
    }

    /// <summary>
    /// Unlock account endpoint (Admin only)
    /// </summary>
    private static async Task<IResult> UnlockAccountAsync(
        Guid userId,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.UnlockAccountAsync(userId, cancellationToken);

        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        return Results.Problem(result.Error, statusCode: StatusCodes.Status400BadRequest);
    }
}