using System.Security.Claims;
using MosquitoApi.Domain.Entities;

namespace MosquitoApi.Application.Interfaces;

/// <summary>
/// Service interface for JWT token operations
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a JWT access token for a user
    /// </summary>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Generates a refresh token for a user
    /// </summary>
    Token GenerateRefreshToken(User user, string? ipAddress = null, string? userAgent = null);

    /// <summary>
    /// Validates a JWT token and returns the claims principal
    /// </summary>
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>
    /// Extracts user ID from JWT token
    /// </summary>
    Guid? GetUserIdFromToken(string token);

    /// <summary>
    /// Extracts claims from JWT token
    /// </summary>
    IEnumerable<Claim> GetClaimsFromToken(string token);

    /// <summary>
    /// Checks if a JWT token has expired
    /// </summary>
    bool IsTokenExpired(string token);

    /// <summary>
    /// Gets the remaining time before token expires
    /// </summary>
    TimeSpan GetTokenRemainingLifetime(string token);

    /// <summary>
    /// Revokes a refresh token
    /// </summary>
    Task RevokeRefreshTokenAsync(string token, string reason, string? ipAddress = null);

    /// <summary>
    /// Rotates a refresh token (creates new one and revokes old one)
    /// </summary>
    Task<Token> RotateRefreshTokenAsync(Token token, string? ipAddress = null, string? userAgent = null);

    /// <summary>
    /// Validates a refresh token
    /// </summary>
    Task<Token?> ValidateRefreshTokenAsync(string token);

    /// <summary>
    /// Cleans up expired tokens
    /// </summary>
    Task CleanupExpiredTokensAsync();
}