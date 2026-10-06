using System.Security.Cryptography;
using System.Text.RegularExpressions;
using BCrypt.Net;
using MosquitoApi.Application.Interfaces;

namespace MosquitoApi.Application.Services;

/// <summary>
/// Service for password operations using BCrypt
/// </summary>
public class PasswordService : IPasswordService
{
    private const int WorkFactor = 12; // BCrypt work factor (higher = more secure but slower)

    /// <summary>
    /// Hashes a password using BCrypt
    /// </summary>
    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    /// <summary>
    /// Verifies a password against its hash
    /// </summary>
    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates password strength
    /// </summary>
    public (bool IsValid, List<string> Errors) ValidatePasswordStrength(string password)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(password))
        {
            errors.Add("Password is required");
            return (false, errors);
        }

        // Check minimum length
        if (password.Length < 8)
            errors.Add("Password must be at least 8 characters long");

        // Check maximum length
        if (password.Length > 128)
            errors.Add("Password cannot exceed 128 characters");

        // Check for at least one lowercase letter
        if (!password.Any(char.IsLower))
            errors.Add("Password must contain at least one lowercase letter");

        // Check for at least one uppercase letter
        if (!password.Any(char.IsUpper))
            errors.Add("Password must contain at least one uppercase letter");

        // Check for at least one digit
        if (!password.Any(char.IsDigit))
            errors.Add("Password must contain at least one digit");

        // Check for at least one special character
        if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
            errors.Add("Password must contain at least one special character (!@#$%^&*()_+-=[]{}|;':\",./<>?)");

        // Check for common weak patterns
        if (ContainsCommonWeakPatterns(password))
            errors.Add("Password contains common weak patterns");

        // Check for sequential characters
        if (HasSequentialCharacters(password))
            errors.Add("Password should not contain sequential characters (e.g., 123, abc)");

        return (errors.Count == 0, errors);
    }

    /// <summary>
    /// Generates a secure random password
    /// </summary>
    public string GenerateSecurePassword(int length = 12)
    {
        if (length < 8)
            throw new ArgumentException("Password length must be at least 8 characters", nameof(length));

        const string lowercase = "abcdefghijklmnopqrstuvwxyz";
        const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string digits = "0123456789";
        const string specialChars = "!@#$%^&*()_+-=[]{}|;':\",./<>?";
        
        var allChars = lowercase + uppercase + digits + specialChars;
        
        using var rng = RandomNumberGenerator.Create();
        var password = new char[length];
        var charArray = allChars.ToCharArray();
        
        // Ensure at least one character from each category
        password[0] = lowercase[GetRandomIndex(rng, lowercase.Length)];
        password[1] = uppercase[GetRandomIndex(rng, uppercase.Length)];
        password[2] = digits[GetRandomIndex(rng, digits.Length)];
        password[3] = specialChars[GetRandomIndex(rng, specialChars.Length)];
        
        // Fill the rest randomly
        for (int i = 4; i < length; i++)
        {
            password[i] = charArray[GetRandomIndex(rng, charArray.Length)];
        }
        
        // Shuffle the password to randomize positions
        for (int i = 0; i < length; i++)
        {
            int randomIndex = GetRandomIndex(rng, length);
            (password[i], password[randomIndex]) = (password[randomIndex], password[i]);
        }
        
        return new string(password);
    }

    /// <summary>
    /// Generates a password reset token
    /// </summary>
    public string GeneratePasswordResetToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    /// <summary>
    /// Checks if a password meets complexity requirements
    /// </summary>
    public bool MeetsComplexityRequirements(string password)
    {
        var (isValid, _) = ValidatePasswordStrength(password);
        return isValid;
    }

    /// <summary>
    /// Gets a cryptographically secure random index
    /// </summary>
    private static int GetRandomIndex(RandomNumberGenerator rng, int maxValue)
    {
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        var value = BitConverter.ToUInt32(bytes, 0);
        return (int)(value % (uint)maxValue);
    }

    /// <summary>
    /// Checks for common weak password patterns
    /// </summary>
    private static bool ContainsCommonWeakPatterns(string password)
    {
        var weakPatterns = new[]
        {
            "password", "123456", "qwerty", "admin", "login",
            "welcome", "monkey", "letmein", "dragon", "master",
            "shadow", "football", "baseball", "sunshine"
        };

        var lowerPassword = password.ToLowerInvariant();
        return weakPatterns.Any(pattern => lowerPassword.Contains(pattern));
    }

    /// <summary>
    /// Checks for sequential characters
    /// </summary>
    private static bool HasSequentialCharacters(string password)
    {
        var sequential = new[]
        {
            "123", "234", "345", "456", "567", "678", "789",
            "abc", "bcd", "cde", "def", "efg", "fgh", "ghi",
            "hij", "ijk", "jkl", "klm", "lmn", "mno", "nop",
            "opq", "pqr", "qrs", "rst", "stu", "tuv", "uvw",
            "vwx", "wxy", "xyz"
        };

        var lowerPassword = password.ToLowerInvariant();
        return sequential.Any(seq => lowerPassword.Contains(seq) || lowerPassword.Contains(new string(seq.Reverse().ToArray())));
    }
}