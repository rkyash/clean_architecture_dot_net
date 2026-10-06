namespace MosquitoApi.Domain.Interfaces;

/// <summary>
/// Unit of Work pattern interface for managing transactions across multiple repositories
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Repository for mosquito data operations
    /// </summary>
    IMosquitoRepository MosquitoRepository { get; }

    /// <summary>
    /// Repository for audit log operations
    /// </summary>
    IAuditRepository AuditRepository { get; }

    /// <summary>
    /// Saves all changes made in the current unit of work to the database
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of entities affected</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new database transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transaction that can be committed or rolled back</returns>
    Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes multiple operations within a single transaction
    /// </summary>
    /// <param name="operation">Function to execute within transaction</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the operation</returns>
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes multiple operations within a single transaction without return value
    /// </summary>
    /// <param name="operation">Action to execute within transaction</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for database transaction management
/// </summary>
public interface IUnitOfWorkTransaction : IDisposable
{
    /// <summary>
    /// Commits the transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}