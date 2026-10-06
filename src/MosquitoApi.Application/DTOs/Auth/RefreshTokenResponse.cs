namespace MosquitoApi.Application.DTOs.Auth;

/// <summary>
/// Response model for refresh token operation
/// </summary>
public sealed record RefreshTokenResponse
{
    /// <summary>
    /// New JWT access token
    /// </summary>
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>
    /// New refresh token (token rotation)
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;

    /// <summary>
    /// Token type (typically "Bearer")
    /// </summary>
    public string TokenType { get; init; } = "Bearer";

    /// <summary>
    /// Access token expiration time in seconds
    /// </summary>
    public int ExpiresIn { get; init; }

    /// <summary>
    /// Timestamp when the token was issued
    /// </summary>
    public DateTime IssuedAt { get; init; } = DateTime.UtcNow;
}