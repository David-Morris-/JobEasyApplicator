using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace Jobs.EasyApply.Dice.Services
{
    public class DiceLoginService : IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly string _email;
        private readonly string _password;
        private readonly bool _headless;
        private readonly bool _reuseProfile;

        private const string DiceLoginUrl = "https://www.dice.com/dashboard/login";
        private const string DashboardUrlKey = "/dashboard";

        public DiceLoginService(string email, string password, bool headless = false, bool reuseProfile = true)
        {
            _email = email;
            _password = password;
            _headless = headless;
            _reuseProfile = reuseProfile;

            _driver = BuildDriver();

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

        private IWebDriver BuildDriver()
        {
            var options = new ChromeOptions();

            if (_headless)
            {
                options.AddArgument("--headless=new");
            }

            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");

            if (_reuseProfile)
            {
                var profilePath = $"C:\\Users\\{Environment.UserName}\\AppData\\Local\\Temp\\ChromeProfile_{Guid.NewGuid()}";
                Directory.CreateDirectory(profilePath);
                options.AddArgument($"--user-data-dir={profilePath}");
            }

            // Anti-detection options
            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalOption("useAutomationExtension", false);

            // Load custom extension to change automation message
            options.AddArgument("--load-extension=C:\\Users\\david\\Repos\\JobEasyApplicator\\ChromeExtension");

            // Suppress default automation message
            options.AddArgument("--disable-blink-features=AutomationControlled");

            return new ChromeDriver(options);
        }

        public IWebElement WaitForAny(string[] selectors, int timeoutSeconds = 15)
        {
            var endTime = DateTime.Now.AddSeconds(timeoutSeconds);

            while (DateTime.Now < endTime)
            {
                foreach (var selector in selectors)
                {
                    try
                    {
                        var element = _driver.FindElement(By.CssSelector(selector));
                        if (element != null && element.Displayed)
                        {
                            return element;
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        // Continue to next selector
                    }
                }
                Thread.Sleep(250);
            }

            throw new TimeoutException($"No selector matched within {timeoutSeconds} seconds: {string.Join(", ", selectors)}");
        }

        public void MaybeAcceptCookies()
        {
            try
            {
                // Check for iframes first
                var iframes = _driver.FindElements(By.TagName("iframe"));
                foreach (var iframe in iframes)
                {
                    try
                    {
                        _driver.SwitchTo().Frame(iframe);
                        var acceptButtons = _driver.FindElements(By.XPath("//button[contains(., 'Accept')] | //a[contains(., 'Accept')]"));
                        if (acceptButtons.Any())
                        {
                            acceptButtons.First().Click();
                            _driver.SwitchTo().DefaultContent();
                            return;
                        }
                        _driver.SwitchTo().DefaultContent();
                    }
                    catch (Exception)
                    {
                        _driver.SwitchTo().DefaultContent();
                    }
                }

                // Fallback to direct buttons
                var directButtons = _driver.FindElements(By.XPath("//button[contains(., 'Accept')] | //a[contains(., 'Accept')]"));
                if (directButtons.Any())
                {
                    directButtons.First().Click();
                }
            }
            catch (Exception ex)
            {
                Log.Debug("No cookie banner found or error accepting cookies: {Message}", ex.Message);
            }
        }

        public bool Login()
        {
            try
            {
                Log.Information("Starting Dice login process for email: {Email}", _email);

                _driver.Navigate().GoToUrl(DiceLoginUrl);
                MaybeAcceptCookies();

                // Wait for email field
                var emailSelectors = new[]
                {
                    "input[type='email']",
                    "input#email",
                    "input[name='email']",
                    "input[id*='email']"
                };

                var emailInput = WaitForAny(emailSelectors, 20);

                // Validate session before typing email
                if (!IsSessionValid())
                {
                    throw new InvalidOperationException("WebDriver session became invalid before email entry");
                }

                emailInput.Clear();

                // Type email one character at a time (like Python script)
                foreach (char c in _email)
                {
                    emailInput.SendKeys(c.ToString());
                    Thread.Sleep(100); // Small delay between characters to mimic human typing

                    // Validate session after each character
                    if (!IsSessionValid())
                    {
                        throw new InvalidOperationException("WebDriver session became invalid during email entry");
                    }
                }

                // Wait for Continue button and click
                var continueSelectors = new[]
                {
                    "button[type='submit']",
                    "button[class*='bg-interaction']"
                };

                var continueButton = WaitForAny(continueSelectors, 10);

                // Validate session before clicking continue
                if (!IsSessionValid())
                {
                    throw new InvalidOperationException("WebDriver session became invalid before clicking continue");
                }

                var form = _driver.FindElement(By.TagName("form"));
                form.Submit();

                // Validate session after form submission
                if (!IsSessionValid())
                {
                    throw new InvalidOperationException("WebDriver session became invalid after form submission");
                }

                // Wait for password page to load
                Thread.Sleep(5000); // Wait for page transition

                // Wait for password field
                var passwordSelectors = new[]
                {
                    "input[type='password']",
                    "input#password",
                    "input[name='password']",
                    "input[id*='password']"
                };

                var passwordInput = WaitForAny(passwordSelectors, 10);

                // Validate session after password field appears
                if (!IsSessionValid())
                {
                    throw new InvalidOperationException("WebDriver session became invalid after password field loaded");
                }

                // Validate session before typing password
                if (!IsSessionValid())
                {
                    throw new InvalidOperationException("WebDriver session became invalid before password entry");
                }

                passwordInput.Clear();

                // Type password one character at a time (like Python script)
                foreach (char c in _password)
                {
                    passwordInput.SendKeys(c.ToString());
                    Thread.Sleep(100); // Small delay between characters to mimic human typing

                    // Validate session after each character
                    if (!IsSessionValid())
                    {
                        throw new InvalidOperationException("WebDriver session became invalid during password entry");
                    }
                }

                // Wait for submit button
                var submitSelectors = new[]
                {
                    "button[type='submit']",
                    "button[class*='bg-interaction']"
                };

                var submitButton = WaitForAny(submitSelectors, 10);

                // Validate session before clicking submit
                if (!IsSessionValid())
                {
                    throw new InvalidOperationException("WebDriver session became invalid before clicking submit");
                }

                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", submitButton);

                // Wait for successful login and redirect to login-landing
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(25));
                wait.Until(driver =>
                    driver.Url.Contains("dashboard/login-landing") ||
                    driver.FindElements(By.XPath("//*[contains(., 'Dashboard') or contains(., 'My Profile') or contains(., 'Jobs')]")).Any()
                );

                Log.Information("Successfully logged in to Dice - redirected to: {Url}", _driver.Url);

                // Validate profile is loaded on the login-landing page
                if (_driver.Url.Contains("dashboard/login-landing"))
                {
                    // Check for profile elements
                    var profileElements = _driver.FindElements(By.XPath("//*[contains(., 'Profile') or contains(., 'Edit Profile') or contains(., 'My Profile')]"));
                    if (profileElements.Any())
                    {
                        Log.Information("Profile loaded successfully on login-landing page - login validated");
                    }
                    else
                    {
                        Log.Warning("Profile elements not found on login-landing page");
                    }
                }
                else
                {
                    Log.Warning("Did not redirect to expected login-landing page. Current URL: {Url}", _driver.Url);
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Login failed: {Message}", ex.Message);

                // Save screenshot for debugging
                var screenshotPath = Path.Combine(Directory.GetCurrentDirectory(), "dice_login_error.png");
                try
                {
                    ((ITakesScreenshot)_driver).GetScreenshot().SaveAsFile(screenshotPath);
                    Log.Information("Screenshot saved to: {Path}", screenshotPath);
                }
                catch (Exception screenshotEx)
                {
                    Log.Error("Failed to save screenshot: {Message}", screenshotEx.Message);
                }

                return false;
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

        public IWebDriver GetDriver()
        {
            return _driver;
        }

        public void Dispose()
        {
            _driver.Quit();
        }
    }
}
