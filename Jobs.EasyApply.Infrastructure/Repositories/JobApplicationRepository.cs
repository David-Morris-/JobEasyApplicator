using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jobs.EasyApply.Common.Models;
using Jobs.EasyApply.Infrastructure.Data;
using Jobs.EasyApply.Infrastructure.Repositories.Specifications;

namespace Jobs.EasyApply.Infrastructure.Repositories
{
    public class JobApplicationRepository : Repository<AppliedJob, int>, IJobApplicationRepository
    {
        private readonly IUnitOfWork _unitOfWork;

        public JobApplicationRepository(
            JobDbContext dbContext,
            IUnitOfWork unitOfWork) : base(dbContext)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        /// <summary>
        /// Checks if a job has already been applied to
        /// </summary>
        /// <param name="jobId">The LinkedIn job ID to check</param>
        /// <returns>True if the job was previously applied to, false otherwise</returns>
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
        /// Records a new job application
        /// </summary>
        /// <param name="appliedJob">The job application details to save</param>
        /// <returns>The saved AppliedJob entity</returns>
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
        /// Gets all previously applied jobs
        /// </summary>
        /// <returns>List of all applied jobs</returns>
        public async Task<IEnumerable<AppliedJob>> GetAppliedJobsAsync()
        {
            return await GetAllAsync();
        }

        /// <summary>
        /// Gets applied jobs based on specification criteria
        /// </summary>
        /// <param name="specification">The specification criteria to filter by</param>
        /// <returns>Filtered list of applied jobs</returns>
        public async Task<IEnumerable<AppliedJob>> GetAppliedJobsAsync(ISpecification<AppliedJob> specification = null)
        {
            return await GetAsync(specification);
        }

        /// <summary>
        /// Gets applied jobs by company name
        /// </summary>
        /// <param name="companyName">The company name to filter by</param>
        /// <returns>List of applied jobs for the specified company</returns>
        public async Task<IEnumerable<AppliedJob>> GetAppliedJobsByCompanyAsync(string companyName)
        {
            var specification = new JobApplicationByCompanySpecification(companyName);
            return await GetAsync(specification);
        }

        /// <summary>
        /// Gets applied jobs within a date range
        /// </summary>
        /// <param name="startDate">Start date for the filter</param>
        /// <param name="endDate">End date for the filter</param>
        /// <returns>List of applied jobs within the date range</returns>
        public async Task<IEnumerable<AppliedJob>> GetAppliedJobsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var specification = new JobApplicationByDateRangeSpecification(startDate, endDate);
            return await GetAsync(specification);
        }

        /// <summary>
        /// Gets successful vs failed application statistics
        /// </summary>
        /// <returns>Application statistics</returns>
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
        /// Gets the count of previously applied jobs
        /// </summary>
        /// <returns>The number of applied jobs</returns>
        public async Task<int> GetAppliedJobsCountAsync()
        {
            return await CountAsync();
        }

        /// <summary>
        /// Saves all pending changes to the database
        /// </summary>
        /// <returns>Number of state entries written to the database</returns>
        public async Task<int> SaveChangesAsync()
        {
            return await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Adds multiple job applications in a single operation
        /// </summary>
        /// <param name="appliedJobs">The job applications to add</param>
        /// <returns>The added job applications</returns>
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
        /// Updates multiple job applications in a single operation
        /// </summary>
        /// <param name="appliedJobs">The job applications to update</param>
        /// <returns>The updated job applications</returns>
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
        /// Deletes multiple job applications by their IDs
        /// </summary>
        /// <param name="ids">The job application IDs</param>
        /// <returns>Number of job applications deleted</returns>
        public new async Task<int> DeleteRangeAsync(IEnumerable<int> ids)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            return await DeleteRangeAsync(ids);
        }

        /// <summary>
        /// Checks if any job applications match the specification criteria
        /// </summary>
        /// <param name="specification">The specification criteria</param>
        /// <returns>True if any job applications match, false otherwise</returns>
        public override async Task<bool> AnyAsync(ISpecification<AppliedJob>? specification)
        {
            return await base.AnyAsync(specification);
        }

        /// <summary>
        /// Gets the first job application matching the specification criteria
        /// </summary>
        /// <param name="specification">The specification criteria</param>
        /// <returns>The first job application if found, null otherwise</returns>
        public override async Task<AppliedJob?> FirstOrDefaultAsync(ISpecification<AppliedJob>? specification)
        {
            return await base.FirstOrDefaultAsync(specification);
        }

        /// <summary>
        /// Gets applied jobs filtered by provider
        /// </summary>
        /// <param name="provider">The job provider to filter by</param>
        /// <returns>List of applied jobs for the specified provider</returns>
        public async Task<IEnumerable<AppliedJob>> GetAppliedJobsByProviderAsync(JobProvider provider)
        {
            var specification = new JobApplicationByProviderSpecification(provider);
            return await GetAsync(specification);
        }
    }
}
