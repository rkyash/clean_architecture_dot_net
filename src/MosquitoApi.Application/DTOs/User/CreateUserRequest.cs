using System.ComponentModel.DataAnnotations;
using MosquitoApi.Domain.Enums;

namespace MosquitoApi.Application.DTOs.User;

/// <summary>
/// Request model for creating a new user
/// </summary>
public class CreateUserRequest
{
    /// <summary>
    /// First name of the user
    /// </summary>
    [Required(ErrorMessage = "First name is required")]
    [StringLength(250, ErrorMessage = "First name cannot exceed 250 characters")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Last name of the user
    /// </summary>
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(250, ErrorMessage = "Last name cannot exceed 250 characters")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Father's name (optional)
    /// </summary>
    [StringLength(250, ErrorMessage = "Father name cannot exceed 250 characters")]
    public string? FatherName { get; set; }

    /// <summary>
    /// Username (must be unique)
    /// </summary>
    [Required(ErrorMessage = "Username is required")]
    [StringLength(250, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 250 characters")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Email address (must be unique)
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(250, ErrorMessage = "Email cannot exceed 250 characters")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Password (will be hashed)
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Mobile phone number (optional)
    /// </summary>
    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(250, ErrorMessage = "Mobile number cannot exceed 250 characters")]
    public string? Mobile { get; set; }

    /// <summary>
    /// Date of birth (optional)
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// User's gender
    /// </summary>
    public GenderType GenderType { get; set; } = GenderType.PreferNotToSay;

    /// <summary>
    /// User's role in the system
    /// </summary>
    public RoleType RoleType { get; set; } = RoleType.User;

    /// <summary>
    /// Profile photo URL (optional)
    /// </summary>
    [Url(ErrorMessage = "Invalid URL format")]
    public string? PhotoUrl { get; set; }

    /// <summary>
    /// Whether the user account should be active upon creation
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether to send email verification upon creation
    /// </summary>
    public bool SendEmailVerification { get; set; } = true;
}