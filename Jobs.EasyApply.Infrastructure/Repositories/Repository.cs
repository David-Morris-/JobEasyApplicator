using System.Linq.Expressions;
using Jobs.EasyApply.Infrastructure.Data;
using Jobs.EasyApply.Infrastructure.Repositories.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Jobs.EasyApply.Infrastructure.Repositories
{
    /// <summary>
    /// Generic repository implementation using Entity Framework
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <typeparam name="TKey">The primary key type</typeparam>
    public class Repository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class
    {
        protected readonly JobDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public Repository(JobDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<TEntity>();
        }

        /// <summary>
        /// Gets an entity by its primary key
        /// </summary>
        /// <param name="id">The primary key value</param>
        /// <returns>The entity if found, null otherwise</returns>
        public virtual async Task<TEntity> GetByIdAsync(TKey id)
        {
            return await _dbSet.FindAsync(id);
        }

        /// <summary>
        /// Gets all entities
        /// </summary>
        /// <returns>List of all entities</returns>
        public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        /// <summary>
        /// Gets entities based on specification criteria
        /// </summary>
        /// <param name="specification">The specification criteria</param>
        /// <returns>Filtered list of entities</returns>
        public virtual async Task<IEnumerable<TEntity>> GetAsync(ISpecification<TEntity> specification = null)
        {
            var query = ApplySpecification(_dbSet.AsQueryable(), specification);
            return await query.ToListAsync();
        }

        /// <summary>
        /// Adds a new entity
        /// </summary>
        /// <param name="entity">The entity to add</param>
        /// <returns>The added entity</returns>
        public virtual async Task<TEntity> AddAsync(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _dbSet.AddAsync(entity);
            return entity;
        }

        /// <summary>
        /// Updates an existing entity
        /// </summary>
        /// <param name="entity">The entity to update</param>
        /// <returns>The updated entity</returns>
        public virtual Task<TEntity> UpdateAsync(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Update(entity);
            return Task.FromResult(entity);
        }

        /// <summary>
        /// Deletes an entity by its primary key
        /// </summary>
        /// <param name="id">The primary key value</param>
        /// <returns>True if deleted, false if not found</returns>
        public virtual async Task<bool> DeleteAsync(TKey id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
                return false;

            _dbSet.Remove(entity);
            return true;
        }

        /// <summary>
        /// Checks if an entity exists by its primary key
        /// </summary>
        /// <param name="id">The primary key value</param>
        /// <returns>True if exists, false otherwise</returns>
        public virtual async Task<bool> ExistsAsync(TKey id)
        {
            return await GetByIdAsync(id) != null;
        }

        /// <summary>
        /// Gets the count of all entities
        /// </summary>
        /// <returns>The total count of entities</returns>
        public virtual async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }

        /// <summary>
        /// Gets the count of entities based on specification criteria
        /// </summary>
        /// <param name="specification">The specification criteria</param>
        /// <returns>The count of filtered entities</returns>
        public virtual async Task<int> CountAsync(ISpecification<TEntity> specification)
        {
            var query = ApplySpecification(_dbSet.AsQueryable(), specification);
            return await query.CountAsync();
        }

        /// <summary>
        /// Applies specification criteria to a query
        /// </summary>
        /// <param name="query">The base query</param>
        /// <param name="specification">The specification to apply</param>
        /// <returns>The modified query</returns>
        protected virtual IQueryable<TEntity> ApplySpecification(IQueryable<TEntity> query, ISpecification<TEntity> specification)
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
