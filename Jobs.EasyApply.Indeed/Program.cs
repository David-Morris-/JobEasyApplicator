using Microsoft.Extensions.Configuration;
using Serilog;
using Jobs.EasyApply.Indeed.Services;
using Jobs.EasyApply.Common.Models;
using System.Net.Http.Json;

namespace Jobs.EasyApply.Indeed
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
                .AddEnvironmentVariables()
                .Build();

            // Credentials from config
            var appSettings = configuration.Get<AppSettings>();
            if (appSettings?.Credentials == null)
            {
                Log.Error("Failed to load Indeed credentials from configuration");
                return;
            }

            var jobTitle = args.Length > 0 ? args[0] : appSettings.JobSearchParams.Title;
            var location = args.Length > 1 ? args[1] : appSettings.JobSearchParams.Location;

            Log.Information("Applying for Indeed Easy Apply jobs with title: {Title}, location: {Location}", jobTitle, location);

            // Initialize counters
            int totalJobsFound = 0;
            int totalApplied = 0;

            // Set up HTTP client for API calls
            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:5070"); // Default API URL

            // Test API connectivity before proceeding
            Log.Information("Testing API connectivity...");
            if (!await TestApiConnectionAsync(httpClient))
            {
                Log.Error("Failed to connect to API. Please ensure the API is running on http://localhost:5070");
                return;
            }
            Log.Information("API connectivity confirmed. Proceeding with job search...");

            var factory = new IndeedJobServiceFactory();
            using var scraper = factory.CreateJobScraper(jobTitle, location, appSettings.Credentials.Email, appSettings.Credentials.Password);

            try
            {
                var jobs = await scraper.SearchJobsAsync();
                totalJobsFound = jobs.Count();

                foreach (var job in jobs)
                {
                    if (!string.IsNullOrWhiteSpace(job.Title) && !string.IsNullOrWhiteSpace(job.Company))
                    {
                        totalApplied++;
                        try
                        {
                            bool success = scraper.ApplyForJob(job);

                            var appliedJob = new AppliedJob
                            {
                                JobTitle = job.Title,
                                Company = job.Company,
                                JobId = job.JobId,
                                Url = job.Url,
                                AppliedDate = DateTime.UtcNow,
                                Success = success,
                                Provider = JobProvider.Indeed
                            };

                            await AddJobApplicationAsync(httpClient, appliedJob);

                            Log.Information("Applied for job - Title: {Title}, Company: {Company}, Url: {Url}, Success: {Success}",
                                job.Title, job.Company, job.Url, success);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Error applying for job: {Title} at {Company}", job.Title, job.Company);
                            // Continue with next job instead of stopping the entire process
                            continue;
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

            // Log summary
            Log.Information("Job application process completed. Total jobs found: {TotalJobsFound}, Total applied for: {TotalApplied}",
                totalJobsFound, totalApplied);
        }

        private static async Task<bool> TestApiConnectionAsync(HttpClient httpClient)
        {
            try
            {
                var response = await httpClient.GetAsync("api/jobs/test-connection");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    Log.Information("API connection test response: {Response}", jsonString);

                    // Simple string-based check for success (case-insensitive)
                    if (jsonString.Contains("\"success\":true") || jsonString.Contains("'success':true") ||
                        jsonString.Contains("\"Success\":true") || jsonString.Contains("'Success':true"))
                    {
                        Log.Information("API connection test successful");
                        return true;
                    }
                    else
                    {
                        Log.Warning("API connection test failed - response indicates failure");
                        return false;
                    }
                }
                else
                {
                    Log.Warning("API connection test failed with status: {StatusCode}", response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error testing API connection");
                return false;
            }
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
                    Success = appliedJob.Success,
                    Provider = appliedJob.Provider
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
