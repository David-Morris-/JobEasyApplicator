using System.Linq.Expressions;
using Jobs.EasyApply.Infrastructure.Data;
using Jobs.EasyApply.Infrastructure.Repositories.Specifications;
using Jobs.EasyApply.Infrastructure.Repositories.Exceptions;
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
        public virtual async Task<TEntity?> GetByIdAsync(TKey id)
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
        public virtual async Task<IEnumerable<TEntity>> GetAsync(ISpecification<TEntity>? specification = null)
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
        public virtual async Task<int> CountAsync(ISpecification<TEntity>? specification)
        {
            var query = ApplySpecification(_dbSet.AsQueryable(), specification);
            return await query.CountAsync();
        }

        /// <summary>
        /// Adds multiple entities in a single operation
        /// </summary>
        /// <param name="entities">The entities to add</param>
        /// <returns>The added entities</returns>
        public virtual async Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            var entitiesList = entities.ToList();
            if (!entitiesList.Any())
                return entitiesList;

            await _dbSet.AddRangeAsync(entitiesList);
            return entitiesList;
        }

        /// <summary>
        /// Updates multiple entities in a single operation
        /// </summary>
        /// <param name="entities">The entities to update</param>
        /// <returns>The updated entities</returns>
        public virtual Task<IEnumerable<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            var entitiesList = entities.ToList();
            if (!entitiesList.Any())
                return Task.FromResult(entitiesList.AsEnumerable());

            _dbSet.UpdateRange(entitiesList);
            return Task.FromResult(entitiesList.AsEnumerable());
        }

        /// <summary>
        /// Deletes multiple entities by their primary keys
        /// </summary>
        /// <param name="ids">The primary key values</param>
        /// <returns>Number of entities deleted</returns>
        public virtual async Task<int> DeleteRangeAsync(IEnumerable<TKey> ids)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            var idsList = ids.ToList();
            if (!idsList.Any())
                return 0;

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
                return entities.Count;
            }

            return 0;
        }

        /// <summary>
        /// Soft deletes an entity by its primary key (marks as deleted)
        /// </summary>
        /// <param name="id">The primary key value</param>
        /// <returns>True if soft deleted, false if not found</returns>
        public virtual async Task<bool> SoftDeleteAsync(TKey id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
                return false;

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
            return true;
        }

        /// <summary>
        /// Checks if any entities match the specification criteria
        /// </summary>
        /// <param name="specification">The specification criteria</param>
        /// <returns>True if any entities match, false otherwise</returns>
        public virtual async Task<bool> AnyAsync(ISpecification<TEntity>? specification)
        {
            var query = ApplySpecification(_dbSet.AsQueryable(), specification);
            return await query.AnyAsync();
        }

        /// <summary>
        /// Gets the first entity matching the specification criteria
        /// </summary>
        /// <param name="specification">The specification criteria</param>
        /// <returns>The first entity if found, null otherwise</returns>
        public virtual async Task<TEntity?> FirstOrDefaultAsync(ISpecification<TEntity>? specification)
        {
            var query = ApplySpecification(_dbSet.AsQueryable(), specification);
            return await query.FirstOrDefaultAsync();
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
