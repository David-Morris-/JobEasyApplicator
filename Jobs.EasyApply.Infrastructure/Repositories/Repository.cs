using System.Linq.Expressions;
using Jobs.EasyApply.Infrastructure.Data;
using Jobs.EasyApply.Infrastructure.Repositories.Specifications;
using Jobs.EasyApply.Infrastructure.Repositories.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Jobs.EasyApply.Infrastructure.Repositories
{
    /// <summary>
    /// Generic repository implementation using Entity Framework with comprehensive error handling and performance monitoring
    /// </summary>
    /// <typeparam name="TEntity">The entity type that must be a class</typeparam>
    /// <typeparam name="TKey">The primary key type</typeparam>
    public class Repository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class
    {
        protected readonly JobDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;
        protected readonly ILogger<Repository<TEntity, TKey>> _logger;

        public Repository(JobDbContext context, ILogger<Repository<TEntity, TKey>> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<TEntity>();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets an entity by its primary key asynchronously
        /// </summary>
        /// <param name="id">The primary key value</param>
        /// <returns>The entity if found, null otherwise</returns>
        /// <exception cref="ArgumentException">Thrown when id is null or invalid</exception>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        public virtual async Task<TEntity?> GetByIdAsync(TKey id)
        {
            try
            {
                _logger.LogDebug("Getting entity of type {EntityType} with id {Id}", typeof(TEntity).Name, id);
                var entity = await _dbSet.FindAsync(id);
                _logger.LogDebug("Entity retrieval completed for type {EntityType} with id {Id}", typeof(TEntity).Name, id);
                return entity;
            }
            catch (Exception ex) when (ex is DbUpdateException or InvalidOperationException)
            {
                _logger.LogError(ex, "Error retrieving entity of type {EntityType} with id {Id}", typeof(TEntity).Name, id);
                throw new RepositoryConnectionException("Failed to retrieve entity from database", ex);
            }
        }

        /// <summary>
        /// Gets all entities from the repository asynchronously
        /// </summary>
        /// <returns>Collection of all entities</returns>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            try
            {
                _logger.LogDebug("Getting all entities of type {EntityType}", typeof(TEntity).Name);
                var entities = await _dbSet.ToListAsync();
                _logger.LogDebug("Retrieved {Count} entities of type {EntityType}", entities.Count, typeof(TEntity).Name);
                return entities;
            }
            catch (Exception ex) when (ex is DbUpdateException or InvalidOperationException)
            {
                _logger.LogError(ex, "Error retrieving all entities of type {EntityType}", typeof(TEntity).Name);
                throw new RepositoryConnectionException("Failed to retrieve entities from database", ex);
            }
        }

        /// <summary>
        /// Gets entities based on specification criteria asynchronously
        /// </summary>
        /// <param name="specification">The specification criteria for filtering, null for all entities</param>
        /// <returns>Filtered collection of entities</returns>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        public virtual async Task<IEnumerable<TEntity>> GetAsync(ISpecification<TEntity>? specification = null)
        {
            try
            {
                _logger.LogDebug("Getting entities of type {EntityType} with specification", typeof(TEntity).Name);
                var query = ApplySpecification(_dbSet.AsQueryable(), specification);
                var entities = await query.ToListAsync();
                _logger.LogDebug("Retrieved {Count} entities of type {EntityType} with specification", entities.Count, typeof(TEntity).Name);
                return entities;
            }
            catch (Exception ex) when (ex is DbUpdateException or InvalidOperationException)
            {
                _logger.LogError(ex, "Error retrieving entities of type {EntityType} with specification", typeof(TEntity).Name);
                throw new RepositoryConnectionException("Failed to retrieve entities from database", ex);
            }
        }

        /// <summary>
        /// Adds a new entity to the repository asynchronously
        /// </summary>
        /// <param name="entity">The entity to add</param>
        /// <returns>The added entity with any generated values</returns>
        /// <exception cref="ArgumentNullException">Thrown when entity is null</exception>
        /// <exception cref="RepositoryValidationException">Thrown when entity validation fails</exception>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        public virtual async Task<TEntity> AddAsync(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            try
            {
                _logger.LogDebug("Adding entity of type {EntityType}", typeof(TEntity).Name);
                await _dbSet.AddAsync(entity);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully added entity of type {EntityType}", typeof(TEntity).Name);
                return entity;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error adding entity of type {EntityType}", typeof(TEntity).Name);
                throw new RepositoryValidationException("Failed to add entity due to validation errors", ex);
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
            {
                _logger.LogError(ex, "Error adding entity of type {EntityType}", typeof(TEntity).Name);
                throw new RepositoryConnectionException("Failed to add entity to database", ex);
            }
        }

        /// <summary>
        /// Updates an existing entity in the repository asynchronously
        /// </summary>
        /// <param name="entity">The entity to update</param>
        /// <returns>The updated entity</returns>
        /// <exception cref="ArgumentNullException">Thrown when entity is null</exception>
        /// <exception cref="EntityNotFoundException">Thrown when entity is not found</exception>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        public virtual async Task<TEntity> UpdateAsync(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            try
            {
                _logger.LogDebug("Updating entity of type {EntityType}", typeof(TEntity).Name);
                _dbSet.Update(entity);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully updated entity of type {EntityType}", typeof(TEntity).Name);
                return entity;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating entity of type {EntityType}", typeof(TEntity).Name);
                throw new ConcurrencyException(typeof(TEntity), "Concurrency conflict occurred while updating entity");
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
            {
                _logger.LogError(ex, "Error updating entity of type {EntityType}", typeof(TEntity).Name);
                throw new RepositoryConnectionException("Failed to update entity in database", ex);
            }
        }

        /// <summary>
        /// Deletes an entity by its primary key asynchronously (hard delete)
        /// </summary>
        /// <param name="id">The primary key value</param>
        /// <returns>True if deleted, false if not found</returns>
        /// <exception cref="ArgumentException">Thrown when id is invalid</exception>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        public virtual async Task<bool> DeleteAsync(TKey id)
        {
            try
            {
                _logger.LogDebug("Deleting entity of type {EntityType} with id {Id}", typeof(TEntity).Name, id);
                var entity = await GetByIdAsync(id);
                if (entity == null)
                {
                    _logger.LogWarning("Entity of type {EntityType} with id {Id} not found for deletion", typeof(TEntity).Name, id);
                    return false;
                }

                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully deleted entity of type {EntityType} with id {Id}", typeof(TEntity).Name, id);
                return true;
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
            {
                _logger.LogError(ex, "Error deleting entity of type {EntityType} with id {Id}", typeof(TEntity).Name, id);
                throw new RepositoryConnectionException("Failed to delete entity from database", ex);
            }
        }

        /// <summary>
        /// Checks if an entity exists by its primary key asynchronously
        /// </summary>
        /// <param name="id">The primary key value</param>
        /// <returns>True if exists, false otherwise</returns>
        /// <exception cref="ArgumentException">Thrown when id is invalid</exception>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        public virtual async Task<bool> ExistsAsync(TKey id)
        {
            try
            {
                _logger.LogDebug("Checking existence of entity of type {EntityType} with id {Id}", typeof(TEntity).Name, id);
                var entity = await GetByIdAsync(id);
                var exists = entity != null;
                _logger.LogDebug("Entity existence check completed for type {EntityType} with id {Id}: {Exists}", typeof(TEntity).Name, id, exists);
                return exists;
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
            {
                _logger.LogError(ex, "Error checking existence of entity of type {EntityType} with id {Id}", typeof(TEntity).Name, id);
                throw new RepositoryConnectionException("Failed to check entity existence in database", ex);
            }
        }

        /// <summary>
        /// Gets the count of all entities asynchronously
        /// </summary>
        /// <returns>The total count of entities</returns>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        public virtual async Task<int> CountAsync()
        {
            try
            {
                _logger.LogDebug("Counting all entities of type {EntityType}", typeof(TEntity).Name);
                var count = await _dbSet.CountAsync();
                _logger.LogDebug("Counted {Count} entities of type {EntityType}", count, typeof(TEntity).Name);
                return count;
            }
            catch (Exception ex) when (ex is InvalidOperationException)
            {
                _logger.LogError(ex, "Error counting entities of type {EntityType}", typeof(TEntity).Name);
                throw new RepositoryConnectionException("Failed to count entities in database", ex);
            }
        }

        /// <summary>
        /// Gets the count of entities based on specification criteria asynchronously
        /// </summary>
        /// <param name="specification">The specification criteria, null for all entities</param>
        /// <returns>The count of filtered entities</returns>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        public virtual async Task<int> CountAsync(ISpecification<TEntity>? specification)
        {
            try
            {
                _logger.LogDebug("Counting entities of type {EntityType} with specification", typeof(TEntity).Name);
                var query = ApplySpecification(_dbSet.AsQueryable(), specification);
                var count = await query.CountAsync();
                _logger.LogDebug("Counted {Count} entities of type {EntityType} with specification", count, typeof(TEntity).Name);
                return count;
            }
            catch (Exception ex) when (ex is InvalidOperationException)
            {
                _logger.LogError(ex, "Error counting entities of type {EntityType} with specification", typeof(TEntity).Name);
                throw new RepositoryConnectionException("Failed to count entities in database", ex);
            }
        }

        /// <summary>
        /// Adds multiple entities in a single operation asynchronously
        /// </summary>
        /// <param name="entities">The entities to add</param>
        /// <returns>The added entities</returns>
        /// <exception cref="ArgumentNullException">Thrown when entities is null</exception>
        /// <exception cref="RepositoryValidationException">Thrown when any entity validation fails</exception>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        public virtual async Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            var entitiesList = entities.ToList();
            if (!entitiesList.Any())
                return entitiesList;

            try
            {
                _logger.LogDebug("Adding {Count} entities of type {EntityType}", entitiesList.Count, typeof(TEntity).Name);
                await _dbSet.AddRangeAsync(entitiesList);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully added {Count} entities of type {EntityType}", entitiesList.Count, typeof(TEntity).Name);
                return entitiesList;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error adding {Count} entities of type {EntityType}", entitiesList.Count, typeof(TEntity).Name);
                throw new RepositoryValidationException("Failed to add entities due to validation errors", ex);
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
            {
                _logger.LogError(ex, "Error adding {Count} entities of type {EntityType}", entitiesList.Count, typeof(TEntity).Name);
                throw new RepositoryConnectionException("Failed to add entities to database", ex);
            }
        }

        /// <summary>
        /// Updates multiple entities in a single operation asynchronously
        /// </summary>
        /// <param name="entities">The entities to update</param>
        /// <returns>The updated entities</returns>
        /// <exception cref="ArgumentNullException">Thrown when entities is null</exception>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        public virtual async Task<IEnumerable<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            var entitiesList = entities.ToList();
            if (!entitiesList.Any())
                return entitiesList;

            try
            {
                _logger.LogDebug("Updating {Count} entities of type {EntityType}", entitiesList.Count, typeof(TEntity).Name);
                _dbSet.UpdateRange(entitiesList);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully updated {Count} entities of type {EntityType}", entitiesList.Count, typeof(TEntity).Name);
                return entitiesList;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating {Count} entities of type {EntityType}", entitiesList.Count, typeof(TEntity).Name);
                throw new ConcurrencyException(typeof(TEntity), "Concurrency conflict occurred while updating entities");
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
            {
                _logger.LogError(ex, "Error updating {Count} entities of type {EntityType}", entitiesList.Count, typeof(TEntity).Name);
                throw new RepositoryConnectionException("Failed to update entities in database", ex);
            }
        }

        /// <summary>
        /// Deletes multiple entities by their primary keys asynchronously
        /// </summary>
        /// <param name="ids">The primary key values</param>
        /// <returns>Number of entities deleted</returns>
        /// <exception cref="ArgumentNullException">Thrown when ids is null</exception>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        public virtual async Task<int> DeleteRangeAsync(IEnumerable<TKey> ids)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            var idsList = ids.ToList();
            if (!idsList.Any())
                return 0;

            try
            {
                _logger.LogDebug("Deleting {Count} entities of type {EntityType}", idsList.Count, typeof(TEntity).Name);
                var entities = new List<TEntity>();
                foreach (var id in idsList)
                {
                    var entity = await GetByIdAsync(id);
                    if (entity != null)
                        entities.Add(entity);
                }

                if (entities.Any())
                {
                    _dbSet.RemoveRange(entities);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Successfully deleted {Count} entities of type {EntityType}", entities.Count, typeof(TEntity).Name);
                    return entities.Count;
                }

                return 0;
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
            {
                _logger.LogError(ex, "Error deleting {Count} entities of type {EntityType}", idsList.Count, typeof(TEntity).Name);
                throw new RepositoryConnectionException("Failed to delete entities from database", ex);
            }
        }

        /// <summary>
        /// Soft deletes an entity by its primary key (marks as deleted without removing from database)
        /// </summary>
        /// <param name="id">The primary key value</param>
        /// <returns>True if soft deleted, false if not found</returns>
        /// <exception cref="ArgumentException">Thrown when id is invalid</exception>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        public virtual async Task<bool> SoftDeleteAsync(TKey id)
        {
            try
            {
                _logger.LogDebug("Soft deleting entity of type {EntityType} with id {Id}", typeof(TEntity).Name, id);
                var entity = await GetByIdAsync(id);
                if (entity == null)
                {
                    _logger.LogWarning("Entity of type {EntityType} with id {Id} not found for soft deletion", typeof(TEntity).Name, id);
                    return false;
                }

                // Set soft delete properties
                var propertyInfo = entity.GetType().GetProperty("IsDeleted");
                if (propertyInfo != null && propertyInfo.PropertyType == typeof(bool))
                {
                    propertyInfo.SetValue(entity, true);
                }

                var deletedAtProperty = entity.GetType().GetProperty("DeletedAt");
                if (deletedAtProperty != null && deletedAtProperty.PropertyType == typeof(DateTime?))
                {
                    deletedAtProperty.SetValue(entity, DateTime.UtcNow);
                }

                _dbSet.Update(entity);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully soft deleted entity of type {EntityType} with id {Id}", typeof(TEntity).Name, id);
                return true;
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
            {
                _logger.LogError(ex, "Error soft deleting entity of type {EntityType} with id {Id}", typeof(TEntity).Name, id);
                throw new RepositoryConnectionException("Failed to soft delete entity in database", ex);
            }
        }

        /// <summary>
        /// Checks if any entities match the specification criteria asynchronously
        /// </summary>
        /// <param name="specification">The specification criteria, null for all entities</param>
        /// <returns>True if any entities match, false otherwise</returns>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        public virtual async Task<bool> AnyAsync(ISpecification<TEntity>? specification)
        {
            try
            {
                _logger.LogDebug("Checking if any entities of type {EntityType} match specification", typeof(TEntity).Name);
                var query = ApplySpecification(_dbSet.AsQueryable(), specification);
                var result = await query.AnyAsync();
                _logger.LogDebug("Any check completed for type {EntityType} with specification: {Result}", typeof(TEntity).Name, result);
                return result;
            }
            catch (Exception ex) when (ex is InvalidOperationException)
            {
                _logger.LogError(ex, "Error checking if any entities of type {EntityType} match specification", typeof(TEntity).Name);
                throw new RepositoryConnectionException("Failed to check entities in database", ex);
            }
        }

        /// <summary>
        /// Gets the first entity matching the specification criteria asynchronously
        /// </summary>
        /// <param name="specification">The specification criteria, null for first entity</param>
        /// <returns>The first entity if found, null otherwise</returns>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        public virtual async Task<TEntity?> FirstOrDefaultAsync(ISpecification<TEntity>? specification)
        {
            try
            {
                _logger.LogDebug("Getting first entity of type {EntityType} matching specification", typeof(TEntity).Name);
                var query = ApplySpecification(_dbSet.AsQueryable(), specification);
                var entity = await query.FirstOrDefaultAsync();
                _logger.LogDebug("FirstOrDefault completed for type {EntityType} with specification: {Found}", typeof(TEntity).Name, entity != null);
                return entity;
            }
            catch (Exception ex) when (ex is InvalidOperationException)
            {
                _logger.LogError(ex, "Error getting first entity of type {EntityType} matching specification", typeof(TEntity).Name);
                throw new RepositoryConnectionException("Failed to retrieve first entity from database", ex);
            }
        }

        /// <summary>
        /// Applies specification criteria to a query
        /// </summary>
        /// <param name="query">The base query</param>
        /// <param name="specification">The specification to apply</param>
        /// <returns>The modified query</returns>
        protected virtual IQueryable<TEntity> ApplySpecification(IQueryable<TEntity> query, ISpecification<TEntity>? specification)
        {
            if (specification == null)
                return query;

            // Apply criteria
            if (specification.Criteria != null)
                query = query.Where(specification.Criteria);

            // Apply includes
            query = specification.Includes.Aggregate(query, (current, include) => current.Include(include));

            // Apply ordering
            if (specification.OrderBy != null)
                query = query.OrderBy(specification.OrderBy);
            else if (specification.OrderByDescending != null)
                query = query.OrderByDescending(specification.OrderByDescending);

            // Apply paging
            if (specification.IsPagingEnabled)
                query = query.Skip(specification.Skip).Take(specification.Take);

            return query;
        }
    }
}
