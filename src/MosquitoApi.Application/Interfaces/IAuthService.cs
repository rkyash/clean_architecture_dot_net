using MosquitoApi.Application.Common;
using MosquitoApi.Application.DTOs;
using MosquitoApi.Application.DTOs.Auth;

namespace MosquitoApi.Application.Interfaces;

/// <summary>
/// Service interface for authentication operations
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user with email and password
    /// </summary>
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, string? ipAddress = null, string? userAgent = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes access token using refresh token
    /// </summary>
    Task<Result<RefreshTokenResponse>> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress = null, string? userAgent = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs out a user by revoking their tokens
    /// </summary>
    Task<Result> LogoutAsync(string refreshToken, string? ipAddress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs out a user from all devices by revoking all their tokens
    /// </summary>
    Task<Result> LogoutAllDevicesAsync(Guid userId, string? ipAddress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a password reset email
    /// </summary>
    Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets password using reset token
    /// </summary>
    Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes user password (requires current password)
    /// </summary>
    Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies email address using verification token
    /// </summary>
    Task<Result> VerifyEmailAsync(string email, string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resends email verification
    /// </summary>
    Task<Result> ResendEmailVerificationAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user account is locked out
    /// </summary>
    Task<bool> IsAccountLockedAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unlocks a user account (admin only)
    /// </summary>
    Task<Result> UnlockAccountAsync(Guid userId, CancellationToken cancellationToken = default);
}