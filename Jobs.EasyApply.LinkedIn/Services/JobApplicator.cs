using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Serilog;
using System.Threading;
using System.Linq;
using Jobs.EasyApply.Common.Models;
using Jobs.EasyApply.LinkedIn.Utilities;

namespace Jobs.EasyApply.LinkedIn.Services
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

        public bool ApplyForJob(JobListing job)
        {
            try
            {
                Log.Information("Applying for job: {Title} at {Company}", job.Title, job.Company);

                // skip previously applied for job
                if (job.PreviouslyApplied)
                {
                    Log.Information("Skipping job {Title} at {Company} - previously applied", job.Title, job.Company);
                    return true; // Return true to indicate we "handled" this job by skipping it
                }

                // Select the job card element and navigate to the job details page
                var jobCard = _htmlScraper.FindJobCard(job.JobId);
                if (jobCard != null)
                {
                    Log.Information("Found job card for {Title}, clicking to navigate to job details", job.Title);
                    jobCard.Click();
                    Thread.Sleep(3000); // Wait for job details page to load
                }
                else
                {
                    Log.Warning("No job card found for job: " + job.Title + " at " + job.Company);
                    return false;
                }

                // Now look for the Easy Apply button on the job details page
                var easyApplyButton = _htmlScraper.FindEasyApplyButton(job.JobId);
                if (easyApplyButton != null)
                {
                    Log.Information("Found Easy Apply button, clicking it");
                    easyApplyButton.Click();
                    Thread.Sleep(2000); // Wait for apply modal to load
                }
                else
                {
                    Log.Warning("No Easy Apply button found for job: {Title} at {Company}", job.Title, job.Company);
                    return false;
                }

                // Handle apply form - contact info
                var nextReviewButton = _htmlScraper.FindReviewNextButton();
                while (nextReviewButton != null)
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
                    nextReviewButton.Click();
                    Thread.Sleep(2000); // Wait for next page to load

                    // Look for the next Next button
                    nextReviewButton = _htmlScraper.FindReviewNextButton();
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
                    //TODO: Check if modal is closed anyway
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
        /// Closes any open Easy Apply modals to prevent blocking subsequent job card clicks
        /// </summary>
        private void CloseOpenModals()
        {
            try
            {
                // Try to find and close the Easy Apply modal
                var closeButtonSelectors = new[]
                {
                    "button[aria-label*='Dismiss']",
                    "button[data-test-modal-close-btn]",
                    "button[aria-label*='Close']",
                    "button[class*='modal-close']",
                    ".artdeco-modal-overlay button[aria-label*='Dismiss']",
                    ".artdeco-modal-overlay button[data-test-modal-close-btn]"
                };

                foreach (var selector in closeButtonSelectors)
                {
                    try
                    {
                        var closeButtons = _driver.FindElements(By.CssSelector(selector));
                        foreach (var button in closeButtons.Where(b => b.Displayed))
                        {
                            button.Click();
                            Thread.Sleep(1000); // Wait for modal to close
                            Log.Information("Closed open modal after job application failure");
                            return;
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        continue;
                    }
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
