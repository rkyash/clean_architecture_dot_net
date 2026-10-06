using System.ComponentModel.DataAnnotations;

namespace MosquitoApi.Domain.Entities;

/// <summary>
/// Refresh token entity for JWT authentication
/// </summary>
public class Token : BaseEntity
{
    /// <summary>
    /// The actual refresh token value
    /// </summary>
    [MaxLength(500)]
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration date and time
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Whether the token is currently active/valid
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Date and time when token was revoked (if applicable)
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Reason for token revocation
    /// </summary>
    [MaxLength(500)]
    public string? RevokedReason { get; set; }

    /// <summary>
    /// IP address from which the token was created
    /// </summary>
    [MaxLength(45)]
    public string? CreatedByIp { get; set; }

    /// <summary>
    /// IP address from which the token was revoked
    /// </summary>
    [MaxLength(45)]
    public string? RevokedByIp { get; set; }

    /// <summary>
    /// Token that replaced this token (for token rotation)
    /// </summary>
    [MaxLength(500)]
    public string? ReplacedByToken { get; set; }

    /// <summary>
    /// User agent string from the client that created the token
    /// </summary>
    [MaxLength(1000)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// ID of the user this token belongs to
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property to the user
    /// </summary>
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Checks if the token is currently valid and not expired
    /// </summary>
    public bool IsValid => IsActive && !IsExpired && !IsRevoked;

    /// <summary>
    /// Checks if the token has expired
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    /// <summary>
    /// Checks if the token has been revoked
    /// </summary>
    public bool IsRevoked => RevokedAt != null;

    /// <summary>
    /// Revokes the token with specified reason and IP
    /// </summary>
    public void Revoke(string reason, string? ipAddress = null)
    {
        RevokedAt = DateTime.UtcNow;
        RevokedReason = reason;
        RevokedByIp = ipAddress;
        IsActive = false;
    }

    /// <summary>
    /// Creates a new refresh token
    /// </summary>
    public static Token CreateRefreshToken(Guid userId, string? ipAddress = null, string? userAgent = null)
    {
        var randomBytes = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        return new Token
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RefreshToken = Convert.ToBase64String(randomBytes),
            ExpiresAt = DateTime.UtcNow.AddDays(7), // 7 days expiration
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress,
            UserAgent = userAgent
        };
    }

    /// <summary>
    /// Validates the token and returns any validation errors
    /// </summary>
    public List<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(RefreshToken))
            errors.Add("Refresh token value is required");

        if (UserId == Guid.Empty)
            errors.Add("User ID is required");

        if (ExpiresAt <= DateTime.UtcNow)
            errors.Add("Token expiration must be in the future");

        return errors;
    }
}