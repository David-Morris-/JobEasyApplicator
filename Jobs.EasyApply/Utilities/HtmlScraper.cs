using OpenQA.Selenium;

namespace Jobs.EasyApply.Utilities
{
    public class HtmlScraper(IWebDriver driver)
    {
        private readonly IWebDriver _driver = driver;

        // Next button finder
        public IWebElement? FindNextButton()
        {
            try
            {
                return _driver.FindElement(By.CssSelector("button[data-easy-apply-next-button]"));
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        }

        // Review button finder
        public IWebElement? FindReviewButton()
        {
            try
            {
                return _driver.FindElement(By.CssSelector("button[data-live-test-easy-apply-review-button]"));
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        }

        // Submit button finder
        public IWebElement? FindSubmitButton()
        {
            try
            {
                return _driver.FindElement(By.CssSelector("button[data-live-test-easy-apply-submit-button]"));
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        }

        // Enhanced Done button finder with multiple strategies
        public IWebElement? FindDoneButton()
        {
            try
            {
                // Try multiple selectors for the Done button
                // First try: button with text "Done"
                try
                {
                    return _driver.FindElement(By.XPath("//button[contains(text(), 'Done')]"));
                }
                catch (NoSuchElementException)
                {
                    // Ignore and try next selector
                }

                // Second try: button with artdeco-button--primary class (most common for primary actions)
                try
                {
                    return _driver.FindElement(By.CssSelector("button[artdeco-button--primary]"));
                }
                catch (NoSuchElementException)
                {
                    // Ignore and try next selector
                }

                // Third try: button in a modal/dialog context with specific class
                try
                {
                    return _driver.FindElement(By.CssSelector("button[data-test-modal-close-btn], button[aria-label*='Done'], button[class*='modal']"));
                }
                catch (NoSuchElementException)
                {
                    // Ignore and try next selector
                }

                // Fourth try: any button with "done" in class name or data attribute
                try
                {
                    return _driver.FindElement(By.CssSelector("button[class*='done'], button[data-qa*='done'], button[aria-label*='done']"));
                }
                catch (NoSuchElementException)
                {
                    // Ignore and try next selector
                }

                // If all selectors fail, return null
                return null;
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        }

        //Composite Methods

        // Combine review and next button search
        public IWebElement? FindReviewNextButton()
        {
            // Try to find review button first, if null then try next button
            return FindReviewButton() ?? FindNextButton();
        }

        public IWebElement? FindJobCard(string jobId)
        {
            try
            {
                return _driver.FindElement(By.CssSelector($"div[data-job-id='{jobId}']"));
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        }

        public IWebElement? FindEasyApplyButton(string jobId)
        {
            try
            {
                return _driver.FindElement(By.CssSelector($"button.jobs-apply-button[data-job-id='{jobId}']"));
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        }

        // Additional Questions module detection
        public bool HasAdditionalQuestionsModule()
        {
            try
            {
                // Look for common selectors that indicate an additional questions section
                // This could be a form with text inputs, textareas, select dropdowns, etc.

                // Check for form sections that might contain questions
                var questionSelectors = new[]
                {
                    "div[data-test-form-element='input']",
                    "div[data-test-form-element='textarea']",
                    "div[data-test-form-element='select']",
                    "div[class*='form'] input[type='text']",
                    "div[class*='form'] textarea",
                    "div[class*='form'] select",
                    "label[class*='question']",
                    "div[class*='additional-questions']",
                    "div[class*='custom-questions']",
                    "div[class*='screening-questions']"
                };

                foreach (var selector in questionSelectors)
                {
                    try
                    {
                        var elements = _driver.FindElements(By.CssSelector(selector));
                        if (elements.Any(e => e.Displayed))
                        {
                            return true;
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        // Continue to next selector
                        continue;
                    }
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Check if additional questions are pre-populated
        public bool AreAdditionalQuestionsPrePopulated()
        {
            try
            {
                // Check if form fields are already filled with answers
                var inputSelectors = new[]
                {
                    "input[type='text']:not([value=''])",
                    "input[type='email']:not([value=''])",
                    "textarea:not(:empty)",
                    "select option[selected]:not([value=''])",
                    "input[checked]", // radio buttons and checkboxes
                    "input[type='radio']:checked",
                    "input[type='checkbox']:checked"
                };

                foreach (var selector in inputSelectors)
                {
                    try
                    {
                        var elements = _driver.FindElements(By.CssSelector(selector));
                        if (elements.Any(e => e.Displayed))
                        {
                            return true;
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        // Continue to next selector
                        continue;
                    }
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Check if any form fields are empty and require input
        public bool HasEmptyRequiredFields()
        {
            try
            {
                // Look for empty required fields
                var emptyFieldSelectors = new[]
                {
                    "input[required]:not([value]):not([checked])",
                    "input[type='text'][required]:empty",
                    "textarea[required]:empty",
                    "select[required] option[selected][value='']",
                    "input[type='radio'][required]:not(:checked)",
                    "input[type='checkbox'][required]:not(:checked)"
                };

                foreach (var selector in emptyFieldSelectors)
                {
                    try
                    {
                        var elements = _driver.FindElements(By.CssSelector(selector));
                        if (elements.Any(e => e.Displayed))
                        {
                            return true;
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        // Continue to next selector
                        continue;
                    }
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
