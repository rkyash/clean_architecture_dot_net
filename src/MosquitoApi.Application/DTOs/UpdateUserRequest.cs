using System.ComponentModel.DataAnnotations;
using MosquitoApi.Domain.Enums;

namespace MosquitoApi.Application.DTOs;

/// <summary>
/// Request model for updating user information
/// </summary>
public sealed record UpdateUserRequest
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
    public GenderType Gender { get; init; }

    /// <summary>
    /// Profile photo URL
    /// </summary>
    [Url(ErrorMessage = "Invalid URL format for photo")]
    public string? PhotoUrl { get; init; }
}