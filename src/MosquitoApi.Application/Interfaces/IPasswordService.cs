namespace MosquitoApi.Application.Interfaces;

/// <summary>
/// Service interface for password operations
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// Hashes a password using BCrypt
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a password against its hash
    /// </summary>
    bool VerifyPassword(string password, string hashedPassword);

    /// <summary>
    /// Validates password strength
    /// </summary>
    (bool IsValid, List<string> Errors) ValidatePasswordStrength(string password);

    /// <summary>
    /// Generates a secure random password
    /// </summary>
    string GenerateSecurePassword(int length = 12);

    /// <summary>
    /// Generates a password reset token
    /// </summary>
    string GeneratePasswordResetToken();

    /// <summary>
    /// Checks if a password meets complexity requirements
    /// </summary>
    bool MeetsComplexityRequirements(string password);
}