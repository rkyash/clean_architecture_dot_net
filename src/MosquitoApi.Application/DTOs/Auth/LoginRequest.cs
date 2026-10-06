using System.ComponentModel.DataAnnotations;

namespace MosquitoApi.Application.DTOs.Auth;

/// <summary>
/// Request model for user login
/// </summary>
public sealed record LoginRequest
{
    /// <summary>
    /// User email address
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(250, ErrorMessage = "Email cannot exceed 250 characters")]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// User password
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Remember me for extended session
    /// </summary>
    public bool RememberMe { get; init; } = false;
}