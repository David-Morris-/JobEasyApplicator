using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Serilog;
using Jobs.EasyApply.Common.Models;
using Jobs.EasyApply.Indeed.Utilities;
using Jobs.EasyApply.Indeed.Services;
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Jobs.EasyApply.Indeed.Services
{
    public class JobScraper : IJobScraper
    {
        private readonly IWebDriver _driver;
        private readonly string _jobTitle;
        private readonly string _location;

        private readonly HtmlScraper _htmlScraper;
        private readonly JobApplicator _jobApplicator;
        private readonly HttpClient _httpClient;

        private readonly IndeedLoginService _loginService;

        public JobScraper(string jobTitle, string location, string email, string password)
        {
            _jobTitle = jobTitle;
            _location = location;

            // Create IndeedLoginService with headless=false for debugging
            _loginService = new IndeedLoginService(email, password, headless: false, reuseProfile: true);

            _driver = _loginService.GetDriver();
            _htmlScraper = new HtmlScraper(_driver);
            _jobApplicator = new JobApplicator(_driver, _htmlScraper);
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5070");

            // Execute script to remove webdriver property
            try
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript("Object.defineProperty(navigator, 'webdriver', {get: () => undefined})");
            }
            catch (Exception ex)
            {
                Log.Warning("Could not execute anti-detection script: {Message}", ex.Message);
            }
        }

        private bool IsSessionValid()
        {
            try
            {
                // Try to access the current URL to check if session is still valid
                var currentUrl = _driver.Url;
                // Also try to access page title as additional validation
                var title = _driver.Title;
                return !string.IsNullOrEmpty(currentUrl) && !string.IsNullOrEmpty(title);
            }
            catch (WebDriverException)
            {
                return false;
            }
        }

        public async Task<List<JobListing>> SearchJobsAsync()
        {
            Log.Information("Starting job search for {Title} in {Location}", _jobTitle, _location);

            // Check if session is valid before starting
            if (!IsSessionValid())
            {
                Log.Error("WebDriver session is invalid. Cannot proceed with job search.");
                return [];
            }

            // Login to Indeed before searching for jobs
            if (!_loginService.Login())
            {
                Log.Error("Failed to login to Indeed. Cannot proceed with job search.");
                return [];
            }

            // Navigate to Indeed.com job search with Easy Apply filter using the specified URL
            var searchUrl = $"https://www.indeed.com/jobs?q={Uri.EscapeDataString(_jobTitle)}&l={Uri.EscapeDataString(_location)}&sc=0kf%3Aattr%28DSQF7%29%3B&fromage=14&vjk=somejobid";
            _driver.Navigate().GoToUrl(searchUrl);

            // Check for human verification checkbox
            _htmlScraper.CheckForHumanVerification();

            // Wait for jobs to load with multiple fallback selectors
            var jobCardFound = false;
            var usedSelector = "";

            var selectorsToTry = new[]
            {
                ".cardOutline.tapItem",
                "div.cardOutline.tapItem",
                "li[class*='css-1ac2h1w'][class*='eu4oa1w0']",
                "div.result.job_[data-jk]",
                "div[class*='result'][class*='job']",
                ".job-card",
                ".job-result"
            };

            foreach (var selector in selectorsToTry)
            {
                try
                {
                    Log.Debug("Trying selector: {Selector}", selector);
                    var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
                    wait.Until(d => d.FindElements(By.CssSelector(selector)).Count > 0);

                    Log.Information("Successfully found job cards using selector: {Selector}", selector);
                    jobCardFound = true;
                    usedSelector = selector;
                    break;
                }
                catch (WebDriverTimeoutException)
                {
                    Log.Debug("Selector '{Selector}' did not find any job cards within 5 seconds", selector);
                    continue;
                }
            }

            if (!jobCardFound)
            {
                Log.Warning("No job cards found with any selector. Debugging page structure...");

                // Debug: Log page title and URL
                Log.Information("Current URL: {Url}", _driver.Url);
                Log.Information("Page title: {Title}", _driver.Title);

                // Try some Indeed.com specific selectors
                var indeedSelectors = new[]
                {
                    "div[data-jk]",
                    "a[data-jk]",
                    "div[class*='job_seen_beacon']",
                    "td[id*='jobTitle']"
                };

                foreach (var selector in indeedSelectors)
                {
                    try
                    {
                        var elements = _driver.FindElements(By.CssSelector(selector));
                        if (elements.Count > 0)
                        {
                            Log.Information("SUCCESS: Found {Count} elements with selector '{Selector}'", elements.Count, selector);
                            usedSelector = selector;
                            jobCardFound = true;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Debug("Selector '{Selector}' failed: {Message}", selector, ex.Message);
                    }
                }

                if (!jobCardFound)
                {
                    Log.Error("No job cards found with any selector. Manual inspection required.");
                    return [];
                }
            }

            var jobs = new List<JobListing>();
            var maxPages = 20; // Maximum number of pages to prevent infinite loops
            var pageCount = 0;

            // Continue pagination until no Next button or max pages reached
            while (pageCount < maxPages)
            {
                pageCount++;
                var jobCards = _driver.FindElements(By.CssSelector(usedSelector));

                Log.Information("Found {Count} job cards on page {Page} using selector: {Selector}", jobCards.Count, pageCount, usedSelector);

                // Process all jobs on this page
                foreach (var card in jobCards)
                {
                    try
                    {
                        // Try multiple selectors for job title - Indeed.com specific
                        var titleElement = FindElementInCard(card, new[]
                        {
                            "span[id*='jobTitle']",
                            "a[data-jk] span[title]",
                            ".jcs-JobTitle span[title]",
                            "h2 .jcs-JobTitle",
                            ".jobtitle",
                            ".jobTitle"
                        });

                        // For Indeed.com, company name
                        var companyElement = FindElementInCard(card, new[]
                        {
                            "[data-testid='company-name']",
                            "[data-testid*='company']",
                            ".companyName",
                            ".company_location [data-testid='text-location']",
                            "span[class*='company']"
                        });

                        // Try multiple selectors for job link
                        var linkElement = FindElementInCard(card, new[]
                        {
                            "a[data-jk]",
                            ".jcs-JobTitle",
                            "h2 .jcs-JobTitle"
                        });

                        var title = titleElement?.Text?.Trim() ?? string.Empty;
                        var company = companyElement?.Text?.Trim() ?? string.Empty;

                        // Get job ID from data-jk attribute (try card first, then link)
                        var jobId = card.GetAttribute("data-jk") ??
                                   linkElement?.GetAttribute("data-jk") ??
                                   linkElement?.GetAttribute("id")?.Replace("job_", "") ??
                                   card.GetAttribute("id")?.Split('_').LastOrDefault() ??
                                   Guid.NewGuid().ToString();

                        var url = linkElement?.GetAttribute("href") ?? string.Empty;

                        // Skip if we don't have minimum required information
                        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(company))
                        {
                            Log.Debug("Skipping job card - missing title or company information");
                            continue;
                        }

                        // Check if already applied
                        if (await IsJobPreviouslyAppliedAsync(jobId))
                        {
                            Log.Information("Skipping already applied job: {Title} at {Company}", title, company);
                            continue;
                        }

                        // Check for "Applied" or "Application Submitted" text
                        if (card.Text.Contains("Applied", StringComparison.OrdinalIgnoreCase) ||
                            card.Text.Contains("Application Submitted", StringComparison.OrdinalIgnoreCase))
                        {
                            Log.Information("Skipping job with 'Applied' or 'Application Submitted' text: {Title} at {Company}", title, company);
                            continue;
                        }

                        // Check if job has Easy Apply indicator
                        var hasEasyApply = _htmlScraper.HasEasyApplyIndicator(card);

                        if (hasEasyApply)
                        {
                            jobs.Add(new JobListing
                            {
                                Title = title,
                                Company = company,
                                JobId = jobId,
                                Url = url,
                                Provider = JobProvider.Indeed,
                                PreviouslyApplied = false
                            });
                            Log.Information("Found Easy Apply job: {Title} at {Company} (ID: {JobId})", title, company, jobId);
                        }
                        else
                        {
                            Log.Debug("Skipping non-Easy Apply job: {Title} at {Company}", title, company);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warning("Error processing job card: {Message}", ex.Message);
                    }
                }

                // Look for the Next button in pagination nav
                try
                {
                    var nextButton = _driver.FindElement(By.CssSelector("a[aria-label='Next']"));

                    if (nextButton.Displayed && nextButton.Enabled &&
                        !string.Equals(nextButton.GetAttribute("aria-disabled"), "true", StringComparison.OrdinalIgnoreCase))
                    {
                        // Scroll to the Next button to ensure it's visible
                        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", nextButton);
                        Thread.Sleep(1000);

                        Log.Information("Clicking Next button for page {Page}", pageCount);
                        nextButton.Click();
                        Thread.Sleep(3000); // Wait for page to load

                        // Wait for jobs to load on new page
                        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
                        wait.Until(d => d.FindElements(By.CssSelector(usedSelector)).Count > 0);

                        // Check for human verification checkbox after page load
                        _htmlScraper.CheckForHumanVerification();
                    }
                    else
                    {
                        Log.Information("Next button not found or not clickable on page {Page}, stopping pagination", pageCount);
                        break;
                    }
                }
                catch (NoSuchElementException)
                {
                    Log.Information("Next button not found on page {Page}, stopping pagination", pageCount);
                    break;
                }
                catch (Exception ex)
                {
                    Log.Warning("Error during pagination on page {Page}: {Message}", pageCount, ex.Message);
                    break;
                }

                // Add a small delay between pages to be respectful to Indeed's servers
                Thread.Sleep(1000);
            }

            Log.Information("Job search completed. Found {Count} total jobs after {Pages} pages", jobs.Count, pageCount);
            return jobs;
        }

        private async Task<bool> IsJobPreviouslyAppliedAsync(string jobId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/jobs/check/{jobId}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<bool>();
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool ApplyForJob(JobListing job)
        {
            return _jobApplicator.ApplyForJob(job);
        }

        private IWebElement? FindElementInCard(IWebElement jobCard, string[] selectors)
        {
            foreach (var selector in selectors)
            {
                try
                {
                    var elements = jobCard.FindElements(By.CssSelector(selector));
                    var visibleElement = elements.FirstOrDefault(e => e.Displayed);
                    if (visibleElement != null)
                    {
                        return visibleElement;
                    }
                }
                catch (NoSuchElementException)
                {
                    continue;
                }
            }
            return null;
        }

        public void Dispose()
        {
            _loginService.Dispose();
            _httpClient.Dispose();
        }
    }
}
