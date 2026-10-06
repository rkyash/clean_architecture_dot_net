using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MosquitoApi.Application.Configuration;
using MosquitoApi.Application.Interfaces;
using MosquitoApi.Domain.Entities;
using MosquitoApi.Domain.Interfaces;

namespace MosquitoApi.Application.Services;

/// <summary>
/// Service for JWT token operations
/// </summary>
public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly ITokenRepository _tokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TokenValidationParameters _tokenValidationParameters;

    public TokenService(
        IOptions<JwtSettings> jwtSettings,
        ITokenRepository tokenRepository,
        IUnitOfWork unitOfWork)
    {
        _jwtSettings = jwtSettings.Value;
        _tokenRepository = tokenRepository;
        _unitOfWork = unitOfWork;
        
        _tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = _jwtSettings.ValidateIssuerSigningKey,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
            ValidateIssuer = _jwtSettings.ValidateIssuer,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateAudience = _jwtSettings.ValidateAudience,
            ValidAudience = _jwtSettings.Audience,
            ValidateLifetime = _jwtSettings.ValidateLifetime,
            ClockSkew = TimeSpan.FromMinutes(_jwtSettings.ClockSkewMinutes),
            RequireExpirationTime = true
        };
    }

    /// <summary>
    /// Generates a JWT access token for a user
    /// </summary>
    public string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.RoleType.ToString()),
            new("user_id", user.Id.ToString()),
            new("email", user.Email),
            new("role", user.RoleType.ToString()),
            new("is_active", user.IsActive.ToString()),
            new("email_verified", user.IsEmailVerified.ToString())
        };

        // Add additional claims if available
        if (!string.IsNullOrEmpty(user.FirstName))
            claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));

        if (!string.IsNullOrEmpty(user.LastName))
            claims.Add(new Claim(ClaimTypes.Surname, user.LastName));

        if (!string.IsNullOrEmpty(user.Mobile))
            claims.Add(new Claim(ClaimTypes.MobilePhone, user.Mobile));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generates a refresh token for a user
    /// </summary>
    public Token GenerateRefreshToken(User user, string? ipAddress = null, string? userAgent = null)
    {
        return Token.CreateRefreshToken(user.Id, ipAddress, userAgent);
    }

    /// <summary>
    /// Validates a JWT token and returns the claims principal
    /// </summary>
    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Extracts user ID from JWT token
    /// </summary>
    public Guid? GetUserIdFromToken(string token)
    {
        var principal = ValidateToken(token);
        var userIdClaim = principal?.FindFirst("user_id")?.Value ?? principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;
            
        return null;
    }

    /// <summary>
    /// Extracts claims from JWT token
    /// </summary>
    public IEnumerable<Claim> GetClaimsFromToken(string token)
    {
        var principal = ValidateToken(token);
        return principal?.Claims ?? Enumerable.Empty<Claim>();
    }

    /// <summary>
    /// Checks if a JWT token has expired
    /// </summary>
    public bool IsTokenExpired(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.ReadJwtToken(token);
            return jwt.ValidTo <= DateTime.UtcNow;
        }
        catch
        {
            return true;
        }
    }

    /// <summary>
    /// Gets the remaining time before token expires
    /// </summary>
    public TimeSpan GetTokenRemainingLifetime(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.ReadJwtToken(token);
            var remaining = jwt.ValidTo - DateTime.UtcNow;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }
        catch
        {
            return TimeSpan.Zero;
        }
    }

    /// <summary>
    /// Revokes a refresh token
    /// </summary>
    public async Task RevokeRefreshTokenAsync(string token, string reason, string? ipAddress = null)
    {
        var refreshToken = await _tokenRepository.GetByTokenAsync(token);
        if (refreshToken != null)
        {
            refreshToken.Revoke(reason, ipAddress);
            _tokenRepository.Update(refreshToken);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Rotates a refresh token (creates new one and revokes old one)
    /// </summary>
    public async Task<Token> RotateRefreshTokenAsync(Token token, string? ipAddress = null, string? userAgent = null)
    {
        // Create new refresh token
        var newToken = Token.CreateRefreshToken(token.UserId, ipAddress, userAgent);
        
        // Revoke old token
        token.Revoke("Replaced by new token", ipAddress);
        token.ReplacedByToken = newToken.RefreshToken;
        
        // Update old token and add new token
        _tokenRepository.Update(token);
        await _tokenRepository.AddAsync(newToken);
        await _unitOfWork.SaveChangesAsync();
        
        return newToken;
    }

    /// <summary>
    /// Validates a refresh token
    /// </summary>
    public async Task<Token?> ValidateRefreshTokenAsync(string token)
    {
        var refreshToken = await _tokenRepository.GetByTokenAsync(token);
        
        if (refreshToken == null || !refreshToken.IsValid)
            return null;
            
        return refreshToken;
    }

    /// <summary>
    /// Cleans up expired tokens
    /// </summary>
    public async Task CleanupExpiredTokensAsync()
    {
        await _tokenRepository.RemoveExpiredTokensAsync();
        await _unitOfWork.SaveChangesAsync();
    }
}