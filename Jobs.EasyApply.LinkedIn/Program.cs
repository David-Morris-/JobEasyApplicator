using Microsoft.Extensions.Configuration;
using Serilog;
using Jobs.EasyApply.LinkedIn.Services;
using Jobs.EasyApply.Common.Models;
using System.Net.Http.Json;

namespace Jobs.EasyApply.LinkedIn
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            // Load configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            //Credentials from config
            var appSettings = configuration.Get<AppSettings>();
            if (appSettings?.Credentials == null)
            {
                Log.Error("Failed to load LinkedIn credentials from configuration");
                return;
            }

            var jobTitle = args.Length > 0 ? args[0] : appSettings.JobSearchParams.Title;
            var location = args.Length > 1 ? args[1] : appSettings.JobSearchParams.Location;

            Log.Information("Applying for jobs with title: {Title}, location: {Location}", jobTitle, location);

            // Set up HTTP client for API calls
            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:5070"); // Default API URL

            using var scraper = new JobScraper(jobTitle, location, appSettings.Credentials.Email, appSettings.Credentials.Password);

            try
            {
                var jobs = scraper.SearchJobs();
                foreach (var job in jobs)
                {
                    if (!string.IsNullOrWhiteSpace(job.Title) && !string.IsNullOrWhiteSpace(job.Company))
                    {
                        if (!await IsJobPreviouslyAppliedAsync(httpClient, job.JobId))
                        {
                            bool success = scraper.ApplyForJob(job);

                            var appliedJob = new AppliedJob
                            {
                                JobTitle = job.Title,
                                Company = job.Company,
                                JobId = job.JobId,
                                Url = job.Url,
                                AppliedDate = DateTime.UtcNow,
                                Success = success
                            };

                            await AddJobApplicationAsync(httpClient, appliedJob);

                            Log.Information("Applied for job - Title: {Title}, Company: {Company}, Url: {Url}, Success: {Success}",
                                job.Title, job.Company, job.Url, success);
                        }
                        else
                        {
                            Log.Information("Already applied for: {Title} at {Company}", job.Title, job.Company);
                        }
                    }
                    else
                    {
                        Log.Warning("Skipping job with incomplete details - Title: '{Title}', Company: '{Company}'", job.Title, job.Company);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during job application process");
            }

            Log.Information("Job application process completed.");
        }

        private static async Task<bool> IsJobPreviouslyAppliedAsync(HttpClient httpClient, string jobId)
        {
            try
            {
                // Check if job was previously applied via API
                var response = await httpClient.GetAsync($"api/jobs/check/{jobId}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<bool>();
                }
                else
                {
                    Log.Warning("API call failed for checking job {JobId}, status: {StatusCode}", jobId, response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error checking if job {JobId} was previously applied via API", jobId);
                return false;
            }
        }

        private static async Task AddJobApplicationAsync(HttpClient httpClient, AppliedJob appliedJob)
        {
            try
            {
                // Send job application to API
                var jobDto = new
                {
                    JobTitle = appliedJob.JobTitle,
                    Company = appliedJob.Company,
                    JobId = appliedJob.JobId,
                    Url = appliedJob.Url,
                    AppliedDate = appliedJob.AppliedDate,
                    Success = appliedJob.Success
                };

                var response = await httpClient.PostAsJsonAsync("api/jobs", jobDto);
                if (response.IsSuccessStatusCode)
                {
                    Log.Information("Successfully added job application via API for {JobTitle}", appliedJob.JobTitle);
                }
                else
                {
                    Log.Warning("API call failed for adding job application {JobTitle}, status: {StatusCode}", appliedJob.JobTitle, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error adding job application for {JobTitle} via API", appliedJob.JobTitle);
            }
        }


    }
}
