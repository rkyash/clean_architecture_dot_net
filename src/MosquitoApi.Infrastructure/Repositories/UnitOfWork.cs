using Microsoft.EntityFrameworkCore.Storage;
using MosquitoApi.Domain.Interfaces;
using MosquitoApi.Infrastructure.Data;

namespace MosquitoApi.Infrastructure.Repositories;

/// <summary>
/// Implementation of Unit of Work pattern with dual database support
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly MosquitoDbContext _mosquitoContext;
    private readonly AuditDbContext _auditContext;
    private readonly Lazy<IMosquitoRepository> _mosquitoRepository;
    private readonly Lazy<IAuditRepository> _auditRepository;
    private bool _disposed;

    public UnitOfWork(
        MosquitoDbContext mosquitoContext,
        AuditDbContext auditContext)
    {
        _mosquitoContext = mosquitoContext;
        _auditContext = auditContext;

        // Use lazy initialization for repositories
        _mosquitoRepository = new Lazy<IMosquitoRepository>(() => new MosquitoRepository(_mosquitoContext));
        _auditRepository = new Lazy<IAuditRepository>(() => new AuditRepository(_auditContext));
    }

    /// <summary>
    /// Repository for mosquito data operations
    /// </summary>
    public IMosquitoRepository MosquitoRepository => _mosquitoRepository.Value;

    /// <summary>
    /// Repository for audit log operations
    /// </summary>
    public IAuditRepository AuditRepository => _auditRepository.Value;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Save changes to both contexts
            var mosquitoChanges = await _mosquitoContext.SaveChangesAsync(cancellationToken);
            var auditChanges = await _auditContext.SaveChangesAsync(cancellationToken);

            return mosquitoChanges + auditChanges;
        }
        catch (Exception)
        {
            // In case of error, we might want to log it or handle it specifically
            throw;
        }
    }

    public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        // Begin transactions on both contexts
        var mosquitoTransaction = await _mosquitoContext.Database.BeginTransactionAsync(cancellationToken);
        var auditTransaction = await _auditContext.Database.BeginTransactionAsync(cancellationToken);

        return new UnitOfWorkTransaction(mosquitoTransaction, auditTransaction);
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
    {
        using var transaction = await BeginTransactionAsync(cancellationToken);
        
        try
        {
            var result = await operation();
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        using var transaction = await BeginTransactionAsync(cancellationToken);
        
        try
        {
            await operation();
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _mosquitoContext.Dispose();
            _auditContext.Dispose();
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Implementation of database transaction management for dual databases
/// </summary>
public class UnitOfWorkTransaction : IUnitOfWorkTransaction
{
    private readonly IDbContextTransaction _mosquitoTransaction;
    private readonly IDbContextTransaction _auditTransaction;
    private bool _disposed;

    public UnitOfWorkTransaction(IDbContextTransaction mosquitoTransaction, IDbContextTransaction auditTransaction)
    {
        _mosquitoTransaction = mosquitoTransaction;
        _auditTransaction = auditTransaction;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Commit both transactions
            await _mosquitoTransaction.CommitAsync(cancellationToken);
            await _auditTransaction.CommitAsync(cancellationToken);
        }
        catch
        {
            // If one fails, rollback both
            await RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _mosquitoTransaction.RollbackAsync(cancellationToken);
        }
        catch
        {
            // Continue with audit rollback even if mosquito rollback fails
        }

        try
        {
            await _auditTransaction.RollbackAsync(cancellationToken);
        }
        catch
        {
            // Log the error but don't throw since we're already in error handling
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _mosquitoTransaction.Dispose();
            _auditTransaction.Dispose();
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}