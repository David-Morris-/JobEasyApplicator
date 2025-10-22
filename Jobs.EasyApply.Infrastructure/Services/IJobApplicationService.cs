using System.Collections.Generic;
using System.Threading.Tasks;
using Jobs.EasyApply.Common.Models;

namespace Jobs.EasyApply.Infrastructure.Services
{
    public interface IJobApplicationService
    {
        /// <summary>
        /// Processes a job application with business logic
        /// </summary>
        /// <param name="job">The job listing to process</param>
        /// <returns>Result of the application process</returns>
        Task<bool> ProcessJobApplicationAsync(JobListing job);

        /// <summary>
        /// Gets application statistics
        /// </summary>
        /// <returns>Application statistics</returns>
        Task<ApplicationStats> GetApplicationStatsAsync();

        /// <summary>
        /// Gets all previously applied jobs
        /// </summary>
        /// <returns>List of all applied jobs</returns>
        Task<IEnumerable<AppliedJob>> GetAppliedJobsAsync();

        /// <summary>
        /// Gets applied jobs filtered by company name
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
        /// Gets successful job applications
        /// </summary>
        /// <returns>List of successful job applications</returns>
        Task<IEnumerable<AppliedJob>> GetSuccessfulApplicationsAsync();

        /// <summary>
        /// Gets failed job applications
        /// </summary>
        /// <returns>List of failed job applications</returns>
        Task<IEnumerable<AppliedJob>> GetFailedApplicationsAsync();

        /// <summary>
        /// Gets recent job applications with pagination
        /// </summary>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>Paginated list of recent job applications</returns>
        Task<IEnumerable<AppliedJob>> GetRecentApplicationsAsync(int pageNumber, int pageSize);

        /// <summary>
        /// Gets the count of previously applied jobs
        /// </summary>
        /// <returns>The number of applied jobs</returns>
        Task<int> GetAppliedJobsCountAsync();
    }
}
