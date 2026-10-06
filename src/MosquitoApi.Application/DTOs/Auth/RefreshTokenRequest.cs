using System.ComponentModel.DataAnnotations;

namespace MosquitoApi.Application.DTOs.Auth;

/// <summary>
/// Request model for refreshing access token
/// </summary>
public sealed record RefreshTokenRequest
{
    /// <summary>
    /// Refresh token to use for obtaining new access token
    /// </summary>
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; init; } = string.Empty;
}