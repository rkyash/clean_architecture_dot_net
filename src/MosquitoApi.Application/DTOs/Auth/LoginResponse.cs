namespace MosquitoApi.Application.DTOs.Auth;

/// <summary>
/// Response model for successful login
/// </summary>
public sealed record LoginResponse
{
    /// <summary>
    /// JWT access token
    /// </summary>
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>
    /// Refresh token for obtaining new access tokens
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
    /// User information
    /// </summary>
    public UserDto User { get; init; } = null!;

    /// <summary>
    /// Timestamp when the token was issued
    /// </summary>
    public DateTime IssuedAt { get; init; } = DateTime.UtcNow;
}