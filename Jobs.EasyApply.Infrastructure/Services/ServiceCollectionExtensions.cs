using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Jobs.EasyApply.Infrastructure.Data;
using Jobs.EasyApply.Infrastructure.Repositories;
using Jobs.EasyApply.Infrastructure.Repositories.Decorators;

namespace Jobs.EasyApply.Infrastructure.Services
{
    /// <summary>
    /// Extension methods for configuring infrastructure services in the dependency injection container
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds infrastructure services including database context, repositories, and services to the dependency injection container
        /// </summary>
        /// <param name="services">The service collection to add services to</param>
        /// <param name="connectionString">The database connection string</param>
        /// <returns>The service collection for method chaining</returns>
        /// <exception cref="ArgumentNullException">Thrown when services is null</exception>
        /// <exception cref="ArgumentException">Thrown when connectionString is null or empty</exception>
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string connectionString)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));

            // Register DbContext
            services.AddDbContext<JobDbContext>(options => options.UseSqlite(connectionString));

            // Register Base Repository implementation with logging
            services.AddScoped(typeof(Repository<,>), typeof(Repository<,>));

            // Register repository decorators (caching and logging)
            services.AddMemoryCache();
            services.AddScoped(typeof(IRepository<,>), typeof(CachingRepositoryDecorator<,>));
            services.AddScoped(typeof(IRepository<,>), typeof(LoggingRepositoryDecorator<,>));

            // Register Specific Repositories
            services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register Services
            services.AddScoped<IJobApplicationService, JobApplicationService>();

            return services;
        }
    }
}
