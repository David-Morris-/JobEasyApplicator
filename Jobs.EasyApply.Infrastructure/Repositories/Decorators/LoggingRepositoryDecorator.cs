using System.Diagnostics;
using Jobs.EasyApply.Infrastructure.Repositories.Specifications;
using Microsoft.Extensions.Logging;

namespace Jobs.EasyApply.Infrastructure.Repositories.Decorators
{
    /// <summary>
    /// Repository decorator that adds logging capabilities
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <typeparam name="TKey">The primary key type</typeparam>
    public class LoggingRepositoryDecorator<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class
    {
        private readonly IRepository<TEntity, TKey> _decoratedRepository;
        private readonly ILogger<LoggingRepositoryDecorator<TEntity, TKey>> _logger;

        public LoggingRepositoryDecorator(
            IRepository<TEntity, TKey> decoratedRepository,
            ILogger<LoggingRepositoryDecorator<TEntity, TKey>> logger)
        {
            _decoratedRepository = decoratedRepository ?? throw new ArgumentNullException(nameof(decoratedRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TEntity?> GetByIdAsync(TKey id)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogDebug("Getting entity of type {EntityType} with id {Id}", typeof(TEntity).Name, id);
                var result = await _decoratedRepository.GetByIdAsync(id);
                stopwatch.Stop();

                if (result != null)
                    _logger.LogInformation("Successfully retrieved entity of type {EntityType} with id {Id} in {Elapsed}ms",
                        typeof(TEntity).Name, id, stopwatch.ElapsedMilliseconds);
                else
                    _logger.LogWarning("Entity of type {EntityType} with id {Id} not found", typeof(TEntity).Name, id);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error getting entity of type {EntityType} with id {Id} after {Elapsed}ms",
                    typeof(TEntity).Name, id, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogDebug("Getting all entities of type {EntityType}", typeof(TEntity).Name);
                var result = await _decoratedRepository.GetAllAsync();
                stopwatch.Stop();

                _logger.LogInformation("Successfully retrieved {Count} entities of type {EntityType} in {Elapsed}ms",
                    result.Count(), typeof(TEntity).Name, stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error getting all entities of type {EntityType} after {Elapsed}ms",
                    typeof(TEntity).Name, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<IEnumerable<TEntity>> GetAsync(ISpecification<TEntity>? specification = null)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogDebug("Getting entities of type {EntityType} with specification", typeof(TEntity).Name);
                var result = await _decoratedRepository.GetAsync(specification);
                stopwatch.Stop();

                _logger.LogInformation("Successfully retrieved {Count} entities of type {EntityType} with specification in {Elapsed}ms",
                    result.Count(), typeof(TEntity).Name, stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error getting entities of type {EntityType} with specification after {Elapsed}ms",
                    typeof(TEntity).Name, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<TEntity> AddAsync(TEntity entity)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogDebug("Adding entity of type {EntityType}", typeof(TEntity).Name);
                var result = await _decoratedRepository.AddAsync(entity);
                stopwatch.Stop();

                _logger.LogInformation("Successfully added entity of type {EntityType} in {Elapsed}ms",
                    typeof(TEntity).Name, stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error adding entity of type {EntityType} after {Elapsed}ms",
                    typeof(TEntity).Name, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogDebug("Adding {Count} entities of type {EntityType}", entities.Count(), typeof(TEntity).Name);
                var result = await _decoratedRepository.AddRangeAsync(entities);
                stopwatch.Stop();

                _logger.LogInformation("Successfully added {Count} entities of type {EntityType} in {Elapsed}ms",
                    result.Count(), typeof(TEntity).Name, stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error adding {Count} entities of type {EntityType} after {Elapsed}ms",
                    entities.Count(), typeof(TEntity).Name, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogDebug("Updating entity of type {EntityType}", typeof(TEntity).Name);
                var result = await _decoratedRepository.UpdateAsync(entity);
                stopwatch.Stop();

                _logger.LogInformation("Successfully updated entity of type {EntityType} in {Elapsed}ms",
                    typeof(TEntity).Name, stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error updating entity of type {EntityType} after {Elapsed}ms",
                    typeof(TEntity).Name, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<IEnumerable<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogDebug("Updating {Count} entities of type {EntityType}", entities.Count(), typeof(TEntity).Name);
                var result = await _decoratedRepository.UpdateRangeAsync(entities);
                stopwatch.Stop();

                _logger.LogInformation("Successfully updated {Count} entities of type {EntityType} in {Elapsed}ms",
                    result.Count(), typeof(TEntity).Name, stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error updating {Count} entities of type {EntityType} after {Elapsed}ms",
                    entities.Count(), typeof(TEntity).Name, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(TKey id)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogDebug("Deleting entity of type {EntityType} with id {Id}", typeof(TEntity).Name, id);
                var result = await _decoratedRepository.DeleteAsync(id);
                stopwatch.Stop();

                if (result)
                    _logger.LogInformation("Successfully deleted entity of type {EntityType} with id {Id} in {Elapsed}ms",
                        typeof(TEntity).Name, id, stopwatch.ElapsedMilliseconds);
                else
                    _logger.LogWarning("Entity of type {EntityType} with id {Id} not found for deletion", typeof(TEntity).Name, id);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error deleting entity of type {EntityType} with id {Id} after {Elapsed}ms",
                    typeof(TEntity).Name, id, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<int> DeleteRangeAsync(IEnumerable<TKey> ids)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogDebug("Deleting {Count} entities of type {EntityType}", ids.Count(), typeof(TEntity).Name);
                var result = await _decoratedRepository.DeleteRangeAsync(ids);
                stopwatch.Stop();

                _logger.LogInformation("Successfully deleted {Count} entities of type {EntityType} in {Elapsed}ms",
                    result, typeof(TEntity).Name, stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error deleting {Count} entities of type {EntityType} after {Elapsed}ms",
                    ids.Count(), typeof(TEntity).Name, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<bool> SoftDeleteAsync(TKey id)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogDebug("Soft deleting entity of type {EntityType} with id {Id}", typeof(TEntity).Name, id);
                var result = await _decoratedRepository.SoftDeleteAsync(id);
                stopwatch.Stop();

                if (result)
                    _logger.LogInformation("Successfully soft deleted entity of type {EntityType} with id {Id} in {Elapsed}ms",
                        typeof(TEntity).Name, id, stopwatch.ElapsedMilliseconds);
                else
                    _logger.LogWarning("Entity of type {EntityType} with id {Id} not found for soft deletion", typeof(TEntity).Name, id);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error soft deleting entity of type {EntityType} with id {Id} after {Elapsed}ms",
                    typeof(TEntity).Name, id, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(TKey id)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogDebug("Checking existence of entity of type {EntityType} with id {Id}", typeof(TEntity).Name, id);
                var result = await _decoratedRepository.ExistsAsync(id);
                stopwatch.Stop();

                _logger.LogDebug("Entity of type {EntityType} with id {Id} exists: {Exists} (checked in {Elapsed}ms)",
                    typeof(TEntity).Name, id, result, stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error checking existence of entity of type {EntityType} with id {Id} after {Elapsed}ms",
                    typeof(TEntity).Name, id, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<int> CountAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogDebug("Counting all entities of type {EntityType}", typeof(TEntity).Name);
                var result = await _decoratedRepository.CountAsync();
                stopwatch.Stop();

                _logger.LogInformation("Found {Count} entities of type {EntityType} in {Elapsed}ms",
                    result, typeof(TEntity).Name, stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error counting entities of type {EntityType} after {Elapsed}ms",
                    typeof(TEntity).Name, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<int> CountAsync(ISpecification<TEntity>? specification)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogDebug("Counting entities of type {EntityType} with specification", typeof(TEntity).Name);
                var result = await _decoratedRepository.CountAsync(specification);
                stopwatch.Stop();

                _logger.LogInformation("Found {Count} entities of type {EntityType} with specification in {Elapsed}ms",
                    result, typeof(TEntity).Name, stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error counting entities of type {EntityType} with specification after {Elapsed}ms",
                    typeof(TEntity).Name, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<bool> AnyAsync(ISpecification<TEntity>? specification)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogDebug("Checking if any entities of type {EntityType} match specification", typeof(TEntity).Name);
                var result = await _decoratedRepository.AnyAsync(specification);
                stopwatch.Stop();

                _logger.LogDebug("Any entities of type {EntityType} match specification: {Result} (checked in {Elapsed}ms)",
                    typeof(TEntity).Name, result, stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error checking if any entities of type {EntityType} match specification after {Elapsed}ms",
                    typeof(TEntity).Name, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<TEntity?> FirstOrDefaultAsync(ISpecification<TEntity>? specification)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogDebug("Getting first entity of type {EntityType} matching specification", typeof(TEntity).Name);
                var result = await _decoratedRepository.FirstOrDefaultAsync(specification);
                stopwatch.Stop();

                if (result != null)
                    _logger.LogInformation("Successfully retrieved first entity of type {EntityType} matching specification in {Elapsed}ms",
                        typeof(TEntity).Name, stopwatch.ElapsedMilliseconds);
                else
                    _logger.LogDebug("No entity of type {EntityType} found matching specification", typeof(TEntity).Name);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error getting first entity of type {EntityType} matching specification after {Elapsed}ms",
                    typeof(TEntity).Name, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
}
