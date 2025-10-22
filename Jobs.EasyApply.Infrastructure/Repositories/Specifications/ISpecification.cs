using System.Linq.Expressions;

namespace Jobs.EasyApply.Infrastructure.Repositories.Specifications
{
    /// <summary>
    /// Specification pattern interface for defining query criteria
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    public interface ISpecification<TEntity> where TEntity : class
    {
        /// <summary>
        /// The criteria expression for filtering entities
        /// </summary>
        Expression<Func<TEntity, bool>> Criteria { get; }

        /// <summary>
        /// Include expressions for eager loading related entities
        /// </summary>
        List<Expression<Func<TEntity, object>>> Includes { get; }

        /// <summary>
        /// Order by expression for sorting entities
        /// </summary>
        Expression<Func<TEntity, object>> OrderBy { get; }

        /// <summary>
        /// Order by descending expression for sorting entities
        /// </summary>
        Expression<Func<TEntity, object>> OrderByDescending { get; }

        /// <summary>
        /// Number of entities to take (for pagination)
        /// </summary>
        int Take { get; }

        /// <summary>
        /// Number of entities to skip (for pagination)
        /// </summary>
        int Skip { get; }

        /// <summary>
        /// Whether paging is enabled
        /// </summary>
        bool IsPagingEnabled { get; }
    }
}
