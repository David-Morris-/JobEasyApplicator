using Microsoft.EntityFrameworkCore;
using Jobs.EasyApply.Common.Models;

namespace Jobs.EasyApply.Infrastructure.Data
{
    public class JobDbContext : DbContext
    {
        public JobDbContext(DbContextOptions<JobDbContext> options) : base(options) { }

        public DbSet<AppliedJob> AppliedJobs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Database configuration is handled through dependency injection in the API project
            // No fallback configuration needed here
        }
    }
}
