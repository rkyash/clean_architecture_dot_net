using AutoMapper;
using Microsoft.Extensions.Logging;
using MosquitoApi.Application.Common;
using MosquitoApi.Application.DTOs;
using MosquitoApi.Application.DTOs.Auth;
using MosquitoApi.Application.Interfaces;
using MosquitoApi.Domain.Entities;
using MosquitoApi.Domain.Interfaces;

namespace MosquitoApi.Application.Services;

/// <summary>
/// Service for authentication operations
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenRepository _tokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        ITokenRepository tokenRepository,
        IUnitOfWork unitOfWork,
        IPasswordService passwordService,
        ITokenService tokenService,
        IMapper mapper,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _tokenRepository = tokenRepository;
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
        _tokenService = tokenService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user with email and password
    /// </summary>
    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, string? ipAddress = null, string? userAgent = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", request.Email);

            // Get user by email
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Login failed - user not found: {Email}", request.Email);
                return Result<LoginResponse>.Failure("Invalid email or password");
            }

            // Check if account is locked
            if (user.IsLockedOut)
            {
                _logger.LogWarning("Login failed - account locked: {Email}", request.Email);
                return Result<LoginResponse>.Failure($"Account is locked until {user.LockoutEndDateUtc:yyyy-MM-dd HH:mm:ss} UTC");
            }

            // Check if account is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Login failed - account inactive: {Email}", request.Email);
                return Result<LoginResponse>.Failure("Account is deactivated");
            }

            // Verify password
            if (!_passwordService.VerifyPassword(request.Password, user.Password))
            {
                _logger.LogWarning("Login failed - invalid password: {Email}", request.Email);
                
                // Increment failed attempts
                user.IncrementFailedPasswordAttempts();
                _userRepository.Update(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<LoginResponse>.Failure("Invalid email or password");
            }

            // Reset failed attempts on successful login
            user.ResetFailedPasswordAttempts();

            // Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken(user, ipAddress, userAgent);

            // Save refresh token
            await _tokenRepository.AddAsync(refreshToken, cancellationToken);
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successful login for user: {UserId}", user.Id);

            var userDto = _mapper.Map<UserDto>(user);
            var response = new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.RefreshToken,
                ExpiresIn = 15 * 60, // 15 minutes in seconds
                User = userDto
            };

            return Result<LoginResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
            return Result<LoginResponse>.Failure("An error occurred during login");
        }
    }

    /// <summary>
    /// Refreshes access token using refresh token
    /// </summary>
    public async Task<Result<RefreshTokenResponse>> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress = null, string? userAgent = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Token refresh attempt");

            // Validate refresh token
            var token = await _tokenService.ValidateRefreshTokenAsync(request.RefreshToken);
            if (token == null)
            {
                _logger.LogWarning("Token refresh failed - invalid token");
                return Result<RefreshTokenResponse>.Failure("Invalid refresh token");
            }

            // Get user
            var user = await _userRepository.GetByIdAsync(token.UserId, cancellationToken);
            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("Token refresh failed - user not found or inactive: {UserId}", token.UserId);
                return Result<RefreshTokenResponse>.Failure("User not found or inactive");
            }

            // Rotate refresh token
            var newToken = await _tokenService.RotateRefreshTokenAsync(token, ipAddress, userAgent);

            // Generate new access token
            var accessToken = _tokenService.GenerateAccessToken(user);

            _logger.LogInformation("Token refreshed successfully for user: {UserId}", user.Id);

            var response = new RefreshTokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = newToken.RefreshToken,
                ExpiresIn = 15 * 60 // 15 minutes in seconds
            };

            return Result<RefreshTokenResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return Result<RefreshTokenResponse>.Failure("An error occurred during token refresh");
        }
    }

    /// <summary>
    /// Logs out a user by revoking their tokens
    /// </summary>
    public async Task<Result> LogoutAsync(string refreshToken, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            await _tokenService.RevokeRefreshTokenAsync(refreshToken, "Logout", ipAddress);
            _logger.LogInformation("User logged out successfully");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return Result.Failure("An error occurred during logout");
        }
    }

    /// <summary>
    /// Logs out a user from all devices by revoking all their tokens
    /// </summary>
    public async Task<Result> LogoutAllDevicesAsync(Guid userId, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            await _tokenRepository.RevokeAllUserTokensAsync(userId, "Logout from all devices", ipAddress, cancellationToken);
            _logger.LogInformation("User logged out from all devices: {UserId}", userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout from all devices for user: {UserId}", userId);
            return Result.Failure("An error occurred during logout");
        }
    }

    /// <summary>
    /// Sends a password reset email
    /// </summary>
    public async Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Password reset requested for email: {Email}", request.Email);

            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null || !user.IsActive)
            {
                // Don't reveal if user exists - return success anyway for security
                _logger.LogWarning("Password reset requested for non-existent or inactive user: {Email}", request.Email);
                return Result.Success();
            }

            // Generate reset token
            user.GeneratePasswordResetToken();
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // TODO: Send email with reset link
            // await _emailService.SendPasswordResetEmailAsync(user.Email, user.ResetId);

            _logger.LogInformation("Password reset token generated for user: {UserId}", user.Id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset request for email: {Email}", request.Email);
            return Result.Failure("An error occurred during password reset request");
        }
    }

    /// <summary>
    /// Resets password using reset token
    /// </summary>
    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Password reset attempt for email: {Email}", request.Email);

            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null)
            {
                return Result.Failure("Invalid reset token or email");
            }

            if (!user.IsPasswordResetTokenValid(request.Token))
            {
                _logger.LogWarning("Invalid or expired reset token for user: {UserId}", user.Id);
                return Result.Failure("Invalid or expired reset token");
            }

            // Validate new password
            var (isValid, errors) = _passwordService.ValidatePasswordStrength(request.NewPassword);
            if (!isValid)
            {
                return Result.ValidationFailure(errors);
            }

            // Hash and set new password
            user.Password = _passwordService.HashPassword(request.NewPassword);
            user.ClearPasswordResetToken();
            user.ResetFailedPasswordAttempts(); // Clear any lockout

            // Revoke all tokens to force re-login
            await _tokenRepository.RevokeAllUserTokensAsync(user.Id, "Password reset", null, cancellationToken);

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Password reset successfully for user: {UserId}", user.Id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset for email: {Email}", request.Email);
            return Result.Failure("An error occurred during password reset");
        }
    }

    /// <summary>
    /// Changes user password (requires current password)
    /// </summary>
    public async Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return Result.Failure("User not found");
            }

            // Verify current password
            if (!_passwordService.VerifyPassword(currentPassword, user.Password))
            {
                return Result.Failure("Current password is incorrect");
            }

            // Validate new password
            var (isValid, errors) = _passwordService.ValidatePasswordStrength(newPassword);
            if (!isValid)
            {
                return Result.ValidationFailure(errors);
            }

            // Set new password
            user.Password = _passwordService.HashPassword(newPassword);
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Password changed successfully for user: {UserId}", userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password change for user: {UserId}", userId);
            return Result.Failure("An error occurred during password change");
        }
    }

    /// <summary>
    /// Verifies email address using verification token
    /// </summary>
    public async Task<Result> VerifyEmailAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user == null)
            {
                return Result.Failure("Invalid verification link");
            }

            if (user.EmailVerificationToken != token)
            {
                return Result.Failure("Invalid verification token");
            }

            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Email verified successfully for user: {UserId}", user.Id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email verification for email: {Email}", email);
            return Result.Failure("An error occurred during email verification");
        }
    }

    /// <summary>
    /// Resends email verification
    /// </summary>
    public async Task<Result> ResendEmailVerificationAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user == null || user.IsEmailVerified)
            {
                return Result.Success(); // Don't reveal if user exists
            }

            user.EmailVerificationToken = _passwordService.GeneratePasswordResetToken();
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // TODO: Send verification email
            // await _emailService.SendVerificationEmailAsync(user.Email, user.EmailVerificationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending email verification for email: {Email}", email);
            return Result.Failure("An error occurred while resending verification email");
        }
    }

    /// <summary>
    /// Checks if a user account is locked out
    /// </summary>
    public async Task<bool> IsAccountLockedAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        return user?.IsLockedOut ?? false;
    }

    /// <summary>
    /// Unlocks a user account (admin only)
    /// </summary>
    public async Task<Result> UnlockAccountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return Result.Failure("User not found");
            }

            user.ResetFailedPasswordAttempts();
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Account unlocked for user: {UserId}", userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking account for user: {UserId}", userId);
            return Result.Failure("An error occurred while unlocking account");
        }
    }
}