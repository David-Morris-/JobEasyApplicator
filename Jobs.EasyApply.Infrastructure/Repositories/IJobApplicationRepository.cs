using System.Collections.Generic;
using System.Threading.Tasks;
using Jobs.EasyApply.Common.Models;
using Jobs.EasyApply.Infrastructure.Repositories.Specifications;

namespace Jobs.EasyApply.Infrastructure.Repositories
{
    public interface IJobApplicationRepository : IRepository<AppliedJob, int>
    {
        /// <summary>
        /// Checks if a job has already been applied to
        /// </summary>
        /// <param name="jobId">The LinkedIn job ID to check</param>
        /// <returns>True if the job was previously applied to, false otherwise</returns>
        Task<bool> IsJobPreviouslyAppliedAsync(string jobId);

        /// <summary>
        /// Gets applied jobs based on specification criteria
        /// </summary>
        /// <param name="specification">The specification criteria to filter by</param>
        /// <returns>Filtered list of applied jobs</returns>
        Task<IEnumerable<AppliedJob>> GetAppliedJobsAsync(ISpecification<AppliedJob> specification = null);

        /// <summary>
        /// Gets applied jobs by company name
        /// </summary>
        /// <param name="companyName">The company name to filter by</param>
        /// <returns>List of applied jobs for the specified company</returns>
        Task<IEnumerable<AppliedJob>> GetAppliedJobsByCompanyAsync(string companyName);

        /// <summary>
        /// Gets applied jobs within a date range
        /// </summary>
        /// <param name="startDate">Start date for the filter</param>
        /// <param name="endDate">End date for the filter</param>
        /// <returns>List of applied jobs within the date range</returns>
        Task<IEnumerable<AppliedJob>> GetAppliedJobsByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets successful vs failed application statistics
        /// </summary>
        /// <returns>Application statistics</returns>
        Task<ApplicationStats> GetApplicationStatisticsAsync();

        /// <summary>
        /// Saves all changes to the database
        /// </summary>
        /// <returns>The number of state entries written to the database</returns>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Gets the count of applied jobs
        /// </summary>
        /// <returns>The total count of applied jobs</returns>
        Task<int> GetAppliedJobsCountAsync();
    }
}
