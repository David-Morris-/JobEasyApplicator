using System.Collections.Generic;
using System.Threading.Tasks;
using Jobs.EasyApply.Common.Models;
using Jobs.EasyApply.Infrastructure.Repositories.Specifications;
using Jobs.EasyApply.Infrastructure.Repositories.Exceptions;

namespace Jobs.EasyApply.Infrastructure.Repositories
{
    /// <summary>
    /// Interface for job application repository operations with specialized methods for job application management
    /// </summary>
    public interface IJobApplicationRepository : IRepository<AppliedJob, int>
    {
        /// <summary>
        /// Checks if a job has already been applied to by job ID
        /// </summary>
        /// <param name="jobId">The unique job identifier from the job platform</param>
        /// <returns>True if the job was previously applied to, false otherwise</returns>
        /// <exception cref="ArgumentException">Thrown when jobId is null or empty</exception>
        Task<bool> IsJobPreviouslyAppliedAsync(string jobId);

        /// <summary>
        /// Records a new job application in the database
        /// </summary>
        /// <param name="appliedJob">The job application details to save</param>
        /// <returns>The saved AppliedJob entity with generated ID</returns>
        /// <exception cref="ArgumentNullException">Thrown when appliedJob is null</exception>
        /// <exception cref="RepositoryValidationException">Thrown when job application validation fails</exception>
        Task<AppliedJob> AddJobApplicationAsync(AppliedJob appliedJob);

        /// <summary>
        /// Gets all previously applied jobs from the database
        /// </summary>
        /// <returns>Collection of all applied jobs (excluding soft-deleted ones)</returns>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        Task<IEnumerable<AppliedJob>> GetAppliedJobsAsync();

        /// <summary>
        /// Gets applied jobs based on specification criteria
        /// </summary>
        /// <param name="specification">The specification criteria to filter by, null for all jobs</param>
        /// <returns>Filtered collection of applied jobs</returns>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        Task<IEnumerable<AppliedJob>> GetAppliedJobsAsync(ISpecification<AppliedJob> specification = null);

        /// <summary>
        /// Gets applied jobs filtered by company name
        /// </summary>
        /// <param name="companyName">The company name to filter by</param>
        /// <returns>Collection of applied jobs for the specified company</returns>
        /// <exception cref="ArgumentException">Thrown when companyName is null or empty</exception>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        Task<IEnumerable<AppliedJob>> GetAppliedJobsByCompanyAsync(string companyName);

        /// <summary>
        /// Gets applied jobs within a specified date range
        /// </summary>
        /// <param name="startDate">Start date for the filter (inclusive)</param>
        /// <param name="endDate">End date for the filter (inclusive)</param>
        /// <returns>Collection of applied jobs within the date range</returns>
        /// <exception cref="ArgumentException">Thrown when startDate is after endDate</exception>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        Task<IEnumerable<AppliedJob>> GetAppliedJobsByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets comprehensive application statistics including success/failure rates
        /// </summary>
        /// <returns>Application statistics object with total, successful, and failed counts</returns>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        Task<ApplicationStats> GetApplicationStatisticsAsync();

        /// <summary>
        /// Gets the total count of applied jobs
        /// </summary>
        /// <returns>The number of applied jobs (excluding soft-deleted ones)</returns>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        Task<int> GetAppliedJobsCountAsync();

        /// <summary>
        /// Saves all pending changes to the database through the unit of work
        /// </summary>
        /// <returns>Number of state entries written to the database</returns>
        /// <exception cref="RepositoryConnectionException">Thrown when database save operation fails</exception>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Adds multiple job applications in a single database operation
        /// </summary>
        /// <param name="appliedJobs">The job applications to add</param>
        /// <returns>The added job applications with generated IDs</returns>
        /// <exception cref="ArgumentNullException">Thrown when appliedJobs is null</exception>
        /// <exception cref="RepositoryValidationException">Thrown when any job application validation fails</exception>
        Task<IEnumerable<AppliedJob>> AddRangeAsync(IEnumerable<AppliedJob> appliedJobs);

        /// <summary>
        /// Updates multiple job applications in a single database operation
        /// </summary>
        /// <param name="appliedJobs">The job applications to update</param>
        /// <returns>The updated job applications</returns>
        /// <exception cref="ArgumentNullException">Thrown when appliedJobs is null</exception>
        /// <exception cref="RepositoryConnectionException">Thrown when database update operation fails</exception>
        Task<IEnumerable<AppliedJob>> UpdateRangeAsync(IEnumerable<AppliedJob> appliedJobs);

        /// <summary>
        /// Deletes multiple job applications by their IDs (hard delete)
        /// </summary>
        /// <param name="ids">The job application IDs to delete</param>
        /// <returns>Number of job applications deleted</returns>
        /// <exception cref="ArgumentNullException">Thrown when ids is null</exception>
        /// <exception cref="RepositoryConnectionException">Thrown when database delete operation fails</exception>
        Task<int> DeleteRangeAsync(IEnumerable<int> ids);

        /// <summary>
        /// Checks if any job applications match the specification criteria
        /// </summary>
        /// <param name="specification">The specification criteria, null for all jobs</param>
        /// <returns>True if any job applications match, false otherwise</returns>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        Task<bool> AnyAsync(ISpecification<AppliedJob>? specification);

        /// <summary>
        /// Gets the first job application matching the specification criteria
        /// </summary>
        /// <param name="specification">The specification criteria, null for first job</param>
        /// <returns>The first job application if found, null otherwise</returns>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        Task<AppliedJob?> FirstOrDefaultAsync(ISpecification<AppliedJob>? specification);

        /// <summary>
        /// Gets applied jobs filtered by job provider platform
        /// </summary>
        /// <param name="provider">The job provider platform to filter by</param>
        /// <returns>Collection of applied jobs for the specified provider</returns>
        /// <exception cref="RepositoryConnectionException">Thrown when database connection fails</exception>
        Task<IEnumerable<AppliedJob>> GetAppliedJobsByProviderAsync(JobProvider provider);
    }
}
