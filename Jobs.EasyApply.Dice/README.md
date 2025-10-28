# Jobs.EasyApply.Dice - Enterprise Dice Job Application Automation

A sophisticated .NET console application that automates job searching and application processes on Dice.com, with advanced Easy Apply job filtering, comprehensive error handling, and robust API integration.

## 🏆 Key Features

### 🎯 Easy Apply Job Filtering
- **Intelligent Detection**: Automatically filters job search results to only include Easy Apply jobs
- **Advanced Detection Strategies**: Uses multiple indicators (badges, CSS classes, text content, data attributes)
- **Dice-Specific Logic**: Optimized for Dice.com's Easy Apply implementation
- **High-Precision Filtering**: Minimizes manual review by focusing on truly automated applications

### 🔍 Advanced Job Search
- **Automated Search**: Searches Dice.com for jobs matching configurable criteria
- **Real-time Filtering**: Applies Easy Apply filter (`applyType=easy_apply`) during search
- **Flexible Parameters**: Supports job title and location customization via command line or configuration
- **Results Validation**: Ensures search results contain all required job information

### 💡 Smart Application Process
- **Multi-step Form Handling**: Manages complex application forms requiring multiple submissions
- **Manual Intervention Support**: Prompts for manual input when additional questions require completion
- **Anti-Detection Measures**: Includes browser fingerprint randomization and stealth techniques
- **Error Recovery**: Continues processing remaining jobs if individual applications fail

### 📊 API Integration
- **Comprehensive Tracking**: Records all applications via REST API with full job details
- **Connectivity Validation**: Tests API connectivity before starting job search
- **Duplicate Prevention**: Checks for previously applied jobs via API
- **Provider Tracking**: All applications tagged with Dice provider identifier

### 📝 Comprehensive Logging
- **Structured Logging**: Detailed logging using Serilog for monitoring and debugging
- **Multiple Log Levels**: Information, Warning, Error, and Debug levels
- **Application Metrics**: Tracks total jobs found, applied, and success rates
- **API Communication Logs**: Detailed logging of all API interactions

## 🎯 Easy Apply Jobs Explained

This application is specifically designed to work with Dice.com's Easy Apply jobs, which offer:
- **Direct Application**: Jobs that can be applied to without leaving Dice.com
- **Simplified Process**: Streamlined application workflows with pre-filled information
- **Profile Integration**: Often pre-populate candidate information from Dice profiles
- **Faster Turnaround**: Typically have accelerated application processing times

### How Easy Apply Detection Works
- **Badge Recognition**: Identifies `data-cy="easy-apply-badge"` and similar data attributes
- **CSS Class Analysis**: Scans for classes containing "easy-apply", "quick-apply", "instant-apply"
- **Text Content Search**: Looks for "Easy Apply", "Quick Apply", "Instant Apply" text
- **Button Type Analysis**: Ensures Apply buttons don't redirect to external sites

## 🔍 Advanced Job Processing

### Application Flow
1. **Search Execution**: Navigates to Dice.com with Easy Apply filter and search parameters
2. **Results Parsing**: Extracts job listings with Easy Apply indicators only
3. **Eligibility Validation**: Filters out previously applied positions
4. **Application Process**: For each Easy Apply job:
   - Initiates application workflow
   - Handles multi-step form navigation
   - Manages manual intervention requirements
   - Records application results via API

### Technical Implementation
- **Web Automation**: Selenium WebDriver for browser interaction
- **Element Detection**: Advanced selectors and fallback strategies
- **Error Handling**: Comprehensive exception management with graceful degradation
- **Rate Limiting**: Built-in delays to prevent detection and maintain platform compliance

## ⚙️ Configuration

### Job Search Configuration (appsettings.json)

```json
{
  "Credentials": {
    "Email": "your-dice-email@example.com",
    "Password": "your-dice-password"
  },
  "JobSearchParams": {
    "Title": ".NET Developer",
    "Location": "Remote"
  }
}
```

### Secure Credential Management

**⚠️ Important**: Credentials are currently stored in `appsettings.json`. For production use, consider:

#### Environment Variables (Recommended for Production)
```bash
# Windows
set DiceCredentials__Email=your-dice-email@example.com
set DiceCredentials__Password=your-dice-password

# Linux/macOS
export DiceCredentials__Email=your-dice-email@example.com
export DiceCredentials__Password=your-dice-password
```

#### Future Enhancement: .NET User Secrets
We recommend implementing .NET User Secrets similar to the LinkedIn project for development environments.

### Configuration Sections

#### JobSearchParams
- **Title**: Default job title to search for (can be overridden via command line)
- **Location**: Default job location to search in (can be overridden via command line)

#### Credentials
- **Email**: Dice.com account email address
- **Password**: Dice.com account password

## Usage

### Command Line
```bash
# Use default settings from appsettings.json
dotnet run

# Override job title and location
dotnet run "Software Engineer" "New York"
```

### Programmatic
```csharp
using Jobs.EasyApply.Dice.Services;

var scraper = new JobScraper("Software Engineer", "Remote", "email@example.com", "password");
var jobs = scraper.SearchJobs(); // Only returns Easy Apply jobs

foreach (var job in jobs)
{
    bool success = scraper.ApplyForJob(job);
    // Application result tracked via API
}
```

## How It Works

1. **Job Search**: Navigates to Dice.com with Easy Apply filter (`applyType=easy_apply`)
2. **Easy Apply Detection**: Scans job cards for Easy Apply indicators (badges, text, CSS classes)
3. **Application Process**: For each Easy Apply job:
   - Clicks Apply button
   - Handles multi-step forms
   - Prompts for manual input if additional questions require completion
   - Submits application
   - Tracks result via API

## Easy Apply Indicators

The application looks for these indicators to identify Easy Apply jobs:
- `data-cy="easy-apply-badge"` or similar data attributes
- CSS classes containing "easy-apply", "quick-apply", "instant-apply"
- Text content containing "Easy Apply", "Quick Apply", "Instant Apply"
- Apply buttons that don't redirect to external sites

## API Integration

Applications are tracked via HTTP API calls to:
- `GET /api/jobs/test-connection` - Test API connectivity
- `GET /api/jobs/check/{jobId}` - Check if job was previously applied to
- `POST /api/jobs` - Record job application with details

## Dependencies

- .NET 9.0
- Selenium WebDriver
- ChromeDriver
- Serilog
- Microsoft.Extensions.Configuration

## Building

```bash
# Build Dice project only
dotnet build Jobs.EasyApply.Dice/Jobs.EasyApply.Dice.csproj

# Build entire solution
dotnet build Jobs.EasyApply.sln
```

## Notes

- Ensure Chrome browser is installed
- Configure Dice.com credentials in `appsettings.json`
- API server must be running on `http://localhost:5070`
- Manual intervention may be required for complex application questions
- Application respects rate limits and includes delays to avoid being blocked

## 🛠️ Technologies Used

### Core Technologies
- **.NET 9.0**: Modern .NET runtime with performance optimizations
- **C#**: Primary programming language
- **Console Application**: Command-line interface for job automation

### Web Automation
- **Selenium WebDriver**: Browser automation for Dice.com interaction
- **ChromeDriver**: WebDriver implementation for Chrome browser
- **OpenQA.Selenium**: .NET bindings for Selenium

### Configuration & Logging
- **Microsoft.Extensions.Configuration**: Configuration management
- **Microsoft.Extensions.Configuration.Json**: JSON configuration provider
- **Serilog**: Structured logging framework
- **Serilog.Sinks.Console**: Console output for logs

### API Integration
- **System.Net.Http**: HTTP client for API communication
- **System.Net.Http.Json**: JSON serialization for API requests

### Additional Dependencies
- **Microsoft.Extensions.DependencyInjection**: Dependency injection container (architecture preparation)
- **Microsoft.Extensions.Hosting**: Hosting abstractions for future enhancements

## 🏗️ Architecture

The Dice application follows a modular architecture designed for maintainability and extensibility:

```
📁 Jobs.EasyApply.Dice/
├── 📁 Services/                # Core business logic services
│   ├── DiceJobServiceFactory.cs # Factory for creating job services
│   ├── DiceLoginService.cs     # Dice.com authentication
│   ├── JobApplicator.cs        # Handles job application workflow
│   └── JobScraper.cs           # Manages job searching and scraping
├── 📁 Utilities/               # Web interaction utilities
│   └── HtmlScraper.cs          # HTML parsing and element interaction
├── Program.cs                  # Application entry point
├── appsettings.json            # Configuration file
└── Jobs.EasyApply.Dice.csproj  # Project file
```

### Architecture Components

#### **DiceJobServiceFactory (Factory Pattern)**
- Creates and configures job scraper and applicator services
- Handles dependency injection and service lifetime management
- Provides consistent service instantiation across the application

#### **JobScraper (Core Service)**
- Implements `IJobScraper` interface from Common library
- Handles Dice.com navigation and job search execution
- Applies Easy Apply filtering during job discovery
- Extracts job listings with complete metadata

#### **JobApplicator (Application Logic)**
- Manages the job application workflow
- Handles multi-step form submissions
- Implements error recovery and retry logic
- Coordinates with API for application tracking

#### **DiceLoginService (Authentication)**
- Manages Dice.com authentication process
- Handles login form submission and session management
- Implements retry logic for authentication failures

#### **HtmlScraper (Utility)**
- Provides advanced HTML parsing capabilities
- Implements robust element detection strategies
- Handles Dice.com-specific DOM structures

## 📐 Design Patterns

The Dice application implements key design patterns for maintainability:

### Factory Pattern
- **Purpose**: Creates job services with proper configuration and dependencies
- **Implementation**: `DiceJobServiceFactory` creates `IJobScraper` implementations
- **Benefits**: Decouples service creation from usage, enables testing and configuration

### Service Layer Pattern
- **Purpose**: Organizes business logic into focused, single-responsibility services
- **Implementation**: Separate services for scraping, application, authentication
- **Benefits**: Maintains separation of concerns and enables reusability

### Retry Pattern
- **Purpose**: Handles transient failures in web automation
- **Implementation**: Built into login and application processes
- **Benefits**: Improves reliability in unpredictable web environments

## 🚀 Getting Started

### Prerequisites

- **.NET 9.0 SDK** or later
- **Chrome Browser** (for Selenium WebDriver)
- **Dice.com Account** with valid credentials

### Installation

1. **Clone the repository**:
   ```bash
   git clone <repository-url>
   cd JobEasyApplicator
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Configure credentials** (current implementation - see security recommendations below):
   ```json
   // appsettings.json
   {
     "Credentials": {
       "Email": "your-dice-email@example.com",
       "Password": "your-dice-password"
     }
   }
   ```

4. **Build the application**:
   ```bash
   dotnet build
   ```

### Running the Application

#### Basic Usage
```bash
# Use default settings from appsettings.json
dotnet run --project Jobs.EasyApply.Dice
```

#### Individual Project Usage
```bash
# Search for specific job title
dotnet run --project Jobs.EasyApply.Dice "Software Engineer"

# Search with custom location
dotnet run --project Jobs.EasyApply.Dice "Full Stack Developer" "Remote"
```

#### Advanced Usage
```bash
# Build for production
dotnet publish -c Release --project Jobs.EasyApply.Dice

# Run published application
./bin/Release/net9.0/publish/Jobs.EasyApply.Dice/Jobs.EasyApply.Dice
```

## 📋 Parameters & Usage

### Command Line Arguments

The application accepts optional command line arguments to override default search parameters:

```bash
# Use default values from appsettings.json
dotnet run --project Jobs.EasyApply.Dice

# Override job title only
dotnet run --project Jobs.EasyApply.Dice "Senior Developer"

# Override both job title and location
dotnet run --project Jobs.EasyApply.Dice "Full Stack Developer" "Remote"
```

### Parameter Priority

1. **Command line arguments** (highest priority)
2. **Configuration file** (`appsettings.json`)
3. **Compiled defaults** (lowest priority)

## 📊 Application Flow

1. **Configuration Loading**: Load settings from `appsettings.json`
2. **API Connectivity Validation**: Tests API connectivity before starting job search
3. **WebDriver Setup**: Initialize Chrome WebDriver with appropriate options
4. **Dice Authentication**: Automated login using provided credentials
5. **Job Search**: Navigate to Dice.com jobs search with specified parameters and Easy Apply filter
6. **Job Processing**: For each Easy Apply job found:
   - Check if previously applied (via API)
   - Navigate to job details page
   - Apply using Easy Apply feature
   - Record application result in database via API
7. **Cleanup**: Close browser and log completion

## 📡 API Integration

The Dice application integrates with the centralized JobEasyApplicator API for tracking applications:

### API Endpoints Used
- **`GET /api/jobs/test-connection`**: Tests API connectivity and retrieves current application count
  ```json
  {
    "Success": true,
    "Message": "Database connection successful",
    "Count": 15
  }
  ```

- **`GET /api/jobs/check/{jobId}`**: Checks if a job was previously applied to (for duplicate prevention)

- **`POST /api/jobs`**: Records successful or failed job applications
  ```json
  {
    "JobTitle": "Senior .NET Developer",
    "Company": "Tech Corp",
    "JobId": "dice-job-123",
    "Url": "https://www.dice.com/jobs/123",
    "Provider": "Dice",
    "AppliedDate": "2025-10-28T10:02:52Z",
    "Success": true
  }
  ```

### Benefits
- **Centralized Tracking**: All applications from Dice and other providers in one database
- **Cross-Platform Analytics**: Unified reporting across job platforms
- **Duplicate Prevention**: Checks for previously applied jobs regardless of platform
- **Historical Data**: Complete application history with detailed metadata

## 🔒 Security Considerations

- **Credential Storage**: Currently stored in configuration (see recommendations for improvement)
- **Browser Security**: Runs in non-headless mode for transparency and debugging
- **Rate Limiting**: Includes delays to respect platform terms and avoid detection
- **Session Management**: Proper cleanup of authentication sessions

## 📝 Logging

The application uses Serilog for comprehensive logging:

- **Information**: General application flow and progress
- **Warning**: Non-critical issues (missing elements, skipped jobs)
- **Error**: Failed operations and exceptions

### Log Output
```
[10:02:52 INF] Starting Dice job search for ".NET Developer" in "Remote"
[10:02:53 INF] Testing API connectivity...
[10:02:54 INF] API connectivity confirmed. Proceeding with job search...
[10:02:56 INF] Found 5 Easy Apply jobs
[10:02:58 INF] Applied for job - Title: Senior Developer, Company: TechCorp, Success: True
```

## 🔧 Troubleshooting

### Common Issues

1. **No jobs found**
   - **Cause**: Easy Apply filter may be disabled or job search has no matching results
   - **Solution**: Verify search parameters and check if Dice.com has Easy Apply jobs for your criteria
   - **Debug**: Check console logs for search result counts

2. **Application fails - Login issues**
   - **Cause**: Invalid credentials or Dice.com login process changes
   - **Solution**: Verify credentials in `appsettings.json` and check Dice.com for login requirements
   - **Debug**: Enable debug logging to see authentication steps

3. **API connection fails**
   - **Cause**: API server not running or network connectivity issues
   - **Solution**: Ensure Jobs.EasyApply.API is running on `http://localhost:5070`
   - **Debug**: Check network connectivity and API server logs

4. **Chrome driver issues**
   - **Cause**: ChromeDriver version mismatch with installed Chrome browser
   - **Solution**: Update ChromeDriver to match your Chrome version
   - **Debug**: Check browser console for WebDriver errors

5. **Easy Apply detection not working**
   - **Cause**: Dice.com changed Easy Apply UI elements or detection logic
   - **Solution**: Update detection selectors in `DiceJobScraper.cs`
   - **Debug**: Use browser developer tools to inspect Easy Apply job elements

6. **Manual intervention timeout**
   - **Cause**: Application forms require extended manual input time
   - **Solution**: Increase timeout values for manual intervention prompts

### Debug Mode

Enable detailed logging by setting Serilog minimum level to Debug in code or configuration.

### Browser Inspection
For troubleshooting web automation issues:
1. Run application in non-headless mode (default)
2. Use browser developer tools (F12) to inspect elements during automation
3. Check console output for element detection failures
4. Verify Dice.com UI changes haven't broken selectors

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/dice-improvement`)
3. Make your changes following the existing code patterns
4. Update tests if applicable
5. Submit a pull request

### Development Guidelines
- Follow existing naming conventions and code structure
- Add comprehensive logging for new features
- Update this README for any configuration or feature changes
- Test with various job types and Easy Apply scenarios

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🆘 Support

For support and questions:
- Create an issue in the repository with label `dice-automation`
- Check existing issues for similar Dice.com problems
- Include relevant log output and browser information in bug reports

---

**Note**: This tool is for educational and personal use only. Ensure compliance with Dice.com's Terms of Service and use responsibly.
