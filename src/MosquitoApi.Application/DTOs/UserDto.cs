using MosquitoApi.Domain.Enums;

namespace MosquitoApi.Application.DTOs;

/// <summary>
/// Data transfer object for user information
/// </summary>
public sealed record UserDto
{
    /// <summary>
    /// User unique identifier
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// First name
    /// </summary>
    public string? FirstName { get; init; }

    /// <summary>
    /// Last name
    /// </summary>
    public string? LastName { get; init; }

    /// <summary>
    /// Father's name
    /// </summary>
    public string? FatherName { get; init; }

    /// <summary>
    /// Email address
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Mobile phone number
    /// </summary>
    public string? Mobile { get; init; }

    /// <summary>
    /// Date of birth as Unix timestamp
    /// </summary>
    public long? Dob { get; init; }

    /// <summary>
    /// Profile photo URL
    /// </summary>
    public string? PhotoUrl { get; init; }

    /// <summary>
    /// User role type
    /// </summary>
    public RoleType RoleType { get; init; }

    /// <summary>
    /// Gender type
    /// </summary>
    public GenderType Gender { get; init; }

    /// <summary>
    /// Whether the account is active
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Whether email is verified
    /// </summary>
    public bool IsEmailVerified { get; init; }

    /// <summary>
    /// Last login timestamp
    /// </summary>
    public DateTime? LastLoginAt { get; init; }

    /// <summary>
    /// Account creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; init; }

    /// <summary>
    /// Full name (computed)
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Role display name
    /// </summary>
    public string RoleDisplayName => RoleType switch
    {
        RoleType.User => "User",
        RoleType.Moderator => "Moderator",
        RoleType.Admin => "Administrator",
        RoleType.SuperAdmin => "Super Administrator",
        _ => "Unknown"
    };
}