using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Serilog;
using Jobs.EasyApply.Common.Models;
using Jobs.EasyApply.Dice.Utilities;
using Jobs.EasyApply.Dice.Services;
using System;
using System.Net.Http.Json;

namespace Jobs.EasyApply.Dice.Services
{
    public class JobScraper : IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly string _jobTitle;
        private readonly string _location;

        private readonly HtmlScraper _htmlScraper;
        private readonly JobApplicator _jobApplicator;
        private readonly HttpClient _httpClient;

        private readonly DiceLoginService _loginService;

        public JobScraper(string jobTitle, string location, string email, string password)
        {
            _jobTitle = jobTitle;
            _location = location;

            // Create DiceLoginService with headless=false for debugging
            _loginService = new DiceLoginService(email, password, headless: false, reuseProfile: true);

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

            // Login to Dice before searching for jobs
            if (!_loginService.Login())
            {
                Log.Error("Failed to login to Dice. Cannot proceed with job search.");
                return [];
            }

            // Navigate to Dice.com job search with Easy Apply filter using the specified URL
            var searchUrl = $"https://www.dice.com/jobs?filters.workplaceTypes=Remote&applyType=easy_apply&filters.easyApply=true&q={Uri.EscapeDataString(_jobTitle)}";
            _driver.Navigate().GoToUrl(searchUrl);

            // Wait for jobs to load with multiple fallback selectors
            var jobCardFound = false;
            var usedSelector = "";

            var selectorsToTry = new[]
            {
                "div[data-cy='card-job']",
                "div[data-testid='card-job']",
                "div[class*='job-card']",
                "div[class*='job-result']",
                "div[class*='search-result']",
                "article[data-cy='card-job']",
                "article[data-testid='card-job']",
                "[data-cy*='job']",
                "[data-testid*='job']",
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

                // Debug: Try alternative selectors and log what we find
                DebugJobCardSelectors();

                // Instead of pausing, try some common Dice.com selectors
                Log.Information("Trying common Dice.com selectors...");

                var diceSelectors = new[]
                {
                    "div[data-cy='job-card']",
                    "div[data-cy='job-result']",
                    "div[data-cy='search-result']",
                    "div[data-testid='job-card']",
                    "div[data-testid='job-result']",
                    "div[class*='job-card']",
                    "div[class*='job-item']",
                    "div[class*='result']",
                    "article[class*='job']",
                    "div[role='article']"
                };

                foreach (var selector in diceSelectors)
                {
                    try
                    {
                        var elements = _driver.FindElements(By.CssSelector(selector));
                        if (elements.Count > 0)
                        {
                            Log.Information("SUCCESS: Found {Count} elements with selector '{Selector}'", elements.Count, selector);
                            usedSelector = selector;
                            jobCardFound = true;

                            // Log details of first element
                            var firstElement = elements.First();
                            Log.Information("First element details: Tag={Tag}, Classes={Classes}, Attributes={Attributes}",
                                firstElement.TagName, firstElement.GetAttribute("class"), GetElementAttributes(firstElement));

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
                    Log.Error("Please run with visible browser and inspect the page structure manually.");
                    return [];
                }
            }

            var jobs = new List<JobListing>();
            var previousJobCount = 0;
            var maxIterations = 20; // Maximum number of scroll attempts to prevent infinite loops
            var iterationCount = 0;

            // Continue loading more jobs until no new jobs are found or max iterations reached
            while (iterationCount < maxIterations)
            {
                iterationCount++;
                var jobCards = _driver.FindElements(By.CssSelector(usedSelector));

                Log.Information("Found {Count} job cards on iteration {Iteration} using selector: {Selector}", jobCards.Count, iterationCount, usedSelector);

                // Process new jobs that haven't been processed yet
                var newJobsFound = false;
                foreach (var card in jobCards.Skip(previousJobCount))
                {
                    try
                    {
                        // Debug the first few job cards to understand their structure
                        if (previousJobCount == 0 && jobs.Count < 3)
                        {
                            _htmlScraper.DebugJobCardStructure(card);
                        }

                        // Try multiple selectors for job title - Dice.com specific
                        var titleElement = FindElementInCard(card, new[]
                        {
                            "[data-testid='job-search-job-detail-link']",
                            "a[data-testid*='job-detail-link']",
                            "a[data-testid*='job-title']",
                            "a[class*='job-title']",
                            "a[href*='/job']", "a[href*='job']"
                        });

                        // For Dice.com, company name is usually the first text in the job card
                        // Try to find it in the card text or look for specific company elements
                        var companyElement = FindElementInCard(card, new[]
                        {
                            "[data-testid*='company']",
                            "[data-cy*='company']",
                            ".company",
                            "span[class*='company']",
                            "div[class*='company']"
                        });

                        // Try multiple selectors for job link - Dice.com specific
                        var linkElement = FindElementInCard(card, new[]
                        {
                            "[data-testid='job-search-job-detail-link']",
                            "a[data-testid*='job-detail-link']",
                            "a[data-testid*='job-title']",
                            "a[href*='/job']", "a[href*='job']"
                        });

                        var title = titleElement?.Text?.Trim() ?? string.Empty;
                        var company = companyElement?.Text?.Trim() ?? string.Empty;
                        var jobId = card.GetAttribute("data-testid")?.Replace("job-", "") ??
                                   card.GetAttribute("data-cy")?.Replace("job-", "") ??
                                   card.GetAttribute("id")?.Replace("job-", "") ??
                                   Guid.NewGuid().ToString();
                        var url = linkElement?.GetAttribute("href") ?? string.Empty;

                        // If no company element found, try to extract company from the card text
                        if (string.IsNullOrWhiteSpace(company))
                        {
                            var cardText = card.Text;
                            var lines = cardText.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
                            if (lines.Length > 0)
                            {
                                // Company is usually the first non-empty line or the line before job title
                                var firstLine = lines[0].Trim();
                                if (!string.IsNullOrWhiteSpace(firstLine) && firstLine.Length < 100)
                                {
                                    company = firstLine;
                                }
                            }
                        }

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

                        // Check for "Application Submitted" text
                        if (card.Text.Contains("Application Submitted", StringComparison.OrdinalIgnoreCase))
                        {
                            Log.Information("Skipping job with 'Application Submitted' text: {Title} at {Company}", title, company);
                            continue;
                        }

                        // Check if job has Easy Apply indicator (since we're filtering for Easy Apply jobs)
                        var hasEasyApply = _htmlScraper.HasEasyApplyIndicator(card);

                        if (hasEasyApply)
                        {
                            // Check if job was previously applied for (Dice doesn't show this in search results)
                            var previouslyApplied = false;

                            jobs.Add(new JobListing
                            {
                                Title = title,
                                Company = company,
                                JobId = jobId,
                                Url = url,
                                Provider = JobProvider.Dice,
                                PreviouslyApplied = previouslyApplied
                            });
                            newJobsFound = true;
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

                previousJobCount = jobCards.Count;

                // If no new jobs were found in this iteration, we've likely reached the end
                if (!newJobsFound)
                {
                    Log.Information("No new jobs found in iteration {Iteration}, stopping search", iterationCount);
                    break;
                }

                // Scroll down to load more jobs (Dice uses infinite scroll)
                try
                {
                    var lastJobCard = jobCards.LastOrDefault();
                    if (lastJobCard != null)
                    {
                        // Scroll to the last job card to trigger loading of more jobs
                        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", lastJobCard);
                        Thread.Sleep(2000); // Wait for potential new jobs to load

                        // Check if more jobs have loaded using the same selector
                        var currentJobCount = _driver.FindElements(By.CssSelector(usedSelector)).Count;
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

                // Add a small delay between iterations to be respectful to Dice's servers
                Thread.Sleep(1000);
            }

            Log.Information("Job search completed. Found {Count} total jobs after {Iterations} iterations", jobs.Count, iterationCount);
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



        private void DebugJobCardSelectors()
        {
            Log.Information("=== DEBUGGING JOB CARD SELECTORS ===");

            // Try various selectors and log results
            var selectorsToTest = new[]
            {
                "div[data-cy='card-job']",
                "div[data-testid='card-job']",
                "div[class*='job-card']",
                "div[class*='job-result']",
                "div[class*='search-result']",
                "article[data-cy='card-job']",
                "article[data-testid='card-job']",
                "div[data-cy='job-card']",
                "div[data-testid='job-card']",
                "[data-cy*='job']",
                "[data-testid*='job']",
                ".job-card",
                ".job-result",
                ".search-result"
            };

            foreach (var selector in selectorsToTest)
            {
                try
                {
                    var elements = _driver.FindElements(By.CssSelector(selector));
                    Log.Information("Selector '{Selector}' found {Count} elements", selector, elements.Count);

                    if (elements.Count > 0 && elements.Count <= 5)
                    {
                        // Log details of first few elements
                        for (int i = 0; i < Math.Min(elements.Count, 3); i++)
                        {
                            var element = elements[i];
                            Log.Information("  Element {Index}: Tag={Tag}, Classes={Classes}, Attributes={Attributes}",
                                i, element.TagName, element.GetAttribute("class"), GetElementAttributes(element));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning("Selector '{Selector}' failed: {Message}", selector, ex.Message);
                }
            }

            // Also check for common job listing patterns
            Log.Information("=== CHECKING FOR COMMON JOB LISTING PATTERNS ===");

            var jobElements = _driver.FindElements(By.XPath("//*[contains(@class, 'job') or contains(@data-cy, 'job') or contains(@data-testid, 'job')]"));
            Log.Information("Found {Count} elements with 'job' in class or data attributes", jobElements.Count);

            // Check page source for job-related keywords
            var pageSource = _driver.PageSource.ToLower();
            var jobKeywords = new[] { "job", "position", "career", "opening", "opportunity" };

            foreach (var keyword in jobKeywords)
            {
                var count = pageSource.Split(keyword).Length - 1;
                Log.Information("Page contains '{Keyword}' {Count} times", keyword, count);
            }

            Log.Information("=== DEBUGGING COMPLETE ===");
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

        private string GetElementAttributes(IWebElement element)
        {
            try
            {
                var attributes = new List<string>();
                var attributeNames = new[] { "data-cy", "data-testid", "id", "class", "data-job-id" };

                foreach (var attr in attributeNames)
                {
                    var value = element.GetAttribute(attr);
                    if (!string.IsNullOrEmpty(value))
                    {
                        attributes.Add($"{attr}='{value}'");
                    }
                }

                return string.Join(", ", attributes);
            }
            catch
            {
                return "error reading attributes";
            }
        }

        public void Dispose()
        {
            _loginService.Dispose();
            _httpClient.Dispose();
        }
    }
}
