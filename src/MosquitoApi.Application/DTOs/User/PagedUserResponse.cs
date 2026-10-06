namespace MosquitoApi.Application.DTOs.User;

/// <summary>
/// Paginated response containing users and pagination metadata
/// </summary>
public class PagedUserResponse
{
    /// <summary>
    /// List of users for the current page
    /// </summary>
    public List<UserDto> Users { get; set; } = new();

    /// <summary>
    /// Current page number
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of users matching the search criteria
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPrevious => CurrentPage > 1;

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNext => CurrentPage < TotalPages;

    /// <summary>
    /// Summary statistics
    /// </summary>
    public UserSearchSummary Summary { get; set; } = new();
}

/// <summary>
/// Summary statistics for user search results
/// </summary>
public class UserSearchSummary
{
    /// <summary>
    /// Number of active users
    /// </summary>
    public int ActiveUsers { get; set; }

    /// <summary>
    /// Number of inactive users
    /// </summary>
    public int InactiveUsers { get; set; }

    /// <summary>
    /// Number of email verified users
    /// </summary>
    public int EmailVerifiedUsers { get; set; }

    /// <summary>
    /// Number of locked users
    /// </summary>
    public int LockedUsers { get; set; }

    /// <summary>
    /// Users by role distribution
    /// </summary>
    public Dictionary<string, int> UsersByRole { get; set; } = new();
}