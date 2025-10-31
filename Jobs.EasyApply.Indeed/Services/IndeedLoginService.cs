using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace Jobs.EasyApply.Indeed.Services
{
    public class IndeedLoginService : IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly string _email;
        private readonly string _password;
        private readonly bool _headless;
        private readonly bool _reuseProfile;

        private const string IndeedLoginUrl = "https://secure.indeed.com/account/login";
        private const string DashboardUrlKey = "/account";

        public IndeedLoginService(string email, string password, bool headless = false, bool reuseProfile = true)
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
                Log.Information("Starting Indeed login process for email: {Email}", _email);

                _driver.Navigate().GoToUrl(IndeedLoginUrl);
                MaybeAcceptCookies();

                // Wait for user to complete the entire login process manually
                Log.Information("*** LOGIN PROCESS - USER ACTION REQUIRED ***");
                Console.WriteLine("*** LOGIN PROCESS - USER ACTION REQUIRED ***");
                Console.WriteLine("Please complete the entire login process manually in the browser:");
                Console.WriteLine("1. Enter your email address");
                Console.WriteLine("2. Complete any passkey authentication or other security steps");
                Console.WriteLine("3. Navigate to a page that shows you are logged in (dashboard, job search, etc.)");
                Console.WriteLine("Press ENTER in this console when the login is fully complete and you are logged in.");

                var loginUserInput = Console.ReadLine();
                if (loginUserInput == null)
                {
                    Log.Warning("No user input received for login completion");
                }

                Log.Information("User indicates login process has been completed");

                // Validate login success
                try
                {
                    Thread.Sleep(3000); // Wait for any final redirects

                    var currentUrl = _driver.Url;
                    var pageSource = _driver.PageSource;

                    if (currentUrl.Contains("dashboard") || currentUrl.Contains("mynetwork") ||
                        pageSource.Contains("Dashboard") || pageSource.Contains("My jobs") ||
                        pageSource.Contains("Search jobs") || pageSource.Contains("Profile") ||
                        pageSource.Contains("Welcome"))
                    {
                        Log.Information("Login validated - user is on a logged-in page: {Url}", currentUrl);
                        return true;
                    }
                    else
                    {
                        Log.Warning("Login validation inconclusive - current URL: {Url}", currentUrl);
                        // Still return true since user indicated completion, but log the issue
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error validating login");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Login failed: {Message}", ex.Message);

                // Save screenshot for debugging
                var screenshotPath = Path.Combine(Directory.GetCurrentDirectory(), "indeed_login_error.png");
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
                var currentUrl = _driver.Url;
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
