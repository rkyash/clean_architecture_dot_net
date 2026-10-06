using System.ComponentModel.DataAnnotations;

namespace MosquitoApi.Application.DTOs.Auth;

/// <summary>
/// Request model for email verification
/// </summary>
public class VerifyEmailRequest
{
    /// <summary>
    /// Email address to verify
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Verification token
    /// </summary>
    [Required(ErrorMessage = "Verification token is required")]
    public string Token { get; set; } = string.Empty;
}