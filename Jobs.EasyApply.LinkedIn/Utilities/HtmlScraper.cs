using OpenQA.Selenium;

namespace Jobs.EasyApply.LinkedIn.Utilities
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

        // Modal close button finder
        public IWebElement? FindModalCloseButton()
        {
            try
            {
                // Try multiple selectors for modal close buttons
                var closeButtonSelectors = new[]
                {
                    "button[aria-label*='Dismiss']",
                    "button[data-test-modal-close-btn]",
                    "button[aria-label*='Close']",
                    "button[class*='modal-close']",
                    ".artdeco-modal-overlay button[aria-label*='Dismiss']",
                    ".artdeco-modal-overlay button[data-test-modal-close-btn]",
                    "button[aria-label*='close']",
                    "button[data-test-id='modal-close']"
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

                // Check for h3 with "Additional Questions" text
                try
                {
                    var h3Elements = _driver.FindElements(By.CssSelector("h3"));
                    if (h3Elements.Any(h3 => h3.Displayed && h3.Text.Contains("Additional Questions")))
                    {
                        return true;
                    }
                }
                catch (NoSuchElementException)
                {
                    // Continue
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
                // First, find the additional questions module/section
                var questionModuleSelectors = new[]
                {
                    "div[class*='additional-questions']",
                    "div[class*='custom-questions']",
                    "div[class*='screening-questions']",
                    "div[data-test-form-element='form']",
                    "form[class*='questions']",
                    "div[class*='form-section']",
                    "div[role='form']"
                };

                IWebElement? questionsModule = null;
                foreach (var selector in questionModuleSelectors)
                {
                    try
                    {
                        var modules = _driver.FindElements(By.CssSelector(selector));
                        questionsModule = modules.FirstOrDefault(e => e.Displayed);
                        if (questionsModule != null)
                            break;
                    }
                    catch (NoSuchElementException)
                    {
                        continue;
                    }
                }

                // If no questions module found, assume no pre-populated questions
                if (questionsModule == null)
                {
                    return false;
                }

                // Check if form fields within the questions module are already filled with answers
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
                        // Look for elements within the questions module only
                        var elements = questionsModule.FindElements(By.CssSelector(selector));
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
                // First, let's check if this is a LinkedIn contact info form by looking for specific indicators
                bool isContactForm = IsContactInfoForm();

                if (isContactForm)
                {
                    // For contact forms, check if all required contact fields are filled
                    return !AreContactFieldsComplete();
                }

                // For other forms, use the comprehensive detection
                // Look for empty required fields with various selectors (organized to avoid duplication)
                var emptyFieldSelectors = new[]
                {
                    // Most specific selectors first (highest priority)
                    "input[data-test-form-element='input'][required]:empty",
                    "input[data-test-form-element='email'][required]:empty",
                    "input[data-test-form-element='tel'][required]:empty",
                    "textarea[data-test-form-element='textarea'][required]:empty",

                    // Type-specific selectors
                    "input[type='text'][required]:empty",
                    "input[type='email'][required]:empty",
                    "input[type='tel'][required]:empty",
                    "input[type='radio'][required]:not(:checked)",
                    "input[type='checkbox'][required]:not(:checked)",
                    "textarea[required]:empty",
                    "select[required] option[selected][value='']",

                    // Attribute-specific selectors
                    "input[aria-required='true']:empty",
                    "textarea[aria-required='true']:empty",
                    "input[data-required='true']:empty",
                    "textarea[data-required='true']:empty",

                    // Class-specific selectors
                    "input[class*='required']:empty",
                    "textarea[class*='required']:empty",

                    // General fallback selectors (lowest priority)
                    "input[required]:empty",
                    "textarea[required]:empty",
                    "select[required]:not([value])"
                };

                // Special handling for LinkedIn forms - check if fields appear filled in browser but are empty in DOM
                // This handles cases where LinkedIn pre-fills values but doesn't set the value attribute
                var prefilledButEmptyInDOM = CheckForPrefilledButEmptyInDOMFields();
                if (prefilledButEmptyInDOM.Any())
                {
                    Console.WriteLine($"Found {prefilledButEmptyInDOM.Count} fields that appear pre-filled in browser but are empty in DOM");
                    // Don't treat these as empty since they appear filled to the user
                    return false;
                }

                // Use a set to track unique elements and avoid duplication
                var foundEmptyElements = new HashSet<string>();
                var foundEmptySelectors = new List<string>();

                foreach (var selector in emptyFieldSelectors)
                {
                    try
                    {
                        var elements = _driver.FindElements(By.CssSelector(selector));
                        foreach (var element in elements.Where(e => e.Displayed))
                        {
                            // Create a unique identifier for this element
                            string elementId = $"{element.TagName}_{element.GetAttribute("type")}_{element.GetAttribute("name")}_{element.GetAttribute("id")}_{element.GetAttribute("class")}";

                            if (IsFieldActuallyEmpty(element) && !foundEmptyElements.Contains(elementId))
                            {
                                foundEmptyElements.Add(elementId);
                                foundEmptySelectors.Add(selector);
                                Console.WriteLine($"Found empty field with selector: {selector}, tag: {element.TagName}, type: {element.GetAttribute("type")}, name: {element.GetAttribute("name")}, id: {element.GetAttribute("id")}");
                            }
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        // Continue to next selector
                        continue;
                    }
                }

                if (foundEmptyElements.Any())
                {
                    Console.WriteLine($"Found {foundEmptyElements.Count} unique empty required fields: {string.Join(", ", foundEmptySelectors.Distinct())}");
                    return true;
                }

                // Additional debugging: Check all visible form fields to see what's actually on the page
                Console.WriteLine("Debugging: Checking all visible form fields on page...");
                try
                {
                    var allInputs = _driver.FindElements(By.CssSelector("input, textarea, select"));
                    var visibleFormFields = allInputs.Where(e => e.Displayed).ToList();

                    Console.WriteLine($"Found {visibleFormFields.Count} total visible form fields:");

                    // Check for text inputs specifically since that's what the user mentioned
                    var textInputs = visibleFormFields.Where(f => f.TagName.ToLower() == "input" &&
                                                                (f.GetAttribute("type")?.ToLower() == "text" || f.GetAttribute("type") == null || f.GetAttribute("type") == ""));
                    Console.WriteLine($"Found {textInputs.Count()} text input fields:");

                    foreach (var field in textInputs)
                    {
                        string tagName = field.TagName;
                        string type = field.GetAttribute("type") ?? "";
                        string value = field.GetAttribute("value") ?? "";
                        string placeholder = field.GetAttribute("placeholder") ?? "";
                        string required = field.GetAttribute("required") ?? "";
                        string ariaLabel = field.GetAttribute("aria-label") ?? "";
                        string className = field.GetAttribute("class") ?? "";
                        string name = field.GetAttribute("name") ?? "";
                        string id = field.GetAttribute("id") ?? "";

                        Console.WriteLine($"  TEXT INPUT: type='{type}' value='{value}' placeholder='{placeholder}' required='{required}' name='{name}' id='{id}' aria-label='{ariaLabel}'");

                        // Check if this field should be considered empty
                        bool isEmpty = IsFieldActuallyEmpty(field);
                        Console.WriteLine($"    -> IsFieldActuallyEmpty() returns: {isEmpty}");

                        if (isEmpty)
                        {
                            Console.WriteLine($"    -> *** THIS FIELD IS DETECTED AS EMPTY ***");
                        }
                    }

                    // Also check all other visible fields
                    var otherFields = visibleFormFields.Except(textInputs);
                    Console.WriteLine($"Found {otherFields.Count()} other form fields:");

                    foreach (var field in otherFields.Take(5)) // Limit for readability
                    {
                        string tagName = field.TagName;
                        string type = field.GetAttribute("type") ?? "";
                        string value = field.GetAttribute("value") ?? "";
                        string placeholder = field.GetAttribute("placeholder") ?? "";
                        string required = field.GetAttribute("required") ?? "";
                        string ariaLabel = field.GetAttribute("aria-label") ?? "";
                        string className = field.GetAttribute("class") ?? "";

                        Console.WriteLine($"  {tagName}: type='{type}' value='{value}' placeholder='{placeholder}' required='{required}' aria-label='{ariaLabel}'");
                    }

                    if (otherFields.Count() > 5)
                    {
                        Console.WriteLine($"  ... and {otherFields.Count() - 5} more fields");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during form field debugging: {ex.Message}");
                }

                // Check for required field indicators that might indicate missing answers
                var requiredIndicatorSelectors = new[]
                {
                    "label[class*='required']",
                    "span[class*='required']",
                    "div[class*='required']",
                    "*[class*='required-field']",
                    "*[class*='mandatory']",
                    "*[class*='error']",
                    "*[class*='validation-error']"
                };

                foreach (var selector in requiredIndicatorSelectors)
                {
                    try
                    {
                        var indicators = _driver.FindElements(By.CssSelector(selector));
                        if (indicators.Any(e => e.Displayed))
                        {
                            Console.WriteLine($"Found required field indicator: {selector}");
                            // If we find required indicators or error messages, assume there are empty fields
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking for empty required fields: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Check if this is a LinkedIn contact information form
        /// </summary>
        private bool IsContactInfoForm()
        {
            try
            {
                // Look for LinkedIn contact form indicators - more specific selectors
                var contactFormSelectors = new[]
                {
                    // LinkedIn specific contact form selectors
                    "div[data-test-form-element='form']",
                    "form[class*='contact']",
                    "div[class*='contact-info']",
                    "div[class*='contact-information']",
                    "div[class*='personal-contact']",
                    "div[class*='profile-contact']",

                    // Header elements that might indicate contact form
                    "h2[class*='contact']",
                    "h3[class*='contact']",
                    "legend[class*='contact']",
                    "div[class*='profile-info']",
                    "div[class*='personal-info']",

                    // LinkedIn Easy Apply specific selectors
                    "div[data-easy-apply-contact-info]",
                    "div[class*='easy-apply-contact']",
                    "div[class*='application-contact']",
                    "div[class*='applicant-contact']"
                };

                foreach (var selector in contactFormSelectors)
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

                // Check for specific LinkedIn contact form text - more comprehensive
                var contactTextSelectors = new[]
                {
                    "h1", "h2", "h3", "h4", "h5", "h6",
                    "legend",
                    "div[class*='header']",
                    "span[class*='title']",
                    "div[class*='form-title']"
                };

                foreach (var selector in contactTextSelectors)
                {
                    try
                    {
                        var elements = _driver.FindElements(By.CssSelector(selector));
                        foreach (var element in elements.Where(e => e.Displayed))
                        {
                            string text = element.Text?.ToLower() ?? "";
                            if (text.Contains("contact") &&
                                (text.Contains("info") || text.Contains("information") ||
                                 text.Contains("details") || text.Contains("profile")))
                            {
                                return true;
                            }
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        continue;
                    }
                }

                // Additional check: Look for email and phone fields together (strong indicator of contact form)
                var emailFields = _driver.FindElements(By.CssSelector("input[type='email'], input[data-test-form-element='email']"));
                var phoneFields = _driver.FindElements(By.CssSelector("input[type='tel'], input[data-test-form-element='tel']"));

                if (emailFields.Any(e => e.Displayed) && phoneFields.Any(p => p.Displayed))
                {
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Check if all required contact fields are complete
        /// </summary>
        private bool AreContactFieldsComplete()
        {
            try
            {
                // Check email field - LinkedIn usually pre-fills this from profile
                var emailSelectors = new[]
                {
                    // Most specific LinkedIn selectors first
                    "input[data-test-form-element='email'][required]",
                    "input[data-test-form-element='email']",
                    "input[type='email'][required]",
                    "input[type='email']",
                    "input[name*='email'][required]",
                    "input[name*='email']",
                    "input[placeholder*='email'][required]",
                    "input[placeholder*='email']",
                    "input[aria-label*='email'][required]",
                    "input[aria-label*='email']",
                    "input[class*='email'][required]",
                    "input[class*='email']",
                    // More general selectors
                    "input[required]",
                    "input"
                };

                bool emailFilled = false;
                foreach (var selector in emailSelectors)
                {
                    try
                    {
                        var emailFields = _driver.FindElements(By.CssSelector(selector));
                        var visibleEmailFields = emailFields.Where(e => e.Displayed);
                        if (visibleEmailFields.Any())
                        {
                            emailFilled = visibleEmailFields.Any(e => !IsFieldActuallyEmpty(e));
                            if (emailFilled)
                            {
                                // Log which selector worked for debugging
                                Console.WriteLine($"Email field detected as filled using selector: {selector}");
                                break;
                            }
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        continue;
                    }
                }

                // Check phone field - this might be optional or pre-filled
                var phoneSelectors = new[]
                {
                    // Most specific LinkedIn selectors first
                    "input[data-test-form-element='tel'][required]",
                    "input[data-test-form-element='tel']",
                    "input[type='tel'][required]",
                    "input[type='tel']",
                    "input[name*='phone'][required]",
                    "input[name*='phone']",
                    "input[placeholder*='phone'][required]",
                    "input[placeholder*='phone']",
                    "input[aria-label*='phone'][required]",
                    "input[aria-label*='phone']",
                    "input[class*='phone'][required]",
                    "input[class*='phone']"
                };

                bool phoneFilled = false;
                foreach (var selector in phoneSelectors)
                {
                    try
                    {
                        var phoneFields = _driver.FindElements(By.CssSelector(selector));
                        // Phone might not be required, so check if field exists and if it does, check if it's filled
                        var visiblePhoneFields = phoneFields.Where(e => e.Displayed);
                        if (visiblePhoneFields.Any())
                        {
                            phoneFilled = visiblePhoneFields.Any(e => !IsFieldActuallyEmpty(e));
                            if (phoneFilled)
                            {
                                Console.WriteLine($"Phone field detected as filled using selector: {selector}");
                                break;
                            }
                        }
                        else
                        {
                            // No phone field found, assume it's not required for this form
                            phoneFilled = true;
                            Console.WriteLine("No phone field found, assuming not required");
                            break;
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        continue;
                    }
                }

                // Check country/region field - LinkedIn usually pre-fills this too
                var countrySelectors = new[]
                {
                    // Most specific LinkedIn selectors first
                    "select[data-test-form-element='select'][required]",
                    "select[data-test-form-element='select']",
                    "select[name*='country'][required]",
                    "select[name*='country']",
                    "select[name*='region'][required]",
                    "select[name*='region']",
                    "select[aria-label*='country'][required]",
                    "select[aria-label*='country']",
                    "select[class*='country'][required]",
                    "select[class*='country']",
                    "select[required]",
                    "select"
                };

                bool countryFilled = false;
                foreach (var selector in countrySelectors)
                {
                    try
                    {
                        var countryFields = _driver.FindElements(By.CssSelector(selector));
                        var visibleCountryFields = countryFields.Where(e => e.Displayed);
                        if (visibleCountryFields.Any())
                        {
                            countryFilled = visibleCountryFields.Any(e => !IsFieldActuallyEmpty(e));
                            if (countryFilled)
                            {
                                Console.WriteLine($"Country field detected as filled using selector: {selector}");
                                break;
                            }
                        }
                        else
                        {
                            // No country field found, assume it's not required for this form
                            countryFilled = true;
                            Console.WriteLine("No country field found, assuming not required");
                            break;
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        continue;
                    }
                }

                Console.WriteLine($"Contact fields status - Email: {emailFilled}, Phone: {phoneFilled}, Country: {countryFilled}");

                return emailFilled && phoneFilled && countryFilled;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking contact fields completion: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Helper method to check if a field is actually empty (more comprehensive check)
        /// </summary>
        private bool IsFieldActuallyEmpty(IWebElement element)
        {
            try
            {
                string tagName = element.TagName.ToLower();
                string type = element.GetAttribute("type")?.ToLower() ?? "";

                switch (tagName)
                {
                    case "input":
                        if (type == "radio" || type == "checkbox")
                        {
                            return !element.Selected;
                        }
                        else
                        {
                            // Check multiple ways that LinkedIn might store values
                            string value = element.GetAttribute("value") ?? "";
                            string text = element.Text ?? "";
                            string innerText = element.GetAttribute("innerText") ?? "";
                            string innerHTML = element.GetAttribute("innerHTML") ?? "";

                            // Consider empty if all possible value sources are empty or contain only whitespace
                            if (string.IsNullOrWhiteSpace(value) &&
                                string.IsNullOrWhiteSpace(text) &&
                                string.IsNullOrWhiteSpace(innerText) &&
                                string.IsNullOrWhiteSpace(innerHTML))
                                return true;

                            // Check for placeholder-only values (common in LinkedIn forms)
                            string placeholder = element.GetAttribute("placeholder") ?? "";
                            if (!string.IsNullOrWhiteSpace(placeholder) &&
                                (value == placeholder || text == placeholder || innerText == placeholder))
                                return true;

                            // Check for common LinkedIn placeholder values
                            var placeholderValues = new[]
                            {
                                "Enter your email", "Enter email", "Enter your phone", "Enter phone",
                                "Select country", "Select region", "Type your answer here", "Add your answer",
                                "Please specify", "Enter text", "Your answer", "Answer"
                            };

                            if (placeholderValues.Any(pv => value == pv || text == pv || innerText == pv))
                                return true;

                            // Check for very short values that might be placeholders
                            if (!string.IsNullOrWhiteSpace(value) && value.Length < 3)
                            {
                                var shortPlaceholderWords = new[] { "N/A", "NA", "n/a", "na", "-", "--", "...", "?", "??", "???", "None", "none" };
                                if (shortPlaceholderWords.Contains(value))
                                    return true;
                            }

                            // For text inputs, be more strict about what constitutes "filled"
                            if (type == "text" || type == "" || type == null)
                            {
                                // Must have actual meaningful content
                                if (string.IsNullOrWhiteSpace(value))
                                    return true;

                                // Check for common placeholder patterns and default values
                                var placeholderPatterns = new[]
                                {
                                    "enter", "type", "add", "your", "please", "optional", "example", "sample", "test",
                                    "n/a", "na", "none", "null", "undefined", "default", "placeholder", "temp",
                                    "lorem", "ipsum", "foo", "bar", "baz", "xxx", "yyy", "zzz", "abc"
                                };

                                var lowerValue = value.ToLower();
                                if (placeholderPatterns.Any(pattern => lowerValue.Contains(pattern)))
                                {
                                    // Additional check: if it contains multiple placeholder words, likely not real content
                                    var placeholderCount = placeholderPatterns.Count(pattern => lowerValue.Contains(pattern));
                                    if (placeholderCount > 0 && value.Length < 20)
                                        return true;
                                }

                                // Check for very short values that are likely placeholders
                                if (value.Length < 3)
                                {
                                    var shortValues = new[] { "...", "..", ".", "-", "--", "_", "__", "?", "??", "???", "!", "!!" };
                                    if (shortValues.Contains(value))
                                        return true;
                                }

                                // Allow numeric-only values - these are often legitimate answers (like "8", "2", etc.)
                                // Only consider numeric values as potentially empty if they match specific placeholder patterns
                                if (value.All(char.IsDigit))
                                {
                                    var numericPlaceholders = new[] { "123", "456", "789", "000", "111", "222", "333", "444", "555", "666", "777", "888", "999" };
                                    if (numericPlaceholders.Contains(value))
                                        return true;
                                    // Single digits and reasonable numbers are likely real answers, so don't reject them
                                }

                                // Check for repetitive characters (likely placeholders)
                                if (value.Length > 2 && value.Distinct().Count() < 3)
                                {
                                    var repetitivePatterns = new[] { "aaa", "bbb", "ccc", "ddd", "xxx", "yyy", "zzz" };
                                    if (repetitivePatterns.Any(pattern => lowerValue.Contains(pattern)))
                                        return true;
                                }

                                // Check for obviously fake content
                                if (lowerValue.Contains("johndoe") || lowerValue.Contains("janedoe") ||
                                    lowerValue.Contains("testuser") || lowerValue.Contains("example.com"))
                                    return true;
                            }

                            // If we have any meaningful value, consider it filled
                            if (!string.IsNullOrWhiteSpace(value) && value.Length > 2)
                                return false;

                            return true;
                        }

                    case "textarea":
                        string textValue = element.Text ?? "";
                        string textareaValue = element.GetAttribute("value") ?? "";
                        string textareaInnerText = element.GetAttribute("innerText") ?? "";
                        string textareaInnerHTML = element.GetAttribute("innerHTML") ?? "";

                        // Consider empty if all possible value sources are empty
                        if (string.IsNullOrWhiteSpace(textValue) &&
                            string.IsNullOrWhiteSpace(textareaValue) &&
                            string.IsNullOrWhiteSpace(textareaInnerText) &&
                            string.IsNullOrWhiteSpace(textareaInnerHTML))
                            return true;

                        return false;

                    case "select":
                        // Try multiple ways to find selected option
                        var selectedElements = element.FindElements(By.CssSelector("option[selected], option:checked"));
                        if (selectedElements.Any())
                        {
                            foreach (var selectedOption in selectedElements)
                            {
                                string optionValue = selectedOption.GetAttribute("value") ?? "";
                                string optionText = selectedOption.Text ?? "";
                                string optionInnerText = selectedOption.GetAttribute("innerText") ?? "";

                                // Consider filled if we have a meaningful value
                                if (!string.IsNullOrWhiteSpace(optionValue) &&
                                    optionValue != "0" &&
                                    optionValue != "-1" &&
                                    !optionValue.StartsWith("Select") &&
                                    !optionValue.StartsWith("Choose"))
                                {
                                    return false;
                                }

                                if (!string.IsNullOrWhiteSpace(optionText) &&
                                    !optionText.StartsWith("Select") &&
                                    !optionText.StartsWith("Choose"))
                                {
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            // Try finding by value attribute
                            var allOptions = element.FindElements(By.CssSelector("option"));
                            var optionsWithAttributes = allOptions.Where(o => o.GetAttribute("selected") != null || o.GetAttribute("checked") != null);
                            if (optionsWithAttributes.Any())
                            {
                                foreach (var selectedOption in optionsWithAttributes)
                                {
                                    string optionValue = selectedOption.GetAttribute("value") ?? "";
                                    string optionText = selectedOption.Text ?? "";

                                    if (!string.IsNullOrWhiteSpace(optionValue) &&
                                        optionValue != "0" &&
                                        optionValue != "-1" &&
                                        !optionValue.StartsWith("Select") &&
                                        !optionValue.StartsWith("Choose"))
                                    {
                                        return false;
                                    }

                                    if (!string.IsNullOrWhiteSpace(optionText) &&
                                        !optionText.StartsWith("Select") &&
                                        !optionText.StartsWith("Choose"))
                                    {
                                        return false;
                                    }
                                }
                            }
                            else
                            {
                                // Check if there's a default selected option
                                var defaultSelected = element.FindElements(By.CssSelector("option[selected]")).FirstOrDefault();
                                if (defaultSelected != null)
                                {
                                    string defaultValue = defaultSelected.GetAttribute("value") ?? "";
                                    if (!string.IsNullOrWhiteSpace(defaultValue) && defaultValue != "0" && defaultValue != "-1")
                                        return false;
                                }
                            }
                        }

                        return true; // No meaningful option selected

                    default:
                        return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Helper method to check if a field is empty (basic check)
        /// </summary>
        private bool IsFieldEmpty(IWebElement element)
        {
            try
            {
                string tagName = element.TagName.ToLower();
                string type = element.GetAttribute("type")?.ToLower() ?? "";

                switch (tagName)
                {
                    case "input":
                        if (type == "radio" || type == "checkbox")
                        {
                            return !element.Selected;
                        }
                        else
                        {
                            string value = element.GetAttribute("value") ?? "";
                            return string.IsNullOrWhiteSpace(value);
                        }

                    case "textarea":
                        string textValue = element.Text ?? "";
                        return string.IsNullOrWhiteSpace(textValue);

                    case "select":
                        var selectedOption = element.FindElements(By.CssSelector("option[selected]")).FirstOrDefault();
                        if (selectedOption != null)
                        {
                            string optionValue = selectedOption.GetAttribute("value") ?? "";
                            return string.IsNullOrWhiteSpace(optionValue) || optionValue == "0" || optionValue == "-1";
                        }
                        return true; // No option selected

                    default:
                        return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Check for fields that appear pre-filled in the browser but are empty in the DOM
        /// This handles LinkedIn's dynamic form population where values are shown but not in the value attribute
        /// </summary>
        private List<IWebElement> CheckForPrefilledButEmptyInDOMFields()
        {
            var prefilledFields = new List<IWebElement>();

            try
            {
                // Look for input fields that are empty in DOM but might be visually filled
                var inputSelectors = new[]
                {
                    "input[type='text'][required]",
                    "input[type='number'][required]",
                    "input[type='email'][required]",
                    "input[type='tel'][required]",
                    "textarea[required]",
                    "select[required]"
                };

                foreach (var selector in inputSelectors)
                {
                    try
                    {
                        var elements = _driver.FindElements(By.CssSelector(selector));
                        foreach (var element in elements.Where(e => e.Displayed))
                        {
                            // Check if field appears empty in DOM but might be visually filled
                            if (IsFieldActuallyEmpty(element))
                            {
                                // Additional check: see if the field has any visual indicators of being filled
                                // LinkedIn sometimes shows values in the UI but not in the DOM

                                // Check if the field has a specific class that indicates it's filled
                                string className = element.GetAttribute("class") ?? "";
                                if (className.Contains("filled") || className.Contains("prefilled") || className.Contains("complete"))
                                {
                                    Console.WriteLine($"Field appears filled based on class: {className}");
                                    prefilledFields.Add(element);
                                    continue;
                                }

                                // Check if there's a parent element that indicates the field is filled
                                var parent = element.FindElement(By.XPath(".."));
                                string parentClass = parent.GetAttribute("class") ?? "";
                                if (parentClass.Contains("filled") || parentClass.Contains("prefilled") || parentClass.Contains("complete"))
                                {
                                    Console.WriteLine($"Field appears filled based on parent class: {parentClass}");
                                    prefilledFields.Add(element);
                                    continue;
                                }

                                // Check for LinkedIn-specific indicators
                                // LinkedIn often uses aria attributes or data attributes to indicate filled fields
                                string ariaValue = element.GetAttribute("aria-valuetext") ?? "";
                                string dataValue = element.GetAttribute("data-value") ?? "";
                                string ariaLabel = element.GetAttribute("aria-label") ?? "";

                                if (!string.IsNullOrWhiteSpace(ariaValue) || !string.IsNullOrWhiteSpace(dataValue))
                                {
                                    Console.WriteLine($"Field appears filled based on aria/data attributes: aria-valuetext='{ariaValue}', data-value='{dataValue}'");
                                    prefilledFields.Add(element);
                                    continue;
                                }

                                // Check if the field has a sibling element that shows the value
                                // LinkedIn sometimes displays values in separate elements
                                var siblings = parent.FindElements(By.XPath("*"));
                                foreach (var sibling in siblings)
                                {
                                    string siblingText = sibling.Text ?? "";
                                    string siblingClass = sibling.GetAttribute("class") ?? "";

                                    // Look for siblings that might contain the actual value
                                    if (siblingClass.Contains("value") || siblingClass.Contains("text") || siblingClass.Contains("display"))
                                    {
                                        if (!string.IsNullOrWhiteSpace(siblingText) &&
                                            (siblingText.All(char.IsDigit) || siblingText.Length > 1)) // Likely a real value
                                        {
                                            Console.WriteLine($"Field appears filled based on sibling element: '{siblingText}'");
                                            prefilledFields.Add(element);
                                            break;
                                        }
                                    }
                                }

                                // Check if the field has focus or cursor position that suggests it's filled
                                // This is a long shot but worth checking
                                string style = element.GetAttribute("style") ?? "";
                                if (style.Contains("cursor") || style.Contains("caret"))
                                {
                                    Console.WriteLine("Field appears filled based on cursor/caret styling");
                                    prefilledFields.Add(element);
                                }
                            }
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking for prefilled fields: {ex.Message}");
            }

            return prefilledFields;
        }
    }
}
