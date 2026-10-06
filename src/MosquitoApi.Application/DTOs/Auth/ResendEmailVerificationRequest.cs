using System.ComponentModel.DataAnnotations;

namespace MosquitoApi.Application.DTOs.Auth;

/// <summary>
/// Request model for resending email verification
/// </summary>
public class ResendEmailVerificationRequest
{
    /// <summary>
    /// Email address to resend verification to
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;
}