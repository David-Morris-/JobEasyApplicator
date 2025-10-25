using System.Collections.Generic;
using System.Threading.Tasks;
using Jobs.EasyApply.Infrastructure.Repositories.Specifications;

namespace Jobs.EasyApply.Infrastructure.Repositories
{
    /// <summary>
    /// Generic repository interface for basic CRUD operations with comprehensive data access patterns
    /// </summary>
    /// <typeparam name="TEntity">The entity type that must be a class</typeparam>
    /// <typeparam name="TKey">The primary key type</typeparam>
    public interface IRepository<TEntity, TKey> where TEntity : class
    {
        /// <summary>
        /// Gets an entity by its primary key asynchronously
        /// </summary>
        /// <param name="id">The primary key value</param>
        /// <returns>The entity if found, null otherwise</returns>
        /// <exception cref="ArgumentException">Thrown when id is null or invalid</exception>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        Task<TEntity?> GetByIdAsync(TKey id);

        /// <summary>
        /// Gets all entities from the repository asynchronously
        /// </summary>
        /// <returns>Collection of all entities</returns>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        Task<IEnumerable<TEntity>> GetAllAsync();

        /// <summary>
        /// Gets entities based on specification criteria asynchronously
        /// </summary>
        /// <param name="specification">The specification criteria for filtering, null for all entities</param>
        /// <returns>Filtered collection of entities</returns>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        Task<IEnumerable<TEntity>> GetAsync(ISpecification<TEntity>? specification = null);

        /// <summary>
        /// Adds a new entity to the repository asynchronously
        /// </summary>
        /// <param name="entity">The entity to add</param>
        /// <returns>The added entity with any generated values</returns>
        /// <exception cref="ArgumentNullException">Thrown when entity is null</exception>
        /// <exception cref="RepositoryValidationException">Thrown when entity validation fails</exception>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        Task<TEntity> AddAsync(TEntity entity);

        /// <summary>
        /// Adds multiple entities in a single operation asynchronously
        /// </summary>
        /// <param name="entities">The entities to add</param>
        /// <returns>The added entities</returns>
        /// <exception cref="ArgumentNullException">Thrown when entities is null</exception>
        /// <exception cref="RepositoryValidationException">Thrown when any entity validation fails</exception>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities);

        /// <summary>
        /// Updates an existing entity in the repository asynchronously
        /// </summary>
        /// <param name="entity">The entity to update</param>
        /// <returns>The updated entity</returns>
        /// <exception cref="ArgumentNullException">Thrown when entity is null</exception>
        /// <exception cref="EntityNotFoundException">Thrown when entity is not found</exception>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        Task<TEntity> UpdateAsync(TEntity entity);

        /// <summary>
        /// Updates multiple entities in a single operation asynchronously
        /// </summary>
        /// <param name="entities">The entities to update</param>
        /// <returns>The updated entities</returns>
        /// <exception cref="ArgumentNullException">Thrown when entities is null</exception>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        Task<IEnumerable<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities);

        /// <summary>
        /// Deletes an entity by its primary key asynchronously (hard delete)
        /// </summary>
        /// <param name="id">The primary key value</param>
        /// <returns>True if deleted, false if not found</returns>
        /// <exception cref="ArgumentException">Thrown when id is invalid</exception>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        Task<bool> DeleteAsync(TKey id);

        /// <summary>
        /// Deletes multiple entities by their primary keys asynchronously
        /// </summary>
        /// <param name="ids">The primary key values</param>
        /// <returns>Number of entities deleted</returns>
        /// <exception cref="ArgumentNullException">Thrown when ids is null</exception>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        Task<int> DeleteRangeAsync(IEnumerable<TKey> ids);

        /// <summary>
        /// Soft deletes an entity by its primary key (marks as deleted without removing from database)
        /// </summary>
        /// <param name="id">The primary key value</param>
        /// <returns>True if soft deleted, false if not found</returns>
        /// <exception cref="ArgumentException">Thrown when id is invalid</exception>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        Task<bool> SoftDeleteAsync(TKey id);

        /// <summary>
        /// Checks if an entity exists by its primary key asynchronously
        /// </summary>
        /// <param name="id">The primary key value</param>
        /// <returns>True if exists, false otherwise</returns>
        /// <exception cref="ArgumentException">Thrown when id is invalid</exception>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        Task<bool> ExistsAsync(TKey id);

        /// <summary>
        /// Gets the count of all entities asynchronously
        /// </summary>
        /// <returns>The total count of entities</returns>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        Task<int> CountAsync();

        /// <summary>
        /// Gets the count of entities based on specification criteria asynchronously
        /// </summary>
        /// <param name="specification">The specification criteria, null for all entities</param>
        /// <returns>The count of filtered entities</returns>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        Task<int> CountAsync(ISpecification<TEntity>? specification);

        /// <summary>
        /// Checks if any entities match the specification criteria asynchronously
        /// </summary>
        /// <param name="specification">The specification criteria, null for all entities</param>
        /// <returns>True if any entities match, false otherwise</returns>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        Task<bool> AnyAsync(ISpecification<TEntity>? specification);

        /// <summary>
        /// Gets the first entity matching the specification criteria asynchronously
        /// </summary>
        /// <param name="specification">The specification criteria, null for first entity</param>
        /// <returns>The first entity if found, null otherwise</returns>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        Task<TEntity?> FirstOrDefaultAsync(ISpecification<TEntity>? specification);
    }
}
