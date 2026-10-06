using Microsoft.EntityFrameworkCore;
using MosquitoApi.Domain.Entities;
using MosquitoApi.Domain.Interfaces;
using MosquitoApi.Infrastructure.Data;

namespace MosquitoApi.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Token entity
/// </summary>
public class TokenRepository : ITokenRepository
{
    private readonly MosquitoDbContext _context;

    public TokenRepository(MosquitoDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets token by refresh token string
    /// </summary>
    public async Task<Token?> GetByTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return await _context.Tokens
            .FirstOrDefaultAsync(t => t.RefreshToken == refreshToken, cancellationToken);
    }

    /// <summary>
    /// Gets all tokens for a user
    /// </summary>
    public async Task<IEnumerable<Token>> GetUserTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Tokens
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets active tokens for a user
    /// </summary>
    public async Task<IEnumerable<Token>> GetActiveUserTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Tokens
            .Where(t => t.UserId == userId && t.IsActive && !t.IsExpired && !t.IsRevoked)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets expired tokens
    /// </summary>
    public async Task<IEnumerable<Token>> GetExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Tokens
            .Where(t => t.ExpiresAt <= now)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets tokens expiring within specified timespan
    /// </summary>
    public async Task<IEnumerable<Token>> GetTokensExpiringWithinAsync(TimeSpan timeSpan, CancellationToken cancellationToken = default)
    {
        var expiryThreshold = DateTime.UtcNow.Add(timeSpan);
        return await _context.Tokens
            .Where(t => t.IsActive && !t.IsRevoked && t.ExpiresAt <= expiryThreshold)
            .OrderBy(t => t.ExpiresAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets tokens that are about to expire (for cleanup or notification)
    /// </summary>
    public async Task<IEnumerable<Token>> GetExpiringTokensAsync(DateTime expirationThreshold, CancellationToken cancellationToken = default)
    {
        return await _context.Tokens
            .Where(t => t.IsActive && !t.IsRevoked && t.ExpiresAt <= expirationThreshold)
            .OrderBy(t => t.ExpiresAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets revoked tokens
    /// </summary>
    public async Task<IEnumerable<Token>> GetRevokedTokensAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tokens
            .Where(t => t.IsRevoked)
            .OrderByDescending(t => t.RevokedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets tokens by IP address
    /// </summary>
    public async Task<IEnumerable<Token>> GetTokensByIpAddressAsync(string ipAddress, CancellationToken cancellationToken = default)
    {
        return await _context.Tokens
            .Where(t => t.CreatedByIp == ipAddress)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets tokens created within date range
    /// </summary>
    public async Task<IEnumerable<Token>> GetTokensByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.Tokens
            .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets count of active tokens for user
    /// </summary>
    public async Task<int> GetActiveTokenCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Tokens
            .CountAsync(t => t.UserId == userId && t.IsActive && !t.IsExpired && !t.IsRevoked, cancellationToken);
    }

    /// <summary>
    /// Validates if token exists and is valid
    /// </summary>
    public async Task<bool> IsTokenValidAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var token = await GetByTokenAsync(refreshToken, cancellationToken);
        return token?.IsValid ?? false;
    }

    /// <summary>
    /// Revokes all tokens for a user
    /// </summary>
    public async Task RevokeAllUserTokensAsync(Guid userId, string reason, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var tokens = await _context.Tokens
            .Where(t => t.UserId == userId && t.IsActive && !t.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.Revoke(reason, ipAddress);
        }

        _context.Tokens.UpdateRange(tokens);
    }

    /// <summary>
    /// Revokes tokens by criteria
    /// </summary>
    public async Task RevokeTokensByCriteriaAsync(
        Guid? userId = null,
        string? ipAddress = null,
        DateTime? createdBefore = null,
        string reason = "Batch revocation",
        string? revokedByIp = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Tokens.Where(t => t.IsActive && !t.IsRevoked);

        if (userId.HasValue)
            query = query.Where(t => t.UserId == userId.Value);

        if (!string.IsNullOrEmpty(ipAddress))
            query = query.Where(t => t.CreatedByIp == ipAddress);

        if (createdBefore.HasValue)
            query = query.Where(t => t.CreatedAt < createdBefore.Value);

        var tokens = await query.ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.Revoke(reason, revokedByIp);
        }

        _context.Tokens.UpdateRange(tokens);
    }

    /// <summary>
    /// Removes expired tokens from database
    /// </summary>
    public async Task RemoveExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        var expiredTokens = await GetExpiredTokensAsync(cancellationToken);
        _context.Tokens.RemoveRange(expiredTokens);
    }

    /// <summary>
    /// Removes tokens older than specified days
    /// </summary>
    public async Task RemoveTokensOlderThanAsync(int days, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        var oldTokens = await _context.Tokens
            .Where(t => t.CreatedAt < cutoffDate)
            .ToListAsync(cancellationToken);

        _context.Tokens.RemoveRange(oldTokens);
    }

    /// <summary>
    /// Gets token statistics
    /// </summary>
    public async Task<TokenStatistics> GetTokenStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var totalTokens = await _context.Tokens.CountAsync(cancellationToken);
        var activeTokens = await _context.Tokens.CountAsync(t => t.IsActive && !t.IsExpired && !t.IsRevoked, cancellationToken);
        var expiredTokens = await _context.Tokens.CountAsync(t => t.ExpiresAt <= now, cancellationToken);
        var revokedTokens = await _context.Tokens.CountAsync(t => t.IsRevoked, cancellationToken);

        return new TokenStatistics
        {
            TotalTokens = totalTokens,
            ActiveTokens = activeTokens,
            ExpiredTokens = expiredTokens,
            RevokedTokens = revokedTokens,
            GeneratedToday = await _context.Tokens.CountAsync(t => t.CreatedAt.Date == now.Date, cancellationToken)
        };
    }

    /// <summary>
    /// Adds new token
    /// </summary>
    public async Task<Token> AddAsync(Token token, CancellationToken cancellationToken = default)
    {
        await _context.Tokens.AddAsync(token, cancellationToken);
        return token;
    }

    /// <summary>
    /// Updates existing token
    /// </summary>
    public void Update(Token token)
    {
        _context.Tokens.Update(token);
    }

    /// <summary>
    /// Deletes token
    /// </summary>
    public void Delete(Token token)
    {
        _context.Tokens.Remove(token);
    }

    /// <summary>
    /// Deletes tokens by criteria
    /// </summary>
    public async Task DeleteByCriteriaAsync(
        Guid? userId = null,
        bool? isExpired = null,
        bool? isRevoked = null,
        DateTime? createdBefore = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Tokens.AsQueryable();

        if (userId.HasValue)
            query = query.Where(t => t.UserId == userId.Value);

        if (isExpired.HasValue)
        {
            var now = DateTime.UtcNow;
            if (isExpired.Value)
                query = query.Where(t => t.ExpiresAt <= now);
            else
                query = query.Where(t => t.ExpiresAt > now);
        }

        if (isRevoked.HasValue)
            query = query.Where(t => t.IsRevoked == isRevoked.Value);

        if (createdBefore.HasValue)
            query = query.Where(t => t.CreatedAt < createdBefore.Value);

        var tokens = await query.ToListAsync(cancellationToken);
        _context.Tokens.RemoveRange(tokens);
    }
}

/// <summary>
/// Token statistics data transfer object
/// </summary>
public class TokenStatistics
{
    public int TotalTokens { get; set; }
    public int ActiveTokens { get; set; }
    public int ExpiredTokens { get; set; }
    public int RevokedTokens { get; set; }
    public int GeneratedToday { get; set; }
}