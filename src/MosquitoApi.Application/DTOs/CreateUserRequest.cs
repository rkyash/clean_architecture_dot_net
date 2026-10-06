using System.ComponentModel.DataAnnotations;
using MosquitoApi.Domain.Enums;

namespace MosquitoApi.Application.DTOs;

/// <summary>
/// Request model for creating a new user
/// </summary>
public sealed record CreateUserRequest
{
    /// <summary>
    /// First name
    /// </summary>
    [Required(ErrorMessage = "First name is required")]
    [MaxLength(250, ErrorMessage = "First name cannot exceed 250 characters")]
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Last name
    /// </summary>
    [MaxLength(250, ErrorMessage = "Last name cannot exceed 250 characters")]
    public string? LastName { get; init; }

    /// <summary>
    /// Father's name
    /// </summary>
    [MaxLength(250, ErrorMessage = "Father name cannot exceed 250 characters")]
    public string? FatherName { get; init; }

    /// <summary>
    /// Email address (unique)
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(250, ErrorMessage = "Email cannot exceed 250 characters")]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Password
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]",
        ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter, one digit, and one special character")]
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Password confirmation
    /// </summary>
    [Required(ErrorMessage = "Password confirmation is required")]
    [Compare(nameof(Password), ErrorMessage = "Password and confirmation do not match")]
    public string ConfirmPassword { get; init; } = string.Empty;

    /// <summary>
    /// Mobile phone number
    /// </summary>
    [MaxLength(250, ErrorMessage = "Mobile cannot exceed 250 characters")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? Mobile { get; init; }

    /// <summary>
    /// Date of birth as Unix timestamp
    /// </summary>
    public long? Dob { get; init; }

    /// <summary>
    /// Gender
    /// </summary>
    public GenderType Gender { get; init; } = GenderType.PreferNotToSay;

    /// <summary>
    /// Role type (only admins can set this)
    /// </summary>
    public RoleType RoleType { get; init; } = RoleType.User;

    /// <summary>
    /// Profile photo URL
    /// </summary>
    [Url(ErrorMessage = "Invalid URL format for photo")]
    public string? PhotoUrl { get; init; }
}