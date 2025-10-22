using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Serilog;
using Jobs.EasyApply.Common.Models;
using Jobs.EasyApply.Utilities;

namespace Jobs.EasyApply.Services
{
    public class JobScraper : IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly string _jobTitle;
        private readonly string _location;
        private readonly string _email;
        private readonly string _password;
        private readonly HtmlScraper _htmlScraper;
        private readonly JobApplicator _jobApplicator;

        public JobScraper(string jobTitle, string location, string email, string password)
        {
            _jobTitle = jobTitle;
            _location = location;
            _email = email;
            _password = password;

            var options = new ChromeOptions();
            options.AddArgument("--start-maximized"); // Start browser maximized
            // options.AddArgument("--headless"); // Run with visible browser for manual login
            _driver = new ChromeDriver(options);
            _htmlScraper = new HtmlScraper(_driver);
            _jobApplicator = new JobApplicator(_driver, _htmlScraper);
        }

        public List<JobListing> SearchJobs()
        {
            Log.Information("Starting job search for {Title} in {Location}", _jobTitle, _location);

            _driver.Navigate().GoToUrl("https://www.linkedin.com/login");

            // Auto login
            Log.Information("Logging in automatically...");
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));
            try
            {
                var emailField = wait.Until(d => d.FindElement(By.Id("username")));
                emailField.SendKeys(_email);

                var passwordField = _driver.FindElement(By.Id("password"));
                passwordField.SendKeys(_password);

                var signInButton = _driver.FindElement(By.CssSelector("button[type='submit']"));
                signInButton.Click();

                // Wait for redirect or feed to load
                wait.Until(d => d.Url.Contains("/feed") || d.Url.Contains("/jobs"));
                Log.Information("Login successful.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to auto login");
                return [];
            }

            //f_AL=true - filter for easy apply jobs
            _driver.Navigate().GoToUrl($"https://www.linkedin.com/jobs/search/?f_AL=true&keywords={Uri.EscapeDataString(_jobTitle)}&location={Uri.EscapeDataString(_location)}");

            // Wait for jobs to load
            try
            {
                wait.Until(d => d.FindElements(By.CssSelector("div[data-job-id]")).Count > 0);
            }
            catch (WebDriverTimeoutException)
            {
                Log.Warning("No job cards found within timeout, job search may have failed.");
                return [];
            }

            var jobs = new List<JobListing>();
            var previousJobCount = 0;
            var maxIterations = 20; // Maximum number of scroll attempts to prevent infinite loops
            var iterationCount = 0;

            // Continue loading more jobs until no new jobs are found or max iterations reached
            while (iterationCount < maxIterations)
            {
                iterationCount++;
                var jobCards = _driver.FindElements(By.CssSelector("div[data-job-id]"));

                Log.Information("Found {Count} job cards on iteration {Iteration}", jobCards.Count, iterationCount);

                // Process new jobs that haven't been processed yet
                var newJobsFound = false;
                foreach (var card in jobCards.Skip(previousJobCount))
                {
                    try
                    {
                        var innerHtml = card.GetAttribute("innerHTML");
                        var titleElement = card.FindElement(By.CssSelector("strong"));
                        var companyElement = card.FindElement(By.CssSelector(".artdeco-entity-lockup__subtitle.ember-view"));
                        var linkElement = card.FindElement(By.CssSelector("a"));
                        var title = titleElement?.Text ?? string.Empty;
                        var company = companyElement?.Text ?? string.Empty;
                        var jobId = card.GetAttribute("data-job-id") ?? string.Empty;
                        var url = linkElement?.GetAttribute("href") ?? string.Empty;

                        // Check if job was previously applied for
                        var previouslyApplied = innerHtml != null && innerHtml.Contains("applied", StringComparison.OrdinalIgnoreCase);

                        jobs.Add(new JobListing { Title = title, Company = company, JobId = jobId, Url = url, PreviouslyApplied = previouslyApplied });
                        newJobsFound = true;
                    }
                    catch (NoSuchElementException ex)
                    {
                        Log.Warning("Skipping job card due to missing element: {Message}", ex.Message);
                    }
                }

                previousJobCount = jobCards.Count;

                // If no new jobs were found in this iteration, we've likely reached the end
                if (!newJobsFound)
                {
                    Log.Information("No new jobs found in iteration {Iteration}, stopping search", iterationCount);
                    break;
                }

                // Scroll down to load more jobs (LinkedIn uses infinite scroll)
                try
                {
                    var lastJobCard = jobCards.LastOrDefault();
                    if (lastJobCard != null)
                    {
                        // Scroll to the last job card to trigger loading of more jobs
                        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", lastJobCard);
                        Thread.Sleep(2000); // Wait for potential new jobs to load

                        // Check if more jobs have loaded
                        var currentJobCount = _driver.FindElements(By.CssSelector("div[data-job-id]")).Count;
                        if (currentJobCount <= previousJobCount)
                        {
                            Log.Information("No additional jobs loaded after scroll, stopping search");
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning("Error during scroll operation: {Message}", ex.Message);
                    break;
                }

                // Add a small delay between iterations to be respectful to LinkedIn's servers
                Thread.Sleep(1000);
            }

            Log.Information("Job search completed. Found {Count} total jobs after {Iterations} iterations", jobs.Count, iterationCount);
            return jobs;
        }

        public bool ApplyForJob(JobListing job)
        {
            return _jobApplicator.ApplyForJob(job);
        }

        public void Dispose()
        {
            _driver.Quit();
        }
    }
}
