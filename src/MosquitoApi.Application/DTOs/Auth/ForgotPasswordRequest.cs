using System.ComponentModel.DataAnnotations;

namespace MosquitoApi.Application.DTOs.Auth;

/// <summary>
/// Request model for forgot password
/// </summary>
public sealed record ForgotPasswordRequest
{
    /// <summary>
    /// Email address of the user requesting password reset
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(250, ErrorMessage = "Email cannot exceed 250 characters")]
    public string Email { get; init; } = string.Empty;
}