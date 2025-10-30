using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Serilog;
using System.Threading;
using System.Linq;
using Jobs.EasyApply.Common.Models;
using Jobs.EasyApply.Indeed.Utilities;

namespace Jobs.EasyApply.Indeed.Services
{
    public class JobApplicator : IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly HtmlScraper _htmlScraper;

        public JobApplicator(IWebDriver driver, HtmlScraper htmlScraper)
        {
            _driver = driver;
            _htmlScraper = htmlScraper;
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

        public bool ApplyForJob(JobListing job)
        {
            try
            {
                Log.Information("Applying for job: {Title} at {Company}", job.Title, job.Company);

                // Check if session is valid before proceeding
                if (!IsSessionValid())
                {
                    Log.Error("WebDriver session is invalid. Cannot apply for job: {Title} at {Company}", job.Title, job.Company);
                    return false;
                }

                // Navigate to the job details page
                _driver.Navigate().GoToUrl(job.Url);
                Thread.Sleep(3000); // Wait for job details page to load

                // Check session again after navigation
                if (!IsSessionValid())
                {
                    Log.Error("WebDriver session became invalid after navigation. Cannot apply for job: {Title} at {Company}", job.Title, job.Company);
                    return false;
                }

                // Check if application has already been submitted
                if (_htmlScraper.HasApplicationSubmittedText())
                {
                    Log.Information("Application already submitted for job: {Title} at {Company}. Skipping.", job.Title, job.Company);
                    return false;
                }

                // Wait for the Apply button to appear on the job details page
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
                var applyButton = wait.Until(driver => _htmlScraper.FindApplyButton(job.JobId));

                if (applyButton != null)
                {
                    Log.Information("Found Apply button for Indeed job, clicking it");
                    applyButton.Click();
                    Thread.Sleep(2000); // Wait for apply modal to load
                }
                else
                {
                    Log.Warning("No Apply button found for Indeed job: {Title} at {Company}", job.Title, job.Company);
                    return false;
                }

                // Handle apply form - check for additional questions
                var nextButton = _htmlScraper.FindNextOrReviewButton();
                while (nextButton != null)
                {
                    // Check for additional questions section before proceeding
                    if (_htmlScraper.HasAdditionalQuestionsModule())
                    {
                        // Smart detection: Check if questions are not pre-populated
                        if (_htmlScraper.HasEmptyRequiredFields())
                        {
                            Log.Warning("Additional questions module detected with empty required fields for job: {Title} at {Company}. Pausing for manual input.", job.Title, job.Company);
                            Console.WriteLine($"*** MANUAL INTERVENTION REQUIRED ***");
                            Console.WriteLine($"Additional questions detected for: {job.Title} at {job.Company}");
                            Console.WriteLine($"Please fill out the questions in the browser and click Next to continue...");
                            Console.WriteLine($"Press Enter in this console when you have completed the questions and clicked Next.");

                            // Wait for user to complete the questions and press Enter
                            Console.ReadLine();

                            Log.Information("User has completed additional questions, continuing with application process");
                        }
                        else
                        {
                            Log.Information("Additional questions detected but no empty required fields found for job: {Title} at {Company}. Continuing automatically.", job.Title, job.Company);
                        }
                    }
                    else
                    {
                        Log.Information("No additional questions detected, proceeding with application");
                    }

                    nextButton.Click();
                    Thread.Sleep(2000); // Wait for next page to load

                    // Look for the next Next button
                    nextButton = _htmlScraper.FindNextOrReviewButton();
                }
                Log.Information("No more Next buttons found, continuing to submit");

                // Final submit
                var submitButton = _htmlScraper.FindSubmitButton();
                if (submitButton != null)
                {
                    Log.Information("Found Submit Application button, clicking it");
                    submitButton.Click();
                    Thread.Sleep(5000); // Wait for submission to process
                }
                else
                {
                    Log.Warning("No Submit Application button found for job: {Title} at {Company}", job.Title, job.Company);
                    return false;
                }

                var doneButton = _htmlScraper.FindDoneButton();
                if (doneButton != null)
                {
                    Log.Information("Found Done button, clicking it to finish application");
                    doneButton.Click();
                    Thread.Sleep(3000); // Wait for modal to close
                }
                else
                {
                    Log.Warning("No Done button found after submission for job: {Title} at {Company}", job.Title, job.Company);
                }

                Log.Information("Successfully applied for job: {Title} at {Company}", job.Title, job.Company);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to apply for job: {Title} at {Company}", job.Title, job.Company);
                // Clean up any open modals before returning
                CloseOpenModals();
                return false;
            }
        }

        /// <summary>
        /// Closes any open application modals to prevent blocking subsequent job interactions
        /// </summary>
        private void CloseOpenModals()
        {
            try
            {
                // Try to find and close any open modals
                var closeButton = _htmlScraper.FindModalCloseButton();
                if (closeButton != null)
                {
                    closeButton.Click();
                    Thread.Sleep(1000); // Wait for modal to close
                    Log.Information("Closed open modal after job application failure");
                    return;
                }

                // Try to press Escape key as a fallback
                try
                {
                    _driver.FindElement(By.TagName("body")).SendKeys(OpenQA.Selenium.Keys.Escape);
                    Thread.Sleep(1000);
                    Log.Information("Pressed Escape key to close any open modals");
                }
                catch (Exception)
                {
                    // Ignore if this fails
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to close open modals after job application failure");
            }
        }

        public void Dispose()
        {
            // JobApplicator doesn't own the driver, so we don't dispose it here
            // The driver disposal is handled by JobScraper
        }
    }
}
