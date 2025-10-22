using System;
using System.Threading.Tasks;
using System.Transactions;
using Jobs.EasyApply.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Jobs.EasyApply.Infrastructure.Repositories
{
    /// <summary>
    /// Unit of Work implementation for managing database transactions and operations
    /// </summary>
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly Jobs.EasyApply.Infrastructure.Data.JobDbContext _context;
        private readonly ILogger<UnitOfWork> _logger;
        private IDbContextTransaction? _currentTransaction;
        private bool _disposed = false;

        public UnitOfWork(Jobs.EasyApply.Infrastructure.Data.JobDbContext context, ILogger<UnitOfWork> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Saves all pending changes to the database
        /// </summary>
        /// <returns>Number of state entries written to the database</returns>
        public async Task<int> SaveChangesAsync()
        {
            try
            {
                _logger.LogDebug("Saving changes to database");
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Successfully saved {ChangesCount} changes to database", result);
                }

                return result;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error saving changes to database");
                throw new InvalidOperationException("Error saving changes to database", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error saving changes to database");
                throw;
            }
        }

        /// <summary>
        /// Begins a database transaction
        /// </summary>
        public async Task BeginTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                throw new InvalidOperationException("A transaction is already in progress");
            }

            try
            {
                _logger.LogDebug("Beginning database transaction");
                _currentTransaction = await _context.Database.BeginTransactionAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error beginning database transaction");
                throw;
            }
        }

        /// <summary>
        /// Commits the current transaction
        /// </summary>
        public async Task CommitTransactionAsync()
        {
            if (_currentTransaction == null)
            {
                throw new InvalidOperationException("No transaction in progress");
            }

            try
            {
                _logger.LogDebug("Committing database transaction");
                await _currentTransaction.CommitAsync();
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error committing database transaction");
                throw;
            }
        }

        /// <summary>
        /// Rolls back the current transaction
        /// </summary>
        public async Task RollbackTransactionAsync()
        {
            if (_currentTransaction == null)
            {
                throw new InvalidOperationException("No transaction in progress");
            }

            try
            {
                _logger.LogDebug("Rolling back database transaction");
                await _currentTransaction.RollbackAsync();
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rolling back database transaction");
                throw;
            }
        }

        /// <summary>
        /// Disposes the Unit of Work and any active transactions
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern
        /// </summary>
        /// <param name="disposing">Whether this method is being called from Dispose</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    if (_currentTransaction != null)
                    {
                        _currentTransaction.Dispose();
                        _currentTransaction = null;
                    }
                }

                _disposed = true;
            }
        }
    }
}
