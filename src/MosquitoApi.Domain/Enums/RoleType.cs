namespace MosquitoApi.Domain.Enums;

/// <summary>
/// Enumeration of user role types
/// </summary>
public enum RoleType
{
    /// <summary>
    /// Regular user with basic permissions
    /// </summary>
    User = 1,

    /// <summary>
    /// Moderator with intermediate permissions
    /// </summary>
    Moderator = 2,

    /// <summary>
    /// Administrator with full permissions
    /// </summary>
    Admin = 3,

    /// <summary>
    /// Super administrator with system-level permissions
    /// </summary>
    SuperAdmin = 4
}