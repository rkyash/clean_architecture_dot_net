using MosquitoApi.Domain.Enums;

namespace MosquitoApi.Application.DTOs.User;

/// <summary>
/// Request model for searching users with filtering and pagination
/// </summary>
public class UserSearchRequest
{
    /// <summary>
    /// Search term to match against username, email, first name, or last name
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Filter by user role
    /// </summary>
    public RoleType? RoleType { get; set; }

    /// <summary>
    /// Filter by gender
    /// </summary>
    public GenderType? GenderType { get; set; }

    /// <summary>
    /// Filter by active status
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Filter by email verification status
    /// </summary>
    public bool? IsEmailVerified { get; set; }

    /// <summary>
    /// Filter by locked status
    /// </summary>
    public bool? IsLocked { get; set; }

    /// <summary>
    /// Filter users created after this date
    /// </summary>
    public DateTime? CreatedAfter { get; set; }

    /// <summary>
    /// Filter users created before this date
    /// </summary>
    public DateTime? CreatedBefore { get; set; }

    /// <summary>
    /// Filter users who logged in after this date
    /// </summary>
    public DateTime? LastLoginAfter { get; set; }

    /// <summary>
    /// Filter users who logged in before this date
    /// </summary>
    public DateTime? LastLoginBefore { get; set; }

    /// <summary>
    /// Page number for pagination (starts from 1)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Sort field (username, email, createdAt, lastLogin, etc.)
    /// </summary>
    public string SortBy { get; set; } = "createdAt";

    /// <summary>
    /// Sort direction (asc or desc)
    /// </summary>
    public string SortDirection { get; set; } = "desc";
}