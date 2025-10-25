using Microsoft.AspNetCore.Mvc;
using Jobs.EasyApply.Common.Models;
using Jobs.EasyApply.Infrastructure.Services;
using Jobs.EasyApply.API.DTOs;

namespace Jobs.EasyApply.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly IJobApplicationService _service;

        public JobsController(IJobApplicationService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAppliedJobs()
        {
            var appliedJobs = await _service.GetAppliedJobsAsync();
            var jobDtos = appliedJobs.Select(job => new JobDTO
            {
                Id = job.Id,
                JobTitle = job.JobTitle,
                Company = job.Company,
                JobId = job.JobId,
                Url = job.Url,
                Provider = job.Provider.ToString(),
                AppliedDate = job.AppliedDate,
                Success = job.Success
            }).ToList();

            return Ok(jobDtos);
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetAppliedJobsCount()
        {
            var count = await _service.GetAppliedJobsCountAsync();
            return Ok(count);
        }

        [HttpPost]
        public async Task<IActionResult> AddJobApplication([FromBody] JobDTO jobDto)
        {
            try
            {
                var appliedJob = new AppliedJob
                {
                    JobTitle = jobDto.JobTitle,
                    Company = jobDto.Company,
                    JobId = jobDto.JobId,
                    Url = jobDto.Url,
                    AppliedDate = jobDto.AppliedDate,
                    Success = jobDto.Success
                };

                // Parse provider from string to enum, default to LinkedIn if parsing fails
                if (!Enum.TryParse<JobProvider>(jobDto.Provider, true, out var provider))
                {
                    provider = JobProvider.LinkedIn; // Default to LinkedIn
                }

                // Use the service to process the job application
                var result = await _service.ProcessJobApplicationAsync(new JobListing
                {
                    Title = jobDto.JobTitle,
                    Company = jobDto.Company,
                    JobId = jobDto.JobId,
                    Url = jobDto.Url,
                    Provider = provider
                });

                if (result)
                {
                    return Ok(new { Success = true, Message = "Job application added successfully" });
                }
                else
                {
                    return BadRequest(new { Success = false, Message = "Failed to add job application" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Internal server error", Error = ex.Message });
            }
        }

        [HttpGet("check/{jobId}")]
        public async Task<IActionResult> CheckJobPreviouslyApplied(string jobId)
        {
            try
            {
                var appliedJobs = await _service.GetAppliedJobsAsync();
                var previouslyApplied = appliedJobs.Any(job => job.JobId == jobId);
                return Ok(previouslyApplied);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Internal server error", Error = ex.Message });
            }
        }

        [HttpGet("company/{companyName}")]
        public async Task<IActionResult> GetAppliedJobsByCompany(string companyName)
        {
            try
            {
                var appliedJobs = await _service.GetAppliedJobsByCompanyAsync(companyName);
                var jobDtos = appliedJobs.Select(job => new JobDTO
                {
                    Id = job.Id,
                    JobTitle = job.JobTitle,
                    Company = job.Company,
                    JobId = job.JobId,
                    Url = job.Url,
                    Provider = job.Provider.ToString(),
                    AppliedDate = job.AppliedDate,
                    Success = job.Success
                }).ToList();

                return Ok(jobDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Internal server error", Error = ex.Message });
            }
        }

        [HttpGet("daterange")]
        public async Task<IActionResult> GetAppliedJobsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var appliedJobs = await _service.GetAppliedJobsByDateRangeAsync(startDate, endDate);
                var jobDtos = appliedJobs.Select(job => new JobDTO
                {
                    Id = job.Id,
                    JobTitle = job.JobTitle,
                    Company = job.Company,
                    JobId = job.JobId,
                    Url = job.Url,
                    Provider = job.Provider.ToString(),
                    AppliedDate = job.AppliedDate,
                    Success = job.Success
                }).ToList();

                return Ok(jobDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Internal server error", Error = ex.Message });
            }
        }

        [HttpGet("successful")]
        public async Task<IActionResult> GetSuccessfulApplications()
        {
            try
            {
                var appliedJobs = await _service.GetSuccessfulApplicationsAsync();
                var jobDtos = appliedJobs.Select(job => new JobDTO
                {
                    Id = job.Id,
                    JobTitle = job.JobTitle,
                    Company = job.Company,
                    JobId = job.JobId,
                    Url = job.Url,
                    Provider = job.Provider.ToString(),
                    AppliedDate = job.AppliedDate,
                    Success = job.Success
                }).ToList();

                return Ok(jobDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Internal server error", Error = ex.Message });
            }
        }

        [HttpGet("failed")]
        public async Task<IActionResult> GetFailedApplications()
        {
            try
            {
                var appliedJobs = await _service.GetFailedApplicationsAsync();
                var jobDtos = appliedJobs.Select(job => new JobDTO
                {
                    Id = job.Id,
                    JobTitle = job.JobTitle,
                    Company = job.Company,
                    JobId = job.JobId,
                    Url = job.Url,
                    Provider = job.Provider.ToString(),
                    AppliedDate = job.AppliedDate,
                    Success = job.Success
                }).ToList();

                return Ok(jobDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Internal server error", Error = ex.Message });
            }
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentApplications([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var appliedJobs = await _service.GetRecentApplicationsAsync(pageNumber, pageSize);
                var jobDtos = appliedJobs.Select(job => new JobDTO
                {
                    Id = job.Id,
                    JobTitle = job.JobTitle,
                    Company = job.Company,
                    JobId = job.JobId,
                    Url = job.Url,
                    Provider = job.Provider.ToString(),
                    AppliedDate = job.AppliedDate,
                    Success = job.Success
                }).ToList();

                return Ok(jobDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Internal server error", Error = ex.Message });
            }
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetApplicationStatistics()
        {
            try
            {
                var stats = await _service.GetApplicationStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Internal server error", Error = ex.Message });
            }
        }

        [HttpGet("test-connection")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                // Test database connectivity by attempting to retrieve applied jobs count
                var count = await _service.GetAppliedJobsCountAsync();
                return Ok(new { Success = true, Message = "Database connection successful", Count = count });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Message = "Database connection failed", Error = ex.Message });
            }
        }
    }
}
