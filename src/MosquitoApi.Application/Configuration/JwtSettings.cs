namespace MosquitoApi.Application.Configuration;

/// <summary>
/// JWT configuration settings
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// Secret key for signing tokens
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Token issuer (usually your application name or domain)
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Token audience (usually your application name or domain)
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Access token expiration time in minutes
    /// </summary>
    public int AccessTokenExpirationMinutes { get; set; } = 15;

    /// <summary>
    /// Refresh token expiration time in days
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;

    /// <summary>
    /// Whether to validate token issuer
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;

    /// <summary>
    /// Whether to validate token audience
    /// </summary>
    public bool ValidateAudience { get; set; } = true;

    /// <summary>
    /// Whether to validate token lifetime
    /// </summary>
    public bool ValidateLifetime { get; set; } = true;

    /// <summary>
    /// Whether to validate issuer signing key
    /// </summary>
    public bool ValidateIssuerSigningKey { get; set; } = true;

    /// <summary>
    /// Clock skew tolerance in minutes
    /// </summary>
    public int ClockSkewMinutes { get; set; } = 5;

    /// <summary>
    /// Whether to require HTTPS for tokens
    /// </summary>
    public bool RequireHttps { get; set; } = true;

    /// <summary>
    /// Whether to save tokens in authentication properties
    /// </summary>
    public bool SaveToken { get; set; } = true;
}