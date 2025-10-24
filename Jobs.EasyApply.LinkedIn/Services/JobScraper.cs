using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Serilog;
using Jobs.EasyApply.Common.Models;
using Jobs.EasyApply.LinkedIn.Utilities;
using System;

namespace Jobs.EasyApply.LinkedIn.Services
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

            // Enhanced anti-detection options
            options.AddArgument("--start-maximized");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--disable-extensions");
            options.AddArgument("--disable-plugins");
            options.AddArgument("--disable-images");
            options.AddArgument("--no-first-run");
            options.AddArgument("--disable-default-apps");
            options.AddArgument("--disable-background-timer-throttling");
            options.AddArgument("--disable-backgrounding-occluded-windows");
            options.AddArgument("--disable-renderer-backgrounding");
            options.AddArgument("--disable-features=TranslateUI");
            options.AddArgument("--disable-ipc-flooding-protection");

            // More realistic browser fingerprint
            options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            options.AddArgument("--lang=en-US,en;q=0.9");
            options.AddArgument("--accept-language=en-US,en;q=0.9");

            // Unique profile with more realistic settings
            var profilePath = $"C:\\Users\\{Environment.UserName}\\AppData\\Local\\Temp\\ChromeProfile_{Guid.NewGuid()}";
            options.AddArgument($"--user-data-dir={profilePath}");

            // Additional stealth options
            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalOption("useAutomationExtension", false);

            // Run with visible browser for debugging (remove --headless if present)
            // options.AddArgument("--headless"); // Commented out for visible browser

            _driver = new ChromeDriver(options);
            _htmlScraper = new HtmlScraper(_driver);
            _jobApplicator = new JobApplicator(_driver, _htmlScraper);

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
                return true;
            }
            catch (WebDriverException)
            {
                return false;
            }
        }

        public List<JobListing> SearchJobs()
        {
            Log.Information("Starting job search for {Title} in {Location}", _jobTitle, _location);

            // Check if session is valid before starting
            if (!IsSessionValid())
            {
                Log.Error("WebDriver session is invalid. Cannot proceed with job search.");
                return [];
            }

            _driver.Navigate().GoToUrl("https://www.linkedin.com/login");

            // Auto login with human-like behavior
            Log.Information("Logging in automatically...");
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));

            try
            {
                // Add random delay before starting login (human-like behavior)
                Thread.Sleep(new Random().Next(1000, 3000));

                // Wait for and fill email field with human-like typing
                Log.Information("Looking for email field...");
                // Try multiple selectors for email field
                var emailField = wait.Until(d =>
                {
                    // Try different selectors for the email field
                    var selectors = new[]
                    {
                        By.Id("username"),
                        By.Name("session_key"),
                        By.CssSelector("input[name='session_key']"),
                        By.CssSelector("input[type='text'][name='session_key']"),
                        By.CssSelector("input[autocomplete='username']")
                    };

                    foreach (var selector in selectors)
                    {
                        try
                        {
                            var element = d.FindElement(selector);
                            if (element.Displayed && element.Enabled)
                            {
                                Log.Information("Found email field with selector: {Selector}", selector);
                                return element;
                            }
                        }
                        catch (NoSuchElementException) { }
                    }
                    return null;
                });

                if (emailField == null)
                {
                    throw new Exception("Could not find email field with any known selector");
                }

                // Clear field first, then type email with human-like delays
                emailField.Clear();
                Thread.Sleep(new Random().Next(200, 500));

                foreach (char c in _email)
                {
                    emailField.SendKeys(c.ToString());
                    Thread.Sleep(new Random().Next(50, 150));
                }

                // Verify email field was filled correctly
                var actualEmailValue = emailField.GetAttribute("value");
                if (actualEmailValue == _email)
                {
                    Log.Information("Email field filled successfully. Value: {Email}", _email);
                }
                else
                {
                    Log.Warning("Email field verification failed. Expected: {Expected}, Actual: {Actual}", _email, actualEmailValue);
                    // Try to fill it again
                    emailField.Clear();
                    Thread.Sleep(500);
                    emailField.SendKeys(_email);
                }

                // Human-like delay between fields
                Thread.Sleep(new Random().Next(500, 1500));

                // Fill password field with human-like typing
                Log.Information("Looking for password field...");
                // Try multiple selectors for password field
                var passwordField = wait.Until(d =>
                {
                    // Try different selectors for the password field
                    var selectors = new[]
                    {
                        By.Id("password"),
                        By.Name("session_password"),
                        By.CssSelector("input[name='session_password']"),
                        By.CssSelector("input[type='password'][name='session_password']"),
                        By.CssSelector("input[autocomplete='current-password']")
                    };

                    foreach (var selector in selectors)
                    {
                        try
                        {
                            var element = d.FindElement(selector);
                            if (element.Displayed && element.Enabled)
                            {
                                Log.Information("Found password field with selector: {Selector}", selector);
                                return element;
                            }
                        }
                        catch (NoSuchElementException) { }
                    }
                    return null;
                });

                if (passwordField == null)
                {
                    throw new Exception("Could not find password field with any known selector");
                }

                // Clear field first, then type password with human-like delays
                passwordField.Clear();
                Thread.Sleep(new Random().Next(200, 500));

                foreach (char c in _password)
                {
                    passwordField.SendKeys(c.ToString());
                    Thread.Sleep(new Random().Next(80, 200));
                }

                // Verify password field was filled correctly
                var actualPasswordValue = passwordField.GetAttribute("value");
                if (actualPasswordValue == _password)
                {
                    Log.Information("Password field filled successfully. Length: {Length}", _password.Length);
                }
                else
                {
                    Log.Warning("Password field verification failed. Expected length: {Expected}, Actual length: {Actual}", _password.Length, actualPasswordValue?.Length ?? 0);
                    // Try to fill it again
                    passwordField.Clear();
                    Thread.Sleep(500);
                    passwordField.SendKeys(_password);
                }

                // Human-like delay before clicking
                Thread.Sleep(new Random().Next(1000, 2500));

                // Click sign in button
                Log.Information("Looking for sign in button...");
                var signInButton = _driver.FindElement(By.CssSelector("button[type='submit']"));
                signInButton.Click();
                Log.Information("Sign in button clicked");

                // Wait for redirect with detailed logging
                Log.Information("Waiting for login redirect... Current URL: {Url}", _driver.Url);
                Thread.Sleep(5000); // Give LinkedIn more time to process

                // Check current URL for debugging
                Log.Information("Post-click URL: {Url}", _driver.Url);

                // If we're at a security checkpoint, pause for manual intervention
                if (_driver.Url.Contains("/checkpoint/challenge/"))
                {
                    Log.Warning("LinkedIn security checkpoint detected! Pausing for manual intervention...");
                    Log.Warning("Please complete the security challenge in the browser window, then press Enter in the console to continue...");

                    // Wait for user input or timeout after 5 minutes
                    var timeout = DateTime.Now.AddMinutes(5);
                    while (_driver.Url.Contains("/checkpoint/challenge/") && DateTime.Now < timeout)
                    {
                        Thread.Sleep(2000);
                        Log.Information("Still waiting for security challenge completion... Current URL: {Url}", _driver.Url);
                    }

                    if (_driver.Url.Contains("/checkpoint/challenge/"))
                    {
                        Log.Warning("Security challenge timeout reached. Proceeding with automation...");
                    }
                    else
                    {
                        Log.Information("Security challenge completed! Continuing with automation...");
                    }
                }

                // Wait for redirect or feed to load (increased timeout and more URL patterns)
                Log.Information("Waiting for successful login redirect...");
                wait.Until(d =>
                {
                    Log.Information("Checking URL: {Url}", d.Url);
                    return d.Url.Contains("/feed") ||
                           d.Url.Contains("/jobs") ||
                           d.Url.Contains("/mynetwork") ||
                           d.Url.Contains("/messaging") ||
                           (d.Url.Contains("linkedin.com") && !d.Url.Contains("/login") && !d.Url.Contains("/checkpoint/challenge/"));
                });
                Log.Information("Login successful. Final URL: {Url}", _driver.Url);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to auto login. Current URL: {Url}", _driver.Url);
                Log.Information("LinkedIn may require manual intervention or have security measures in place");
                Log.Information("Consider updating credentials in appsettings.json or running with visible browser");

                // Provide debugging information
                Log.Information("Debugging info:");
                Log.Information("- Current URL: {Url}", _driver.Url);
                Log.Information("- Page title: {Title}", _driver.Title);
                Log.Information("- Email field found: {Found}", _driver.FindElements(By.Id("username")).Count > 0);
                Log.Information("- Password field found: {Found}", _driver.FindElements(By.Id("password")).Count > 0);

                // Check if we're still on the login page
                if (_driver.Url.Contains("/login"))
                {
                    Log.Information("Still on login page - credentials may be incorrect or CAPTCHA required");
                }
                else
                {
                    Log.Information("Redirected away from login page - may need manual intervention");
                }

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
