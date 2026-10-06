using MosquitoApi.Domain.Enums;

namespace MosquitoApi.Application.DTOs.User;

/// <summary>
/// User data transfer object for API responses
/// </summary>
public class UserDto
{
    /// <summary>
    /// Unique identifier for the user
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// First name of the user
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Last name of the user
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Full name (computed from first and last name)
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Father's name
    /// </summary>
    public string? FatherName { get; set; }

    /// <summary>
    /// Username
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Mobile phone number
    /// </summary>
    public string? Mobile { get; set; }

    /// <summary>
    /// Date of birth
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// User's gender
    /// </summary>
    public GenderType GenderType { get; set; }

    /// <summary>
    /// User's role in the system
    /// </summary>
    public RoleType RoleType { get; set; }

    /// <summary>
    /// Profile photo URL
    /// </summary>
    public string? PhotoUrl { get; set; }

    /// <summary>
    /// Whether the user account is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Whether the email is verified
    /// </summary>
    public bool IsEmailVerified { get; set; }

    /// <summary>
    /// Last login timestamp
    /// </summary>
    public DateTime? LastLoginDateUtc { get; set; }

    /// <summary>
    /// Account creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Who created this user
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Who last updated this user
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Number of failed login attempts
    /// </summary>
    public int FailedPasswordAttemptCount { get; set; }

    /// <summary>
    /// Whether the account is currently locked
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    /// When the account lockout ends (if locked)
    /// </summary>
    public DateTime? LockoutEndDateUtc { get; set; }
}