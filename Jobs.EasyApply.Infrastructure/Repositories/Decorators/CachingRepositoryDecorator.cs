using System.Collections.Concurrent;
using Jobs.EasyApply.Infrastructure.Repositories.Specifications;
using Microsoft.Extensions.Caching.Memory;

namespace Jobs.EasyApply.Infrastructure.Repositories.Decorators
{
    /// <summary>
    /// Repository decorator that adds caching capabilities
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <typeparam name="TKey">The primary key type</typeparam>
    public class CachingRepositoryDecorator<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class
    {
        private readonly IRepository<TEntity, TKey> _decoratedRepository;
        private readonly IMemoryCache _cache;
        private readonly ConcurrentDictionary<string, object> _cacheKeys;

        public CachingRepositoryDecorator(
            IRepository<TEntity, TKey> decoratedRepository,
            IMemoryCache cache)
        {
            _decoratedRepository = decoratedRepository ?? throw new ArgumentNullException(nameof(decoratedRepository));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _cacheKeys = new ConcurrentDictionary<string, object>();
        }

        public async Task<TEntity?> GetByIdAsync(TKey id)
        {
            var cacheKey = $"{typeof(TEntity).Name}_GetById_{id}";

            if (_cache.TryGetValue(cacheKey, out TEntity? cachedEntity))
            {
                return cachedEntity;
            }

            var entity = await _decoratedRepository.GetByIdAsync(id);

            if (entity != null)
            {
                _cache.Set(cacheKey, entity, TimeSpan.FromMinutes(10));
                _cacheKeys.TryAdd(cacheKey, null);
            }

            return entity;
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            var cacheKey = $"{typeof(TEntity).Name}_GetAll";

            if (_cache.TryGetValue(cacheKey, out IEnumerable<TEntity>? cachedEntities))
            {
                return cachedEntities!;
            }

            var entities = await _decoratedRepository.GetAllAsync();

            _cache.Set(cacheKey, entities, TimeSpan.FromMinutes(5));
            _cacheKeys.TryAdd(cacheKey, null);

            return entities;
        }

        public async Task<IEnumerable<TEntity>> GetAsync(ISpecification<TEntity>? specification = null)
        {
            // For simplicity, not caching specification-based queries
            // In a real implementation, you might want to cache based on specification hash
            return await _decoratedRepository.GetAsync(specification);
        }

        public async Task<TEntity> AddAsync(TEntity entity)
        {
            var result = await _decoratedRepository.AddAsync(entity);
            InvalidateCache();
            return result;
        }

        public async Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities)
        {
            var result = await _decoratedRepository.AddRangeAsync(entities);
            InvalidateCache();
            return result;
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            var result = await _decoratedRepository.UpdateAsync(entity);
            InvalidateCache();
            return result;
        }

        public async Task<IEnumerable<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities)
        {
            var result = await _decoratedRepository.UpdateRangeAsync(entities);
            InvalidateCache();
            return result;
        }

        public async Task<bool> DeleteAsync(TKey id)
        {
            var result = await _decoratedRepository.DeleteAsync(id);
            InvalidateCache();
            return result;
        }

        public async Task<int> DeleteRangeAsync(IEnumerable<TKey> ids)
        {
            var result = await _decoratedRepository.DeleteRangeAsync(ids);
            InvalidateCache();
            return result;
        }

        public async Task<bool> SoftDeleteAsync(TKey id)
        {
            var result = await _decoratedRepository.SoftDeleteAsync(id);
            InvalidateCache();
            return result;
        }

        public async Task<bool> ExistsAsync(TKey id)
        {
            var cacheKey = $"{typeof(TEntity).Name}_Exists_{id}";

            if (_cache.TryGetValue(cacheKey, out bool cachedExists))
            {
                return cachedExists;
            }

            var exists = await _decoratedRepository.ExistsAsync(id);

            _cache.Set(cacheKey, exists, TimeSpan.FromMinutes(10));
            _cacheKeys.TryAdd(cacheKey, null);

            return exists;
        }

        public async Task<int> CountAsync()
        {
            var cacheKey = $"{typeof(TEntity).Name}_Count";

            if (_cache.TryGetValue(cacheKey, out int cachedCount))
            {
                return cachedCount;
            }

            var count = await _decoratedRepository.CountAsync();

            _cache.Set(cacheKey, count, TimeSpan.FromMinutes(5));
            _cacheKeys.TryAdd(cacheKey, null);

            return count;
        }

        public async Task<int> CountAsync(ISpecification<TEntity>? specification)
        {
            // Not caching specification-based counts for simplicity
            return await _decoratedRepository.CountAsync(specification);
        }

        public async Task<bool> AnyAsync(ISpecification<TEntity>? specification)
        {
            return await _decoratedRepository.AnyAsync(specification);
        }

        public async Task<TEntity?> FirstOrDefaultAsync(ISpecification<TEntity>? specification)
        {
            return await _decoratedRepository.FirstOrDefaultAsync(specification);
        }

        private void InvalidateCache()
        {
            foreach (var key in _cacheKeys.Keys)
            {
                _cache.Remove(key);
            }
            _cacheKeys.Clear();
        }
    }
}
