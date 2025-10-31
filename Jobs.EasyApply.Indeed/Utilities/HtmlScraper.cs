using OpenQA.Selenium;
using Serilog;

namespace Jobs.EasyApply.Indeed.Utilities
{
    public class HtmlScraper(IWebDriver driver)
    {
        private readonly IWebDriver _driver = driver;

        // Apply button finder for Indeed
        public IWebElement? FindApplyButton(string jobId)
        {
            try
            {
                // First, try to find the apply button in shadow DOM using JavaScript
                try
                {
                    IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
                    var shadowButton = (IWebElement)js.ExecuteScript("return document.querySelector('apply-button-wc').shadowRoot.querySelector('button.btn.btn-primary');");
                    if (shadowButton != null && shadowButton.Displayed && shadowButton.Enabled)
                    {
                        Log.Information("Found apply button in shadow DOM");
                        return shadowButton;
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Shadow DOM apply button not found");
                }

                // Try multiple selectors for Indeed apply buttons
                var selectors = new[]
                {
                    "button[data-testid='apply-button']",
                    "button[data-cy='apply-button']",
                    "button[class*='apply']",
                    "button[class*='btn-apply']",
                    "a[data-testid='apply-link']",
                    "a[data-cy='apply-link']",
                    "a[href*='apply']",
                    "button[title*='Apply']",
                    "button[type='button'][class*='apply']",
                    "input[value*='Apply']",
                    "button[class*='apply-now']",
                    "a[class*='apply-now']",
                    "a[class*='bg-interaction']",
                    "button[class*='bg-interaction']",
                    "button[class*='btn-primary']"
                };

                foreach (var selector in selectors)
                {
                    try
                    {
                        var elements = _driver.FindElements(By.CssSelector(selector));
                        var visibleElement = elements.FirstOrDefault(e => e.Displayed && e.Enabled);
                        if (visibleElement != null)
                        {
                            Log.Information("Found apply button with selector: {Selector}", selector);
                            return visibleElement;
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        continue;
                    }
                }

                // Also try XPath for buttons containing "Apply", "Apply Now", or "Easy Apply"
                try
                {
                    var xpathElements = _driver.FindElements(By.XPath("//button[contains(translate(text(), 'APPLY', 'apply'), 'apply') or contains(translate(text(), 'APPLY NOW', 'apply now'), 'apply now') or contains(translate(text(), 'EASY APPLY', 'easy apply'), 'easy apply')] | //a[contains(translate(text(), 'APPLY', 'apply'), 'apply') or contains(translate(text(), 'APPLY NOW', 'apply now'), 'apply now') or contains(translate(text(), 'EASY APPLY', 'easy apply'), 'easy apply')]"));
                    var visibleElement = xpathElements.FirstOrDefault(e => e.Displayed && e.Enabled);
                    if (visibleElement != null)
                    {
                        Log.Information("Found apply button with XPath");
                        return visibleElement;
                    }
                }
                catch (Exception)
                {
                    // Ignore XPath errors
                }

                Log.Warning("No apply button found for job {JobId}", jobId);
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error finding apply button for job {JobId}", jobId);
                return null;
            }
        }

        // Easy Apply indicator finder
        public bool HasEasyApplyIndicator(IWebElement jobCard)
        {
            try
            {
                // Look for Easy Apply badges or indicators on job cards
                var easyApplySelectors = new[]
                {
                    "[data-testid='indeedApply']",
                    "span[data-cy='easy-apply-badge']",
                    "span[data-testid='easy-apply-badge']",
                    "div[class*='easy-apply']",
                    "span[class*='easy-apply']",
                    "div[class*='quick-apply']",
                    "span[class*='quick-apply']",
                    "div[class*='instant-apply']",
                    "span[class*='instant-apply']",
                    "button[data-cy='easy-apply']",
                    "button[data-testid='easy-apply']",
                    "a[data-cy='easy-apply']",
                    "a[data-testid='easy-apply']",
                    "[data-cy*='easy']",
                    "[data-testid*='easy']",
                    "[class*='easy-apply']",
                    "[class*='quick-apply']"
                };

                foreach (var selector in easyApplySelectors)
                {
                    try
                    {
                        var elements = jobCard.FindElements(By.CssSelector(selector));
                        if (elements.Any(e => e.Displayed))
                        {
                            Console.WriteLine($"Found Easy Apply indicator with selector: {selector}");
                            return true;
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        continue;
                    }
                }

                // Check for text indicators
                var cardText = jobCard.Text?.ToLower() ?? "";
                if (cardText.Contains("easy apply") ||
                    cardText.Contains("quick apply") ||
                    cardText.Contains("instant apply") ||
                    cardText.Contains("apply now") ||
                    cardText.Contains("1-click"))
                {
                    Console.WriteLine($"Found Easy Apply text indicator: {cardText}");
                    return true;
                }

                // Check for specific Indeed.com Easy Apply patterns
                var innerHtml = jobCard.GetAttribute("innerHTML")?.ToLower() ?? "";
                if (innerHtml.Contains("easy apply") ||
                    innerHtml.Contains("quick apply") ||
                    innerHtml.Contains("1 click") ||
                    innerHtml.Contains("one click"))
                {
                    Console.WriteLine($"Found Easy Apply in HTML: {innerHtml}");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking Easy Apply indicator: {ex.Message}");
                return false;
            }
        }

        // Debug method to analyze job card structure
        public void DebugJobCardStructure(IWebElement jobCard)
        {
            try
            {
                Console.WriteLine("=== JOB CARD DEBUG INFO ===");
                Console.WriteLine($"Tag: {jobCard.TagName}");
                Console.WriteLine($"Classes: {jobCard.GetAttribute("class")}");
                Console.WriteLine($"ID: {jobCard.GetAttribute("id")}");
                Console.WriteLine($"Data-cy: {jobCard.GetAttribute("data-cy")}");
                Console.WriteLine($"Data-testid: {jobCard.GetAttribute("data-testid")}");
                Console.WriteLine($"Text: {jobCard.Text}");
                Console.WriteLine($"Inner HTML length: {jobCard.GetAttribute("innerHTML")?.Length ?? 0}");

                // Check for child elements
                var children = jobCard.FindElements(By.XPath(".//*"));
                Console.WriteLine($"Child elements: {children.Count}");

                // Look for common job card child elements
                var childSelectors = new[]
                {
                    "h1", "h2", "h3", "h4", "h5", "h6",
                    "a", "button", "span", "div"
                };

                foreach (var selector in childSelectors)
                {
                    var elements = jobCard.FindElements(By.CssSelector(selector));
                    if (elements.Count > 0)
                    {
                        Console.WriteLine($"  {selector}: {elements.Count} elements");
                        if (elements.Count <= 3)
                        {
                            foreach (var element in elements)
                            {
                                Console.WriteLine($"    - Text: '{element.Text}', Class: '{element.GetAttribute("class")}'");
                            }
                        }
                    }
                }

                Console.WriteLine("=== END JOB CARD DEBUG ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error debugging job card: {ex.Message}");
            }
        }

        // Submit button finder
        public IWebElement? FindSubmitButton()
        {
            try
            {
                // First, try to find submit button in shadow DOM
                try
                {
                    IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
                    var shadowSubmit = (IWebElement)js.ExecuteScript("return document.querySelector('apply-button-wc')?.shadowRoot?.querySelector('button[type=\"submit\"]') || document.querySelector('form')?.querySelector('button[type=\"submit\"]')");
                    if (shadowSubmit != null && shadowSubmit.Displayed && shadowSubmit.Enabled)
                    {
                        Log.Information("Found submit button in shadow DOM or form");
                        return shadowSubmit;
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Shadow DOM submit button not found");
                }

                // Try multiple selectors for submit buttons
                var submitSelectors = new[]
                {
                    "button[type='submit']",
                    "button[data-cy='submit']",
                    "button[data-testid='submit']",
                    "button[class*='submit']",
                    "input[type='submit']",
                    "button[title*='Submit']",
                    "button[aria-label*='Submit']",
                    "button[class*='btn-primary'][type='submit']",
                    "button[class*='apply']",
                    "button[class*='send']",
                    "button[class*='next']",
                    "button[class*='btn-primary']",
                    "button[class*='bg-interaction']",
                    "button[class*='apply-now']"
                };

                foreach (var selector in submitSelectors)
                {
                    try
                    {
                        var elements = _driver.FindElements(By.CssSelector(selector));
                        var visibleElement = elements.FirstOrDefault(e => e.Displayed && e.Enabled);
                        if (visibleElement != null)
                        {
                            Log.Information("Found submit button with selector: {Selector}", selector);
                            return visibleElement;
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        continue;
                    }
                }

                // Try XPath for submit buttons
                try
                {
                    var xpathElements = _driver.FindElements(By.XPath("//button[contains(translate(text(), 'SUBMIT', 'submit'), 'submit') or @type='submit' or @data-cy='submit' or @data-testid='submit'] | //input[@type='submit']"));
                    var visibleElement = xpathElements.FirstOrDefault(e => e.Displayed && e.Enabled);
                    if (visibleElement != null)
                    {
                        Log.Information("Found submit button with XPath");
                        return visibleElement;
                    }
                }
                catch (Exception)
                {
                    // Ignore XPath errors
                }

                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error finding submit button");
                return null;
            }
        }

        // Done button finder
        public IWebElement? FindDoneButton()
        {
            try
            {
                // Try multiple selectors for the Done button
                try
                {
                    return _driver.FindElement(By.XPath("//button[contains(text(), 'Done')]"));
                }
                catch (NoSuchElementException)
                {
                    // Ignore and try next selector
                }

                try
                {
                    return _driver.FindElement(By.CssSelector("button[aria-label*='Done']"));
                }
                catch (NoSuchElementException)
                    {
                    // Ignore and try next selector
                }

                return null;
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        }

        // Modal close button finder
        public IWebElement? FindModalCloseButton()
        {
            try
            {
                var closeButtonSelectors = new[]
                {
                    "button[aria-label*='Close']",
                    "button[class*='modal-close']",
                    ".modal button[aria-label*='Close']",
                    "button[data-testid='modal-close']"
                };

                foreach (var selector in closeButtonSelectors)
                {
                    try
                    {
                        var buttons = _driver.FindElements(By.CssSelector(selector));
                        var visibleButton = buttons.FirstOrDefault(b => b.Displayed);
                        if (visibleButton != null)
                        {
                            return visibleButton;
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        continue;
                    }
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Job card finder
        public IWebElement? FindJobCard(string jobId)
        {
            try
            {
                return _driver.FindElement(By.CssSelector($"div[data-cy='card-job'][data-job-id='{jobId}']"));
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        }

        // Next button finder for multi-step forms
        public IWebElement? FindNextButton()
        {
            try
            {
                // First, try to find next button in shadow DOM
                try
                {
                    IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
                    var shadowNext = (IWebElement)js.ExecuteScript("return document.querySelector('apply-button-wc')?.shadowRoot?.querySelector('button[class*=\"next\"]') || document.querySelector('form')?.querySelector('button[class*=\"next\"]')");
                    if (shadowNext != null && shadowNext.Displayed && shadowNext.Enabled)
                    {
                        Log.Information("Found next button in shadow DOM or form");
                        return shadowNext;
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Shadow DOM next button not found");
                }

                // Try multiple selectors for next buttons
                var nextSelectors = new[]
                {
                    "button[data-cy='next-button']",
                    "button[data-testid='next-button']",
                    "button[class*='next']",
                    "button[title*='Next']",
                    "button[aria-label*='Next']",
                    "button[class*='btn-primary'][type='button']",
                    "button[class*='continue']",
                    "input[type='button'][value*='Next']"
                };

                foreach (var selector in nextSelectors)
                {
                    try
                    {
                        var elements = _driver.FindElements(By.CssSelector(selector));
                        var visibleElement = elements.FirstOrDefault(e => e.Displayed && e.Enabled);
                        if (visibleElement != null)
                        {
                            Log.Information("Found next button with selector: {Selector}", selector);
                            return visibleElement;
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        continue;
                    }
                }

                // Try XPath for next buttons
                try
                {
                    var xpathElements = _driver.FindElements(By.XPath("//button[contains(translate(text(), 'NEXT', 'next'), 'next') or @data-cy='next' or @data-testid='next' or @class='next'] | //input[@type='button'][contains(translate(@value, 'NEXT', 'next'), 'next')]"));
                    var visibleElement = xpathElements.FirstOrDefault(e => e.Displayed && e.Enabled);
                    if (visibleElement != null)
                    {
                        Log.Information("Found next button with XPath");
                        return visibleElement;
                    }
                }
                catch (Exception)
                {
                    // Ignore XPath errors
                }

                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error finding next button");
                return null;
            }
        }

        // Review button finder
        public IWebElement? FindReviewButton()
        {
            try
            {
                // Try multiple selectors for review buttons
                var reviewSelectors = new[]
                {
                    "button[data-cy='review-button']",
                    "button[data-testid='review-button']",
                    "button[class*='review']",
                    "button[title*='Review']",
                    "button[aria-label*='Review']",
                    "button[class*='btn-primary'][type='button']",
                    "button[class*='continue']"
                };

                foreach (var selector in reviewSelectors)
                {
                    try
                    {
                        var elements = _driver.FindElements(By.CssSelector(selector));
                        var visibleElement = elements.FirstOrDefault(e => e.Displayed && e.Enabled);
                        if (visibleElement != null)
                        {
                            Log.Information("Found review button with selector: {Selector}", selector);
                            return visibleElement;
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        continue;
                    }
                }

                // Try XPath for review buttons
                try
                {
                    var xpathElements = _driver.FindElements(By.XPath("//button[contains(translate(text(), 'REVIEW', 'review'), 'review') or @data-cy='review' or @data-testid='review']"));
                    var visibleElement = xpathElements.FirstOrDefault(e => e.Displayed && e.Enabled);
                    if (visibleElement != null)
                    {
                        Log.Information("Found review button with XPath");
                        return visibleElement;
                    }
                }
                catch (Exception)
                {
                    // Ignore XPath errors
                }

                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error finding review button");
                return null;
            }
        }

        // Composite method to find next or review button
        public IWebElement? FindNextOrReviewButton()
        {
            return FindReviewButton() ?? FindNextButton();
        }

        // Check if additional questions module exists
        public bool HasAdditionalQuestionsModule()
        {
            try
            {
                var questionSelectors = new[]
                {
                    "div[data-cy='additional-questions']",
                    "div[class*='questions']",
                    "div[class*='form-section']",
                    "form[class*='application-form']"
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

        // Check if form has empty required fields
        public bool HasEmptyRequiredFields()
        {
            try
            {
                var emptyFieldSelectors = new[]
                {
                    "input[required]:empty",
                    "textarea[required]:empty",
                    "select[required] option[selected][value='']",
                    "input[aria-required='true']:empty",
                    "textarea[aria-required='true']:empty"
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

        // Check if page contains "Application Submitted" text
        public bool HasApplicationSubmittedText()
        {
            try
            {
                var pageText = _driver.FindElement(By.TagName("body")).Text;
                return pageText.Contains("Application Submitted", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "Error checking for Application Submitted text");
                return false;
            }
        }

        // Check for and handle human verification checkbox (only when "Additional Verification Required" is present)
        public void CheckForHumanVerification()
        {
            try
            {
                // First check if "Additional Verification Required" heading is present
                var verificationHeadingSelectors = new[]
                {
                    "h1[id='heading']",
                    "h1[id='heading']:contains('Additional Verification Required')",
                    "#heading"
                };

                bool requiresVerification = false;
                foreach (var selector in verificationHeadingSelectors)
                {
                    try
                    {
                        var headingElement = _driver.FindElement(By.CssSelector(selector));
                        if (headingElement != null && headingElement.Displayed)
                        {
                            string headingText = headingElement.Text?.Trim() ?? "";
                            if (headingText.Contains("Additional Verification Required", StringComparison.OrdinalIgnoreCase))
                            {
                                requiresVerification = true;
                                Log.Information("Found 'Additional Verification Required' heading - waiting 5 seconds before continuing");
                                Thread.Sleep(5000); // Wait 5 seconds when verification required
                                Log.Information("Resume processing after verification wait");
                                break;
                            }
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        continue;
                    }
                }

                // Only proceed with verification if the heading indicates it's required
                if (!requiresVerification)
                {
                    Log.Debug("No 'Additional Verification Required' heading found - skipping verification checkbox check");
                    return;
                }

                // Look for the specific verification checkbox structure
                var verificationSelectors = new[]
                {
                    "div[id='TktRY1'] input[type='checkbox']",
                    "input[type='checkbox']",
                    ".cb-c input[type='checkbox']",
                    "label.cb-lb input[type='checkbox']",
                    "[role='alert'] input[type='checkbox']"
                };

                foreach (var selector in verificationSelectors)
                {
                    try
                    {
                        var checkbox = _driver.FindElement(By.CssSelector(selector));
                        if (checkbox != null && checkbox.Displayed && checkbox.Enabled)
                        {
                            Log.Information("Found human verification checkbox with selector: {Selector}", selector);

                            // Check if it's already checked
                            if (!checkbox.Selected)
                            {
                                Log.Information("Checking human verification checkbox");
                                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", checkbox);

                                // Wait 3 seconds after checking the checkbox for processing
                                Thread.Sleep(3000);

                                Log.Information("Successfully checked human verification checkbox");
                                return;
                            }
                            else
                            {
                                Log.Information("Human verification checkbox is already checked");
                                return;
                            }
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        continue;
                    }
                }

                Log.Warning("Human verification required but no checkbox found on this page");
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "Error checking for human verification checkbox");
            }
        }
    }
}
