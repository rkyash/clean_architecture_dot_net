using System.ComponentModel.DataAnnotations;
using MosquitoApi.Domain.Enums;

namespace MosquitoApi.Domain.Entities;

/// <summary>
/// User entity representing system users
/// </summary>
public class User : BaseEntity
{
    public User()
    {
        IsActive = true;
        Tokens = new HashSet<Token>();
        FailedPasswordAttemptCount = 0;
        RoleType = RoleType.User;
        GenderType = GenderType.PreferNotToSay;
    }

    /// <summary>
    /// First name of the user
    /// </summary>
    [MaxLength(250)]
    public string? FirstName { get; set; }

    /// <summary>
    /// Last name of the user
    /// </summary>
    [MaxLength(250)]
    public string? LastName { get; set; }

    /// <summary>
    /// Father's name
    /// </summary>
    [MaxLength(250)]
    public string? FatherName { get; set; }

    /// <summary>
    /// ID of the user who created this user
    /// </summary>
    [MaxLength(250)]
    public string? CreatedById { get; set; }

    /// <summary>
    /// Hashed password
    /// </summary>
    [MaxLength(250)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Date of birth as Unix timestamp
    /// </summary>
    public long? Dob { get; set; }

    /// <summary>
    /// Mobile phone number
    /// </summary>
    [MaxLength(250)]
    public string? Mobile { get; set; }

    /// <summary>
    /// Email address (unique identifier)
    /// </summary>
    [MaxLength(250)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// URL to user's profile photo
    /// </summary>
    public string? PhotoUrl { get; set; }

    /// <summary>
    /// User's role type
    /// </summary>
    public RoleType RoleType { get; set; }

    /// <summary>
    /// Number of failed password attempts
    /// </summary>
    public int FailedPasswordAttemptCount { get; set; }

    /// <summary>
    /// Whether the user account is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Password reset token ID
    /// </summary>
    [MaxLength(250)]
    public string? ResetId { get; set; }

    /// <summary>
    /// Password reset token expiration date
    /// </summary>
    public DateTime? ResetIdExpiration { get; set; }

    /// <summary>
    /// Username (unique)
    /// </summary>
    [MaxLength(250)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Date of birth
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// User's gender
    /// </summary>
    public GenderType GenderType { get; set; }

    /// <summary>
    /// Last login timestamp
    /// </summary>
    public DateTime? LastLoginDateUtc { get; set; }

    /// <summary>
    /// Last password change timestamp
    /// </summary>
    public DateTime? LastPasswordChangeDateUtc { get; set; }

    /// <summary>
    /// Email verification status
    /// </summary>
    public bool IsEmailVerified { get; set; }

    /// <summary>
    /// Email verification token
    /// </summary>
    [MaxLength(250)]
    public string? EmailVerificationToken { get; set; }

    /// <summary>
    /// Account lockout end time
    /// </summary>
    public DateTime? LockoutEndDateUtc { get; set; }

    /// <summary>
    /// Collection of refresh tokens for this user
    /// </summary>
    public virtual ICollection<Token> Tokens { get; set; }

    /// <summary>
    /// Full name computed property
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Checks if the account is currently locked out
    /// </summary>
    public bool IsLockedOut => LockoutEndDateUtc.HasValue && LockoutEndDateUtc.Value > DateTime.UtcNow;

    /// <summary>
    /// Validates user data according to business rules
    /// </summary>
    public List<string> Validate()
    {
        var errors = new List<string>();

        // Email validation
        if (string.IsNullOrWhiteSpace(Email))
            errors.Add("Email is required");
        else if (!IsValidEmail(Email))
            errors.Add("Email format is invalid");

        // Password validation (only for new users or password changes)
        if (string.IsNullOrWhiteSpace(Password))
            errors.Add("Password is required");

        // Name validation
        if (string.IsNullOrWhiteSpace(FirstName))
            errors.Add("First name is required");

        if (!string.IsNullOrEmpty(FirstName) && FirstName.Length > 250)
            errors.Add("First name cannot exceed 250 characters");

        if (!string.IsNullOrEmpty(LastName) && LastName.Length > 250)
            errors.Add("Last name cannot exceed 250 characters");

        // Mobile validation
        if (!string.IsNullOrEmpty(Mobile) && Mobile.Length > 250)
            errors.Add("Mobile number cannot exceed 250 characters");

        return errors;
    }

    /// <summary>
    /// Increments failed password attempts and locks account if necessary
    /// </summary>
    public void IncrementFailedPasswordAttempts()
    {
        FailedPasswordAttemptCount++;
        
        // Lock account after 5 failed attempts for 30 minutes
        if (FailedPasswordAttemptCount >= 5)
        {
            LockoutEndDateUtc = DateTime.UtcNow.AddMinutes(30);
        }
    }

    /// <summary>
    /// Resets failed password attempts (called on successful login)
    /// </summary>
    public void ResetFailedPasswordAttempts()
    {
        FailedPasswordAttemptCount = 0;
        LockoutEndDateUtc = null;
        LastLoginDateUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Generates a password reset token
    /// </summary>
    public void GeneratePasswordResetToken()
    {
        ResetId = Guid.NewGuid().ToString();
        ResetIdExpiration = DateTime.UtcNow.AddHours(1);
    }

    /// <summary>
    /// Checks if password reset token is valid and not expired
    /// </summary>
    public bool IsPasswordResetTokenValid(string token)
    {
        if (string.IsNullOrWhiteSpace(ResetId) || ResetId != token)
            return false;

        if (!ResetIdExpiration.HasValue)
            return false;

        return DateTime.UtcNow < ResetIdExpiration.Value;
    }

    /// <summary>
    /// Clears password reset token
    /// </summary>
    public void ClearPasswordResetToken()
    {
        ResetId = null;
        ResetIdExpiration = null;
    }

    /// <summary>
    /// Validates email format
    /// </summary>
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}