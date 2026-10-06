namespace MosquitoApi.Application.DTOs.Auth;

/// <summary>
/// Response model for account status
/// </summary>
public class AccountStatusResponse
{
    /// <summary>
    /// Email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Whether the account is locked
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    /// When the lockout expires (if locked)
    /// </summary>
    public DateTime? LockoutExpiresAt { get; set; }

    /// <summary>
    /// Whether the email is verified
    /// </summary>
    public bool IsEmailVerified { get; set; }

    /// <summary>
    /// Whether the account is active
    /// </summary>
    public bool IsActive { get; set; }
}