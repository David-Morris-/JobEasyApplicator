using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jobs.EasyApply.Common.Models;
using Jobs.EasyApply.Infrastructure.Repositories;
using Jobs.EasyApply.Infrastructure.Repositories.Specifications;
using Microsoft.Extensions.Logging;

namespace Jobs.EasyApply.Infrastructure.Services
{
    /// <summary>
    /// Service implementation for job application business logic
    /// </summary>
    public class JobApplicationService : IJobApplicationService
    {
        private readonly IJobApplicationRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<JobApplicationService> _logger;

        public JobApplicationService(
            IJobApplicationRepository repository,
            IUnitOfWork unitOfWork,
            ILogger<JobApplicationService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Processes a job application with business logic
        /// </summary>
        /// <param name="job">The job listing to process</param>
        /// <returns>Result of the application process</returns>
        public async Task<bool> ProcessJobApplicationAsync(JobListing job)
        {
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }

            try
            {
                _logger.LogInformation("Processing job application for {JobTitle} at {Company}", job.Title, job.Company);

                // Check if job was previously applied to using specification pattern
                var previouslyApplied = await _repository.IsJobPreviouslyAppliedAsync(job.JobId);

                if (previouslyApplied)
                {
                    _logger.LogWarning("Job {JobId} was previously applied to. Skipping.", job.JobId);
                    return false;
                }

                // Create applied job record
                var appliedJob = new AppliedJob
                {
                    JobTitle = job.Title,
                    Company = job.Company,
                    JobId = job.JobId,
                    Url = job.Url,
                    AppliedDate = DateTime.UtcNow,
                    Success = true // This would be determined by the actual application process
                };

                // Save the application record using generic repository
                await _repository.AddAsync(appliedJob);
                await _repository.SaveChangesAsync();

                _logger.LogInformation("Successfully processed job application for {JobTitle}", job.Title);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing job application for {JobTitle}", job.Title);
                return false;
            }
        }

        /// <summary>
        /// Gets application statistics using the repository's enhanced method
        /// </summary>
        /// <returns>Application statistics</returns>
        public async Task<ApplicationStats> GetApplicationStatsAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving application statistics");
                return await _repository.GetApplicationStatisticsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving application statistics");
                return new Jobs.EasyApply.Common.Models.ApplicationStats
                {
                    TotalApplications = 0,
                    SuccessfulApplications = 0,
                    FailedApplications = 0
                };
            }
        }

        /// <summary>
        /// Gets all previously applied jobs
        /// </summary>
        /// <returns>List of all applied jobs</returns>
        public async Task<IEnumerable<AppliedJob>> GetAppliedJobsAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all applied jobs");
                return await _repository.GetAppliedJobsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving applied jobs");
                return new List<AppliedJob>();
            }
        }

        /// <summary>
        /// Gets applied jobs filtered by company name using specification pattern
        /// </summary>
        /// <param name="companyName">The company name to filter by</param>
        /// <returns>List of applied jobs for the specified company</returns>
        public async Task<IEnumerable<AppliedJob>> GetAppliedJobsByCompanyAsync(string companyName)
        {
            try
            {
                _logger.LogInformation("Retrieving applied jobs for company: {CompanyName}", companyName);
                return await _repository.GetAppliedJobsByCompanyAsync(companyName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving applied jobs for company {CompanyName}", companyName);
                return new List<AppliedJob>();
            }
        }

        /// <summary>
        /// Gets applied jobs within a date range using specification pattern
        /// </summary>
        /// <param name="startDate">Start date for the filter</param>
        /// <param name="endDate">End date for the filter</param>
        /// <returns>List of applied jobs within the date range</returns>
        public async Task<IEnumerable<AppliedJob>> GetAppliedJobsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.LogInformation("Retrieving applied jobs between {StartDate} and {EndDate}", startDate, endDate);
                return await _repository.GetAppliedJobsByDateRangeAsync(startDate, endDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving applied jobs for date range {StartDate} to {EndDate}", startDate, endDate);
                return new List<AppliedJob>();
            }
        }

        /// <summary>
        /// Gets successful job applications using specification pattern
        /// </summary>
        /// <returns>List of successful job applications</returns>
        public async Task<IEnumerable<AppliedJob>> GetSuccessfulApplicationsAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving successful job applications");
                var specification = new SuccessfulJobApplicationsSpecification();
                return await _repository.GetAppliedJobsAsync(specification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving successful applications");
                return new List<AppliedJob>();
            }
        }

        /// <summary>
        /// Gets failed job applications using specification pattern
        /// </summary>
        /// <returns>List of failed job applications</returns>
        public async Task<IEnumerable<AppliedJob>> GetFailedApplicationsAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving failed job applications");
                var specification = new FailedJobApplicationsSpecification();
                return await _repository.GetAppliedJobsAsync(specification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving failed applications");
                return new List<AppliedJob>();
            }
        }

        /// <summary>
        /// Gets recent job applications with pagination using specification pattern
        /// </summary>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>Paginated list of recent job applications</returns>
        public async Task<IEnumerable<AppliedJob>> GetRecentApplicationsAsync(int pageNumber, int pageSize)
        {
            try
            {
                _logger.LogInformation("Retrieving recent applications - Page {PageNumber}, Size {PageSize}", pageNumber, pageSize);
                var skip = (pageNumber - 1) * pageSize;
                var specification = new RecentJobApplicationsSpecification(skip, pageSize);
                return await _repository.GetAppliedJobsAsync(specification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent applications");
                return new List<AppliedJob>();
            }
        }

        /// <summary>
        /// Gets the count of previously applied jobs
        /// </summary>
        /// <returns>The number of applied jobs</returns>
        public async Task<int> GetAppliedJobsCountAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving applied jobs count");
                return await _repository.GetAppliedJobsCountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving applied jobs count");
                return 0;
            }
        }
    }
}
