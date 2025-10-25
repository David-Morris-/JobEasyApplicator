using System.Collections.Generic;
using System.Threading.Tasks;
using Jobs.EasyApply.Infrastructure.Repositories.Specifications;

namespace Jobs.EasyApply.Infrastructure.Repositories
{
    /// <summary>
    /// Generic repository interface for basic CRUD operations
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <typeparam name="TKey">The primary key type</typeparam>
    public interface IRepository<TEntity, TKey> where TEntity : class
    {
        /// <summary>
        /// Gets an entity by its primary key
        /// </summary>
        /// <param name="id">The primary key value</param>
        /// <returns>The entity if found, null otherwise</returns>
        Task<TEntity?> GetByIdAsync(TKey id);

        /// <summary>
        /// Gets all entities
        /// </summary>
        /// <returns>List of all entities</returns>
        Task<IEnumerable<TEntity>> GetAllAsync();

        /// <summary>
        /// Gets entities based on specification criteria
        /// </summary>
        /// <param name="specification">The specification criteria</param>
        /// <returns>Filtered list of entities</returns>
        Task<IEnumerable<TEntity>> GetAsync(ISpecification<TEntity>? specification = null);

        /// <summary>
        /// Adds a new entity
        /// </summary>
        /// <param name="entity">The entity to add</param>
        /// <returns>The added entity</returns>
        Task<TEntity> AddAsync(TEntity entity);

        /// <summary>
        /// Adds multiple entities in a single operation
        /// </summary>
        /// <param name="entities">The entities to add</param>
        /// <returns>The added entities</returns>
        Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities);

        /// <summary>
        /// Updates an existing entity
        /// </summary>
        /// <param name="entity">The entity to update</param>
        /// <returns>The updated entity</returns>
        Task<TEntity> UpdateAsync(TEntity entity);

        /// <summary>
        /// Updates multiple entities in a single operation
        /// </summary>
        /// <param name="entities">The entities to update</param>
        /// <returns>The updated entities</returns>
        Task<IEnumerable<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities);

        /// <summary>
        /// Deletes an entity by its primary key
        /// </summary>
        /// <param name="id">The primary key value</param>
        /// <returns>True if deleted, false if not found</returns>
        Task<bool> DeleteAsync(TKey id);

        /// <summary>
        /// Deletes multiple entities by their primary keys
        /// </summary>
        /// <param name="ids">The primary key values</param>
        /// <returns>Number of entities deleted</returns>
        Task<int> DeleteRangeAsync(IEnumerable<TKey> ids);

        /// <summary>
        /// Soft deletes an entity by its primary key (marks as deleted)
        /// </summary>
        /// <param name="id">The primary key value</param>
        /// <returns>True if soft deleted, false if not found</returns>
        Task<bool> SoftDeleteAsync(TKey id);

        /// <summary>
        /// Checks if an entity exists by its primary key
        /// </summary>
        /// <param name="id">The primary key value</param>
        /// <returns>True if exists, false otherwise</returns>
        Task<bool> ExistsAsync(TKey id);

        /// <summary>
        /// Gets the count of all entities
        /// </summary>
        /// <returns>The total count of entities</returns>
        Task<int> CountAsync();

        /// <summary>
        /// Gets the count of entities based on specification criteria
        /// </summary>
        /// <param name="specification">The specification criteria</param>
        /// <returns>The count of filtered entities</returns>
        Task<int> CountAsync(ISpecification<TEntity>? specification);

        /// <summary>
        /// Checks if any entities match the specification criteria
        /// </summary>
        /// <param name="specification">The specification criteria</param>
        /// <returns>True if any entities match, false otherwise</returns>
        Task<bool> AnyAsync(ISpecification<TEntity>? specification);

        /// <summary>
        /// Gets the first entity matching the specification criteria
        /// </summary>
        /// <param name="specification">The specification criteria</param>
        /// <returns>The first entity if found, null otherwise</returns>
        Task<TEntity?> FirstOrDefaultAsync(ISpecification<TEntity>? specification);
    }
}
