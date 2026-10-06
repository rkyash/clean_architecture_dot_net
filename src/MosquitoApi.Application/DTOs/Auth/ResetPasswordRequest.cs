using System.ComponentModel.DataAnnotations;

namespace MosquitoApi.Application.DTOs.Auth;

/// <summary>
/// Request model for resetting password
/// </summary>
public sealed record ResetPasswordRequest
{
    /// <summary>
    /// Password reset token received via email
    /// </summary>
    [Required(ErrorMessage = "Reset token is required")]
    public string Token { get; init; } = string.Empty;

    /// <summary>
    /// Email address of the user
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// New password
    /// </summary>
    [Required(ErrorMessage = "New password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]",
        ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter, one digit, and one special character")]
    public string NewPassword { get; init; } = string.Empty;

    /// <summary>
    /// Password confirmation
    /// </summary>
    [Required(ErrorMessage = "Password confirmation is required")]
    [Compare(nameof(NewPassword), ErrorMessage = "Password and confirmation do not match")]
    public string ConfirmPassword { get; init; } = string.Empty;
}