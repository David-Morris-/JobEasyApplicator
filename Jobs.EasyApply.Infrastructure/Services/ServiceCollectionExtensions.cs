using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Jobs.EasyApply.Infrastructure.Data;
using Jobs.EasyApply.Infrastructure.Repositories;
using Jobs.EasyApply.Infrastructure.Repositories.Decorators;

namespace Jobs.EasyApply.Infrastructure.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string connectionString)
        {
            // Register DbContext
            services.AddDbContext<JobDbContext>(options => options.UseSqlite(connectionString));

            // Register Base Repository (decorated with logging)
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
