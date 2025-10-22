using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Serilog;
using System.Threading;
using Jobs.EasyApply.Common.Models;
using Jobs.EasyApply.Utilities;

namespace Jobs.EasyApply.Services
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
                    Log.Warning("No job card found for job: {Title} at {Company}", job.Title, job.Company);
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
                var nextButton = _htmlScraper.FindNextButton();
                if (nextButton != null)
                {
                    Log.Information("Found ContactInfo Next button, clicking it");
                    nextButton.Click();
                    Thread.Sleep(2000); // Wait for next page to load
                }
                else
                {
                    Log.Information("No ContactInfo Next button found, continuing to submit");
                }

                // Handle apply form - resume selection
                nextButton = _htmlScraper.FindNextButton();
                if (nextButton != null)
                {
                    Log.Information("Found ResumeSelectionNext button, clicking it");
                    nextButton.Click();
                    Thread.Sleep(2000); // Wait for next page to load
                }
                else
                {
                    Log.Information("No Resume Selection Next button found, continuing to submit");
                }

                // Handle apply form - Review screen and optional mark job as a top selection
                nextButton = _htmlScraper.FindReviewNextButton();
                if (nextButton != null)
                {
                    // Check for additional questions section before proceeding
                    if (_htmlScraper.HasAdditionalQuestionsModule())
                    {
                        // Smart detection: Check if questions are pre-populated
                        if (_htmlScraper.AreAdditionalQuestionsPrePopulated())
                        {
                            Log.Information("Additional questions detected but appear to be pre-populated for job: {Title} at {Company}. Continuing automatically.", job.Title, job.Company);
                        }
                        else if (_htmlScraper.HasEmptyRequiredFields())
                        {
                            Log.Warning("Additional questions module detected with empty required fields for job: {Title} at {Company}. Pausing for manual input.", job.Title, job.Company);
                            Console.WriteLine($"*** MANUAL INTERVENTION REQUIRED ***");
                            Console.WriteLine($"Additional questions detected for: {job.Title} at {job.Company}");
                            Console.WriteLine($"Some questions appear to be empty and require input.");
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
                }
                else
                {
                    Log.Information("No Review or Next button found, this should never occur.");
                    return false;
                }

                // Handle apply form - Additional Questions screen or Review screen
                nextButton = _htmlScraper.FindReviewNextButton();
                if (nextButton != null)
                {
                    // Check for additional questions section before proceeding
                    if (_htmlScraper.HasAdditionalQuestionsModule())
                    {
                        // Smart detection: Check if questions are pre-populated
                        if (_htmlScraper.AreAdditionalQuestionsPrePopulated())
                        {
                            Log.Information("Additional questions detected but appear to be pre-populated for job: {Title} at {Company}. Continuing automatically.", job.Title, job.Company);
                        }
                        else if (_htmlScraper.HasEmptyRequiredFields())
                        {
                            Log.Warning("Additional questions module detected with empty required fields for job: {Title} at {Company}. Pausing for manual input.", job.Title, job.Company);
                            Console.WriteLine($"*** MANUAL INTERVENTION REQUIRED ***");
                            Console.WriteLine($"Additional questions detected for: {job.Title} at {job.Company}");
                            Console.WriteLine($"Some questions appear to be empty and require input.");
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
                }
                else
                {
                    Log.Information("No Review Next button found, check for submit.");
                }

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
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to apply for job: {Title} at {Company}", job.Title, job.Company);
                return false;
            }
        }

        public void Dispose()
        {
            // JobApplicator doesn't own the driver, so we don't dispose it here
            // The driver disposal is handled by JobScraper
        }
    }
}
