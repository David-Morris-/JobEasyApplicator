using System.Linq.Expressions;

namespace Jobs.EasyApply.Infrastructure.Repositories.Specifications
{
    /// <summary>
    /// Base specification implementation with common functionality
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    public abstract class BaseSpecification<TEntity> : ISpecification<TEntity> where TEntity : class
    {
        protected BaseSpecification(Expression<Func<TEntity, bool>> criteria)
        {
            Criteria = criteria;
        }

        protected BaseSpecification()
        {
        }

        public Expression<Func<TEntity, bool>> Criteria { get; }
        public List<Expression<Func<TEntity, object>>> Includes { get; } = new List<Expression<Func<TEntity, object>>>();
        public Expression<Func<TEntity, object>> OrderBy { get; private set; }
        public Expression<Func<TEntity, object>> OrderByDescending { get; private set; }
        public int Take { get; private set; }
        public int Skip { get; private set; }
        public bool IsPagingEnabled { get; private set; } = false;

        /// <summary>
        /// Adds an include expression for eager loading
        /// </summary>
        /// <param name="includeExpression">The include expression</param>
        protected virtual void AddInclude(Expression<Func<TEntity, object>> includeExpression)
        {
            Includes.Add(includeExpression);
        }

        /// <summary>
        /// Adds multiple include expressions for eager loading
        /// </summary>
        /// <param name="includeExpressions">The include expressions</param>
        protected virtual void AddIncludes(params Expression<Func<TEntity, object>>[] includeExpressions)
        {
            foreach (var includeExpression in includeExpressions)
            {
                Includes.Add(includeExpression);
            }
        }

        /// <summary>
        /// Sets the order by expression for ascending sort
        /// </summary>
        /// <param name="orderByExpression">The order by expression</param>
        protected virtual void AddOrderBy(Expression<Func<TEntity, object>> orderByExpression)
        {
            OrderBy = orderByExpression;
        }

        /// <summary>
        /// Sets the order by expression for descending sort
        /// </summary>
        /// <param name="orderByDescendingExpression">The order by descending expression</param>
        protected virtual void AddOrderByDescending(Expression<Func<TEntity, object>> orderByDescendingExpression)
        {
            OrderByDescending = orderByDescendingExpression;
        }

        /// <summary>
        /// Applies paging to the specification
        /// </summary>
        /// <param name="skip">Number of entities to skip</param>
        /// <param name="take">Number of entities to take</param>
        protected virtual void ApplyPaging(int skip, int take)
        {
            Skip = skip;
            Take = take;
            IsPagingEnabled = true;
        }
    }
}
