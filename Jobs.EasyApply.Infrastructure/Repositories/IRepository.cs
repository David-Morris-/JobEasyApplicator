using System.Collections.Generic;
using System.Threading.Tasks;

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
        Task<TEntity> GetByIdAsync(TKey id);

        /// <summary>
        /// Gets all entities
        /// </summary>
        /// <returns>List of all entities</returns>
        Task<IEnumerable<TEntity>> GetAllAsync();

        /// <summary>
        /// Adds a new entity
        /// </summary>
        /// <param name="entity">The entity to add</param>
        /// <returns>The added entity</returns>
        Task<TEntity> AddAsync(TEntity entity);

        /// <summary>
        /// Updates an existing entity
        /// </summary>
        /// <param name="entity">The entity to update</param>
        /// <returns>The updated entity</returns>
        Task<TEntity> UpdateAsync(TEntity entity);

        /// <summary>
        /// Deletes an entity by its primary key
        /// </summary>
        /// <param name="id">The primary key value</param>
        /// <returns>True if deleted, false if not found</returns>
        Task<bool> DeleteAsync(TKey id);

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
    }
}
