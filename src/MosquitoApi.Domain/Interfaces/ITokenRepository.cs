using MosquitoApi.Domain.Entities;

namespace MosquitoApi.Domain.Interfaces;

/// <summary>
/// Repository interface for Token entity operations
/// </summary>
public interface ITokenRepository
{
    /// <summary>
    /// Gets a token by its value
    /// </summary>
    Task<Token?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all tokens for a user
    /// </summary>
    Task<IEnumerable<Token>> GetUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active tokens for a user
    /// </summary>
    Task<IEnumerable<Token>> GetActiveUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new token
    /// </summary>
    Task<Token> AddAsync(Token token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing token
    /// </summary>
    void Update(Token token);

    /// <summary>
    /// Removes a token
    /// </summary>
    void Delete(Token token);

    /// <summary>
    /// Revokes all active tokens for a user
    /// </summary>
    Task RevokeAllUserTokensAsync(Guid userId, string reason, string? ipAddress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes expired tokens
    /// </summary>
    Task RemoveExpiredTokensAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tokens that are about to expire (for cleanup or notification)
    /// </summary>
    Task<IEnumerable<Token>> GetExpiringTokensAsync(DateTime expirationThreshold, CancellationToken cancellationToken = default);
}