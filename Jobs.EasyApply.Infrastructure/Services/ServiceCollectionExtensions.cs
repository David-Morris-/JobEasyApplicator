using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Jobs.EasyApply.Infrastructure.Data;
using Jobs.EasyApply.Infrastructure.Repositories;

namespace Jobs.EasyApply.Infrastructure.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string connectionString)
        {
            // Register DbContext
            services.AddDbContext<JobDbContext>(options => options.UseSqlite(connectionString));

            // Register Repositories
            services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register Services
            services.AddScoped<IJobApplicationService, JobApplicationService>();

            return services;
        }
    }
}
