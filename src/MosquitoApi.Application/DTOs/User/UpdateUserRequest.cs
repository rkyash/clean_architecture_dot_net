using System.ComponentModel.DataAnnotations;
using MosquitoApi.Domain.Enums;

namespace MosquitoApi.Application.DTOs.User;

/// <summary>
/// Request model for updating an existing user
/// </summary>
public class UpdateUserRequest
{
    /// <summary>
    /// First name of the user
    /// </summary>
    [StringLength(250, ErrorMessage = "First name cannot exceed 250 characters")]
    public string? FirstName { get; set; }

    /// <summary>
    /// Last name of the user
    /// </summary>
    [StringLength(250, ErrorMessage = "Last name cannot exceed 250 characters")]
    public string? LastName { get; set; }

    /// <summary>
    /// Father's name (optional)
    /// </summary>
    [StringLength(250, ErrorMessage = "Father name cannot exceed 250 characters")]
    public string? FatherName { get; set; }

    /// <summary>
    /// Username (must be unique)
    /// </summary>
    [StringLength(250, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 250 characters")]
    public string? Username { get; set; }

    /// <summary>
    /// Email address (must be unique)
    /// </summary>
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(250, ErrorMessage = "Email cannot exceed 250 characters")]
    public string? Email { get; set; }

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
    public GenderType? GenderType { get; set; }

    /// <summary>
    /// User's role in the system
    /// </summary>
    public RoleType? RoleType { get; set; }

    /// <summary>
    /// Profile photo URL (optional)
    /// </summary>
    [Url(ErrorMessage = "Invalid URL format")]
    public string? PhotoUrl { get; set; }

    /// <summary>
    /// Whether the user account is active
    /// </summary>
    public bool? IsActive { get; set; }
}