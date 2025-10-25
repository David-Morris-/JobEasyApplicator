using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jobs.EasyApply.Common.Models;
using Jobs.EasyApply.Infrastructure.Data;
using Jobs.EasyApply.Infrastructure.Repositories.Specifications;
using Jobs.EasyApply.Infrastructure.Repositories.Exceptions;
using Microsoft.Extensions.Logging;

namespace Jobs.EasyApply.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for AppliedJob entities with specialized job application operations
    /// </summary>
    public class JobApplicationRepository : Repository<AppliedJob, int>, IJobApplicationRepository
    {
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Initializes a new instance of the JobApplicationRepository
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="unitOfWork">The unit of work for transaction management</param>
        /// <param name="logger">The logger for repository operations</param>
        public JobApplicationRepository(
            JobDbContext dbContext,
            IUnitOfWork unitOfWork,
            ILogger<JobApplicationRepository> logger) : base(dbContext, logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        /// <summary>
        /// Checks if a job has already been applied to by job ID
        /// </summary>
        /// <param name="jobId">The unique job identifier from the job platform</param>
        /// <returns>True if the job was previously applied to, false otherwise</returns>
        /// <exception cref="ArgumentException">Thrown when jobId is null or empty</exception>
        public async Task<bool> IsJobPreviouslyAppliedAsync(string jobId)
        {
            if (string.IsNullOrWhiteSpace(jobId))
            {
                return false;
            }

            var specification = new JobApplicationByJobIdSpecification(jobId);
            return await CountAsync(specification) > 0;
        }

        /// <summary>
        /// Records a new job application in the database
        /// </summary>
        /// <param name="appliedJob">The job application details to save</param>
        /// <returns>The saved AppliedJob entity with generated ID</returns>
        /// <exception cref="ArgumentNullException">Thrown when appliedJob is null</exception>
        /// <exception cref="RepositoryValidationException">Thrown when job application validation fails</exception>
        public async Task<AppliedJob> AddJobApplicationAsync(AppliedJob appliedJob)
        {
            if (appliedJob == null)
            {
                throw new ArgumentNullException(nameof(appliedJob));
            }

            // Ensure the AppliedDate is set
            if (appliedJob.AppliedDate == default)
            {
                appliedJob.AppliedDate = DateTime.UtcNow;
            }

            return await AddAsync(appliedJob);
        }

        /// <summary>
        /// Gets all previously applied jobs from the database
        /// </summary>
        /// <returns>Collection of all applied jobs (excluding soft-deleted ones)</returns>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        public async Task<IEnumerable<AppliedJob>> GetAppliedJobsAsync()
        {
            return await GetAllAsync();
        }

        /// <summary>
        /// Gets applied jobs based on specification criteria
        /// </summary>
        /// <param name="specification">The specification criteria to filter by, null for all jobs</param>
        /// <returns>Filtered collection of applied jobs</returns>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        public async Task<IEnumerable<AppliedJob>> GetAppliedJobsAsync(ISpecification<AppliedJob> specification = null)
        {
            return await GetAsync(specification);
        }

        /// <summary>
        /// Gets applied jobs filtered by company name
        /// </summary>
        /// <param name="companyName">The company name to filter by</param>
        /// <returns>Collection of applied jobs for the specified company</returns>
        /// <exception cref="ArgumentException">Thrown when companyName is null or empty</exception>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        public async Task<IEnumerable<AppliedJob>> GetAppliedJobsByCompanyAsync(string companyName)
        {
            var specification = new JobApplicationByCompanySpecification(companyName);
            return await GetAsync(specification);
        }

        /// <summary>
        /// Gets applied jobs within a specified date range
        /// </summary>
        /// <param name="startDate">Start date for the filter (inclusive)</param>
        /// <param name="endDate">End date for the filter (inclusive)</param>
        /// <returns>Collection of applied jobs within the date range</returns>
        /// <exception cref="ArgumentException">Thrown when startDate is after endDate</exception>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        public async Task<IEnumerable<AppliedJob>> GetAppliedJobsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var specification = new JobApplicationByDateRangeSpecification(startDate, endDate);
            return await GetAsync(specification);
        }

        /// <summary>
        /// Gets comprehensive application statistics including success/failure rates
        /// </summary>
        /// <returns>Application statistics object with total, successful, and failed counts</returns>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        public async Task<ApplicationStats> GetApplicationStatisticsAsync()
        {
            var allJobs = await GetAllAsync();

            var totalApplications = allJobs.Count();
            var successfulApplications = allJobs.Count(job => job.Success);
            var failedApplications = totalApplications - successfulApplications;

            return new ApplicationStats
            {
                TotalApplications = totalApplications,
                SuccessfulApplications = successfulApplications,
                FailedApplications = failedApplications
            };
        }

        /// <summary>
        /// Gets the total count of applied jobs
        /// </summary>
        /// <returns>The number of applied jobs (excluding soft-deleted ones)</returns>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        public async Task<int> GetAppliedJobsCountAsync()
        {
            return await CountAsync();
        }

        /// <summary>
        /// Saves all pending changes to the database through the unit of work
        /// </summary>
        /// <returns>Number of state entries written to the database</returns>
        /// <exception cref="RepositoryConnectionException">Thrown when database save operation fails</exception>
        public async Task<int> SaveChangesAsync()
        {
            return await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Adds multiple job applications in a single database operation
        /// </summary>
        /// <param name="appliedJobs">The job applications to add</param>
        /// <returns>The added job applications with generated IDs</returns>
        /// <exception cref="ArgumentNullException">Thrown when appliedJobs is null</exception>
        /// <exception cref="RepositoryValidationException">Thrown when any job application validation fails</exception>
        public new async Task<IEnumerable<AppliedJob>> AddRangeAsync(IEnumerable<AppliedJob> appliedJobs)
        {
            if (appliedJobs == null)
                throw new ArgumentNullException(nameof(appliedJobs));

            var jobsList = appliedJobs.ToList();
            if (!jobsList.Any())
                return jobsList;

            // Ensure AppliedDate is set for all jobs
            foreach (var job in jobsList)
            {
                if (job.AppliedDate == default)
                    job.AppliedDate = DateTime.UtcNow;
            }

            return await AddRangeAsync(jobsList);
        }

        /// <summary>
        /// Updates multiple job applications in a single database operation
        /// </summary>
        /// <param name="appliedJobs">The job applications to update</param>
        /// <returns>The updated job applications</returns>
        /// <exception cref="ArgumentNullException">Thrown when appliedJobs is null</exception>
        /// <exception cref="RepositoryConnectionException">Thrown when database update operation fails</exception>
        public new async Task<IEnumerable<AppliedJob>> UpdateRangeAsync(IEnumerable<AppliedJob> appliedJobs)
        {
            if (appliedJobs == null)
                throw new ArgumentNullException(nameof(appliedJobs));

            var jobsList = appliedJobs.ToList();
            if (!jobsList.Any())
                return jobsList;

            return await UpdateRangeAsync(jobsList);
        }

        /// <summary>
        /// Deletes multiple job applications by their IDs (hard delete)
        /// </summary>
        /// <param name="ids">The job application IDs to delete</param>
        /// <returns>Number of job applications deleted</returns>
        /// <exception cref="ArgumentNullException">Thrown when ids is null</exception>
        /// <exception cref="RepositoryConnectionException">Thrown when database delete operation fails</exception>
        public new async Task<int> DeleteRangeAsync(IEnumerable<int> ids)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            return await DeleteRangeAsync(ids);
        }

        /// <summary>
        /// Checks if any job applications match the specification criteria
        /// </summary>
        /// <param name="specification">The specification criteria, null for all jobs</param>
        /// <returns>True if any job applications match, false otherwise</returns>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        public override async Task<bool> AnyAsync(ISpecification<AppliedJob>? specification)
        {
            return await base.AnyAsync(specification);
        }

        /// <summary>
        /// Gets the first job application matching the specification criteria
        /// </summary>
        /// <param name="specification">The specification criteria, null for first job</param>
        /// <returns>The first job application if found, null otherwise</returns>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        public override async Task<AppliedJob?> FirstOrDefaultAsync(ISpecification<AppliedJob>? specification)
        {
            return await base.FirstOrDefaultAsync(specification);
        }

        /// <summary>
        /// Gets applied jobs filtered by job provider platform
        /// </summary>
        /// <param name="provider">The job provider platform to filter by</param>
        /// <returns>Collection of applied jobs for the specified provider</returns>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        public async Task<IEnumerable<AppliedJob>> GetAppliedJobsByProviderAsync(JobProvider provider)
        {
            var specification = new JobApplicationByProviderSpecification(provider);
            return await GetAsync(specification);
        }
    }
}
